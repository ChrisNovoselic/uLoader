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
            LIST_SRC_GROUP_SOURCES
            , LIST_SRC_GROUP_SOURCE_ITEMS
            , LIST_SRC_GROUP_SIGNALS
            , LIST_SRC_GROUP_SIGNAL_ITEMS
            , LIST_DEST_GROUP_SOURCES
            , LIST_DEST_GROUP_SOURCE_ITEMS
            , LIST_DEST_GROUP_SIGNALS
            , LIST_DEST_GROUP_SIGNAL_ITEMS
        }

        private FormMain.FileINI m_fileINI;

        public HHandlerQueue()
            : base ()
        {
            m_fileINI = new FormMain.FileINI();
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCES:
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCES:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
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
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCES:
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNALS:
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
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

            switch (state)
            {
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCES:
                    error = false;
                    outobj = m_fileINI.ListSrcGroupSources;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                    error = false;
                    //outobj = m_fileINI.GetListSrcGroupSourceItems();

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNALS:
                    error = false;
                    outobj = m_fileINI.ListSrcGroupSignals;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                    error = false;
                    //outobj = m_fileINI.GetListSrcGroupSignalItems();

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCES:
                    error = false;
                    outobj = m_fileINI.ListDestGroupSources;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                    error = false;
                    //outobj = m_fileINI.GetListDestGroupSourceItems();

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNALS:
                    error = false;
                    outobj = m_fileINI.ListDestGroupSignals;

                    iRes = 0;
                    break;
                case (int)StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:                    
                    error = false;
                    //outobj = m_fileINI.GetListDestGroupSignalItems();

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
