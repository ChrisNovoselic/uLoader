using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace xmlLoader
{
    public partial class WriterHandlerQueue : HClassLibrary.HHandlerQueue
    {
        /// <summary>
        /// Структура для хранения значений настраиваемых параметров
        ///  (из файла конфигурации)
        /// </summary>
        public struct OPTION
        {
            /// <summary>
            /// Количество одновременно отображаемых наборов
            /// </summary>
            public int COUNT_VIEW_ITEM;
            /// <summary>
            /// Интервал времени, в течение которого в ОЗУ удерживаются наборы
            ///  , полученные для сохранения в БД
            /// </summary>
            public TimeSpan TS_HISTORY_RUNTIME;
        }
        /// <summary>
        /// Объект со значениями настраиваемых параметров
        /// </summary>
        private static OPTION s_Option;
        /// <summary>
        /// Класс для хранения информации о параметрах соединения с БД
        ///  (только для добавления параметра AUTO_START)
        /// </summary>
        public class ConnectionSettings : HClassLibrary.ConnectionSettings
        {
            /// <summary>
            /// Перечисление - индексы для известных параметров соединения
            ///  , доступ к 'Items'
            /// </summary>
            public enum INDEX_ITEM {
                AUTO_START, TURN
                , ID, NAME_SHR, SERVER, INSTANCE, NPORT, DB_NAME, UID, PSWD
            }
            /// <summary>
            /// Список со значениями параметров соединения с БД
            /// </summary>
            public List<object> Items;
            /// <summary>
            /// Инициализация значений параметров
            /// </summary>
            /// <param name="bAutoStart">Значение для параметра AUTO_START</param>
            private void initialize(bool bAutoStart)
            {
                Items = new List<object>() {
                    bAutoStart, false
                    , base.id, base.name, base.server, base.instance, base.port, base.dbName, base.userName, base.password
                };
            }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="bAutoStart">Значение для параметра AUTO_START</param>
            public ConnectionSettings(bool bAutoStart) : base()
            {
                initialize(bAutoStart);
            }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="bAutoStart">Значение для параметра AUTO_START</param>
            /// <param name="connSett">Объект базового класса</param>
            public ConnectionSettings(bool bAutoStart, HClassLibrary.ConnectionSettings connSett) : base(connSett)
            {
                initialize(bAutoStart);
            }
            /// <summary>
            /// Конструктор - дополнительный (с параметрами)
            /// </summary>
            /// <param name="bAutoStart">Значение для параметра AUTO_START</param>
            /// <param name="nameConn">Наименовнаие соединения</param>
            /// <param name="srv">IP-адрес, хост-имя сервера(СУБД)</param>
            /// <param name="instance">Наименование экземпляра MSSQLServer, при необходимости</param>
            /// <param name="port">№ порта взаимодействия с СУБД</param>
            /// <param name="dbName">Наименование БД</param>
            /// <param name="uid">Пользователь для подключения к БД</param>
            /// <param name="pswd">для подключения к БД</param>
            public ConnectionSettings(bool bAutoStart, string nameConn, string srv, string instance, int port, string dbName, string uid, string pswd)
                : base(nameConn, srv, instance, port, dbName, uid, pswd)
            {
                initialize(bAutoStart);
            }
            /// <summary>
            /// Конструктор - дополнительный (с параметрами)
            /// </summary>
            /// <param name="bAutoStart">Значение для параметра AUTO_START</param>
            /// <param name="id">Идентификатор источника данных</param>
            /// <param name="nameConn">Наименовнаие соединения</param>
            /// <param name="srv">IP-адрес, хост-имя сервера(СУБД)</param>
            /// <param name="instance">Наименование экземпляра MSSQLServer, при необходимости</param>
            /// <param name="port">№ порта взаимодействия с СУБД</param>
            /// <param name="dbName">Наименование БД</param>
            /// <param name="uid">Пользователь для подключения к БД</param>
            /// <param name="pswd">для подключения к БД</param>
            public ConnectionSettings(bool bAutoStart, int id, string nameConn, string srv, string instance, int port, string dbName, string uid, string pswd)
                : base(id, nameConn, srv, instance, port, dbName, uid, pswd)
            {
                initialize(bAutoStart);
            }
        }
        /// <summary>
        /// Перечисление - возможные состояния для обработки
        /// </summary>
        public enum StatesMachine
        {
            UNKNOWN = -1
            , NEW = 200// получен новый пакет
            , LIST_DEST // параметры соединения с БД
            , LIST_DATASET // запрос для получения списка пакетов
            , DATASET_CONTENT // запрос для получения пакета
            //, STATISTIC
            , CONNSET_USE_CHANGED // изменилось состояние источника данных отправлять/запретить отправку наборов
            , OPTION // установить значения настраиваемых параметров
            , MESSAGE_TO_STATUSSTRIP // сообщение для вывода в строку статуса главной формы
        }
        /// <summary>
        /// Событие для отправки сообщения главной экранной форме
        ///  (в последующем для постановки(ретрансляции) в главную очередь обработки событий)
        /// </summary>
        public event DelegateObjectFunc EvtToFormMain;
        /// <summary>
        /// Объект для взаимодействия с БД(выполнение запросов - сохранение данных)
        /// </summary>
        private WriterDbHandler _writer;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public WriterHandlerQueue()
        {
            _listDataSet = new List<DATASET>();
            // создать объект взаимодействия с БД, подписаться на событие - результат выполнения запросов
            _writer = new WriterDbHandler();
            _writer.EvtDataAskedHost += new DelegateObjectFunc(writerDbHandler_OnDataAskedHost);
        }
        /// <summary>
        /// Инициализация объекта взаимодействия с БД
        ///  (только после получения значений параметров соединения с БД)
        /// </summary>
        /// <param name="listConnSett">Список объектов со значениями параметров соединения с БД</param>
        private void initialize(List<ConnectionSettings>listConnSett)
        {
            _writer.Initialize(listConnSett);
        }
        /// <summary>
        /// Останов (полный) очереди обработки событий по сохранению наборов в целевой БД
        /// </summary>
        public override void Stop()
        {
            _writer.Activate(false); _writer.Stop();

            base.Stop();
        }
        /// <summary>
        /// Структура для хранения статистических данных
        /// </summary>
        public struct STATISTIC
        {
            /// <summary>
            /// Перечисление - индексы статистических параметров
            /// </summary>
            public enum INDEX_ITEM {
                DATETIME_DATASET_LAST_RECIEVED
                , LENGTH_DATASET_LAST_RECIEVED
                , COUNT_DATASET_RECIEVED
                , COUNT_DATASET_QUERED
            }
            /// <summary>
            /// Установить значение параметру
            /// </summary>
            /// <param name="indx">Индекс параметра</param>
            /// <param name="value">Значение параметра</param>
            public void SetAt(INDEX_ITEM indx, object value)
            {
            }
            /// <summary>
            /// Метод увеличения счетчика выполненных операций
            /// </summary>
            /// <param name="indx">Индекс параметра-счетчика</param>
            public void Counter(INDEX_ITEM indx)
            {
            }
        }
        /// <summary>
        /// Объект со статистическими данными
        /// </summary>
        public static STATISTIC s_Statistic;
        /// <summary>
        /// Структура для хранения информации по одному набору - результату обработки XML-пакета
        /// </summary>
        private struct DATASET
        {
            /// <summary>
            /// Перечисление - индексы известных состояний набора
            /// </summary>
            public enum STATE : short { UNKNOWN = -1
                , NEW, QUERING, QUERED, ERROR
            }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="keys">Перечень идентификаторов источников данных</param>
            /// <param name="dtRecieved">Метка даты/времени отправления XML-пакета для сохранения</param>
            /// <param name="tableValues">Результат обработки XML-пакета - таблица со значенями</param>
            /// <param name="tableParameters">Результат обработки XML-пакета - таблица с параметрами</param>
            public DATASET(IEnumerable<int>keys, DateTime dtRecieved, DataTable tableValues, DataTable tableParameters)
            {
                m_values = string.Empty;

                m_dtRecieved = dtRecieved;

                m_dictDatetimeQuered = new Dictionary<int, DateTime>();
                foreach (int key in keys)
                    m_dictDatetimeQuered.Add(key, DateTime.MinValue);

                m_state = STATE.NEW;

                m_tableValues = tableValues.Copy();

                m_tableParameters = tableParameters.Copy();

                if (m_tableValues.Rows.Count > 0) {
                    foreach (DataRow r in m_tableValues.Rows) {
                        m_values += string.Format(@"('{0}','{1}',{2}),"
                            , r[@"XML_SECTION_NAME"]
                            , r[@"XML_ITEM_NAME"]
                            , r[@"VALUE"]
                        );
                    }

                    if (m_values.Length > 0) {
                    // удалить лишний символ, установить новое состояние
                        m_values = m_values.Substring(0, m_values.Length - 1);

                        m_state = STATE.QUERING;
                    } else
                    // состояние набора остается прежним 'NEW'
                        ;
                } else
                // состояние набора остается прежним 'NEW'
                    ;
            }
            /// <summary>
            /// Метка даты/времени отправления XML-пакета для обработки(сохранения в БД)
            /// </summary>
            public DateTime m_dtRecieved;
            /// <summary>
            /// Словарь с метками даты/времени для каждого из источников данных
            /// </summary>
            public Dictionary<int, DateTime> m_dictDatetimeQuered;
            /// <summary>
            /// Результат обработки XML-пакета - таблица со значенями
            /// </summary>
            public DataTable m_tableValues;
            /// <summary>
            /// Результат обработки XML-пакета - таблица с параметрами
            /// </summary>
            public DataTable m_tableParameters;
            /// <summary>
            /// Текущее состояние набора данных
            /// </summary>
            public STATE m_state;
            /// <summary>
            /// Значения в виде строки, подготовленной для вставки в строку запроса по сохранению данных
            /// </summary>
            public string m_values;
        }
        /// <summary>
        /// Список наборов данных принятых к обработке
        ///  , и не утативших актуальность в связи с настаиваемыми параметрами
        /// </summary>
        private List<DATASET> _listDataSet;
        /// <summary>
        /// Добавить набор в список наборов, принятых к обработке
        /// </summary>
        /// <param name="keys">Список идентификаторов источников данных</param>
        /// <param name="dtDataSet">Метка даты/времени отправления XML-пакета для обработки(сохранения в БД)</param>
        /// <param name="values">Результат обработки XML-пакета - таблица со значенями</param>
        /// <param name="parameters">Результат обработки XML-пакета - таблица с параметрами</param>
        /// <returns>Результат выполнения операции</returns>
        private int addDataSet(IEnumerable<int>keys, DateTime dtDataSet, DataTable values, DataTable parameters)
        {
            int iRes = 0;

            DATASET dataSet;
            // определить лимит даты/времени хранения пакетов времени выполнения
            DateTime dtLimit = dtDataSet - s_Option.TS_HISTORY_RUNTIME;
            //список индексов элементов(пакетов) для удаления
            List<int> listIndxToRemove = new List<int>();
            for (int i = 0; i < _listDataSet.Count; i++)
                if ((dtLimit - _listDataSet[i].m_dtRecieved).TotalSeconds > 0)
                    listIndxToRemove.Add(i);
                else
                    ;
            // удалить пакеты дата/время получения которых больше, чем "лимит"
            listIndxToRemove.ForEach(indx => {
                Logging.Logg().Debug(MethodBase.GetCurrentMethod(), string.Format(@"удален набор [{0}]", _listDataSet[indx].m_dtRecieved), Logging.INDEX_MESSAGE.NOT_SET);

                _listDataSet.RemoveAt(indx);
            });
            // добавить текущий пакет (даже, если он не удовлетворяет критерию "лимит")
            try {
                _listDataSet.Add(dataSet = new DATASET(keys, dtDataSet, values, parameters));

                s_Statistic.SetAt(STATISTIC.INDEX_ITEM.DATETIME_DATASET_LAST_RECIEVED, dataSet.m_dtRecieved);
                s_Statistic.SetAt(STATISTIC.INDEX_ITEM.LENGTH_DATASET_LAST_RECIEVED, dataSet.m_tableValues.Rows.Count);
                s_Statistic.Counter(STATISTIC.INDEX_ITEM.COUNT_DATASET_RECIEVED);
                if (dataSet.m_state == DATASET.STATE.QUERED)
                    s_Statistic.Counter(STATISTIC.INDEX_ITEM.COUNT_DATASET_QUERED);
                else
                    ;
            } catch (Exception e) {
                iRes = -1;

                Logging.Logg().Exception(e, string.Format(@"Добавление набора дата/время получения={0} и статистики для него", dtDataSet), Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }
        /// <summary>
        /// Возвратить список наборов для отображения на главной форме в соответствии с 
        /// </summary>
        /// <param name="key">Идентификатор источника данных(установлен автоматически или пользователем)</param>
        /// <returns>Список информации о наброах к отображению</returns>
        private List<FormMain.VIEW_ITEM> getListViewDataSetItem(int key)
        {
            List<FormMain.VIEW_ITEM> listRes = new List<FormMain.VIEW_ITEM>();

            (from dataSet in _listDataSet
                orderby dataSet.m_dtRecieved //descending
                select new FormMain.VIEW_ITEM {
                    Values = new object[] {
                        dataSet.m_tableValues.Rows.Count
                        , dataSet.m_dtRecieved
                        , dataSet.m_dictDatetimeQuered[key]
                    }
                }
            ).Take(s_Option.COUNT_VIEW_ITEM).ToList().ForEach(item => listRes.Add(item));

            return listRes;
        }
        /// <summary>
        /// Обработчик событий от объекта взаимодействия с БД
        /// </summary>
        /// <param name="obj">Объект с идентификатором события, аргументами события (при необходимости)</param>
        private void writerDbHandler_OnDataAskedHost(object obj)
        {
            INDEX_WAITHANDLE_REASON indxReasonCompleted = (INDEX_WAITHANDLE_REASON)(obj as object[])[0];
            int idConnSett = (int)(obj as object[])[1];
            DateTime dtRecieved = (DateTime)(obj as object[])[2];

            try {
                var writerDataSet = (from dataSet in _listDataSet where dataSet.m_dtRecieved == dtRecieved select dataSet).ElementAt(0);
                writerDataSet.m_dictDatetimeQuered[idConnSett] =
                    indxReasonCompleted == INDEX_WAITHANDLE_REASON.SUCCESS ? DateTime.UtcNow :
                        indxReasonCompleted == INDEX_WAITHANDLE_REASON.ERROR ? DateTime.MaxValue :
                            DateTime.MinValue;

                if (indxReasonCompleted == INDEX_WAITHANDLE_REASON.ERROR)
                    EvtToFormMain?.Invoke(new object[] {
                        StatesMachine.MESSAGE_TO_STATUSSTRIP
                        , FormMain.StatusStrip.STATE.Error
                        , string.Format(@"WriterHandlerQueue - ошибка при сохранении значений для источника={0} за {1}", idConnSett, dtRecieved)
                    });
                else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e
                    , string.Format(@"не найден набор для IdConnSett={0}, [{1}]", idConnSett, dtRecieved.ToString())
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// Реализация очереди обработки событий - получение данных
        /// </summary>
        /// <param name="s">Идентификатор события</param>
        /// <param name="error">Признак ошибки при выполнении</param>
        /// <param name="outobj">Объект с полученными данными</param>
        /// <returns>Результат выполнения метода</returns>
        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;

            DATASET dataSet;
            StatesMachine state = (StatesMachine)s;
            string debugMsg = string.Empty;

            error = true;
            outobj = null;

            ItemQueue itemQueue = null;

            try {
                switch (state) {
                    case StatesMachine.NEW: // 
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        error = (iRes = addDataSet(_writer.ListConnSettKey, (DateTime)itemQueue.Pars[0], itemQueue.Pars[1] as DataTable, itemQueue.Pars[2] as DataTable)) < 0 ? true : false; ;

                        debugMsg = string.Format(@"Добавление нового набора [{0}]...", (DateTime)itemQueue.Pars[0]);

                        if (error == false) {
                        // добавленный набор поставить в очередь на запись
                            outobj = new DATASET_WRITER() {
                                 m_dtRecieved = _listDataSet[_listDataSet.Count - 1].m_dtRecieved
                                , m_values = _listDataSet[_listDataSet.Count - 1].m_values
                            };

                            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
                        } else
                            Logging.Logg().Error(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                    case StatesMachine.LIST_DEST: // получен спискок объектов с парметрами соединения с БД
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
                        // инициализация переданными значениями
                        initialize(itemQueue.Pars[0] as List<WriterHandlerQueue.ConnectionSettings>);
                        // установка соединения с источниками данных
                        _writer.StartDbInterfaces();
                        // обеспечить готовность к приему событий (TSQL-запросов для выполнения)
                        _writer.Start(); _writer.Activate(true);
                        break;
                    case StatesMachine.LIST_DATASET: // запрос на формирование актуального списка переданных для записи наборов
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        if (!((int)itemQueue.Pars[0] < 0))
                            outobj = getListViewDataSetItem((int)itemQueue.Pars[0]);
                        else
                            ;
                        break;
                    case StatesMachine.DATASET_CONTENT: // запрос на передачу содержимого набора, выбранного пользователем для отображения
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        var selectDataSets = from l in _listDataSet where l.m_dtRecieved == (DateTime)itemQueue.Pars[0] select l;
                        if (selectDataSets.Count() == 1) {
                            dataSet = selectDataSets.ElementAt(0);

                            switch ((FormMain.INDEX_CONTROL)itemQueue.Pars[1]) {
                                case FormMain.INDEX_CONTROL.TABPAGE_VIEW_DATASET_TABLE_VALUE:
                                    outobj = dataSet.m_tableValues;
                                    break;
                                case FormMain.INDEX_CONTROL.TABPAGE_VIEW_DATASET_TABLE_PARAMETER:
                                    outobj = dataSet.m_tableParameters;
                                    break;
                                default: //??? - ошибка неизвестный тип вкладки просмотра набора
                                    break;
                            }
                        } else
                            ; //??? - ошибка пакет не найден либо пакетов много
                        break;
                    //case StatesMachine.STATISTIC: // 
                    //    iRes = 0;
                    //    error = false;

                    //    itemQueue = Peek;
                    //    break;
                    case StatesMachine.CONNSET_USE_CHANGED:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
                        _writer.ChangeConnSettUse((int)itemQueue.Pars[0]);
                        break;
                    case StatesMachine.OPTION:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
                        s_Option = (OPTION)itemQueue.Pars[0];

                        //outobj = ??? только в одну сторону: форма -> handler
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"WriteHandlerQueue::StateCheckResponse (state=" + state.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

                error = true;
                iRes = -1 * (int)state;
            }

            return iRes;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            EvtToFormMain?.Invoke(new object[] {
                StatesMachine.MESSAGE_TO_STATUSSTRIP
                , FormMain.StatusStrip.STATE.Error
                , string.Format(@"WriterHandlerQueue - обработка события {0}", ((StatesMachine)state).ToString())
            });

            switch ((StatesMachine)state) {
                default:
                    break;
            }

            Logging.Logg().Error(string.Format(@"WriterHandlerQueue::StateErrors () - не обработана ошибка [{0}, REQ={1}, RES={2}] ..."
                    , ((StatesMachine)state).ToString(), req, res)
                , Logging.INDEX_MESSAGE.NOT_SET);

            return HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch ((StatesMachine)state)
            {
                case StatesMachine.NEW: // 
                case StatesMachine.LIST_DEST:
                case StatesMachine.LIST_DATASET: //
                case StatesMachine.DATASET_CONTENT: //
                //case StatesMachine.STATISTIC: //
                case StatesMachine.CONNSET_USE_CHANGED: // 
                case StatesMachine.OPTION: //
                case StatesMachine.MESSAGE_TO_STATUSSTRIP: //
                    // не требуют запроса
                default:
                    break;
            }

            return iRes;
        }
        /// <summary>
        /// Структура для взаимодействия между очередью обработки событий и объектом взаимодействия с БД
        /// </summary>
        private struct DATASET_WRITER
        {
            /// <summary>
            /// Метка даты/времени набопа данных
            /// </summary>
            public DateTime m_dtRecieved;
            /// <summary>
            /// Значения в виде строки, подготовленной для вставки в строку запроса по сохранению данных
            /// </summary>
            public string m_values;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            EvtToFormMain?.Invoke(new object[] {
                StatesMachine.MESSAGE_TO_STATUSSTRIP
                , FormMain.StatusStrip.STATE.Action
                , string.Format(@"WriterHandlerQueue - обработка события {0}", ((StatesMachine)state).ToString())
            });

            switch ((StatesMachine)state) {
                case StatesMachine.NEW: // самый старый набор для постановки в очередь на запись, 
                    _writer.Request((DATASET_WRITER)obj);
                    break;
                case StatesMachine.LIST_DEST: // получены параметры соединения БД, ответа не требуется
                case StatesMachine.CONNSET_USE_CHANGED:
                case StatesMachine.OPTION: //
                    //Ответа не требуется/не требуют обработки результата
                    break;
                case StatesMachine.LIST_DATASET: // отправить главному окну(для отображения) информацию о поученных/обработанных наборах с данными
                case StatesMachine.DATASET_CONTENT: // отправить главному окну(для отображения) информацию о выбранном для отображения в главном окне
                //case StatesMachine.STATISTIC: // статический объект (obj = null)
                    if ((!(itemQueue == null))
                        //&& (!(itemQueue.m_dataHostRecieved == null)) FormMain не реализует интерфейс 'IDataHost'
                        )
                        // вариант для объекта с интерфейсом 'IDataHost'
                        //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj })
                        EvtToFormMain?.Invoke(new object[] { state, obj })
                        ;
                    else
                        ;
                    break;
                default:
                    break;
            }

            //Debug.WriteLine(string.Format(@"WriterHandlerQueue::StateResponse(state={0}) - ...", ((StatesMachine)state).ToString()));

            return iRes;
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(string.Format(@"WriterHandlerQueue::StateWarnings () - не обработано предупреждение [{0}, REQ={1}, RES={2}] ..."
                    , ((StatesMachine)state).ToString(), req, res)
                , Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
