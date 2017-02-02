using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Data;
using System.Reflection;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;

namespace xmlLoader
{
    partial class WriterHandlerQueue
    {
        private class WriterDbHandler : HHandlerDb, IDataHost
        {
            /// <summary>
            /// Перечисление - 
            /// </summary>
            private enum StatesMachine { Truncate, Merge, SP }
            /// <summary>
            /// Список объектов с параметрами соединения с БД (источники данных)
            /// </summary>
            private List<ConnectionSettings> m_listConnSett;
            /// <summary>
            /// Очередь (из идентификаторов источников данных) для очередного запроса
            ///  - зависит от вкл./откл. конкретного источника данных
            /// </summary>
            private Queue<int> m_queueIdConnSett;
            /// <summary>
            /// Реализация интерфейса 'IDataHost' - событие для передачи данных
            /// </summary>
            public event DelegateObjectFunc EvtDataAskedHost;
            /// <summary>
            /// Возвратить список идентификаторов известных источников данных
            /// </summary>
            public IEnumerable<int> ListConnSettKey
            {
                get {
                    //List<int> listRes = new List<int>();

                    return (from connSett in m_listConnSett select connSett.id)
                        //.ToList()
                        ;

                    //return listRes;
                }
            }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public WriterDbHandler()
            {
                m_queueIdConnSett = new Queue<int>();
                //??? некорректно, следует пересмотреть порядок инициализации объектов
                m_manualEventInitListConnSett = new ManualResetEvent(false);
            }
            /// <summary>
            /// Инициализировать дополнительные объекты синхронизации
            /// </summary>
            protected override void InitializeSyncState()
            {
                m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON];
                base.InitializeSyncState();
                for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++)
                    m_waitHandleState[i] = new ManualResetEvent(false);
            }

            public override void Stop()
            {
                //_threadRequest.Dispose();

                base.Stop();
            }

            CancellationTokenSource _cancelRequest;
            /// <summary>
            /// Выполнить запрос
            /// </summary>
            /// <param name="query">Запрос для выполнения</param>
            public void Request(DATASET_WRITER dataSetWriter)
            {
                if (!(_cancelRequest == null)) {
                    _cancelRequest.Cancel();
                    _cancelRequest.Dispose();
                    _cancelRequest = null;
                } else
                    ;

                // очистить очередь идентификаторов источников данных
                m_queueIdConnSett.Clear();
                // сформировать очередь с источниками данных, в ~ от его вкл./выкл. для выполнения запроса к ним
                foreach (ConnectionSettings connSett in m_listConnSett)
                    if ((bool)connSett.Items[(int)ConnectionSettings.INDEX_ITEM.TURN] == true) m_queueIdConnSett.Enqueue(connSett.id); else;

                _cancelRequest = new CancellationTokenSource();

                Task.Run(() => fThreadRequest(dataSetWriter), _cancelRequest.Token);                
            }
            /// <summary>
            /// Потоковая функция - выполнить запросы к источникам данных в ~ соответствии со сформированной очередью
            /// </summary>
            /// <param name="obj">Аргумент при вызове</param>
            private void fThreadRequest(DATASET_WRITER dataSet)
            //private void fThreadRequest_DoWork(object obj, DoWorkEventArgs ev)
            {
                INDEX_WAITHANDLE_REASON indxReasonCompleted = INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON;
                DATASET_WRITER dataSetWriter = dataSet;
                string fmtMsg = @"сохранение набора для IdConnSett={0}, [{1}]";

                for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                    ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                Query = dataSetWriter.m_values;

                try {
                    while (m_queueIdConnSett.Count > 0) {
                        ClearStates();

                        IdConnSettCurrent = m_queueIdConnSett.Dequeue();
                        // добавить необходимые состояния
                        foreach (StatesMachine state in Enum.GetValues(typeof(StatesMachine)))
                            AddState((int)state);

                        Run(string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString()));
                        // ожидать завершения обработки состояния
                        switch(WaitHandle.WaitAny(new WaitHandle[] {
                            m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.BREAK] // признак успешной обработки крайнего состояния
                            , m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.ERROR]                             
                        })) {
                            case 0:
                                indxReasonCompleted = INDEX_WAITHANDLE_REASON.SUCCESS;
                                break;
                            case 1:
                            default:
                                indxReasonCompleted = INDEX_WAITHANDLE_REASON.ERROR;
                                break;
                        }

