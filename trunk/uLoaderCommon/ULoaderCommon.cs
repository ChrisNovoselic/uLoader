using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;

using HClassLibrary;

namespace uLoaderCommon
{
    public interface ILoader
    {
        void Start();
        void Stop();

        bool IsStarted { get; }

        bool Activate(bool active);
        bool Actived { get; }

        int Initialize(object[] pars);
        int Initialize(int id, object[] pars);
    }
    
    public interface ILoaderSrc : ILoader
    {
        //void ChangeState ();
        void Start(int id);
    }

    /// <summary>
    /// Перечисление (дата/время) константы по умолчанию
    /// </summary>
    public enum DATETIME
    {
        /// <summary>
        /// Период (секунды) опроса группы сигналов
        /// </summary>
        SEC_SPANPERIOD_DEFAULT = 60,
        /// <summary>
        /// Интервал (милисекунды) опроса группы сигналов
        /// </summary>
        MSEC_INTERVAL_DEFAULT = 6666,
        /// <summary>
        /// Интервал (милисекунды) проверки времени - до активации опроса группы сигналов
        /// </summary>
        MSEC_INTERVAL_TIMER_ACTIVATE = 66
    }
    /// <summary>
    /// Перечисление для типов опроса
    /// </summary>
    public enum MODE_WORK
    {
        UNKNOWN = -2,
        /// <summary>
        /// по требованию (для назначения)
        /// </summary>
        ON_REQUEST = -1,
        /// <summary>
        /// по текущему интервалу
        /// </summary>        
        CUR_INTERVAL,
        //CUR_INTERVAL_CAUSEPERIOD,
        //CUR_INTERVAL_CAUSENOT,
        /// <summary>
        /// выборочно (история)
        /// </summary>
        COSTUMIZE,        
            COUNT_MODE_WORK
    }
    /// <summary>
    /// Класс - базовый для описания целевого объекта для загрузки/вагрузки данных
    /// </summary>
    public abstract class HHandlerDbULoader : HHandlerDb
    {
        /// <summary>
        /// Ссылка на объект "связи" с клиентом
        /// </summary>
        protected IPlugIn _iPlugin;
        /// <summary>
        /// Класс - базовый для описания группы сигналов
        /// </summary>
        public abstract class GroupSignals
        {
            /// <summary>
            /// Сылка на объект владельца текущего объекта
            /// </summary>
            protected HHandlerDbULoader _parent;
            /// <summary>
            /// Целочисленный идентификатор группы (дублирует значение ключа в словаре объекта-владельца группы)
            /// </summary>
            public int m_Id;
            /// <summary>
            /// Перечисление возможных слстояний для группы сигналов
            /// </summary>
            public enum STATE { UNKNOWN = -1, STOP, SLEEP, TIMER, QUEUE, ACTIVE }
            private STATE m_state;
            /// <summary>
            /// Состояние группы сигналов
            /// </summary>
            public virtual STATE State { get { return m_state; } set { m_state = value; } }
            /// <summary>
            /// Признак
            /// </summary>
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

