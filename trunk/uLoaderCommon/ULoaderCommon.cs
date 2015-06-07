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
            public enum STATE { UNKNOWN = -1, STOP, SLEEP, TIMER, QUEUE, ACTIVE }
            private STATE m_state;
            public STATE State { get { return m_state; } set { m_state = value; } }

            public bool IsStarted
            {
                get
                {
                    return (State == GroupSignals.STATE.ACTIVE)
                      || (State == GroupSignals.STATE.QUEUE)
                      || (State == GroupSignals.STATE.TIMER)
                      || (State == GroupSignals.STATE.SLEEP);
                }
            }

            private uLoaderCommon.MODE_WORK m_mode;
            public uLoaderCommon.MODE_WORK Mode { get { return m_mode; } set { m_mode = value; } }

            private TimeSpan m_tmSpanPeriod;
            /// <summary>
            /// Период времени для формирования запроса значений
            /// </summary>
            public TimeSpan TimeSpanPeriod { get { return m_tmSpanPeriod; } set { m_tmSpanPeriod = value; } }

            private long m_msecInterval;
            /// <summary>
            /// Интервал (милисекунды) между опросами значений
            /// </summary>
            public long MSecInterval { get { return m_msecInterval; } set { m_msecInterval = value; } }
            private long m_msecRemaindToActivate;
            public long MSecRemaindToActivate { get { return m_msecRemaindToActivate; } set { m_msecRemaindToActivate = value; } }

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

            public abstract DataTable TableRecieved { get; set; }

            public GroupSignals(object[] pars)
            {
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

            public static STATE NewState(uLoaderCommon.MODE_WORK mode, STATE prevState)
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

            /// <summary>
            /// Очистить "текущую" таблицу от записей,
            ///  содержащихся в "предыдущей" таблице
            /// </summary>
            /// <param name="tblPrev">"Предыдущая таблица"</param>
            /// <param name="tblRes">"Текущая" таблица</param>
            /// <returns>Таблица без "дублирующих" записей</returns>
            public static DataTable clearDupValues(DataTable tblPrev, DataTable tblRes)
            {
                int iDup = 0;

                if (((!(tblRes.Columns.IndexOf(@"ID") < 0)) && (!(tblRes.Columns.IndexOf(@"DATETIME") < 0)))
                    && (tblRes.Rows.Count > 0))
                {
                    DataRow[] arSel;
                    foreach (DataRow rRes in tblPrev.Rows)
                    {
                        arSel = (tblRes as DataTable).Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                        iDup += arSel.Length;
                        foreach (DataRow rDel in arSel)
                            (tblRes as DataTable).Rows.Remove(rDel);
                        tblRes.AcceptChanges();
                    }

                    //!!! См. ВНИМАТЕЛЬНО файл конфигурации - ИДЕНТИФИКАТОРЫ д.б. уникальные
                    //int cnt = -1;
                    //List <int>listDel = new List<int>();
                    //foreach (DataRow rRes in table.Rows)
                    //{
                    //    arSel = (table as DataTable).Select(@"ID=" + rRes[@"ID"]
                    //        + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                    //    if (arSel.Length > 1)
                    //    {
                    //        iRes ++;
                    //        //Logging.Logg().Error(@"HBiyskTMOra::clearDupValues () - "
                    //        //    + @"ID=" + rRes[@"ID"]
                    //        //    + @", " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'"
                    //        //    + @", " + @"QUALITY[" + arSel[0][@"QUALITY"] + @"," + arSel[1][@"QUALITY"] + @"]"
                    //        //, Logging.INDEX_MESSAGE.NOT_SET);
                    //        cnt = listDel.Count + arSel.Length;
                    //        foreach (DataRow rDel in arSel)
                    //            if (listDel.Count < (cnt - 1))
                    //                listDel.Add(table.Rows.IndexOf(rDel));
                    //            else
                    //                break;
                    //    }
                    //    else
                    //        ;
                    //}

                    //foreach (int indx in listDel)
                    //    //(table as DataTable).Rows.Remove(rDel);
                    //    (table as DataTable).Rows.RemoveAt(indx);
                    //table.AcceptChanges();
                }
                else
                    ;

                return tblRes;
            }

            public static DataTable clearDupValues(DataTable tblDup)
            {
                DataTable tblRes = tblDup.Clone();

                List<int> listIndxToDelete = new List<int>();
                DataRow[] arDup = null;

                foreach (DataRow r in tblDup.Rows)
                {
                    if (listIndxToDelete.IndexOf (tblDup.Rows.IndexOf(r)) < 0)
                    {
                        arDup = (tblDup as DataTable).Select(@"ID=" + r[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)r[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                        if (arDup.Length > 1)
                            for (int i = 1; i < arDup.Length; i ++)
                                listIndxToDelete.Add(tblDup.Rows.IndexOf(arDup[i]));
                        else
                            if (arDup.Length == 0)
                                throw new Exception("HHandlerDbULoader.GroupSignals.clearDupValues () - в таблице не найдена \"собственная\" строка...");
                            else
                                ;

                        tblRes.ImportRow(arDup[0]);                            
                    }
                    else
                        ;
                }

                tblRes.AcceptChanges();

                return tblRes;
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
        /// <summary>
        /// Интервал (милисекунды) между опросами значений обрабатываемой группы сигналов
        /// </summary>
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

        public DataTable TableRecieved
        {
            get
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[m_IdGroupSignalsCurrent].TableRecieved;
                else
                    throw new Exception(@"ULoaderCommon::TableResults.get ...");
            }

            set
            {
                if (!(m_IdGroupSignalsCurrent < 0))
                {
                    int cntPrev = -1;
                    if (! (m_dictGroupSignals[m_IdGroupSignalsCurrent].TableRecieved == null))
                        cntPrev = m_dictGroupSignals[m_IdGroupSignalsCurrent].TableRecieved.Rows.Count;
                    else
                        ;

                    string msg = @"HHandlerDbULoader::TableRecieved.set - "
                        + @"[ID=" + (_iPlugin as PlugInBase)._Id
                        + @", key=" + m_IdGroupSignalsCurrent + @"] "
                        + @"строк_было=" + cntPrev
                        + @", строк_стало=" + value.Rows.Count
                        + @" ...";
                    Console.WriteLine (msg);
                    //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

                    m_dictGroupSignals[m_IdGroupSignalsCurrent].TableRecieved = value;
                }
                else
                    throw new Exception(@"ULoaderCommon::TableResults.set ...");
            }
        }

        protected DateTime m_dtServer;        
        public ConnectionSettings m_connSett;        
        protected int m_IdGroupSignalsCurrent;

        protected object m_lockStateGroupSignals;
        private object m_lockQueue;        
        private Thread m_threadQueue;
        private Queue<int> m_queueIdGroupSignals;
        private int threadQueueIsWorking;
        private Semaphore m_semaQueue;

        public HHandlerDbULoader()
        {
            m_dtServer = DateTime.MinValue;            

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

        public virtual int Initialize(int id, object[] pars)
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
                        m_dictGroupSignals[id].State = GroupSignals.STATE.STOP;
                        //m_dictGroupSignals[id].DateTimeStart = (DateTime)pars[1];
                        m_dictGroupSignals[id].TimeSpanPeriod = TimeSpan.FromSeconds((double)pars[2]);
                        m_dictGroupSignals[id].MSecInterval = (int)pars[3];
                    }
                }
            }

            return iRes;
        }

        protected abstract GroupSignals createGroupSignals(object []objs);

        protected void push(int key)
        {
            Logging.Logg().Debug(@"HHandlerDbULoader::enqueue () - [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + key + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
            
            lock (m_lockQueue)
            {
                m_queueIdGroupSignals.Enqueue(key);

                if (m_queueIdGroupSignals.Count == 1)
                    m_semaQueue.Release(1);
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

                    State = GroupSignals.STATE.ACTIVE;

                    //Logging.Logg().Debug(@"HHandlerDbULoader::fThreadQueue () - начало обработки группы событий очереди (ID_PLUGIN=" + (_iPlugin as PlugInBase)._Id + @", ID_GSGNLS=" + m_IdGroupSignalsCurrent + @")", Logging.INDEX_MESSAGE.NOT_SET);

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

                    GroupSignals.STATE newState = GroupSignals.NewState(Mode, State);

                    lock (m_lockStateGroupSignals)
                    {
                        State = newState;
                        MSecRemaindToActivate = MSecInterval; //(long)TimeSpanPeriod.TotalMilliseconds;
                    }

                    ((PlugInBase)_iPlugin).DataAskedHost(new object[] { ID_DATA_ASKED_HOST.TABLE_RES, m_IdGroupSignalsCurrent, TableRecieved });

                    //Logging.Logg().Debug(@"HHandlerDbULoader::fThreadQueue () - окончание обработки группы событий очереди (ID_PLUGIN=" + (_iPlugin as PlugInBase)._Id + @", ID_GSGNLS=" + m_IdGroupSignalsCurrent + @")", Logging.INDEX_MESSAGE.NOT_SET);

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
            //??? "везде" 'true'
            if (bRes == true)
                try
                {
                    m_semaQueue.Release(1);
                }
                catch (Exception e)
                { //System.Threading.SemaphoreFullException
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "HHandlerDbULoader::fThreadQueue () - semaState.Release(1)");
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
        }

        public override bool IsStarted
        {
            get
            {
                bool bRes = false;

                lock (m_lockStateGroupSignals)
                {
                    foreach (GroupSignals grpSgnls in m_dictGroupSignals.Values)
                        if (grpSgnls.IsStarted == true)
                        {
                            bRes = true;

                            break;
                        }
                        else
                            ;
                }

                if ((bRes == true) && (base.IsStarted == false))
                    throw new Exception (@"HHandlerDbULoader::IsStarted.get - несовпадение признака 'Старт' с базовым классом...");

                return bRes;
            }
        }

        public void Start(int id)
        {
            bool bNeedStarted = ! IsStarted;            

            GroupSignals.STATE initState = GroupSignals.STATE.UNKNOWN;
            switch (m_dictGroupSignals[id].Mode)
            {
                case MODE_WORK.CUR_INTERVAL:
                    initState = GroupSignals.STATE.TIMER;
                    break;
                case MODE_WORK.COSTUMIZE:
                    initState = GroupSignals.STATE.SLEEP;
                    break;
                default:
                    break;
            }
            
            lock (m_lockStateGroupSignals)
            {
                m_dictGroupSignals[id].State = initState;
            }

            if (bNeedStarted == true)
            {
                Start();
                Activate(true);
            }
            else
                ;
        }

        public override void Stop()
        {
            Logging.Logg().Debug(@"HHandlerDbULoader::Stop () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            stopThreadQueue();

            base.Stop();
        }

        public void Stop(int id)
        {
            bool bNeedStopped = true;

            lock (m_lockStateGroupSignals)
            {
                m_dictGroupSignals[id].State = GroupSignals.STATE.STOP;
                m_dictGroupSignals[id].TableRecieved = new DataTable();
            }

            bNeedStopped = ! IsStarted;

            if (bNeedStopped == true)
            {
                Activate(false);
                Stop();
            }
            else
                ;
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
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::startThreadQueue () - ...");
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
                m_queueIdGroupSignals.Clear();
                m_threadQueue = null;
            }
            else ;

            return iRes;
        }

        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            return response(out error, out table);
        }
    }

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

            public GroupSignalsSrc(object[] pars)
                : base(pars)
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

            Logging.Logg().Debug(@"HBiyskTMOra::actualizeDateTimeStart () - "
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

            Run(@"HBiyskTMOra::ChangeState ()");
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
                        throw new Exception(@"HBiyskTMOra::StateRequest (::CurrentTime) - ...");
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
                    //    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateRequest (::Values) - ...");
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
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse (::CurrentTime) - ...");
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
    }

    public abstract class HHandlerDbULoaderDest : HHandlerDbULoader
    {
        enum StatesMachine
        {
            Unknown = -1
            ,
            CurrentTime
                ,
            Values
                , Insert
        }

        public HHandlerDbULoaderDest()
            : base()
        {
        }

        public HHandlerDbULoaderDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        protected abstract class GroupSignalsDest : GroupSignals
        {
            public enum INDEX_DATATABLE_RES
            {
                PREVIOUS,
                CURRENT
                    , COUNT_INDEX_DATATABLE_RES
            }

            protected class SIGNALDest : GroupSignals.SIGNAL
            {
                public int m_idLink;

                public SIGNALDest(int idMain, int idLink)
                    : base(idMain)
                {
                    this.m_idLink = idLink;
                }
            }

            private DataTable[] m_arTableRec;
            public override DataTable TableRecieved
            {
                get
                {
                    lock (this)
                    {
                        return m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT];
                    }
                }

                set
                {
                    lock (this)
                    {
                        //Вариант №1
                        if (m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count > 0)
                            m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                        else
                            ;
                        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] = value.Copy();

                        ////Вариант №2
                        //if (value.Rows.Count > 0)
                        //{
                        //    if (value.Rows.Count == m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count)
                        //        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Clear();
                        //    else
                        //    {
                        //        m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                        //        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] = value.Copy();
                        //    }
                        //}
                        //else
                        //    ;

                        ////Вариант №3
                        //if (value.Rows.Count > 0)
                        //{
                        //    if (value.Rows.Count == m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count)
                        //        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Clear();
                        //    else
                        //    {
                        //        m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                        //        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] = value.Copy();
                        //    }
                        //}
                        //else
                        //    ;
                    }
                }
            }

            //Для 'GetInsertQuery'
            public DataTable TableRecievedPrev { get { return m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS]; } }
            public DataTable TableRecievedCur { get { return m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT]; } }

            public GroupSignalsDest(object[] pars)
                : base(pars)
            {
                m_arTableRec = new DataTable[(int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES];
                for (int i = 0; i < (int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES; i++)
                    m_arTableRec[i] = new DataTable();
            }

            protected abstract DataTable getTableIns(ref DataTable table);

            protected abstract object getIdToInsert(int idLink);

            protected abstract string getInsertValuesQuery(DataTable tblRes);

            public string GetInsertValuesQuery()
            {
                string strRes = string.Empty;

                DataTable tblRes = getTableRes();

                if ((!(tblRes == null))
                    && (tblRes.Rows.Count > 0))
                    strRes = getInsertValuesQuery(tblRes);
                else
                    ;

                return
                    //string.Empty
                    strRes
                    ;
            }

            protected abstract DataTable getTableRes();
        }

        public override void ClearValues()
        {
            //TableResults = new DataTable ();
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch ((StatesMachine)state)
            {
                case StatesMachine.CurrentTime:
                    if (!(m_IdGroupSignalsCurrent < 0))
                        GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    else
                        throw new Exception(@"statdidsql::StateRequest () - state=" + state.ToString() + @"...");
                    break;
                case StatesMachine.Values:
                    break;
                case StatesMachine.Insert:
                    string query = (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsDest).GetInsertValuesQuery();
                    if (query.Equals(string.Empty) == false)
                        Request(m_dictIdListeners[m_IdGroupSignalsCurrent][0], query);
                    else
                        Logging.Logg().Error(@"statidsql::StateRequest () ::" + ((StatesMachine)state).ToString() + @" - "
                            + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                            + @"query=Empty" + @"..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;

            switch ((StatesMachine)state)
            {
                case StatesMachine.CurrentTime:
                    m_dtServer = (DateTime)(obj as DataTable).Rows[0][0];
                    //string msg = @"statidsql::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
                    //    + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                    //    + @"DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff") + @"...";
                    //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                    //Console.WriteLine (msg);
                    break;
                case StatesMachine.Values:
                    Logging.Logg().Action(@"statidsql::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
                        + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                        + @"Ok! ...", Logging.INDEX_MESSAGE.NOT_SET);
                    break;
                case StatesMachine.Insert:
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, int req, int res)
        {
            Logging.Logg().Error(@"statidsql::StateErrors (state=" + ((StatesMachine)state).ToString() + @", req=" + req + @", res=" + res + @") - "
                + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]"
                + @"..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(@"statidsql::StateWarnings (state" + ((StatesMachine)state).ToString() + @", req=" + req + @", res=" + res + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        public int Insert(int id, DataTable tableIn)
        {
            int iRes = 0;

            lock (m_lockStateGroupSignals)
            {
                if (m_dictGroupSignals[id].IsStarted == true)
                {
                    m_dictGroupSignals[id].TableRecieved = tableIn.Copy();

                    push(id);
                }
                else
                    ;
            }

            return iRes;
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            //AddState((int)StatesMachine.Values);
            AddState((int)StatesMachine.Insert);

            return iRes;
        }
    }

    public abstract class HHandlerDbULoaderStatTMDest : HHandlerDbULoaderDest
    {
        public HHandlerDbULoaderStatTMDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }
        
        protected abstract class GroupSignalsStatTMDest : GroupSignalsDest
        {
            public GroupSignalsStatTMDest(object[] pars)
                : base(pars)
            {
            }

            protected override DataTable getTableIns(ref DataTable table)
            {
                DataTable tableRes = new DataTable();
                DataRow[] arSelIns = null;
                DataRow rowCur = null
                    , rowAdd
                    , rowPrev = null;
                int idSgnl = -1
                    , tmDelta = -1;

                if ((table.Columns.Count > 2)
                    && ((!(table.Columns.IndexOf(@"ID") < 0)) && (!(table.Columns.IndexOf(@"DATETIME") < 0))))
                {
                    table.Columns.Add(@"tmdelta", typeof(int));
                    tableRes = table.Clone();

                    for (int s = 0; s < Signals.Length; s++)
                    {
                        try
                        {
                            idSgnl = (Signals[s] as GroupSignalsDest.SIGNALDest).m_idLink;

                            //arSelIns = (table as DataTable).Select(string.Empty, @"ID, DATETIME");
                            arSelIns = (table as DataTable).Select(@"ID=" + idSgnl, @"DATETIME");
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"statidsql::getTableIns () - ...");
                        }

                        if (!(arSelIns == null))
                            for (int i = 0; i < arSelIns.Length; i++)
                            {
                                if (i < (arSelIns.Length - 1))
                                {
                                    tableRes.ImportRow(arSelIns[i]);
                                    rowCur = tableRes.Rows[tableRes.Rows.Count - 1];
                                }
                                else
                                    //Не вставлять без известной 'tmdelta'
                                    rowCur = null;

                                //Проверитьт № итерации
                                if (i == 0)
                                {//Только при прохождении 1-ой итерации цикла
                                    tmDelta = -1;
                                    //Определить 'tmdelta' для записи из предыдущего опроса
                                    rowAdd = null;
                                    rowPrev = setTMDelta(idSgnl, (DateTime)arSelIns[i][@"DATETIME"], out tmDelta);

                                    if ((!(rowPrev == null))
                                        && (tmDelta > 0))
                                    {
                                        //Добавить из предыдущего опроса
                                        rowAdd = tableRes.Rows.Add();
                                        //Скопировать все значения
                                        foreach (DataColumn col in tableRes.Columns)
                                        {
                                            if (col.ColumnName.Equals(@"tmdelta") == true)
                                                //Для "нового" столбца - найденное значение
                                                rowAdd[col.ColumnName] = tmDelta;
                                            else
                                                //"Старые" значения
                                                rowAdd[col.ColumnName] = rowPrev[col.ColumnName];
                                        }

                                        //Console.WriteLine(@"Установлен для ID=" + idSgnl + @", DATETIME=" + ((DateTime)rowAdd[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + rowAdd[@"tmdelta"]);
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    //Определить смещение "соседних" значений сигнала
                                    long iTMDelta = (((DateTime)arSelIns[i][@"DATETIME"]).Ticks - ((DateTime)arSelIns[i - 1][@"DATETIME"]).Ticks) / TimeSpan.TicksPerMillisecond;
                                    rowPrev[@"tmdelta"] = (int)iTMDelta;
                                    //Console.WriteLine(@", tmdelta=" + rowPrev[@"tmdelta"]);
                                }

                                //if (!(rowCur == null))
                                //    Console.Write(@"ID=" + rowCur[@"ID"] + @", DATETIME=" + ((DateTime)rowCur[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                                //else
                                //    Console.Write(@"ID=" + arSelIns[i][@"ID"] + @", DATETIME=" + ((DateTime)arSelIns[i][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));

                                rowPrev = rowCur;
                            }
                        else
                            ; //arSelIns == null

                        //Корректировать вывод
                        if (arSelIns.Length > 0)
                            Console.WriteLine();
                        else ;
                    } //Цикл по сигналам...
                }
                else
                    ; //Отсутствуют необходимые столбцы (т.е. у таблицы нет структуры)

                tableRes.AcceptChanges();

                return tableRes;
            }
            
            protected override DataTable getTableRes()
            {
                DataTable tblDiff = clearDupValues(TableRecievedPrev.Copy(), TableRecieved.Copy())
                    , tblRes = getTableIns(ref tblDiff);

                return tblRes;
            }

            protected DataRow setTMDelta(int id, DateTime dtCurrent, out int tmDelta)
            {
                tmDelta = -1;
                DataRow rowRes = null;
                DataRow[] arSelWas = null;

                //Проверить наличие столбцов в результ./таблице (признак получения рез-та)
                if (TableRecievedPrev.Columns.Count > 0)
                {//Только при наличии результата
                    arSelWas = TableRecievedPrev.Select(@"ID=" + id, @"DATETIME DESC");
                    //Проверить результат для конкретного сигнала
                    if ((!(arSelWas == null)) && (arSelWas.Length > 0))
                    {//Только при наличии рез-та по конкретному сигналу
                        rowRes = arSelWas[0];
                        tmDelta = (int)(dtCurrent - (DateTime)arSelWas[0][@"DATETIME"]).TotalMilliseconds;

                        //arSelWas[0][@"tmdelta"] = iRes;
                        //Console.WriteLine(@"Установлен для ID=" + id + @", DATETIME=" + ((DateTime)arSelWas[0][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + iRes);
                    }
                    else
                        ; //Без полученного рез-та для конкретного сигнала - невозможно
                }
                else
                    ; //Без полученного рез-та - невозможно

                return rowRes;
            }
        }
    }

    public class PlugInULoaderDest : PlugInULoader
    {
        public PlugInULoaderDest()
            : base()
        {
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            HHandlerDbULoaderDest target = _object as HHandlerDbULoaderDest;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.TO_INSERT:
                    target.Insert((int)(ev.par as object[])[0], (ev.par as object[])[1] as DataTable);
                    break;
                default:
                    break;
            }

            base.OnEvtDataRecievedHost(obj);
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
                            target.Start((int)(ev.par as object[])[0]);
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
