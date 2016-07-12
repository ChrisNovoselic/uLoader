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
    public class DestBiTECStatIDsql : HHandlerDbULoaderStatTMDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO"
        //    , s_strIdTEC = @"6";

        public DestBiTECStatIDsql()
            : base()
        {
        }

        public DestBiTECStatIDsql(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatIDsql : GroupSignalsStatTMDest
        {
            public GroupSignalsStatIDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new GroupSignalsDest.SIGNALIDsql(this, (int)objs[0], (int)objs[1], (int)objs[3]);
            }

            protected override void setTableRes()
            {
                //Заполнить таблицы с повторяющимися/уникальными записями
                base.setTableRes();
                //добаить поле [tmdelta]
                (m_DupTables as DataTableDuplicateTMDelta).Convert(TableRecievedPrev, Signals);
            }

            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRow = string.Empty;
                int iUTCOffsetToDataTotalHours = (int)(_parent as DestBiTECStatIDsql).m_tsUTCOffsetToData.Value.TotalHours;

                strRes = @"INSERT INTO [dbo].[" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"] ("
                    + @"[ID]"
                    + @",[ID_TEC]"
                    + @",[Value]"
                    + @",[last_changed_at]"
                    + @",[tmdelta]"
                    + @",[INSERT_DATETIME]"
                        + @") VALUES";

                foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                {
                    strRow = @"(";

                    strRow += getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim())) + @",";
                    strRow += m_IdSourceTEC + @",";
                    strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                    strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(iUTCOffsetToDataTotalHours).ToString(s_strFormatDbDateTime) + @"',";
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
}
