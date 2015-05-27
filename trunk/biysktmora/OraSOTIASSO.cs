using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;

namespace biysktmora
{    
    public class HBiyskTMOra : HHandlerDb
    {
        protected IPlugIn _iPlugin;

        private class GroupSignals
        {
            public enum STATE { UNKNOWN = -1, SLEEP, TIMER, QUEUE, ACTIVE }
            private STATE m_state;
            public STATE State { get { return m_state; } set { m_state = value; } }

            private uLoaderCommon.MODE_WORK m_mode;
            public uLoaderCommon.MODE_WORK Mode { get { return m_mode; } set { m_mode = value; } }

            public struct SIGNAL
            {
                public int m_id;
                public string m_NameTable;

                public SIGNAL(int id, string table)
                {
                    this.m_id = id;
                    this.m_NameTable = table;
                }
            }

            private SIGNAL[] m_arSignals;
            public SIGNAL[] Signals { get { return m_arSignals; } }

            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }
            private TimeSpan m_tmSpanPeriod;
            public TimeSpan TimeSpanPeriod { get { return m_tmSpanPeriod; } set { m_tmSpanPeriod = value; } }
            private long m_msecInterval;
            public long MSecInterval { get { return m_msecInterval; } set { m_msecInterval = value; } }
            private long m_msecRemaindToActivate;
            public long MSecRemaindToActivate { get { return m_msecRemaindToActivate; } set { m_msecRemaindToActivate = value; } }

            private DataTable m_tableResults;
            public DataTable TableResults { get { return m_tableResults; } set { m_tableResults = value; } }

            public GroupSignals(object [] pars)
            {
                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart = DateTime.MinValue;
                m_tmSpanPeriod = new TimeSpan((long)((int)uLoaderCommon.DATETIME.SEC_SPANPERIOD_DEFAULT * Math.Pow(10, 7)));
                m_msecInterval = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_DEFAULT;
                //Инициализировать массив сигналов
                if (pars.Length > 0)
                {
                    m_arSignals = new SIGNAL[pars.Length];                

                    for (int i = 0; i < pars.Length; i ++)
                        m_arSignals[i] = new SIGNAL((int)(pars[i] as object[])[0], (pars[i] as object[])[2] as string);
                }
                else
                    ;

                /*m_arSignals = null; new SIGNAL[]
                                    { new SIGNAL(20049, @"TAG_000049")
                                        , new SIGNAL(20002, @"TAG_000047")
                                        , new SIGNAL(20003, @"TAG_000048")
                                        , new SIGNAL(20004, @"TAG_000049")
                                        , new SIGNAL(20005, @"TAG_000050")
                                        , new SIGNAL(20006, @"TAG_000051")
                                        , new SIGNAL(20007, @"TAG_000052")
                                        , new SIGNAL(20008, @"TAG_000053")
                                    };*/
            }

            public static STATE GetMode (uLoaderCommon.MODE_WORK mode, STATE prevState)
            {
                GroupSignals.STATE stateRes = GroupSignals.STATE.UNKNOWN;
                if (mode == uLoaderCommon.MODE_WORK.CUR_INTERVAL)
                    switch (prevState)
                    {
                        case STATE.ACTIVE:
                        case STATE.UNKNOWN:
                            stateRes = GroupSignals.STATE.TIMER;
                            break;
                        default:
                            stateRes = GroupSignals.STATE.SLEEP;
                            break;
                    }
                else
                    if (mode == uLoaderCommon.MODE_WORK.COSTUMIZE)
                        stateRes = GroupSignals.STATE.SLEEP;
                    else
                        //??? throw new Exception
                        ;

                return stateRes;
            }
        }

        private Dictionary<int, GroupSignals> m_dictGroupSignals;

