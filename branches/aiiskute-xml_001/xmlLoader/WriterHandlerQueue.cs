using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Data;
using System.Reflection;
using System.Diagnostics;

namespace xmlLoader
{
    public partial class WriterHandlerQueue : HClassLibrary.HHandlerQueue
    {
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
            , CONNSET_USE_CHANGED
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
            _writer.EvtDatasetQuered += new DelegateIntFunc(writerDbHandler_OnDatasetQuered);
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

            public DATASET(IEnumerable<int>keys, DateTime dtRecieved, DataTable tableValues, DataTable tableParameters)
            {
                m_dtRecieved = dtRecieved;

                m_dictDatetimeQuered = new Dictionary<int, DateTime>();
                foreach (int key in keys)
                    m_dictDatetimeQuered.Add(key, DateTime.MinValue);

                m_state = STATE.NEW;

                m_tableValues = tableValues.Copy();

                m_tableParameters = tableParameters.Copy();

                m_query = string.Empty;

                m_state = STATE.QUERING;
            }

            public DateTime m_dtRecieved;

            public Dictionary<int, DateTime> m_dictDatetimeQuered;

            public DataTable m_tableValues;

            public DataTable m_tableParameters;

            public STATE m_state;

            public string m_query;
        }

        private List<DATASET> _listDataSet;

        private int addDataSet(IEnumerable<int>keys, DateTime dtDataSet, DataTable values, DataTable parameters)
        {
            int iRes = 0;

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

        private static int COUNT_VIEW_DATASET_ITEM = 6;

        private List<FormMain.VIEW_ITEM> getListViewDataSetItem(int key)
        {
            List<FormMain.VIEW_ITEM> listRes = new List<FormMain.VIEW_ITEM>();

            (from dataSet in _listDataSet
                orderby dataSet.m_dtRecieved descending
                select new FormMain.VIEW_ITEM {
                    Values = new object[] {
                    dataSet.m_tableValues.Rows.Count
                    , dataSet.m_dtRecieved
                    , dataSet.m_dictDatetimeQuered[key]
                }
                }).Take(COUNT_VIEW_DATASET_ITEM).ToList().ForEach(item => listRes.Add(item));

            return listRes;
        }

        private void writerDbHandler_OnDatasetQuered(int idConnSett)
        {
            _listDataSet[_listDataSet.Count - 1].m_dictDatetimeQuered[idConnSett] = DateTime.UtcNow;
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

                        error = (iRes = addDataSet(_writer.ListConnSettKey, (DateTime)itemQueue.Pars[0], itemQueue.Pars[1] as DataTable, itemQueue.Pars[2] as DataTable)) < 0 ? true : false; ;

                        debugMsg = string.Format(@"Добавление нового набора [{0}]...", (DateTime)itemQueue.Pars[0]);

                        if (error == false) {
                        // добавленный набор поставить в очередь на запись
                            outobj = _listDataSet[_listDataSet.Count - 1].m_query;

                            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
                        } else
                            Logging.Logg().Error(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
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

                        if (!((int)itemQueue.Pars[0] < 0))
                            outobj = getListViewDataSetItem((int)itemQueue.Pars[0]);
                        else
                            ;
                        break;
                    case StatesMachine.DATASET_CONTENT: // запрос на передачу содержимого набора, выбранного пользователем для отображения
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;
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
                //case StatesMachine.STATISTIC: //
                case StatesMachine.CONNSET_USE_CHANGED: // 
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
                case StatesMachine.NEW: // самый старый набор для постановки в очередь на запись, 
                    _writer.Request((string)obj);
                    break;
                case StatesMachine.LIST_DEST: // получены параметры соединения БД, ответа не требуется
                case StatesMachine.CONNSET_USE_CHANGED:
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

            Debug.WriteLine(string.Format(@"WriterHandlerQueue::StateResponse(state={0}) - ...", ((StatesMachine)state).ToString()));

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
