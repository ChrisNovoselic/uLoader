using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Data;
using System.Reflection;

namespace xmlLoader
{
    public class WriterHandlerQueue : HClassLibrary.HHandlerQueue
    {
        private class WriterDbHandler : HHandlerDb
        {
            private List<ConnectionSettings> m_listConnSett;

            public void Initialize(List<ConnectionSettings>listConnSett)
            {
                m_listConnSett = listConnSett;
            }

            public override void ClearValues()
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Старт потоков для обмена данными с источниками информации
            /// </summary>
            public override void StartDbInterfaces()
            {
                foreach (ConnectionSettings connSett in m_listConnSett)
                    register(connSett.id, 0, connSett, string.Empty);
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

                if (bReq == true) {
                    base.register(id, indx, connSett, name);

                    Logging.Logg().Debug(
                        string.Format(@"WriterDbHandler::register ({0}) iListenerId={1}, идентификаторов_источников={2}", id, m_dictIdListeners[id][indx], m_dictIdListeners.Count)
                        , Logging.INDEX_MESSAGE.NOT_SET
                    );
                }
                else
                    ;
            }

            private int IdConnSettCurrent { get; set; }

            private string Query { get; set; }

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

            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                HHandler.INDEX_WAITHANDLE_REASON resReason = INDEX_WAITHANDLE_REASON.SUCCESS;

                Logging.Logg().Error(
                    string.Format(@"WriterDbHandler::StateErrors (state={0}, req={1}, res={2}) - ...", ((StatesMachine)state).ToString(), req, res)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                return resReason;
            }

            protected override int StateRequest(int state)
            {
                int iRes = 0;

                if (Query.Equals(string.Empty) == false)
                    Request(m_dictIdListeners[IdConnSettCurrent][0], Query);
                else
                    ;

                return iRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                // ответ не требуется
                return 0;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                Logging.Logg().Warning(
                    string.Format(@"WriterDbHandler::StateWarnings (state={0}, req={1}, res={2}) - ...", ((StatesMachine)state).ToString(), req, res)
                    , Logging.INDEX_MESSAGE.NOT_SET);
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
            , STATISTIC
        }
        /// <summary>
        /// Событие для отправки сообщения главной экранной форме
        ///  (в последующем для постановки(ретрансляции) в главную очередь обработки событий)
        /// </summary>
        public event DelegateObjectFunc EvtToFormMain;

        private WriterDbHandler _writer;

        public WriterHandlerQueue()
        {
            _listDataSet = new List<DATASET>();

            _writer = new WriterDbHandler();
        }

        private void initialize(List<ConnectionSettings>listConnSett)
        {
            _writer.Initialize(listConnSett);
        }

        public override void Stop()
        {
            _writer.Activate(false); _writer.Stop();

            base.Stop();
        }

        public struct STATISTIC
        {
            public enum INDEX_ITEM {
                DATETIME_DATASET_LAST_RECIEVED
                , LENGTH_DATASET_LAST_RECIEVED
                , COUNT_DATASET_RECIEVED
                , COUNT_DATASET_QUERED
            }

            public void SetAt(INDEX_ITEM indx, object value)
            {
            }

            public void Counter(INDEX_ITEM indx)
            {
            }
        }

        public static STATISTIC s_Statistic;

        private static TimeSpan TS_HISTORY_RUNTIME = TimeSpan.FromSeconds(5 * 60);

        private struct DATASET
        {
            public enum STATE : short { UNKNOWN = -1
                , NEW, QUERING, QUERED, ERROR
            }

            public DATASET(DateTime dtRecieved, DataTable tableValues, DataTable tableParameters)
            {
                m_dtRecieved = dtRecieved;

                m_dtQuered = DateTime.MinValue;

                m_state = STATE.NEW;

                m_tableValues = tableValues.Copy();

                m_tableParameters = tableParameters.Copy();

                m_query = string.Empty;

                m_state = STATE.QUERING;
            }

            public DateTime m_dtRecieved;

            public DateTime m_dtQuered;

            public DataTable m_tableValues;

            public DataTable m_tableParameters;

            public STATE m_state;

            public string m_query;
        }

        private List<DATASET> _listDataSet;

