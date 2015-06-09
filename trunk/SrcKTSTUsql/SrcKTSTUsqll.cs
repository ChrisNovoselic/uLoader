﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using uLoaderCommon;

namespace SrcKTSTUsql
{
    public class SrcKTSTUsql : HHandlerDbULoaderSrc
    {
        public SrcKTSTUsql()
            : base()
        {
        }

        public SrcKTSTUsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsBiyskTMOra : GroupSignalsSrc
        {
            public GroupSignalsBiyskTMOra(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            public class SIGNALMSTTMsql : SIGNAL
            {
                public SIGNALMSTTMsql(int idMain)
                    : base(idMain)
                {
                }
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALMSTTMsql((int)objs[0]);
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsBiyskTMOra(this, objs);
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
