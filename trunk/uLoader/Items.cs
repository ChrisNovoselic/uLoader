using System;
using System.Collections.Generic;

using System.Reflection; //Assembly
using System.IO;
using System.Data;

using System.Threading;
using System.ComponentModel;

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
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain = HTimeSpan.FromHours(1);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal = HTimeSpan.FromMinutes(1);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsIntervalLocal = HTimeSpan.FromMilliseconds((int)DATETIME.MSEC_INTERVAL_DEFAULT);
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
                HTimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsIntervalLocal = HTimeSpan.FromMilliseconds((int)DATETIME.MSEC_INTERVAL_DEFAULT);
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
        public HTimeSpan m_tsIntervalLocal;
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
        public HTimeSpan m_tsPeriodLocal;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DATETIME_WORK ()
        {
            m_dtStart = new DateTime ();
            m_tsPeriodMain =
            m_tsPeriodLocal =
                HTimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_tsIntervalLocal = HTimeSpan.NotValue;
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
            if ((!(pars.IndexOf(@"CURINTERVAL_PERIODMAIN") < 0))
                && (!(pars.IndexOf(@"CURINTERVAL_PERIODLOCAL") < 0)))
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
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodLocal = new HTimeSpan(vals[pars.IndexOf(@"CURINTERVAL_PERIODMAIN")]); //CURINTERVAL_PERIODMAIN
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsIntervalLocal = new HTimeSpan(vals[pars.IndexOf(@"CURINTERVAL_PERIODLOCAL")]); //CURINTERVAL_PERIODLOCAL
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
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_tsIntervalLocal =
                        pars.m_arWorkIntervals[(int)mode].m_tsIntervalLocal;
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
    /// <summary>
    /// Группа источников с "прикрепленными" группами сигналов
    /// </summary>
    public class GroupSources : GROUP_SRC//, IPlugInHost
    {
        /// <summary>
        /// Класс - словарь для хранения объектов-плюгИнов
        ///  - вырожден в коллекцию(словарь) с единственным объектом
        /// </summary>
        private class PlugIns : HPlugIns
        {
            /// <summary>
            /// Идентификатор единственного загруженного плюгИна
            /// </summary>
            private int _id;
            /// <summary>
            /// Объект-плюгИн - загруженая библиотека с единственным созданным объектом класса
            ///  , из зарегистрированных в ней 
            /// </summary>
            public PlugInULoader Loader {
                get {
                    //if (this.ContainsKey(_id) == true)
                        return this[_id] as PlugInULoader;
                    //else
                    //{
                    //    Logging.Logg().Error(@"GroupSources.PlugIns::Loader - не загружена библиотека с единственным созданным объектом класса ...", Logging.INDEX_MESSAGE.NOT_SET);
                    //
                    //    return null;
                    //}
                }
            }
            /// <summary>
            /// Загрузить библиотеку с именем 'm_strDLLName'
            /// </summary>
            /// <param name="iRes">Признак выполнения загрузки библиотеки</param>
            /// <returns>Объект с загруженной библиотекой</returns>
            public void LoadPlugIn(string strDLLName, out STATE_DLL stateRes)
            //private IPlugIn loadPlugIn(string name, out int iRes)
            {
                stateRes = STATE_DLL.UNKNOWN;

                PlugInULoader plugIn = null;
                int iLoadRes = -1;

                string name = Path.GetFileNameWithoutExtension(strDLLName.Split(new char[] { ':' }, StringSplitOptions.None)[0]);

                plugIn = load(name, out iLoadRes) as PlugInULoader;

                switch (iLoadRes)
                {
                    case 0:
                        stateRes = STATE_DLL.LOADED;

                        _id = plugIn._Id;
                        Add(_id, plugIn);
                        break;
                    case -1:
                        stateRes = STATE_DLL.NOT_LOAD;
                        break;
                    case -2:
                        stateRes = STATE_DLL.TYPE_MISMATCH;
                        break;
                    default:
                        //stateRes = STATE_DLL.UNKNOWN;
                        break;
                }
            }
        }
        /// <summary>
        /// Загрузить плюгИн
        /// </summary>
        private void loadPlugIn()
        {
            PlugInULoader loader = null;
            string strTypeName = string.Empty;

            if (m_plugIns == null)
                m_plugIns = new PlugIns();
            else
                ;

            m_plugIns.LoadPlugIn(this.m_strDLLName, out _iStateDLL);

            if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
            {
                loader = m_plugIns.Loader;

                strTypeName = this.m_strDLLName.Split(new char[] { ':' }, StringSplitOptions.None)[1];
                if (loader.CreateObject(strTypeName) == 0)
                {
                    _iIdTypePlugInObjectLoaded = loader.KeySingleton; //plugInRes.GetKeyType(strTypeName);

                    //Взаимная "привязка" для обмена сообщениями
                    if (!(loader == null))
                    {
                        // библиотека - объект класса
                        loader.EvtDataAskedHost += new DelegateObjectFunc(plugIn_OnEvtDataAskedHost);
                        // объект класса - библиотека
                        EvtDataAskedHostPlugIn += loader.OnEvtDataRecievedHost;
                        ////??? - повторная установка значения
                        //_iStateDLL = HClassLibrary.HPlugIns.STATE_DLL.LOADED;
                    }
                    else
                        ;
                }
                else
                    ;
            }
            else
                _iStateDLL = HClassLibrary.HPlugIns.STATE_DLL.NOT_LOAD;

            if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
                foreach (GroupSignals itemGroupSignals in m_listGroupSignals)
                    if (itemGroupSignals.Validated == 0)
                        itemGroupSignals.State = STATE.STOPPED;                        
                    else
                        itemGroupSignals.State = STATE.UNAVAILABLE;
            else
                //throw new Exception(@"GroupSources::GroupSources () - ...")
                ;
        }
        /// <summary>
        /// Выгрузить из ОЗУ библиотеку (точнее - объект класса библиотеки)
        /// </summary>
        private void unloadPlugIn()
        {
            PlugInBase loader = null;

            if (!(m_plugIns == null))
            {
                _iIdTypePlugInObjectLoaded = -1;

                //Взаимная "РАЗпривязка" для обмена сообщениями - в обратном порядке                
                loader = m_plugIns.Loader;
                if (!(loader == null))
                {
                    // объект класса - библиотека
                    EvtDataAskedHostPlugIn -= loader.OnEvtDataRecievedHost;
                    // библиотека - объект класса
                    loader.EvtDataAskedHost -= new DelegateObjectFunc(plugIn_OnEvtDataAskedHost);
                }
                else
                    ;

                m_plugIns.UnloadPlugIn();
            }
            else
                ;

            _iStateDLL = HPlugIns.STATE_DLL.UNKNOWN;
        }
        /// <summary>
        /// Перечисление состояний группы источников
        /// </summary>
        public enum STATE { UNKNOWN = -2, REVERSE, UNAVAILABLE, STOPPED, STARTED }        
        /// <summary>
        /// Объекты синхронизации (при автоматическом старте группы)
        ///  требуется ожидание перед инициализацией 1-ой из групп сигналов
        /// </summary>
        private ManualResetEvent m_evtInitSourceSend
            , m_evtInitSourceConfirm
            , m_evtGroupSgnlsState;
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
                    if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
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

        private HClassLibrary.HPlugIns.STATE_DLL _iStateDLL;
        /// <summary>
        /// Состояние группы источников
        /// </summary>
        public HClassLibrary.HPlugIns.STATE_DLL StateDLL
        {
            get { return _iStateDLL; }
        }

        # region ??? Обязательный метод для интерфейса 'IPlugInHost'
        public int Register (IPlugIn plugIn)
        {
            int iRes = 0;

            return iRes;
        }
        # endregion
        /// <summary>
        /// Событие для обмена данными с очередью обработки событий
        /// </summary>
        public event DelegateObjectFunc EvtDataAskedHostQueue;
        /// <summary>
        /// Передать в очередь обработки событий сообщение о необходимости установления/разрыва связи между группами источников
        /// </summary>
        /// <param name="ev">Аргумент при передаче сообщения</param>
        public virtual void PerformDataAskedHostQueue(EventArgsDataHost ev)
        {
            EvtDataAskedHostQueue(ev);
        }

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

                // инициализировать данные
                this.m_tableData = new DataTable();

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
        private PlugIns m_plugIns;
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

            m_evtInitSourceSend = new ManualResetEvent (false);
            m_evtInitSourceConfirm = new ManualResetEvent(false);
            m_evtGroupSgnlsState = new /*Semaphore*/ ManualResetEvent(/*0, 1*/false);

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

            _iStateDLL = HPlugIns.STATE_DLL.UNKNOWN;
            loadPlugIn();            
        }

        public void AutoStart()
        {
            ////Вариант №0
            //new Thread(new ParameterizedThreadStart(new Action <object> (autoStart)));
            ////Вариант №1
            //BackgroundWorker thread = new BackgroundWorker ();
            //thread.DoWork += new DoWorkEventHandler(autoStart);
            //thread.RunWorkerAsync ();
            //Вариант №2
            autoStart ();
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

            m_evtInitSourceSend.Set();

            //Console.WriteLine(@"GroupSources::sendInitSource (id=" + m_iIdTypePlugInObjectLoaded + @") - ...");
            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.INIT_SOURCE, Pack()));

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
            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.INIT_SIGNALS, new object[] { iIDGroupSignals, arToDataHost }));

            return iRes;
        }
        /// <summary>
        /// Отправить DLL сообщение о "новом" (START, STOP) состоянии группы сигналов
        /// </summary>
        /// <param name="iIDGroupSignals">Идентификатор группы сигналов</param>
        /// <param name="state">"Новое" состояние</param>
        /// <returns>Признак результата выполнения</returns>
        private int sendState(int iIDGroupSignals, STATE state)
        {
            int iRes = 0;
            ID_DATA_ASKED_HOST idToSend = ID_DATA_ASKED_HOST.UNKNOWN;

            GROUP_SIGNALS_PARS grpSgnlsPars = getGroupSignalsPars(iIDGroupSignals);
            object[] arDataAskedHost = null;
            MODE_WORK mode = MODE_WORK.UNKNOWN;
            TimeSpan tsPeriodMain = TimeSpan.FromMilliseconds (-1);

            switch (state)
            {
                case STATE.STARTED:
                    idToSend = ID_DATA_ASKED_HOST.START;
                    break;
                case STATE.STOPPED:
                    idToSend = ID_DATA_ASKED_HOST.STOP;
                    break;
                default:
                    throw new Exception(@"GroupSources::sendState (id=" + m_strID + @", key=" + iIDGroupSignals + @") - неизвестное состояние: state=" + state + @" - ...");
                    break;
            }

            if (idToSend == ID_DATA_ASKED_HOST.START)
            {
                //Проверить признак группы сигналов: источник или назначение
                //if (! (grpSgnlsPars.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL] == null))
                if (grpSgnlsPars is GROUP_SIGNALS_SRC_PARS)
                {
                    mode = (grpSgnlsPars as GROUP_SIGNALS_SRC_PARS).m_mode;
                    tsPeriodMain = grpSgnlsPars.m_arWorkIntervals[(int)mode].m_tsPeriodMain.Value;

                    arDataAskedHost = new object[]
                        {
                            iIDGroupSignals
                            , new object[]
                            {
                                mode
                                , grpSgnlsPars.m_arWorkIntervals[(int)mode].m_dtStart
                                , TimeSpan.FromSeconds(tsPeriodMain.TotalSeconds)
                                , TimeSpan.FromSeconds(grpSgnlsPars.m_arWorkIntervals [(int)mode].m_tsPeriodLocal.Value.TotalSeconds)
                                , (int)grpSgnlsPars.m_arWorkIntervals [(int)mode].m_tsIntervalLocal.Value.TotalMilliseconds
                            }
                        };
                }
                else
                {
                    //??? а если группа сигналов-источник запрашивает данные с бОльшим периодом
                    // в таком случае, группа сигналов-назначение будет останавливаться/запускаться
                    // последствия могут быть - потеря сообщения
                    // вероятно, устанавливать/снимать контроль над группами сигналов-назначения следут
                    // в момент установки взаимосвязи между граппами сигналов-источниками группами сигналов-назначения
                    tsPeriodMain = TimeSpan.FromMilliseconds(66667);

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
                }
                // установить контроль за группой сигналов
                PerformDataAskedHostQueue(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)idToSend, new object[] { iIDGroupSignals, tsPeriodMain }));
            }
            else
                if (idToSend == ID_DATA_ASKED_HOST.STOP)
                {
                    arDataAskedHost = new object[]
                        {
                            iIDGroupSignals
                        };
                    // снять контроль за группой сигналов
                    PerformDataAskedHostQueue(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)idToSend, new object[] { iIDGroupSignals }));
                }
                else
                    ; //??? других команд не предусмотрено

            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)idToSend, arDataAskedHost));

            return iRes;
        }
        /// <summary>
        /// Обработка сообщений "от" библиотеки
        /// </summary>
        /// <param name="obj">Сообщение "от" библиотеки</param>
        private void plugIn_OnEvtDataAskedHost  (object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            GroupSignals grpSgnls = null;
            ID_DATA_ASKED_HOST id_cmd = ID_DATA_ASKED_HOST.UNKNOWN;
            int iIDGroupSignals = -1; //??? д.б. указана в "запросе"
            // [0] - идентификатор команды
            // [1] - идентификатор группы сигналов
            // [2] - идентификатор признака подтверждения/запроса ИЛИ таблица с результатом
            object []pars = null;
            
            try
            {
                pars = 
                    //(ev.par as object[])[0] as object []
                    ev.par as object[]
                    ;

                id_cmd = (ID_DATA_ASKED_HOST)pars[0];
                
                iIDGroupSignals = (int)pars[1];
                if (! (iIDGroupSignals < 0))
                    grpSgnls = getGroupSignals(iIDGroupSignals);
                else
                    ;

                string msgDebugLog = string.Empty;

                switch (id_cmd)
                {
                    case ID_DATA_ASKED_HOST.INIT_SOURCE: //Получен запрос на парметры инициализации
                        if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.GET)
                        {
                            //Отправить данные для инициализации
                            sendInitSource ();                            
                            msgDebugLog = @"отправлен: ";
                        }
                        else
                            if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.CONFIRM)
                            {
                                //Подтвердить установку параметров группы источников
                                m_evtInitSourceConfirm.Set();
                                msgDebugLog = @"подтверждено: ";
                            }
                            else
                                ;

                        msgDebugLog += id_cmd.ToString ();
                        break;
                    case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Получен запрос на обрабатываемую группу сигналов
                        if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.GET)
                        {
                            //Отправить данные для инициализации
                            sendInitGroupSignals(iIDGroupSignals);
                        }
                        else
                            if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.CONFIRM)
                                //Отправить команду старт
                                if (!(grpSgnls.State == STATE.UNAVAILABLE))
                                {
                                    sendState(iIDGroupSignals, STATE.STARTED);
                                    //Подтвердить установку параметров группы сигналов
                                    msgDebugLog = @"подтверждено: ";
                                }
                                else
                                    ;
                            else
                                ;

                        msgDebugLog += id_cmd.ToString();
                        break;
                    case ID_DATA_ASKED_HOST.TABLE_RES:
                        if ((!(grpSgnls == null))
                            && (!(pars[2] == null)))
                        {
                            msgDebugLog = @"получено строк=";

                            if ((pars[2] as DataTable).Rows.Count > 0)
                            {
                                grpSgnls.m_tableData = (pars[2] as DataTable).Copy();
                                msgDebugLog += grpSgnls.m_tableData.Rows.Count;
                            }
                            else
                                msgDebugLog += 0.ToString();

                            this.PerformDataAskedHostQueue(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)id_cmd, new object[] { iIDGroupSignals }));
                        }
                        else
                            ;
                        break;
                    case ID_DATA_ASKED_HOST.START:
                    case ID_DATA_ASKED_HOST.STOP:
                        //m_semaStateChange.Release (1);
                        //Вариант №2 (пост-установка)
                        grpSgnls.StateChange();
                        //Подтвердить изменение состояния группы сигналов
                        // + установить/разорвать взаимосвязь между группами источников (при необходимости) - для 'GroupSourcesDest'
                        this.PerformDataAskedHostQueue(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)id_cmd, new object[] { iIDGroupSignals, ID_HEAD_ASKED_HOST.CONFIRM }));

                        msgDebugLog = @"подтверждено: " + id_cmd.ToString();

                        try
                        {
                            if ((grpSgnls.State == STATE.STOPPED)
                                && (State == STATE.STOPPED)
                                )
                            {
                                m_evtInitSourceSend.Reset();
                                m_evtInitSourceConfirm.Reset();
                            }
                            else
                                ;
                            //Разрешить очередную команду на изменение состояния
                            //Console.WriteLine(@"GroupSources::plugIn_OnEvtDataAskedHost () - m_evtGroupSgnlsState.Set() - " + msgDebugLog + @"...");
                            m_evtGroupSgnlsState./*Release(1)*/Set();
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e
                                , @"GroupSources::plugIn_OnEvtDataAskedHost () - idGroupSgnls=" + iIDGroupSignals + @" ..."
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        // очистить данные
                        grpSgnls.m_tableData = new DataTable();
                        break;
                    case ID_DATA_ASKED_HOST.ERROR:
                        //???
                        msgDebugLog = @"получена ошибка: ???";
                        break;
                    default:
                        break;
                }

                Logging.Logg().Debug(@"GroupSources::plugIn_OnEvtDataAskedHost (id=" + m_strID + @", key=" + (grpSgnls == null ? @"НЕ_ТРЕБУЕТСЯ" : grpSgnls.m_strID) + @") - " + msgDebugLog + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"GroupSources::plugIn_OnEvtDataAskedHost (id=" + m_strID + @", key=" + grpSgnls.m_strID + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
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

        //private void autoStart(object obj) //Вариант №0
        //private void autoStart(object obj, DoWorkEventArgs ev) //Вариант №1
        private void autoStart() //Вариант №2
        {
            int idGrpSgnls = -1;
            GROUP_SIGNALS_PARS grpSgnlsPars;

            if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
                foreach (GroupSignals itemGroupSignals in m_listGroupSignals)
                    if (itemGroupSignals.State == STATE.STOPPED)
                    {
                        idGrpSgnls = FormMain.FileINI.GetIDIndex(itemGroupSignals.m_strID);

                        grpSgnlsPars = getGroupSignalsPars(idGrpSgnls);
                        if (grpSgnlsPars.m_iAutoStart == 1)
                        {
                            stateChange(idGrpSgnls, STATE.STARTED);
                        }
                        else
                            ;
                    }
                    else
                        ;
            else
                ;
        }
        /// <summary>
        /// Выгрузить/загрузить библиотеку (объект класса)
        /// </summary>
        /// <returns>Признак выполнения функции</returns>
        public int Reload()
        {
            int iRes = 0;            

            if ((_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
                && (!(State == STATE.UNAVAILABLE)))
                //Изменить(остановить) состояние ВСЕХ групп сигналов
                stateChange(STATE.STOPPED);
            else
                ; //??? ошибка

            unloadPlugIn();
            loadPlugIn();

            autoStart();

            return iRes;
        }
        /// <summary>
        /// Остановить/запустить все группы сигналов
        /// </summary>
        /// <returns>Признак выполнения функции</returns>
        public int StateChange()
        {
            int iRes = 0;
            STATE newState = STATE.UNKNOWN;

            if ((_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
                && (! (State == STATE.UNAVAILABLE)))
            {
                newState = getNewState(State, out iRes);

                //Изменить состояние ВСЕХ групп сигналов
                stateChange(newState);                
            }
            else
                iRes = -1;

            return iRes;
        }

        public int StateChange(int iId, STATE prevState = STATE.REVERSE)
        {
            int iRes = 0;

            //Изменить состояние только ОДНой группы сигналов
            GroupSignals grpSgnls = getGroupSignals(iId);
            STATE newState = getNewState(grpSgnls.State, out iRes);

            if ((!(grpSgnls == null))
                && (iRes == 0))
            {
                if ((grpSgnls.State == STATE.STARTED)
                    && (!(State == STATE.STARTED)))
                    throw new Exception(@"GroupSources::StateChange (ID=" + iId + @") - несовместимые состояния групп источников и сигналов...");
                else
                    ;

                stateChange(iId, newState);
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
        /// Остановить/запустить группу сигналов
        /// </summary>
        /// <param name="strId">Строковый идентификатор группы источников</param>
        /// <param name="prevState">Признак для изменения состояния</param>
        /// <returns>Признак выполнения функции</returns>
        public int StateChange (string strId, STATE prevState = STATE.REVERSE)
        {
            return StateChange(FormMain.FileINI.GetIDIndex(strId), prevState);
        }
        /// <summary>
        /// Изменитьсостояние ВСЕХ групп сигналов
        /// </summary>
        /// <param name="newState">Новое состояние для групп</param>
        private void stateChange(STATE newState)
        {
            //Изменить состояние ВСЕХ групп сигналов
            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                //Проверить текущее стостояние
                if ((!(grpSgnls.State == newState))
                    && (!(grpSgnls.State == STATE.UNAVAILABLE)))
                    stateChange(FormMain.FileINI.GetIDIndex(grpSgnls.m_strID), newState);
                else
                    ; //Группа сигналов уже имеет указанное состояние ИЛИ не может изменить состояние на указанное
        }

        private void stateChange (int iId, STATE newState)
        {
            bool bSync = false;

            if ((newState == STATE.STARTED)
                && (m_evtInitSourceSend.WaitOne(0) == false)
                )
                //??? возможно отправление повторного сообщения
                // требуется ожидать подтверждения приема параметров соединения с источником
                sendInitSource();
            else
                //if (newState == STATE.STOPPED)
                //    m_evtInitSource.Reset();
                //else
                    ;

            //Вариант №2 (пост-установка)
            if (newState == STATE.STARTED)
                // ожидать подтверждения инициализации параметров соединения с источником
                if (m_evtInitSourceConfirm.WaitOne())
                    // отправление 'STARTED' будет произведено после подтверждения инициализации группы сигналов
                    sendInitGroupSignals(iId);
                else
                    ;
            else
                // отправить 'STOPPED'
                sendState(iId, newState);

            //Ожидать подтверждение об изменении состояния
            m_evtGroupSgnlsState.WaitOne ();
            //Console.WriteLine(@"GroupSources::stateChange (iId=" + iId + @", newState=" + newState.ToString() + @") - m_evtGroupSgnlsState.WaitOne() ...");
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
            DataTable tblRec = null
                , tblToPanel = null;
            DataRow[]arSel;
            object [] arObjToRow = null;
            string strNameFieldID = string.Empty;
            HTimeSpan tsToData = HTimeSpan.NotValue;

            //Проверить длину идентификатора
            if (id.Length > 1)
            {//Только при длине > 1 м. определить целочисленное значение идентификатора
                iID = uLoader.FormMain.FileINI.GetIDIndex(id);
                grpSgnls = getGroupSignals(iID);
                if (!(grpSgnls == null))
                    if (grpSgnls.State == STATE.STARTED)
                    {
                        tblRec = grpSgnls.m_tableData;
                        tblToPanel = new DataTable();

                        if (!(tblRec == null))
                            if (tblRec.Rows.Count > 0)
                                if ((!(tblRec.Columns.IndexOf(@"ID") < 0)) && (!(tblRec.Columns.IndexOf(@"DATETIME") < 0)))
                                {
                                    if (m_dictAdding.ContainsKey(@"UTC_OFFSET_TO_DATA") == true)
                                        tsToData = new HTimeSpan(m_dictAdding[@"UTC_OFFSET_TO_DATA"]);
                                    else
                                        ;
                                    //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - получено строк=" + tblRec.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);

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
                                        arSel = tblRec.Select(@"ID=" + sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf(strNameFieldID)], @"DATETIME DESC");
                                        if (arSel.Length > 0)
                                            arObjToRow = new object[] { sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf (@"NAME_SHR")]
                                                , arSel[0][@"VALUE"]
                                                , ((DateTime)arSel[0][@"DATETIME"]).Add(tsToData == HTimeSpan.NotValue ? TimeSpan.Zero : - tsToData.Value).ToString(@"dd.MM.yyyy HH:mm:ss.fff")
                                                , arSel.Length };
                                        else
                                            arObjToRow = new object[] { sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf(@"NAME_SHR")], string.Empty, string.Empty, string.Empty };

                                        tblToPanel.Rows.Add(arObjToRow);
                                    }

                                    //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - строк для отображения=" + tblToPanel.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                                }
                                else
                                    throw new Exception(@"uLoader::GroupSources::GetDataToPanel () - ...");
                            else
                                ; //tblRec.Rows.Count == 0
                        else
                            //tblRec == null
                            Logging.Logg().Warning(@"GroupSources::GetDataToPanel (id=" + id + @") - таблица 'received' не существует...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    else
                        ; // группа сигналов не выполняется
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
        /// <returns>Признак выполнения функции</returns>
        public int Stop()
        {
            int iRes = 0;

            if ((_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
                && (!(State == STATE.UNAVAILABLE)))
                //Изменить(остановить) состояние ВСЕХ групп сигналов
                stateChange(STATE.STOPPED);
            else
                ; //??? ошибка

            //m_evtGroupSgnlsState.WaitOne();
            
            unloadPlugIn();

            return iRes;
        }
        /// <summary>
        /// Добавить обработчик событий от присоединенной библиотеки
        /// </summary>
        /// <param name="indxGrpSrcDest">Индекс группы источников (газначения) - не используется</param>
        /// <param name="fOnEvt">Функция обработки</param>
        public void AddDelegatePlugInOnEvtDataAskedHost (int indxGrpSrcDest, DelegateObjectFunc fOnEvt)
        {
            try
            {
                m_plugIns.Loader.EvtDataAskedHost += fOnEvt;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"GroupSources::AddDelegatePlugInOnEvtDataAskedHost (IdTypePlugInObject=" + _iIdTypePlugInObjectLoaded + @") - ошибка обращения к объекту ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// Удалить обработчик событий от присоединенной библиотеки
        /// </summary>
        /// <param name="indxGrpSrcDest">Индекс группы источников (газначения) - не используется</param>
        /// <param name="fOnEvt">Функция обработки</param>
        public void RemoveDelegatePlugInOnEvtDataAskedHost(int indxGrpSrcDest, DelegateObjectFunc fOnEvt)
        {
            try {
                if (_iIdTypePlugInObjectLoaded > 0)
                    m_plugIns.Loader.EvtDataAskedHost -= fOnEvt;
                else
                    ; // плюгИн уже выгружен, либо идентификатор типа некорректен
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"GroupSources::RemoveDelegatePlugInOnEvtDataAskedHost (IdTypePlugInObject=" + _iIdTypePlugInObjectLoaded + @") - ошибка обращения к объекту ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
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

        public string GetIdGroupSignals(int indx)
        {
            GroupSignals grpSgnls = getGroupSignals(indx);

            return grpSgnls == null ? string.Empty : grpSgnls.m_strID;
        }
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

    public class GroupSourcesDest : GroupSources
    {
        /// <summary>
        /// Словарь с ключами - индексами групп источников
        ///  значениями - списками индексов групп сигналов
        ///  , подписчиками на таблицы результатов
        /// </summary>
        Dictionary<int, List<int>> m_dictLinkedIndexGroupSources;        
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

                if (!(m_listSgnls == null))
                    foreach (SIGNAL_SRC sgnl in m_listSgnls)
                    {
                        iIdGrpSgnls = Convert.ToInt16(sgnl.m_arSPars[m_listSKeys.IndexOf(@"ID_SRC_SGNL")].Substring(1, 2)) - 1;
                        if (listRes.IndexOf(iIdGrpSgnls) < 0)
                            listRes.Add(iIdGrpSgnls);
                        else
                            ;
                    }
                else
                    ;

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
            object[] pars = ev.par as object[];
            object[] parsToSend = null;

            //pars[0] - идентификатор события
            switch ((ID_DATA_ASKED_HOST)pars[0])
            {
                case ID_DATA_ASKED_HOST.INIT_SOURCE: //Получен запрос на парметры инициализации
                    break;
                case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Получен запрос на обрабатываемую группу сигналов
                    break;
                case ID_DATA_ASKED_HOST.TABLE_RES:
                    parsToSend = new object[pars.Length - 1];
                    //Проверить таблицу со значениями от библиотеки на 'null'
                    if ((!(pars[2] == null))
                        && ((pars[2] as DataTable).Rows.Count > 0))
                    {
                        //Заполнить для передачи основные параметры - таблицу
                        parsToSend[1] = (pars[2] as DataTable).Copy();
                        //Проверить наличие дополнительных параметров
                        //??? 07.12.2015 лишнее 'Dest' не обрабатывает - см. TO_START
                        if ((parsToSend.Length > 2)
                            && (pars.Length > 3)
                            && (!(pars[3] == null)))
                        {
                            parsToSend[2] = new object[(pars[3] as object[]).Length];
                            //Заполнить для передачи дополнительные параметры - массив объектов
                            ////Вариант №1
                            //for (int i = 0; i < (parsToSend[2] as object []).Length; i ++)
                            //    (parsToSend[2] as object [])[i] = (pars[3] as object[])[i];
                            //Вариант №2
                            (pars[3] as object[]).CopyTo(parsToSend[2] as object[], 0);
                        }
                        else
                            ;
                        //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                        foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                            if (!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0))
                            {//Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]'
                                parsToSend[0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                                PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.TO_INSERT, parsToSend));

                                //Logging.Logg().Debug(@"GroupSources::Clone_OnEvtDataAskedHost () - NAME=" + m_strShrName + @", от [ID=" + (int)pars[1] + @"] для [ID=" + parsToSend[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                            else
                                ;
                    }
                    else
                        ; // таблица со значениями от библиотеки = null
                    break;
                //case ID_DATA_ASKED_HOST.START:
                //    parsToSend = new object[(pars[3] as object[]).Length + 1]; // '+1' для идентификатора группы сигналов
                //    //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                //    foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                //        if (!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0))
                //        {//Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]'
                //            parsToSend [0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                //            parsToSend[1] = (MODE_WORK)(pars[3] as object[])[0]; //MODE_WORK
                //            parsToSend[2] = (int)(pars[3] as object[])[1]; //IdSourceConnSett
                //            parsToSend[3] = (int)(pars[3] as object[])[2]; //ID_TEC
                //            PerformDataAskedHostPlugIn(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.TO_START, parsToSend));

                //            //Logging.Logg().Debug(@"GroupSources::Clone_OnEvtDataAskedHost () - NAME=" + m_strShrName + @", от [ID=" + (int)pars[1] + @"] для [ID=" + parsToSend[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
                //        }
                //        else
                //            ;
                //    break;
                case ID_DATA_ASKED_HOST.STOP:
                    parsToSend = new object [1];
                    //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                    foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                        if ((!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0))
                            && (grpSgnls.State == STATE.STARTED))
                        {
                            parsToSend[0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                            //Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]';
                            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.TO_STOP, parsToSend));
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
        public override void PerformDataAskedHostQueue (EventArgsDataHost ev)
        {
            //id_main - идентификатор типа объекта в загруженной в ОЗУ библиотеки
            //id_detail - команда на изменение состояния группы сигналов
            //В 0-ом параметре передан индекс (???идентификатор) группы сигналов
            int indxGrpSgnls = (int)(ev.par as object[])[0];
            ////В 1-ом параметре передан признак инициирования/подтверждения изменения состояния группы сигналов
            //ID_HEAD_ASKED_HOST idHeadAskedHost = (ID_HEAD_ASKED_HOST)(ev.par as object[])[1];

            base.PerformDataAskedHostQueue(ev);

            List<int> listNeededIndexGroupSources = GetListNeededIndexGroupSources(indxGrpSgnls);
            bool bEvtDataAskedHostQueue = false;

            lock (this)
            {
                foreach (int indx in listNeededIndexGroupSources)
                {
                    bEvtDataAskedHostQueue = false;

                    if (m_dictLinkedIndexGroupSources.ContainsKey(indx) == true)
                        if ((ID_DATA_ASKED_HOST)ev.id_detail == ID_DATA_ASKED_HOST.START)
                        {
                            m_dictLinkedIndexGroupSources[indx].Add(indxGrpSgnls);

                            if (m_dictLinkedIndexGroupSources[indx].Count == 1)
                                bEvtDataAskedHostQueue = true;
                            else
                                ;
                        }
                        else
                            if ((ID_DATA_ASKED_HOST)ev.id_detail == ID_DATA_ASKED_HOST.STOP)
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
                        base.PerformDataAskedHostQueue(new EventArgsDataHost(ev.id_main, ev.id_detail, new object[] { this, indx }));
                    else
                        ;
                }
            }
        }
    }
}