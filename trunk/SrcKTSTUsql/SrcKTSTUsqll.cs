using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using uLoaderCommon;

namespace SrcKTSTUsql
{
    public class SrcKTSTUsql : HHandlerDbULoader
    {
        public void Initialize(ConnectionSettings connSett)
        {
        }

        public void Initialize(int id, object [] pars)
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

        protected override int addAllStates()
        {
            int iRes = 0;

            return iRes;
        }

        protected override GroupSignals createGroupSignals(object[] objs)
        {
            throw new NotImplementedException();
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1002;

            createObject(typeof(SrcKTSTUsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
