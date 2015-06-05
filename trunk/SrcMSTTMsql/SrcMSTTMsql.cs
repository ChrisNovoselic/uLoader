﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMSTTMsql
{
    public class SrcMSTTMsql : HHandlerDbULoaderSrc
    {
        private class GroupSignalsBiyskTMOra : GroupSignalsSrc
        {
            public GroupSignalsBiyskTMOra(object[] pars)
                : base(pars)
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
            return new GroupSignalsBiyskTMOra(objs);
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
            _Id = 1001;

            createObject(typeof(SrcMSTTMsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
