using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// Наименование группы элементов
        /// </summary>
        public string m_strID;
        /// <summary>
        /// Массив ключей для словаря со значениями параметров
        /// </summary>
        public string[] m_keys;
    }
    /// <summary>
    /// Параметры сигнала в группе сигналов (источник, назначение)
    /// </summary>            
    public class SIGNAL_SRC
    {
        //Ключами для словаря являются 'ITEM_SRC.m_keys'
        public Dictionary<string, string> m_dictPars;

        public SIGNAL_SRC()
        {
            m_dictPars = new Dictionary<string, string>();
        }
    };
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
        /// Признак автоматического запуска опроса
        /// </summary>
        public int m_iAutoStart;
        /// <summary>
        /// Признак текущего режима работы
        /// </summary>
        public MODE_WORK m_mode;
        /// <summary>
        /// Массив параметров для всех (== COUNT_MODE_WORK) режимов работы
        /// </summary>
        public DATETIME_WORK[] m_arWorkIntervals;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC()
        {
            //Режим работы по умолчанию - текущий интервал
            m_mode = MODE_WORK.CUR_INTERVAL;
            m_arWorkIntervals = new DATETIME_WORK[(int)MODE_WORK.COUNT_MODE_WORK];
            for (int i = 0; i < (int)MODE_WORK.COUNT_MODE_WORK; i ++)
                m_arWorkIntervals[i] = new DATETIME_WORK();

            m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_dtStart = DateTime.Now;
            //Выровнять по текущей минуте
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.AddMilliseconds (-1 * m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Second * 1000 + m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Millisecond);
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
        public int m_iInterval;
        /// <summary>
        /// Начало интервала
        /// </summary>
        public DateTime m_dtStart;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        /// </summary>
        public TimeSpan m_tsPeriod;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DATETIME_WORK ()
        {
            m_iInterval = -1;
            m_dtStart = new DateTime ();
            m_tsPeriod = TimeSpan.FromSeconds (60);
        }
        /// <summary>
        /// Установить значения для интервала
        /// </summary>
        /// <param name="iInterval">Длительность (секунды)</param>
        /// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        public int Set (int iInterval)
        {
            int iRes = 0;

            return iRes;
        }
        /// <summary>
        /// Установить значения для интервала
        /// </summary>
        /// <param name="iInterval">Длительность (секунды)</param>
        /// <param name="dtBegin">Начало интервала</param>
        /// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        public int Set(int iInterval, DateTime dtBegin)
        {
            int iRes = 0;

            return iRes;
        }
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
        public List<ConnectionSettings> m_listConnSett;
        /// <summary>
        /// Наименование библиотеки для работы с группами сигналов
        /// </summary>
        public string m_strDLLName;
        /// <summary>
        /// Массив строк с наименованями "присоединенных" к группе источников групп сигналов
        /// </summary>
        public string []m_arIDGroupSignals;        
        /// <summary>
        /// Конструктор - основной (без парпаметров)
        /// </summary>
        public GROUP_SRC ()
        {
            m_IDCurrentConnSett = string.Empty;
            m_listConnSett = new List<ConnectionSettings> ();
            m_strDLLName = string.Empty;
            m_arIDGroupSignals = new string [] { };
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
        public enum STATE { UNAVAILABLE, STOPPED, STARTED }
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
                STATE stateRes = STATE.UNAVAILABLE;

                if (_iStateDLL == STATE_DLL.LOADED)
                {
                    stateRes = STATE.STOPPED;

                    foreach (GroupSignals grpSgnls in m_listGroupSignals)
                    {
                        if (grpSgnls.State == STATE.STARTED)
                        {
                            stateRes = STATE.STARTED;
                            break;
                        }
                        else
                            ;
                    }
                }
                else
                    ;

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

        /// <summary>
        /// Группа сигналов (для получения данных)
        /// </summary>
        private class GroupSignals : GROUP_SIGNALS_SRC
        {
            private System.Threading.Timer m_timerUpdate;

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

            public GroupSignals(GROUP_SIGNALS_SRC srcItem)
                : base()
            {
                this.m_arWorkIntervals = new DATETIME_WORK[srcItem.m_arWorkIntervals.Length];
                //??? Значения массивов независимы
                srcItem.m_arWorkIntervals.CopyTo(this.m_arWorkIntervals, 0);

                this.m_iAutoStart = srcItem.m_iAutoStart;

                this.m_keys = new string[srcItem.m_keys.Length];
                srcItem.m_keys.CopyTo(this.m_keys, 0);

                //??? Значения списка независимы
                this.m_listSgnls = srcItem.m_listSgnls;

                this.m_mode = srcItem.m_mode;

                this.m_strID = srcItem.m_strID;

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
                object[] arObjRes = new object[m_listSgnls.Count];

                int i = -1
                    , j = -1
                    , iVal = -1;

                for (i = 0; i < arObjRes.Length; i ++)
                {
                    arObjRes[i] = new object[m_keys.Length];

                    for (j = 0; j < m_keys.Length; j++)
                        if (Int32.TryParse(m_listSgnls[i].m_dictPars[m_keys[j]].ToString().Trim(), out iVal) == true)
                            (arObjRes[i] as object [])[j] = iVal;
                        else
                            (arObjRes[i] as object[])[j] = m_listSgnls[i].m_dictPars[m_keys[j]];
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
        List <GroupSignals> m_listGroupSignals;
        /// <summary>
        /// Возвратить массив состояний групп сигналов для группы источников
        /// </summary>
        public STATE[] GetStateGroupSignals ()
        {
            STATE []arRes = new STATE [m_listGroupSignals.Count];

            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                arRes[(int)m_listGroupSignals.IndexOf(grpSgnls)] = grpSgnls.State;

            return arRes;
        }
        /// <summary>
        /// Объект с загруженной библиотекой
        /// </summary>
        private IPlugIn m_plugIn;
        /// <summary>
        /// Событие для обмена данными с библиотекой
        /// </summary>
        private event DelegateObjectFunc EvtDataAskedHost; 
        /// <summary>
        /// Коструктор (с параметрами)
        /// </summary>
        /// <param name="srcItem">Объект с информациеий о группе источников</param>
        /// <param name="listGroupSignals">Список объектов с информацией о группах сигналов</param>
        public GroupSources (GROUP_SRC srcItem, List <GROUP_SIGNALS_SRC>listGroupSignals) : base ()
        {
            this.m_arIDGroupSignals = new string [srcItem.m_arIDGroupSignals.Length];
            srcItem.m_arIDGroupSignals.CopyTo(this.m_arIDGroupSignals, 0);

            m_listGroupSignals = new List<GroupSignals> ();
            foreach (GROUP_SIGNALS_SRC itemGroupSignals in listGroupSignals)
                m_listGroupSignals.Add(new GroupSignals(itemGroupSignals));

            this.m_keys = new string [srcItem.m_keys.Length];
            srcItem.m_keys.CopyTo(this.m_keys, 0);

            this.m_listConnSett = new List<ConnectionSettings> ();
            foreach (ConnectionSettings connSett in srcItem.m_listConnSett)
                //??? Значения списка независимы
                this.m_listConnSett.Add (connSett);

            this.m_strID = srcItem.m_strID;

            this.m_IDCurrentConnSett = srcItem.m_IDCurrentConnSett;

            this.m_strDLLName = srcItem.m_strDLLName;

            m_plugIn = loadPlugIn(out _iStateDLL);
            if (!(_iStateDLL == STATE_DLL.LOADED))
                throw new Exception(@"GroupSources::GroupSources () - ...");
            else
                ;

            sendInitConnSett();

            foreach (GroupSignals itemGroupSignals in m_listGroupSignals)
            {
                itemGroupSignals.State = STATE.STOPPED;
                sendInitGroupSignals (FormMain.FileINI.GetIDIndex(itemGroupSignals.m_strID));
            }
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
                    EvtDataAskedHost += (plugInRes as PlugInBase).OnEvtDataRecievedHost;

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

        private void sendInitConnSett ()
        {
            PerformDataAskedHost(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT, new object[] { this.m_listConnSett[FormMain.FileINI.GetIDIndex(m_IDCurrentConnSett)] }));
        }

        protected void PerformDataAskedHost (EventArgsDataHost ev)
        {
            EvtDataAskedHost (ev);
        }

        private void sendInitGroupSignals (int iIDGroupSignals)
        {
            GroupSignals grpSgnls = getGroupSignals(iIDGroupSignals);
            object[] arToDataHost = grpSgnls.Pack ();            
            //Отправить данные для инициализации
            PerformDataAskedHost(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP, new object[] { iIDGroupSignals, arToDataHost }));
        }

        private void sendState(int iIDGroupSignals, STATE state)
        {
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

            GroupSignals grpSgnls = getGroupSignals (iIDGroupSignals);
            object []arDataAskedHost = null;

            if (idToSend == ID_DATA_ASKED_HOST.START)
                arDataAskedHost = new object[]
                    {
                        iIDGroupSignals
                        , new object[]
                        {
                            grpSgnls.m_mode
                            , grpSgnls.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart
                            , grpSgnls.m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_tsPeriod.TotalSeconds
                            , grpSgnls.m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_iInterval
                        }
                    };
            else
                arDataAskedHost = new object[]
                    {
                        iIDGroupSignals
                    };

            PerformDataAskedHost(new EventArgsDataHost((int)idToSend, arDataAskedHost));
        }

        /// <summary>
        /// Обработка сообщений "от" библиотеки
        /// </summary>
        /// <param name="obj"></param>
        private void plugIn_OnEvtDataAskedHost  (object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            int iIDGroupSignals = 0; //??? д.б. указана в "запросе"
            object []pars = (ev.par as object[])[0] as object [];

            switch ((ID_DATA_ASKED_HOST)pars[0])
            {
                case ID_DATA_ASKED_HOST.INIT_CONN_SETT: //Получен запрос на парметры инициализации                    
                    //Отправить данные для инициализации
                    sendInitConnSett ();
                    if (! (m_listGroupSignals[iIDGroupSignals].State == STATE.UNAVAILABLE))
                        sendState(iIDGroupSignals, m_listGroupSignals[iIDGroupSignals].State);
                    else
                        ;
                    break;
                case ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP: //Получен запрос на обрабатываемую группу сигналов
                    sendInitGroupSignals(iIDGroupSignals);
                    if (! (m_listGroupSignals[iIDGroupSignals].State == STATE.UNAVAILABLE))
                        sendState(iIDGroupSignals, m_listGroupSignals[iIDGroupSignals].State);
                    else
                        ;
                    break;
                case ID_DATA_ASKED_HOST.TABLE_RES:
                    iIDGroupSignals = (int)pars[1];
                    m_listGroupSignals[iIDGroupSignals].m_tableData = (pars[2] as DataTable).Copy();                    
                    break;
                case ID_DATA_ASKED_HOST.ERROR:
                    iIDGroupSignals = (int)pars[1];
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Возвратить группу сигналов по целочисленному идентификатору
        /// </summary>
        /// <param name="id">Целочисленный идентификатор (целочисленное окончание строкового идентификатора)</param>
        /// <returns>Объект группы сигналов</returns>
        private GroupSignals getGroupSignals (int id)
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
        /// Остановить/запустить группу сигналов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int StateChange (int id)
        {
            int iRes = 0;
            //Проверить параметр - идентификатор
            if (! (id < 0))
            {//Изменить состояние только ОДНой группы сигналов
                GroupSignals grpSgnls = getGroupSignals(id);
                iRes = grpSgnls.StateChange ();

                sendState (id, grpSgnls.State);
            }
            else
            {//Изменить состояние ВСЕХ групп сигналов
                STATE newState = getNewState (State, out iRes);
                
                foreach (GroupSignals grpSgnls in m_listGroupSignals)
                    //Проверить текцщее стостояние
                    if (!(grpSgnls.State == newState))
                    {
                        //Изменить только, если "другое"
                        grpSgnls.StateChange(newState);

                        sendState(FormMain.FileINI.GetIDIndex(grpSgnls.m_strID), grpSgnls.State);
                    }
                    else
                        ;
            }

            return iRes;
        }
        /// <summary>
        /// Получить данные (результаты запроса в ~ режима) 'DataTable' по указанной группе сигналов
        /// </summary>
        /// <param name="id">Строковый идентификатор группы сигналов</param>
        /// <param name="bErr">Признак ошибки при выполнении функции</param>
        /// <returns>Таблица-результат</returns>
        public DataTable GetData(string id, out bool bErr)
        {
            int iID = -1;            
            bErr = false;
            //Проверить длину идентификатора
            if (id.Length > 1)
            {//Только при длине > 1 м. определить целочисленное значение идентификатора
                iID = uLoader.FormMain.FileINI.GetIDIndex(id);

                return getGroupSignals (iID).m_tableData;
            }
            else
            {//Нельзя определить целочисленный идентификатор - возвратить пустую таблицу
                //bErr = true;

                return new DataTable();
            }
        }
        /// <summary>
        /// Остановить группу источников
        /// </summary>
        /// <returns></returns>
        public int Stop()
        {
            int iRes = 0;
            
            foreach (GroupSignals grpSgnls in m_listGroupSignals)
                sendState(FormMain.FileINI.GetIDIndex(grpSgnls.m_strID), STATE.STOPPED);

            return iRes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fOnEvt">Функция обработки</param>
        public void AddDelegatePlugInOnEvtDataAskedHost (DelegateObjectFunc fOnEvt)
        {
            (m_plugIn as PlugInBase).EvtDataAskedHost += fOnEvt;
        }        
    }

    public class GroupSourcesDest : GroupSources
    {
        public GroupSourcesDest(GROUP_SRC grpSrc, List<GROUP_SIGNALS_SRC> listGrpSgnls)
            : base(grpSrc, listGrpSgnls)
        {
        }
        /// <summary>
        /// Получает сообщения от библиотеки из "другого" (источника) объекта
        /// </summary>
        /// <param name="obj"></param>
        public void Clone_OnEvtDataAskedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            int iIDGroupSignals = 0; //??? д.б. указана в "запросе"
            object[] pars = (ev.par as object[])[0] as object[];

            switch ((ID_DATA_ASKED_HOST)pars[0])
            {
                case ID_DATA_ASKED_HOST.INIT_CONN_SETT: //Получен запрос на парметры инициализации
                    break;
                case ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP: //Получен запрос на обрабатываемую группу сигналов
                    break;
                case ID_DATA_ASKED_HOST.TABLE_RES:
                    //pars[1] - идентификатор группы сигналов
                    //pars[2] - таблица с данными для "вставки"
                    PerformDataAskedHost(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.TO_INSERT, new object[] { (int)pars[1], (pars[2] as DataTable).Copy() }));
                    break;
                case ID_DATA_ASKED_HOST.ERROR:
                    iIDGroupSignals = (int)pars[1];
                    break;
                default:
                    break;
            }
        }
    }
}