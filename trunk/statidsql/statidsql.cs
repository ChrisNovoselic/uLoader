using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace statidsql
{
    public class statidsql : HHandlerDb
    {
        protected IPlugIn _iPlugin;

        public statidsql()
        {
        }

        public statidsql(IPlugIn iPlugIn)
            : this()
        {
            this._iPlugin = iPlugIn;
        }
        
        public override void Start()
        {
            base.Start();
        }
        
        public override void StartDbInterfaces()
        {
            throw new NotImplementedException();
        }

        public int Initialize(ConnectionSettings connSett)
        {
            int iRes = 0;

            return iRes;
        }

        public int Initialize(int id, object[] pars)
        {
            int iRes = 0;

            return iRes;
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }

        protected override int StateCheckResponse(int state, out bool error, out object outobj)
        {
            throw new NotImplementedException();
        }

        protected override int StateRequest(int state)
        {
            throw new NotImplementedException();
        }

        protected override int StateResponse(int state, object obj)
        {
            throw new NotImplementedException();
        }

        protected override void StateErrors(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public void Stop(int id)
        {
            Stop();
        }
    }

    public class PlugIn : HHPlugIn
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2001;

            createObject(typeof(statidsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            statidsql target = _object as statidsql;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT:
                    ConnectionSettings connSett = ev.par[0] as ConnectionSettings;
                    target.Initialize(new ConnectionSettings(
                        connSett.name
                        , connSett.server
                        , connSett.port
                        , connSett.dbName
                        , connSett.userName
                        , connSett.password
                    ));
                    break;
                case (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP:
                    target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]);
                    break;
                case (int)ID_DATA_ASKED_HOST.START:
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT) == true)
                    {
                        if (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0)
                        {
                            target.Start();
                            target.Activate(true);
                        }
                        else
                            DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP, (int)(ev.par as object[])[0] });
                    }
                    else
                        DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT });
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Stop((int)(ev.par as object[])[0]);
                    break;
                default:
                    break;
            }

            base.OnEvtDataRecievedHost(obj);
        }
    }
}
