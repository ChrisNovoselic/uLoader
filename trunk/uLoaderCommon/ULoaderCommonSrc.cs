using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;

using HClassLibrary;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderSrc : HHandlerDbULoader, ILoaderSrc
    {
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
            public string Query { get { return m_strQuery; } set { m_strQuery = value; } }
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
        private int RowCountRecieved { get { return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved; } set { (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved = value; } }
        //Строка запроса для текущей (обрабатываемой) группы
        private string Query { get { return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).Query; } }

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
                        RowCountRecieved = table.Rows.Count;

                        //msg = @"Получено строк [" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count;
                        //Console.WriteLine (msg);
                        //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

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

            Logging.Logg ().Error (@"[" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]" + msgErr, Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine(@"Ошибка. " + msgErr);

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
        /// <summary>
        /// Перечисление - идентификаторы режимов вычисления даты/времени начала опроса
        /// </summary>
        public enum MODE_CURINTERVAL { CAUSE_PERIOD /*округление до ПЕРИОД, ожидание полного набора записей за ПЕРИОД*/, CAUSE_NOT /*текущее время сервера*/ };
        public static MODE_CURINTERVAL s_modeCurInterval = MODE_CURINTERVAL.CAUSE_NOT;

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
            private DateTime m_dtBegin;
            public DateTime DateTimeBegin { get { return m_dtBegin; } set { m_dtBegin = value; } }

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

                    switch (Mode)
                    {
                        case MODE_WORK.CUR_INTERVAL:
                            strRes = DateTimeBegin.ToString(@"yyyyMMdd HHmmss");
                            //switch (
                            //    //((HHandlerDbULoaderDatetimeSrc)_parent).s_modeCurInterval
                            //    HHandlerDbULoaderDatetimeSrc.s_modeCurInterval
                            //    )
                            //{
                            //    case MODE_CURINTERVAL.CAUSE_PERIOD:
                            //        break;
                            //    case MODE_CURINTERVAL.CAUSE_NOT:
                            //        break;
                            //    default:
                            //        break;
                            //}
                            break;
                        case MODE_WORK.COSTUMIZE:
                            strRes = DateTimeBegin.ToString(@"yyyyMMdd HHmmss");
                            break;
                        default:
                            break;
                    }

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
                    long msecDiff = -1;

                    switch (Mode)
                    {
                        case MODE_WORK.CUR_INTERVAL:
                            msecDiff = (long)(PeriodMain.TotalMilliseconds); //'PeriodLocal' тоже валиден, т.к. они равны
                            //switch (HHandlerDbULoaderDatetimeSrc.s_modeCurInterval)
                            //{
                            //    case MODE_CURINTERVAL.CAUSE_PERIOD:
                            //        break;
                            //    case MODE_CURINTERVAL.CAUSE_NOT:
                            //        break;
                            //    default:
                            //        break;
                            //}                            
                            break;
                        case MODE_WORK.COSTUMIZE:
                            msecDiff = (long)(PeriodLocal.TotalMilliseconds);
                            break;
                        default:
                            break;
                    }

                    strRes = DateTimeBegin.AddMilliseconds(msecDiff).ToString(@"yyyyMMdd HHmmss");
                    //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

                    return strRes;
                }
            }

            protected override bool isUpdateQuery (int cntRec)
            {
                bool bRes = false;

                switch (Mode)
                {
                    case MODE_WORK.CUR_INTERVAL:
                        if (s_modeCurInterval == MODE_CURINTERVAL.CAUSE_PERIOD)
                            bRes = ((RowCountRecieved < 0) || (RowCountRecieved == cntRec)) && ((!(EvtActualizeDateTimeBegin == null)) && (EvtActualizeDateTimeBegin() == 1));
                        else
                            if (s_modeCurInterval == MODE_CURINTERVAL.CAUSE_NOT)
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

                            Logging.Logg().Debug(@"HHandlerDbULoaderDatetimeSrc::Initialize () - параметры группы сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoaderDatetimeSrc::Initialize () - ...");

                iRes = -1;
            }

            if (m_dictGroupSignals.Keys.Contains(id) == true)
                (m_dictGroupSignals[id] as GroupSignalsDatetimeSrc).SetDelegateActualizeDateTimeBegin(actualizeDateTimeBegin);
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
        protected int actualizeDateTimeBegin()
        {
            int iRes = 0;
            if (Mode == MODE_WORK.CUR_INTERVAL)
                //Проверить признак 1-го запуска (в режиме CUR_INTERVAL)
                if (DateTimeBegin == DateTime.MinValue)
                {
                    if (s_modeCurInterval == MODE_CURINTERVAL.CAUSE_PERIOD)
                    {
                        //Выравнивание по "минуте"
                        DateTimeBegin = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
                        //CountMSecInterval = 0;
                    }
                    else
                        if (s_modeCurInterval == MODE_CURINTERVAL.CAUSE_NOT)
                            DateTimeBegin = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds / 2);
                        else
                            ;
                    
                    iRes = 1;
                }
                else
                    //Переход на очередной интервал (повторный опрос)
                    switch (s_modeCurInterval)
                    {
                        case MODE_CURINTERVAL.CAUSE_PERIOD:
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
                            DateTimeBegin = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds / 2);
                            //Установить признак перехода
                            iRes = 1;
                            break;
                        default:
                            break;
                    }
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
                if (! ((DateTimeBegin + TimeSpan.FromMilliseconds(PeriodLocal.TotalMilliseconds)) > (DateTimeStart + PeriodMain)))
                    ////Поставить в очередь для обработки идентификатор группы сигналов
                    //push(IdGroupSignalsCurrent)
                        ;
                else
                    //Остановить сбор данных
                    new Thread(new ParameterizedThreadStart(new DelegateObjectFunc (stop))).Start (IdGroupSignalsCurrent);
            else
                ;
        }

        private void stop (object id)
        {
            Stop ((int)id);
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
            protected override string DateTimeBeginFormat
            {
                get
                {
                    string strRes = string.Empty;
                    long msecDiff = -1;

                    switch (Mode)
                    {
                        case MODE_WORK.CUR_INTERVAL:
                            msecDiff = (long)(1000 * (_parent as HHandlerDbULoaderMSTTMSrc).m_iCurIntervalShift * (int)PeriodLocal.TotalSeconds);                          
                            break;
                        case MODE_WORK.COSTUMIZE:
                            msecDiff = 0; //MSecInterval;
                            break;
                        default:
                            break;
                    }

                    strRes = DateTimeBegin.AddHours(-6).AddMilliseconds(msecDiff).ToString(@"yyyy/MM/dd HH:mm:ss");
                    //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

                    return strRes;
                }
            }
            // окончание
            protected override string DateTimeEndFormat
            {
                get
                {
                    string strRes = string.Empty;
                    long msecDiff = -1;

                    switch (Mode)
                    {
                        case MODE_WORK.CUR_INTERVAL:
                            msecDiff = 0;                          
                            break;
                        case MODE_WORK.COSTUMIZE:
                            msecDiff = (int)PeriodLocal.TotalMilliseconds;
                            break;
                        default:
                            break;
                    }

                    strRes = DateTimeBegin.AddHours(-6).AddMilliseconds(msecDiff).ToString(@"yyyy/MM/dd HH:mm:ss");
                    //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

                    return strRes;
                }
            }
        }
    }
}
