using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;

using HClassLibrary;

namespace uLoaderCommon
{
    public class HTimeSpan : object
    {
        private bool isError
        {
            get
            {
                return (_value == TimeSpan.MinValue)
                    || (! (_prefix.Length == 2));
            }
        }
        
        public HTimeSpan(string value)
        {
            Text = value;
        }

        private string _prefix;
        private TimeSpan _value;
        public TimeSpan Value { get { return _value; } }

        public string Text
        {
            get { return ToString(); }
            
            set
            {
                _value = parse(value, out _prefix);//ошибка парсер(mi0)

                if (isError == true)
                    Logging.Logg().Error(@"HTimeSpan::ctor () - error parsing value ...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;
            }
        }

        public override string ToString()
        {
            string strRes = string.Empty;
            int iSign = 0
                , iValue = 0;

            if (isError == false)
            {
                iSign = _value.TotalMilliseconds < 0 ? -1 : 0;

                switch (_prefix)
                {
                    case @"ms":
                        iValue = (int)_value.TotalMilliseconds;
                        break;
                    case @"ss":
                        iValue = (int)_value.TotalSeconds;
                        break;
                    case @"mi":
                        iValue = (int)_value.TotalMinutes;
                        break;
                    case @"hh":
                        iValue = (int)_value.TotalHours;
                        break;
                    case @"dd":
                        iValue = (int)_value.TotalDays;
                        break;
                    default:
                        break;
                }

                if (iSign < 0)
                    strRes = "-";
                else
                    ;

                strRes += _prefix + iValue.ToString();
            }
            else
                ;

            return strRes;
        }

        private HTimeSpan(string prefix, int iValue)
            : this((iValue < 0 ? @"-" : string.Empty) + prefix + Math.Abs(iValue).ToString ())
        {
        }

        private static TimeSpan parse(string value, out string prefix)
        {
            prefix = string.Empty;

            TimeSpan tsRes = TimeSpan.Zero;
            Char ch = Char.MinValue;
            int iSign = 0
                , iValue = 0;

            if (value.Length > 0)
            {
                ch = value[0];
                iSign = iSign = (ch.Equals('+') == true) ? 1 : (ch.Equals('-') == true) ? -1 : 0;
            }
            else
                // признак ошибки
                tsRes = TimeSpan.MinValue;

            if (tsRes == TimeSpan.Zero)
                if (value.Length > (Math.Abs(iSign) + 2))
                {
                    prefix = value.Substring(Math.Abs(iSign), 2);
                    iValue = Int32.Parse(value.Substring(Math.Abs(iSign) + 2));

                    switch (prefix)
                    {
                        case @"ms":
                            tsRes = TimeSpan.FromMilliseconds(iValue);
                            break;
                        case @"ss":
                            tsRes = TimeSpan.FromSeconds(iValue);
                            break;
                        case @"mi":
                            tsRes = TimeSpan.FromMinutes(iValue);
                            break;
                        case @"hh":
                            tsRes = TimeSpan.FromHours(iValue);
                            break;
                        case @"dd":
                            tsRes = TimeSpan.FromDays(iValue);
                            break;
                        default:
                            // признак ошибки
                            tsRes = TimeSpan.MinValue;
                            break;
                    }

                    if (iSign < 0)
                        tsRes = TimeSpan.Zero - tsRes;
                    else
                        ;
                }
                else
                    // признак ошибки
                    tsRes = TimeSpan.MinValue;
            else
                ;

            return tsRes;
        }

        public static HTimeSpan NotValue { get { return HTimeSpan.FromMilliseconds(-1); } }

        public static HTimeSpan FromMilliseconds(int msecs)
        {
            return new HTimeSpan(@"ms", msecs);
        }
        
        public static HTimeSpan FromSeconds(int secs)
        {
            return new HTimeSpan(@"ss", secs);
        }

        public static HTimeSpan FromMinutes(int mins)
        {
            return new HTimeSpan(@"mi", mins);
        }

        public static HTimeSpan FromHours(int hours)
        {
            return new HTimeSpan(@"hh", hours);
        }

        public static HTimeSpan FromDays(int day)
        {
            return new HTimeSpan(@"dd", day);
        }

        public override bool Equals(object obj)
        {
            return (obj is HTimeSpan) == true ? this == obj as HTimeSpan : false;
        }

        public static bool operator ==(HTimeSpan equ1, HTimeSpan equ2)
        {
            return equ1.Value == equ2.Value;
        }

        public static bool operator !=(HTimeSpan equ1, HTimeSpan equ2)
        {
            return equ1.Value != equ2.Value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    
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
    //TODO: перенести в общую библиотеку, т.к.
    /// <summary>
    /// Перечисление - способы назначения меток времени
    /// </summary>
    public enum MODE_DATA_DATETIME
    {
        /// <summary>
        /// Метка для значения за интервал назначается началом интервала
        /// </summary>
        Begined
        /// <summary>
        /// Метка для значения за интервал назначается окончанием интервала
        /// </summary>
        , Ended
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
        //public HTimeSpan m_tsUTCOffset;
        /// <summary>
        /// Ссылка на объект "связи" с клиентом
        /// </summary>
        protected PlugInULoader _iPlugin;
        /// <summary>
        /// Разность между часовыми поясами даты/времени сервера и метками даты/времени значений в БД
        /// </summary>
        public HTimeSpan m_tsOffsetUTCToServer
            , m_tsOffsetUTCToData
            , m_tsOffsetUTCToQuery;
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
                foreach (SIGNAL s in m_arSignals)
                    s.ClearFormula();

                State = GroupSignals.STATE.STOP;
                TableRecieved = null;
            }

            private uLoaderCommon.MODE_WORK m_mode;
            public uLoaderCommon.MODE_WORK Mode { get { return m_mode; } set { m_mode = value; } }

            private TimeSpan m_tsPeriodMain
                , m_tsPeriodLocal;
            /// <summary>
            /// Период времени для формирования запроса значений (глобальный)
            /// </summary>
            public TimeSpan PeriodMain { get { return m_tsPeriodMain; } 
                set { m_tsPeriodMain = value; } }
            /// <summary>
            /// Период времени для формирования запроса значений (локальный)
            /// </summary>
            public TimeSpan PeriodLocal { get { return m_tsPeriodLocal; } 
                set { m_tsPeriodLocal = value; } }
            /// <summary>
            /// Таблица источника/назначения
            /// </summary>
            public string NameTable { get; set; }
            private long m_msecIntervalLocal;
            /// <summary>
            /// Интервал (милисекунды) между опросами значений
            /// </summary>
            public long MSecIntervalLocal { get { return m_msecIntervalLocal; } set { m_msecIntervalLocal = value; } }

            /// <summary>
            /// Класс для объекта СИГНАЛ
            /// </summary>
            public class SIGNAL
            {
                //private GroupSignals _parent;
                /// <summary>
                /// Строка с идентификатором формулы и указанием идентификаторов-аргументов
                /// </summary>
                private string m_strFormula;
                /// <summary>
                /// Ключ формулы
                /// </summary>
                public string m_fKey;
                /// <summary>
                /// Список идентификаторов (idMain) - аргументов формулы
                /// </summary>
                public List<int> m_listIdArgs;
                /// <summary>
                /// Идентификатор сигнала, уникальный в границах приложения
                /// </summary>
                public int m_idMain;
                /// <summary>
                /// Конструктор - основной (с параметром)
                /// </summary>
                /// <param name="idMain">Идентификатор сигнала, уникальный в границах приложения</param>
                public SIGNAL(GroupSignals parent, int idMain, object formula)
                {
                    //_parent = parent;

                    this.m_idMain = idMain;

                    setFormula(formula);
                }

                public bool IsFormula { get { return (m_strFormula.Equals(string.Empty) == false) && (m_fKey.Equals(string.Empty) == false); } }

                private void setFormula(object oFormula)
                {
                    string formula = string.Empty;
                    
                    m_strFormula =
                    m_fKey =
                         string.Empty;

                    if (oFormula.GetType().Equals(typeof(string)) == true)
                    {// требуется распознать формула ИЛИ стандартный текстовый идентификатор (например: KKS_NAME, nameTable)
                        formula = oFormula as string;
                        //!!! наличие скобок - открыть, закрыть ПРИЗНАК формулы
                        if ((!(formula.IndexOf('(') < 0))
                            && (!(formula.IndexOf(')') < 0)))
                        {
                            m_strFormula = formula as string;
                            m_fKey = m_strFormula.Substring(0, formula.IndexOf('('));
                        }
                        else
                            ;
                    }
                    else
                        //if (formula.GetType().IsPrimitive == true)
                        //    ;
                        //else
                            ;
                }
                /// <summary>
                /// Возвратить массив индексов сигналов, являющихся аргументами формулы
                /// </summary>
                /// <returns>Массив с индексами сигналов</returns>
                public int[] GetIndexArgs(out int error)
                {
                    error = 0;
                    
                    int[] arRes = null;

                    int iStartArgs = -1
                        , indxArg = -1;
                    string[] indxArgs = null;
                    // индекс символа в строке, где начинается перечисление индексов сигналов
                    iStartArgs = m_strFormula.IndexOf('(') + 1;
                    // проверить наличие аргументов
                    if (iStartArgs > 0)
                    {
                        // получить список индексов аргументов
                        indxArgs = m_strFormula.Substring(iStartArgs, m_strFormula.Length - iStartArgs - 1).Split(';');
                        if (indxArgs.Length > 0)
                        {
                            // выделить память для результата
                            arRes = new int[indxArgs.Length];

                            for (int i = 0; i < indxArgs.Length; i++)
                            {
                                if (int.TryParse(indxArgs[i], out indxArg) == true)
                                    arRes[i] = indxArg;
                                else
                                {
                                    error = -3;
                                    arRes[i] = -1;
                                    break;
                                }
                            }
                        }
                        else
                            error = -2; // кол-во аргументов = 0
                    }
                    else
                    {
                        error = -1; // нет аргументов

                        arRes = new int[] { };
                    }

                    return arRes;
                }
                /// <summary>
                /// Добавить идентификатор аргумента при вычислении формулы (порядок учитывать)
                /// </summary>
                /// <param name="idMain">Идентификатор аргумента</param>
                public void AddIdArg(int idMain)
                {
                    if (m_listIdArgs == null)
                        m_listIdArgs = new List<int>();
                    else
                        ;

                    m_listIdArgs.Add(idMain);
                }
                /// <summary>
                /// Очистить значение и аргументы для формулы (с проверкой наличия формулы)
                /// </summary>
                public void ClearFormula()
                {
                    if (IsFormula == true)
                    {
                        m_strFormula = string.Empty;
                        if (!(m_listIdArgs == null))
                            m_listIdArgs.Clear();
                        else
                            ;
                    }
                    else
                        ;
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
                int err = 0;

                int[] indxArgs = null;
                
                //Владелец объекта
                _parent = parent;
                //Целочисленный идентификатор
                m_Id = id;
                //Значения по умолчанию
                m_tsPeriodMain =
                m_tsPeriodLocal =
                    new TimeSpan((long)((int)uLoaderCommon.DATETIME.SEC_SPANPERIOD_DEFAULT * Math.Pow(10, 7)));
                m_msecIntervalLocal = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_DEFAULT;

                //Инициализировать массив сигналов
                if (pars.Length > 0)
                {
                    m_arSignals = new SIGNAL[pars.Length];
                    // создать сигналы
                    for (int i = 0; i < pars.Length; i++)
                        m_arSignals[i] = createSignal (pars[i] as object []);
                    // при наличии формул определить и установить идентификаторы аргументов
                    for (int i = 0; i < m_arSignals.Length; i++)
                        if (m_arSignals[i].IsFormula == true)
                        {
                            indxArgs = m_arSignals[i].GetIndexArgs(out err);

                            if (err == 0)
                            {
                                foreach (int indxArg in indxArgs)
                                    m_arSignals[i].AddIdArg(m_arSignals[indxArg].m_idMain);
                            }
                            else
                                Logging.Logg().Error(@"GroupSignals::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                            ; // нет формулы
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

            public class DataTableDuplicate
            {
                private enum INDEX_RESULT { UNKNOWN = -1, EQUALE, DISTINCT, COUNT };
                private DataTable[] _arTables;

                public DataTable TableEquale { get { return _arTables[(int)INDEX_RESULT.EQUALE]; } set { _arTables[(int)INDEX_RESULT.EQUALE] = value; } }
                public DataTable TableDistinct { get { return _arTables[(int)INDEX_RESULT.DISTINCT]; } set { _arTables[(int)INDEX_RESULT.DISTINCT] = value; } }

                public DataTableDuplicate()
                {
                    _arTables = new DataTable[(int)INDEX_RESULT.COUNT];
                }
                /// <summary>
                /// Признак наличия строк после удаления дублирующих
                ///  по итогам сравнения предыдущей и текущей таблиц 
                /// </summary>
                public bool IsDeterminate { get { return Equals(TableDistinct, null) == false ? TableDistinct.Rows.Count > 0 : false; } }
                /// <summary>
                /// Очистить "текущую" таблицу от записей,
                ///  содержащихся в "предыдущей" таблице
                /// </summary>
                /// <param name="tblPrev">"Предыдущая" таблица</param>
                /// <param name="tblRes">"Текущая" таблица</param>
                /// <param name="keyFields">Наименования полей в составе ключа по которому происходит сравнение записей</param>
                public void Clear(DataTable tblPrev, DataTable tblCur, string keyFields = @"ID, DATETIME")
                {
                    int iDup = 0;
                    DataRow[] arSel = null;

                    if (!(tblCur == null))
                        if ((((!(tblCur.Columns.IndexOf(@"ID") < 0)) && (!(tblCur.Columns.IndexOf(@"DATETIME") < 0)))
                            && (tblCur.Rows.Count > 0)))
                        {
                            _arTables[(int)INDEX_RESULT.EQUALE] = tblCur.Clone();
                            _arTables[(int)INDEX_RESULT.DISTINCT] = tblCur.Copy();

                            if (!(tblPrev == null))
                                if (!(tblPrev.Columns.IndexOf(@"DATETIME") < 0))
                                    foreach (DataRow rRes in tblPrev.Rows)
                                    {
                                        arSel = _arTables[(int)INDEX_RESULT.DISTINCT].Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fffffff") + @"'");
                                        iDup += arSel.Length;
                                        foreach (DataRow rDel in arSel)
                                        {
                                            _arTables[(int)INDEX_RESULT.EQUALE].ImportRow(rDel);
                                            _arTables[(int)INDEX_RESULT.DISTINCT].Rows.Remove(rDel);
                                        }

                                        _arTables[(int)INDEX_RESULT.EQUALE].AcceptChanges();
                                        _arTables[(int)INDEX_RESULT.DISTINCT].AcceptChanges();
                                    }
                                else
                                    ;
                            else
                                ;

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
                            ; // текущая таблица не имеет требуемую структуру
                    else
                        ; // текущая таблица = null
                }
                /// <summary>
                /// Удалить дублированные записи из таблицы
                /// </summary>
                /// <param name="tblDup">Таблица, содержащая дублирующие записи</param>
                /// <returns>Таблица без дублирующих записей</returns>
                public static DataTable Clear(DataTable tblDup)
                {
                    DataTable tblRes = tblDup.Clone();
                    ////Вариант №1
                    ////Список индексов строк для удаления
                    //List<int> listIndxToDelete = new List<int>();
                    ////Массив дублированных строк
                    //DataRow[] arDup = null;
                    ////Признак наличия кавычек для значений в поле [ID]
                    //bool bQuote = false;
                    ////Строка запроса для поиска дублирующих записей
                    //string strSel = string.Empty;
                    //Вариант№2
                    List<DataRow> listRes = null;

                    if ((!(tblDup.Columns.IndexOf(@"ID") < 0))
                        && (!(tblDup.Columns.IndexOf(@"DATETIME") < 0)))
                    {
                        try
                        {
                            ////Вариант №1
                            //bQuote = !tblDup.Columns[@"ID"].DataType.IsPrimitive;

                            //foreach (DataRow r in tblDup.Rows)
                            //{
                            //    //Проверить наличие индекса строки в уже найденных (как дублированные)
                            //    if (listIndxToDelete.IndexOf(tblDup.Rows.IndexOf(r)) < 0)
                            //    {
                            //        //Сформировать строку запроса
                            //        strSel = @"ID=" + (bQuote == true ? @"'" : string.Empty) + r[@"ID"] + (bQuote == true ? @"'" : string.Empty) + @" AND " + @"DATETIME='" + ((DateTime)r[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fffffff") + @"'";
                            //        arDup = (tblDup as DataTable).Select(strSel);
                            //        //Проверить наличие дублирующих записей
                            //        if (arDup.Length > 1)
                            //            //Добавить индексы всех найденных дублирующих строк в список для удаления
                            //            // , КРОМЕ 1-ой!
                            //            for (int i = 1; i < arDup.Length; i++)
                            //                listIndxToDelete.Add(tblDup.Rows.IndexOf(arDup[i]));
                            //        else
                            //            if (arDup.Length == 0)
                            //                throw new Exception("HHandlerDbULoader.GroupSignals.clearDupValues () - в таблице не найдена \"собственная\" строка...");
                            //            else
                            //                ;

                            //        //Добавить строку в таблицу-результат
                            //        tblRes.ImportRow(arDup[0]);
                            //    }
                            //    else
                            //        ;
                            //}

                            //Вариант№2
                            if (tblDup.Columns[@"ID"].DataType.Equals (typeof (int)) == true)
                                listRes = tblDup.AsEnumerable().GroupBy(g => new { ID = g.Field<int>(@"ID"), DATETIME = g.Field<DateTime>(@"DATETIME") }).Select(s => s.First()).ToList();
                            else
                                if (tblDup.Columns[@"ID"].DataType.Equals (typeof (string)) == true)
                                    listRes = tblDup.AsEnumerable().GroupBy(g => new { ID = g.Field<string>(@"ID"), DATETIME = g.Field<DateTime>(@"DATETIME") }).Select(s => s.First()).ToList();
                                else
                                    listRes = tblDup.AsEnumerable().GroupBy(g => new { ID = g.Field<object>(@"ID"), DATETIME = g.Field<DateTime>(@"DATETIME") }).Select(s => s.First()).ToList();
                            //Добавить строки в таблицу-результат
                            if (!(listRes == null))
                                listRes.ForEach(r => tblRes.ImportRow(r));
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"HHandlerDbULoader.GroupSignals.DataTableDuplicate::Clear () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                            tblRes.Clear();
                        }
                    }
                    else
                        // отсутствует необходимое поле "ID"
                        Logging.Logg().Warning(@"HHandlerDbULoader.GroupSignals.DataTableDuplicate::Clear  () - отсутствует необходимое поле [ID], [DATETIME] ...", Logging.INDEX_MESSAGE.NOT_SET);

                    //Принять внесенные изменения в таблицу-результат
                    tblRes.AcceptChanges();

                    return tblRes;
                }
                /// <summary>
                /// Очистить "текущую" таблицу от записей,
                ///  с более старыми метками времени
                /// </summary>
                /// <param name="tblPrev">"Предыдущая" таблица</param>
                /// <param name="tblRes">"Текущая" таблица</param>
                /// <param name="keyFields">Наименования полей в составе ключа по которому происходит сравнение записей</param>
                public void Top(DataTable tblPrev, DataTable tblCur, string keyFields = @"ID, DATETIME")
                {
                    int iDup = 0;
                    DataRow[] arPrev = null;
                    //int iIdType = -1; // неизвестный тип поля [ID] таблиц [tblPrev], [tblCur]

                    if (!(tblCur == null))
                        if ((((!(tblCur.Columns.IndexOf(@"ID") < 0)) && (!(tblCur.Columns.IndexOf(@"DATETIME") < 0)))
                            && (tblCur.Rows.Count > 0)))
                        {
                            var listCurDistinct = tblCur.AsEnumerable().GroupBy(g => g[@"ID"]).Select(s => s.Last()).ToList();

                            _arTables[(int)INDEX_RESULT.DISTINCT] = tblCur.Clone();
                            _arTables[(int)INDEX_RESULT.EQUALE] = tblCur.Clone();

                            if ((!(tblPrev == null))
                                && (((!(tblPrev.Columns.IndexOf(@"ID") < 0)) && (!(tblPrev.Columns.IndexOf(@"DATETIME") < 0)))
                                && (tblPrev.Rows.Count > 0)))
                            {
                                //iIdType = tblPrev.Columns[tblPrev.Columns.IndexOf(@"ID")].GetType().Ge

                                var listPrevDistinct = tblPrev.AsEnumerable().GroupBy(g => g[@"ID"]).Select(s => s.Last()).ToList();

                                foreach (DataRow rCur in listCurDistinct)
                                {
                                    arPrev = listPrevDistinct.Where(r => Convert.ToInt32(r[@"ID"]) == Convert.ToInt32(rCur[@"ID"])).ToArray(); //tblPrev.Select(@"ID=" + rCur[@"ID"]);
                                    if (arPrev.Length > 0)
                                        if (arPrev.Length == 1)
                                            if (!(DateTime.Compare((DateTime)rCur[@"DATETIME"], (DateTime)arPrev[0][@"DATETIME"]) < 0))
                                            {// удалить более старую (ИЛИ с одинаковой меткой времени) запись
                                                _arTables[(int)INDEX_RESULT.DISTINCT].ImportRow(rCur);
                                                _arTables[(int)INDEX_RESULT.DISTINCT].AcceptChanges();
                                            }
                                            else
                                            {
                                                _arTables[(int)INDEX_RESULT.EQUALE].ImportRow(rCur);
                                                _arTables[(int)INDEX_RESULT.EQUALE].AcceptChanges();
                                            }
                                        else
                                            ; //??? ошибка записей >, чем 1
                                    else
                                        ; // в предыдущей таблице нет сигнала с идентификатором
                                }
                            }
                            else
                                // предыдущая таблица не существует, не имеет одного из полей [ID], [DATETIME] составного ключа
                                foreach (DataRow rCur in listCurDistinct)
                                    _arTables[(int)INDEX_RESULT.DISTINCT].ImportRow(rCur);

                            _arTables[(int)INDEX_RESULT.DISTINCT].AcceptChanges();
                        }
                        else
                            ; // текущая таблица не имеет одного из полей составного ключа ИЛИ не имеет ни одной строки
                    else
                        ; // текущая таблица не существует
                }
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
        public MODE_WORK Mode
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
        protected TimeSpan PeriodMain
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].PeriodMain;
                else
                    throw new Exception(@"ULoaderCommon::PeriodMain.get ...");
            }
        }
        /// <summary>
        /// Период времени опроса текущей (обрабатываемой) группы сигналов
        /// </summary>
        protected TimeSpan PeriodLocal
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].PeriodLocal;
                else
                    throw new Exception(@"ULoaderCommon::PeriodLocal.get ...");
            }
        }
        /// <summary>
        /// Интервал (милисекунды) между опросами значений обрабатываемой группы сигналов
        /// </summary>
        protected long MSecIntervalLocal
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return m_dictGroupSignals[IdGroupSignalsCurrent].MSecIntervalLocal;
                else
                    throw new Exception(@"ULoaderCommon::MSecIntervalLocal.get ...");
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

                    //string msg = @"HHandlerDbULoader::TableRecieved.set - "
                    //    + @"[" + PlugInId
                    //    + @", key=" + IdGroupSignalsCurrent + @"] "
                    //    + @"строк_было=" + cntPrev
                    //    + @", строк_стало=" + value.Rows.Count
                    //    + @" ...";
                    //Console.WriteLine (msg);
                    //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

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
        /// Объект синхронизации - указывает выполняется ли обработка состояния в текущий момент
        /// </summary>
        protected ManualResetEvent m_manualEvtStateHandlerCompleted;
        /// <summary>
        /// Целочисленный идентификатор группы сигналов
        ///  , ключ для словаря с группами сигналов
        ///  , уникальный в границах приложения, передается из-вне (файл конфигурации)
        /// </summary>
        protected virtual int IdGroupSignalsCurrent {
            get { return m_iIdGroupSignalsCurrent; }

            set {
                if (value == -1)
                    m_manualEvtStateHandlerCompleted.Set();
                else
                    m_manualEvtStateHandlerCompleted.Reset();

                m_iIdGroupSignalsCurrent = value;
            }
        }

        private Semaphore m_semaInitId;
        private ManualResetEvent m_evtInitSource;
        private object m_lockInitSource;
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

            m_semaInitId = new Semaphore (1, 1);
            m_evtInitSource = new ManualResetEvent (false);
            m_lockInitSource = new object ();
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
        public HHandlerDbULoader(PlugInULoader iPlugIn)
            : this()
        {
            this._iPlugin = iPlugIn;
            m_connSett = null;
        }
        /// <summary>
        /// Инициализация данных объекта
        /// </summary>
        /// <param name="pars">Массив параметров для инициализации</param>
        /// <returns>Признак выполнения</returns>
        public virtual int Initialize(object[] pars)
        {
            int iRes = 0;
            string strMsg = string.Empty;

            lock (m_lockInitSource)
            {
                if (m_connSett == null) {
                    //Значения параметров соединения с источником данных
                    m_connSett = new ConnectionSettings((pars[0] as ConnectionSettings));

                    strMsg = @"HHandlerDbUloader::Initialize [" + string.Format(@"ID={0}:{1}", _iPlugin._Id, _iPlugin.KeySingleton) + @"] - объект: ConnectionSettings.ID=" + m_connSett.id + @" ...";
                    //Console.WriteLine(strMsg);
                    Logging.Logg ().Debug (strMsg, Logging.INDEX_MESSAGE.NOT_SET);

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

                    //
                    m_tsOffsetUTCToServer = HTimeSpan.NotValue;
                    if (m_dictAdding.ContainsKey(@"OFFSET_UTC_TO_SERVER") == true)
                        m_tsOffsetUTCToServer = new HTimeSpan(m_dictAdding[@"OFFSET_UTC_TO_SERVER"]);
                    else
                        ;

                    m_tsOffsetUTCToData = HTimeSpan.NotValue;
                    if (m_dictAdding.ContainsKey(@"OFFSET_UTC_TO_DATA") == true)
                        m_tsOffsetUTCToData = new HTimeSpan(m_dictAdding[@"OFFSET_UTC_TO_DATA"]);
                    else
                        ;

                    m_tsOffsetUTCToQuery = HTimeSpan.NotValue;
                    if (m_dictAdding.ContainsKey(@"OFFSET_UTC_TO_QUERY") == true)
                        m_tsOffsetUTCToQuery = new HTimeSpan(m_dictAdding[@"OFFSET_UTC_TO_QUERY"]);
                    else
                        ;

                    m_evtInitSource.Set ();
                } else
                    ;
            }

            return iRes;
        }
        /// <summary>
        /// Инициализация группы сигналов
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов</param>
        /// <param name="pars">Параметры группы сигналов для инициализации</param>
        /// <returns>Результат выполнения метода</returns>
        public virtual int Initialize(/*INDEX_INIT_PARAMETER indxPars, */int id, object[] pars)
        {
            int iRes = 0;

            string[] arFormula = null;
            string fKey = string.Empty
                , formula = string.Empty;

            try
            {
                lock (m_lockStateGroupSignals)
                {
                    if (m_dictGroupSignals.Keys.Contains(id) == false)
                    {//Считать переданные параметры - параметрами сигналов
                        m_dictGroupSignals.Add(id, createGroupSignals (id, pars));

                        //Logging.Logg().Debug(@"HHandlerDbULoader::Initialize () - добавить группу сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
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

                                Logging.Logg().Debug(string.Format(@"HHandlerDbULoader::Initialize () - ПЕРЕсоздать группу сигналов [ID={0}:{1}, key={2}]..."
                                        , _iPlugin._Id, _iPlugin.KeySingleton, id)
                                    , Logging.INDEX_MESSAGE.NOT_SET);
                            }
                            else
                            {//Считать переданные параметры - параметрами группы сигналов
                                //lock (m_lockStateGroupSignals)
                                //{
                                    m_dictGroupSignals[id].Mode = (uLoaderCommon.MODE_WORK)pars[0];
                                    m_dictGroupSignals[id].State = GroupSignals.STATE.STOP;
                                    //Переопределено в 'HHandlerDbULoaderDatetimeSrc'
                                    //if (m_dictGroupSignals[id].Mode == MODE_WORK.COSTUMIZE)
                                    //    if ((!(((DateTime)pars[1] == null)))
                                    //        && (!(((DateTime)pars[1] == DateTime.MinValue))))
                                    //        m_dictGroupSignals[id]. = (DateTime)pars[1];
                                    //    else
                                    //        ;
                                    //else
                                    //    ;
                                    m_dictGroupSignals[id].PeriodMain = (TimeSpan)pars[2];
                                    m_dictGroupSignals[id].PeriodLocal = (TimeSpan)pars[3];
                                    m_dictGroupSignals[id].MSecIntervalLocal = (int)pars[4];
                                    if(pars.Length>=7)
                                        m_dictGroupSignals[id].NameTable = (string)pars[6];
                                    // инициализация расчетных формул
                                    // только для Source
                                    //}

                        //Logging.Logg().Debug(@"HHandlerDbULoader::Initialize () - параметры группы сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"HHandlerDbULoader::Initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);

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

            string msgDebug = string.Format(@"HHandlerDbULoader::enqueue () - [ID={0}:{1}, key={2}] - queue.Count=", _iPlugin._Id, _iPlugin.KeySingleton, key);
            int keyQueueCount = -1;

            lock (m_lockQueue)
            {
                keyQueueCount = m_queueIdGroupSignals.Count (delegate(int i1) { return i1 == key; });

                msgDebug += string.Format(@"{0}({1})", QueueCount, iRes = keyQueueCount);

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
                            Logging.Logg().Exception(e, string.Format(@"{0} ...", msgDebug), Logging.INDEX_MESSAGE.NOT_SET);
                            msgDebug = string.Empty;
                        }
                    else
                        ;
                else
                    msgDebug += @"-ПРОПУСК!";
            }

            if (msgDebug.Equals (string.Empty) == false)
                Logging.Logg().Debug(string.Format(@"{0} ...", msgDebug), Logging.INDEX_MESSAGE.NOT_SET);
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
            GroupSignals.STATE newState = GroupSignals.STATE.UNKNOWN;

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

                    try
                    {
                        //m_manualEvtStateHandlerCompleted.Reset();
                        //Получить объект очереди событий
                        IdGroupSignalsCurrent = m_queueIdGroupSignals.Peek();

                        lock (m_lockStateGroupSignals)
                        {
                            State = GroupSignals.STATE.ACTIVE;
                        }

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

                        lock (m_lockStateGroupSignals)
                        {
                            newState = GroupSignals.NewState(Mode, State);
                            State = newState;
                        }

                        if (! (_iPlugin == null))
                            ((PlugInBase)_iPlugin).DataAskedHost(getDataAskedHost ());
                        else
                            ;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fThreadQueue () - окончание обработки группы событий очереди (" + PlugInId + @", ID_GSGNLS=" + IdGroupSignalsCurrent + @")", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"HHandlerDbULoader.fThreadQueue () - IdGroupSignalsCurrent=" + IdGroupSignalsCurrent + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    finally
                    {
                        IdGroupSignalsCurrent = -1;
                    }
                }
            }
            //Освободить ресурс ядра ОС
            //??? "везде" 'true'
            try {
                if (bRes == false)
                    m_autoResetEvtQueue.Reset();
                else
                    ;

                m_autoResetEvtQueue.Close();
                //??? предполагается, что для этого объекта 'Reset' уже выполнен
                IdGroupSignalsCurrent = -1;                
            } catch (Exception e) { //System.Threading.SemaphoreFullException
                Logging.Logg().Exception(e, "HHandlerDbULoader::fThreadQueue () - m_autoResetEvtQueue.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
            } finally {
                m_autoResetEvtQueue.Close();
                m_manualEvtStateHandlerCompleted.Close();
            }
        }
        /// <summary>
        /// Возвратить массив объектов для передачи клиенту
        /// </summary>
        /// <returns>Массив объектов для передачи сообщения серверу</returns>
        protected virtual object [] getDataAskedHost ()
        {
            // null - для передачи дополнительной информации
            //Для передачи дополнительной информации - переопределить метод, заполнить 'array[3]'
            return new object[] { _iPlugin.KeySingleton, ID_DATA_ASKED_HOST.TABLE_RES, IdGroupSignalsCurrent, TableRecieved, null };
        }

        protected virtual object[] getConfirmStartAskedHost(int id)
        {
            // null - для передачи дополнительной информации
            //Для передачи дополнительной информации - переопределить метод, заполнить 'array[3]'
            return new object[]
                        { _iPlugin.KeySingleton 
                            , ID_DATA_ASKED_HOST.START
                            , id
                            , ID_HEAD_ASKED_HOST.CONFIRM
                            , null
                        };
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
            {
                base.register(id, indx, connSett, name);

                Logging.Logg().Debug(@"HHandlerDbLoader::register (" + string.Format(@"ID={0}:{1}, key={2}", _iPlugin._Id, _iPlugin.KeySingleton, id) + @")"
                        + @" iListenerId = " + m_dictIdListeners[id][indx]
                        + @"; кол-во_групп=" + m_dictGroupSignals.Count
                        + @", идентификаторов_источников=" + m_dictIdListeners.Count
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }
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

            Logging.Logg().Debug(@"HHandlerDbULoader::Start (" + string.Format(@"ID={0}:{1}", _iPlugin._Id, _iPlugin.KeySingleton) + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);            
        }

        public bool IsInitSource { get { lock (m_lockInitSource) { return !(m_connSett == null); } } }
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

                if ((bRes == true)
                    && (base.IsStarted == false))
                    Logging.Logg ().Error(string.Format(@"HHandlerDbULoader::IsStarted.get [{0}:{1}] - несовпадение признака 'Старт' с базовым классом..."
                            , _iPlugin._Id, _iPlugin.KeySingleton)
                        , Logging.INDEX_MESSAGE.NOT_SET);
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
            m_semaInitId.WaitOne ();

            //Console.WriteLine(@"HHandlerDbULoader::Start (" + string.Format(@"ID={0}:{1}, key={2}", _iPlugin._Id, _iPlugin.KeySingleton, id) + @") - ...");
            Logging.Logg().Debug(@"HHandlerDbULoader::Start (" + string.Format(@"ID={0}:{1}, key={2}", _iPlugin._Id, _iPlugin.KeySingleton, id) + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

            int iNeedStarted = -1; //Признак необходимости запуска "родительского" объекта
            GroupSignals.STATE initState = GroupSignals.STATE.UNKNOWN; //Новое состояние группы сигналов при старте
            //Установить признак необходимости запуска "родительского" объекта
            try
            {
                iNeedStarted = IsStarted == false ? 1 : 0;

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

                //Console.WriteLine(@"HHandlerDbULoader::Start (" + string.Format(@"ID={0}:{1}, key={2}", _iPlugin._Id, _iPlugin.KeySingleton, id) + @") - iNeedStarted=" + iNeedStarted + @" ...");
                Logging.Logg().Debug(@"HHandlerDbULoader::Start (" + string.Format(@"ID={0}:{1}, key={2}", _iPlugin._Id, _iPlugin.KeySingleton, id) + @") - iNeedStarted=" + iNeedStarted + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                //Проврить признак необходимости запуска "родительского" объекта
                if (iNeedStarted == 1)
                {
                    m_evtInitSource.WaitOne ();
                    //Запустиь объект и все зависимые потоки
                    Start();
                    //Активировать объект
                    Activate(true);
                }
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"HHandlerDbULoader::Start (" + string.Format(@"ID={0}:{1}, key={2}", _iPlugin._Id, _iPlugin.KeySingleton, id) + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            //Регистрация источника дфнных и установка с ним соединения
            lock (m_lockStateGroupSignals)
            {
                register(id, 0, m_connSett, string.Empty);

                if (!(_iPlugin == null))
                    //Подтвердить клиенту изменение состояние
                    (_iPlugin as PlugInBase).DataAskedHost(getConfirmStartAskedHost(id));
                else
                    ;
            }

            m_semaInitId.Release(1);
        }
        /// <summary>
        /// Остановить объект и все зависимые потоки
        /// </summary>
        public override void Stop()
        {
            lock (m_lockInitSource)
            {
                //Console.WriteLine(@"HHandlerDbULoader::Stop (" + PlugInId + @") - ...");
                Logging.Logg().Debug(@"HHandlerDbULoader::Stop (ID={0}:{1}) - ...", Logging.INDEX_MESSAGE.NOT_SET);
                // остановить поток обработки очереди событий
                stopThreadQueue();                
                //??? метод должен вызываться из-вне
                //Activate (false);
                // "забыть" парметры соединения
                m_evtInitSource.Reset ();
                m_connSett = null;
                if (! (_iPlugin == null))
                    (_iPlugin as PlugInULoader).SetMark(_iPlugin.KeySingleton, (int)ID_DATA_ASKED_HOST.INIT_SOURCE, false);
                else
                    ;

                //Вызвать "базовый" метод
                base.Stop();
            }
        }
        /// <summary>
        /// Максимальное время ожидания завершения штатной операции обработки очередного события очередью обработки событий
        /// </summary>
        private const int MSEC_MAX_TIMEOUT_STOP_ID = 6666;
        /// <summary>
        /// Остановить группу сигналов по указанному идентификатору
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов</param>
        /// <param name="direct">Направление запроса (запрос ИЛИ подтверждение получения ответа на запрос)</param>
        public virtual void Stop(int id, ID_HEAD_ASKED_HOST direct)
        {
            int iNeedStopped = 0; //Признак необходимости останова "родительского" объекта
            List<int> listIdGroupSignals = new List<int>(); // список идентификаторов групп сигналов - копия очереди для обработки (для исключения из очереди идентификатора останавливаемой группы)

            m_semaInitId.WaitOne ();
            // если обрабатывается указанная группа сигналов
            if (IdGroupSignalsCurrent == id)
            {
                lock (m_lockState)
                {
                    //Очистить все состояния
                    ClearStates();
                }
                // инициировать досрочное завершение обработки группы состояний
                this.completeHandleStates(INDEX_WAITHANDLE_REASON.SUCCESS);

                Logging.Logg().Debug(@"HHandlerDbULoader::Stop (Id=" + _iPlugin._Id + @", key=" + id + @") - ожидание окончания обработки группы сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
                // ожидать окончания обработки
                if (m_manualEvtStateHandlerCompleted.WaitOne(MSEC_MAX_TIMEOUT_STOP_ID) == false)
                    Logging.Logg().Error(string.Format(@"HHandlerDbULoader::Stop (Id={0}, key={1}) - превышено допустимое время({2}) ожидания окончания обработки группы сигналов..."
                            , _iPlugin._Id, id, MSEC_MAX_TIMEOUT_STOP_ID)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;
            }
            else
                ;

            lock (m_lockStateGroupSignals)
            {
                if ((!(m_dictGroupSignals == null))
                    && (m_dictGroupSignals.Keys.Contains (id) == true))
                {
                    if (m_dictGroupSignals[id].IsStarted == true)
                    {
                        //Console.WriteLine(@"HHandlerDbULoader::Stop (" + PlugInId + @", key=" + id + @") - ...");
                        m_dictGroupSignals[id].Stop();
                    }
                    else
                        iNeedStopped = -1;
                }
                else
                    iNeedStopped = -1;

                //Проверить возможность останова объекта
                if (! (iNeedStopped < 0))
                {
                    if (! (_iPlugin == null))
                        //Подтвердить клиенту останов группы сигналов
                        (_iPlugin as PlugInBase).DataAskedHost(new object[] { _iPlugin.KeySingleton, ID_DATA_ASKED_HOST.STOP, id, direct });
                    else
                        ;
                }
                else
                    ;

                //Установить необходимость останова "родительского" объекта
                try
                {
                    iNeedStopped = IsStarted == false ? 1 : 0;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e
                        , string.Format(@"HHandlerDbULoader::Stop (ID={0}:{1}, key={2}) - ...", _iPlugin._Id, _iPlugin.KeySingleton, id)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
                //Проверить необходимость останова "родительского" для группы сигнала объекта
                if (iNeedStopped == 1)
                {
                    Activate(false);
                    Stop();

                    if (!(_iPlugin == null))
                        //Подтвердить клиенту останов группы сигналов
                        (_iPlugin as PlugInBase).DataAskedHost(new object[] { _iPlugin.KeySingleton, ID_DATA_ASKED_HOST.STOP, int.MaxValue, direct });
                    else
                        ;
                }
                else
                    ;
            }

            lock (m_lockQueue)
            {
                listIdGroupSignals = m_queueIdGroupSignals.ToList<int>();
                m_queueIdGroupSignals.Clear();
                while (listIdGroupSignals.Contains(id) == true)
                    listIdGroupSignals.Remove(id);

                listIdGroupSignals.ForEach(delegate(int i1) { m_queueIdGroupSignals.Enqueue(i1); });
            }

            m_semaInitId.Release(1);
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
                m_manualEvtStateHandlerCompleted = new ManualResetEvent(true);

                //InitializeSyncState();
                //Установить в "несигнальное" состояние
                m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne(System.Threading.Timeout.Infinite, true);

                try { m_threadQueue.Start(); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"HHandlerDbULoader::startThreadQueue () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                iRes = 1;

            return iRes;
        }
        /// <summary>
        /// Остановить поток обработки очереди событий
        /// </summary>
        /// <returns>Результат выполнения метода</returns>
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
                    Logging.Logg().Exception(e, "HHandlerDbULoader::stopThreadQueue () - m_semaQueue.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
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
            //_MarkReversed = true;
        }

        ///// <summary>
        ///// Зарегистрировать тип объекта библиотеки
        ///// </summary>
        ///// <param name="key">Ключ регистрируемого типа объекиа</param>
        ///// <param name="type">Регистрируемый тип</param>
        //protected override void registerType(int key, Type type)
        //{
        //    base.registerType(key, type);

        //    createObject(key);
        //}

        public int CreateObject(string nameTypeObj)
        {
            int iRes = -1;

            foreach (KeyValuePair<int, Type> pair in _types)
                if (pair.Value.Name == nameTypeObj)
                {
                    if (createObject(pair.Key) == 1)
                        iRes = 0;
                    else
                        ;

                    break;
                }
                else
                    ;

            return iRes;
        }

        public int KeySingleton { get { return (_objects.Count == 1) ? _objects.Keys.ElementAt(0) : -1; } }

        public void SetMark(int id_obj, int key, bool val)
        {
            KeyValuePair<int, int> pair = new KeyValuePair<int, int>(id_obj, key);

            //m_markDataHost.Set(indx, val);
            if (m_dictDataHostCounter.ContainsKey(pair) == true)
            {
                if (val == true)
                    m_dictDataHostCounter[pair]++;
                else
                    if (val == false)
                        m_dictDataHostCounter[pair]--;
                    else
                        ; // недостижимый код

                //Console.WriteLine(@"PlugInULoader::SetMark (id=" + id_obj + @", key=" + key + @", val=" + val + @") - counter=" + m_dictDataHostCounter[pair] + @" ...");
            }
            else
                ;
        }

        protected bool isMarked(int id_obj, int key)
        {
            KeyValuePair<int, int> pair = new KeyValuePair<int, int>(id_obj, key);

            return (m_dictDataHostCounter.ContainsKey(pair) == true)
                && (m_dictDataHostCounter[pair] % 2 == 1);
        }
        /// <summary>
        /// Обработчик запросов от клиента
        /// </summary>
        /// <param name="obj"></param>
        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost; //Переданные значения из-вне
            int id_obj = KeySingleton;
            HHandlerDbULoader target = _objects[id_obj] as HHandlerDbULoader; //Целевой объект

            switch ((ID_DATA_ASKED_HOST)ev.id_detail)
            {
                case ID_DATA_ASKED_HOST.INIT_SOURCE: //Приняты параметры для инициализации целевого объекта
                    //??? проверка на повторный прием параметров
                    // требуется исключить повторную отпавку сообщения
                    if (isMarked(ev.id_main, (int)ID_DATA_ASKED_HOST.INIT_SOURCE) == false)
                        if (target.Initialize(ev.par as object[]) == 0)
                            //Подтвердить клиенту  получение параметров
                            DataAskedHost(new object[] { id_obj, ID_DATA_ASKED_HOST.INIT_SOURCE, -1, ID_HEAD_ASKED_HOST.CONFIRM });
                        else
                            ; // ошибка при инициализации
                    else
                        ; // параметры уже инициализированы
                    break;
                case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Приняты параметры инициализации группы сигналов
                    ID_HEAD_ASKED_HOST idHead = ID_HEAD_ASKED_HOST.CONFIRM;
                    //Инициализация группы сигналов по идентифактору [0]
                    if (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0)                        
                        ;
                    else
                        ; //??? сообщить об ошибке idHead = ID_HEAD_ASKED_HOST.ERROR
                    //Подтвердить клиенту  получение параметров
                    DataAskedHost(new object[] { id_obj, ID_DATA_ASKED_HOST.INIT_SIGNALS, (ev.par as object[])[0], idHead });
                    break;
                case ID_DATA_ASKED_HOST.START: //Принята команда на запуск группы сигналов
                    //Проверить признак получения целевым объектом параметоров для инициализации
                    if ((isMarked(ev.id_main, (int)ID_DATA_ASKED_HOST.INIT_SOURCE) == true) && (target.IsInitSource == true))
                    //if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_SOURCE) == true)
                    {
                        //Инициализация группы сигналов по идентифактору [0]
                        if (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0)
                            //Запустить на выполнение группу сигналов
                            target.Start((int)(ev.par as object[])[0]);
                        else
                            ////Отправить запрос клиенту для получения параметров инициализации для группы сигналов
                            //DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SIGNALS, (int)(ev.par as object[])[0], ID_HEAD_ASKED_HOST.GET })
                            ;
                    }
                    else
                        ////Отправить запрос клиенту для получения целевым объектом параметоров для инициализации
                        //DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SOURCE, (int)(ev.par as object[])[0], ID_HEAD_ASKED_HOST.GET })
                        ;
                    break;
                case ID_DATA_ASKED_HOST.STOP:
                    target.Stop((int)(ev.par as object[])[0], ID_HEAD_ASKED_HOST.CONFIRM);
                    break;
                default:
                    break;
            }
            //Вызвать метод "базового" объекта
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
