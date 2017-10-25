using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

////using HClassLibrary;
using uLoaderCommon;

namespace SrcVzlet
{
    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 109;

            registerType(10914, typeof(SrcVzletSql));
            registerType(10915, typeof(SrcVzletDsql));
            registerType(10916, typeof(SrcVzletNativeSql));
            registerType(10917, typeof(SrcVzletNativeDSql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
