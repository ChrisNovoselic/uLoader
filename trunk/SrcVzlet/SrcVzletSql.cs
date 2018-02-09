using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP;

namespace SrcVzlet
{
    class SrcVzletSql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcVzletSql()
            : base(UCHET.Trice, @"yyyyMMdd HH:mm:ss", MODE_CURINTERVAL.CAUSE_PERIOD_HOUR, MODE_CURINTERVAL.NEXTSTEP_FULL_PERIOD)
        {

        }

        public SrcVzletSql(PlugInULoader iPlugIn)
            : base(iPlugIn, UCHET.Trice, @"yyyyMMdd HH:mm:ss", MODE_CURINTERVAL.CAUSE_PERIOD_HOUR, MODE_CURINTERVAL.NEXTSTEP_FULL_PERIOD)
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

                foreach (SIGNALVzletKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        if (sgnl.m_bAVG == false)
                            strSumIds += @"'" + sgnl.m_kks_name + @"',";
                        else
                            if (sgnl.m_bAVG == true)
                                strAVGIds += @"'" + sgnl.m_kks_name + @"',";
                            else
                                ;
                    else
                        ; // формула
                // удалить "лишнюю" запятую
                strSumIds = string.IsNullOrEmpty(strSumIds) == false ? strSumIds.Substring(0, strSumIds.Length - 1) : string.Empty;
                strAVGIds = string.IsNullOrEmpty(strAVGIds) == false ? strAVGIds.Substring(0, strAVGIds.Length - 1) : string.Empty;

                m_strQuery = "DECLARE @dtReq DateTime; SELECT @dtReq = CAST('" + DateTimeEndFormat + @"' as DateTime);";

                if (string.IsNullOrEmpty(strSumIds) == false)
                    m_strQuery += string.Format(@"SELECT [KKS_NAME], SUM(VALUE) as VALUE, @dtReq as [DATETIME], COUNT(VALUE) * 60 as [COUNT]"
                        + " FROM ("
                            + @"SELECT[KKS_NAME], AVG(VALUE) as VALUE, DATEADD(MINUTE, (DATEDIFF(MINUTE, DATEADD(DAY, -1, @dtReq), [DATETIME]) / 60) * 60, DATEADD(DAY, -1, @dtReq)) as [DATETIME], COUNT(VALUE) as [COUNT]"
                            + @" FROM {0}"
                            + " WHERE [DATETIME] >= DATEADD(HOUR, -1, @dtReq) and [DATETIME] < @dtReq"
                                + " AND [KKS_NAME] IN ({1})"
                            + " GROUP BY [KKS_NAME], DATEADD(MINUTE, (DATEDIFF(MINUTE, DATEADD(DAY, -1, @dtReq), [DATETIME]) / 60) * 60, DATEADD(DAY, -1, @dtReq))) as VZLET"
                        + " GROUP BY [KKS_NAME]", NameTable, strSumIds);
                else
                    ;

                if (string.IsNullOrEmpty(strAVGIds) == false) {
                    if (string.IsNullOrEmpty(strSumIds) == false)
                        m_strQuery += @" UNION";
                    else
                        ;

                    m_strQuery += string.Format(" SELECT [KKS_NAME], AVG(VALUE) as VALUE, @dtReq as [DATETIME], COUNT(VALUE) as [COUNT]"
                        + " FROM {0}"
                        + " WHERE [DATETIME] >= DATEADD(HOUR, -1, @dtReq) and [DATETIME] < @dtReq"
                            + " AND [KKS_NAME] IN ({1})"
                        + @" GROUP BY [KKS_NAME]", NameTable, strAVGIds);
                } else
                    ;
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
        /// Обработка результатов запроса
        /// </summary>
        /// <param name="table">Таблица с данными - результат запроса</param>
        protected override void parseValues(DataTable table)
        {
            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblValue = -1F;
            int countRow = 60;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
                , //??? QUALITY
            });

            if (table.Rows.Count > 0) {
                foreach (GroupSignalsVzletSql.SIGNALVzletKKSNAMEsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals) {
                    rowsSgnl = table.Select("KKS_NAME ='" + sgnl.m_kks_name + "'");

                    if (rowsSgnl.Length > 0)
                        foreach (DataRow r in rowsSgnl) {
                            //if (int.Parse(r["COUNT"].ToString()) > countRow) {//???проверка кол-ва строк
                                try {
                                    dtValue = DateTime.Parse(r["DATETIME"].ToString());
                                    //dtValue = ToUtcTime (dtValue); //OFFSET

                                    if (sgnl.IsFormula == false) {
                                        dblValue = double.Parse(r["VALUE"].ToString());

                                        // вставить строку
                                        tblRes.Rows.Add(new object[] {
                                            sgnl.m_idMain
                                            , dtValue
                                            , dblValue
                                        });
                                    }
                                    else
                                        // формула
                                        continue
                                        ;
                                } catch (Exception e) {
                                    Logging.Logg().Exception(e
                                        , string.Format(@"SrcVzletsql::parseValue () - разбор значения для сигнала {}", sgnl.m_kks_name)
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                                }
                            //} else
                            //    ;
                        }
                    else
                        ;
                }
                // вызвать базовый метод
                base.parseValues(tblRes);
            }
        }
    }
}
