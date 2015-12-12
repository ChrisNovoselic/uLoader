using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMSTASUTPIDT5tg1sql
{
    public class SrcMSTASUTPIDT5tg1sql : HHandlerDbULoaderMSTIDsql
    {
        public SrcMSTASUTPIDT5tg1sql(IPlugIn plugIn)
            : base(plugIn, MODE_CURINTERVAL.CAUSE_PERIOD_HOUR, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        public SrcMSTASUTPIDT5tg1sql()
            : base(MODE_CURINTERVAL.CAUSE_PERIOD_HOUR, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }
        
        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsMSTASUTPT5tg1IDsql(this, id, objs);
        }

        protected override void parseValues(System.Data.DataTable table)
        {
            //base.parseValues (table);

            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblSumValue = -1F;
            int iHour = -1
                ,iHourAdding = -1;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsMSTIDsql.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
            {
                try
                {
                    rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal);

                    if ((rowsSgnl.Length > 0)
                        //??? если строк > 1
                        && (((int)rowsSgnl[0][@"CNT"] % 60) == 0))
                    {// только при кол-ве записей = 60 (все минуты часа)
                        iHourAdding = 0;
                        iHour = (int)rowsSgnl[0][@"HOUR"];
                        if (iHour > 23)
                            iHourAdding = 24;
                        else
                            ;
                        iHour -= iHourAdding;

                        dtValue = new DateTime((int)rowsSgnl[0][@"YEAR"]
                            , (int)rowsSgnl[0][@"MONTH"]
                            , (int)rowsSgnl[0][@"DAY"]
                            , iHour
                            , 0
                            , 0).AddHours(iHourAdding);

                        dblSumValue = (double)rowsSgnl[0][@"VALUE"];

                        // при необходимости найти среднее
                        if (sgnl.m_bAVG == true)
                            dblSumValue /= 60; //cntRec
                        else
                            ;
                        // вставить строку
                        tblRes.Rows.Add(new object[] {
                            sgnl.m_idMain
                            , dtValue
                            , dblSumValue
                        });
                    }
                    else
                        ; // не полные данные
                }
                catch (Exception e)
                {
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
                int offsetHour = 0;

                if ((_parent as HHandlerDbULoaderSrc).Mode == MODE_WORK.CUR_INTERVAL)
                    offsetHour = -1;
                else
                    ;

                foreach (SIGNALIdsql sgnl in m_arSignals)
                    strIds += sgnl.m_iIdLocal + @",";
                // удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery = @"SELECT [ID], SUM([VALUE]) as [VALUE], COUNT(*) as [CNT]"
	                    + @", DATEPART(YEAR, [last_changed_at]) as [YEAR]"
	                    + @", DATEPART(MONTH, [last_changed_at]) as [MONTH]"
	                    + @", DATEPART(DAY, [last_changed_at]) as [DAY]"
                        + @", (DATEPART(HOUR, [last_changed_at]) + 1) as [HOUR]"
                    + @" FROM [dbo].[states_real_his_0]"
                    + @" WHERE"
                        + @" [last_changed_at] >=DATEADD(HOUR, " + offsetHour + @", CAST('" + DateTimeBeginFormat + @"' as datetime))"
                        + @" AND [last_changed_at] <DATEADD(HOUR, " + offsetHour + @", CAST('" + DateTimeEndFormat + @"' as datetime))"
                            + @" AND [ID] IN (" + strIds + @")"
                    + @" GROUP BY [ID]"
	                    + @", DATEPART(YYYY, [last_changed_at])"
	                    + @", DATEPART(MM, [last_changed_at])"
	                    + @", DATEPART(dd, [last_changed_at])"
	                    + @", DATEPART(HH, [last_changed_at])"
                    ;
            }
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1003;

            createObject(typeof(SrcMSTASUTPIDT5tg1sql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