                        // зафиксировать в логе результат
                        if (indxReasonCompleted == INDEX_WAITHANDLE_REASON.SUCCESS)
                            Logging.Logg().Debug(MethodBase.GetCurrentMethod()
                                , string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        else
                        // ERROR
                            Logging.Logg().Error(MethodBase.GetCurrentMethod()
                                , string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        // оповестить об завершении выполнения группы запросов
                        DataAskedHost(new object[] { indxReasonCompleted, IdConnSettCurrent, dataSetWriter.m_dtRecieved });
                    } // while
                } catch (Exception e) {
                    Logging.Logg().Exception(e
                        , string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            private ManualResetEvent m_manualEventInitListConnSett;

            public void Initialize(List<ConnectionSettings> listConnSett)
            {
                m_listConnSett = listConnSett;
                // показать, что инициализация списка завершена
                m_manualEventInitListConnSett.Set();
            }

            public void ChangeConnSettUse(int idConnSett)
            {
                // ожидать пока не завершится инициализация списка
                m_manualEventInitListConnSett.WaitOne();

                ConnectionSettings connSett = null;

                if (!(idConnSett < 0)) {
                    connSett = m_listConnSett.Find(item => { return item.id == idConnSett; });
                    // проверить результат поиска
                    if (!(connSett == null))
                    // изменить сотояние вкл./откл. источника данных
                        connSett.Items[(int)ConnectionSettings.INDEX_ITEM.TURN] = !(bool)connSett.Items[(int)ConnectionSettings.INDEX_ITEM.TURN];
                    else
                        ;
                } else
                    Logging.Logg().Error(MethodBase.GetCurrentMethod(), string.Format(@"не найден источник данных с идентификатором = {0}", idConnSett), Logging.INDEX_MESSAGE.NOT_SET);
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
            protected override void register(int id, int indx, HClassLibrary.ConnectionSettings connSett, string name)
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

            private static string s_Replacing = @"?VALUES?";

            private static string[] s_Queries = {
                @"TRUNCATE TABLE [dbo].[BIYSK_LOADER]" // Truncate
                , @"MERGE [dbo].[BIYSK_LOADER] AS [T]"
                        + @" USING ("
                        + @"SELECT [XML_SECTION_NAME],[XML_ITEM_NAME],[VALUE] FROM (VALUES"
                        + s_Replacing //replacing
                        + @") AS [SOURCE]([XML_SECTION_NAME],[XML_ITEM_NAME],[VALUE])"
                    + @") AS [S]"
                    + @" ON ([T].[XML_SECTION_NAME] = [S].[XML_SECTION_NAME] AND [T].[XML_ITEM_NAME] = [S].[XML_ITEM_NAME])"
                    + @" WHEN NOT MATCHED BY TARGET THEN INSERT ([XML_SECTION_NAME], [XML_ITEM_NAME], [VALUE], [DATETIME])"
                    + @" VALUES ([XML_SECTION_NAME], [XML_ITEM_NAME], [VALUE], GETUTCDATE());" // Merge
                , @"EXECUTE [dbo].[WORK_DATALOADER]" // Exec
            };

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
                HHandler.INDEX_WAITHANDLE_REASON indxReasonRes =
                    //INDEX_WAITHANDLE_REASON.SUCCESS
                    INDEX_WAITHANDLE_REASON.ERROR
                    ;

                //this.completeHandleStates(INDEX_WAITHANDLE_REASON.ERROR);

                Logging.Logg().Error(
                    string.Format(@"WriterDbHandler::StateErrors (state={0}, req={1}, res={2}) - ...", ((StatesMachine)state).ToString(), req, res)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                return indxReasonRes;
            }

            protected override int StateRequest(int state)
            {
                int iRes = 0;

                if (Query.Equals(string.Empty) == false) {
                    switch ((StatesMachine)state) {
                        case StatesMachine.Truncate:
                            Request(m_dictIdListeners[IdConnSettCurrent][0]
                                ,
                                    s_Queries[state]
                                    //@"SELECT * FROM [dbo].[BIYSK_LOADER]"
                            );
                            break;
                        case StatesMachine.Merge:
                            Request(m_dictIdListeners[IdConnSettCurrent][0], s_Queries[state].Replace(s_Replacing, Query));
                            break;
                        case StatesMachine.SP:
                            Request(m_dictIdListeners[IdConnSettCurrent][0], s_Queries[state]);
                            break;
                        default:
                            break;
                    }
                } else
                    ;                

                return iRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                INDEX_WAITHANDLE_REASON indxReasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                if (isLastState(state) == true)
                    //indxReasonRes = INDEX_WAITHANDLE_REASON.BREAK;
                    this.completeHandleStates(INDEX_WAITHANDLE_REASON.BREAK);
                else
                    ;

                // ответ не требуется
                return (int)indxReasonRes;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                Logging.Logg().Warning(
                    string.Format(@"WriterDbHandler::StateWarnings (state={0}, req={1}, res={2}) - ...", ((StatesMachine)state).ToString(), req, res)
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            public void DataAskedHost(object par)
            {
                EvtDataAskedHost?.Invoke(par);
            }

            public void OnEvtDataRecievedHost(object res)
            {
                throw new NotImplementedException();
            }
        }
    }
}
