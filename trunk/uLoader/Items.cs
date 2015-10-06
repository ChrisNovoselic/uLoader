using System;
using System.Collections.Generic;

using System.Reflection; //Assembly
using System.IO;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
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
    /// Класс для параметров группы сигналов
    /// </summary>
    public class GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Строковый идентификатор группы сигналов
        /// </summary>
        public string m_strId;
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
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_PARS ()
        {
            m_iAutoStart = -1;
            m_bToolsEnabled = false;
            m_arWorkIntervals = new DATETIME_WORK[(int)MODE_WORK.COUNT_MODE_WORK];
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE] = new DATETIME_WORK();

            //Дата/время начала опроса (режим: выборочно)
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart = DateTime.Now;
            // округлить по прошедшему часу
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.AddHours(-1);
            // округлить по 0-ой минуте
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.AddMinutes(-1 * m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.Minute);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.AddMilliseconds(-1 * m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.Second * 1000 + m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.Millisecond);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain = TimeSpan.FromHours(1);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal = TimeSpan.FromMinutes(1);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_iIntervalLocal = (int)DATETIME.MSEC_INTERVAL_DEFAULT;
        }
    }
    /// <summary>
    /// Класс для параметров группы сигналов (источник)
    /// </summary>
    public class GROUP_SIGNALS_SRC_PARS : GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Признак текущего режима работы
        /// </summary>
        public MODE_WORK m_mode;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC_PARS ()
        {
            //Режим работы по умолчанию - текущий интервал
            m_mode = MODE_WORK.CUR_INTERVAL;

            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL] = new DATETIME_WORK();
            //Дата/время начала опроса (режим: тек./дата/время)
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart = DateTime.Now;
            // округлить по текущей минуте
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.AddMilliseconds(-1 * m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Second * 1000 + m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Millisecond);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain =
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodLocal =
                TimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_iIntervalLocal = (int)DATETIME.MSEC_INTERVAL_DEFAULT;
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
        {
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
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC()
        {
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
        public int m_iIntervalLocal;
        /// <summary>
        /// Начало интервала
        /// </summary>
        public DateTime m_dtStart;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        /// </summary>
        public TimeSpan m_tsPeriodMain;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        ///  для "текущ./интервала" равен 'm_tsPeriodMain'
        /// </summary>
        public TimeSpan m_tsPeriodLocal;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DATETIME_WORK ()
        {
            m_iIntervalLocal = -1;
            m_dtStart = new DateTime ();
            m_tsPeriodMain =
            m_tsPeriodLocal =
                TimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
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
            if ((!(pars.IndexOf(@"CUR_INTERVAL_PERIOD") < 0))
                && (!(pars.IndexOf(@"CUR_INTERVAL_VALUE") < 0)))
                typeRes = typeof(GROUP_SIGNALS_SRC_PARS);
            else
                if (!(pars.IndexOf(@"ID_GS") < 0))
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
            string strId = vals[pars.IndexOf(@"ID")];
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

            item.m_iAutoStart = Int32.Parse(vals[pars.IndexOf(@"AUTO_START")]); //AUTO_START
            item.m_bToolsEnabled = bool.Parse(vals[pars.IndexOf(@"TOOLS_ENABLED")]); //TOOLS_ENABLED
            if (item is GROUP_SIGNALS_SRC_PARS)
            {
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain =
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodLocal =
                    TimeSpan.FromSeconds(Int32.Parse(vals[pars.IndexOf(@"CUR_INTERVAL_PERIOD")])); //CUR_INTERVAL_PERIOD                                
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_iIntervalLocal = Int32.Parse(vals[pars.IndexOf(@"CUR_INTERVAL_VALUE")]); //CUR_INTERVAL_VALUE
            }
            else
                if (item is GROUP_SIGNALS_DEST_PARS)
                    (item as GROUP_SIGNALS_DEST_PARS).m_idGrpSrcs = vals[pars.IndexOf(@"ID_GS")];
                else
                    ;

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

            if (pars is GROUP_SIGNALS_SRC_PARS)
            {
                MODE_WORK mode =
                (m_listGroupSignalsPars[iRes] as GROUP_SIGNALS_SRC_PARS).m_mode =
                    (pars as GROUP_SIGNALS_SRC_PARS).m_mode;

                m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_dtStart =
                    pars.m_arWorkIntervals[(int)mode].m_dtStart;
                m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_tsPeriodMain =
                    pars.m_arWorkIntervals[(int)mode].m_tsPeriodMain;
                m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_tsPeriodLocal =
                    pars.m_arWorkIntervals[(int)mode].m_tsPeriodLocal;

                //if (mode == MODE_WORK.CUR_INTERVAL)
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_iIntervalLocal =
                        pars.m_arWorkIntervals[(int)mode].m_iIntervalLocal;
                //else ;
            }
            else
                if (pars is GROUP_SIGNALS_DEST_PARS)
                {
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart =
                        pars.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart;
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain =                    
                        pars.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain;
                    //m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal =
                    //    pars.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal;
                }
                else
                    ;

            return iRes;
        }

        public GROUP_SIGNALS_SRC_PARS GetGroupSignalsPars (string id)
        {
            return m_listGroupSignalsPars[getIndexGroupSignalsPars(id)] as GROUP_SIGNALS_SRC_PARS;
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
    };
    /// <summary>
    /// Параметры панели (источник, назначение)
    /// </summary>
    public class SRC
    {
        public List<GROUP_SRC> m_listGroupSrc;
        public List<GROUP_SIGNALS_SRC> m_listGroupSgnlsSrc;
    }
    ///// <summary>
    ///// Состояние объекта 'GroupSignals'
    ///// </summary>
    //public class StateGroupSignals
    //{
    //    /// <summary>
    //    /// Признак запуска/останова
    //    /// </summary>
    //    public bool m_bStarted;
    //    /// <summary>
    //    /// Признак ативности
    //    /// </summary>
    //    public bool m_bActived;
    //    /// <summary>
    //    /// Конструктор - основной (без параметров)
    //    /// </summary>
    //    public StateGroupSignals()
    //    {
    //        m_bStarted =
    //        m_bActived =
    //            false;
    //    }
    //}
    ///// <summary>
    ///// Состояние объекта 'GroupSources'
    ///// </summary>
    //public class StateGroupSources
    //{
    //    /// <summary>
    //    /// Перечисление - состояния библиотеки
    //    /// </summary>
    //    public enum STATE_DLL { NOT_FOUND = -2, NOT_LOAD, LOADED, UNKNOWN }
    //    /// <summary>
    //    /// Состояние библиотеки
    //    /// </summary>
    //    public STATE_DLL m_StateDLL;
    //    /// <summary>
    //    /// Текущий идентификатор источника
    //    /// </summary>
    //    public string m_strCurrentIDSource;
    //    /// <summary>
    //    /// Список объектов состояния групп сигналов
    //    /// </summary>
    //    public List<StateGroupSignals> m_listStateGroupSignals;
    //    /// <summary>
    //    /// Конструктор - основной (без параметров)
    //    /// </summary>
    //    public StateGroupSources()
    //    {
    //        m_StateDLL = STATE_DLL.UNKNOWN;
    //        m_strCurrentIDSource = string.Empty;

    //        m_listStateGroupSignals = new List<StateGroupSignals>();
    //    }
    //}
    /// <summary>
    /// Группа источников с "прикрепленными" группами сигналов
    /// </summary>
    public class GroupSources : GROUP_SRC, IPlugInHost
    {
        /// <summary>
        /// Перечисление состояний группы источников
        /// </summary>
        public enum STATE { UNKNOWN = -2, REVERSE, UNAVAILABLE, STOPPED, STARTED }
        /// <summary>
        /// Перечисление состояний библиотеки
        /// </summary>
        public enum STATE_DLL { UNKNOWN = -3, NOT_LOAD, TYPE_MISMATCH, LOADED, }

        /// <summary>
        /// Состояние группы источников
        /// </summary>
        public STATE State
        {
            get
            {
                STATE stateRes = STATE.UNKNOWN;

                if (m_listGroupSignals.Count > 0)
                    foreach (GroupSignals grpSgnls in m_listGroupSignals)
                        if (grpSgnls.State == STATE.UNAVAILABLE)
                        {
                            stateRes = STATE.UNAVAILABLE;
                            break;
                        }
                        else
                            ;
                else
                    stateRes = STATE.UNAVAILABLE;

                if (stateRes == STATE.UNKNOWN)
                    //все группы сигналов имеют "известное" состояние
                    if (_iStateDLL == STATE_DLL.LOADED)
                    {
                        stateRes = STATE.STOPPED;

                        foreach (GroupSignals grpSgnls in m_listGroupSignals)
                            if (grpSgnls.State == STATE.STARTED)
                            {
                                stateRes = STATE.STARTED;
                                break;
                            }
                            else
                                ;
                    }
                    else
                        ;
                else
                    ; // хотя бы одна из групп сигналов имеет "неизвестное" состояние

                return stateRes;
            }
        }

        private STATE_DLL _iStateDLL;
        /// <summary>
        /// Состояние группы источников
        /// </summary>
        public STATE_DLL StateDLL
        {
            get { return _iStateDLL; }
        }

        # region ??? Обязательный метод для интерфейса 'IPlugInHost'
        public bool Register (IPlugIn plugIn)
        {
            bool bRes = true;

            return true;
        }
        # endregion

        protected virtual GroupSignals createGroupSignals(GROUP_SIGNALS_SRC grpSgnls)
        {
            return createGroupSignals(typeof(GroupSignals), grpSgnls) as GroupSignals;
        }

        protected object createGroupSignals(Type typeObjGroupSignals, GROUP_SIGNALS_SRC par)
        {
            return Activator.CreateInstance(typeObjGroupSignals, new object [] { par });
        }

        /// <summary>
        /// Группа сигналов (для получения данных)
        /// </summary>
        protected class GroupSignals : GROUP_SIGNALS_SRC
        {
            private STATE _state;
            /// <summary>
            /// Состояние группы сигналов
            /// </summary>
            public STATE State
            {
                get { return _state; }

                set
                {
                    if (value == STATE.STARTED)
                        //Запустить таймер
                        ;
                    else
                        if ((value == STATE.STOPPED)
                            && (_state == STATE.STARTED))
                            //Остановить таймер
                            ;
                        else
                            ;

                    _state = value;
                }
            }
            
            public DataTable m_tableData;

            public int Validated
            {
                get
                {
                    return 0;
                }
            }

            public GroupSignals(GROUP_SIGNALS_SRC srcItem/*, GROUP_SIGNALS_PARS srcPars*/)
                : base()
            {
                //this.m_arWorkIntervals = new DATETIME_WORK[srcItem.m_arWorkIntervals.Length];
                ////??? Значения массивов независимы
                //srcItem.m_arWorkIntervals.CopyTo(this.m_arWorkIntervals, 0);

                //this.m_iAutoStart = srcItem.m_iAutoStart;

                this.m_listSKeys = new List<string> ();
                foreach (string skey in srcItem.m_listSKeys)
                    this.m_listSKeys.Add (skey);

                //??? Значения списка независимы
                this.m_listSgnls = srcItem.m_listSgnls;

                //this.m_mode = srcItem.m_mode;

                this.m_strID = srcItem.m_strID;
                this.m_strShrName = srcItem.m_strShrName;

                _state = STATE.UNAVAILABLE;
            }

            public int StateChange ()
            {
                int iRes = 0;

                State = GroupSources.getNewState (State, out iRes);

                return iRes;
            }

            public int StateChange (STATE newState)
            {
                int iRes = 0;

                if (State == STATE.UNAVAILABLE)
                    iRes = -1;
                else
                    State = newState;

                return iRes;
            }

            public object [] Pack ()
            {
                object[] arObjRes = null;

                if (m_listSgnls == null)
                {
                    arObjRes = new object[] { };

                    Logging.Logg().Warning(@"GroupSignals::Pack (id=" + m_strID + @") - список сигналов == null...", Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    arObjRes = new object[m_listSgnls.Count];

                int i = -1
                    , j = -1
                    , iVal = -1;

                for (i = 0; i < arObjRes.Length; i ++)
                {
                    arObjRes[i] = new object[m_listSKeys.Count];

                    for (j = 0; j < m_listSKeys.Count; j++)
                        if (Int32.TryParse(m_listSgnls[i].m_arSPars[j].Trim(), out iVal) == true)
                            (arObjRes[i] as object [])[j] = iVal;
                        else
                            (arObjRes[i] as object[])[j] = m_listSgnls[i].m_arSPars[j];
                }                

                return arObjRes;
            }
        }
        /// <summary>
        /// Вспомогательный домен приложения для загрузки/выгрузки библиотеки
        ///  в режиме реального времени
        /// </summary>
        AppDomain m_appDomain;
        /// <summary>
        /// Список групп сигналов, принадлежащих группе источников
        /// </summary>
        protected List <GroupSignals> m_listGroupSignals;
        /// <summary>
        /// Возвратить массив состояний групп сигналов для группы источников
        /// </summary>
        public object[] GetArgGroupSignals()
        {
            object[] arRes = new object[m_listGroupSignals.Count];

            int i = 0;
            foreach (GroupSignals grpSgnls in m_listGroupSignals)
            {
                arRes[i] = new object[] { grpSgnls.State, m_listGroupSignalsPars[i].m_bToolsEnabled, m_listGroupSignalsPars[i].m_iAutoStart };

                i ++;
            }

            return arRes;
        }
        /// <summary>
        /// Объект с загруженной библиотекой
        /// </summary>
        private IPlugIn m_plugIn;
        /// <summary>
        /// Событие для обмена данными с библиотекой
        /// </summary>
        private event DelegateObjectFunc EvtDataAskedHostPlugIn;        
        /// <summary>
        /// Коструктор (с параметрами)
        /// </summary>
        /// <param name="srcItem">Объект с информациеий о группе источников</param>
        /// <param name="listGroupSignals">Список объектов с информацией о группах сигналов</param>
        public GroupSources (GROUP_SRC srcItem, List <GROUP_SIGNALS_SRC>listGroupSignals) : base ()
        {
            this.m_listGroupSignalsPars = new List <GROUP_SIGNALS_PARS> ();
            foreach (GROUP_SIGNALS_PARS pars in srcItem.m_listGroupSignalsPars)
                this.m_listGroupSignalsPars.Add (pars);

            m_listGroupSignals = new List<GroupSignals>();
            foreach (GROUP_SIGNALS_SRC itemGroupSignals in listGroupSignals)
                m_listGroupSignals.Add(createGroupSignals(itemGroupSignals));

            this.m_listSKeys = new List<string> ();
            foreach (string skey in srcItem.m_listSKeys)
                this.m_listSKeys.Add (skey);

            //Список с объектми параметров соединения с источниками данных
            foreach (KeyValuePair <string, ConnectionSettings> pair in srcItem.m_dictConnSett)
                //??? Значения списка независимы
                this.m_dictConnSett.Add (pair.Key, pair.Value);

            //Строковый идентификтор группы
            this.m_strID = srcItem.m_strID;
            this.m_strShrName = srcItem.m_strShrName;

            //Идентификатор текущего источника данных
            this.m_IDCurrentConnSett = srcItem.m_IDCurrentConnSett;

            //Наименование библиотеки
            this.m_strDLLName = srcItem.m_strDLLName;

            //Дополнительные параметры
            this.m_dictAdding = new Dictionary<string,string> ();
            if ((!(srcItem.m_dictAdding == null))
                && (srcItem.m_dictAdding.Count > 0))
                foreach (KeyValuePair <string, string> pair in srcItem.m_dictAdding)
                    this.m_dictAdding.Add (pair.Key, pair.Value);
            else
                ;

            m_plugIn = loadPlugIn(out _iStateDLL);
            if (_iStateDLL == STATE_DLL.LOADED)
            {
                int idGrpSgnls = -1;
                GROUP_SIGNALS_PARS grpSgnlsPars;
                foreach (GroupSignals itemGroupSignals in m_listGroupSignals)
                {
                    //if (sendInitGroupSignals(FormMain.FileINI.GetIDIndex(itemGroupSignals.m_strID)) == 0)
                    if (itemGroupSignals.Validated == 0)
                    {
                        itemGroupSignals.State = STATE.STOPPED;

                        idGrpSgnls = FormMain.FileINI.GetIDIndex(itemGroupSignals.m_strID);

                        grpSgnlsPars = getGroupSignalsPars(idGrpSgnls);
                        if (grpSgnlsPars.m_iAutoStart == 1)
                        {
                            sendInitGroupSignals(idGrpSgnls);
                            sendState(idGrpSgnls, STATE.STARTED);
                        }
                        else
                            ;
                    }
                    else
                        itemGroupSignals.State = STATE.UNAVAILABLE;
                }
            }
            else
                //throw new Exception(@"GroupSources::GroupSources () - ...")
                ;

            
        }
        /// <summary>
        /// Загрузить библиотеку с именем 'm_strDLLName'
        /// </summary>
        /// <param name="iRes">Признак выполнения загрузки библиотеки</param>
        /// <returns>Объект с загруженной библиотекой</returns>
        private IPlugIn loadPlugIn(out STATE_DLL iRes)
        //private IPlugIn loadPlugIn(string name, out int iRes)
        {
            IPlugIn plugInRes = null;
            iRes = STATE_DLL.UNKNOWN;

            string name =
                Path.GetFileNameWithoutExtension (this.m_strDLLName)
                //"biysktmora"
                ;
            Type objType = null;
            try
            {
                Assembly ass = null;
                //Вариант №1
                ass = Assembly.LoadFrom(Environment.CurrentDirectory + @"\" + name + @".dll");
                //ass = Assembly.LoadFrom(Environment.CurrentDirectory + @"\" + name);
                //Вариант №2
                //m_appDomain = AppDomain.CreateDomain(@"appDomain_" + m_strID);
                //ass = m_appDomain.Load(Environment.CurrentDirectory + @"\" + name + @".dll");
                if (!(ass == null))
                {
                    objType = ass.GetType(name + ".PlugIn");
                }
                else
                    ;
            }
            catch (Exception e)
            {
                iRes = STATE_DLL.NOT_LOAD;                
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"GroupSources::loadPlugin () ... LoadFrom () ... plugIn.Name = " + name);
            }

            if (!(objType == null))
                try
                {
                    plugInRes = ((IPlugIn)Activator.CreateInstance(objType));
                    plugInRes.Host = (IPlugInHost)this;
                    //Взаимная "привязка" для обмена сообщениями
                    // библиотека - объект класса
                    (plugInRes as PlugInBase).EvtDataAskedHost += new DelegateObjectFunc(plugIn_OnEvtDataAskedHost);
                    // объект класса - библиотека
                    EvtDataAskedHostPlugIn += (plugInRes as PlugInBase).OnEvtDataRecievedHost;

                    iRes = STATE_DLL.LOADED;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"GroupSources::loadPlugin () ... CreateInstance ... plugIn.Name = " + name);
                }
            else
            {
                iRes = STATE_DLL.TYPE_MISMATCH;
                Logging.Logg().Error(@"GroupSources::loadPlugin () ... Assembly.GetType()=null ... plugIn.Name = " + name, Logging.INDEX_MESSAGE.NOT_SET);
            }

            return plugInRes;
        }

        private object [] Pack ()
        {
            object[] arObjRes = new object[1 + m_dictAdding.Count];

            arObjRes [0] = this.m_dictConnSett[m_IDCurrentConnSett];

            string adding = string.Empty;
            int i = 1;
            foreach (KeyValuePair <string, string> pair in m_dictAdding)
                arObjRes[i++] = pair.Key + FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE] + pair.Value;

            return arObjRes;
        }

        private int sendInitSource ()
        {
            int iRes = 0;

            PerformDataAskedHostPlugIn(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.INIT_SOURCE, Pack ()));

            return iRes;
        }
        /// <summary>
        /// Передать сообщение библиотеке
        /// </summary>
        /// <param name="ev">Аргумент при передаче сообщения</param>
        protected void PerformDataAskedHostPlugIn (EventArgsDataHost ev)
        {
            EvtDataAskedHostPlugIn (ev);
        }

        private int sendInitGroupSignals (int iIDGroupSignals)
        {
            int iRes = 0;
            GroupSignals grpSgnls = getGroupSignals(iIDGroupSignals);
            object[] arToDataHost = new object[] { };

            if (!(grpSgnls == null))
            {
                arToDataHost = grpSgnls.Pack();

                if (!(arToDataHost.Length > 0))
                    iRes = -2;
                else
                    ;
            }
            else
            {
                iRes = -1;

                // не найдена группа сигналов
                Logging.Logg().Warning(@"GroupSources::sendInitGroupSignals (iIDGroupSignals=)" + iIDGroupSignals + @" - не найдена группа сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            //Отправить данные для инициализации
            PerformDataAskedHostPlugIn(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.INIT_SIGNALS, new object[] { iIDGroupSignals, arToDataHost }));

            return iRes;
        }

        private int sendState(int iIDGroupSignals, STATE state)
        {
            int iRes = 0;
            ID_DATA_ASKED_HOST idToSend = ID_DATA_ASKED_HOST.UNKNOWN;

            switch (state)
            {
                case STATE.STARTED:
                    idToSend = ID_DATA_ASKED_HOST.START;
                    break;
                case STATE.STOPPED:
                    idToSend = ID_DATA_ASKED_HOST.STOP;
                    break;
                default:
                    break;
            }

            //GroupSignals grpSgnls = getGroupSignals (iIDGroupSignals);
            GROUP_SIGNALS_PARS grpSgnlsPars = getGroupSignalsPars (iIDGroupSignals);
            object []arDataAskedHost = null;

            if (idToSend == ID_DATA_ASKED_HOST.START)
                //Проверить признак группы сигналов: источник или назначение
                //if (! (grpSgnlsPars.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL] == null))
                if (grpSgnlsPars is GROUP_SIGNALS_SRC_PARS)
                {
                    MODE_WORK mode = (grpSgnlsPars as GROUP_SIGNALS_SRC_PARS).m_mode;
                    
                    arDataAskedHost = new object[]
                        {
                            iIDGroupSignals
                            , new object[]
                            {
                                mode
                                , grpSgnlsPars.m_arWorkIntervals[(int)mode].m_dtStart
                                , TimeSpan.FromSeconds(grpSgnlsPars.m_arWorkIntervals [(int)mode].m_tsPeriodMain.TotalSeconds)
                                , TimeSpan.FromSeconds(grpSgnlsPars.m_arWorkIntervals [(int)mode].m_tsPeriodLocal.TotalSeconds)
                                , grpSgnlsPars.m_arWorkIntervals [(int)mode].m_iIntervalLocal
                            }
                        };
                }
                else
                    if (grpSgnlsPars is GROUP_SIGNALS_DEST_PARS)
                        arDataAskedHost = new object[]
                            {
                                iIDGroupSignals
                                , new object[]
                                {
                                    MODE_WORK.ON_REQUEST
                                    , DateTime.MinValue
                                    , TimeSpan.Zero
                                    , TimeSpan.Zero
                                    , -1
                                }
                            };
                    else
                        ;
            else
                if (idToSend == ID_DATA_ASKED_HOST.STOP)
                    arDataAskedHost = new object[]
                        {
                            iIDGroupSignals
                        };
                else
                    ; //

            PerformDataAskedHostPlugIn(new EventArgsDataHost((int)idToSend, arDataAskedHost));

            return iRes;
        }

        /// <summary>
        /// Обработка сообщений "от" библиотеки
        /// </summary>
        /// <param name="obj"></param>
        private void plugIn_OnEvtDataAskedHost  (object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            GroupSignals grpSgnls = null;
            int iIDGroupSignals = -1; //??? д.б. указана в "запросе"
            object []pars = null;
            
            try
            {
                pars = (ev.par as object[])[0] as object [];

                iIDGroupSignals = (int)pars[1];
                grpSgnls = getGroupSignals(iIDGroupSignals);

                string msgDebugLog = string.Empty;

                switch ((ID_DATA_ASKED_HOST)pars[0])
                {
                    case ID_DATA_ASKED_HOST.INIT_SOURCE: //Получен запрос на парметры инициализации                    
                        //Отправить данные для инициализации
                        sendInitSource ();
                        if (!(grpSgnls.State == STATE.UNAVAILABLE))
                            //sendState(iIDGroupSignals, m_listGroupSignals[iIDGroupSignals].State);
                            sendState(iIDGroupSignals, STATE.STARTED);
                        else
                            ;

                        msgDebugLog = @"отправлен: " + ((ID_DATA_ASKED_HOST)pars[0]).ToString ();
                        break;
                    case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Получен запрос на обрабатываемую группу сигналов
                        sendInitGroupSignals(iIDGroupSignals);
                        if (!(grpSgnls.State == STATE.UNAVAILABLE))
                            //sendState(iIDGroupSignals, m_listGroupSignals[iIDGroupSignals].State);
                            sendState(iIDGroupSignals, STATE.STARTED);
                        else
                            ;

                        msgDebugLog = @"отправлен: " + ((ID_DATA_ASKED_HOST)pars[0]).ToString();
                        break;
                    case ID_DATA_ASKED_HOST.TABLE_RES:
                        if ((!(grpSgnls == null))
                            && (!(pars[2] == null)))
                        {
                            msgDebugLog = @"получено строк=" + (pars[2] as DataTable).Rows.Count;

                            grpSgnls.m_tableData = (pars[2] as DataTable).Copy();
                        
                        }
                        else
                            ;
                        break;
                    case ID_DATA_ASKED_HOST.START:
                    case ID_DATA_ASKED_HOST.STOP:
                        //Вариант №2 (пост-установка)
                        grpSgnls.StateChange();
                        //Установить/разорвать взаимосвязь между группами источников (при необходимости)
                        if (this is GroupSourcesDest)
                            (this as GroupSourcesDest).PerformDataAskedHostQueue(new EventArgsDataHost((int)pars[0], new object[] { iIDGroupSignals }));
                        else
                            ;

                        msgDebugLog = @"получено подтверждение: " + ((ID_DATA_ASKED_HOST)pars[0]).ToString();
                        break;
                    case ID_DATA_ASKED_HOST.ERROR:
                        //???
                        msgDebugLog = @"получена ошибка: ???";
                        break;
                    default:
                        break;
                }

                Logging.Logg().Debug(@"GroupSources::plugIn_OnEvtDataAskedHost (id=" + m_strID + @", key=" + grpSgnls.m_strID + @") - " + msgDebugLog + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            catch (Exception e)
            {
                Logging.Logg ().Exception (e, Logging.INDEX_MESSAGE.NOT_SET, @"GroupSources::plugIn_OnEvtDataAskedHost (id=" + m_strID + @", key=" + grpSgnls.m_strID + @") - ...");
            }
        }
        /// <summary>
        /// Возвратить параметры группы сигналов по целочисленному идентификатору
        /// </summary>
        /// <param name="id">Целочисленный идентификатор (целочисленное окончание строкового идентификатора)</param>
        /// <returns>Объект параметров группы сигналов</returns>
        protected GROUP_SIGNALS_PARS getGroupSignalsPars(int id)
        {
            GROUP_SIGNALS_PARS grpSgnlsRes = null;

            foreach (GROUP_SIGNALS_PARS grpSgnlsPars in m_listGroupSignalsPars)
                if (uLoader.FormMain.FileINI.GetIDIndex(grpSgnlsPars.m_strId) == id)
                {
                    grpSgnlsRes = grpSgnlsPars;

                    break;
                }
                else
                    ;

            return grpSgnlsRes;
        }
        /// <summary>
        /// Возвратить группу сигналов по целочисленному идентификатору
        /// </summary>
        /// <param name="id">Целочисленный идентификатор (целочисленное окончание строкового идентификатора)</param>
        /// <returns>Объект группы сигналов</returns>
        protected GroupSignals getGroupSignals (int id)
        {
            GroupSignals grpRes = null;

            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                if (uLoader.FormMain.FileINI.GetIDIndex(grpSgnls.m_strID) == id)
                {
                    grpRes = grpSgnls;

                    break;
                }
                else
                    ;

            return grpRes;
        }
        /// <summary>
        /// Возвратить "новое" состояние в ~ предыдущего состояния
        /// </summary>
        /// <param name="prevState">Предыдущее состояние</param>
        /// <param name="iRes">Результат выполнения функции</param>
        /// <returns>"Новое" состояние</returns>
        private static STATE getNewState (STATE prevState, out int iRes)
        {
            STATE stateRes = STATE.UNAVAILABLE;
            iRes = 0;

            if (prevState == STATE.UNAVAILABLE)
                iRes = -1;
            else
            {
                if (prevState == STATE.STARTED)
                    stateRes = STATE.STOPPED;
                else
                    if (prevState == STATE.STOPPED)
                        stateRes = STATE.STARTED;
                    else
                        throw new Exception(@"GroupSources::getNewState () - неизвестное состояние...");
            }

            return stateRes;
        }
        /// <summary>
        /// Остановить/запустить все группы сигналов
        /// </summary>
        /// <returns></returns>
        public int StateChange()
        {
            int iRes = 0
                , iId = -1;

            STATE newState = getNewState (State, out iRes);

            //Изменить состояние ВСЕХ групп сигналов
            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                //Проверить текцщее стостояние
                if (!(grpSgnls.State == newState))
                {
                    iId = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);

                    ////Вариант №1 (пред-установка)
                    ////Изменить только, если "другое"
                    //grpSgnls.StateChange(newState);
                    //sendState(FormMain.FileINI.GetIDIndex(grpSgnls.m_strID), grpSgnls.State);

                    //Вариант №2  (пост-установка)
                    if (newState == STATE.STARTED)
                        sendInitGroupSignals(iId);
                    else
                        ;
                    sendState(iId, newState);
                }
                else
                    ;

            return iRes;
        }
        /// <summary>
        /// Остановить/запустить группу сигналов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int StateChange (string strId, STATE prevState = STATE.REVERSE)
        {
            int iRes = 0
                , iId = FormMain.FileINI.GetIDIndex (strId);

            //Изменить состояние только ОДНой группы сигналов
            GroupSignals grpSgnls = getGroupSignals(iId);
            STATE newState = getNewState(grpSgnls.State, out iRes);

            if ((!(grpSgnls == null))
                && (iRes == 0))
            {
                if ((grpSgnls.State == STATE.STOPPED) 
                    && (State == STATE.STOPPED))
                    sendInitSource ();
                else
                    if ((grpSgnls.State == STATE.STARTED)
                        &&(!(State == STATE.STARTED)))
                        throw new Exception(@"GroupSources::StateChange (ID=" + strId + @") - несовместимые состояния групп источников и сигналов...");
                    else
                        ;

                ////Вариант №1 (пред-установка)
                //iRes = grpSgnls.StateChange();
                //sendState(id, grpSgnls.State);

                //Вариант №2 (пост-установка)
                if (newState == STATE.STARTED)
                    sendInitGroupSignals(iId);
                else
                    ;
                sendState(iId, newState);
            }
            else
            {
                iRes = -1;

                // не найдена группа сигналов
                Logging.Logg().Error(@"GroupSources::StateChange (id=" + iId + @") - не найдена группа сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }
        /// <summary>
        /// Получить данные (результаты запроса в ~ режима) 'DataTable' по указанной группе сигналов
        /// </summary>
        /// <param name="id">Строковый идентификатор группы сигналов</param>
        /// <param name="bErr">Признак ошибки при выполнении функции</param>
        /// <returns>Таблица-результат</returns>
        public DataTable GetDataToPanel(string id, out bool bErr)
        {
            int iID = -1;            
            bErr = false;

            GroupSignals grpSgnls;
            DataTable tblRec = new DataTable()
                , tblToPanel = null;
            DataRow[]arSel;
            object [] arObjToRow = null;
            string strNameFieldID = string.Empty;

            //Проверить длину идентификатора
            if (id.Length > 1)
            {//Только при длине > 1 м. определить целочисленное значение идентификатора
                iID = uLoader.FormMain.FileINI.GetIDIndex(id);
                grpSgnls = getGroupSignals(iID);
                if (!(grpSgnls == null))
                {
                    tblRec = grpSgnls.m_tableData;

                    if ((!(tblRec == null))
                        && (tblRec.Rows.Count > 0))
                    {
                        if ((!(tblRec.Columns.IndexOf(@"ID") < 0)) && (!(tblRec.Columns.IndexOf(@"DATETIME") < 0)))
                        {
                            //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - получено строк=" + tblRec.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                            
                            tblToPanel = new DataTable();
                            tblToPanel.Columns.AddRange(new DataColumn[] { new DataColumn (@"NAME_SHR", typeof (string))
                                                                            , new DataColumn (@"VALUE", typeof (string)) //new DataColumn (@"VALUE", typeof (decimal))
                                                                            , new DataColumn (@"DATETIME", typeof (string)) //new DataColumn (@"DATETIME", typeof (DateTime))
                                                                            , new DataColumn (@"COUNT", typeof (string)) //new DataColumn (@"COUNT", typeof (int))
                                                                            });

                            //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - добавлен диапазон столбцов...", Logging.INDEX_MESSAGE.NOT_SET);

                            if (this.GetType().Equals(typeof(GroupSources)) == true)
                                strNameFieldID = @"ID";
                            else
                                if (this.GetType().Equals(typeof(GroupSourcesDest)) == true)
                                    strNameFieldID = @"ID_SRC_SGNL";
                                else
                                    ;

                            //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - определено наименование поля идентификатора...", Logging.INDEX_MESSAGE.NOT_SET);

                            foreach (SIGNAL_SRC sgnl in grpSgnls.m_listSgnls)
                            {
                                arSel = tblRec.Select(@"ID=" + sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf (strNameFieldID)], @"DATETIME DESC");
                                if (arSel.Length > 0)
                                    arObjToRow = new object[] { sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf (@"NAME_SHR")], arSel[0][@"VALUE"], ((DateTime)arSel[0][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"), arSel.Length };
                                else
                                    arObjToRow = new object[] { sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf (@"NAME_SHR")], string.Empty, string.Empty, string.Empty };

                                tblToPanel.Rows.Add(arObjToRow);
                            }

                            //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - строк для отображения=" + tblToPanel.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                            throw new Exception(@"uLoader::GroupSources::GetDataToPanel () - ...");
                    }
                    else
                        //tblRec == null или tblRec.Rows.Count == 0
                        Logging.Logg().Warning(@"GroupSources::GetDataToPanel (id=" + id + @") - таблица 'recieved' не валидна...", Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    //grpSgnls == null
                    Logging.Logg().Warning(@"GroupSources::GetDataToPanel (id=" + id + @") - не найдена группа сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            {//Нельзя определить целочисленный идентификатор - возвратить пустую таблицу
                //bErr = true;
            }

            return tblToPanel;
        }
        /// <summary>
        /// Остановить группу источников
        /// </summary>
        /// <returns></returns>
        public int Stop()
        {
            int iRes = 0;
            
            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                if (grpSgnls.State == STATE.STARTED)
                    sendState(FormMain.FileINI.GetIDIndex(grpSgnls.m_strID), STATE.STOPPED);
                else
                    ;

            return iRes;
        }
        /// <summary>
        /// Добавить обработчик событий от присоединенной библиотеки
        /// </summary>
        /// <param name="indxGrpSrcDest">Индекс группы источников (газначения) - не используется</param>
        /// <param name="fOnEvt">Функция обработки</param>
        public void AddDelegatePlugInOnEvtDataAskedHost (int indxGrpSrcDest, DelegateObjectFunc fOnEvt)
        {
            (m_plugIn as PlugInBase).EvtDataAskedHost += fOnEvt;
        }
        /// <summary>
        /// Удалить обработчик событий от присоединенной библиотеки
        /// </summary>
        /// <param name="indxGrpSrcDest">Индекс группы источников (газначения) - не используется</param>
        /// <param name="fOnEvt">Функция обработки</param>
        public void RemoveDelegatePlugInOnEvtDataAskedHost(int indxGrpSrcDest, DelegateObjectFunc fOnEvt)
        {
            (m_plugIn as PlugInBase).EvtDataAskedHost -= fOnEvt;
        }
        /// <summary>
        /// Получить признак наличия группы сигналов среди списка присоединенных групп сигналов
        /// </summary>
        /// <param name="indx">Индекс (целочисленный идентификатор) группв сигналов</param>
        /// <returns>Признак наличия идентификатора группы сигналов</returns>
        public int ContainsIndexGroupSignals (int indx)
        {
            return getGroupSignals(indx) == null ? -1 : 0;
        }
    }

    public class GroupSourcesDest : GroupSources
    {
        /// <summary>
        /// Словарь с ключами - индексами групп источников
        ///  значениями - списками индексов групп сигналов
        ///  , подписчиками на таблицы результатов
        /// </summary>
        Dictionary<int, List<int>> m_dictLinkedIndexGroupSources;
        /// <summary>
        /// Событие для обмена данными с очередью обработки событий
        /// </summary>
        public event DelegateObjectFunc EvtDataAskedHostQueue;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="grpSrc">Объект группы источников из файла конфигурации</param>
        /// <param name="listGrpSgnls">Список объектов групп сигналов из файла конфигурации</param>
        public GroupSourcesDest(GROUP_SRC grpSrc, List<GROUP_SIGNALS_SRC> listGrpSgnls)
            : base(grpSrc, listGrpSgnls)
        {
            m_dictLinkedIndexGroupSources = new Dictionary<int, List<int>>();

            List<int> listNeededIndexGroupSources = GetListNeededIndexGroupSources();
            foreach (int indx in listNeededIndexGroupSources)
                m_dictLinkedIndexGroupSources.Add(indx, new List<int>());
        }
        /// <summary>
        /// Класс для описания группы сигналов назначения
        /// </summary>
        protected class GroupSignalsDest : GroupSignals
        {
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="grpSgnls">Объект группы сигналов из файла конфигурации</param>
            public GroupSignalsDest (GROUP_SIGNALS_SRC grpSgnls) : base (grpSgnls)
            {
            }
            ///// <summary>
            ///// Получить список индексов групп источников
            /////  , являющимися источниками значений для группы сигналов
            ///// </summary>
            ///// <returns></returns>
            //public List<int> GetListNeededIndexGroupSources()
            //{
            //    List<int> listRes = new List<int>();

            //    int indxGroupSrc = -1
            //        , iPosGroupSrc = m_listSKeys.IndexOf (@"ID_GROUP_SOURCES");
            //    //Поиск индексов групп источников
            //    foreach (SIGNAL_SRC sgnl in m_listSgnls)
            //    {
            //        //Получить индекс группы источников
            //        indxGroupSrc = FormMain.FileINI.GetIDIndex(sgnl.m_arSPars[iPosGroupSrc]);

            //        if (listRes.IndexOf (indxGroupSrc) < 0)
            //            listRes.Add(indxGroupSrc);
            //        else
            //            ;
            //    }

            //    return listRes;
            //}

            /// <summary>
            /// Получить список индексов групп сигналов, использующихся в качестве источников
            /// </summary>
            /// <returns>Список индексов групп сигналов</returns>
            public List<int> GetListNeededIndexGroupSignals()
            {
                List<int> listRes = new List<int>();

                int iIdGrpSgnls = -1;

                foreach (SIGNAL_SRC sgnl in m_listSgnls)
                {
                    iIdGrpSgnls = Convert.ToInt16(sgnl.m_arSPars[m_listSKeys.IndexOf(@"ID_SRC_SGNL")].Substring(1, 2)) - 1;
                    if (listRes.IndexOf(iIdGrpSgnls) < 0)
                        listRes.Add(iIdGrpSgnls);
                    else
                        ;
                }

                return listRes;
            }
        }
        /// <summary>
        /// Создать группу сигналов
        /// </summary>
        /// <param name="grpSgnls">Объект группы сигналов из файла конфигурации</param>
        /// <returns>Объект группы сигналов</returns>
        protected override GroupSignals createGroupSignals(GROUP_SIGNALS_SRC grpSgnls)
        {
            GroupSignals grpSgnlsRes = createGroupSignals(typeof(GroupSignalsDest), grpSgnls) as GroupSignals;

            return grpSgnlsRes;
        }

        /// <summary>
        /// Получить список индексов групп сигналов
        /// </summary>
        /// <returns>Список индексов ожидаемых групп сигналов</returns>
        public List<int> GetListNeededIndexGroupSignals()
        {
            List<int> listRes = new List<int>()
                , listGrpSgnls = new List<int>();

            foreach (GroupSourcesDest.GroupSignalsDest grpSgnls in m_listGroupSignals)
            {
                ////Вариант №1
                //listRes.Union(grpSgnls.GetListNeededIndexGroupSignals());
                //Вариант №2
                listGrpSgnls = grpSgnls.GetListNeededIndexGroupSignals();
                foreach (int id in listGrpSgnls)
                    if (listRes.IndexOf(id) < 0)
                        listRes.Add(id);
                    else
                        ;
            }

            return listRes;
        }
        /// <summary>
        /// Получить список индексов групп источников
        ///  , являющимися источниками значений для указанной группы сигналов (назначения)
        /// </summary>
        /// <param name="id">Целочисленный идентификатор (индекс) группы сигналов</param>
        /// <returns>Список индексов групп источников</returns>
        public List<int> GetListNeededIndexGroupSources(int id)
        {
            List<int> listRes = new List<int>();
            GroupSourcesDest.GroupSignalsDest grpSgnls = getGroupSignals(id) as GroupSignalsDest;
            GROUP_SIGNALS_DEST_PARS grpSgnlsPars = getGroupSignalsPars(id) as GROUP_SIGNALS_DEST_PARS;

            listRes = new List <int> () { FormMain.FileINI.GetIDIndex (grpSgnlsPars.m_idGrpSrcs) };

            return listRes;
        }
        /// <summary>
        /// Получить список индексов групп источников
        ///  , являющимися источниками значений для группы источников (назначения)
        /// </summary>
        /// <returns>Список индексов групп источников</returns>
        public List<int> GetListNeededIndexGroupSources()
        {
            List<int> listRes = new List<int>()
                , listGrpSrcs = new List<int>();

            foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
            {
                ////Вариант №1
                //listRes.Union(grpSgnls.GetListNeededIndexGroupSignals());
                //Вариант №2
                listGrpSrcs = GetListNeededIndexGroupSources(FormMain.FileINI.GetIDIndex (grpSgnls.m_strID));
                foreach (int id in listGrpSrcs)
                    if (listRes.IndexOf(id) < 0)
                        listRes.Add(id);
                    else
                        ;
            }

            return listRes;
        }

        /// <summary>
        /// Получает сообщения от библиотеки из "другого" (источника) объекта
        /// </summary>
        /// <param name="obj"></param>
        public void Clone_OnEvtDataAskedHost(object obj)
        {
            //Logging.Logg().Debug(@"GroupSources::Clone_OnEvtDataAskedHost () - NAME=" + m_strShrName + @"...", Logging.INDEX_MESSAGE.NOT_SET);

            EventArgsDataHost ev = obj as EventArgsDataHost;
            int iIDGroupSignals = 0; //??? д.б. указана в "запросе"
            //pars[0] - идентификатор события
            //pars[1] - идентификатор группы сигналов
            //pars[2] - таблица с данными для "вставки"
            //??? pars[3] - object [] с доп./параметрами, для ретрансляции
            object[] pars = (ev.par as object[])[0] as object[];

            //pars[0] - идентификатор события
            switch ((ID_DATA_ASKED_HOST)pars[0])
            {
                case ID_DATA_ASKED_HOST.INIT_SOURCE: //Получен запрос на парметры инициализации
                    break;
                case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Получен запрос на обрабатываемую группу сигналов
                    break;
                case ID_DATA_ASKED_HOST.TABLE_RES:
                    object[] parsToSend = new object[pars.Length - 1];
                    //Заполнить для передачи основные параметры - таблицу
                    parsToSend[1] = (pars[2] as DataTable).Copy();
                    //Проверить наличие дополнительныз параметров
                    if ((parsToSend.Length > 2)
                        && (pars.Length > 3))
                    {
                        parsToSend[2] = new object[(pars[3] as object[]).Length];
                        //Заполнить для передачи дополнительные параметры - массив объектов
                        ////Вариант №1
                        //for (int i = 0; i < (parsToSend[2] as object []).Length; i ++)
                        //    (parsToSend[2] as object [])[i] = (pars[3] as object[])[i];
                        //Вариант №2
                        (pars[3] as object[]).CopyTo (parsToSend[2] as object [], 0);
                    }
                    else
                        ;
                    //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                    foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                        if (!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0))
                        {//Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]'
                            parsToSend [0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                            PerformDataAskedHostPlugIn(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.TO_INSERT, parsToSend));

                            //Logging.Logg().Debug(@"GroupSources::Clone_OnEvtDataAskedHost () - NAME=" + m_strShrName + @", от [ID=" + (int)pars[1] + @"] для [ID=" + parsToSend[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                            ;
                    break;
                case ID_DATA_ASKED_HOST.ERROR:
                    iIDGroupSignals = (int)pars[1];
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Передать в очередь обработки событий сообщение о необходимости установления/разрыва связи между группами источников
        /// </summary>
        /// <param name="ev">Аргумент при передаче сообщения</param>
        public void PerformDataAskedHostQueue (EventArgsDataHost ev)
        {
            //В 0-ом параметре пердан индекс (???идентификатор) группы сигналов
            int indxGrpSgnls = (int)(ev.par as object[])[0];

            List<int> listNeededIndexGroupSources = GetListNeededIndexGroupSources(indxGrpSgnls);
            bool bEvtDataAskedHostQueue = false;

            lock (this)
            {
                foreach (int indx in listNeededIndexGroupSources)
                {
                    bEvtDataAskedHostQueue = false;

                    if (m_dictLinkedIndexGroupSources.ContainsKey(indx) == true)
                        if ((ID_DATA_ASKED_HOST)ev.id == ID_DATA_ASKED_HOST.START)
                        {
                            m_dictLinkedIndexGroupSources[indx].Add(indxGrpSgnls);

                            if (m_dictLinkedIndexGroupSources[indx].Count == 1)
                                bEvtDataAskedHostQueue = true;
                            else
                                ;
                        }
                        else
                            if ((ID_DATA_ASKED_HOST)ev.id == ID_DATA_ASKED_HOST.STOP)
                            {
                                m_dictLinkedIndexGroupSources[indx].Remove(indxGrpSgnls);

                                if (m_dictLinkedIndexGroupSources[indx].Count == 0)
                                    bEvtDataAskedHostQueue = true;
                                else
                                    ;
                            }
                            else
                                ;
                    else
                        ;

                    if (bEvtDataAskedHostQueue == true)
                        //Передать для добавления обработчика событий
                        EvtDataAskedHostQueue(new EventArgsDataHost(ev.id, new object[] { this, indx }));
                    else
                        ;
                }
            }
        }
    }
}