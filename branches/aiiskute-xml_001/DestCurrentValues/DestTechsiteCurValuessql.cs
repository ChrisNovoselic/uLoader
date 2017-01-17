using System;
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
                    , strValues = string.Empty
                    , strIdTarget = string.Empty;
                Type typeVal = m_DupTables.TableDistinct.Columns[@"VALUE"].DataType;
                int idSrvTM = (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).GetIdSrvTM(m_IdSourceConnSett)
                    , iOffsetUTCToDataTotalHours = (int)(_parent as DestTechsiteCurValuessql).m_tsOffsetUTCToData.Value.TotalHours;
                //HTimeSpan tsUTCOffset = _parent.m_tsUTCOffset == HTimeSpan.NotValue ? new HTimeSpan(@"ss0") : _parent.m_tsUTCOffset; //??? OFFSET

                //Logging.Logg().Debug(@"GroupSignalsStatKKSNAMEsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                if (m_DupTables.TableDistinct.Rows.Count > 0)
                {
                    strRes = @"DECLARE @VALUES_TABLE AS TABLE([KKS_NAME] [nvarchar](256) NOT NULL, [VALUE] [real] NOT NULL, [DATETIME] [datetime] NOT NULL, [UPDATE_DATETIME] [datetime] NOT NULL, [ID_SRV_TM] [int] NOT NULL);";
                    strRes += @"INSERT INTO @VALUES_TABLE([KKS_NAME],[VALUE],[DATETIME],[UPDATE_DATETIME],[ID_SRV_TM])"
                        + @" SELECT [KKS_NAME],[VALUE],[DATETIME], GETDATE() AS [UPDATE_DATETIME], " + idSrvTM + @" AS [ID_SRV_TM] FROM (VALUES ";

                    foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                    {
                        strIdTarget = (string)getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim()));

                        if (strIdTarget.Equals(string.Empty) == false)
                        {
                            strValues += @"(";

                            strValues += @"'" + strIdTarget + @"'" + @",";

                            if (typeVal.Equals(typeof(decimal)) == true)
                                strValues += ((decimal)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                            else
                                if (typeVal.Equals(typeof(double)) == true)
                                    strValues += ((double)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                                else
                                    strValues += row[@"VALUE"];
                            strValues += @",";

                            strValues += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(iOffsetUTCToDataTotalHours).ToString(s_strFormatDbDateTime) + @"'" + @"),";
                        }
                        else
                            ;
                    }
                    //Лишняя ','
                    if (strValues.Equals(string.Empty) == false)
                    {
                        strValues = strValues.Substring(0, strValues.Length - 1);

                        strRes += strValues;
                    }
                    else
                        ;

                    strRes += @") AS [TORIS_SOURCE]([KKS_NAME], [VALUE], [DATETIME]);";

                    strRes += @" MERGE [dbo].[" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"] AS [T]"
                        + @" USING @VALUES_TABLE AS [S]"
                        + @" ON ([T].[KKS_NAME] = [S].[KKS_NAME])"
                            + @" WHEN MATCHED AND ([S].[DATETIME] > [T].[DATETIME])"
                            + @" THEN UPDATE SET [VALUE] = [S].[VALUE], [DATETIME] = [S].[DATETIME], [UPDATE_DATETIME] = [S].[UPDATE_DATETIME], [ID_SRV_TM] = [S].[ID_SRV_TM];";
                }

                return strValues.Equals(string.Empty) == true ? string.Empty : strRes;
            }

            protected override void setTableRes()
            {
                m_DupTables.Top(TableRecievedPrev, TableRecieved);
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