        private void addDataSet(DateTime dtDataSet, DataTable values, DataTable parameters)
        {
            DATASET dataSet;
            // определить лимит даты/времени хранения пакетов времени выполнения
            DateTime dtLimit = dtDataSet - TS_HISTORY_RUNTIME;
            //список индексов элементов(пакетов) для удаления
            List<int> listIndxToRemove = new List<int>();
            for (int i = 0; i < _listDataSet.Count; i++)
                if ((dtLimit - _listDataSet[i].m_dtRecieved).TotalSeconds > 0)
                    listIndxToRemove.Add(i);
                else
                    ;
            // удалить пакеты дата/время получения которых больше, чем "лимит"
            listIndxToRemove.ForEach(indx => {
                Logging.Logg().Debug(MethodBase.GetCurrentMethod(), string.Format(@"удален пакет {0}", _listDataSet[indx].m_dtRecieved), Logging.INDEX_MESSAGE.NOT_SET);

                _listDataSet.RemoveAt(indx);
            });
            // добавить текущий пакет (даже, если он не удовлетворяет критерию "лимит")
            try {
                _listDataSet.Add(dataSet = new DATASET(dtDataSet, values, parameters));

                s_Statistic.SetAt(STATISTIC.INDEX_ITEM.DATETIME_DATASET_LAST_RECIEVED, dataSet.m_dtRecieved);
                s_Statistic.SetAt(STATISTIC.INDEX_ITEM.LENGTH_DATASET_LAST_RECIEVED, dataSet.m_tableValues.Rows.Count);
                s_Statistic.Counter(STATISTIC.INDEX_ITEM.COUNT_DATASET_RECIEVED);
                if (dataSet.m_state == DATASET.STATE.QUERED)
                    s_Statistic.Counter(STATISTIC.INDEX_ITEM.COUNT_DATASET_QUERED);
                else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"Добавление набора дата/время получения={0} и статистики для него", dtDataSet), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private static int COUNT_VIEW_DATASET_ITEM = 6;

        private List<FormMain.VIEW_ITEM> listViewDataSetItem
        {
            get {
                List<FormMain.VIEW_ITEM> listRes = new List<FormMain.VIEW_ITEM>();

                (from dataSet in _listDataSet
                 orderby dataSet.m_dtRecieved descending
                 select new FormMain.VIEW_ITEM {
                     Values = new object[] {
                        dataSet.m_tableValues.Rows.Count
                        , dataSet.m_dtRecieved
                        , dataSet.m_dtQuered
                    }
                 }).Take(COUNT_VIEW_DATASET_ITEM).ToList().ForEach(item => listRes.Add(item));

                return listRes;
            }
        }

        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;
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

                        addDataSet((DateTime)itemQueue.Pars[0], itemQueue.Pars[1] as DataTable, itemQueue.Pars[2] as DataTable);
                        break;
                    case StatesMachine.LIST_DEST: // получен спискок объектов с парметрами соединения с БД
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
                        // инициализация переданными значениями
                        initialize(itemQueue.Pars[0] as List<ConnectionSettings>);
                        // установка соединения с источниками данных
                        _writer.StartDbInterfaces();
                        // обеспечить готовность к приему событий (TSQL-запросов для выполнения)
                        _writer.Start(); _writer.Activate(true);
                        break;
                    case StatesMachine.LIST_DATASET: // запрос на формирование актуального списка переданных для записи наборов
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = listViewDataSetItem;
                        break;
                    case StatesMachine.DATASET_CONTENT: // запрос на передачу содержимого набора, выбранного пользователем для отображения
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
                        break;
                    case StatesMachine.STATISTIC: // 
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
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
                case StatesMachine.STATISTIC: //
                    // не требуют запроса
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            switch ((StatesMachine)state) {
                case StatesMachine.NEW: // 
                case StatesMachine.LIST_DEST: // получены параметры соединения БД, ответа не требуется
                    //Ответа не требуется/не требуют обработки результата
                    break;
                case StatesMachine.LIST_DATASET: // отправить главному окну(для отображения) информацию о поученных/обработанных наборах с данными
                case StatesMachine.DATASET_CONTENT: // отправить главному окну(для отображения) информацию о выбранном для отображения в главном окне
                case StatesMachine.STATISTIC: // статический объект (obj = null)
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
