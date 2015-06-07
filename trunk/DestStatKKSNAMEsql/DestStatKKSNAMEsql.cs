using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestStatKKSNAMEsql
{
    public class DestStatKKSNAMEsql : HHandlerDbULoaderStatTMDest
    {
        private static string m_strNameDestTable = @"ALL_PARAM_SOTIASSO_KKS";

        public DestStatKKSNAMEsql()
            : base()
        {
        }

        public DestStatKKSNAMEsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatKKSNAMEsql : GroupSignalsStatTMDest
        {
            public GroupSignalsStatKKSNAMEsql(object []pars) : base (pars)
            {
            }
            
            protected class SIGNALStatKKSNAMEsql : GroupSignalsDest.SIGNALDest
            {
                public string m_strStatKKSName;

                public SIGNALStatKKSNAMEsql(int idMain, int idLink, string statKKSName)
                    : base(idMain, idLink)
                {
                    m_strStatKKSName = statKKSName;
                }
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALStatKKSNAMEsql((int)objs[0], (int)objs[1], (string)objs[4]);
            }

            protected override object getIdToInsert(int idLink)
            {
                string strRes = string.Empty;

                foreach (SIGNALStatKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        strRes = sgnl.m_strStatKKSName;

                        break;
                    }
                    else
                        ;

                return strRes;
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                strRes = @"INSERT INTO [dbo].[" + m_strNameDestTable + @"] ("
                    + @"[KKS_NAME]"
                    + @",[ID_TEC]"
                    + @",[Value]"
                    + @",[last_changed_at]"
                    + @",[tmdelta]"
                    + @",[INSERT_DATETIME]"
                    + @",[ID_SOURCE]"
                    + @",[ID_SRV_TM]"
                        + @") VALUES";

                foreach (DataRow row in tblRes.Rows)
                {
                    strRow = @"(";

                    strRow += @"'" + getIdToInsert(Int32.Parse(row[@"ID"].ToString().Trim())) + @"'" + @",";
                    strRow += @"6" + @",";
                    strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                    strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(-6).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"',";
                    strRow += row[@"tmdelta"] + @",";
                    strRow += @"GETDATE()" + @",";
                    strRow += @"63" + @",";
                    strRow += @"2";

                    strRow += @"),";

                    strRes += strRow;
                }
                //Лишняя ','
                strRes = strRes.Substring(0, strRes.Length - 1);

                return
                    //string.Empty
                    strRes
                    ;
            }
        }

        protected override GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsStatKKSNAMEsql(objs);
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2002;

            createObject(typeof(DestStatKKSNAMEsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
