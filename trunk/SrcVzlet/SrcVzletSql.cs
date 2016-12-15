using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace SrcVzlet
{
    class SrcVzletSql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcVzletSql()
            : base(@"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {

        }

        public SrcVzletSql(PlugInULoader iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        private class GroupSignalsVzletSql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsVzletSql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {

            }
            /// <summary>
            /// Установить содержание для запроса
            /// </summary>
            protected override void setQuery()
            {
                m_strQuery = string.Empty;
                string strSumIds = string.Empty
                    , strAVGIds = string.Empty;

                long secUTCOffsetToData = m_msecUTCOffsetToData / 1000;
                long secUTCOffsetToServer = m_msecUTCOffsetToServer / 1000;

                DateTime dt_begin, dt_end;

                if (PeriodMain.Days >= 1)
                {
                    dt_begin = DateTime.Parse(DateTimeBeginFormat).Date;
                    //dt_begin = dt_begin.AddHours(-dt_begin.Hour);
                    //dt_begin = dt_begin.AddMinutes(-dt_begin.Minute);
                    //dt_begin = dt_begin.AddSeconds(-dt_begin.Second);
                    //dt_begin = dt_begin.AddMilliseconds(-dt_begin.Millisecond);
                    dt_begin = dt_begin.AddSeconds(- secUTCOffsetToData + secUTCOffsetToServer);
                    //dt_begin = dt_begin.AddDays(PeriodMain.Days);

                    dt_end = DateTime.Parse(DateTimeEndFormat).Date;
                    //dt_end = dt_end.AddHours(-dt_end.Hour);
                    //dt_end = dt_end.AddMinutes(-dt_end.Minute);
                    //dt_end = dt_end.AddSeconds(-dt_end.Second);
                    //dt_end = dt_end.AddMilliseconds(-dt_end.Millisecond);
                    dt_end = dt_end.AddSeconds(- secUTCOffsetToData + secUTCOffsetToServer);
                    //dt_end = dt_begin.AddDays(PeriodMain.Days);
                } else {
                    dt_begin = DateTime.Parse(DateTimeBeginFormat);
                    //dt_begin = dt_begin.AddMinutes(-dt_begin.Minute);
                    //dt_begin = dt_begin.AddSeconds(-dt_begin.Second);
                    //dt_begin = dt_begin.AddMilliseconds(-dt_begin.Millisecond);
                    dt_begin = new DateTime((long)(Math.Floor((dt_begin.Ticks / 10000000) / (decimal)(60 * 60)) * (60 * 60)) * 10000000);
                    //dt_begin = dt_begin.AddSeconds(-secUTCOffsetToData + secUTCOffsetToServer);

                    dt_end = DateTime.Parse(DateTimeEndFormat);
                    //dt_end = dt_end.AddMinutes(-dt_end.Minute);
                    //dt_end = dt_end.AddSeconds(-dt_end.Second);
                    //dt_end = dt_end.AddMilliseconds(-dt_end.Millisecond);
                    dt_end = new DateTime((long)(Math.Floor((dt_end.Ticks / 10000000) / (decimal)(60 * 60)) * (60 * 60)) * 10000000);
                    //dt_end = dt_end.AddSeconds(-secUTCOffsetToData + secUTCOffsetToServer);
                }

                foreach (SIGNALVzletKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        if (sgnl.m_bAVG == true)
                            strSumIds += @"'" + sgnl.m_kks_name + @"',";
                        else
                            if (sgnl.m_bAVG == false)
                                strAVGIds += @"'" + sgnl.m_kks_name + @"',";
                            else
                                ;
                    else
                        ; // формула
                // удалить "лишнюю" запятую
                strSumIds = string.IsNullOrEmpty(strSumIds) == false ? strSumIds.Substring(0, strSumIds.Length - 1) : string.Empty;
                strAVGIds = string.IsNullOrEmpty(strAVGIds) == false ? strAVGIds.Substring(0, strAVGIds.Length - 1) : string.Empty;

                m_strQuery = "DECLARE @dtReq DateTime; SELECT @dtReq = CAST('" + dt_end.ToString("yyyyMMdd H:mm:ss") + @"' as DateTime);"
                    + @"SELECT [KKS_NAME], SUM(VALUE) as VALUE, @dtReq as [DATETIME], COUNT(VALUE) * 60 as [COUNT]"
                    + " FROM ("
                        + @"SELECT[KKS_NAME], AVG(VALUE) as VALUE, DATEADD(MINUTE, (DATEDIFF(MINUTE, DATEADD(DAY, -1, @dtReq), [DATETIME]) / 60) * 60, DATEADD(DAY, -1, @dtReq)) as [DATETIME], COUNT(VALUE) as [COUNT]"
                        + @" FROM " + NameTable
                        + " WHERE [DATETIME] >= DATEADD(DAY, -1, @dtReq) and [DATETIME] < @dtReq"
                            + " AND [KKS_NAME] IN (" + strSumIds + ")"
                        + " GROUP BY [KKS_NAME], DATEADD(MINUTE, (DATEDIFF(MINUTE, DATEADD(DAY, -1, @dtReq), [DATETIME]) / 60) * 60, DATEADD(DAY, -1, @dtReq))) as VZLET"
                    + " GROUP BY [KKS_NAME]"
                    
                    + @" UNION"

                    + " SELECT [KKS_NAME], AVG(VALUE) as VALUE, '" + dt_end.ToString("yyyy.MM.dd H:mm:ss") + @"' as [DATETIME], COUNT(VALUE) as [COUNT]"
                    + " FROM " + NameTable
                    + " WHERE [DATETIME] >= '" + dt_begin.ToString("yyyyMMdd H:mm:ss") + @"' AND [DATETIME] < '" + dt_end.ToString("yyyyMMdd H:mm:ss") + @"'"
                        + " AND [KKS_NAME] IN (" + strAVGIds + @")"
                    + @" GROUP BY [KKS_NAME])";
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG
                return new SIGNALVzletKKSNAMEsql(this, (int)objs[0], /*(int)*/objs[2], bool.Parse((string)objs[3]));
            }

            protected override object getIdMain(object id_link)
            {
                throw new NotImplementedException();
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsVzletSql(this, id, objs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        protected override void parseValues(DataTable table)
        {
            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblValue = -1F;
            int countRow = 0;

            if (PeriodMain.Days >= 1)
            {
                countRow = 1440;
            }
            else
            {
                countRow = 60;
            }


                tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            if (table.Rows.Count > 0)
            {
                foreach (GroupSignalsVzletSql.SIGNALMSTKKSNAMEsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
                {
                    DataRow[] rows = table.Select("KKS_NAME ='" + sgnl.m_kks_name + "'");
                    if (rows.Length > 0)
                    {
                        foreach (DataRow r in rows)
                        {
                            //if (int.Parse(r["COUNT"].ToString()) > countRow)//???проверка кол-ва строк
                            //{
                                dtValue = DateTime.Parse(r["DATETIME"].ToString());
                                dtValue = dtValue + m_tsUTCOffsetToData.Value;
                                
                                if (sgnl.IsFormula == false)
                                {
                                    // вставить строку
                                    tblRes.Rows.Add(new object[] {
                                    sgnl.m_idMain
                                    , dtValue
                                    , double.Parse(r["VALUE"].ToString())
                                    });
                                }
                                else
                                    // формула
                                    continue
                                    ;
                            //}
                        }
                    }
                }
                base.parseValues(tblRes);
            }
        }
    }
}
