using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestStat
{
    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 201;

            registerType(20101, typeof(DestStatIDsql));
            registerType(20102, typeof(DestStatKKSNAMEsql));
            registerType(20107, typeof(DestTorisStatKKSNAMEsql));

            registerType(20103, typeof(DestBiTECStatIDsql));
            registerType(20104, typeof(DestBiTECStatKKSNAMEsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
