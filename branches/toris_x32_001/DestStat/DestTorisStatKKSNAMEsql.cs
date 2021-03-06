﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;
using System.Diagnostics;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP;

namespace DestStat
{
    public class DestTorisStatKKSNAMEsql : HHandlerDbULoaderStatTMKKSNAMEDest //HHandlerDbULoaderStatTMKKSNAMEDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO_KKS"
        //    , s_strIdTEC = @"6"
        //    , s_strID_SRV_TM = @"2";

        public DestTorisStatKKSNAMEsql()
            : base()
        {
        }

        public DestTorisStatKKSNAMEsql(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsTorisStatKKSNAMEsql : HHandlerDbULoaderStatTMKKSNAMEDest.GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsTorisStatKKSNAMEsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            //protected override void setTableRes()
            //{
            //    //Заполнить таблицы с повторяющимися/уникальными записями
            //    base.setTableRes();
            //    //добавить поле [tmdelta]
            //    (m_DupTables as DataTableDuplicateTMDelta).Convert(TableRecievedPrev, Signals);
            //}

            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRow = string.Empty;
                int idSrvTM = (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).GetIdSrvTM(m_IdSourceConnSett);

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
                foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                {
                    //Debug.Print("Записано в БД: " + row[@"KKSNAME_MST"] + " " + ((double)row[@"VALUE"]).ToString("F4", CultureInfo.InvariantCulture) + " " + ((DateTime)row[@"DATETIME"]).AddHours(0).ToString(s_strFormatDbDateTime)+".");
                    if (((string)getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim()))).Length > 0)
                    {
                        strRow = @"(";

                        strRow += @"'" + row[@"KKSNAME_MST"] + @"'" + @",";
                        strRow += m_IdSourceTEC + @",";
                        strRow += ((double)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                        strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(0).ToString(s_strFormatDbDateTime) + @"',";
                        //strRow += "0" + @",";
                        strRow += row[@"tmdelta"] + @",";
                        strRow += @"GETDATE()" + @",";
                        strRow += m_IdSourceConnSett + @",";
                        strRow += idSrvTM;

                        strRow += @"),";

                        strRes += strRow;
                    }
                    else
                        ; // не найдено соответствие с Id источника
                }
                Logging.Logg().Action("Записано в БД " + DateTime.Now.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
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
            return new GroupSignalsTorisStatKKSNAMEsql(this, id, objs);
        }
    }
}