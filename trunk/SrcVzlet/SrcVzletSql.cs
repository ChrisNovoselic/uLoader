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
                long secUTCOffsetToData = m_msecUTCOffsetToData / 1000;
                long secUTCOffsetToServer = m_msecUTCOffsetToServer / 1000;

                DateTime dt_begin, dt_end;

                if (PeriodMain.Days >= 1)
                {
                    dt_begin = DateTime.Parse(DateTimeBeginFormat);
                    dt_begin = dt_begin.AddHours(-dt_begin.Hour);
                    dt_begin = dt_begin.AddMinutes(-dt_begin.Minute);
                    dt_begin = dt_begin.AddSeconds(-dt_begin.Second);
                    dt_begin = dt_begin.AddMilliseconds(-dt_begin.Millisecond);
                    dt_begin = dt_begin.AddSeconds(- secUTCOffsetToData + secUTCOffsetToServer);
                    //dt_begin = dt_begin.AddDays(PeriodMain.Days);

                    dt_end = DateTime.Parse(DateTimeEndFormat);
                    dt_end = dt_end.AddHours(-dt_end.Hour);
                    dt_end = dt_end.AddMinutes(-dt_end.Minute);
                    dt_end = dt_end.AddSeconds(-dt_end.Second);
                    dt_end = dt_end.AddMilliseconds(-dt_end.Millisecond);
                    dt_end = dt_end.AddSeconds(- secUTCOffsetToData + secUTCOffsetToServer);
                    //dt_end = dt_begin.AddDays(PeriodMain.Days);
                }
                else
                {
                    dt_begin = DateTime.Parse(DateTimeBeginFormat);
                    dt_begin = dt_begin.AddMinutes(-dt_begin.Minute);
                    dt_begin = dt_begin.AddSeconds(-dt_begin.Second);
                    dt_begin = dt_begin.AddMilliseconds(-dt_begin.Millisecond);
                    //dt_begin = dt_begin.AddSeconds(-secUTCOffsetToData + secUTCOffsetToServer);

                    dt_end = DateTime.Parse(DateTimeEndFormat);
                    dt_end = dt_end.AddMinutes(-dt_end.Minute);
                    dt_end = dt_end.AddSeconds(-dt_end.Second);
                    dt_end = dt_end.AddMilliseconds(-dt_end.Millisecond);
                    //dt_end = dt_end.AddSeconds(-secUTCOffsetToData + secUTCOffsetToServer);
                }



                string strIds =
  "select * from (SELECT[KKS_NAME], SUM(VALUE) as VALUE, '" + dt_end.ToString("yyyy.MM.dd H:mm:ss") + @"' as [DATETIME] "
  + "FROM " + NameTable
  + " where[DATETIME] >= '" + dt_begin.ToString("yyyy.MM.dd H:mm:ss") + @"' and [DATETIME] < '" + dt_end/*.AddSeconds(PeriodMain.TotalSeconds)*/.ToString("yyyy.MM.dd H:mm:ss") + @"'"
  + " and KKS_NAME in ('T5#ASKUTE_G_COLDWATER', 'T5#ASKUTE_G_DVL', 'T5#ASKUTE_G_DVP', 'T5#ASKUTE_G_OBR_1B', 'T5#ASKUTE_G_OBR_2B'"
  + ", 'T5#ASKUTE_G_OBR_3B', 'T5#ASKUTE_G_OBR_4B', 'T5#ASKUTE_G_OBR_5B', 'T5#ASKUTE_G_OBR_IBK', 'T5#ASKUTE_G_OBR_OMTC', 'T5#ASKUTE_G_OBR_OVK'"
  + ", 'T5#ASKUTE_G_OBR_PERVOM', 'T5#ASKUTE_G_OBR_PVK', 'T5#ASKUTE_G_OBR_STOL', 'T5#ASKUTE_G_OBR_STROY', 'T5#ASKUTE_G_OBR_STROY2'"
  + ", 'T5#ASKUTE_G_PITVODA', 'T5#ASKUTE_G_POD_1B', 'T5#ASKUTE_G_POD_2B', 'T5#ASKUTE_G_POD_3B', 'T5#ASKUTE_G_POD_4B', 'T5#ASKUTE_G_POD_5B'"
  + ", 'T5#ASKUTE_G_POD_IBK', 'T5#ASKUTE_G_POD_OMTC', 'T5#ASKUTE_G_POD_OVK', 'T5#ASKUTE_G_POD_PERVOM', 'T5#ASKUTE_G_POD_PVK', 'T5#ASKUTE_G_POD_STOL'"
  + ", 'T5#ASKUTE_G_POD_STROY', 'T5#ASKUTE_G_POD_STROY2', 'T5#ASKUTE_G_PODPIT') group by KKS_NAME union"
  
  + " SELECT [KKS_NAME], AVG(VALUE) as VALUE, '" + dt_end.ToString("yyyy.MM.dd H:mm:ss") + @"' as [DATETIME] "
  + " FROM " + NameTable
  + " where[DATETIME] >= '" + dt_begin.ToString("yyyy.MM.dd H:mm:ss") + @"' and [DATETIME] < '" + dt_end/*.AddSeconds(PeriodMain.TotalSeconds)*/.ToString("yyyy.MM.dd H:mm:ss") + @"'"
  + " and KKS_NAME in ('T5#ASKUTE_H_LEVEL_PODPIT2','T5#ASKUTE_H_LEVEL_PODPIT1','T5#ASKUTE_P_COLDWATER','T5#ASKUTE_P_EXT_AIR_KTS','T5#ASKUTE_P_OBR_1B','T5#ASKUTE_P_OBR_2B','T5#ASKUTE_P_OBR_3B','T5#ASKUTE_P_OBR_4B'"
  + ",'T5#ASKUTE_P_OBR_5B','T5#ASKUTE_P_OBR_IBK','T5#ASKUTE_P_OBR_OMTC','T5#ASKUTE_P_OBR_OVK','T5#ASKUTE_P_OBR_PERVOM','T5#ASKUTE_P_OBR_PVK'"
  + ",'T5#ASKUTE_P_OBR_STOL','T5#ASKUTE_P_OBR_STROY','T5#ASKUTE_P_OBR_STROY2','T5#ASKUTE_P_POD_1B','T5#ASKUTE_P_POD_2B','T5#ASKUTE_P_POD_3B'"
  + ",'T5#ASKUTE_P_POD_4B','T5#ASKUTE_P_POD_5B','T5#ASKUTE_P_POD_IBK','T5#ASKUTE_P_POD_OMTC','T5#ASKUTE_P_POD_OVK','T5#ASKUTE_P_POD_PERVOM'"
  + ",'T5#ASKUTE_P_POD_PVK','T5#ASKUTE_P_POD_STOL','T5#ASKUTE_P_POD_STROY','T5#ASKUTE_P_POD_STROY2','T5#ASKUTE_P_PODPIT','T5#ASKUTE_T_COLDWATER'"
  + ",'T5#ASKUTE_T_EXTAIR','T5#ASKUTE_T_OBR_1B','T5#ASKUTE_T_OBR_2B','T5#ASKUTE_T_OBR_3B','T5#ASKUTE_T_OBR_4B','T5#ASKUTE_T_OBR_5B','T5#ASKUTE_T_OBR_IBK'"
  + ",'T5#ASKUTE_T_OBR_OMTC','T5#ASKUTE_T_OBR_OVK','T5#ASKUTE_T_OBR_PERVOM','T5#ASKUTE_T_OBR_PVK','T5#ASKUTE_T_OBR_STOL','T5#ASKUTE_T_OBR_STROY'"
  + ",'T5#ASKUTE_T_OBR_STROY2','T5#ASKUTE_T_POD_1B','T5#ASKUTE_T_POD_2B','T5#ASKUTE_T_POD_3B','T5#ASKUTE_T_POD_4B','T5#ASKUTE_T_POD_5B','T5#ASKUTE_T_POD_IBK'"
  + ",'T5#ASKUTE_T_POD_OMTC','T5#ASKUTE_T_POD_OVK','T5#ASKUTE_T_POD_PERVOM','T5#ASKUTE_T_POD_PVK','T5#ASKUTE_T_POD_STOL','T5#ASKUTE_T_POD_STROY'"
  + ",'T5#ASKUTE_T_POD_STROY2','T5#ASKUTE_T_PODPIT') group by KKS_NAME ) as a WHERE KKS_NAME in (";

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        strIds += @"'" + sgnl.m_kks_name + @"',";
                    else
                        ; // формула
                // удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery = strIds + ")"
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
            int countDay = 0;

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
                        }
                    }
                }
                base.parseValues(tblRes);
            }
        }
    }
}
