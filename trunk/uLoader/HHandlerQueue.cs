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
            LIST_SRC_GROUP_SOURCES //Список групп источников (источник)
            , LIST_SRC_GROUP_SOURCE_ITEMS //Список источников в группе источников (истиочник)
            , LIST_SRC_GROUP_SOURCE_PARS //Список наименовний параметров соединения источников в группе источников (истиочник)
            , LIST_SRC_GROUP_SOURCE_PROP //Список параметров соединения источников в группе источников (истиочник)
            , LIST_SRC_GROUP_SIGNALS //Список групп сигналов (источник)
            , LIST_SRC_GROUP_SIGNAL_ITEMS //Список сигналов в группе сигналов (истиочник)
            , LIST_SRC_GROUP_SIGNAL_PARS //Список наименовний параметров сигналов в группе сигналов (истиочник)
            , LIST_SRC_GROUP_SIGNAL_PROP //Список параметров сигналов в группе сигналов (истиочник)
            , LIST_DEST_GROUP_SOURCES //Список групп источников (назначение)
            , LIST_DEST_GROUP_SOURCE_ITEMS //Список источников в группе источников (назначение)
            , LIST_DEST_GROUP_SOURCE_PARS //Список наименований параметров соединения источников в группе источников (назначение)
            , LIST_DEST_GROUP_SOURCE_PROP //Список параметров соединения источников в группе источников (назначение)
            , LIST_DEST_GROUP_SIGNALS //Список групп сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_ITEMS //Список сигналов в группе сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_PARS //Список наименований параметров сигналов в группе сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_PROP //Список параметров сигналов в группе сигналов (назначение)
            , OBJ_SRC_GROUP_SOURCES //Объект группы источников (источник)
            , OBJ_DEST_GROUP_SOURCES //Объект группы источников (назначение)
            , TIMER_WORK_UPDATE //Период обновления панели "Работа"
            ,
        }

        private FormMain.FileINI m_fileINI;
        private List <GroupSources> m_listGroupSources;

        public HHandlerQueue()
            : base ()
        {
            m_fileINI = new FormMain.FileINI(@"setup_biysktmora.ini");

            GROUP_SRC []arGroupSources = m_fileINI.AllObjectsSrcGroupSources;
            GROUP_SIGNALS_SRC[] arGroupSignals = m_fileINI.AllObjectsSrcGroupSignals;

            m_listGroupSources = new List<GroupSources> ();

            List <GROUP_SIGNALS_SRC>listGroupSignals = new List<GROUP_SIGNALS_SRC> ();
            string []arIDGroupSignals;
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

                m_listGroupSources.Add(new GroupSources(itemSrc, listGroupSignals));
            }
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCES:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCES:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                case (int)StatesMachine.OBJ_SRC_GROUP_SOURCES:
                case (int)StatesMachine.OBJ_DEST_GROUP_SOURCES:
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

            switch (state)
            {
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCES:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCES:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                case (int)StatesMachine.OBJ_SRC_GROUP_SOURCES:                
                case (int)StatesMachine.OBJ_DEST_GROUP_SOURCES:
                    dataHost.m_objRecieved.OnEvtDataRecievedHost(new object [] { state, obj });
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateCheckResponse(int state, out bool error, out object outobj)
        {
            int iRes = -1;

            error = true;
            outobj = null;

            HDataHost dataHost = null;

            switch (state)
            {
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCES:
                    error = false;
                    outobj = m_fileINI.ListSrcGroupSources;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSource(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSource (dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSource(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNALS:
                    error = false;
                    outobj = m_fileINI.ListSrcGroupSignals;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSignal(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSignal (dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSignal(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCES:
                    error = false;
                    outobj = m_fileINI.ListDestGroupSources;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSource(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSource (dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSource(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNALS:
                    error = false;
                    outobj = m_fileINI.ListDestGroupSignals;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemsOfGroupSignal(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListParsOfGroupSignal (dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                    error = false;
                    dataHost = Peek;
                    outobj = m_fileINI.GetListItemPropOfGroupSignal(dataHost.m_pars.ToArray());

                    iRes = 0;
                    break;
                case (int)StatesMachine.OBJ_SRC_GROUP_SOURCES:
                    error = false;
                    dataHost = Peek;
                    //??? 0-й параметр индекс "выбранноой" группы источников
                    outobj = m_fileINI.AllObjectsSrcGroupSources[(int)dataHost.m_pars[0]];

                    iRes = 0;
                    break;
                case (int)StatesMachine.OBJ_DEST_GROUP_SOURCES:
                    error = false;
                    dataHost = Peek;
                    //??? 0-й параметр индекс "выбранноой" группы источников
                    outobj = m_fileINI.AllObjectsDestGroupSources[(int)dataHost.m_pars[0]];

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
