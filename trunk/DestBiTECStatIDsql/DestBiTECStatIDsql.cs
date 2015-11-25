using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestBiTECStatIDsql
{
    public class DestBiTECStatIDsql : HHandlerDbULoaderStatTMMSTDest //HHandlerDbULoaderStatTMIDDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO"
        //    , s_strIdTEC = @"6";

        public DestBiTECStatIDsql()
            : base()
        {
        }

        public DestBiTECStatIDsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatIDsql : GroupSignalsStatTMMSTDest //GroupSignalsStatTMIDDest
        {
            public GroupSignalsStatIDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override DataTable getTableIns(ref DataTable table)
            {
                return new TableInsTMDelta(table, TableRecievedPrev, Signals).Result;
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                strRes = @"INSERT INTO [dbo].[" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"] ("
                    + @"[ID]"
                    + @",[ID_TEC]"
                    + @",[Value]"
                    + @",[last_changed_at]"
                    + @",[tmdelta]"
                    + @",[INSERT_DATETIME]"
                        + @") VALUES";

                foreach (DataRow row in tblRes.Rows)
                {
                    strRow = @"(";

                    strRow += getIdToInsert(Int32.Parse(row[@"ID"].ToString().Trim())) + @",";
                    strRow += (_parent as HHandlerDbULoaderStatTMDest).m_strIdTEC + @",";
                    strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                    strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(-6).ToString(s_strFormatDbDateTime) + @"',";
                    strRow += row[@"tmdelta"] + @",";
                    strRow += @"GETDATE()";

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

            protected override string getExistsValuesQuery()
            {
                return string.Empty;
            }
        }

        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsStatIDsql(this, id, objs);
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2003;

            createObject(typeof(DestBiTECStatIDsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