        private GroupSignals.STATE State { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].State; }  set { m_dictGroupSignals[m_IdGroupSignalsCurrent].State = value; } }

        private uLoaderCommon.MODE_WORK Mode { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].Mode; } /*set { m_dictGroupSignals[m_IdGroupSignalsCurrent].Mode = value; }*/ }
        
        private GroupSignals.SIGNAL[] Signals { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].Signals; } }

        private TimeSpan TimeSpanPeriod { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].TimeSpanPeriod; } }
        private DateTime DateTimeStart { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].DateTimeStart; } set { m_dictGroupSignals[m_IdGroupSignalsCurrent].DateTimeStart = value; } }
        private long MSecInterval { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecInterval; } }
        private long MSecRemaindToActivate { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecRemaindToActivate; } set { m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecRemaindToActivate = value; } }

        public DataTable TableResults { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults; } set { m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults = value; } }

        enum StatesMachine {
            CurrentTime
            , Values
        }

        private DateTime m_dtServer;
        private int m_msecIntervalTimerActivate;
        public ConnectionSettings m_connSett;
        public string m_strQuery = string.Empty;
        private int m_IdGroupSignalsCurrent;

        private object m_lockStateGroupSignals
            , m_lockQueue;
        private System.Threading.Timer m_timerActivate;
        private Thread m_threadQueue;
        private Queue <int> m_queueIdGroupSignals;
        private int threadQueueIsWorking;
        private Semaphore m_semaQueue;

        public HBiyskTMOra()
        {
            m_dtServer = DateTime.MinValue;
            m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;

            m_IdGroupSignalsCurrent = -1;
            m_dictGroupSignals = new Dictionary<int, GroupSignals>();

            m_lockStateGroupSignals = new object();

            //m_threadQueue = //Создание при "старте"
            m_lockQueue = new object ();
            m_queueIdGroupSignals = new Queue<int> ();
            threadQueueIsWorking = -1;
            m_semaQueue = null; //Создание при "старте"
        }

        public HBiyskTMOra(IPlugIn iPlugIn) : this ()
        {
            this._iPlugin = iPlugIn;
        }

        public int Initialize(ConnectionSettings connSett)
        {
            int iRes = 0;

            m_connSett = new ConnectionSettings (connSett);

            return iRes;
        }

        public int Initialize(int id, object [] pars)
        {
            int iRes = 0;

            if (m_dictGroupSignals.Keys.Contains(id) == false)
                //Считать переданные параметры - параметрами сигналов
                m_dictGroupSignals.Add(id, new GroupSignals(pars));
            else
            {//Считать переданные параметры - параметрами группы сигналов
                if (m_dictGroupSignals[id].Signals == null)
                    iRes = -1;
                else
                {                
                    lock (m_lockStateGroupSignals)
                    {
                        m_dictGroupSignals[id].Mode = (uLoaderCommon.MODE_WORK)pars [0];
                        m_dictGroupSignals[id].State = GroupSignals.GetMode(m_dictGroupSignals[id].Mode, GroupSignals.STATE.UNKNOWN);
                        //m_dictGroupSignals[id].DateTimeStart = (DateTime)pars[1];
                        //m_dictGroupSignals[id].TimeSpanPeriod = TimeSpan.FromSeconds((double)pars[2]);
                        //m_dictGroupSignals[id].MSecInterval = (int)pars[3];
                    }
                }
            }

            return iRes;
        }

        private void fTimerActivate (object obj)
        {
            lock (m_lockStateGroupSignals)
            {
                //Проверить наличие "активных" групп сигналов
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
                    if (pair.Value.State == GroupSignals.STATE.ACTIVE)
                        return;
                    else
                        ;
                //Проверить наличие ожидающих обработки групп сигналов
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
                    if (pair.Value.State == GroupSignals.STATE.QUEUE)
                    {
                        pair.Value.State = GroupSignals.STATE.ACTIVE;                        
                        return;
                    }
                    else
                        ;
                bool bActivated = false; //Признак установки состояния "активное" для одной из групп
                //Перевести в состояние "активное" ("ожидание") группы сигналов
                foreach (KeyValuePair <int,GroupSignals> pair in m_dictGroupSignals)
                    if (pair.Value.State == GroupSignals.STATE.TIMER)
                    {
                        pair.Value.MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        if (pair.Value.MSecRemaindToActivate < 0)
                        {
                            if (bActivated == false)
                            {
                                pair.Value.State = GroupSignals.STATE.ACTIVE;
                                bActivated = true;

                                lock (m_lockQueue)
                                {
                                    m_queueIdGroupSignals.Enqueue(pair.Key);

                                    if (m_queueIdGroupSignals.Count == 1)
                                        m_semaQueue.Release (1);
                                    else
                                        ;
                                }
                            }
                            else
                                pair.Value.State = GroupSignals.STATE.QUEUE;
                        }
                        else
                            ;
                    }
                    else
                        ;
            }
        }

        /// <summary>
        /// Потоковая функция очереди обработки объектов с событиями
        /// </summary>
        private void fThreadQueue()
        {
            bool bRes = false;

            while (!(threadQueueIsWorking < 0))
            {
                bRes = false;
                //Ожидать когда появятся объекты для обработки
                bRes = m_semaQueue.WaitOne();

                while (true)
                {
                    lock (m_lockQueue)
                    {
                        if (m_queueIdGroupSignals.Count == 0)
                            //Прерват, если обработаны все объекты
                            break;
                        else
                            ;
                    }
                    //Получить объект очереди событий
                    m_IdGroupSignalsCurrent = m_queueIdGroupSignals.Peek ();

                    lock (m_lockState)
                    {
                        //Очистить все состояния
                        ClearStates();
                        //Добавить все состояния
                        AddState((int)StatesMachine.CurrentTime);
                        AddState((int)StatesMachine.Values);
                    }

                    //Обработать все состояния
                    Run(@"HBiyskTMOra::fThreadQueue ()");

                    //Ожидать обработки всех состояний
                    m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne(System.Threading.Timeout.Infinite);

                    lock (m_lockQueue)
                    {
                        //Удалить объект очереди событий (обработанный)
                        m_queueIdGroupSignals.Dequeue();
                    }

                    GroupSignals.STATE newState = GroupSignals.GetMode (Mode, State);

                    lock (m_lockStateGroupSignals)
                    {                        
                        State = newState;
                        MSecRemaindToActivate = MSecInterval; //(long)TimeSpanPeriod.TotalMilliseconds;
                    }

                    ((HHPlugIn)_iPlugin).DataAskedHost(new object[] { ID_DATA_ASKED_HOST.TABLE_RES, m_IdGroupSignalsCurrent, TableResults });

                    m_IdGroupSignalsCurrent = -1;
                }
            }
            //Освободить ресурс ядра ОС
            if (bRes == true)
                try
                {
                    m_semaQueue.Release(1);
                }
                catch (Exception e)
                { //System.Threading.SemaphoreFullException
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "HHandler::fThreadQueue () - semaState.Release(1)");
                }
            else
                ;
        }

        private void setQuery(DateTime dtStart, int secInterval = -1)
        {
            m_strQuery = string.Empty;            

            if (secInterval  < 0)
            {
                secInterval =
                    //SEC_INTERVAL_DEFAULT
                    (int)TimeSpanPeriod.TotalSeconds
                    ;
            }
            else
                ;

            string strUnion = @" UNION "
                //Строки для условия "по дате/времени"
                , strStart = dtStart.ToString(@"yyyyMMdd HHmmss")
                , strEnd = dtStart.AddSeconds(secInterval).ToString(@"yyyyMMdd HHmmss");
            //Формировать зпрос
            foreach (GroupSignals.SIGNAL s in Signals)
            {
                m_strQuery += @"SELECT " + s.m_id + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.m_NameTable + @" WHERE DATETIME BETWEEN"
                    + @" to_timestamp('" + strStart + @"', 'yyyymmdd hh24miss')" + @" AND"
                    + @" to_timestamp('" + strEnd + @"', 'yyyymmdd hh24miss')"
                    + strUnion
                ;
            }

            //Удалить "лишний" UNION
            m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
            ////Установить сортировку
            //m_strQuery += @" ORDER BY DATETIME DESC";
        }

        public void ChangeState ()
        {
            ClearStates ();

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            Run(@"HBiyskTMOra::ChangeState ()");
        }

        public override void StartDbInterfaces ()
        {            
            foreach (int id in m_dictGroupSignals.Keys)
                register(id, 0, m_connSett, string.Empty);
        }

        protected override void register(int id, int indx, ConnectionSettings connSett, string name)
        {
            bool bReq = true;
            
            if (m_dictIdListeners.ContainsKey (id) == false)
                m_dictIdListeners.Add(id, new int[] { -1 });
            else
                if (! (m_dictIdListeners[id][indx] < 0))
                    bReq = false;
                else
                    ;

            if (bReq == true)
                base.register(id, indx, connSett, name);
            else
                ;
        }

        public override void Start()
        {
            base.Start();

            StartDbInterfaces ();

            startThreadQueue ();

            startTimerActivate();
        }

        public override void Stop()
        {            
            stopTimerActivate ();

            stopThreadQueue();

            base.Stop();
        }

        public void Stop(int id)
        {
            bool bStopped = true;

            lock (m_lockStateGroupSignals)
            {
                m_dictGroupSignals[id].State = GroupSignals.STATE.SLEEP;

                foreach (GroupSignals grpSgnls in m_dictGroupSignals.Values)
                    if ((grpSgnls.State == GroupSignals.STATE.ACTIVE)
                        || (grpSgnls.State == GroupSignals.STATE.QUEUE)
                        || (grpSgnls.State == GroupSignals.STATE.TIMER)
                        )
                    {
                        bStopped = false;

                        break;
                    }
                    else
                        ;
            }

            if (bStopped == true)
            {
                Activate(false);
                Stop();
            }
            else
                ;
        }

        private int startTimerActivate ()
        {
            int iRes = 0;

            stopTimerActivate();
            m_timerActivate = new System.Threading.Timer(fTimerActivate, null, 0, m_msecIntervalTimerActivate);

            return iRes;
        }

        private int stopTimerActivate()
        {
            int iRes = 0;

            if (! (m_timerActivate == null))
            {
                m_timerActivate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerActivate.Dispose();
                m_timerActivate = null;
            }
            else
                ;

            return iRes;
        }

        private int startThreadQueue()
        {
            int iRes = 0;

            if (threadQueueIsWorking < 0)
            {
                threadQueueIsWorking = 0;

                m_threadQueue = new Thread(new ThreadStart(fThreadQueue));
                m_threadQueue.Name = "Обработка очереди для объекта " + this.GetType().AssemblyQualifiedName;
                m_threadQueue.IsBackground = true;
                m_threadQueue.CurrentCulture =
                m_threadQueue.CurrentUICulture =
                    ProgramBase.ss_MainCultureInfo;

                m_semaQueue = new Semaphore(1, 1);

                //InitializeSyncState();
                //Установить в "несигнальное" состояние
                m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne(System.Threading.Timeout.Infinite);

                m_semaQueue.WaitOne();
                
                try { m_threadQueue.Start(); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::startThreadQueue () - ...");
                }
            }
            else
                iRes = 1;

            return iRes;
        }

        private int stopThreadQueue()
        {
            int iRes = 0;

            bool joined;
            threadQueueIsWorking = -1;
            //Очисить очередь событий
            ClearStates();
            //Прверить выполнение потоковой функции
            if ((!(m_threadQueue == null)) && (m_threadQueue.IsAlive == true))
            {
                //Выход из потоковой функции
                try { m_semaQueue.Release(1); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "HBiyskTMOra::stopThreadQueue () - m_semaQueue.Release(1)");
                }
                //Ожидать завершения потоковой функции
                joined = m_threadQueue.Join(666);
                //Проверить корректное завершение потоковой функции
                if (joined == false)
                    //Завершить аварийно потоковую функцию
                    m_threadQueue.Abort();
                else
                    ;

                m_semaQueue = null;
                m_threadQueue = null;
            }
            else ;

            return iRes;
        }

        public override void ClearValues()
        {
            int iPrev = 0, iDel = 0, iCur = 0;
            if (! (TableResults == null))
            {
                iPrev = TableResults.Rows.Count;
                string strSel =
                    @"DATETIME<'" + DateTimeStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' OR DATETIME>='" + DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    ;

                DataRow[] rowsDel = null;
                try { rowsDel = TableResults.Select(strSel); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ClearValues () - ...");
                }

                if (! (rowsDel == null))
                {
                    iDel = rowsDel.Length;
                    if (rowsDel.Length > 0)
                    {
                        foreach (DataRow r in rowsDel)
                            TableResults.Rows.Remove(r);
                        //??? Обязательно ли...
                        TableResults.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;

                iCur = TableResults.Rows.Count;

                Console.WriteLine(@"Обновление рез-та: [было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @"]");
            }
            else
                ;
        }                

        private int actualizeDatetimeStart ()
        {
            int iRes = 0;

            if (DateTimeStart == DateTime.MinValue)
            {
                DateTimeStart = m_dtServer;
                DateTimeStart = DateTimeStart.AddSeconds(-1 * DateTimeStart.Second);
            }
            else
                ;

            if ((m_dtServer - DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds)).TotalSeconds > 6)
                DateTimeStart = DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds);
            else
                ;

            return iRes;
        }

        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            return response(out error, out table);
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.Oracle, m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    break;
                case (int)StatesMachine.Values:                    
                    try
                    {
                        actualizeDatetimeStart ();
                        ClearValues();
                        setQuery(DateTimeStart);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateRequest () - ::Values - ...");
                    }
                    Request (m_dictIdListeners[0][0], m_strQuery);
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
                        Console.WriteLine(m_dtServer.ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse () - ::CurrentTime - ...");
                    }
                    break;
                case (int)StatesMachine.Values:
                    try
                    {
                        Console.WriteLine(@"Получено строк: " + (table as DataTable).Rows.Count);
                        if (TableResults == null)
                        {
                            TableResults = new DataTable();
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

                        TableResults = table.Copy ();
                    }
                    catch (Exception e)
                    {
                        Logging.Logg ().Exception (e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse () - ::Values - ...");
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

            (_iPlugin as HHPlugIn).DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.ERROR, 0, state, msgErr });
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

    public class PlugIn : HHPlugIn
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 1001;

            createObject(typeof(HBiyskTMOra));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            HBiyskTMOra target = _object as HBiyskTMOra;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT:
                    target.Initialize(ev.par [0] as ConnectionSettings);
                    break;
                case (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP:
                    target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object []);
                    break;
                case (int)ID_DATA_ASKED_HOST.START:
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT) == true)
                    {
                        if (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0)
                        {
                            target.Start();
                            target.Activate(true);
                        }
                        else
                            DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP, (int)(ev.par as object[])[0] });
                    }
                    else
                        DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT } );
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Stop((int)(ev.par as object[])[0]);
                    break;
                default:
                    break;
            }
            
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
