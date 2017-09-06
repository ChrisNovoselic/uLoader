using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
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
            public PlugInULoader Loader
            {
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

                if (name.Equals(string.Empty) == false) {
                    plugIn = load(name, out iLoadRes) as PlugInULoader;

                    switch (iLoadRes) {
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
                } else
                    ;
            }
        }
        /// <summary>
        /// Загрузить плюгИн
        /// </summary>
        private void loadPlugIn()
        {
            PlugInULoader loader = null;
            string strTypeName = string.Empty;
            int iLoaderCreateObject = -1;

            if (m_plugIns == null)
                m_plugIns = new PlugIns();
            else
                ;

            m_plugIns.LoadPlugIn(this.m_strDLLName, out _iStateDLL);

            if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED) {
                loader = m_plugIns.Loader;

                if (!(loader == null)) {
                    strTypeName = this.m_strDLLName.Split(new char[] { ':' }, StringSplitOptions.None)[1];

                    iLoaderCreateObject = loader.CreateObject(strTypeName);
                    if (iLoaderCreateObject == 0) {
                        //Статус "загруженная библиотека" сохраняется
                        //Взаимная "привязка" для обмена сообщениями
                        _iIdTypePlugInObjectLoaded = loader.KeySingleton; //plugInRes.GetKeyType(strTypeName);                    
                        // библиотека - объект класса
                        loader.EvtDataAskedHost += new DelegateObjectFunc(plugIn_OnEvtDataAskedHost);
                        // объект класса - библиотека
                        EvtDataAskedHostPlugIn += loader.OnEvtDataRecievedHost;
                        ////??? - повторная установка значения
                        //_iStateDLL = HClassLibrary.HPlugIns.STATE_DLL.LOADED;                    
                    } else {
                        //Статус "загруженная библиотека" изменяется
                        _iStateDLL = iLoaderCreateObject == -1 ? HClassLibrary.HPlugIns.STATE_DLL.NOT_LOAD
                            : iLoaderCreateObject == -2 ? HPlugIns.STATE_DLL.TYPE_MISMATCH
                                : HClassLibrary.HPlugIns.STATE_DLL.NOT_LOAD;

                        try { throw new Exception(string.Format(@"Внимание!")); } catch (Exception e) { Logging.Logg().Exception(e, string.Format(@"GroupSources::loadPlugIn(ID={0}, SHR_NAME={1}, DLL_NAME={2}) - ошибка при создании объекта библиотеки...", m_strID, m_strShrName, m_strDLLName), Logging.INDEX_MESSAGE.NOT_SET); }
                    }
                } else
                    ;
            } else
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

            if (!(m_plugIns == null)) {
                _iIdTypePlugInObjectLoaded = -1;

                //Взаимная "РАЗпривязка" для обмена сообщениями - в обратном порядке                
                loader = m_plugIns.Loader;
                if (!(loader == null)) {
                    // объект класса - библиотека
                    EvtDataAskedHostPlugIn -= loader.OnEvtDataRecievedHost;
                    // библиотека - объект класса
                    loader.EvtDataAskedHost -= new DelegateObjectFunc(plugIn_OnEvtDataAskedHost);
                } else
                    ;

                m_plugIns.UnloadPlugIn();
            } else
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
        private ManualResetEvent /*m_evtStartDo
            ,*/ m_evtInitSourceConfirm;
        private AutoResetEvent m_evtGroupSgnlsState;
        /// <summary>
        /// Состояние группы источников
        /// </summary>
        public STATE State
        {
            get {
                STATE stateRes = STATE.UNKNOWN;

                if (m_listGroupSignals.Count > 0)
                    foreach (GroupSignals grpSgnls in m_listGroupSignals)
                        if (grpSgnls.State == STATE.UNAVAILABLE) {
                            stateRes = STATE.UNAVAILABLE;
                            break;
                        } else
                            ;
                else
                    stateRes = STATE.UNAVAILABLE;

                if (stateRes == STATE.UNKNOWN)
                    //все группы сигналов имеют "известное" состояние
                    if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED) {
                        stateRes = STATE.STOPPED;

                        foreach (GroupSignals grpSgnls in m_listGroupSignals)
                            if (grpSgnls.State == STATE.STARTED) {
                                stateRes = STATE.STARTED;
                                break;
                            } else
                                ;
                    } else
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
        public int Register(IPlugIn plugIn)
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
            return Activator.CreateInstance(typeObjGroupSignals, new object[] { par });
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

                set {
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
                get {
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

                this.m_listSKeys = new List<string>();
                foreach (string skey in srcItem.m_listSKeys)
                    this.m_listSKeys.Add(skey);

                //??? Значения списков независимы
                this.m_listSgnls = srcItem.m_listSgnls;
                //??? Значения словарей независимы
                this.m_dictFormula = srcItem.m_dictFormula;

                //this.m_mode = srcItem.m_mode;

                this.m_strID = srcItem.m_strID;
                this.m_strShrName = srcItem.m_strShrName;

                // инициализировать данные
                this.m_tableData = new DataTable();

                _state = STATE.UNAVAILABLE;
            }

            public int StateChange()
            {
                int iRes = 0;

                State = GroupSources.getNewState(State, out iRes);

                return iRes;
            }

            //public int StateChange (STATE newState)
            //{
            //    int iRes = 0;

            //    if (State == STATE.UNAVAILABLE)
            //        iRes = -1;
            //    else
            //        State = newState;

            //    return iRes;
            //}

            public object[] Pack()
            {
                object[] arObjRes = null;

                if (m_listSgnls == null) {
                    arObjRes = new object[] { };

                    Logging.Logg().Warning(@"GroupSignals::Pack (id=" + m_strID + @") - список сигналов == null...", Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    arObjRes = new object[m_listSgnls.Count];

                int i = -1
                    , j = -1
                    , iVal = -1;

                for (i = 0; i < arObjRes.Length; i++) {
                    arObjRes[i] = new object[m_listSKeys.Count];

                    for (j = 0; j < m_listSKeys.Count; j++)
                        if (Int32.TryParse(m_listSgnls[i].m_arSPars[j].Trim(), out iVal) == true)
                            (arObjRes[i] as object[])[j] = iVal;
                        else
                            (arObjRes[i] as object[])[j] = m_listSgnls[i].m_arSPars[j];
                }

                return arObjRes;
            }
        }
        /// <summary>
        /// Список групп сигналов, принадлежащих группе источников
        /// </summary>
        protected List<GroupSignals> m_listGroupSignals;
        /// <summary>
        /// Возвратить массив состояний групп сигналов для группы источников
        /// </summary>
        public object[] GetArgGroupSignals()
        {
            object[] arRes = new object[m_listGroupSignals.Count];

            int i = 0;
            foreach (GroupSignals grpSgnls in m_listGroupSignals) {
                arRes[i] = new object[] { grpSgnls.State, m_listGroupSignalsPars[i].m_bToolsEnabled, m_listGroupSignalsPars[i].m_iAutoStart };

                i++;
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
        public GroupSources(GROUP_SRC srcItem, List<GROUP_SIGNALS_SRC> listGroupSignals) : base()
        {
            this.m_listGroupSignalsPars = new List<GROUP_SIGNALS_PARS>();
            foreach (GROUP_SIGNALS_PARS pars in srcItem.m_listGroupSignalsPars)
                this.m_listGroupSignalsPars.Add(pars);

            m_listGroupSignals = new List<GroupSignals>();
            foreach (GROUP_SIGNALS_SRC itemGroupSignals in listGroupSignals)
                m_listGroupSignals.Add(createGroupSignals(itemGroupSignals));

            this.m_listSKeys = new List<string>();
            foreach (string skey in srcItem.m_listSKeys)
                this.m_listSKeys.Add(skey);

            //m_evtStartDo = new ManualResetEvent (true);
            m_evtInitSourceConfirm = new ManualResetEvent(false);
            m_evtGroupSgnlsState = new /*Semaphore*/ AutoResetEvent(/*0, 1*/false);

            //Список с объектми параметров соединения с источниками данных
            foreach (KeyValuePair<string, ConnectionSettings> pair in srcItem.m_dictConnSett)
                //??? Значения списка независимы
                this.m_dictConnSett.Add(pair.Key, pair.Value);

            //Строковый идентификтор группы
            this.m_strID = srcItem.m_strID;
            this.m_strShrName = srcItem.m_strShrName;

            //Идентификатор текущего источника данных
            this.m_IDCurrentConnSett = srcItem.m_IDCurrentConnSett;

            //Наименование библиотеки
            this.m_strDLLName = srcItem.m_strDLLName;

            //Дополнительные параметры
            this.m_dictAdding = new Dictionary<string, string>();
            if ((!(srcItem.m_dictAdding == null))
                && (srcItem.m_dictAdding.Count > 0))
                foreach (KeyValuePair<string, string> pair in srcItem.m_dictAdding)
                    this.m_dictAdding.Add(pair.Key, pair.Value);
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
            autoStart();
        }

        public void AutoStop()
        {
            int idGrpSgnls = -1;
            GROUP_SIGNALS_PARS grpSgnlsPars;

            if (_iStateDLL == HClassLibrary.HPlugIns.STATE_DLL.LOADED)
                foreach (GroupSignals itemGroupSignals in m_listGroupSignals)
                    if (itemGroupSignals.State == STATE.STARTED) {
                        idGrpSgnls = FormMain.FileINI.GetIDIndex(itemGroupSignals.m_strID);

                        stateChange(idGrpSgnls, STATE.STOPPED);
                    } else
                        ;
            else
                ;
        }

        private object[] Pack()
        {
            object[] arObjRes = new object[1 + m_dictAdding.Count];

            arObjRes[0] = this.m_dictConnSett[m_IDCurrentConnSett];

            string adding = string.Empty;
            int i = 1;
            foreach (KeyValuePair<string, string> pair in m_dictAdding)
                arObjRes[i++] = pair.Key + FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE] + pair.Value;

            return arObjRes;
        }

        private int sendInitSource()
        {
            int iRes = 0;

            //Console.WriteLine(@"GroupSources::sendInitSource (id=" + m_iIdTypePlugInObjectLoaded + @") - ...");
            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.INIT_SOURCE, Pack()));

            return iRes;
        }
        /// <summary>
        /// Передать сообщение библиотеке
        /// </summary>
        /// <param name="ev">Аргумент при передаче сообщения</param>
        protected void PerformDataAskedHostPlugIn(EventArgsDataHost ev)
        {
            EvtDataAskedHostPlugIn(ev);
        }

        private int sendInitGroupSignals(int iIDGroupSignals)
        {
            int iRes = 0;
            GroupSignals grpSgnls = getGroupSignals(iIDGroupSignals);
            object[] arToDataHost = new object[] { };

            if (!(grpSgnls == null)) {
                arToDataHost = grpSgnls.Pack();

                if (!(arToDataHost.Length > 0))
                    iRes = -2;
                else
                    ;
            } else {
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

            GroupSignals grpSgnls = null;
            GROUP_SIGNALS_PARS grpSgnlsPars = null;
            object[] arDataAskedHost = null;
            MODE_WORK mode = MODE_WORK.UNKNOWN;
            TimeSpan tsPeriodMain = TimeSpan.FromMilliseconds(-1);
            string srlzFormula = string.Empty;

            // получить группу сигналов для сериализации словаря с формулами
            grpSgnls = getGroupSignals(iIDGroupSignals);
            // получить параметры группы сигналов
            grpSgnlsPars = getGroupSignalsPars(iIDGroupSignals);
            // сериализовать словарь с формулами
            if (!(grpSgnls.m_dictFormula == null))
                foreach (KeyValuePair<string, string> pair in grpSgnls.m_dictFormula)
                    if (pair.Value.Equals(string.Empty) == false)
                        srlzFormula += pair.Key + @"=" + pair.Value + @";";
                    else
                        ;
            else
                ; // нет ни одной формулы
            // удалить лишний символ ';' из результирующей строки сериализованного словаоя с формулами
            if (srlzFormula.Equals(string.Empty) == false)
                srlzFormula = srlzFormula.Substring(0, srlzFormula.Length - 1);
            else
                ;

            switch (state) {
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

            if (idToSend == ID_DATA_ASKED_HOST.START) {
                //Проверить признак группы сигналов: источник или назначение
                //if (! (grpSgnlsPars.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL] == null))
                if (grpSgnlsPars is GROUP_SIGNALS_SRC_PARS) {
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
                                , srlzFormula
                                , grpSgnlsPars.m_TableName
                            }
                        };
                } else {
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
                                    , -1 //TotalMilliseconds
                                    , srlzFormula
                                    ,
                                }
                            };
                    else
                        ;
                }
                // установить контроль за группой сигналов
                PerformDataAskedHostQueue(new EventArgsDataHost(
                    (int)Index // тип группы источников (источник/назначение)
                    , FormMain.FileINI.GetIDIndex(m_strID) // индекс/идентификатор группы источников
                    , new object[] { iIDGroupSignals // индекс/идентификатор группы сигналов
                        , idToSend // команда для группы сигналов
                        , tsPeriodMain } // опционально - главный период группы сигналов (для источника период за который запрашиваются/агрегируются данные)
                    ));
            } else
                if (idToSend == ID_DATA_ASKED_HOST.STOP) {
                arDataAskedHost = new object[]
                    {
                            iIDGroupSignals
                    };
                // снять контроль за группой сигналов
                PerformDataAskedHostQueue(new EventArgsDataHost(
                    (int)Index
                    , FormMain.FileINI.GetIDIndex(m_strID)
                    , new object[] { iIDGroupSignals
                            , idToSend}
                    ));
            } else
                ; //??? других команд не предусмотрено

            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)idToSend, arDataAskedHost));

            return iRes;
        }

        private INDEX_SRC Index { get { return this is GroupSourcesDest ? INDEX_SRC.DEST : INDEX_SRC.SOURCE; } }
        /// <summary>
        /// Обработка сообщений "от" библиотеки
        /// </summary>
        /// <param name="obj">Сообщение "от" библиотеки</param>
        private void plugIn_OnEvtDataAskedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            GroupSignals grpSgnls = null;
            ID_DATA_ASKED_HOST id_cmd = ID_DATA_ASKED_HOST.UNKNOWN;
            int iIDGroupSignals = -1 //??? д.б. указана в "запросе"
                , cntRecievedRows = -1; // кол-во строк в полученной таблице рез-та
            // [0] - идентификатор команды
            // [1] - идентификатор группы сигналов
            // [2] - идентификатор признака подтверждения/запроса ИЛИ таблица с результатом
            object[] pars = null;

            try {
                pars =
                    //(ev.par as object[])[0] as object []
                    ev.par as object[]
                    ;

                id_cmd = (ID_DATA_ASKED_HOST)pars[0];

                iIDGroupSignals = (int)pars[1];
                if ((!(iIDGroupSignals < 0))
                    && (iIDGroupSignals < int.MaxValue))
                    grpSgnls = getGroupSignals(iIDGroupSignals);
                else
                    ;

                string msgDebugLog = string.Empty;

                switch (id_cmd) {
                    case ID_DATA_ASKED_HOST.INIT_SOURCE: //Получен запрос на парметры инициализации
                        if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.GET) {
                            //Отправить данные для инициализации
                            sendInitSource();
                            msgDebugLog = @"отправлен: ";
                        } else
                            if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.CONFIRM) {
                            //Подтвердить установку параметров группы источников
                            m_evtInitSourceConfirm.Set();
                            msgDebugLog = @"подтверждено: ";
                        } else
                            ;

                        msgDebugLog += id_cmd.ToString();
                        break;

                    #region Отправить данные для инициализации группы сигналов, команду старт, если получено подтверждение приема/обработки данных инициализации
                    case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Получен запрос на обрабатываемую группу сигналов
                        if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.GET) {
                            //Отправить данные для инициализации
                            sendInitGroupSignals(iIDGroupSignals);
                        } else
                            if ((ID_HEAD_ASKED_HOST)pars[2] == ID_HEAD_ASKED_HOST.CONFIRM)
                            //Отправить команду старт
                            if (!(grpSgnls.State == STATE.UNAVAILABLE)) {
                                sendState(iIDGroupSignals, STATE.STARTED);
                                //Подтвердить установку параметров группы сигналов
                                msgDebugLog = @"подтверждено: ";
                            } else
                                ;
                        else
                            ;

                        msgDebugLog += id_cmd.ToString();
                        break;
                    #endregion

                    #region Обработать полученные из библиотеки результаты
                    case ID_DATA_ASKED_HOST.TABLE_RES:
                        if ((!(grpSgnls == null))
                            && ((!(pars[2] == null))
                                && (pars[2] is DataTable))
                            ) {
                            cntRecievedRows = (pars[2] as DataTable).Rows.Count;
                            msgDebugLog = @"получено строк=" + cntRecievedRows;

                            if (cntRecievedRows > 0)
                                // при наличии в ответе строк
                                grpSgnls.m_tableData = (pars[2] as DataTable).Copy();
                            else
                                ;

                            // обновить состояние контролируемой группы сигналов (без проверки кол-ва строк)
                            this.PerformDataAskedHostQueue(new EventArgsDataHost(
                                (int)Index // тип группы источников (источник/назначение)
                                , FormMain.FileINI.GetIDIndex(m_strID) // индекс/идентификатор группы источников
                                , new object[] { iIDGroupSignals // индекс/идентификатор группы сигналов
                                    , id_cmd // команда для группы сигналов
                                    , cntRecievedRows }
                                ));
                        } else
                            ;
                        break;
                    #endregion

                    case ID_DATA_ASKED_HOST.START:
                    case ID_DATA_ASKED_HOST.STOP:
                        try {
                            if ((!(grpSgnls == null))
                                && (iIDGroupSignals < int.MaxValue)) {
                                grpSgnls.StateChange();
                                //Подтвердить изменение состояния группы сигналов
                                // + установить/разорвать взаимосвязь между группами источников (при необходимости) - для 'GroupSourcesDest'
                                this.PerformDataAskedHostQueue(new EventArgsDataHost(
                                    (int)Index
                                    , FormMain.FileINI.GetIDIndex(m_strID)
                                    , new object[] { iIDGroupSignals
                                        , id_cmd
                                        , ID_HEAD_ASKED_HOST.CONFIRM } // опционально - признак подтверждения выполнения команды
                                    ));

                                msgDebugLog = @"подтверждено: " + id_cmd.ToString();

                                //Разрешить очередную команду на изменение состояния
                                //Console.WriteLine(@"GroupSources::plugIn_OnEvtDataAskedHost () - m_evtGroupSgnlsState.Set() - " + msgDebugLog + @"...");
                                m_evtGroupSgnlsState./*Release(1)*/Set();

                                // очистить данные
                                if (!(grpSgnls == null))
                                    grpSgnls.m_tableData = new DataTable();
                                else
                                    ;
                            } else
                            // работа по группе источников
                                if (
                                    (State == STATE.STOPPED) // признак, что все группы сигналов остановлены
                                    && (iIDGroupSignals == int.MaxValue) // признак, что выполнена команда стоп для интерфейса
                                    ) {
                                //m_evtStartDo.Set();
                                m_evtInitSourceConfirm.Reset();
                            } else
                                ;
                        } catch (Exception e) {
                            Logging.Logg().Exception(e
                                , @"GroupSources::plugIn_OnEvtDataAskedHost () - idGroupSgnls=" + iIDGroupSignals + @" ..."
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        break;
                    case ID_DATA_ASKED_HOST.ERROR:
                        //???
                        msgDebugLog = @"получена ошибка: ???";
                        break;
                    default:
                        break;
                }

                Logging.Logg().Debug(@"GroupSources::plugIn_OnEvtDataAskedHost (id=" + m_strID + @", key=" + (grpSgnls == null ? @"НЕ_ТРЕБУЕТСЯ" : grpSgnls.m_strID) + @") - " + msgDebugLog + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
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
                if (uLoader.FormMain.FileINI.GetIDIndex(grpSgnlsPars.m_strId) == id) {
                    grpSgnlsRes = grpSgnlsPars;

                    break;
                } else
                    ;

            return grpSgnlsRes;
        }
        /// <summary>
        /// Возвратить группу сигналов по целочисленному идентификатору
        /// </summary>
        /// <param name="id">Целочисленный идентификатор (целочисленное окончание строкового идентификатора)</param>
        /// <returns>Объект группы сигналов</returns>
        protected GroupSignals getGroupSignals(int id)
        {
            GroupSignals grpRes = null;

            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                if (uLoader.FormMain.FileINI.GetIDIndex(grpSgnls.m_strID) == id) {
                    grpRes = grpSgnls;

                    break;
                } else
                    ;

            return grpRes;
        }
        /// <summary>
        /// Возвратить "новое" состояние в ~ предыдущего состояния
        /// </summary>
        /// <param name="prevState">Предыдущее состояние</param>
        /// <param name="iRes">Результат выполнения функции</param>
        /// <returns>"Новое" состояние</returns>
        private static STATE getNewState(STATE prevState, out int iRes)
        {
            STATE stateRes = STATE.UNAVAILABLE;
            iRes = 0;

            if (prevState == STATE.UNAVAILABLE)
                iRes = -1;
            else {
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
                    if (itemGroupSignals.State == STATE.STOPPED) {
                        idGrpSgnls = FormMain.FileINI.GetIDIndex(itemGroupSignals.m_strID);

                        grpSgnlsPars = getGroupSignalsPars(idGrpSgnls);
                        if (grpSgnlsPars.m_iAutoStart == 1) {
                            stateChange(idGrpSgnls, STATE.STARTED);
                        } else
                            ;
                    } else
                        ;
            else
                ;
        }
        /// <summary>
        /// Выгрузить/загрузить библиотеку (объект класса)
        /// </summary>
        /// <returns>Признак выполнения функции</returns>
        public int Reload(bool bAbort)
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
                && (!(State == STATE.UNAVAILABLE))) {
                newState = getNewState(State, out iRes);

                //Изменить состояние ВСЕХ групп сигналов
                stateChange(newState);
            } else
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
                && (iRes == 0)) {
                if ((grpSgnls.State == STATE.STARTED)
                    && (!(State == STATE.STARTED)))
                    throw new Exception(@"GroupSources::StateChange (ID=" + iId + @") - несовместимые состояния групп источников и сигналов...");
                else
                    ;

                stateChange(iId, newState);
            } else {
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
        public int StateChange(string strId, STATE prevState = STATE.REVERSE)
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
        /// <summary>
        /// Изменить состояние группы сигналов
        /// </summary>
        /// <param name="iId">Идентификатор группы сигналов</param>
        /// <param name="newState">Новое состояние группы сигналов</param>
        private void stateChange(int iId, STATE newState)
        {
            bool bSync = false;

            if ((newState == STATE.STARTED)
                && (State == STATE.STOPPED))
                /*if (m_evtStartDo.WaitOne() == true)*/
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
            m_evtGroupSgnlsState.WaitOne();
            //Console.WriteLine(@"GroupSources::stateChange (iId=" + iId + @", newState=" + newState.ToString() + @") - m_evtGroupSgnlsState.WaitOne() ...");
        }
        /// <summary>
        /// Получить данные (результаты запроса в ~ режима) 'DataTable' по указанной группе сигналов
        /// </summary>
        /// <param name="id_grp_sgnls">Строковый идентификатор группы сигналов</param>
        /// <param name="bErr">Признак ошибки при выполнении функции</param>
        /// <returns>Таблица-результат</returns>
        public DataTable GetDataToPanel(string id_grp_sgnls, out bool bErr)
        {
            bErr = false;

            GroupSignals grpSgnls;
            DataTable tblRec = null
                , tblToPanel = null;
            DataRow[] arSel;
            object[] arObjToRow = null;
            TimeSpan tsToPanel = TimeSpan.Zero;
            HTimeSpan tsToData = HTimeSpan.NotValue
                , tsToQuery = HTimeSpan.NotValue;
            bool bIdLocalCalculated = false; // признак необходимости вычисления локального идентификатора строки (вычислений для Source - нет, для Dest - есть)
            int id_local = -1
                , id = -1;

            //Проверить длину идентификатора
            if (id_grp_sgnls.Length > 1) {//Только при длине > 1 м. определить целочисленное значение идентификатора
                grpSgnls = getGroupSignals(uLoader.FormMain.FileINI.GetIDIndex(id_grp_sgnls));
                if (!(grpSgnls == null))
                    if (grpSgnls.State == STATE.STARTED) {
                        tblRec = grpSgnls.m_tableData;
                        tblToPanel = new DataTable();

                        if (!(tblRec == null))
                            if (tblRec.Rows.Count > 0)
                                if ((!(tblRec.Columns.IndexOf(@"ID") < 0))
                                    && (!(tblRec.Columns.IndexOf(@"DATETIME") < 0))) {
                                    if (m_dictAdding.ContainsKey(@"OFFSET_UTC_TO_DATA") == true)
                                        tsToData = new HTimeSpan(m_dictAdding[@"OFFSET_UTC_TO_DATA"]);
                                    else
                                        ;
                                    if (m_dictAdding.ContainsKey(@"OFFSET_UTC_TO_QUERY") == true)
                                        tsToQuery = new HTimeSpan(m_dictAdding[@"OFFSET_UTC_TO_QUERY"]);
                                    else
                                        ;

                                    tsToPanel = tsToData == HTimeSpan.NotValue ? TimeSpan.Zero : -tsToData.Value;
                                    //tsToPanel += tsToQuery == HTimeSpan.NotValue ? TimeSpan.Zero : -tsToQuery.Value;

                                    //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - получено строк=" + tblRec.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                                    tblToPanel.Columns.AddRange(new DataColumn[] { new DataColumn (@"ID", typeof (int))
                                        , new DataColumn (@"NAME_SHR", typeof (string))
                                        , new DataColumn (@"VALUE", typeof (string)) //new DataColumn (@"VALUE", typeof (decimal))
                                        , new DataColumn (@"DATETIME", typeof (DateTime)) // new DataColumn (@"DATETIME", typeof (string))
                                        , new DataColumn (@"COUNT", typeof (string)) //new DataColumn (@"COUNT", typeof (int))
                                    });

                                    //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - добавлен диапазон столбцов...", Logging.INDEX_MESSAGE.NOT_SET);

                                    bIdLocalCalculated = this.GetType().Equals(typeof(GroupSourcesDest));

                                    //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - определено наименование поля идентификатора...", Logging.INDEX_MESSAGE.NOT_SET);

                                    foreach (SIGNAL_SRC sgnl in grpSgnls.m_listSgnls) {
                                        if (bIdLocalCalculated == false) {
                                            id =
                                            id_local =
                                                int.Parse(sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf(@"ID")]);
                                        } else {
                                            id = int.Parse(sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf(@"ID_SRC_SGNL")]);
                                            id_local = int.Parse(sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf(@"ID")]);
                                        }

                                        arSel = tblRec.Select(@"ID=" + id, @"DATETIME DESC");
                                        if (arSel.Length > 0)
                                            arObjToRow = new object[] { id_local
                                                , sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf (@"NAME_SHR")]
                                                , arSel[0][@"VALUE"]
                                                , ((DateTime)arSel[0][@"DATETIME"]).Add(tsToPanel)
                                                , arSel.Length };
                                        else
                                            arObjToRow = new object[] { id_local, sgnl.m_arSPars[grpSgnls.m_listSKeys.IndexOf(@"NAME_SHR")], string.Empty, DateTime.MinValue, string.Empty };

                                        tblToPanel.Rows.Add(arObjToRow);
                                    }

                                    //Logging.Logg().Debug(@"GroupSources::GetDataToPanel () - строк для отображения=" + tblToPanel.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                                } else
                                    throw new Exception(@"uLoader::GroupSources::GetDataToPanel () - ...");
                            else
                                ; //tblRec.Rows.Count == 0
                        else
                            //tblRec == null
                            Logging.Logg().Warning(@"GroupSources::GetDataToPanel (id=" + id_grp_sgnls + @") - таблица 'received' не существует...", Logging.INDEX_MESSAGE.NOT_SET);
                    } else
                        ; // группа сигналов не выполняется
                else
                    //grpSgnls == null
                    Logging.Logg().Warning(@"GroupSources::GetDataToPanel (id=" + id_grp_sgnls + @") - не найдена группа сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
            } else {//Нельзя определить целочисленный идентификатор - возвратить пустую таблицу
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
        public void AddDelegatePlugInOnEvtDataAskedHost(int indxGrpSrcDest, DelegateObjectFunc fOnEvt)
        {
            try {
                m_plugIns.Loader.EvtDataAskedHost += fOnEvt;
            } catch (Exception e) {
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
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"GroupSources::RemoveDelegatePlugInOnEvtDataAskedHost (IdTypePlugInObject=" + _iIdTypePlugInObjectLoaded + @") - ошибка обращения к объекту ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// Получить признак наличия группы сигналов среди списка присоединенных групп сигналов
        /// </summary>
        /// <param name="indx">Индекс (целочисленный идентификатор) группв сигналов</param>
        /// <returns>Признак наличия идентификатора группы сигналов</returns>
        public int ContainsIndexGroupSignals(int indx)
        {
            return getGroupSignals(indx) == null ? -1 : 0;
        }

        public string GetIdGroupSignals(int indx)
        {
            GroupSignals grpSgnls = getGroupSignals(indx);

            return grpSgnls == null ? string.Empty : grpSgnls.m_strID;
        }
    }
}
