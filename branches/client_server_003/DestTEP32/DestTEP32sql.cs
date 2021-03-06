﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestTEP32
{
    public class DestTEP32sql : HHandlerDbULoaderIDDest //HHandlerDbULoaderStatTMMSTDest
    {
        /// <summary>
        /// Конструктор - вспомогательный (статическая сборка)
        /// </summary>
        public DestTEP32sql()
            : base()
        {
        }
        /// <summary>
        /// Конструктор - основной (динамическая загрузка)
        /// </summary>
        /// <param name="iPlugIn">Объект для связи с "родительским" приложением</param>
        public DestTEP32sql(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsTEP32sql : GroupSignalsIDDest //GroupSignalsStatTMMSTDest
        {
            public GroupSignalsTEP32sql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALIDsql(this, (int)objs[0], (int)objs[1], (int)objs[3]);
            }

            /// <summary>
            /// Формирование запроса на вставку данных
            /// </summary>
            /// <returns></returns>
            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRows = string.Empty
                    , strRow = string.Empty;
                int /*iIdToInsert = -1,*/
                    iUTCOffsetToDataTotalHours = (int)(_parent as DestTEP32sql).m_tsUTCOffsetToData.Value.TotalHours;
                DateTime? dtToInsert = null;
                bool bBreak = false; // признак аварийного завершения цикла

                //Logging.Logg().Debug(@"GroupSignalsStatIDsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                if (m_DupTables.TableDistinct.Rows.Count > 0)
                {
                    strRes = @"INSERT INTO [dbo].[NAMETABLE_INSERT_INTO] ("
                        + @"[ID_PUT]"
                        + @",[ID_USER]"
                        + @",[ID_SOURCE]"
                        + @",[DATE_TIME]"
                        + @",[ID_TIME]"
                        + @",[ID_TIMEZONE]"
                        + @",[QUALITY]"
                        + @",[VALUE]"
                        + @",[WR_DATETIME]"
                            + @") VALUES";

                    foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                    {
                        (getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim())) as List<int>).ForEach(iIdToInsert => {
                            if (dtToInsert == null)
                                dtToInsert = ((DateTime)row[@"DATETIME"]).AddHours(iUTCOffsetToDataTotalHours);
                            else
                                if (dtToInsert.Equals(((DateTime)row[@"DATETIME"]).AddHours(iUTCOffsetToDataTotalHours)) == false) {
                                    Logging.Logg().Error(@"GroupSignalsTEP32sql::getInsertValuesQuery () - в наборе различные дата/время...", Logging.INDEX_MESSAGE.NOT_SET);

                                    bBreak = true;
                                } else
                                    ;

                                if (iIdToInsert > 0) {
                                    strRow = @"(";

                                    strRow += iIdToInsert + @",";
                                    //strRow += (_parent as HHandlerDbULoaderStatTMDest).m_strIdTEC + @",";
                                    strRow += 0.ToString() + @","; //ID_USER
                                    strRow += m_IdSourceConnSett + @","; //ID_SOURCE
                                    strRow += @"'" + dtToInsert.GetValueOrDefault().ToString(s_strFormatDbDateTime) + @"',";
                                    strRow += 13.ToString() + @","; //ID_TIME = 1 ч
                                    strRow += 0.ToString() + @","; //ID_TIMEZONE = UTC
                                    strRow += 0.ToString() + @","; //QUALITY
                                    strRow += ((float)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                                    strRow += @"GETDATE()";

                                    strRow += @"),";

                                    strRows += strRow;
                                } else
                                    ; // не найдено соответствие с Id источника
                        }); // анонимная функция

                        if (bBreak == true)
                            break;
                        else
                            ;
                    }

                    if (strRows.Equals(string.Empty) == false)
                    {
                        strRes += strRows;

                        strRes = strRes.Replace(@"NAMETABLE_INSERT_INTO", (_parent as DestTEP32sql).GetNameTable(dtToInsert.GetValueOrDefault()));
                        //Лишняя ','
                        strRes = strRes.Substring(0, strRes.Length - 1);
                    }
                    else
                        strRes = string.Empty;
                }
                else
                    ; // нет строк для вставки

                return
                    //string.Empty
                    strRes
                    ;
            }

            /// <summary>
            /// Формирование запроса на выборку данных по сигналам
            /// </summary>
            /// <returns></returns>
            protected override string getExistsValuesQuery()
            {
                string strRes = string.Empty
                    , strIds = string.Empty;

                foreach (SIGNALIDsql sgnl in m_arSignals)
                    strIds += sgnl.m_idTarget + @",";
                // удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                DateTime? dtToSelect = null;
                //    // т.к. записи в таблице отсортированы по [DATE_TIME]
                //    DateTimeRangeRecieved.Set((DateTime)value.Rows[0][@"DATETIME"]
                //        , (DateTime)value.Rows[value.Rows.Count - 1][@"DATETIME"]);                
                if ((!(TableRecieved == null))
                    && (TableRecieved.Rows.Count > 0)
                    && (TableRecieved.Columns.Contains(@"DATETIME") == true))
                {                    
                    dtToSelect = ((DateTime)TableRecieved.Rows[0][@"DATETIME"]);

                    strRes = @"SELECT [ID] as [ID_REC]"
                        + @", [ID_PUT] as [ID]"
                        + @", [DATE_TIME] as [DATETIME]"
                        + @", [ID_TIMEZONE]"
                        + @", [QUALITY]"
                        + @" FROM [" + (_parent as DestTEP32sql).GetNameTable(dtToSelect.GetValueOrDefault()) + @"]"
                        + @" WHERE [DATE_TIME]='" + dtToSelect.GetValueOrDefault().ToString(s_strFormatDbDateTime) + @"'"
                            + @" AND [ID_SOURCE]=" + m_IdSourceConnSett
                            + @" AND [ID_PUT] IN (" + strIds + @")";
                }
                else
                    ;

                return strRes;
            }
        }

        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsTEP32sql(this, id, objs);
        }

        public string GetNameTable(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"DestTEP32sql::GetNameTable () - невозможно определить наименование таблицы...");
            else
                ;
            
            strRes = m_strNameTable + @"_" + dtInsert.Year.ToString () + dtInsert.Month.ToString (@"00");

            return strRes;
        }
    }
}
