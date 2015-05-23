using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace ktstusql
{
    public class ktstusql : HHandlerDb
    {
        public void Initialize(ConnectionSettings connSett)
        {
        }

        public void Initialize(object [] pars)
        {
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }

        public override void StartDbInterfaces()
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
    }

    public class PlugIn : HHPlugIn
    {
        public PlugIn()
            : base()
        {
            _Id = 1002;

            createObject(typeof(ktstusql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            ktstusql target = _object as ktstusql;

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
                    target.Initialize(ev.par as object[]);
                    break;
                case (int)ID_DATA_ASKED_HOST.START:
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT) == true)
                        if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP) == true)
                            target.Start();
                        else
                            DataAskedHost((int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP);
                    else
                        DataAskedHost((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT);
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Stop();
                    break;
                default:
                    break;
            }

            base.OnEvtDataRecievedHost(obj);
        }
    }
}
