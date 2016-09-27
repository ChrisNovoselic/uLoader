using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMST
{
    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1003;

            registerType(10303, typeof(SrcMSTASUTPIDT5tg1sql));
            registerType(10304, typeof(SrcMSTKKSNAMEsql));
            registerType(10307, typeof(SrcMSTKKSNAMEtoris));
            registerType(10308, typeof(SrcMSTASUTPIDT5tg6sql));
            registerType(10309, typeof(SrcMSTASUTPIdT5tg1Dsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
