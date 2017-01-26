using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Data;
using System.Reflection;
using System.Threading;

namespace xmlLoader
{
    partial class WriterHandlerQueue
    {
        private class WriterDbHandler : HHandlerDb
        {
            private enum StatesMachine { Truncate, Merge, SP }

            private List<ConnectionSettings> m_listConnSett;

            private Queue<int> m_queueIdConnSett;

            private Dictionary<int, bool> m_dictConnSettUsed;

            public DelegateIntFunc EvtDatasetQuered;

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
            }

            protected override void InitializeSyncState()
            {
                base.InitializeSyncState();
            }

            public void Request(string query)
            {
                Thread threadRequest;

                m_queueIdConnSett.Clear();

                foreach (ConnectionSettings connSett in m_listConnSett)
                    if (m_dictConnSettUsed[connSett.id] == true) m_queueIdConnSett.Enqueue(connSett.id); else;

                threadRequest = new Thread(new ParameterizedThreadStart(fThreadRequest));
                threadRequest.IsBackground = true;
                threadRequest.Name = @"WriterDbHandler.Request";
                threadRequest.Start(query);
            }

            private void fThreadRequest(object obj)
            {
                INDEX_WAITHANDLE_REASON indxReasonCompleted = INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON;
                string fmtMsg = @"Cохранение значений для {0}";

                Query =
                    //(string)obj
                    @"INSERT INTO [dbo].[BIYSK_LOADER] ([XML_SECTION_NAME],[XML_ITEM_NAME],[VALUE],[DATA_DATE]) VALUES ('SEC', 'PAR', 0.00, GETDATE())"
                    ;

                try {
                    while (m_queueIdConnSett.Count > 0) {
                        IdConnSettCurrent = m_queueIdConnSett.Dequeue();

                        AddState((int)StatesMachine.Truncate);
                        AddState((int)StatesMachine.Merge);
                        AddState((int)StatesMachine.SP);

                        Run(string.Format(fmtMsg, IdConnSettCurrent));

                        indxReasonCompleted = (INDEX_WAITHANDLE_REASON)WaitHandle.WaitAny(m_waitHandleState);

                        if (indxReasonCompleted == INDEX_WAITHANDLE_REASON.SUCCESS)
                            EvtDatasetQuered?.Invoke(IdConnSettCurrent);
                        else
                            ;
                    }
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(fmtMsg, IdConnSettCurrent), Logging.INDEX_MESSAGE.NOT_SET);
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
                if (!(idConnSett < 0))
                    m_dictConnSettUsed[idConnSett] = !m_dictConnSettUsed[idConnSett];
                else
                    ;
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

                if (isLastState(state) == true)
                    this.completeHandleStates(INDEX_WAITHANDLE_REASON.ERROR);
                else
                    ;

                Logging.Logg().Error(
                    string.Format(@"WriterDbHandler::StateErrors (state={0}, req={1}, res={2}) - ...", ((StatesMachine)state).ToString(), req, res)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                return resReason;
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
                if (isLastState(state) == true)
                    this.completeHandleStates(INDEX_WAITHANDLE_REASON.SUCCESS);
                else
                    ;

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
    }
}
