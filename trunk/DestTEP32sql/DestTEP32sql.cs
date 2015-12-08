using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestTEP32sql
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
        public DestTEP32sql(IPlugIn iPlugIn)
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
                return new SIGNALIDsql((int)objs[0], (int)objs[1], (int)objs[3]);
            }

            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRows = string.Empty
                    , strRow = string.Empty;
                int iIdToInsert = -1;
                DateTime? dtToInsert = null;

                //Logging.Logg().Debug(@"GroupSignalsStatIDsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                if (m_DupTables.TableDistinct.Rows.Count > 0)
                {
                    strRes = @"INSERT INTO [dbo].[NAMETABLE_INSERT_INTO] ("
                        + @"[ID_INPUT]"
                        + @",[ID_USER]"
                        + @",[ID_SOURCE]"
                        + @",[DATE_TIME]"
                        + @",[QUALITY]"
                        + @",[VALUE]"
                        + @",[WR_DATETIME]"
                            + @") VALUES";

                    foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                    {
                        iIdToInsert = (int)getIdTarget(Int32.Parse(row[@"ID"].ToString().Trim()));
                        if (dtToInsert == null)
                            dtToInsert = ((DateTime)row[@"DATETIME"]).AddHours(0);
                        else
                            if (dtToInsert.Equals(((DateTime)row[@"DATETIME"]).AddHours(0)) == false)
                            {
                                Logging.Logg().Error(@"GroupSignalsTEP32sql::getInsertValuesQuery () - в наборе различные дата/время...", Logging.INDEX_MESSAGE.NOT_SET);

                                break;
                            }
                            else
                                ;

                        if (iIdToInsert > 0)
                        {
                            strRow = @"(";

                            strRow += iIdToInsert + @",";
                            //strRow += (_parent as HHandlerDbULoaderStatTMDest).m_strIdTEC + @",";
                            strRow += 0.ToString() + @","; //ID_USER
                            strRow += m_IdSourceConnSett + @","; //ID_SOURCE
                            strRow += @"'" + dtToInsert.GetValueOrDefault().ToString(s_strFormatDbDateTime) + @"',";
                            strRow += 0.ToString() + @","; //QUALITY
                            strRow += ((float)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                            strRow += @"GETDATE()";

                            strRow += @"),";

                            strRows += strRow;
                        }
                        else
                            ; // не найдено соответствие с Id источника
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

            protected override string getExistsValuesQuery()
            {
                string strRes = string.Empty;

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
                        + @", [ID_INPUT] as [ID]"
                        + @", [DATE_TIME] as [DATETIME]"
                        + @", [QUALITY]"
                        + @" FROM [" + (_parent as DestTEP32sql).GetNameTable(dtToSelect.GetValueOrDefault()) + @"]"
                        + @" WHERE [DATE_TIME]='" + dtToSelect.GetValueOrDefault().ToString(s_strFormatDbDateTime) + @"'"
                            + @" AND [ID_SOURCE]=" + m_IdSourceConnSett;
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
            
            strRes = m_strNameTable + @"_" + dtInsert.Year.ToString () + (dtInsert.Month + 0).ToString ();

            return strRes;
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2006;

            createObject(typeof(DestTEP32sql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
