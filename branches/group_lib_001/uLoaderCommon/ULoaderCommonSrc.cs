﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Threading;
using System.Diagnostics;

using HClassLibrary;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderSrc : HHandlerDbULoader, ILoaderSrc
    {
        /// <summary>
        /// Идентификатор ТЭЦ
        ///  (при наличии в файле конфигурации для группы источников)
        /// </summary>
        public int m_IdTEC;

        private int m_msecIntervalTimerActivate;
        private int m_msecCorrectTimerActivate;
        private
            //Вариант №1
            System.Threading.Timer
            ////Вариант №2
            //System.Timers.Timer
            ////Вариант №3
            //System.Windows.Threading.DispatcherTimer
                m_timerActivate;

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

        public HHandlerDbULoaderSrc(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
            initialize();
        }

        private int initialize()
        {
            int iRes = 0;

            m_IdTEC = -1;

            m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;

            return iRes;
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            string[] arDictAddingKeys = new string[] { @"ID_TEC" };

            foreach (string key in arDictAddingKeys)
                if (m_dictAdding.ContainsKey(key) == true)
                    switch (key)
                    {
                        case @"ID_TEC":
                            m_IdTEC = Int32.Parse(m_dictAdding[key]);
                            break;
                        default:
                            iRes = 1; // необрабатываемый параметр
                            break;
                    }
                else
                    switch (key)
                    {
                        case @"NAME_TABLE":
                            iRes = -1;
                            break;
                        case @"ID_TEC":
                            m_IdTEC = -1;
                            break;
                        default: ;
                            break;
                    }

            return iRes;
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = -1;            

            iRes = base.Initialize(id, pars);            

            return iRes;
        }

        protected override object[] getConfirmStartAskedHost(int id)
        {
            object[] objRes = base.getConfirmStartAskedHost(id);

            objRes[3] = new object[]
                {
                    m_dictGroupSignals[id].Mode
                    , m_connSett.id
                    , ((m_dictAdding.ContainsKey(@"ID_TEC") == true) ? Int32.Parse(m_dictAdding[@"ID_TEC"]) : -1)
                };

            return objRes;
        }

        protected override GroupSignals.STATE State
        {
            get { return base.State; }

            set
            {
                base.State = value;

                MSecRemaindToActivate = MSecIntervalLocal;
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

        ///// <summary>
        ///// Старт группы сигналов с указанным идентификаторм
        ///// </summary>
        ///// <param name="id">Идентификатор группы сигналов</param>
        //public override void Start(int id)
        //{
        //    base.Start (id);

        //    //changeState (m_dictGroupSignals[id].State);
        //    changeState(id);
        //}

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
            m_timerActivate =
                //Вариант №1
                new System.Threading.Timer(fTimerActivate, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite)
                ////Вариант №2
                //new System.Timers.Timer (m_msecIntervalTimerActivate);
                ;
            //Вариант №1
            m_timerActivate.Change(0, System.Threading.Timeout.Infinite);
            ////Вариант №2
            //m_timerActivate.Elapsed += new System.Timers.ElapsedEventHandler(timerActivate_OnElapsed);
            //m_timerActivate.Start ();

            return iRes;
        }

        private int stopTimerActivate()
        {
            int iRes = 0;

            if (!(m_timerActivate == null))
            {
                //Вариант №1
                m_timerActivate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                ////Вариант №2
                //m_timerActivate.Stop ();
                //m_timerActivate.Close ();
                m_timerActivate.Dispose();
                m_timerActivate = null;
            }
            else
                ;

            return iRes;
        }
        //Вариант №1
        /// <summary>
        /// Обработчик события таймера 'System.Threading.Timer'
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        private void fTimerActivate(object obj)
        {
            changeState();

            if (!(m_timerActivate == null))
                m_timerActivate.Change(m_msecIntervalTimerActivate, System.Threading.Timeout.Infinite);
            else
                ;
        }
        ////Вариант №2
        ///// <summary>
        /////  Обработчик события таймера 'System.Timers.Timer'
        ///// </summary>
        ///// <param name="obj">Объект, инициировавший событие</param>
        ///// <param name="ev">Аргумент события</param>
        //private void timerActivate_OnElapsed (object obj, System.Timers.ElapsedEventArgs ev)
        //{
        //    changeState(GroupSignals.STATE.TIMER);
        //}
        /// <summary>
        /// Класс для описания группы сигналов источника
        /// </summary>
        protected abstract class GroupSignalsSrc : GroupSignals
        {
            protected int m_UTCOffsetToServerTotalHours {
                get {
                    return (_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToServer == HTimeSpan.NotValue ?
                        0 : (int)(_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToServer.Value.TotalHours;
                }
            }

            protected int m_UTCOffsetToDataTotalHours {
                get {
                    return (_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToData == HTimeSpan.NotValue ?
                        0 : (int)(_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToData.Value.TotalHours;
                }
            }

            //protected int m_ServerOffsetToDataTotalHours {
            //    get {
            //        return ((_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToServer == HTimeSpan.NotValue)
            //            || ((_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToData == HTimeSpan.NotValue) ?
            //                0 : (int)((_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToServer.Value - (_parent as HHandlerDbULoaderSrc).m_tsUTCOffsetToData.Value).TotalHours;
            //    }            
            //}

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

            public class SIGNALIdsql : SIGNAL
            {
                public int m_iIdLocal;
                public bool m_bAVG;

                public SIGNALIdsql(int idMain, int idLocal, bool bAVG)
                    : base(idMain)
                {
                    m_iIdLocal = idLocal;
                    m_bAVG = bAVG;
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
                    ////Вариант №1
                    //if (isUpdateQuery (value) == true)
                    //    setQuery ();
                    //else
                    //    ;
                    ////Вариант №2 - признак обновления содержания запроса перед его выполнением ()
                    //m_bIsUpdateQuery = isUpdateQuery(value);
                    //Вариант №2 - 
                    if (isUpdateQuery(value) == true)
                        m_strQuery = string.Empty;
                    else
                        ;

                    m_iRowCountRecieved = value;
                }
            }

            public override void Stop()
            {
                base.Stop();

                m_msecRemaindToActivate = 0;
                m_iRowCountRecieved = -1;
            }

            protected string m_strQuery;
            /// <summary>
            /// Строка для запроса
            /// </summary>
            public string Query
            {
                get
                {
                    if (m_strQuery.Equals(string.Empty) == true)
                        setQuery();
                    else
                        ;

                    return m_strQuery;
                }

                /*set { m_strQuery = value; }*/
            }
            /// <summary>
            ///  Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="parent">Объект-владелей группы сигналов источника</param>
            /// <param name="pars">Параметры для инициализации группы сигналов источника</param>
            public GroupSignalsSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                m_msecRemaindToActivate = 0;
                m_iRowCountRecieved = -1;
            }
            /// <summary>
            /// Возвратить признак необходимости обновления содержания запроса
            ///  (составляющая дата/время)
            /// </summary>
            /// <param name="cntRec">Количество записей в крайней (по времени) полуенной таблице-рез-те</param>
            /// <returns>Признак необходимости обновления содержания запроса</returns>
            protected virtual bool isUpdateQuery(int cntRec)
            {
                return true;
            }
            /// <summary>
            /// Установить содержание для запроса
            /// </summary>
            protected abstract void setQuery();
            /// <summary>
            /// Возвратить основной идентификатор по вспомогательному
            ///  , указыаются в файле конфигурации для каждого сигнала отдельно
            /// </summary>
            /// <param name="id_link">Вспомогательный идентификатор</param>
            /// <returns>Основной идентификатор</returns>
            protected abstract object getIdMain(object id_link);
        }
        /// <summary>
        /// Добавить все известные состояния для обработки
        /// </summary>
        /// <returns>Результат выполнения</returns>
        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            return iRes;
        }

        private void changeState()
        {
            lock (m_lockStateGroupSignals)
            {
                //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @"]" + @" 'QUEUE' не найдено...", Logging.INDEX_MESSAGE.NOT_SET);
                //Перевести в состояние "активное" ("ожидание") группы сигналов
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
                    if ((pair.Value.State == GroupSignals.STATE.TIMER)
                        || (pair.Value.State == GroupSignals.STATE.SLEEP))
                    {
                        (pair.Value as GroupSignalsSrc).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                        if ((pair.Value as GroupSignalsSrc).MSecRemaindToActivate < 0)
                        {
                            pair.Value.State = GroupSignals.STATE.QUEUE;
                            //Debug.Print("Timer end " + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                            push(pair.Key);
                        }
                        else
                            ;
                    }
                    else
                        ;
            }
        }
        ///// <summary>
        ///// Изменить состояние - инициировать очередной запрос
        ///// </summary>
        ///// <param name="id">Идентификатор группы сигналов</param>
        //private void changeState (int id)
        //{

        //    lock (m_lockStateGroupSignals)
        //    {
        //        if (m_dictGroupSignals[id].State == GroupSignals.STATE.SLEEP)
        //        {
        //            m_dictGroupSignals[id].State = GroupSignals.STATE.QUEUE;

        //            push (id);
        //        }
        //        else
        //            ;
        //    }
        //}
        ///// <summary>
        ///// Изменить состояние - инициировать очередной запрос
        ///// </summary>
        ///// <param name="state">Сотояние групп сигналов</param>
        //private void changeState(GroupSignals.STATE state)
        //{
        //    lock (m_lockStateGroupSignals)
        //    {
        //        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @"]" + @" 'QUEUE' не найдено...", Logging.INDEX_MESSAGE.NOT_SET);
        //        //Перевести в состояние "активное" ("ожидание") группы сигналов
        //        foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
        //            if (pair.Value.State == state)
        //                if (pair.Value.State == GroupSignals.STATE.TIMER)
        //                {
        //                    (pair.Value as GroupSignalsSrc).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

        //                    //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

        //                    if ((pair.Value as GroupSignalsSrc).MSecRemaindToActivate < 0)
        //                    {
        //                        pair.Value.State = GroupSignals.STATE.QUEUE;

        //                        push(pair.Key);
        //                    }
        //                    else
        //                        ;
        //                }
        //                else
        //                    if (pair.Value.State == GroupSignals.STATE.SLEEP)
        //                    {
        //                        pair.Value.State = GroupSignals.STATE.QUEUE;

        //                        push(pair.Key);
        //                    }
        //                    else
        //                        ;
        //            else
        //                ;
        //    }
        //}
        ///// <summary>
        ///// Изменить состояние - интциировать очередной запрос
        ///// </summary>
        //public void ChangeState()
        //{
        //    changeState (GroupSignals.STATE.SLEEP);
        //}
        //Количество строк в таблице-результате для текущей (обрабатываемой) группы
        protected int RowCountRecieved { get { return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved; } set { (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved = value; } }
        //Строка запроса для текущей (обрабатываемой) группы
        private string Query { get { return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).Query; } }

        protected virtual void parseValues(DataTable table)
        {
            RowCountRecieved = table.Rows.Count;

            if (TableRecieved == null)
            {
                TableRecieved = new DataTable();
            }
            else
                ;

            TableRecieved = GroupSignals.DataTableDuplicate.Clear(table);
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;
            string msg = string.Empty;

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

                    msg = @"HHandlerDbULoaderSrc::StateRequest (::Values) - Query=" + Query;
                    //Console.WriteLine (msg);
                    //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

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
            string msg = string.Empty;

            try
            {
                switch (state)
                {
                    case (int)StatesMachine.CurrentTime:
                        m_dtServer = (DateTime)(table as DataTable).Rows[0][0];
                        //msg =+ @"DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff") + @"...";                        
                        break;
                    case (int)StatesMachine.Values:
                        parseValues(table);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"HHandlerDbULoader::StateResponse (::" + ((StatesMachine)state).ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
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
                    msgErr = @"дату/время СУБД";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"значения в БД";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals(unknownErr) == false)
            {
                msgErr = @"Не удалось получить " + msgErr;
            }
            else
                ;

            Logging.Logg().Error(@"[" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"] - " + msgErr, Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine(@"Ошибка. " + msgErr);

            if (!(_iPlugin == null))
                (_iPlugin as PlugInBase).DataAskedHost(new object[] { -1, (int)ID_DATA_ASKED_HOST.ERROR, IdGroupSignalsCurrent, state, msgErr }); //-1 неизвестный идентификатор типа (класса)объекта
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
            object[] arObjToSend = base.getDataAskedHost();

            arObjToSend[arObjToSend.Length - 1] = new object[] {
                Mode // MODE_WORK
                , m_connSett.id //IdSourceConnSett
                , m_IdTEC
            };

            return arObjToSend;
        }
    }

    public abstract class HHandlerDbULoaderDatetimeSrc : HHandlerDbULoaderSrc
    {
        public HTimeSpan m_tsCurIntervalOffset;

        protected virtual string m_strDateTimeDBFormat { get; set; }

        public enum INDEX_MODE_CURINTERVAL { CAUSE, NEXTSTEP, COUNT };
        /// <summary>
        /// Перечисление - идентификаторы режимов вычисления даты/времени начала опроса
        /// </summary>
        public enum MODE_CURINTERVAL
        {
            CAUSE_PERIOD_MINUTE,
            CAUSE_PERIOD_HOUR /*округление до ПЕРИОД, ожидание полного набора записей за ПЕРИОД*/
                ,
            CAUSE_NOT /*текущее время сервера*/
                , HALF_PERIOD, FULL_PERIOD, CAUSE_PERIOD_DAY
        };
        public MODE_CURINTERVAL[] m_modeCurIntervals;

        public HHandlerDbULoaderDatetimeSrc(string dtDBFormat, params MODE_CURINTERVAL[] modeCurIntervals)
            : base()
        {
            m_strDateTimeDBFormat = dtDBFormat;
            m_modeCurIntervals = new MODE_CURINTERVAL[(int)INDEX_MODE_CURINTERVAL.COUNT];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP];
        }

        public HHandlerDbULoaderDatetimeSrc(PlugInULoader iPlugIn, string dtDBFormat, params MODE_CURINTERVAL[] modeCurIntervals)
            : base(iPlugIn)
        {
            m_strDateTimeDBFormat = dtDBFormat;
            m_modeCurIntervals = new MODE_CURINTERVAL[(int)INDEX_MODE_CURINTERVAL.COUNT];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP];
        }
        /// <summary>
        /// Класс для описания группы сигналов источника
        /// </summary>
        protected abstract class GroupSignalsDatetimeSrc : GroupSignalsSrc
        {
            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }
            private DateTime m_dtBegin;
            public DateTime DateTimeBegin
            {
                get { return m_dtBegin; }
                set { m_dtBegin = value; }
            }

            public GroupSignalsDatetimeSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart =
                m_dtBegin =
                    DateTime.MinValue;
            }

            public override void Stop()
            {
                base.Stop();

                DateTimeStart =
                DateTimeBegin =
                    DateTime.MinValue;
            }
            /// <summary>
            /// Строка для условия "по дате/времени"
            ///  - начало
            /// </summary>
            protected virtual string DateTimeBeginFormat
            {
                get
                {
                    string strRes = string.Empty;
                    long msec = -1L
                        , msecDiff = -1L;

                    msec = (long)(_parent as HHandlerDbULoaderDatetimeSrc).m_tsUTCOffsetToServer.Value.TotalMilliseconds;
                    if (Math.Abs(msec) > 1)
                        msecDiff = msec;
                    else
                        msecDiff = 0L;

                    switch (Mode)
                    {
                        case MODE_WORK.CUR_INTERVAL:
                            msec = (long)(_parent as HHandlerDbULoaderDatetimeSrc).m_tsCurIntervalOffset.Value.TotalMilliseconds;
                            if (Math.Abs(msec) > 1)
                                msecDiff += msec;
                            else
                                ;
                            break;
                        case MODE_WORK.COSTUMIZE:
                            break;
                        default:
                            break;
                    }

                    strRes = DateTimeBegin.AddMilliseconds(msecDiff).ToString((_parent as HHandlerDbULoaderDatetimeSrc).m_strDateTimeDBFormat, CultureInfo.InvariantCulture);

                    return strRes;
                }
            }
            /// <summary>
            /// Строка для условия "по дате/времени"
            ///  - окончание
            /// </summary>
            protected virtual string DateTimeEndFormat
            {
                get
                {
                    string strRes = string.Empty;
                    long msec = -1L
                        , msecDiff = -1L;
                    //int pday = 1;

                    msec = (long)(_parent as HHandlerDbULoaderDatetimeSrc).m_tsUTCOffsetToServer.Value.TotalMilliseconds;
                    if (Math.Abs(msec) > 1)
                        msecDiff = msec;
                    else
                        msecDiff = 0L;

                    switch (Mode)
                    {
                        case MODE_WORK.CUR_INTERVAL:
                            msec = (long)(_parent as HHandlerDbULoaderDatetimeSrc).m_tsCurIntervalOffset.Value.TotalMilliseconds;
                            if (Math.Abs(msec) > 1)
                                msecDiff += msec;
                            else
                                ;
                            msecDiff += (long)(PeriodMain.TotalMilliseconds); //'PeriodLocal' тоже валиден, т.к. они равны                         
                            break;
                        case MODE_WORK.COSTUMIZE:
                            msecDiff += (long)(PeriodLocal.TotalMilliseconds);
                            break;
                        default:
                            break;
                    }
                    //if (PeriodMain.Days == pday)
                    //    strRes = DateTimeStart.ToString((_parent as HHandlerDbULoaderDatetimeSrc).m_strDateTimeDBFormat, CultureInfo.InvariantCulture);
                    //else
                    strRes = DateTimeBegin.AddMilliseconds(msecDiff).ToString((_parent as HHandlerDbULoaderDatetimeSrc).m_strDateTimeDBFormat, CultureInfo.InvariantCulture);
                    //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

                    return strRes;
                }
            }

            protected override bool isUpdateQuery(int cntRec)
            {
                bool bRes = false;

                switch (Mode)
                {
                    case MODE_WORK.CUR_INTERVAL:
                        if (((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_HOUR)
                            || ((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_MINUTE))
                            bRes = ((RowCountRecieved < 0) || (RowCountRecieved == cntRec)) && ((!(EvtActualizeDateTimeBegin == null)) && (EvtActualizeDateTimeBegin() == 1));
                        else
                            if ((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_NOT)
                                bRes = (!(EvtActualizeDateTimeBegin == null)) && (EvtActualizeDateTimeBegin() == 1);
                            else
                                ; //throw new Exception (@"Неизвестный режим...")
                        break;
                    case MODE_WORK.COSTUMIZE:
                        bRes = (!(EvtActualizeDateTimeBegin == null)) && (EvtActualizeDateTimeBegin() == 1);
                        break;
                    default:
                        ; //throw new Exception (@"Неизвестный режим...")
                        break;
                }

                return bRes;
            }
            /// <summary>
            /// Событие для инициирования процесса актуализации даты/времени объекта
            /// </summary>
            private event IntDelegateFunc EvtActualizeDateTimeBegin;
            /// <summary>
            /// Установить метод для актуализации начальной даты/времени для опроса
            /// </summary>
            /// <param name="fActualize">Функция-делегат для актуализации начальной даты/времени для опроса</param>
            public void SetDelegateActualizeDateTimeBegin(IntDelegateFunc fActualize)
            {
                //Установить обработчик только один раз
                if (EvtActualizeDateTimeBegin == null)
                {
                    EvtActualizeDateTimeBegin += fActualize;
                }
                else
                    ;

                ////??? - вызов при инициализации (сброс каждый раз...)
                //RowCountRecieved = -1;
            }
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = base.Initialize(id, pars);

            //Повторная проверка назначения массива параметров
            try
            {
                if (m_dictGroupSignals.Keys.Contains(id) == true)
                    //Сигналы д.б. инициализированы
                    if (m_dictGroupSignals[id].Signals == null)
                        ;
                    else
                        if (pars[0].GetType().IsArray == true)
                            ;
                        else
                        {//Считать переданные параметры - параметрами группы сигналов
                            lock (m_lockStateGroupSignals)
                            {
                                if (m_dictGroupSignals[id].Mode == MODE_WORK.COSTUMIZE)
                                    if ((!(((DateTime)pars[1] == null)))
                                        && (!(((DateTime)pars[1] == DateTime.MinValue))))
                                    {
                                        ((GroupSignalsDatetimeSrc)m_dictGroupSignals[id]).DateTimeStart = (DateTime)pars[1];
                                        //m_dictGroupSignals[id].MSecIntervalLocal *= 1000; //Т.к. для реж. 'COSTUMIZE' - секунды
                                    }
                                    else
                                        ;
                                else
                                    ;
                            }

                            //Logging.Logg().Debug(@"HHandlerDbULoaderDatetimeSrc::Initialize () - параметры группы сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"HHandlerDbULoaderDatetimeSrc::Initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                iRes = -1;
            }

            if (m_dictGroupSignals.Keys.Contains(id) == true)
                (m_dictGroupSignals[id] as GroupSignalsDatetimeSrc).SetDelegateActualizeDateTimeBegin(actualizeDateTimeBegin);
            else
                ;

            
            // = Convert.ToInt32(m_dictAdding[@"CUR_INTERVAL_OFFSET"]);
            m_tsCurIntervalOffset = HTimeSpan.NotValue;
            if (m_dictAdding.ContainsKey(@"CUR_INTERVAL_OFFSET") == true)
                m_tsCurIntervalOffset = new HTimeSpan(m_dictAdding[@"CUR_INTERVAL_OFFSET"]);
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
        /// Дата/время начала опроса для текущей (обрабатываемой) группы
        /// </summary>
        protected DateTime DateTimeBegin
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeBegin
                        //+ TimeSpan.FromMilliseconds (CountMSecInterval * MSecInterval)
                        ;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeBegin.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeBegin = value;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeBegin.set ...");
            }
        }
        /// <summary>
        /// Актулизировать дату/время начала опроса
        /// </summary>
        /// <returns>Признак изменения даты/времени начала опроса</returns>
        protected virtual int actualizeDateTimeBegin()
        {
            int iRes = 0
                , denum = 0
                //, pday = 1
                ;

            if (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] == MODE_CURINTERVAL.FULL_PERIOD)
                // использовать полный период
                denum = 1;
            else
                if (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] == MODE_CURINTERVAL.HALF_PERIOD)
                    // использовать полу/период
                    denum = 2;
                else
                    ;

            if (Mode == MODE_WORK.CUR_INTERVAL)
                ////"сутки"
                //if (PeriodMain.Days == pday)
                //{
                //    DateTimeBegin = (m_dtServer - m_dtServer.TimeOfDay);
                //    DateTimeStart = m_dtServer;
                //    iRes = 1;
                //}
                //else
                //{
                    //Проверить признак 1-го запуска (в режиме CUR_INTERVAL)
                    if (DateTimeBegin == DateTime.MinValue)
                    {
                        if (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_NOT)
                            DateTimeBegin = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds / denum);
                        else
                        {
                            DateTimeBegin = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));

                            if (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_MINUTE)
                                //Выравнивание по "минуте"
                                ; // уже выполнено
                            else
                                if (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_HOUR)
                                    //Выравнивание по "час"
                                    DateTimeBegin = DateTimeBegin.AddSeconds(-1 * (DateTimeBegin.Minute * 60 + DateTimeBegin.Second));
                                else
                                    ;
                        }
                        //Установить признак перехода
                        iRes = 1;
                    }

                    else
                        //Переход на очередной интервал (повторный опрос)
                        switch (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE])
                        {
                            case MODE_CURINTERVAL.CAUSE_PERIOD_HOUR:
                            case MODE_CURINTERVAL.CAUSE_PERIOD_MINUTE:
                                //Проверить необходимость изменения даты/времени
                                if ((m_dtServer - DateTimeBegin.AddSeconds(PeriodLocal.TotalSeconds)).TotalMilliseconds > MSecIntervalLocal)
                                {
                                    DateTimeBegin = DateTimeBegin.AddSeconds(PeriodLocal.TotalSeconds);
                                    //CountMSecInterval++;
                                    //Установить признак перехода
                                    iRes = 1;
                                }
                                else
                                    ;
                                break;
                            case MODE_CURINTERVAL.CAUSE_NOT:
                                DateTimeBegin = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds / denum);
                                //Установить признак перехода
                                iRes = 1;
                                break;
                            default:
                                break;
                        }
                //}
            else
                if (Mode == MODE_WORK.COSTUMIZE)
                {
                    //Проверить признак 1-го запуска (в режиме COSTUMIZE)
                    if (DateTimeBegin == DateTime.MinValue)
                    {
                        //Проверить указано ли дата/время начала опроса
                        if (DateTimeStart == DateTime.MinValue)
                            //Не указано - опросить ближайший к текущей дате/времени период
                            DateTimeStart = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds);
                        else
                            ;

                        DateTimeBegin = DateTimeStart;
                    }
                    else
                        //Повторный опрос
                        DateTimeBegin = DateTimeBegin.AddMilliseconds(PeriodLocal.TotalMilliseconds);

                    iRes = 1;
                }
                else
                    throw new Exception(@"HHandlerDbULoaderDatetimeSrc::actualizeDateTimeStart () - неизвестный режим ...");

            Logging.Logg().Debug(@"HHandlerDbULoader::actualizeDateTimeStart () - "
                                + @"[" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]"
                                + @", m_dtServer=" + m_dtServer.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", DateTimeBegin=" + DateTimeBegin.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", iRes=" + iRes
                                + @"...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        protected override int IdGroupSignalsCurrent { get { return base.IdGroupSignalsCurrent; } set { if (value == -1) completeGroupSignalsCurrent(); else ; base.IdGroupSignalsCurrent = value; } }

        private void completeGroupSignalsCurrent()
        {
            if (State == GroupSignals.STATE.SLEEP)
                if (!((DateTimeBegin + TimeSpan.FromMilliseconds(PeriodLocal.TotalMilliseconds)) > (DateTimeStart + PeriodMain)))
                    ////Поставить в очередь для обработки идентификатор группы сигналов
                    //push(IdGroupSignalsCurrent)
                    ;
                else
                    //Остановить сбор данных
                    new Thread(new ParameterizedThreadStart(new DelegateObjectFunc(stop))).Start(IdGroupSignalsCurrent);
            else
                ;
        }

        private void stop(object id)
        {
            Stop((int)id, ID_HEAD_ASKED_HOST.GET);
        }
    }

    //public abstract class HHandlerDbULoaderMSTTMSrc : HHandlerDbULoaderDatetimeSrc
    //{
    //    public TimeSpan m_tsCurIntervalOffset
    //        , m_tsUTCOffset;

    //    public HHandlerDbULoaderMSTTMSrc()
    //        : base(@"yyyy/MM/dd HH:mm:ss")
    //    {
    //    }

    //    public HHandlerDbULoaderMSTTMSrc(PlugInULoader iPlugIn)
    //        : base(iPlugIn, @"yyyy/MM/dd HH:mm:ss")
    //    {
    //    }

    //    public override int Initialize(object[] pars)
    //    {
    //        int iRes = base.Initialize(pars);

    //        if (m_dictAdding.ContainsKey(@"CUR_INTERVAL_OFFSET") == true)
    //            m_iCurIntervalShift = Convert.ToInt32(m_dictAdding[@"CUR_INTERVAL_OFFSET"]);
    //        else
    //            ;

    //        return iRes;
    //    }

    //    protected abstract class GroupSignalsMSTTMSrc : GroupSignalsDatetimeSrc
    //    {
    //        public GroupSignalsMSTTMSrc(HHandlerDbULoader parent, int id, object[] pars)
    //            : base(parent, id, pars)
    //        {
    //        }
    //        /// <summary>
    //        /// Строки для условия "по дате/времени" - начало
    //        /// </summary>
    //        protected override string DateTimeBeginFormat
    //        {
    //            get
    //            {
    //                string strRes = string.Empty;
    //                long msecDiff = -1;

    //                switch (Mode)
    //                {
    //                    case MODE_WORK.CUR_INTERVAL:
    //                        msecDiff = (long)((_parent as HHandlerDbULoaderMSTTMSrc).m_tsCurIntervalOffset.TotalMilliseconds);
    //                        break;
    //                    case MODE_WORK.COSTUMIZE:
    //                        msecDiff = 0; //MSecInterval;
    //                        break;
    //                    default:
    //                        break;
    //                }

    //                strRes = DateTimeBegin.AddSeconds(m_tsUTCOffset.TotalSeconds).AddMilliseconds(msecDiff).ToString((_parent as HHandlerDbULoaderMSTTMSrc).m_strDateTimeDBFormat);
    //                //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

    //                return strRes;
    //            }
    //        }
    //        /// <summary>
    //        /// Строки для условия "по дате/времени" - окончание
    //        /// </summary>
    //        protected override string DateTimeEndFormat
    //        {
    //            get
    //            {
    //                string strRes = string.Empty;
    //                long msecDiff = -1;

    //                switch (Mode)
    //                {
    //                    case MODE_WORK.CUR_INTERVAL:
    //                        msecDiff = 0;                          
    //                        break;
    //                    case MODE_WORK.COSTUMIZE:
    //                        msecDiff = (int)PeriodLocal.TotalMilliseconds;
    //                        break;
    //                    default:
    //                        break;
    //                }

    //                strRes = DateTimeBegin.AddHours(-6).AddMilliseconds(msecDiff).ToString((_parent as HHandlerDbULoaderMSTTMSrc).m_strDateTimeDBFormat);
    //                //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

    //                return strRes;
    //            }
    //        }
    //    }
    //}

    public abstract class HHandlerDbULoaderMSTIDsql : HHandlerDbULoaderDatetimeSrc
    {
        public HHandlerDbULoaderMSTIDsql(params MODE_CURINTERVAL[] modeCurIntervals)
            : base(@"yyyyMMdd HH:mm:ss", modeCurIntervals)
        {
        }

        public HHandlerDbULoaderMSTIDsql(PlugInULoader iPlugIn, params MODE_CURINTERVAL[] modeCurIntervals)
            : base(iPlugIn, @"yyyyMMdd HH:mm:ss", modeCurIntervals)
        {
        }

        public override void ClearValues()
        {
        }

        protected abstract class GroupSignalsMSTIDsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsMSTIDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG
                return new SIGNALIdsql((int)objs[0], (int)objs[2], bool.Parse((string)objs[3]));
            }

            protected override object getIdMain(object id_mst)
            {
                int iRes = -1;

                foreach (SIGNALIdsql sgnl in m_arSignals)
                    if (sgnl.m_iIdLocal == (int)id_mst)
                    {
                        iRes = sgnl.m_idMain;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            //public override DataTable TableRecieved
            //{
            //    get { return base.TableRecieved; }

            //    set
            //    {
            //        //Требуется добавить идентификаторы 'id_main'
            //        if ((!(value == null)) && (!(value.Columns.IndexOf(@"ID") < 0)))
            //        {
            //            DataTable tblVal = value.Copy();
            //            tblVal.Columns.Add(@"ID_MST", typeof(int));

            //            foreach (DataRow r in tblVal.Rows)
            //            {
            //                r[@"ID_MST"] = r[@"ID"];
            //                r[@"ID"] = getIdMain((int)r[@"ID_MST"]);
            //            }

            //            base.TableRecieved = tblVal;
            //        }
            //        else
            //        {
            //            base.TableRecieved = value;
            //        }
            //    }
            //}
        }
    }
}
