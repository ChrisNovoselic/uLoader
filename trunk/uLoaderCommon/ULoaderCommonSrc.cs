﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using HClassLibrary;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderSrc : HHandlerDbULoader, ILoaderSrc
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

        protected override GroupSignals.STATE State
        {
            get { return base.State; }

            set
            {
                base.State = value;

                MSecRemaindToActivate = MSecInterval;
            }
        }

        protected long MSecRemaindToActivate
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).MSecRemaindToActivate;
                else
                    throw new Exception(@"ULoaderCommon::MSecRemaindToActivate.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).MSecRemaindToActivate = value;
                else
                    throw new Exception(@"ULoaderCommon::MSecRemaindToActivate.set ...");
            }
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
            changeState (GroupSignals.STATE.TIMER);

            if (!(m_timerActivate == null))
                m_timerActivate.Change(m_msecIntervalTimerActivate, System.Threading.Timeout.Infinite);
            else
                ;
        }
        /// <summary>
        /// Класс для описания группы сигналов источника
        /// </summary>
        protected abstract class GroupSignalsSrc : GroupSignals
        {
            protected class SIGNALBiyskTMoraSrc : SIGNAL
            {
                public string m_NameTable;

                public SIGNALBiyskTMoraSrc(int idMain, string nameTable)
                    : base(idMain)
                {
                    this.m_NameTable = nameTable;
                }
            }

            protected class SIGNALMSTKKSNAMEsql : SIGNAL
            {
                public string m_kks_name;

                public SIGNALMSTKKSNAMEsql(int idMain, string kks_name)
                    : base(idMain)
                {
                    m_kks_name = kks_name;
                }
            }

            public class SIGNALKTSTUsql : SIGNAL
            {
                public SIGNALKTSTUsql(int idMain)
                    : base(idMain)
                {
                }
            }

            private long m_msecRemaindToActivate;
            /// <summary>
            /// Интервал (милисекунды) - оставшееся время до очередной активации запроса
            /// </summary>
            public long MSecRemaindToActivate { get { return m_msecRemaindToActivate; } set { m_msecRemaindToActivate = value; } }

            private DataTable m_tableRec;
            /// <summary>
            /// Таблица результат выполнения запроса
            /// </summary>
            public override DataTable TableRecieved { get { return m_tableRec; } set { m_tableRec = value; } }

            private int m_iRowCountRecieved;
            /// <summary>
            /// Количество строк в таблице-результате выполнения запроса
            /// </summary>
            public int RowCountRecieved
            {
                get { return m_iRowCountRecieved; }

                set
                {
                    if (isUpdateQuery (value) == true)
                        setQuery ();
                    else
                        ;

                    m_iRowCountRecieved = value;
                }
            }

            protected string m_strQuery;
            /// <summary>
            /// Строка для запроса
            /// </summary>
            public string Query { get { return m_strQuery; } set { m_strQuery = value; } }
            /// <summary>
            ///  Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="parent">Объект-владелей группы сигналов источника</param>
            /// <param name="pars">Параметры для инициализации группы сигналов источника</param>
            public GroupSignalsSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                m_iRowCountRecieved = -1;
            }

            protected virtual bool isUpdateQuery (int cntRec)
            {
                return true;
            }

            protected abstract void setQuery();
        }
        /// <summary>
        /// Добавить все известные состояния для обработки
        /// </summary>
        /// <returns></returns>
        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            return iRes;
        }
        /// <summary>
        /// Изменить состояние - интциировать очередной запрос
        /// </summary>
        private void changeState(GroupSignals.STATE state)
        {
            lock (m_lockStateGroupSignals)
            {
                //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @"]" + @" 'QUEUE' не найдено...", Logging.INDEX_MESSAGE.NOT_SET);
                //Перевести в состояние "активное" ("ожидание") группы сигналов
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
                    if (pair.Value.State == state)
                    {
                        (pair.Value as GroupSignalsSrc).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

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
        }
        /// <summary>
        /// Изменить состояние - интциировать очередной запрос
        /// </summary>
        public void ChangeState()
        {
            changeState (GroupSignals.STATE.SLEEP);
        }
        //Количество строк в таблице-результате для текущей (обрабатываемой) группы
        private int RowCountRecieved { get { return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved; } set { (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved = value; } }
        //Строка запроса для текущей (обрабатываемой) группы
        private string Query { get { return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).Query; } }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (!(IdGroupSignalsCurrent < 0))
                        GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_connSett.port), m_dictIdListeners[IdGroupSignalsCurrent][0]);
                    else
                        throw new Exception(@"HHandlerDbULoader::StateRequest (::CurrentTime) - ...");
                    break;
                case (int)StatesMachine.Values:
                    if (RowCountRecieved == -1)
                        RowCountRecieved = 0;
                    else
                        ;

                    //Logging.Logg().Debug(@"HHandlerDbULoaderSrc::StateRequest (::Values) - Query=" + Query, Logging.INDEX_MESSAGE.NOT_SET);

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
                    Request(m_dictIdListeners[IdGroupSignalsCurrent][0], Query);
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
            //string msg = @"HHandlerDbULoaderDest::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
            //    + @"[" + PlugInId + @", key=" + m_IdGroupSignalsCurrent + @"] ";

            try
            {
                switch (state)
                {
                    case (int)StatesMachine.CurrentTime:
                        m_dtServer = (DateTime)(table as DataTable).Rows[0][0];
                        //msg =+ @"DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff") + @"...";                        
                        break;
                    case (int)StatesMachine.Values:
                        //msg =+ @"Ok ...";
                        RowCountRecieved = table.Rows.Count;
                        Logging.Logg().Debug(@"Получено строк [" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);
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
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::StateResponse (::" + ((StatesMachine)state).ToString () + @") - ...");
            }

            //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine (msg);

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            HHandler.INDEX_WAITHANDLE_REASON resReason = INDEX_WAITHANDLE_REASON.SUCCESS;

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

            if (! (_iPlugin == null))
                (_iPlugin as PlugInBase).DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.ERROR, IdGroupSignalsCurrent, state, msgErr });
            else
                ;

            return resReason;
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

        protected override bool isPush(int curCount)
        {
            return curCount == 0;
        }

        protected override object[] getDataAskedHost()
        {
            object[] arObjToSend = base.getDataAskedHost ();

            arObjToSend [arObjToSend.Length - 1] = new object [] { m_connSett.id };

            return arObjToSend;
        }
    }

    public abstract class HHandlerDbULoaderDatetimeSrc : HHandlerDbULoaderSrc
    {
        public HHandlerDbULoaderDatetimeSrc()
            : base()
        {
        }

        public HHandlerDbULoaderDatetimeSrc(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        /// <summary>
        /// Класс для описания группы сигналов источника
        /// </summary>
        protected abstract class GroupSignalsDatetimeSrc : GroupSignalsSrc
        {
            //private long m_lCountMSecInterval;
            //public long CountMSecInterval { get { return m_lCountMSecInterval; } set { m_lCountMSecInterval = value; } }

            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }

            public GroupSignalsDatetimeSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart = DateTime.MinValue;
            }

            /// <summary>
            /// Строка для условия "по дате/времени"
            ///  - начало
            /// </summary>
            protected virtual string DateTimeStartFormat
            {
                get { return DateTimeStart.ToString(@"yyyyMMdd HHmmss"); }
            }
            /// <summary>
            /// Строка для условия "по дате/времени"
            ///  - окончание
            /// </summary>
            protected virtual string DateTimeCurIntervalEndFormat
            {
                get { return DateTimeStart.AddSeconds((int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyyMMdd HHmmss"); }
            }

            protected override bool isUpdateQuery (int cntRec)
            {
                return ((RowCountRecieved < 0) || (RowCountRecieved == cntRec)) && ((!(EvtActualizeDateTimeStart == null)) && (EvtActualizeDateTimeStart() == 1));
            }

            private event IntDelegateFunc EvtActualizeDateTimeStart;
            /// <summary>
            /// Установить метод для актуализации начальной даты/времени для опроса
            /// </summary>
            /// <param name="fActualize">Функция-делегат для актуализации начальной даты/времени для опроса</param>
            public void SetDelegateActualizeDateTimeStart(IntDelegateFunc fActualize)
            {
                if (EvtActualizeDateTimeStart == null)
                {
                    RowCountRecieved = -1;

                    EvtActualizeDateTimeStart += fActualize;
                }
                else
                    ;
            }
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = base.Initialize(id, pars);

            if (m_dictGroupSignals.Keys.Contains(id) == true)
                (m_dictGroupSignals[id] as GroupSignalsDatetimeSrc).SetDelegateActualizeDateTimeStart(actualizeDateTimeStart);
            else
                ;

            return iRes;
        }

        //protected long CountMSecInterval
        //{
        //    get
        //    {
        //        if (!(IdGroupSignalsCurrent < 0))
        //            return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).CountMSecInterval;
        //        else
        //            throw new Exception(@"ULoaderCommon::CountMSecInterval.get ...");
        //    }

        //    set
        //    {
        //        if (!(IdGroupSignalsCurrent < 0))
        //            (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).CountMSecInterval = value;
        //        else
        //            throw new Exception(@"ULoaderCommon::CountMSecInterval.set ...");
        //    }
        //}

        /// <summary>
        /// Дата/время начала опроса для текущей (обрабатываемой) группы
        /// </summary>
        protected DateTime DateTimeStart
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeStart
                        //+ TimeSpan.FromMilliseconds (CountMSecInterval * MSecInterval)
                        ;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeStart = value;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.set ...");
            }
        }

        /// <summary>
        /// Актулизировать дату/время начала опроса
        /// </summary>
        /// <returns>Признак изменения даты/времени начала опроса</returns>
        protected int actualizeDateTimeStart()
        {
            int iRes = 0;
            //Проверить признак 1-го запуска
            if (DateTimeStart == DateTime.MinValue)
            {
                DateTimeStart = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
                //CountMSecInterval = 0;
                iRes = 1;
            }
            else
                //Проверить необходимость изменения даты/времени
                if ((m_dtServer - DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds)).TotalMilliseconds > MSecInterval)
                {
                    //Переход на очередной интервал
                    DateTimeStart = DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds);
                    //CountMSecInterval++;
                    //Установить признак перехода
                    iRes = 1;
                }
                else
                    ;

            Logging.Logg().Debug(@"HHandlerDbULoader::actualizeDateTimeStart () - "
                                + @"[" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]"
                                + @", m_dtServer=" + m_dtServer.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", DateTimeStart=" + DateTimeStart.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", iRes=" + iRes
                                + @"...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        protected override int IdGroupSignalsCurrent { get { return base.IdGroupSignalsCurrent; } set { if (value == -1) completeGroupSignalsCurrent(); else ; base.IdGroupSignalsCurrent = value; } }

        private void completeGroupSignalsCurrent()
        {
            //if (State == GroupSignals.STATE.SLEEP)
            //    if (! ((DateTimeStart + TimeSpan.FromMilliseconds (MSecInterval)) < (DateTimeStart - TimeSpan.FromMilliseconds(CountMSecInterval * MSecInterval) + TimeSpanPeriod)))
            //        Stop(IdGroupSignalsCurrent);
            //    else
            //    {
            //        CountMSecInterval ++;
            //        push(IdGroupSignalsCurrent);
            //    }
            //else
            //    ;
        }
    }

    public abstract class HHandlerDbULoaderMSTTMSrc : HHandlerDbULoaderDatetimeSrc
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

            if (m_dictAdding.ContainsKey(@"CUR_INTERVAL_SHIFT") == true)
                m_iCurIntervalShift = Convert.ToInt32(m_dictAdding[@"CUR_INTERVAL_SHIFT"]);
            else
                ;

            return iRes;
        }

        protected abstract class GroupSignalsMSTTMSrc : GroupSignalsDatetimeSrc
        {
            public GroupSignalsMSTTMSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
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
