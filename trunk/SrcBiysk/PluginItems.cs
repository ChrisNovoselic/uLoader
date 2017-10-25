using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

////using HClassLibrary;
using uLoaderCommon;

namespace SrcBiysk
{
    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 106;

            registerType(10101, typeof(SrcBiyskTMora));            
            registerType(10105, typeof(SrcBiyskTMLastora));
            registerType(10106, typeof(SrcBiyskDiscrLastora));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
