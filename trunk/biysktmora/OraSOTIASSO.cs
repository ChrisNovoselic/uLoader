using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;
using uLoaderCommon;

namespace biysktmora
{    
    public class HBiyskTMOra : HHandlerDbULoader
    {
        private int m_msecIntervalTimerActivate;
        private System.Threading.Timer m_timerActivate;

        enum StatesMachine
        {
            CurrentTime
            , Values
        }

        public HBiyskTMOra()
            : base()
        {
            initialize();
        }
        
        public HBiyskTMOra(IPlugIn iPlugIn)
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
            m_timerActivate = new System.Threading.Timer(fTimerActivate, null, 0, m_msecIntervalTimerActivate);

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
                        (pair.Value as GroupSignalsBiyskTMOra).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                        if ((pair.Value as GroupSignalsBiyskTMOra).MSecRemaindToActivate < 0)
                        {
                            pair.Value.State = GroupSignals.STATE.QUEUE;

                            enqueue(pair.Key);
                        }
                        else
                            ;
                    }
                    else
                        ;
            }
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = base.Initialize(id, pars);

            if (m_dictGroupSignals.Keys.Contains(id) == true)
                (m_dictGroupSignals[id] as GroupSignalsBiyskTMOra).SetDelegateActualizeDateTimeStart(actualizeDateTimeStart);
            else
                ;

            return iRes;
        }

        private class GroupSignalsBiyskTMOra : GroupSignals
        {
            public class SIGNALBiyskTMOra : SIGNAL
            {
                public string m_NameTable;

                public SIGNALBiyskTMOra(int idMain, string nameTable) : base (idMain)
                {
                    this.m_NameTable = nameTable;
                }
            }

            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }            

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

            private string m_strQuery;
            public string Query { get { return m_strQuery; } set { m_strQuery = value; } }

            public GroupSignalsBiyskTMOra(object[] pars) : base (pars)
            {
                m_iRowCountRecieved = -1;

                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart = DateTime.MinValue;                
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALBiyskTMOra((int)objs[0], objs[2] as string);
            }

            private void setQuery()
            {
                m_strQuery = string.Empty;

                string strUnion = @" UNION "
                    //Строки для условия "по дате/времени"
                    , strStart = DateTimeStart.ToString(@"yyyyMMdd HHmmss")
                    , strEnd = DateTimeStart.AddSeconds((int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyyMMdd HHmmss");
                //Формировать зпрос
                foreach (GroupSignalsBiyskTMOra.SIGNALBiyskTMOra s in m_arSignals)
                {
                    m_strQuery += @"SELECT " + s.m_idMain + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.m_NameTable
                        + @" WHERE"
                        + @" DATETIME >=" + @" to_timestamp('" + strStart + @"', 'yyyymmdd hh24miss')"
                        + @" AND DATETIME <" + @" to_timestamp('" + strEnd + @"', 'yyyymmdd hh24miss')"
                        + strUnion
                    ;
                }

                //Удалить "лишний" UNION
                m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
                ////Установить сортировку
                //m_strQuery += @" ORDER BY DATETIME DESC";

                //Logging.Logg().Debug(@"GroupSignalsBiystTMOra::setQuery() - m_strQuery=" + m_strQuery + @"]..."
                //        , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            return iRes;
        }        

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsBiyskTMOra(objs);
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
                if ((m_dtServer - DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds)).TotalMilliseconds > 666)
                {
                    DateTimeStart = DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds);
                    iRes = 1;
                }
                else
                    ;

            Logging.Logg().Debug(@"HBiyskTMOra::actualizeDateTimeStart () - "
                                + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]"
                                + @", m_dtServer=" + m_dtServer.ToString (@"dd.MM.yyyy HH:mm.ss.fff")
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
                    return (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsBiyskTMOra).DateTimeStart;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.get ...");
            }

            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsBiyskTMOra).DateTimeStart = value;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.set ...");
            }
        }        

        public void ChangeState()
        {
            ClearStates();

            addAllStates();

            Run(@"HBiyskTMOra::ChangeState ()");
        }

        public override void ClearValues()
        {
            int iPrev = 0, iDel = 0, iCur = 0;
            if (!(TableRecieved == null))
            {
                iPrev = TableRecieved.Rows.Count;
                string strSel =
                    @"DATETIME<'" + DateTimeStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' OR DATETIME>='" + DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    ;

                DataRow[] rowsDel = null;
                try { rowsDel = TableRecieved.Select(strSel); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ClearValues () - ...");
                }

                if (!(rowsDel == null))
                {
                    iDel = rowsDel.Length;
                    if (rowsDel.Length > 0)
                    {
                        foreach (DataRow r in rowsDel)
                            TableRecieved.Rows.Remove(r);
                        //??? Обязательно ли...
                        TableRecieved.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;

                iCur = TableRecieved.Rows.Count;

                Console.WriteLine(@"Обновление рез-та [ID=" + m_IdGroupSignalsCurrent + @"]: " + @"(было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @")");
            }
            else
                ;
        }

        private int RowCountRecieved { get { return (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsBiyskTMOra).RowCountRecieved; } set { (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsBiyskTMOra).RowCountRecieved = value; } }

        private string Query { get { return (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsBiyskTMOra).Query; } }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (! (m_IdGroupSignalsCurrent < 0))
                        GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.Oracle, m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    else
                        throw new Exception(@"HBiyskTMOra::StateRequest (::CurrentTime) - ...");
                    break;
                case (int)StatesMachine.Values:
                    if (RowCountRecieved == -1)
                        RowCountRecieved = 0;
                    else
                        ;

                    //Logging.Logg().Debug(@"HBiyskTMOra::StateRequest (::Values) - Query=" + Query, Logging.INDEX_MESSAGE.NOT_SET);
                    
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
                    //    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateRequest (::Values) - ...");
                    //}
                    Request (m_dictIdListeners[m_IdGroupSignalsCurrent][0], Query);
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
                        //Console.WriteLine(m_dtServer.ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                        Logging.Logg().Debug(@"HBiyskTMOra::StateResponse (::CurrentTime) - DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH:mm:ss.fff"), Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse (::CurrentTime) - ...");
                    }
                    break;
                case (int)StatesMachine.Values:
                    try
                    {
                        RowCountRecieved = table.Rows.Count;
                        Console.WriteLine(@"Получено строк [ID=" + m_IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count);
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

                        TableRecieved = table.Copy();
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse (::Values) - ...");
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

            if (msgErr.Equals (unknownErr) == false)
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
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1001;

            createObject(typeof(HBiyskTMOra));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
