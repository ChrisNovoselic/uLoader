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
            : base(@"dd/MM/yyyy HH:mm:ss")
        {
        }

        public SrcKTSTUsql(IPlugIn iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss")
        {
        }

        private class GroupSignalsKTSTUsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsKTSTUsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN
                return new SIGNALKTSTUsql((int)objs[0], bool.Parse((string)objs[2]));
            }

            protected override void setQuery()
            {
                int idReq = HMath.GetRandomNumber ()
                    , i = -1;
                string cmd = string.Empty;
                //Формировать запрос
                i = 0;
                foreach (GroupSignalsKTSTUsql.SIGNALKTSTUsql s in m_arSignals)
                {
                    if (i == 0)
                        cmd = @"List";
                    else
                        if (i == 1)
                            cmd = @"ListAdd";
                        else
                            ; // оставить без изменений

                    m_strQuery += @"exec e6work.dbo.ep_AskVTIdata @cmd='" + cmd + @"',"
                        + @"@idVTI=" + s.m_idMain + @","
                        + @"@TimeStart='" + DateTimeBeginFormat + @"',"
                        + @"@TimeEnd='" + DateTimeEndFormat + @"',"
                        + @"@idReq=" + idReq
                        + @";";

                    i ++;
                }

                m_strQuery += @"SELECT idVTI as [ID],idReq,TimeIdx,TimeRTC,TimeSQL as [DATETIME],idState,ValueFl as [VALUE],ValueInt,IsInteger,idUnit"
                    + @" FROM e6work.dbo.VTIdataList"
                    + @" WHERE idReq=" + idReq
                    + @";";


                m_strQuery += @"exec e6work.dbo.ep_AskVTIdata @cmd='" + @"Clear" + @"',"
                    + @"@idReq=" + idReq
                    + @";";
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsKTSTUsql(this, id, objs);
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
