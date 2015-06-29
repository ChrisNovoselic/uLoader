using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestBiTECStatKKSNAMEsql
{
    public class DestBiTECStatKKSNAMEsql : HHandlerDbULoaderStatTMKKSNAMEDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO_KKS"
        //    , s_strIdTEC = @"6"
        //    , s_strID_SRV_TM = @"2";

        public DestBiTECStatKKSNAMEsql()
            : base()
        {
        }

        public DestBiTECStatKKSNAMEsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatKKSNAMEsql : GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsStatKKSNAMEsql(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected override DataTable getTableIns(ref DataTable table)
            {
                return new TableInsTMDelta ().Result (table, TableRecievedPrev, Signals);
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                strRes = @"INSERT INTO [dbo].[" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"] ("
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
                    strRow += (_parent as HHandlerDbULoaderStatTMDest).m_strIdTEC + @",";
                    strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                    strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(-6).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"',";
                    strRow += row[@"tmdelta"] + @",";
                    strRow += @"GETDATE()" + @",";
                    strRow += (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).m_strIdSource + @",";
                    strRow += (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).m_strIdSrvTM;

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
            return new GroupSignalsStatKKSNAMEsql(this, objs);
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2004;

            createObject(typeof(DestBiTECStatKKSNAMEsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
