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
    public class DestStatKKSNAMEsql : HHandlerDbULoaderStatTMKKSNAMEDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO_KKS"
        //    , s_strIdTEC = @"6"
        //    , s_strID_SRV_TM = @"2";

        public DestStatKKSNAMEsql()
            : base()
        {
        }

        public DestStatKKSNAMEsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatKKSNAMEsql : HHandlerDbULoaderStatTMMSTDest.GroupSignalsStatTMMSTDest
        {
            public GroupSignalsStatKKSNAMEsql(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                //Logging.Logg().Debug(@"GroupSignalsStatKKSNAMEsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

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
                    if (((int)getIdToInsert(Int32.Parse(row[@"ID"].ToString().Trim()))) == 0)
                    {
                        strRow = @"(";

                        strRow += @"'" + row[@"KKSNAME_MST"] + @"'" + @",";
                        strRow += (_parent as HHandlerDbULoaderStatTMDest).m_strIdTEC + @",";
                        strRow += ((double)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                        strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(0).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"',";
                        strRow += row[@"tmdelta"] + @",";
                        strRow += @"GETDATE()" + @",";
                        strRow += (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).m_strIdSource + @",";
                        strRow += (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).m_strIdSrvTM;

                        strRow += @"),";

                        strRes += strRow;
                    }
                    else
                        ; // не найдено соответствие с Id источника
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
            _Id = 2002;

            createObject(typeof(DestStatKKSNAMEsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
