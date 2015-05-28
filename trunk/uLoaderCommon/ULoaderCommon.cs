using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

using HClassLibrary;

namespace uLoaderCommon
{
    public enum DATETIME
    {
        SEC_SPANPERIOD_DEFAULT = 60
        , MSEC_INTERVAL_DEFAULT = 6666
        , MSEC_INTERVAL_TIMER_ACTIVATE = 66
    }
    /// <summary>
    /// Перечисление для типов опроса
    /// </summary>
    public enum MODE_WORK
    {
        UNKNOWN = -1
        , CUR_INTERVAL // по текущему интервалу
        , COSTUMIZE // выборочно (история)
            , COUNT_MODE_WORK
    }

    public abstract class HHandlerDbULoader : HHandlerDb
    {
        protected IPlugIn _iPlugin;

        protected abstract class GroupSignals
        {
            public enum STATE { UNKNOWN = -1, SLEEP, TIMER, QUEUE, ACTIVE }
            private STATE m_state;
            public STATE State { get { return m_state; } set { m_state = value; } }

            private uLoaderCommon.MODE_WORK m_mode;
            public uLoaderCommon.MODE_WORK Mode { get { return m_mode; } set { m_mode = value; } }

            public class SIGNAL
            {
                public int m_idMain;
                //public string m_NameTable;

                public SIGNAL(int idMain)
                {
                    this.m_idMain = idMain;
                    //this.m_NameTable = table;
                }
            }

            protected SIGNAL[] m_arSignals;
            public SIGNAL[] Signals { get { return m_arSignals; } }

            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }
            private TimeSpan m_tmSpanPeriod;
            public TimeSpan TimeSpanPeriod { get { return m_tmSpanPeriod; } set { m_tmSpanPeriod = value; } }
            private long m_msecInterval;
            public long MSecInterval { get { return m_msecInterval; } set { m_msecInterval = value; } }
            private long m_msecRemaindToActivate;
            public long MSecRemaindToActivate { get { return m_msecRemaindToActivate; } set { m_msecRemaindToActivate = value; } }

            public abstract DataTable TableResults { get; set; }

            public GroupSignals(object[] pars)
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

