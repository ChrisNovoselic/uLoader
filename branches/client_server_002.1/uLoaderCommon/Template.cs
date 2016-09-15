using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

//Источник
namespace uLoaderCommonSrcTemplate
{
    public class SrcTemplate : HHandlerDbULoaderSrc
    {
        public SrcTemplate()
            : base()
        {
        }

        public SrcTemplate(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsBiyskTMOra : GroupSignalsSrc
        {
            public GroupSignalsBiyskTMOra(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected class SIGNALTemplate : SIGNAL
            {
                public SIGNALTemplate(int idMain)
                    : base(idMain)
                {
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALTemplate((int)objs[0]);
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
            _Id = -666;

            createObject(typeof(SrcTemplate));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}

//Назначение
namespace uLoaderCommonDestTemplate
{
    public class DestTemplate : HHandlerDbULoaderDest
    {
        public DestTemplate()
            : base()
        {
        }

        public DestTemplate(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsTemplateDest : GroupSignalsDest
        {
            public GroupSignalsTemplateDest(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected class SIGNALTemplateDest : GroupSignalsDest.SIGNALDest
            {
                public SIGNALTemplateDest(int idMain, int idLink)
                    : base(idMain, idLink)
                {
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALTemplateDest((int)objs[0], (int)objs[1]);
            }

            protected override DataTable getTableRes()
            {
                throw new NotImplementedException();
            }

            protected override DataTable getTableIns(ref DataTable table)
            {
                throw new NotImplementedException();
            }

            protected override object getIdToInsert(int idLink)
            {
                object objRes = null;

                return objRes;
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                return
                    //string.Empty
                    strRes
                    ;
            }
        }

        protected override GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsTemplateDest(this, objs);
        }
    }

    public class PlugIn : uLoaderCommon.PlugInULoaderDest
    {
        public PlugIn()
            : base()
        {
            _Id = -666;

            createObject(typeof(DestTemplate));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }

}
