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

        //protected GroupSignals.SIGNAL[] Signals
        //{
        //    get
        //    {
        //        if (!(m_IdGroupSignalsCurrent < 0))
        //            return m_dictGroupSignals[m_IdGroupSignalsCurrent].Signals;
        //        else
        //            throw new Exception(@"ULoaderCommon::Signals.get ...");
        //    }
        //}

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

        //protected string Query
        //{
        //    get
        //    {
        //        if (!(m_IdGroupSignalsCurrent < 0))
        //            return m_dictGroupSignals[m_IdGroupSignalsCurrent].Query;
        //        else
        //            throw new Exception(@"ULoaderCommon::Query.get ...");
        //    }

        //    set
        //    {
        //        if (!(m_IdGroupSignalsCurrent < 0))
        //            m_dictGroupSignals[m_IdGroupSignalsCurrent].Query = value;
        //        else
        //            throw new Exception(@"ULoaderCommon::Query.set ...");
        //    }
        //}

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

        protected void enqueue(int key)
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
