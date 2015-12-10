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

namespace SrcMSTASUTPIDT5tg6sql
{
    public class SrcMSTASUTPIDT5tg6sql : HHandlerDbULoaderMSTIDsql
    {
        public SrcMSTASUTPIDT5tg6sql(IPlugIn plugIn)
            : base(plugIn, MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        public SrcMSTASUTPIDT5tg6sql()
            : base(MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsMSTASUTPT5tg6IDsql(this, id, objs);
        }

        protected override void parseValues(System.Data.DataTable table)
        {
            //base.parseValues (table);

            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblValue = -1F;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsMSTIDsql.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
            {
                rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal);

                //??? если строк > 1
                if (rowsSgnl.Length > 0)
                {
                    dtValue = (DateTime)rowsSgnl[0][@"DATETIME"];

                    dblValue = (double)rowsSgnl[0][@"VALUE"];

                    //// при необходимости найти среднее
                    //if (sgnl.m_bAVG == true)
                    //    dblSumValue /= 60; //cntRec
                    //else
                    //    ;
                    // вставить строку
                    tblRes.Rows.Add(new object[] {
                        sgnl.m_idMain
                        , dtValue
                        , dblValue
                    });
                }
                else
                    ; // не полные данные
            }

            base.parseValues(tblRes);
        }

        private class GroupSignalsMSTASUTPT5tg6IDsql : GroupSignalsMSTIDsql
        {
            public GroupSignalsMSTASUTPT5tg6IDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;
                string strIds = string.Empty;

                foreach (GroupSignalsSrc.SIGNALIdsql sgnl in m_arSignals)
                    strIds += sgnl.m_iIdLocal + @",";
                //удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery = @"SELECT [ID]"
                    + @", [VALUE]"
                    + @", DATEADD(HOUR, 1, [dt]) as [DATETIME]"
                    + @" FROM [dbo].[tep_allcalcs_oper]"
                        + @" WHERE"
                        + @" [dt] >='" + DateTimeBeginFormat + @"'"
                        + @" AND [dt] <'" + DateTimeEndFormat + @"'"
                            + @" AND [ID] IN (" + strIds + @")"
                    ;
            }
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1008;

            createObject(typeof(SrcMSTASUTPIDT5tg6sql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