                    for (int i = 0; i < pars.Length; i++)
                        m_arSignals[i] = createSignal (pars[i] as object []);
                }
                else
                    ;
            }

            public abstract SIGNAL createSignal(object []objs);

            public static STATE GetMode(uLoaderCommon.MODE_WORK mode, STATE prevState)
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

        protected Dictionary<int, GroupSignals> m_dictGroupSignals;

        private GroupSignals.STATE State
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].State;
                else
                    throw new Exception(@"ULoaderCommon::State.get ...");
            }
            
            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    m_dictGroupSignals[m_IdGroupSignalsCurrent].State = value;
                else
                    throw new Exception(@"ULoaderCommon::State.set ...");
            }
        }

        private MODE_WORK Mode
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].Mode;
                else
                    throw new Exception(@"ULoaderCommon::Mode.get ...");
            }
            
            /*set { m_dictGroupSignals[m_IdGroupSignalsCurrent].Mode = value; }*/
        }

        protected GroupSignals.SIGNAL[] Signals
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].Signals;
                else
                    throw new Exception(@"ULoaderCommon::Signals.get ...");
            }
        }

        protected TimeSpan TimeSpanPeriod
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].TimeSpanPeriod;
                else
                    throw new Exception(@"ULoaderCommon::TimeSpanPeriod.get ...");
            }
        }

        protected DateTime DateTimeStart
        {
            get
            {
                if (! (m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].DateTimeStart;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.get ...");
            }

            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    m_dictGroupSignals[m_IdGroupSignalsCurrent].DateTimeStart = value;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.set ...");
            }
        }

        protected long MSecInterval
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecInterval;
                else
                    throw new Exception(@"ULoaderCommon::MSecInterval.get ...");
            }
        }

        protected long MSecRemaindToActivate
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecRemaindToActivate;
                else
                    throw new Exception(@"ULoaderCommon::MSecRemaindToActivate.get ...");
            }

            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecRemaindToActivate = value;
                else
                    throw new Exception(@"ULoaderCommon::MSecRemaindToActivate.set ...");
            }
        }

        public DataTable TableResults
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults;
                else
                    throw new Exception(@"ULoaderCommon::TableResults.get ...");
            }

            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults = value;
                else
                    throw new Exception(@"ULoaderCommon::TableResults.set ...");
            }
        }

        protected DateTime m_dtServer;
        private int m_msecIntervalTimerActivate;
        public ConnectionSettings m_connSett;
        public string m_strQuery = string.Empty;
        protected int m_IdGroupSignalsCurrent;

        private object m_lockStateGroupSignals
            , m_lockQueue;
        private System.Threading.Timer m_timerActivate;
        private Thread m_threadQueue;
        private Queue<int> m_queueIdGroupSignals;
        private int threadQueueIsWorking;
        private Semaphore m_semaQueue;

        public HHandlerDbULoader()
        {
            m_dtServer = DateTime.MinValue;
            m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;

            m_IdGroupSignalsCurrent = -1;
            m_dictGroupSignals = new Dictionary<int, GroupSignals>();

            m_lockStateGroupSignals = new object();

            //m_threadQueue = //Создание при "старте"
            m_lockQueue = new object();
            m_queueIdGroupSignals = new Queue<int>();
            threadQueueIsWorking = -1;
            m_semaQueue = null; //Создание при "старте"
        }

        public HHandlerDbULoader(IPlugIn iPlugIn)
            : this()
        {
            this._iPlugin = iPlugIn;
        }

        public int Initialize(ConnectionSettings connSett)
        {
            int iRes = 0;

            m_connSett = new ConnectionSettings(connSett);

            return iRes;
        }

        public int Initialize(int id, object[] pars)
        {
            int iRes = 0;

            if (m_dictGroupSignals.Keys.Contains(id) == false)
                //Считать переданные параметры - параметрами сигналов
                m_dictGroupSignals.Add(id, createGroupSignals (pars));
            else
            {//Считать переданные параметры - параметрами группы сигналов
                if (m_dictGroupSignals[id].Signals == null)
                    iRes = -1;
                else
                {
                    lock (m_lockStateGroupSignals)
                    {
                        m_dictGroupSignals[id].Mode = (uLoaderCommon.MODE_WORK)pars[0];
                        m_dictGroupSignals[id].State = GroupSignals.GetMode(m_dictGroupSignals[id].Mode, GroupSignals.STATE.UNKNOWN);
                        //m_dictGroupSignals[id].DateTimeStart = (DateTime)pars[1];
                        //m_dictGroupSignals[id].TimeSpanPeriod = TimeSpan.FromSeconds((double)pars[2]);
                        //m_dictGroupSignals[id].MSecInterval = (int)pars[3];
                    }
                }
            }

            return iRes;
        }

        protected abstract GroupSignals createGroupSignals(object []objs);

        private void fTimerActivate(object obj)
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
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
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
                                        m_semaQueue.Release(1);
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

        protected abstract int addAllStates ();

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
                            //Прервать, если обработаны все объекты
                            break;
                        else
                            ;
                    }
                    //Получить объект очереди событий
                    m_IdGroupSignalsCurrent = m_queueIdGroupSignals.Peek();

                    lock (m_lockState)
                    {
                        //Очистить все состояния
                        ClearStates();
                        //Добавить все состояния
                        addAllStates();
                    }

                    //Обработать все состояния
                    Run(@"HHandlerDbULoader::fThreadQueue ()");

                    //Ожидать обработки всех состояний
                    m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne(System.Threading.Timeout.Infinite);

                    lock (m_lockQueue)
                    {
                        //Удалить объект очереди событий (обработанный)
                        m_queueIdGroupSignals.Dequeue();
                    }

                    GroupSignals.STATE newState = GroupSignals.GetMode(Mode, State);

                    lock (m_lockStateGroupSignals)
                    {
                        State = newState;
                        MSecRemaindToActivate = MSecInterval; //(long)TimeSpanPeriod.TotalMilliseconds;
                    }

                    ((PlugInBase)_iPlugin).DataAskedHost(new object[] { ID_DATA_ASKED_HOST.TABLE_RES, m_IdGroupSignalsCurrent, TableResults });

                    try
                    {
                        m_IdGroupSignalsCurrent = -1;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception (e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader.fThreadQueue () - ...");
                    }
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

        public override void StartDbInterfaces()
        {
            foreach (int id in m_dictGroupSignals.Keys)
                register(id, 0, m_connSett, string.Empty);
        }

        protected override void register(int id, int indx, ConnectionSettings connSett, string name)
        {
            bool bReq = true;

            if (m_dictIdListeners.ContainsKey(id) == false)
                m_dictIdListeners.Add(id, new int[] { -1 });
            else
                if (!(m_dictIdListeners[id][indx] < 0))
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

            StartDbInterfaces();

            startThreadQueue();

            startTimerActivate();
        }

        public override void Stop()
        {
            stopTimerActivate();

            stopThreadQueue();

            base.Stop();
        }

        public void Stop(int id)
        {
            bool bStopped = true;

            lock (m_lockStateGroupSignals)
            {
                m_dictGroupSignals[id].State = GroupSignals.STATE.SLEEP;
                m_dictGroupSignals[id].TableResults = new DataTable ();

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

        protected int actualizeDatetimeStart()
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
    }

    public class PlugInULoader : PlugInBase
    {
        public PlugInULoader()
            : base()
        {
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            HHandlerDbULoader target = _object as HHandlerDbULoader;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT:
                    target.Initialize(ev.par[0] as ConnectionSettings);
                    break;
                case (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP:
                    target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]);
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
                        DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT });
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
