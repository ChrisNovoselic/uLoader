using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using HClassLibrary; //HHandler

namespace uLoader
{
    class HHandlerQueue : HClassLibrary.HHandlerQueue
    {
        public enum StatesMachine
        {
            UNKNOWN = -1
            , LIST_GROUP_SOURCES //Список групп источников (источник, назначение)
            , LIST_SRC_GROUP_SOURCE_ITEMS //Список источников в группе источников (источник)
            , LIST_SRC_GROUP_SOURCE_PARS //Список наименовний параметров соединения источников в группе источников (источник)
            , LIST_SRC_GROUP_SOURCE_PROP //Список параметров соединения источников в группе источников (источник)
            , LIST_GROUP_SIGNALS //Список групп сигналов (источник, назначение)
            , LIST_SRC_GROUP_SIGNAL_ITEMS //Список сигналов в группе сигналов (источник)
            , LIST_SRC_GROUP_SIGNAL_PARS //Список наименовний параметров сигналов в группе сигналов (источник)
            , LIST_SRC_GROUP_SIGNAL_PROP //Список параметров сигналов в группе сигналов (источник)
            , LIST_DEST_GROUP_SOURCE_ITEMS //Список источников в группе источников (назначение)
            , LIST_DEST_GROUP_SOURCE_PARS //Список наименований параметров соединения источников в группе источников (назначение)
            , LIST_DEST_GROUP_SOURCE_PROP //Список параметров соединения источников в группе источников (назначение)            
            , LIST_DEST_GROUP_SIGNAL_ITEMS //Список сигналов в группе сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_PARS //Список наименований параметров сигналов в группе сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_PROP //Список параметров сигналов в группе сигналов (назначение)
            , OBJ_SRC_GROUP_SOURCES //Объект группы источников (источник)
            , OBJ_DEST_GROUP_SOURCES //Объект группы источников (назначение)
            , OBJ_SRC_GROUP_SIGNALS //Объект группы сигналов (источник)
            , OBJ_DEST_GROUP_SIGNALS //Объект группы сигналов (назначение)
            , TIMER_WORK_UPDATE //Период обновления панели "Работа"
            , STATE_GROUP_SOURCES //Состояние группы источников (источник, назначение)
            , STATE_GROUP_SIGNALS //Состояние группы сигналов (источник, назначение)
            , STATE_CHANGED_GROUP_SOURCES //Изменение состояния группы источников (источник, назначение) - инициатива пользователя
            , STATE_CHANGED_GROUP_SIGNALS //Изменение состояния группы сигналов (источник, назначение) - инициатива пользователя
            , DATA_SRC_GROUP_SIGNALS //Данные группы сигналов (источник)
            , DATA_DEST_GROUP_SIGNALS //Дфнные группы сигналов (назначение)
        }

        private FormMain.FileINI m_fileINI;
        private List <GroupSources> [] m_listGroupSources;

