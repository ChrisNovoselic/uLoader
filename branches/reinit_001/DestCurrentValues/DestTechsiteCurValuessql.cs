﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestCurrentValues
{
    public class DestTechsiteCurValuessql : HHandlerDbULoaderStatTMKKSNAMEDest
    {
        /// <summary>
        /// Конструктор - вспомогательный (статическая сборка)
        /// </summary>
        public DestTechsiteCurValuessql()
            : base()
        {
        }
        /// <summary>
        /// Конструктор - основной (динамическая загрузка)
        /// </summary>
        /// <param name="iPlugIn">Объект для связи с "родительским" приложением</param>
        public DestTechsiteCurValuessql(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsTechSiteLastsql : GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsTechSiteLastsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                s_strFormatDbDateTime = @"yyyyMMdd HH:mm:ss.fff";
            }

            //protected override object getIdToInsert(int idLink)
            //{
            //    throw new NotImplementedException();
            //}

            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRow = string.Empty;
                Type typeVal = m_DupTables.TableDistinct.Columns[@"VALUE"].DataType;
                int idSrvTM = (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).GetIdSrvTM(m_IdSourceConnSett)
                    , iUTCOffsetToDataTotalHours = (int)(_parent as DestTechsiteCurValuessql).m_tsUTCOffsetToData.Value.TotalHours;
                HTimeSpan tsUTCOffset = _parent.m_tsUTCOffset == HTimeSpan.NotValue ? new HTimeSpan(@"ss0") : _parent.m_tsUTCOffset;

                //Logging.Logg().Debug(@"GroupSignalsStatKKSNAMEsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                if (m_DupTables.TableDistinct.Rows.Count > 0)
                {
                    strRes = @"DECLARE @VALUES_TABLE AS TABLE([KKS_NAME] [nvarchar](256) NOT NULL, [VALUE] [real] NOT NULL, [DATETIME] [datetime] NOT NULL, [UPDATE_DATETIME] [datetime] NOT NULL, [ID_SRV_TM] [int] NOT NULL);";
                    strRes += @"INSERT INTO @VALUES_TABLE([KKS_NAME],[VALUE],[DATETIME],[UPDATE_DATETIME],[ID_SRV_TM])"
                        + @" SELECT [KKS_NAME],[VALUE],[DATETIME], GETDATE() AS [UPDATE_DATETIME], " + idSrvTM + @" AS [ID_SRV_TM] FROM (VALUES ";

                    foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                    {
                        strRes += @"(";

                        strRes += @"'" + (string)getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim())) + @"'" + @",";

                        if (typeVal.Equals(typeof(decimal)) == true)
                            strRes += ((decimal)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                        else
                            if (typeVal.Equals(typeof(double)) == true)
                                strRes += ((double)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                            else
                                strRes += row[@"VALUE"];
                        strRes += @",";

                        strRes += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(iUTCOffsetToDataTotalHours).ToString(s_strFormatDbDateTime) + @"'" + @"),";
                    }

                    //Лишняя ','
                    strRes = strRes.Substring(0, strRes.Length - 1);

                    strRes += @")AS [TORIS_SOURCE]([KKS_NAME], [VALUE], [DATETIME]);";

                    strRes += @"MERGE [WEB_TECHSITE].[dbo].[TECHSITE_OIK_CURRENT] AS [T]"
                        + @" USING @VALUES_TABLE AS [S]"
                        + @" ON ([T].[KKS_NAME] = [S].[KKS_NAME])"
                            + @" WHEN MATCHED AND ([S].[DATETIME] > [T].[DATETIME])"
                            + @" THEN UPDATE SET [VALUE] = [S].[VALUE], [DATETIME] = [S].[DATETIME], [UPDATE_DATETIME] = [S].[UPDATE_DATETIME], [ID_SRV_TM] = [S].[ID_SRV_TM];";
                }

                return
                    //string.Empty
                    strRes
                    ;
            }

            protected override void setTableRes()
            {
                Top(TableRecievedPrev, TableRecieved);
            }

            protected override string getExistsValuesQuery()
            {
                return string.Empty;
            }
        }

        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsTechSiteLastsql(this, id, objs);
        }
    }
}
