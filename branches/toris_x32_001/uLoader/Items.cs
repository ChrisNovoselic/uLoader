using System;
using System.Collections.Generic;

using System.Reflection; //Assembly
using System.IO;
using System.Data;

using System.Threading;
using System.ComponentModel;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP.Database;
using ASUTP.Helper;

namespace uLoader
{
    interface IItemSrc
    {
        void Initialize(string []values);
    }
    
    /// <summary>
    /// Базовый класс для группы элементов (источников данных, сигналов)
    /// </summary>
    public class ITEM_SRC
    {
        /// <summary>
        /// Строковый идентификатор группы элементов
        /// </summary>
        public string m_strID;
        /// <summary>
        /// Наименование группы элементов
        /// </summary>
        public string m_strShrName;
        /// <summary>
        /// Массив ключей для словаря со значениями параметров соединения (сигналов)
        /// </summary>
        public List<string> m_listSKeys;
        /// <summary>
        /// Массив ключей для словаря со значениями параметров групп сигналов
        /// </summary>
        public string[] m_GSgnlsKeys;
    }

    /// <summary>
    /// Параметры сигнала в группе сигналов (источник, назначение)
    /// </summary>
    public class SIGNAL_SRC
    {
        //Ключами для словаря являются 'ITEM_SRC.m_SKeys'
        public string[] m_arSPars;

        public SIGNAL_SRC()
        {
            m_arSPars = new string[] {};
        }
    };

    /// <summary>
    /// Перечисление - параметры, обрабатываемые для группы сигналов
    /// </summary>
    // TODO: заменить 'CURINTERVAL_PERIODLOCAL' на 'CURINTERVAL_REQUERY'
    public enum INDEX_GROUP_SIGNALS_PARAMETER {
        ID, ID_GS = 0, AUTO_START, TOOLS_ENABLED, CURINTERVAL_PERIODMAIN, CURINTERVAL_PERIODLOCAL
    }

    /// <summary>
    /// Класс для параметров группы сигналов
    /// </summary>
    public abstract class GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Строковый идентификатор группы сигналов
        /// </summary>
        public string m_strId;

        public FormMain.INDEX_SRC Index { get { return this is GROUP_SIGNALS_DEST_PARS ? FormMain.INDEX_SRC.DEST : FormMain.INDEX_SRC.SOURCE; } }
        /// <summary>
        /// Наименование краткое группы сигналов
        /// </summary>
        public string m_strShrName;
        /// <summary>
        /// Признак возможности включения режима "выборочно"
        /// </summary>
        public bool m_bToolsEnabled;
        /// <summary>
        /// Признак автоматического запуска опроса
        /// </summary>
        public int m_iAutoStart;
        /// <summary>
        /// Массив параметров для всех (== COUNT_MODE_WORK) режимов работы
        /// </summary>
        public DATETIME_WORK[] m_arWorkIntervals;
        /// <summary>
        /// Наименование таблицы источника
        /// </summary>
        public string m_TableName;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_PARS ()
        {
            m_iAutoStart = -1;
            m_bToolsEnabled = false;
            m_arWorkIntervals = new DATETIME_WORK[(int)MODE_WORK.COUNT_MODE_WORK];
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE] = new DATETIME_WORK();

