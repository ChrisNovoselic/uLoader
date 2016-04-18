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
    public class DestStatCurValuessql : HHandlerDbULoaderStatTMKKSNAMEDest
    {
        /// <summary>
        /// Конструктор - вспомогательный (статическая сборка)
        /// </summary>
        public DestStatCurValuessql()
            : base()
        {
        }
        /// <summary>
        /// Конструктор - основной (динамическая загрузка)
        /// </summary>
        /// <param name="iPlugIn">Объект для связи с "родительским" приложением</param>
        public DestStatCurValuessql(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsTechSiteLastsql : GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsTechSiteLastsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
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
                    , iUTCOffsetToDataTotalHours = (int)(_parent as DestStatCurValuessql).m_tsUTCOffsetToData.Value.TotalHours;
                HTimeSpan tsUTCOffset = _parent.m_tsUTCOffset == HTimeSpan.NotValue ? new HTimeSpan(@"ss0") : _parent.m_tsUTCOffset;

                //Logging.Logg().Debug(@"GroupSignalsStatKKSNAMEsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                strRow = @"UPDATE [" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"]"
                            //+ @"SET [ID_SRV_TM]=" + idSrvTM + @",";
                            + @" SET ";

                foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                {
                    strRes += strRow;

                    strRes += @"[VALUE]='";
                    if (typeVal.Equals(typeof (decimal)) == true)
                        strRes += ((decimal)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                    else
                        if(typeVal.Equals(typeof (double)) == true)
                            strRes += ((double)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                        else
                            strRes += row[@"VALUE"];
                    strRes +=  @"',";
                    strRes += @"[DATETIME]='" + ((DateTime)row[@"DATETIME"]).AddHours(iUTCOffsetToDataTotalHours).ToString(s_strFormatDbDateTime) + @"'" + @",";
                    strRes += @"[UPDATE_DATETIME]=GETDATE()";

                    //strRes += @" WHERE [KKS_NAME]='" + (string)getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim())) + @"';";
                    strRes += @" WHERE [ID_SIGNAL]='" + (string)getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim())) + @"';";

                }

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
            return new GroupSignalsTechSiteLastsql(this, id, objs);
        }
    }
}
