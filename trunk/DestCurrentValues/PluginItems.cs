using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

////using HClassLibrary;
using uLoaderCommon;

namespace DestCurrentValues
{
    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 205;

            //registerType(20501, typeof(DestStatCurValuessql));
            registerType(20502, typeof(DestTechsiteCurValuessql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
