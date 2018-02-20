using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP;

namespace SrcMST
{
    public class SrcMSTASUTPIDT5tg1sql : HHandlerDbULoaderMSTIDsql
    {
        public SrcMSTASUTPIDT5tg1sql(PlugInULoader plugIn)
            : base(plugIn, MODE_CURINTERVAL.CAUSE_PERIOD_HOUR, MODE_CURINTERVAL.NEXTSTEP_FULL_PERIOD)
        {
        }

        public SrcMSTASUTPIDT5tg1sql()
            : base(MODE_CURINTERVAL.CAUSE_PERIOD_HOUR, MODE_CURINTERVAL.NEXTSTEP_FULL_PERIOD)
        {
        }
        
        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsMSTASUTPT5tg1IDsql(this, id, objs);
        }

        protected override void Initialize()
        {
            base.Initialize();

            _modeWhereDatetime = MODE_WHERE_DATETIME.UNKNOWN;
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = base.Initialize(id, pars);

            if (m_dictAdding.ContainsKey(@"WHERE_DATETIME") == true)
                _modeWhereDatetime = (MODE_WHERE_DATETIME)Enum.Parse (typeof (MODE_WHERE_DATETIME), m_dictAdding [@"WHERE_DATETIME"]);
            else
                ;

            if (m_dictAdding.ContainsKey (@"TABLE_STATES") == true)
                _modeTableStates = (MODE_TABLE_STATES)Enum.Parse (typeof (MODE_TABLE_STATES), m_dictAdding [@"TABLE_STATES"]);
            else
                _modeTableStates = MODE_TABLE_STATES.MINUTE;

            return iRes;
        }

        protected override void parseValues(System.Data.DataTable table)
        {
            //base.parseValues (table);

            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblSumValue = -1F;
            //int iHour = -1
            //    ,iHourAdding = -1;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
                //???, QUALITY
            });

            foreach (GroupSignalsMSTIDsql.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals) {
                try {
                    if (sgnl.IsFormula == false) {
                        rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal);

                        if ((rowsSgnl.Length > 0)
                            //??? если строк > 1
                            && (((int)rowsSgnl[0][@"CNT"] % DataRowCountFlush) == 0)) {
                            // только при кол-ве записей = 60 (все минуты часа)
                            //iHourAdding = 0;
                            //iHour = (int)rowsSgnl[0][@"HOUR"];
                            //if (iHour > 23)
                            //    iHourAdding = 24;
                            //else
                            //    ;
                            //iHour -= iHourAdding;

                            dtValue = (DateTime)rowsSgnl [0] [@"DATETIME"]; //??? OFFSET

                            dblSumValue = (double)rowsSgnl[0][@"VALUE"];

                            // при необходимости найти среднее
                            if (sgnl.IsFormula == false)
                                dblSumValue /= DataRowCountFlush; //cntRec
                            else
                                ;
                            // вставить строку
                            tblRes.Rows.Add(new object[] {
                                sgnl.m_idMain
                                , dtValue
                                , dblSumValue
                                //, QUALITY
                            });
                        } else
                            ; // неполные данные
                    } else
                        // формула
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"SrcMSTASUTPIDT5tg1sql:: parseValues (sgnl.Id=" + sgnl.m_idMain + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            base.parseValues(tblRes);
        }

        private class GroupSignalsMSTASUTPT5tg1IDsql : GroupSignalsMSTIDsql
        {
            public GroupSignalsMSTASUTPT5tg1IDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;

                string strIds = string.Empty;

                foreach (SIGNALIdsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        strIds += sgnl.m_iIdLocal + @",";
                    else
                    // формула
                        ;
                // удалить "лишнюю" запятую
                strIds = strIds.Substring (0, strIds.Length - 1);

                m_strQuery = string.Format (@"SELECT [ID], SUM([VALUE]) as [VALUE], COUNT(*) as [CNT]"
                        + @", DATEADD({4}, (DATEDIFF({4}, CAST('{0}' as datetime), [last_changed_at]) / 60) * 60, CAST('{0}' as datetime)) as [DATETIME]"
                    + @" FROM [dbo].[{1}]"
                    + @" WHERE {2}"
                        + @" AND [ID] IN ({3})"
                    + @" GROUP BY [ID]"
                        + @", DATEADD({4}, (DATEDIFF({4}, CAST('{0}' as datetime), [last_changed_at]) / 60) * 60, CAST('{0}' as datetime))"
                        , DateTimeEndFormat  // {0}
                        , (_parent as SrcMSTASUTPIDT5tg1sql).NameTableSource  // {1}
                        , WhereDatetime  // {2}
                        , strIds  // {3}
                        , (_parent as SrcMSTASUTPIDT5tg1sql).NameDatePart  // {4}
                    );
            }
        }
    }
}