            public virtual void Stop ()
            {
                State = GroupSignals.STATE.STOP;                
                TableRecieved = null;
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

            /// <summary>
            /// Класс для объекта СИГНАЛ
            /// </summary>
            public class SIGNAL
            {
                /// <summary>
                /// Идентификатор сигнала, уникальный в границах приложения
                /// </summary>
                public int m_idMain;
                /// <summary>
                /// Конструктор - основной (с параметром)
                /// </summary>
                /// <param name="idMain">Идентификатор сигнала, уникальный в границах приложения</param>
                public SIGNAL(int idMain)
                {
                    this.m_idMain = idMain;
                }
            }

            protected SIGNAL[] m_arSignals;
            /// <summary>
            /// Массив сигналов в группе
            /// </summary>
            public SIGNAL[] Signals { get { return m_arSignals; } }
            /// <summary>
            /// Таблица результата
            /// </summary>
            public abstract DataTable TableRecieved { get; set; }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="parent">Объект-владелец (для последующего обращения к его членам-данным)</param>
            /// <param name="pars">Параметры группы сигналов</param>
            public GroupSignals(HHandlerDbULoader parent, int id, object[] pars)
            {
                //Владелец объекта
                _parent = parent;
                //Целочисленный идентификатор
                m_Id = id;
                //Значения по умолчанию
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
            /// <summary>
            /// Создать объект сигнала
            /// </summary>
            /// <param name="objs">Массив параметров для передачи в конструктор объекта сигнала</param>
            /// <returns>Созданный объект</returns>
            protected abstract SIGNAL createSignal(object []objs);
            /// <summary>
            /// Определить нове значение для состояния
            /// </summary>
            /// <param name="mode">Режим работы группы сигналов</param>
            /// <param name="prevState">Предыдущее значение состояния</param>
            /// <returns>Новое состояние</returns>
            public static STATE NewState(uLoaderCommon.MODE_WORK mode, STATE prevState)
            {
                GroupSignals.STATE stateRes = GroupSignals.STATE.UNKNOWN;
                //Проверить режим работы
                if (mode == uLoaderCommon.MODE_WORK.CUR_INTERVAL)
                    // для режима "текущий интервал"
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
                        // для режима "выборочно"    
                        stateRes = GroupSignals.STATE.SLEEP;
                    else
                        if (mode == uLoaderCommon.MODE_WORK.ON_REQUEST)
                            // для режима "по требованию"    
                            stateRes = GroupSignals.STATE.SLEEP;
                        else
                            //??? throw new Exception (@"")
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
                        arSel = (tblRes as DataTable).Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fffffff") + @"'");
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
                    //        //Logging.Logg().Error(@"HHandlerDbULoader::clearDupValues () - "
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
            /// <summary>
            /// Удалить дублированные записи из таблицы
            /// </summary>
            /// <param name="tblDup">Таблица, содержащая дублирующие записи</param>
            /// <returns>Таблица без дублирующих записей</returns>
            public static DataTable clearDupValues(DataTable tblDup)
            {
                DataTable tblRes = tblDup.Clone();
                //Список индексов строк для удаления
                List<int> listIndxToDelete = new List<int>();
                //Массив дублированных строк
                DataRow[] arDup = null;
                //Признак наличия кавычек для значений в поле [ID]
                bool bQuote = !tblDup.Columns[@"ID"].DataType.IsPrimitive;
                //Строка запроса для поиска дублирующих записей
                string strSel = string.Empty;

                try
                {
                    foreach (DataRow r in tblDup.Rows)
                    {
                        //Проверить наличие индекса строки в уже найденных (как дублированные)
                        if (listIndxToDelete.IndexOf (tblDup.Rows.IndexOf(r)) < 0)
                        {
                            //Сформировать строку запроса
                            strSel = @"ID=" + (bQuote == true ? @"'" : string.Empty) + r[@"ID"] + (bQuote == true ? @"'" : string.Empty) + @" AND " + @"DATETIME='" + ((DateTime)r[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fffffff") + @"'";
                            arDup = (tblDup as DataTable).Select(strSel);
                            //Проверить наличие дублирующих записей
                            if (arDup.Length > 1)
                                //Добавить индексы всех найденных дублирующих строк в список для удаления
                                // , КРОМЕ 1-ой!
                                for (int i = 1; i < arDup.Length; i ++)
                                    listIndxToDelete.Add(tblDup.Rows.IndexOf(arDup[i]));
                            else
                                if (arDup.Length == 0)
                                    throw new Exception("HHandlerDbULoader.GroupSignals.clearDupValues () - в таблице не найдена \"собственная\" строка...");
                                else
                                    ;

                            //Добавить строку в таблицу-результат
                            tblRes.ImportRow(arDup[0]);                            
                        }
                        else
                            ;
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::GroupSignals::clearDupValues () - ...");

                    tblRes.Clear ();
                }

                //Принять внесенные изменения в таблицу-результат
                tblRes.AcceptChanges();

                return tblRes;
            }
        }
        /// <summary>
        /// Словарь с группами сигналов
        /// </summary>
        protected Dictionary<int, GroupSignals> m_dictGroupSignals;
        /// <summary>
        /// Состояние текущей (обрабатываемой) группы сигналов
        /// </summary>
        protected virtual GroupSignals.STATE State
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].State;
                else
                    throw new Exception(@"ULoaderCommon::State.get ...");
            }
            
            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    m_dictGroupSignals[IdGroupSignalsCurrent].State = value;
                else
                    throw new Exception(@"ULoaderCommon::State.set ...");
            }
        }
        /// <summary>
        /// Режим работы текущей (обрабатываемой) группы сигналов
        /// </summary>
        protected MODE_WORK Mode
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].Mode;
                else
                    throw new Exception(@"ULoaderCommon::Mode.get ...");
            }
            
