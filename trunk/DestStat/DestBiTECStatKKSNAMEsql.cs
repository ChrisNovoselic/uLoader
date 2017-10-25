using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

////using HClassLibrary;
using uLoaderCommon;

namespace DestStat
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

        public DestBiTECStatKKSNAMEsql(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatKKSNAMEsql : GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsStatKKSNAMEsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            //protected override void setTableRes()
            //{
            //    //Заполнить таблицы с повторяющимися/уникальными записями
            //    base.setTableRes();
            //    //добаить поле [tmdelta]
            //    (m_DupTables as DataTableDuplicateTMDelta).Convert(TableRecievedPrev, Signals);
            //}

            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRow = string.Empty;
                int idSrvTM = (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).GetIdSrvTM(m_IdSourceConnSett)
                    , iOffsetUTCToDataTotalHours = (int)(_parent as DestBiTECStatKKSNAMEsql).m_tsOffsetUTCToData.Value.TotalHours;

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

                foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                {
                    strRow = @"(";

                    strRow += @"'" + getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim())) + @"'" + @",";
                    strRow += m_IdSourceTEC + @",";
                    strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                    strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(iOffsetUTCToDataTotalHours).ToString(s_strFormatDbDateTime) + @"',";
                    strRow += row[@"tmdelta"] + @",";
                    strRow += @"GETDATE()" + @",";
                    strRow += m_IdSourceConnSett + @",";
                    strRow += idSrvTM;

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
            return new GroupSignalsStatKKSNAMEsql(this, id, objs);
        }
    }
}
