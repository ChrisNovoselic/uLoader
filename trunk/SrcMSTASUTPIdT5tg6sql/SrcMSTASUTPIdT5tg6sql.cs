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
        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsMSTASUTPT5tg6IDsql(this, id, objs);
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

                foreach (SIGNALMSTIDsql sgnl in m_arSignals)
                    strIds += sgnl.m_id + @",";
                //удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery = @"SELECT [ID]"
                    + @", [VALUE]"
                    + @", DATEADD(HOUR, 1, [last_changed_at]) as [DATETIME]"
                    + @" FROM [dbo].[states_real_his_2]"
                        + @" WHERE"
                        + @" [last_changed_at] >='" + DateTimeBeginFormat + @"'"
                        + @" AND [last_changed_at] <'" + DateTimeEndFormat + @"'"
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