            //Дата/время начала опроса (режим: выборочно)
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart = DateTime.Now;
            // округлить по прошедшему часу
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart.AddHours(-1);
            // округлить по 0-ой минуте
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart.AddMinutes(-1 * m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart.Minute);
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart.AddMilliseconds(-1 * m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart.Second * 1000 + m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_dtStart.Millisecond);
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_tsPeriodMain = HTimeSpan.FromHours(1);
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_tsIntervalCustomize = HTimeSpan.FromHours (1);
            m_arWorkIntervals[(int)MODE_WORK.CUSTOMIZE].m_tsRequery = HTimeSpan.FromMilliseconds((int)DATETIME.MSEC_INTERVAL_DEFAULT);
        }

        public bool IsUpdateParameterRequired
        {
            get
            {
                return ((Index == FormMain.INDEX_SRC.SOURCE)
                    && ((this as GROUP_SIGNALS_SRC_PARS).m_mode == MODE_WORK.CUR_INTERVAL))
                    || (Index == FormMain.INDEX_SRC.DEST);
            }
        }

        public abstract List<string> ValuesToFileINI (List<string> listParPrevValues);

        public abstract /*virtual*/ void SetPars (GROUP_SIGNALS_PARS src)
        //{
        //    m_iAutoStart = src.m_iAutoStart == 2
        //        ? m_iAutoStart == 1 ? 0 : 1 // изменить на противоположный
        //            : m_iAutoStart;

        //    src.m_iAutoStart = m_iAutoStart;
        //}
        ;
    }
    
    /// <summary>
    /// Класс для параметров группы сигналов (источник)
    /// </summary>
    public class GROUP_SIGNALS_SRC_PARS : GROUP_SIGNALS_PARS
    {
        private MODE_WORK _mode;
        /// <summary>
        /// Признак текущего режима работы
        /// </summary>
        [State (Changed = true)]
        public MODE_WORK m_mode
        {
            get
            {
                return _mode;
            }

            set
            {
                _mode = value;
            }
        }

        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC_PARS ()
            : base()
        {
            //Режим работы по умолчанию - текущий интервал
            _mode = MODE_WORK.CUR_INTERVAL;

            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL] = new DATETIME_WORK();
            //Дата/время начала опроса (режим: тек./дата/время)
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart = DateTime.Now;
            // округлить по текущей минуте
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.AddMilliseconds(-1 * m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Second * 1000 + m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Millisecond);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain =
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsIntervalCustomize =
                HTimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsRequery = HTimeSpan.FromMilliseconds((int)DATETIME.MSEC_INTERVAL_DEFAULT);
        }

        public override void SetPars (GROUP_SIGNALS_PARS src)
        {
            if (src.Index == FormMain.INDEX_SRC.SOURCE)
            {
                ////??? изменяется 'm_iAutoStart'
                //base.SetPars (src);

                MODE_WORK mode
                    , amode;

                mode =
                m_mode =
                    (src as GROUP_SIGNALS_SRC_PARS).m_mode;
                amode = mode == MODE_WORK.CUR_INTERVAL ? MODE_WORK.CUSTOMIZE
                    : mode == MODE_WORK.CUSTOMIZE ? MODE_WORK.CUR_INTERVAL
                        : MODE_WORK.UNKNOWN;

                m_arWorkIntervals[(int)mode].m_dtStart =
                    src.m_arWorkIntervals[(int)mode].m_dtStart;
                m_arWorkIntervals [(int)mode].m_tsPeriodMain =
                        src.m_arWorkIntervals [(int)mode].m_tsPeriodMain;
                #region Отладка
                if ((m_arWorkIntervals [(int)mode].m_tsPeriodMain - src.m_arWorkIntervals [(int)mode].m_tsPeriodMain).TotalSeconds < 0)
                // устанавливаемое значение всегда равно или больше
                    ASUTP.Logging.Logg ().Warning ($"GROUP_SRC::SetGroupSignalsPars ()- ModeWork={mode}, новое значение для PeriodMain меньше текущего...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                else
                    
                #endregion
                m_arWorkIntervals[(int)mode].m_tsIntervalCustomize =
                    src.m_arWorkIntervals[(int)mode].m_tsIntervalCustomize;

                //if (mode == MODE_WORK.CUR_INTERVAL)
                    m_arWorkIntervals[(int)mode].m_tsRequery =
                        src.m_arWorkIntervals[(int)mode].m_tsRequery;
                //else ;
            }
            else
                ;
        }

        public override List<string> ValuesToFileINI (List<string> listParPrevValues)
        {
            List<string> listRes;
            string value;

            ////Получить ниаменования параметров для групп сигналов
            //List<string> pars = GetSecValueOfKey (SEC_SRC_TYPES [(int)type] + s_chSecDelimeters [(int)INDEX_DELIMETER.SEC_PART_TARGET] + strIdGroup
            //        , KEY_TREE_SGNLS [(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS] + s_chSecDelimeters [(int)INDEX_DELIMETER.SEC_PART_TARGET] + @"PARS").Split (s_chSecDelimeters [(int)INDEX_DELIMETER.PAIR_VAL]).ToList<string> ()
            //    , listParValues = GetSecValueOfKey (SEC_SRC_TYPES [(int)type] + s_chSecDelimeters [(int)INDEX_DELIMETER.SEC_PART_TARGET] + strIdGroup
            //        , KEY_TREE_SGNLS [(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS] + indxGrpSgnls).Split (s_chSecDelimeters [(int)INDEX_DELIMETER.PAIR_VAL]).ToList<string> ();

            listRes = new List<string> (Enum.GetValues (typeof (INDEX_GROUP_SIGNALS_PARAMETER)).Length);

            foreach (INDEX_GROUP_SIGNALS_PARAMETER par in Enum.GetValues (typeof (INDEX_GROUP_SIGNALS_PARAMETER))) {
                value = string.Empty;

                switch (par) {
                    //case INDEX_GROUP_SIGNALS_PARAMETER.ID:
                    //case INDEX_GROUP_SIGNALS_PARAMETER.ID_GS:
                    case 0:
                        if (listRes.Count == 0)
                            value = m_strId;
                        else
                            ;
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.AUTO_START:
                        ////Вариант №1, 2
                        //listParValues[indxPar] = parValues.m_iAutoStart.ToString ();

                        //Вариант №3
                        if (!(m_iAutoStart == 2))
                            //Признак изменения значения
                            value = ((m_iAutoStart == 0) ? 0 : 1).ToString ();
                        else
                            //??? Не изменять
                            value = int.Parse (listParPrevValues [(int)par]) == 0 ? 1.ToString () : 0.ToString ();

                        //Console.WriteLine(@"MainForm.FileINI::makeValueGroupSignalsPars () - iAutoStart=" + listParValues[indxPar] + @"...");
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.TOOLS_ENABLED:
                        //Не устанавливается с помощью GUI
                        value = listParPrevValues [(int)par];
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODMAIN:
                        value = m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain.Text;
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODLOCAL:
                        value = m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_tsRequery.ToString ();
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty (value) == false)
                    listRes.Add (value);
                else
                //??? пустые пропускать
                    ;
            }

            return listRes;
        }
    }

    /// <summary>
    /// Класс для параметров группы сигналов (назначение)
    /// </summary>
    public class GROUP_SIGNALS_DEST_PARS : GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Строковый идентификатор группы источников
        ///  к которой прикреплена группа сигналов-источник
        ///  для текущей группы сигналов
        /// </summary>
        public string m_idGrpSrcs;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_DEST_PARS ()
            : base ()
        {
        }

        public GROUP_SIGNALS_DEST_PARS (string idGrpSrcs)
            : base ()
        {
            m_idGrpSrcs = idGrpSrcs;
        }

        public override void SetPars (GROUP_SIGNALS_PARS src)
        {
            ////??? изменяется 'm_iAutoStart'
            //base.SetPars (src);

            //!!! только единственный режим работы
            m_arWorkIntervals [(int)MODE_WORK.CUSTOMIZE].m_dtStart =
                src.m_arWorkIntervals [(int)MODE_WORK.CUSTOMIZE].m_dtStart;
            m_arWorkIntervals [(int)MODE_WORK.CUSTOMIZE].m_tsPeriodMain =
                src.m_arWorkIntervals [(int)MODE_WORK.CUSTOMIZE].m_tsPeriodMain;
            //m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal =
            //    src.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal;
        }

        public override List<string> ValuesToFileINI (List<string> listParPrevValues)
        {
            List<string> listRes;
            string value;

            ////Получить ниаменования параметров для групп сигналов
            //List<string> pars = GetSecValueOfKey (SEC_SRC_TYPES [(int)type] + s_chSecDelimeters [(int)INDEX_DELIMETER.SEC_PART_TARGET] + strIdGroup
            //        , KEY_TREE_SGNLS [(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS] + s_chSecDelimeters [(int)INDEX_DELIMETER.SEC_PART_TARGET] + @"PARS").Split (s_chSecDelimeters [(int)INDEX_DELIMETER.PAIR_VAL]).ToList<string> ()
            //    , listParValues = GetSecValueOfKey (SEC_SRC_TYPES [(int)type] + s_chSecDelimeters [(int)INDEX_DELIMETER.SEC_PART_TARGET] + strIdGroup
            //        , KEY_TREE_SGNLS [(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS] + indxGrpSgnls).Split (s_chSecDelimeters [(int)INDEX_DELIMETER.PAIR_VAL]).ToList<string> ();

            listRes = new List<string> (Enum.GetValues (typeof (INDEX_GROUP_SIGNALS_PARAMETER)).Length);

            foreach (INDEX_GROUP_SIGNALS_PARAMETER par in Enum.GetValues (typeof (INDEX_GROUP_SIGNALS_PARAMETER))) {
                value = string.Empty;

                switch (par) {
                    //case INDEX_GROUP_SIGNALS_PARAMETER.ID:
                    //case INDEX_GROUP_SIGNALS_PARAMETER.ID_GS:
                    case 0:
                        if (listRes.Count == 0)
                            value = m_strId;
                        else
                            value = m_idGrpSrcs;
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.AUTO_START:
                        ////Вариант №1, 2
                        //listParValues[indxPar] = parValues.m_iAutoStart.ToString ();

                        //Вариант №3
                        if (!(m_iAutoStart == 2))
                        //Признак изменения значения
                            value = ((m_iAutoStart == 0) ? 0 : 1).ToString ();
                        else
                        //??? Не изменять
                            value = int.Parse (listParPrevValues [(int)par + 1]) == 0 ? 1.ToString () : 0.ToString ();

                        //Console.WriteLine(@"MainForm.FileINI::makeValueGroupSignalsPars () - iAutoStart=" + listParValues[indxPar] + @"...");
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.TOOLS_ENABLED:
                        //Не устанавливается с помощью GUI
                        value = listParPrevValues [(int)par + 1];
                        break;
                    case INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODMAIN:
                    case INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODLOCAL:
                    // не требуется
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty (value) == false)
                    listRes.Add (value);
                else
                    ASUTP.Logging.Logg().Exception(new InvalidDataException ("'value' is null or empty")
                        , $"GROUP_SIGNALS_PARS::ValuesToFileINI () - Id={m_strId}, Name={m_strShrName} значение параметра <{par}> не может быть установлено..."
                        , ASUTP.Logging.INDEX_MESSAGE.NOT_SET)
                        ;
            }

            return listRes;
        }
    }

    /// <summary>
    /// Параметры группы сигналов (источник, назначение)
    /// </summary>
    public class GROUP_SIGNALS_SRC : ITEM_SRC
    {
        /// <summary>
        /// Список сигналов в группе
        /// </summary>
        public List<SIGNAL_SRC> m_listSgnls;

        public Dictionary<string, string> m_dictFormula;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC()
        {
        }
        /// <summary>
        /// Добавить сигнал в группу
        /// </summary>
        /// <param name="values">Значения параметров сигнала</param>
        public void Add(string []values)
        {
            //Инициализация, если элемент группы 1-ый
            if (m_listSgnls == null)
                m_listSgnls = new List<SIGNAL_SRC>();
            else
                ;
            // выделить память для сигнала
            m_listSgnls.Add(new SIGNAL_SRC());
            // выделить память для параметров сигнала
            m_listSgnls[m_listSgnls.Count - 1].m_arSPars = new string[values.Length];
            // ??? уточнить не является ли один из параметров формула
            values.CopyTo(m_listSgnls[m_listSgnls.Count - 1].m_arSPars, 0);
        }
        /// <summary>
        /// Добавить описание формулы, используемой в группе
        /// </summary>
        /// <param name="fKey">Ключ формулы</param>
        /// <param name="formula">Содержание формулы</param>
        public void AddFormula(string fKey, string formula)
        {
            if (m_dictFormula == null)
                m_dictFormula = new Dictionary<string, string>();
            else
                ;
            // проверить не была ли формула уже добавлена
            if (m_dictFormula.ContainsKey(fKey) == false)
                m_dictFormula.Add(fKey, formula);
            else
                ;
        }

        public int ReinitArgs(int iSgnl, int iPar)
        {
            int iRes = 0; // признак ошибки (0 - нет ошибок)
            
            // найти в параметре 'item.m_listSgnls[i].m_arSPars[j]' скобки ('(', ')')
            int iStartArgs = m_listSgnls[iSgnl].m_arSPars[iPar].IndexOf('(') + 1;
            if (iStartArgs > 0)
                ;
            else
                ;

            return iRes;
        }
    };

    /// <summary>
    /// Параметры интервала опроса
    /// </summary>
    public class DATETIME_WORK
    {
        /// <summary>
        /// Длительность интервала (секунды)
        /// </summary>
        public HTimeSpan m_tsRequery;
        /// <summary>
        /// Начало интервала
        /// </summary>
        public DateTime m_dtStart;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        /// </summary>
        public HTimeSpan m_tsPeriodMain;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        ///  для "текущ./интервала" равен 'm_tsPeriodMain'
        /// </summary>
        public HTimeSpan m_tsIntervalCustomize;

        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DATETIME_WORK ()
        {
            m_dtStart = new DateTime ();
            m_tsPeriodMain =
            m_tsIntervalCustomize =
                HTimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_tsRequery = HTimeSpan.Zero;
        }
        ///// <summary>
        ///// Установить значения для интервала
        ///// </summary>
        ///// <param name="iInterval">Длительность (секунды)</param>
        ///// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        //public int Set (int iInterval)
        //{
        //    int iRes = 0;

        //    return iRes;
        //}
        ///// <summary>
        ///// Установить значения для интервала
        ///// </summary>
        ///// <param name="iInterval">Длительность (секунды)</param>
        ///// <param name="dtBegin">Начало интервала</param>
        ///// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        //public int Set(int iInterval, DateTime dtBegin)
        //{
        //    int iRes = 0;

        //    return iRes;
        //}
    }

    /// <summary>
    /// Параметры группы источников информации
    /// </summary>
    public class GROUP_SRC : ITEM_SRC
    {
        /// <summary>
        /// Индекс текущего источника данных
        ///  цифровое обозначение (окончание) строкового идентифиатора
        /// </summary>
        public string m_IDCurrentConnSett;
        /// <summary>
        /// Список объектов с параметрами соединения
        /// </summary>
        public Dictionary<string, ConnectionSettings> m_dictConnSett;
        /// <summary>
        /// Наименование библиотеки для работы с группами сигналов
        /// </summary>
        public string m_strDLLName;

        protected int _iIdTypePlugInObjectLoaded;
        /// <summary>
        /// Идентификатор типа объекта плюгИна, используемого для обращения к данным
        /// </summary>
        public int m_iIdTypePlugInObjectLoaded { get { return _iIdTypePlugInObjectLoaded; } } // _plugIn.KeySingleton
        /// <summary>
        /// Список с параметрами "присоединенных" к группе источников групп сигналов
        /// </summary>
        public List <GROUP_SIGNALS_PARS> m_listGroupSignalsPars;
        /// <summary>
        /// Словарь для "дополнительных" параметров
        /// </summary>
        public Dictionary<string, string> m_dictAdding;

        /// <summary>
        /// Установить значения дополнительных параметров для группы источников
        /// </summary>
        /// <param name="vals">Дополнительные параметры</param>
        public void SetAdding(string []vals)
        {
            if (m_dictAdding == null)
                m_dictAdding = new Dictionary<string, string>();
            else
                m_dictAdding.Clear();

            if ((vals.Length > 0)
                && (vals[0].Equals(string.Empty) == false))
                foreach (string pair in vals)
                    m_dictAdding.Add(pair.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[0], pair.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[1]);
            else
                //throw new Exception (@"FileINI::addGroupValues () - ADDING - некорректные разделители...")
                ;
        }

        private static Type getTypeGroupSignals(List<string> pars)
        {
            Type typeRes = Type.Missing as Type;
            if ((!(pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODMAIN.ToString()) < 0))
                && (!(pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODLOCAL.ToString ()) < 0)))
                typeRes = typeof(GROUP_SIGNALS_SRC_PARS);
            else
                if (!(pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.ID_GS.ToString ()) < 0))
                    typeRes = typeof(GROUP_SIGNALS_DEST_PARS);
                else
                    ;

            return typeRes;
        }

        public int SetGroupSignalsPars (List <string> pars, string []vals)
        {
            int iRes = -1;
            //Объект с параметрами группы сигналов
            GROUP_SIGNALS_PARS item;
            //Строковый идентификатор группы сигналов
            string strId = vals[pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.ID.ToString ())];
            //Тип группы сигналов (для источника, для назаначения)
            Type typeGrpSgnls = Type.Missing as Type;

            iRes = getIndexGroupSignalsPars (strId);

            if (iRes < 0)
            {
                typeGrpSgnls = getTypeGroupSignals(pars);
                item = Activator.CreateInstance(typeGrpSgnls) as GROUP_SIGNALS_PARS; //new GROUP_SIGNALS_PARS ();
                m_listGroupSignalsPars.Add(item);
                iRes = m_listGroupSignalsPars.Count - 1;
            }
            else
                item = m_listGroupSignalsPars[iRes];

            item.m_strId = strId; //ID

            //AUTO_START
            iRes = Int32.TryParse (vals [pars.IndexOf (INDEX_GROUP_SIGNALS_PARAMETER.AUTO_START.ToString())], out item.m_iAutoStart) == true ? 0 : -1;
            if (iRes == 0) {
                //TOOLS_ENABLED
                iRes = bool.TryParse(vals[pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.TOOLS_ENABLED.ToString ())], out item.m_bToolsEnabled) == true ? 0 : -1;
                if (iRes == 0) {
                    if (pars.IndexOf (@"TABLE") > 0)
                        item.m_TableName = vals[pars.IndexOf(@"TABLE")];

                    if (item.Index == FormMain.INDEX_SRC.SOURCE)
                    {
                        //CURINTERVAL_PERIODMAIN, CURINTERVAL_PERIODLOCAL
                        item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain =
                        item.m_arWorkIntervals [(int)MODE_WORK.CUSTOMIZE].m_tsPeriodMain =
                        item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsIntervalCustomize =
                        item.m_arWorkIntervals [(int)MODE_WORK.CUSTOMIZE].m_tsIntervalCustomize =
                            new HTimeSpan(vals[pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODMAIN.ToString ())]);
                        item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsRequery =
                            new HTimeSpan(vals[pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.CURINTERVAL_PERIODLOCAL.ToString ())]);
                    }
                    else
                        if (item.Index == FormMain.INDEX_SRC.DEST)
                            (item as GROUP_SIGNALS_DEST_PARS).m_idGrpSrcs = vals[pars.IndexOf(INDEX_GROUP_SIGNALS_PARAMETER.ID_GS.ToString ())];
                        else
                            ;
                } else
                    ;
            } else
                ASUTP.Logging.Logg().Error("GROUP_SRC::SetGroupSignalsPars () - ", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        /// <summary>
        /// Установить новые значения параметров для СУЩЕСТВУЮЩей группы сигналов
        ///  в контексте списка объектов выполнения, возвращает (внутренний) индекс группы сигналов в списке
        /// </summary>
        /// <param name="idGroupSgnls">Идентификатор (строковый) группы сигналов</param>
        /// <param name="pars">Параметры группы сигналов для установки</param>
        /// <returns>Индекс (внутренний) группы сигналов в списке</returns>
        public int SetGroupSignalsPars (/*string idGroupSgnls,*/ GROUP_SIGNALS_PARS pars)
        {
            int iRes = getIndexGroupSignalsPars (pars.m_strId/*idGroupSgnls*/);

            if (iRes < m_listGroupSignalsPars.Count)
                m_listGroupSignalsPars [iRes].SetPars (pars);
            else
                ;

            return iRes;
        }

        public GROUP_SIGNALS_SRC_PARS GetGroupSignalsPars (string id)
        {
            GROUP_SIGNALS_SRC_PARS parsRes;

            parsRes = m_listGroupSignalsPars [getIndexGroupSignalsPars (id)] as GROUP_SIGNALS_SRC_PARS;

            return parsRes;
        }

        /// <summary>
        /// Возвращает (внутренний) индекс группы сигналов в списке
        /// </summary>
        /// <param name="idGroupSgnls">Идентификатор (строковый) группы сигналов</param>
        /// <returns>Индекс (внутренний) группы сигналов в списке</returns>
        private int getIndexGroupSignalsPars (string idGroupSgnls)
        {
            int iRes = -1;

            foreach (GROUP_SIGNALS_PARS par in m_listGroupSignalsPars)
                if (par.m_strId.Equals(idGroupSgnls) == true)
                {
                    iRes = m_listGroupSignalsPars.IndexOf (par);

                    break;
                }
                else
                    ;

            return iRes;
        }
                
        /// <summary>
        /// Конструктор - основной (без парпаметров)
        /// </summary>
        public GROUP_SRC ()
        {
            m_IDCurrentConnSett = string.Empty;
            m_dictConnSett = new Dictionary<string, ConnectionSettings>();
            m_strDLLName = string.Empty;
            m_listGroupSignalsPars = new List <GROUP_SIGNALS_PARS> ();
        }

        /// <summary>
        /// Добавить в группу источник
        /// </summary>
        /// <param name="key">Базовая строка для формирования ключа добавляемого источника</param>
        /// <param name="values">Значения параметров источника (параметры соединения с БД)</param>
        public void Add(string key, string [] values)
        {
            //Инициализация, если элемент группы 1-ый
            if (m_dictConnSett == null)
                m_dictConnSett = new Dictionary<string, ConnectionSettings>();
            else
                ;

            m_dictConnSett.Add(key + m_dictConnSett.Count
                , new ConnectionSettings(
                    Int32.Parse(values[m_listSKeys.IndexOf(@"ID")])
                    , values[m_listSKeys.IndexOf(@"NAME_SHR")]
                    , values[m_listSKeys.IndexOf(@"IP")]
                    , string.Empty // Instanse
                    , Int32.Parse(values[m_listSKeys.IndexOf(@"PORT")])
                    , values[m_listSKeys.IndexOf(@"DB_NAME")]
                    , values[m_listSKeys.IndexOf(@"UID")]
                    , values[m_listSKeys.IndexOf(@"PSWD*")]
                ));
        }
    };

    /// <summary>
    /// Параметры панели (источник, назначение)
    /// </summary>
    public class SRC
    {
        public List<GROUP_SRC> m_listGroupSrc;
        public List<GROUP_SIGNALS_SRC> m_listGroupSgnlsSrc;
    }

    //public class GroupSourcesSrc : GroupSources
    //{
    //    /// <summary>
    //    /// Передать в очередь обработки событий сообщение о необходимости установления/разрыва связи между группами источников
    //    /// </summary>
    //    /// <param name="ev">Аргумент при передаче сообщения</param>
    //    public override void PerformDataAskedHostQueue(EventArgsDataHost ev)
    //    {
    //        //id_main - идентификатор типа объекта в загруженной в ОЗУ библиотеки
    //        //id_detail - команда на изменение состояния группы сигналов
    //        //В 0-ом параметре передан индекс (???идентификатор) группы сигналов
    //        int indxGrpSgnls = (int)(ev.par as object[])[0];
    //        //В 1-ом параметре передан признак инициирования/подтверждения изменения состояния группы сигналов
    //        ID_HEAD_ASKED_HOST idHeadAskedHost = (ID_HEAD_ASKED_HOST)(ev.par as object[])[1];

    //        base.PerformDataAskedHostQueue(new EventArgsDataHost(ev.id_main, ev.id_detail, new object[] { this, indxGrpSgnls, idHeadAskedHost }));
    //    }
    //}
}