﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMST
{
    class SrcMSTASUTPIdT5tg6Dsql : HHandlerDbULoaderMSTIDsql
    {
        private enum MODE_WHERE_DATETIME : short
        {
            UNKNOWN = -1,
            BETWEEN_0_0, BETWEEN_1_0, BETWEEN_0_1, // использовать 'BETWEEN' - ВКЛючить значения для левой, правой границам, IN_IN_0_0, IN_IN_1_0, IN_IN_0_1 // принудительное указание ВКЛючения значений для левой, правой границам
            EX_EX_0_0, EX_EX_1_0, EX_EX_0_1, // принудительное указание ИСКЛючения значений для левой, правой границам
            IN_EX_0_0, IN_EX_1_0, IN_EX_0_1, // принудительное указание ВКЛючения значений для левой, ИСКЛючения значений для правой границам
            COUNT
        }

        private MODE_WHERE_DATETIME _modeWhereDatetime;

        public SrcMSTASUTPIdT5tg6Dsql(PlugInULoader plugIn)
            : base(plugIn, MODE_CURINTERVAL.CAUSE_PERIOD_DAY, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        public SrcMSTASUTPIdT5tg6Dsql()
            : base(MODE_CURINTERVAL.CAUSE_PERIOD_DAY, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsMSTASUTPT5tg6IDsqlD(this, id, objs);
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
                _modeWhereDatetime = (MODE_WHERE_DATETIME)Convert.ToInt16(m_dictAdding[@"WHERE_DATETIME"]);

            return iRes;
        }

        protected override void parseValues(DataTable table)
        {
            //base.parseValues (table);

            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblSumValue = -1F;
            int iHour = -1
                , iHourAdding = -1;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsSrc.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
                try {
                    if (sgnl.IsFormula == false) {
                        rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal);

                        if ((rowsSgnl.Length > 0)
                            //??? если строк > 1
                            && (((int)rowsSgnl[0][@"CNT"] % 24) == 0))
                        {// только при кол-ве записей = 24 (все часы)
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
                                dblSumValue /= 24; //cntRec
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
                            ; // неполные данные
                    }
                    else
                        // формула
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"SrcMSTASUTPIDT5tg1sqlD:: parseValues (sgnl.Id=" + sgnl.m_idMain + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

            // вызов базового метода
            base.parseValues(tblRes);
        }

        private class GroupSignalsMSTASUTPT5tg6IDsqlD : GroupSignalsMSTIDsql
        {
            public GroupSignalsMSTASUTPT5tg6IDsqlD(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {

            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;
                string strIds = string.Empty
                    , strWhereDatetime = string.Empty;
                int offsetHour = 0;
                bool bOffsetOutInclude = true;
                long secOffsetUTCToData = m_secOffsetUTCToServer; //OFFSET

                if ((_parent as HHandlerDbULoaderSrc).Mode == MODE_WORK.CUR_INTERVAL)
                    offsetHour = -1;
                else
                    ;
                offsetHour = (int)secOffsetUTCToData;

                foreach (SIGNALIdsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        strIds += sgnl.m_iIdLocal + @",";
                    else
                        ; // формула
                // удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                switch ((_parent as SrcMSTASUTPIdT5tg6Dsql)._modeWhereDatetime)
                {
                    case MODE_WHERE_DATETIME.IN_EX_0_0:
                    case MODE_WHERE_DATETIME.IN_EX_1_0:
                    case MODE_WHERE_DATETIME.IN_EX_0_1:
                    case MODE_WHERE_DATETIME.EX_EX_0_0:
                    case MODE_WHERE_DATETIME.EX_EX_1_0:
                    case MODE_WHERE_DATETIME.EX_EX_0_1:
                        bOffsetOutInclude = false;
                        break;
                    case MODE_WHERE_DATETIME.UNKNOWN:
                    case MODE_WHERE_DATETIME.BETWEEN_1_0:
                    default:
                        // оставить значение 'true'
                        break;
                }
                // определить содержание 'where'
                switch ((_parent as SrcMSTASUTPIdT5tg6Dsql)._modeWhereDatetime)
                {
                    case MODE_WHERE_DATETIME.IN_EX_0_0:
                        strWhereDatetime = @" [last_changed_at] >= DATEADD(HOUR, " + offsetHour + @", CAST('" + DateTimeBeginFormat + @"' as datetime))"
                            + @" AND [last_changed_at] < DATEADD(HOUR, " + offsetHour + @", CAST('" + DateTimeEndFormat + @"' as datetime))";
                        break;
                    case MODE_WHERE_DATETIME.UNKNOWN:
                    case MODE_WHERE_DATETIME.BETWEEN_1_0:
                    default:
                        strWhereDatetime = @" [last_changed_at] BETWEEN DATEADD(SECOND, 1, DATEADD(HOUR, " + offsetHour + @", CAST('" + DateTimeBeginFormat + @"' as datetime)))"
                            + @" AND DATEADD(HOUR, " + offsetHour + @", CAST('" + DateTimeEndFormat + @"' as datetime))";
                        break;
                }

                m_strQuery = @"SELECT [ID], SUM([VALUE]) as [VALUE], COUNT(*) as [CNT]"
                        //+ @", DATEPART(YEAR, [last_changed_at]) as [YEAR]"
                        //+ @", DATEPART(MONTH, [last_changed_at]) as [MONTH]"
                        //+ @", DATEPART(DAY, [last_changed_at]) as [DAY]"
                        //+ @", (DATEPART(HOUR, [last_changed_at]) + 1) as [HOUR]"
                        + @", DATEPART(YEAR, MAX([last_changed_at])) as [YEAR]"
                        + @", DATEPART(MONTH, MAX([last_changed_at])) as [MONTH]"
                        + @", DATEPART(DAY, MAX([last_changed_at])) as [DAY]"
                        //+ @", (DATEPART(HOUR, MAX([last_changed_at])) + 1) as [HOUR]"
                        + @", (DATEPART(HOUR, MAX([last_changed_at])) + " + (bOffsetOutInclude == false ? 1 : 0) + @") as [HOUR]"
                    + @" FROM [dbo].[states_real_his_2]"
                    + @" WHERE"
                        + strWhereDatetime
                        + @" AND [ID] IN (" + strIds + @")"
                    + @" GROUP BY [ID]"
                    //+ @", DATEPART(YYYY, [last_changed_at])"
                    //+ @", DATEPART(MM, [last_changed_at])"
                    //+ @", DATEPART(dd, [last_changed_at])"
                    //+ @", DATEPART(HH, [last_changed_at])"
                    ;
            }
        }
    }
}