        public HHandlerQueue()
            : base ()
        {
            string strNameFileINI = string.Empty;
            string []args = Environment.GetCommandLineArgs ();

            if (args.Length > 1)
            {
                //strNameFileINI = @"setup_biysttmora.ini";
                //strNameFileINI = @"setup_ktstusql.ini";
                strNameFileINI = args[1];
            }
            else
                strNameFileINI = @"setup.ini";
            
            m_fileINI = new FormMain.FileINI(strNameFileINI);

            m_listGroupSources = new List<GroupSources> [(int)INDEX_SRC.COUNT_INDEX_SRC];

            setListGroupSources(INDEX_SRC.SOURCE, m_fileINI.AllObjectsSrcGroupSources, m_fileINI.AllObjectsSrcGroupSignals);
            setListGroupSources(INDEX_SRC.DEST, m_fileINI.AllObjectsDestGroupSources, m_fileINI.AllObjectsDestGroupSignals);
        }
        /// <summary>
        /// Заполнить список с информацией о группах источников
        /// </summary>
        /// <param name="indxSrc">Индекс панели</param>
        /// <param name="arGroupSources">Массив с информацией о группах с источниками</param>
        /// <param name="arGroupSignals">Массив с информацией о группах с сигналами</param>
        /// <returns>Признак выполнения функции</returns>
        private int setListGroupSources(INDEX_SRC indxSrc, GROUP_SRC[] arGroupSources, GROUP_SIGNALS_SRC[] arGroupSignals)
        {
            int iRes = 0;

            if (m_listGroupSources[(int)indxSrc] == null)
                m_listGroupSources[(int)indxSrc] = new List<GroupSources>();
            else
                ;

            List<GROUP_SIGNALS_SRC> listGroupSignals = new List<GROUP_SIGNALS_SRC>();
            string[] arIDGroupSignals;
            foreach (GROUP_SRC itemSrc in arGroupSources)
            {
                arIDGroupSignals = (itemSrc as GROUP_SRC).m_arIDGroupSignals;
                foreach (string id in arIDGroupSignals)
                    foreach (GROUP_SIGNALS_SRC itemGrooupSignals in arGroupSignals)
                        if (itemGrooupSignals.m_strID.Equals(id) == true)
                        {
                            listGroupSignals.Add(itemGrooupSignals);
                            break;
                        }
                        else
                            ;

                m_listGroupSources[(int)indxSrc].Add(new GroupSources(itemSrc, listGroupSignals));
            }

            return iRes;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;
            HDataHost dataHost = null;

            switch ((StatesMachine)state)
            {
                case StatesMachine.LIST_GROUP_SOURCES:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_GROUP_SIGNALS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                case StatesMachine.OBJ_SRC_GROUP_SOURCES:
                case StatesMachine.OBJ_DEST_GROUP_SOURCES:
                case StatesMachine.TIMER_WORK_UPDATE:
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                    //Не требуют запроса
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            HDataHost dataHost = Peek;

            switch ((StatesMachine)state)
            {
                case StatesMachine.LIST_GROUP_SOURCES:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_GROUP_SIGNALS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                case StatesMachine.OBJ_SRC_GROUP_SOURCES:                
                case StatesMachine.OBJ_DEST_GROUP_SOURCES:
                case StatesMachine.TIMER_WORK_UPDATE:
                case StatesMachine.OBJ_SRC_GROUP_SIGNALS:
                case StatesMachine.STATE_GROUP_SOURCES:
                case StatesMachine.STATE_GROUP_SIGNALS:
                case StatesMachine.STATE_CHANGED_GROUP_SOURCES:
                case StatesMachine.STATE_CHANGED_GROUP_SIGNALS:
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                    dataHost.m_objRecieved.OnEvtDataRecievedHost(new object [] { state, obj });
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;
            StatesMachine state = (StatesMachine)s;

            error = true;
            outobj = null;

            HDataHost dataHost = null;

            switch (state)
            {
                case (int)StatesMachine.LIST_GROUP_SOURCES:
                    error = false;
                    outobj = new object [] {
                        m_fileINI.ListSrcGroupSources
                        , m_fileINI.ListDestGroupSources
                    };

                    iRes = 0;
                    break;
                case StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSource(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSource (dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSource(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_GROUP_SIGNALS:
                    error = false;
                    outobj = new object[] {
                        m_fileINI.ListSrcGroupSignals
                        , m_fileINI.ListDestGroupSignals
                    };

                    iRes = 0;
                    break;
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSignal(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSignal (dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSignal(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSource(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSource (dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSource(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSignal(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSignal (dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSignal(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.OBJ_SRC_GROUP_SOURCES:
                    error = false;
                    dataHost = Peek;
                    //??? 0-й параметр индекс "выбранноой" группы источников
                    outobj = m_fileINI.AllObjectsSrcGroupSources[(int)dataHost.Pars((int)state)[0]];

                    iRes = 0;
                    break;
                case StatesMachine.OBJ_DEST_GROUP_SOURCES:
                    error = false;
                    dataHost = Peek;
                    //??? 0-й параметр индекс "выбранноой" группы источников
                    outobj = m_fileINI.AllObjectsDestGroupSources[(int)dataHost.Pars((int)state)[0]];

                    iRes = 0;
                    break;
                case StatesMachine.TIMER_WORK_UPDATE:
                    error = false;
                    outobj = m_fileINI.SecondWorkUpdate;

                    iRes = 0;
                    break;
                case StatesMachine.OBJ_SRC_GROUP_SIGNALS:
                    error = false;
                    dataHost = Peek;
                    //??? 0-й параметр идентификатор "выбранноой" группы сигналов
                    outobj = m_fileINI.GetObjectGroupSignals(dataHost.Pars((int)state).ToArray());

                    iRes = 0;
                    break;
                case StatesMachine.STATE_GROUP_SOURCES:
                    error = false;
                    outobj = new object[(int)INDEX_SRC.COUNT_INDEX_SRC];
                    for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc++)
                    {
                        (outobj as object[])[(int)indxSrc] = new GroupSources.STATE[m_listGroupSources[(int)indxSrc].Count];

                        foreach (GroupSources grpSrc in m_listGroupSources[(int)indxSrc])
                            ((outobj as object[])[(int)indxSrc] as GroupSources.STATE[])[m_listGroupSources[(int)indxSrc].IndexOf(grpSrc)] = grpSrc.State;
                    }

                    iRes = 0;
                    break;
                case StatesMachine.STATE_GROUP_SIGNALS:
                    error = false;
                    dataHost = Peek;
                    //??? 0-й параметр индекс "выбранноой" группы сигналов
                    outobj = new object[(int)INDEX_SRC.COUNT_INDEX_SRC];
                    for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc++)
                        if (!((int)dataHost.Pars((int)state)[(int)indxSrc] < 0))
                            (outobj as object[])[(int)indxSrc] = m_listGroupSources[(int)indxSrc][(int)dataHost.Pars((int)state)[(int)indxSrc]].GetStateGroupSignals();
                        else
                            (outobj as object[])[(int)indxSrc] = new GroupSources.STATE[] { };

                    iRes = 0;
                    break;
                case StatesMachine.STATE_CHANGED_GROUP_SOURCES:
                    error = false;
                    dataHost = Peek;

                    iRes = m_listGroupSources[(int)((INDEX_SRC)dataHost.Pars((int)state)[0])][(int)dataHost.Pars((int)state)[1]].StateChange((int)dataHost.Pars((int)state)[2]);
                    break;
                case StatesMachine.STATE_CHANGED_GROUP_SIGNALS:
                    error = false;
                    dataHost = Peek;

                    iRes = m_listGroupSources[(int)((INDEX_SRC)dataHost.Pars((int)state)[0])][(int)dataHost.Pars((int)state)[1]].StateChange((int)dataHost.Pars((int)state)[2]);
                    break;
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                    error = false;
                    dataHost = Peek;

                    outobj = m_listGroupSources[(int)INDEX_SRC.SOURCE][(int)dataHost.Pars((int)state)[0]].GetData(dataHost.Pars((int)state)[1] as string, out error);

                    iRes = 0;
                    break;
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                    error = false;

                    iRes = 0;
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }
    }
}