            /*set { m_dictGroupSignals[IdGroupSignalsCurrent].Mode = value; }*/
        }
        /// <summary>
        /// Период времени опроса текущей (обрабатываемой) группы сигналов
        /// </summary>
        protected TimeSpan TimeSpanPeriod
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].TimeSpanPeriod;
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
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].MSecInterval;
                else
                    throw new Exception(@"ULoaderCommon::MSecInterval.get ...");
            }
        }
        /// <summary>
        /// Таблица результата обрабатываемой группы сигналов
        /// </summary>
        public DataTable TableRecieved
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].TableRecieved;
                else
                    throw new Exception(@"ULoaderCommon::TableResults.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                {
                    int cntPrev = -1;
                    if (! (m_dictGroupSignals[IdGroupSignalsCurrent].TableRecieved == null))
                        cntPrev = m_dictGroupSignals[IdGroupSignalsCurrent].TableRecieved.Rows.Count;
                    else
                        ;

                    string msg = @"HHandlerDbULoader::TableRecieved.set - "
                        + @"[" + PlugInId
                        + @", key=" + IdGroupSignalsCurrent + @"] "
                        + @"строк_было=" + cntPrev
                        + @", строк_стало=" + value.Rows.Count
                        + @" ...";
                    Console.WriteLine (msg);
                    Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

                    m_dictGroupSignals[IdGroupSignalsCurrent].TableRecieved = value;
                }
                else
                    throw new Exception(@"ULoaderCommon::TableResults.set ...");
            }
        }
        /// <summary>
        /// Дата/время - результат запроса к источнику данных
        /// </summary>
        protected DateTime m_dtServer;
        /// <summary>
        /// Параметры соединения с источником данных
        /// </summary>
        protected ConnectionSettings m_connSett;
        private int m_iIdGroupSignalsCurrent;
        /// <summary>
        /// Целочисленный идентификатор группы сигналов
        ///  , ключ для словаря с группами сигналов
        ///  , уникальный в границах приложения, передается из-вне (файл конфигурации)
        /// </summary>
        protected virtual int IdGroupSignalsCurrent { get { return m_iIdGroupSignalsCurrent; } set { m_iIdGroupSignalsCurrent = value; } }
        /// <summary>
        /// Объект для синхронизации изменения состояний групп сигналов
        /// </summary>
        protected object m_lockStateGroupSignals;
        /// <summary>
        /// Объект для синхронизации изменения очереди событий
        /// </summary>
        private object m_lockQueue;
        /// <summary>
        /// Объект потока очереди обработки событий
        /// </summary>
        private Thread m_threadQueue;
        /// <summary>
        /// Длина очереди обработки
        /// </summary>
        public int QueueCount {
            get
            {
                int iRes = -1;
                
                //lock (this) { iRes = m_queueIdGroupSignals.Count; }
                iRes = m_queueIdGroupSignals.Count;
                
                return iRes;
            }
        }
        /// <summary>
        /// Очередь для обработки с идентификаторами групп сигналов
        /// </summary>
        private Queue<int> m_queueIdGroupSignals;
        /// <summary>
        /// Признак состояния потока очереди обработки событий
        /// </summary>
        private int threadQueueIsWorking;
        /// <summary>
        /// Объект синхронизации организации обработки событий в очереди обработки событий
        /// </summary>
        private
            //Semaphore m_semaQueue
            AutoResetEvent m_autoResetEvtQueue
            //ManualResetEvent m_mnlResetEvtQueue
            ;
        /// <summary>
        /// Словарь дополнительных параметров (передаются из-вне)
        /// </summary>
        protected Dictionary<string, string> m_dictAdding;
        /// <summary>
        /// Конструктор - основной (без параметров) для создания объекта при "прямой" сборке приложения
        /// </summary>
        public HHandlerDbULoader()
        {
            //Время источника данных "по умолчанию"
            m_dtServer = DateTime.MinValue;            

            m_iIdGroupSignalsCurrent = -1;
            m_dictGroupSignals = new Dictionary<int, GroupSignals>();

            m_lockStateGroupSignals = new object();

            //m_threadQueue = //Создание при "старте"
            m_lockQueue = new object();
            m_queueIdGroupSignals = new Queue<int>();
            threadQueueIsWorking = -1;
            //m_semaQueue
            m_autoResetEvtQueue
            //m_mnlResetEvtQueue
                = null; //Создание при "старте"

            m_dictAdding = new Dictionary<string, string>();
        }
        /// <summary>
        /// Конструктор - дополнительный для создания объекта при динамическом подключении библиотеки к приложению
        /// </summary>
        /// <param name="iPlugIn"></param>
        public HHandlerDbULoader(IPlugIn iPlugIn)
            : this()
        {
            this._iPlugin = iPlugIn;
        }
        /// <summary>
        /// Инициализация данных объекта
        /// </summary>
        /// <param name="pars">Массив параметров для инициализации</param>
        /// <returns>Признак выполнения</returns>
        public virtual int Initialize(object[] pars)
        {
            int iRes = 0;
            //Значения параметров соединения с источником данных
            m_connSett = new ConnectionSettings((pars[0] as ConnectionSettings));
            //Очистить словарь с доп./параметрами
            m_dictAdding.Clear();
            
            string key = string.Empty
                , val = string.Empty;
            //Проверить наличие дополнительных параметров
            if (pars.Length > 1)
            {
                //Сохранить переданные из-вне параметры
                for (int i = 1; i < pars.Length; i ++)
                {
                    if (pars[i] is string)
                    {
                        key = ((string)pars[i]).Split (FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[0];
                        val = ((string)pars[i]).Split (FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[1];

                        m_dictAdding.Add(key, val);
                    }
                    else
                        ;
                }
            }
            else
                ; //Для инициализации передан только 'ConnectionSettings' (pars[0])

            return iRes;
        }
        /// <summary>
        /// Инициализация группы сигналов
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов</param>
        /// <param name="pars">Параметры группы сигналов для инициализации</param>
        /// <returns></returns>
        public virtual int Initialize(/*INDEX_INIT_PARAMETER indxPars, */int id, object[] pars)
        {
            int iRes = 0;

            try
            {
                if (m_dictGroupSignals.Keys.Contains(id) == false)
                {//Считать переданные параметры - параметрами сигналов                
                    m_dictGroupSignals.Add(id, createGroupSignals (id, pars));

                    Logging.Logg().Debug(@"HHandlerDbULoader::Initialize () - добавить группу сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    //Сигналы д.б. инициализированы
                    if (m_dictGroupSignals[id].Signals == null)
                        iRes = -1;
                    else
                        if (pars[0].GetType().IsArray == true)
                        {
                            //Считать переданные параметры - параметрами сигналов
                            m_dictGroupSignals[id] = createGroupSignals(id, pars);

                            Logging.Logg().Debug(@"HHandlerDbULoader::Initialize () - ПЕРЕсоздать группу сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                        {//Считать переданные параметры - параметрами группы сигналов
                            lock (m_lockStateGroupSignals)
                            {
                                m_dictGroupSignals[id].Mode = (uLoaderCommon.MODE_WORK)pars[0];
                                m_dictGroupSignals[id].State = GroupSignals.STATE.STOP;
                                //Переопределено в 'HHandlerDbULoaderDatetimeSrc'
                                //if (m_dictGroupSignals[id].Mode == MODE_WORK.COSTUMIZE)
                                //    if ((!(((DateTime)pars[1] == null)))
                                //        && (!(((DateTime)pars[1] == DateTime.MinValue))))
                                //        m_dictGroupSignals[id].DateTimeStart = (DateTime)pars[1];
                                //    else
                                //        ;
                                //else
                                //    ;
                                m_dictGroupSignals[id].TimeSpanPeriod = (TimeSpan)pars[2];
                                m_dictGroupSignals[id].MSecInterval = (int)pars[3];
                            }

                            Logging.Logg().Debug(@"HHandlerDbULoader::Initialize () - параметры группы сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception (e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::Initialize () - ...");

                iRes = -1;
            }

            return iRes;
        }
        /// <summary>
        /// Создать объект - группа сигналов
        /// </summary>
        /// <param name="objs">Массив параметров для создания группы сигналов (параметры сигналов)</param>
        /// <returns></returns>
        protected abstract GroupSignals createGroupSignals(int id, object []objs);
        /// <summary>
        /// Проверить требуется ли поставить идентификатор в очередь обработки
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract bool isPush (int curCount);
        /// <summary>
        /// Добавить в очередь обработки событий группу сигналов
        /// </summary>
        /// <param name="key">Идентификатор группы сигналов</param>
        /// <returns>Количество событий в очереди для указанной группы</returns>
        protected int push(int key)
        {
            int iRes = -1;

            string msgDebug = @"HHandlerDbULoader::enqueue () - [" + PlugInId + @", key=" + key + @"] - queue.Count=";
            int keyQueueCount = -1;

            lock (m_lockQueue)
            {
                keyQueueCount = m_queueIdGroupSignals.Count (delegate(int i1) { return i1 == key; });

                msgDebug += QueueCount;
                msgDebug += @"(" + (iRes = keyQueueCount) + @")";

                if (isPush (keyQueueCount) == true)
                    if (!(m_autoResetEvtQueue == null))
                        try
                        {                        
                            m_queueIdGroupSignals.Enqueue(key);
                            iRes ++;                        

                            //Проверить активность потока очереди обработки событий
                            bool bSet = m_autoResetEvtQueue.WaitOne(0, true);

                            //if (m_queueIdGroupSignals.Count == 1)
                            if (bSet == false)
                                //Активировать поток очереди обработки событий
                                //m_semaQueue.Release(1);
                                m_autoResetEvtQueue.Set();
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, msgDebug + @" ...");
                            msgDebug = string.Empty;
                        }
                    else
                        ;
                else
                    msgDebug += @"-ПРОПУСК!";
            }

            if (msgDebug.Equals (string.Empty) == false)
                Logging.Logg().Debug(msgDebug + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
            else
                ;

            return iRes;
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

                //Проверить наличие объектов для обработки
                lock (m_lockQueue)
                {
                    bRes = QueueCount > 0;
                }

                if (bRes == false)
                    //Ожидать когда появятся объекты для обработки
                    //bRes = m_semaQueue.WaitOne();
                    bRes = m_autoResetEvtQueue.WaitOne();
                else
                    ; //Начать обработку немедленно

                while (true)
                {
                    lock (m_lockQueue)
                    {
                        if (QueueCount == 0)
                            //Прервать, если обработаны все объекты
                            break;
                        else
                            ;
                    }
                    //Получить объект очереди событий
                    IdGroupSignalsCurrent = m_queueIdGroupSignals.Peek();

                    State = GroupSignals.STATE.ACTIVE;

                    //Logging.Logg().Debug(@"HHandlerDbULoader::fThreadQueue () - начало обработки группы событий очереди (" + PlugInId + @", ID_GSGNLS=" + IdGroupSignalsCurrent + @")", Logging.INDEX_MESSAGE.NOT_SET);

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
                    m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne(System.Threading.Timeout.Infinite, true);

                    lock (m_lockQueue)
                    {
                        //Удалить объект очереди событий (обработанный)
                        m_queueIdGroupSignals.Dequeue();
                    }

                    GroupSignals.STATE newState = GroupSignals.NewState(Mode, State);

                    lock (m_lockStateGroupSignals)
                    {
                        State = newState;
                    }

                    if (! (_iPlugin == null))
                        ((PlugInBase)_iPlugin).DataAskedHost(getDataAskedHost ());
                    else
                        ;

                    //Logging.Logg().Debug(@"HHandlerDbULoader::fThreadQueue () - окончание обработки группы событий очереди (" + PlugInId + @", ID_GSGNLS=" + IdGroupSignalsCurrent + @")", Logging.INDEX_MESSAGE.NOT_SET);

                    try
                    {
                        IdGroupSignalsCurrent = -1;
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
                    //m_semaQueue.Release(1);
                    m_autoResetEvtQueue.Reset();
                }
                catch (Exception e)
                { //System.Threading.SemaphoreFullException
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "HHandlerDbULoader::fThreadQueue () - semaState.Release(1)");
                }
            else
                ;
        }
        /// <summary>
        /// Возвратить массив объектов для передачи клиенту
        /// </summary>
        /// <returns></returns>
        protected virtual object [] getDataAskedHost ()
        {
            return new object[] { ID_DATA_ASKED_HOST.TABLE_RES, IdGroupSignalsCurrent, TableRecieved, null };
        }
        /// <summary>
        /// Старт потоков для обмена данными с источниками информации
        /// </summary>
        public override void StartDbInterfaces()
        {
            lock (m_lockStateGroupSignals)
            {
                foreach (int id in m_dictGroupSignals.Keys)
                    register(id, 0, m_connSett, string.Empty);
            }
        }
        /// <summary>
        /// Регистрация источника информации
        /// </summary>
        /// <param name="id">Ключ в словаре с идентификаторами соединений</param>
        /// <param name="indx">Индекс в массиве - элементе словаря с идентификаторами соединений</param>
        /// <param name="connSett">Параметры соединения с источником информации</param>
        /// <param name="name">Наименование соединения</param>
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
        /// <summary>
        /// Старт зависимых потоков
        /// </summary>
        protected virtual void startThreadDepended ()
        {
            StartDbInterfaces();

            startThreadQueue();
        }
        /// <summary>
        /// Старт объекта и всех зависимых потоков
        /// </summary>
        public override void Start()
        {
            base.Start();

            startThreadDepended ();

            Logging.Logg().Debug(@"HHandlerDbULoader::Start (" + PlugInId + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);            
        }
        /// <summary>
        /// Признак выполнения объекта и всех зависимых потоков
        /// </summary>
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
                    Logging.Logg ().Error(@"HHandlerDbULoader::IsStarted.get [" + PlugInId + @" - несовпадение признака 'Старт' с базовым классом...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;

                return bRes;
            }
        }
        /// <summary>
        /// Старт группы сигналов с указанным идентификаторм
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов</param>
        public virtual void Start(int id)
        {
            Logging.Logg().Debug(@"HHandlerDbULoader::Start (" + PlugInId + @", key=" + id + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            
            int iNeedStarted = -1; //Признак необходимости запуска "родительского" объекта
            GroupSignals.STATE initState = GroupSignals.STATE.UNKNOWN; //Новое состояние группы сигналов при старте
            //Установить признак необходимости запуска "родительского" объекта
            try
            {
                iNeedStarted = IsStarted == false ? 1 : 0;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::Start (" + PlugInId + @", key=" + id + @") - ...");
            }
            //Новое состояние в зависимости от режима группы сигналов
            switch (m_dictGroupSignals[id].Mode)
            {
                case MODE_WORK.CUR_INTERVAL:
                    initState = GroupSignals.STATE.TIMER;
                    break;
                case MODE_WORK.COSTUMIZE:
                case MODE_WORK.ON_REQUEST: // для состояния 'UNKNOWN' (для группы сигналов назначения)
                    initState = GroupSignals.STATE.SLEEP;
                    break;
                default:
                    break;
            }
            //Изменить состояние
            lock (m_lockStateGroupSignals)
            {
                if ((!(m_dictGroupSignals == null))
                    && (m_dictGroupSignals.Keys.Contains(id) == true))
                    m_dictGroupSignals[id].State = initState;
                else
                    iNeedStarted = -1;
            }

            Logging.Logg().Debug(@"HHandlerDbULoader::Start (" + PlugInId + @", key=" + id + @") - iNeedStarted=" + iNeedStarted + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (! (_iPlugin == null))
                //Подтвердить клиенту изменение состояние
                (_iPlugin as PlugInBase).DataAskedHost(new object[] { ID_DATA_ASKED_HOST.START, id });
            else
                ;

            //Проврить признак необходимости запуска "родительского" объекта
            if (iNeedStarted == 1)
            {
                //Запустиь объект и все зависимые потоки
                Start();
                //Активировать объект
                Activate(true);
            }
            else
                ;
            //Регистрация источника дфнных и установка с ним соединения
            lock (m_lockStateGroupSignals)
            {
                register(id, 0, m_connSett, string.Empty);
            }
        }
        /// <summary>
        /// Идентификатор плюгина (строка, для лог-сообщений)
        /// </summary>
        protected string PlugInId { get { return @"PlugInID=" + ((_iPlugin == null) ? @"в составе проекта" : (_iPlugin as PlugInBase)._Id.ToString()); } }
        /// <summary>
        /// Остановить объект и все зависимые потоки
        /// </summary>
        public override void Stop()
        {
            Logging.Logg().Debug(@"HHandlerDbULoader::Stop (" + PlugInId + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

            stopThreadQueue();

            Activate (false);

            //Вызвать "базовый" метод
            base.Stop();
        }
        /// <summary>
        /// Остановить группу сигналов по указанному идентификатору
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов</param>
        public virtual void Stop(int id)
        {
            int iNeedStopped = 0; //Признак необходимости останова "родительского" объекта

            lock (m_lockStateGroupSignals)
            {
                if ((!(m_dictGroupSignals == null))
                    && (m_dictGroupSignals.Keys.Contains (id) == true))
                {
                    if (m_dictGroupSignals[id].IsStarted == true)
                    {
                        m_dictGroupSignals[id].Stop();
                    }
                    else
                        ;
                }
                else
                    iNeedStopped = -1;
            }
            //Проверить возможность останова объекта
            if (! (iNeedStopped < 0))
            {
                if (! (_iPlugin == null))
                    //Подтвердить клиенту останов группы сигналов
                    (_iPlugin as PlugInBase).DataAskedHost(new object[] { ID_DATA_ASKED_HOST.STOP, id });
                else
                    ;

                //Установить необходимость останова "родительского" объекта
                try
                {
                    iNeedStopped = IsStarted == false ? 1 : 0;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::Stop (" + PlugInId + @", key=" + id + @") - ...");
                }
                //Проверить необходимость останова "родительского" для группы сигнала объекта
                if (iNeedStopped == 1)
                {
                    Activate(false);
                    Stop();
                }
                else
                    ;
            }
            else
                ;
        }
        /// <summary>
        /// Запустить поток обработки очереди событий
        /// </summary>
        /// <returns>Результат запуска потока</returns>
        protected int startThreadQueue()
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

                //m_semaQueue = new Semaphore(1, 1);
                //m_semaQueue.WaitOne();
                m_autoResetEvtQueue = new AutoResetEvent (false);
                //m_mnlResetEvtQueue = new ManualResetEvent (false);

                //InitializeSyncState();
                //Установить в "несигнальное" состояние
                m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne(System.Threading.Timeout.Infinite, true);

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
        /// <summary>
        /// Остановить поток обработки очереди событий
        /// </summary>
        /// <returns></returns>
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
                try
                {
                    //m_semaQueue.Release(1);
                    m_autoResetEvtQueue.Set();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "HHandlerDbULoader::stopThreadQueue () - m_semaQueue.Release(1)");
                }
                //Ожидать завершения потоковой функции
                joined = m_threadQueue.Join(666);
                //Проверить корректное завершение потоковой функции
                if (joined == false)
                    //Завершить аварийно потоковую функцию
                    m_threadQueue.Abort();
                else
                    ;

                //m_semaQueue = null;                
                m_autoResetEvtQueue = null;
                m_queueIdGroupSignals.Clear();
                m_threadQueue = null;
            }
            else ;

            return iRes;
        }
        /// <summary>
        /// Проверить наличие ответа на запрос к источнику данных
        /// </summary>
        /// <param name="state">Состояние</param>
        /// <param name="error">Признак ошибки</param>
        /// <param name="table">Таблица - результат запроса</param>
        /// <returns>Результат проверки наличия ответа на запрос</returns>
        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            return response(out error, out table);
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Класс для связи клиента - загрузчика библиотеки и целевого объекта в библиотеке
    /// </summary>
    public class PlugInULoader : PlugInBase
    {
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public PlugInULoader()
            : base()
        {
        }
        /// <summary>
        /// Обработчик запросов от клиента
        /// </summary>
        /// <param name="obj"></param>
        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost; //Переданные значения из-вне
            HHandlerDbULoader target = _object as HHandlerDbULoader; //Целевой объект

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.INIT_SOURCE: //Приняты параметры для инициализации целевого объекта
                    target.Initialize(ev.par as object []);
                    break;
                case (int)ID_DATA_ASKED_HOST.INIT_SIGNALS: //Приняты параметры инициализации группы сигналов
                    //Инициализация группы сигналов по идентифактору [0]
                    if (! (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0))
                        //??? ошибка
                        ;
                    else
                        ;
                    break;
                case (int)ID_DATA_ASKED_HOST.START: //Принята команда на запуск группы сигналов
                    //Проверить признак получения целевым объектом параметоров для инициализации
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_SOURCE) == true)
                    {
                        //Инициализация группы сигналов по идентифактору [0]
                        if (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0)
                            //Запустить на выполнение группу сигналов
                            target.Start((int)(ev.par as object[])[0]);
                        else
                            //Отправить запрос клиенту для получения параметров инициализации для группы сигналов
                            DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SIGNALS, (int)(ev.par as object[])[0] });
                    }
                    else
                        //Отправить запрос клиенту для получения целевым объектом параметоров для инициализации
                        DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SOURCE, (int)(ev.par as object[])[0] });
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Stop((int)(ev.par as object[])[0]);
                    break;
                default:
                    break;
            }
            //Вызвать метод "базового" объекта
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
