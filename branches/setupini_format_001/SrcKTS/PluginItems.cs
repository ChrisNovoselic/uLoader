using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using HClassLibrary;
using uLoaderCommon;

namespace SrcKTS
{
    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 102;

            registerType(10202, typeof(SrcKTSTUsql));
            registerType(10209, typeof(SrcKTSTUDsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
