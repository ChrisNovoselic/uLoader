using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

namespace SrcVzlet
{
    class SrcVzletDSql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcVzletDSql()
            : base(@"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {

        }

        public SrcVzletDSql(PlugInULoader iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {

        }

        private class GroupSignalsVzletDSql : GroupSignalsDatetimeSrc
        {
            //protected string m_nameTable;

            public GroupSignalsVzletDSql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {

            }

            /// <summary>
            /// Установить содержание для запроса
            /// </summary>
            protected override void setQuery()
            {
                int idReq = HMath.GetRandomNumber()
                    , i = -1;
                string cmd = string.Empty;

                long secUTCOffsetToData = m_msecUTCOffsetToServer / 1000;
                //перевод даты для суточного набора
                if (DateTimeStart != DateTimeBegin)
                {
                    DateTimeBegin = (DateTimeBegin - DateTimeBegin.TimeOfDay).AddHours(23);
                }
                else
                {
                    DateTimeBegin = (DateTimeStart - DateTimeStart.TimeOfDay).AddHours(-1);
                }


                //Формировать запрос
                i = 0;

                m_strQuery = "SELECT ДатаВремя, ";
                foreach (GroupSignalsVzletDSql.SIGNALMSTKKSNAMEsql s in m_arSignals)
                {
                    m_strQuery += s.m_kks_name + ", ";
                }

                m_strQuery = m_strQuery.Remove(m_strQuery.Length - 2, 1);

                m_strQuery += " FROM " + NameTable + " ";

                m_strQuery += @"WHERE ДатаВремя > '" + DateTimeBegin + "' and ДатаВремя <= '" + DateTimeBegin.AddSeconds(PeriodMain.TotalSeconds) + "'";

                //DateTimeBegin = DateTimeBegin.AddSeconds(secUTCOffsetToData);

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
            return new GroupSignalsVzletDSql(this, id, objs);
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
                foreach (DataRow r in table.Rows)
                {

                    foreach (GroupSignalsVzletDSql.SIGNALMSTKKSNAMEsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
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
                    }
                }

                base.parseValues(tblRes);
            }
        }
    }
}
