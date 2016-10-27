using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace SrcIstok
{
    public class SrcIstokSql : HHandlerDbULoaderDatetimeSrc //HHandlerDbULoaderStatTMMSTDest
    {
        public SrcIstokSql()
            : base(@"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {

        }

        public SrcIstokSql(PlugInULoader iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        private class GroupSignalsIstokSql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsIstokSql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {

            }

            /// <summary>
            /// Установить содержание для запроса
            /// </summary>
            protected override void setQuery()
            {
                m_strQuery = string.Empty;
                long secUTCOffsetToData = m_msecUTCOffsetToServer / 1000;

                if (DateTimeBegin != DateTimeStart)
                {
                    DateTimeBegin = DateTimeBegin.AddSeconds(-1 * secUTCOffsetToData);
                    DateTimeBegin = new DateTime(DateTimeBegin.Year, DateTimeBegin.Month, DateTimeBegin.Day).AddHours(DateTimeBegin.Hour);
                }
                else
                {
                    DateTimeBegin = DateTimeStart;
                    DateTimeBegin = new DateTime(DateTimeBegin.Year, DateTimeBegin.Month, DateTimeBegin.Day).AddHours(DateTimeBegin.Hour);
                }

                string strIds = "SELECT ДатаВремя, ";

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        strIds += @"" + sgnl.m_kks_name + @",";
                    else
                        ; // формула
                // удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery = strIds
                        + @" FROM " + NameTable + " WHERE [ДатаВремя] >='" + DateTimeBegin.AddHours(-1) + @"'"
                        + @" AND [ДатаВремя] <'" + DateTimeBegin + @"'"
                    ;
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG
                return new SIGNALMSTKKSNAMEsql(this, (int)objs[0], /*(int)*/objs[2]);
            }

            protected override object getIdMain(object id_link)
            {
                throw new NotImplementedException();
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsIstokSql(this, id, objs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        protected override void parseValues(System.Data.DataTable table)
        {
            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblValue = -1F;
            int countDay = 0;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            if (table.Rows.Count > 0)
            {
                foreach (GroupSignalsIstokSql.SIGNALMSTKKSNAMEsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
                {
                    foreach (DataRow r in table.Rows)
                    {
                        dtValue = DateTime.Parse(r["ДатаВремя"].ToString());

                        if (sgnl.IsFormula == false)
                        {
                            // вставить строку
                            tblRes.Rows.Add(new object[] {
                            sgnl.m_idMain
                            , dtValue
                            , double.Parse(r[sgnl.m_kks_name].ToString())
                        });
                        }
                        else
                            // формула
                            continue
                            ;

                        //cntHour = cntHour + 48;//за месяц
                        base.parseValues(tblRes);
                    }
                }
            }
        }
    }
}
