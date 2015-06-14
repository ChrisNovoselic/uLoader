using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

using HClassLibrary;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderSrc : HHandlerDbULoader
    {
        private int m_msecIntervalTimerActivate;
        private int m_msecCorrectTimerActivate;
        private System.Threading.Timer m_timerActivate;

        enum StatesMachine
        {
            CurrentTime
            , Values
        }

        public HHandlerDbULoaderSrc()
            : base()
        {
            initialize();
        }

        public HHandlerDbULoaderSrc(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
            initialize();
        }

        private int initialize()
        {
            int iRes = 0;

            m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;

            return iRes;
        }

        public override void Start()
        {
            base.Start();

            startTimerActivate();
        }

        public override void Stop()
        {
            stopTimerActivate();

            base.Stop();
        }

        private int startTimerActivate()
        {
            int iRes = 0;

            stopTimerActivate();
            m_timerActivate = new System.Threading.Timer(fTimerActivate, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            m_timerActivate.Change(0, System.Threading.Timeout.Infinite);

            return iRes;
        }

        private int stopTimerActivate()
        {
            int iRes = 0;

            if (!(m_timerActivate == null))
            {
                m_timerActivate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerActivate.Dispose();
                m_timerActivate = null;
            }
            else
                ;

            return iRes;
        }

        private void fTimerActivate(object obj)
        {
            lock (m_lockStateGroupSignals)
            {
                //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [ID=" + (_iPlugin as PlugInBase)._Id + @"]" + @" 'QUEUE' не найдено...", Logging.INDEX_MESSAGE.NOT_SET);
                //Перевести в состояние "активное" ("ожидание") группы сигналов
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
                    if (pair.Value.State == GroupSignals.STATE.TIMER)
                    {
                        (pair.Value as GroupSignalsSrc).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                        if ((pair.Value as GroupSignalsSrc).MSecRemaindToActivate < 0)
                        {
                            pair.Value.State = GroupSignals.STATE.QUEUE;

                            push(pair.Key);
                        }
                        else
                            ;
                    }
                    else
                        ;
            }

            if (!(m_timerActivate == null))
                m_timerActivate.Change(m_msecIntervalTimerActivate, System.Threading.Timeout.Infinite);
            else
                ;
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = base.Initialize(id, pars);

            if (m_dictGroupSignals.Keys.Contains(id) == true)
                (m_dictGroupSignals[id] as GroupSignalsSrc).SetDelegateActualizeDateTimeStart(actualizeDateTimeStart);
            else
                ;

            return iRes;
        }

        protected abstract class GroupSignalsSrc : GroupSignals
        {
            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }

            //Строки для условия "по дате/времени"
            // начало
            protected virtual string DateTimeStartFormat
            {
                get { return DateTimeStart.ToString(@"yyyyMMdd HHmmss"); }
            }
            // окончание
            protected virtual string DateTimeCurIntervalEndFormat
            {
                get { return DateTimeStart.AddSeconds((int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyyMMdd HHmmss"); }
            }

            private event IntDelegateFunc EvtActualizeDateTimeStart;
            public void SetDelegateActualizeDateTimeStart(IntDelegateFunc fActualize)
            {
                if (EvtActualizeDateTimeStart == null)
                {
                    m_iRowCountRecieved = -1;

                    EvtActualizeDateTimeStart += fActualize;
                }
                else
                    ;
            }

            private DataTable m_tableRec;
            public override DataTable TableRecieved { get { return m_tableRec; } set { m_tableRec = value; } }

            private int m_iRowCountRecieved;
            public int RowCountRecieved
            {
                get { return m_iRowCountRecieved; }

                set
                {
                    //int iActualizeDateTimeStart = -1;

                    if ((m_iRowCountRecieved < 0)
                        || (m_iRowCountRecieved == value))
                    {
                        if (EvtActualizeDateTimeStart() == 1)
                            setQuery();
                        else
                            ;
                    }
                    else
                        ;

                    //Logging.Logg().Debug(@"GroupSignalsBiystTMOra::RowCountRecieved.set [" + m_iRowCountRecieved + @", newValue=" + value + @"]"
                    //    + @" iActualizeDateTimeStart=" + iActualizeDateTimeStart
                    //    + @"..."
                    //    , Logging.INDEX_MESSAGE.NOT_SET);

                    m_iRowCountRecieved = value;
                }
            }

            protected string m_strQuery;
            public string Query { get { return m_strQuery; } set { m_strQuery = value; } }

            public GroupSignalsSrc(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
                m_iRowCountRecieved = -1;

                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart = DateTime.MinValue;
            }

            protected abstract void setQuery();
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            return iRes;
        }

        protected int actualizeDateTimeStart()
        {
            int iRes = 0;

            if (DateTimeStart == DateTime.MinValue)
            {
                DateTimeStart = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
                iRes = 1;
            }
            else
                if ((m_dtServer - DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds)).TotalMilliseconds > MSecInterval)
                {
                    DateTimeStart = DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds);
                    iRes = 1;
                }
                else
                    ;

            Logging.Logg().Debug(@"HHandlerDbULoader::actualizeDateTimeStart () - "
                                + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]"
                                + @", m_dtServer=" + m_dtServer.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", DateTimeStart=" + DateTimeStart.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", iRes=" + iRes
                                + @"...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        protected DateTime DateTimeStart
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsSrc).DateTimeStart;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.get ...");
            }

            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsSrc).DateTimeStart = value;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.set ...");
            }
        }

        public void ChangeState()
        {
            ClearStates();

            addAllStates();

            Run(@"HHandlerDbULoader::ChangeState ()");
        }

        private int RowCountRecieved { get { return (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved; } set { (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved = value; } }

        private string Query { get { return (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsSrc).Query; } }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (!(m_IdGroupSignalsCurrent < 0))
                        GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_connSett.port), m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    else
                        throw new Exception(@"HHandlerDbULoader::StateRequest (::CurrentTime) - ...");
                    break;
                case (int)StatesMachine.Values:
                    if (RowCountRecieved == -1)
                        RowCountRecieved = 0;
                    else
                        ;

                    Logging.Logg().Debug(@"HHandlerDbULoaderSrc::StateRequest (::Values) - Query=" + Query, Logging.INDEX_MESSAGE.NOT_SET);

                    //try
                    //{
                    //    iActualizeDatetimeStart = actualizeDatetimeStart ();
                    //    if (iActualizeDatetimeStart == 1)
                    //    {//Дата/время "старта" изменено
                    //ClearValues();

                    //        setQuery(DateTimeStart);
                    //    }
                    //    else
                    //        ;
                    //}
                    //catch (Exception e)
                    //{
                    //    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::StateRequest (::Values) - ...");
                    //}
                    Request(m_dictIdListeners[m_IdGroupSignalsCurrent][0], Query);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            DataTable table = obj as DataTable;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    try
                    {
                        m_dtServer = (DateTime)(table as DataTable).Rows[0][0];
                        //string msg = @"SrcBiyskTMora::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
                        //    + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                        //    + @"DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff") + @"...";
                        //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        //Console.WriteLine (msg);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::StateResponse (::CurrentTime) - ...");
                    }
                    break;
                case (int)StatesMachine.Values:
                    try
                    {
                        RowCountRecieved = table.Rows.Count;
                        Logging.Logg().Debug(@"Получено строк [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);
                        if (TableRecieved == null)
                        {
                            TableRecieved = new DataTable();
                        }
                        else
                            ;

                        //int iPrev = -1, iDupl = -1, iAdd = -1, iCur = -1;
                        //iPrev = 0; iDupl = 0; iAdd = 0; iCur = 0;
                        //iPrev = TableResults.Rows.Count;

                        //if (results.Rows.Count == 0)
                        //{
                        //    results = table.Copy ();
                        //}
                        //else
                        //    ;

                        //!!! Перенесена в библ. для "вставки"
                        ////Удалить из таблицы записи, метки времени в которых, совпадают с метками времени в таблице-рез-те предыдущего опроса
                        //iDupl = clearDupValues (ref table);

                        ////!!! Перенесена в библ. для "вставки"
                        ////Сформировать таблицу с "новыми" данными
                        //DataTable tableIns = getTableIns(ref table);
                        //tableIns.Columns.Add(@"tmdelta", typeof(int));

                        //iAdd = table.Rows.Count;
                        //TableResults.Merge(table);
                        //iCur = TableResults.Rows.Count;
                        //Console.WriteLine(@"Объединение таблицы-рез-та: [было=" + iPrev + @", дублирущих= " + iDupl + @", добавлено=" + iAdd + @", стало=" + iCur + @"]");

                        TableRecieved = GroupSignals.clearDupValues(table);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::StateResponse (::Values) - ...");
                    }
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, int request, int result)
        {
            string unknownErr = @"Неизвестная ошибка"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получения даты/времени сервера-источника";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"получения значений источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals(unknownErr) == false)
                msgErr = @"Ошибка " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);

            (_iPlugin as PlugInBase).DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.ERROR, 0, state, msgErr });
        }

        protected override void StateWarnings(int state, int request, int result)
        {
            string unknownErr = @"Неизвестное предупреждение"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получении даты/времени сервера-источника";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"получении значений источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals(unknownErr) == false)
                msgErr = @"Предупреждение при " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);
        }

        protected override object[] getDataAskedHost()
        {
            object[] arObjToSend = base.getDataAskedHost ();

            arObjToSend [arObjToSend.Length - 1] = new object [] { m_connSett.id };

            return arObjToSend;
        }
    }

    public abstract class HHandlerDbULoaderMSTTMSrc : HHandlerDbULoaderSrc
    {
        public int m_iCurIntervalShift;

        public HHandlerDbULoaderMSTTMSrc()
            : base()
        {
        }

        public HHandlerDbULoaderMSTTMSrc(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            m_iCurIntervalShift = Convert.ToInt32(m_dictAdding[@"CUR_INTERVAL_SHIFT"]);

            return iRes;
        }

        protected abstract class GroupSignalsMSTTMSrc : GroupSignalsSrc
        {
            public GroupSignalsMSTTMSrc(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            //Строки для условия "по дате/времени"
            // начало
            protected override string DateTimeStartFormat
            {
                get { return DateTimeStart.AddHours(-6).AddSeconds((_parent as HHandlerDbULoaderMSTTMSrc).m_iCurIntervalShift * (int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss"); }
            }
            // окончание
            protected override string DateTimeCurIntervalEndFormat
            {
                //get { return DateTimeStart.AddHours(-6).AddSeconds((int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss"); }
                get { return DateTimeStart.AddHours(-6).ToString(@"yyyy/MM/dd HH:mm:ss"); }
            }
        }
    }
}
