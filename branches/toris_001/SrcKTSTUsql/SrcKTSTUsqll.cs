using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using uLoaderCommon;

namespace SrcKTSTUsql
{
    public class SrcKTSTUsql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcKTSTUsql()
            : base()
        {
        }

        public SrcKTSTUsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsKTSTUsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsKTSTUsql(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN
                return new SIGNALKTSTUsql((int)objs[0]);
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsKTSTUsql(this, objs);
        }

        public override void ClearValues()
        {
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
