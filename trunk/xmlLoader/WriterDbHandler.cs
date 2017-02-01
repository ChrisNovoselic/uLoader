using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Data;
using System.Reflection;
using System.Threading;
using System.ComponentModel;

namespace xmlLoader
{
    partial class WriterHandlerQueue
    {
        private class WriterDbHandler : HHandlerDb, IDataHost
        {
            private enum StatesMachine { Truncate, Merge, SP }

            private List<ConnectionSettings> m_listConnSett;

            private Queue<int> m_queueIdConnSett;

            private Dictionary<int, bool> m_dictConnSettUsed;

            //private Thread _threadRequest;
            private BackgroundWorker _threadRequest;

            //public event DelegateIntFunc EvtDatasetQuered;
            public event DelegateObjectFunc EvtDataAskedHost;

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

            public WriterDbHandler()
            {
                m_dictConnSettUsed = new Dictionary<int, bool>();

                m_queueIdConnSett = new Queue<int>();

                _threadRequest = new BackgroundWorker();
                _threadRequest.WorkerSupportsCancellation = true;
                _threadRequest.DoWork += new DoWorkEventHandler(fThreadRequest);
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
                _threadRequest.Dispose();

                base.Stop();
            }
            /// <summary>
            /// Выполнить запрос
            /// </summary>
            /// <param name="query">Запрос для выполнения</param>
            public void Request(DATASET_WRITER dataSetWriter)
            {
                // остановить поток, если выполняется
                if (!(_threadRequest == null)) {
                    //if (_threadRequest.IsAlive == true) {
                    //    if (_threadRequest.Join(666) == false)
                    //    {
                    //        _threadRequest.Interrupt();
                    //        if (((_threadRequest.ThreadState & ThreadState.Running) == ThreadState.Running)
                    //            || ((_threadRequest.ThreadState & ThreadState.Background) == ThreadState.Background)
                    //            || ((_threadRequest.ThreadState & ThreadState.Suspended) == ThreadState.Suspended))
                    //            _threadRequest.Abort();
                    //        else
                    //            ;
                    //    } else
                    //        ;
                    //} else {
                    //}

                    ////WaitHandle.
                    ////_threadRequest.
                    //_threadRequest = null;

                    if (_threadRequest.IsBusy == true) {
                        _threadRequest.CancelAsync();
                    } else {
                    }

                    GC.Collect();
                } else
                    ;

                // очистить очередь идентификаторов источников данных
                m_queueIdConnSett.Clear();
                // сформировать очередь с источниками данных, в ~ от его вкл./выкл. для выполнения запроса к ним
                foreach (ConnectionSettings connSett in m_listConnSett)
                    if (m_dictConnSettUsed[connSett.id] == true) m_queueIdConnSett.Enqueue(connSett.id); else;
                // запустить, при необходимости поток выполнения запроса
                if (m_queueIdConnSett.Count > 0) {
                    // вариант №1 - поток
                    //_threadRequest = //new Thread(new ParameterizedThreadStart(fThreadRequest));
                    //_threadRequest.IsBackground = true;
                    //_threadRequest.Name = @"WriterDbHandler.Request";
                    //_threadRequest.Start(dataSetWriter);
                    // вариант №2
                    _threadRequest.RunWorkerAsync(dataSetWriter);
                    //// вариант №3 - без потока
                    //fThreadRequest(dataSetWriter);
                } else
                    ;
            }
            /// <summary>
            /// Потоковая функция - выполнить запросы к источникам данных в ~ соответствии со сформированной очередью
            /// </summary>
            /// <param name="obj">Аргумент при вызове</param>
            private void fThreadRequest(object obj, DoWorkEventArgs ev)
            {
                INDEX_WAITHANDLE_REASON indxReasonCompleted = INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON;
                DATASET_WRITER dataSetWriter = (DATASET_WRITER)ev.Argument;
                string fmtMsg = @"сохранение набора для IdConnSett={0}, [{1}]";

                for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                    ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                Query = dataSetWriter.m_query;

                try {
                    while (m_queueIdConnSett.Count > 0) {
                        ClearStates();

                        IdConnSettCurrent = m_queueIdConnSett.Dequeue();

                        foreach (StatesMachine state in Enum.GetValues(typeof(StatesMachine))) {
                            AddState((int)state);

                            Run(string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString()));
                            // ожидать завершения обработки состояния
                            indxReasonCompleted = (INDEX_WAITHANDLE_REASON)WaitHandle.WaitAny(m_waitHandleState);

                            if (indxReasonCompleted == INDEX_WAITHANDLE_REASON.ERROR)
                                break;
                            else
                                ; // continue
                        }
                        // зафиксировать в логе результат
                        if (indxReasonCompleted == INDEX_WAITHANDLE_REASON.SUCCESS)
                            Logging.Logg().Debug(MethodBase.GetCurrentMethod()
                                , string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            Logging.Logg().Error(MethodBase.GetCurrentMethod()
                                , string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        // оповестить об завершении выполнения группы запросов
                        DataAskedHost(new object[] { indxReasonCompleted, IdConnSettCurrent, dataSetWriter.m_dtRecieved });
                    }
                } catch (Exception e) {
                    Logging.Logg().Exception(e
                        , string.Format(fmtMsg, IdConnSettCurrent, dataSetWriter.m_dtRecieved.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public void Initialize(List<ConnectionSettings> listConnSett)
            {
                m_listConnSett = listConnSett;

                m_dictConnSettUsed.Clear();
                foreach (ConnectionSettings connSett in m_listConnSett)
                    m_dictConnSettUsed.Add(connSett.id, false);
            }

            public void ChangeConnSettUse(int idConnSett)
            {
                if ((!(idConnSett < 0))
                    && (m_dictConnSettUsed.ContainsKey(idConnSett) == true))
                    m_dictConnSettUsed[idConnSett] = !m_dictConnSettUsed[idConnSett];
                else
                    Logging.Logg().Error(MethodBase.GetCurrentMethod(), string.Format(@"не найден ключ={0}", idConnSett), Logging.INDEX_MESSAGE.NOT_SET);
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
                                @"TRUNCATE TABLE [dbo].[BIYSK_LOADER]"
                                //@"SELECT * FROM [dbo].[BIYSK_LOADER]"
                            );
                            break;
                        case StatesMachine.Merge:
                            Request(m_dictIdListeners[IdConnSettCurrent][0], Query);
                            break;
                        case StatesMachine.SP:
                            Request(m_dictIdListeners[IdConnSettCurrent][0], @"EXECUTE [dbo].[WORK_DATALOADER]");
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

                //if (isLastState(state) == true)
                //    this.completeHandleStates(INDEX_WAITHANDLE_REASON.SUCCESS);
                //else
                //    ;

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
