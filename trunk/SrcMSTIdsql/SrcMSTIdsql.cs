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

namespace SrcMSTIdsql
{
    public class SrcMSTIDsql : HHandlerDbULoaderSrc
    {
        public SrcMSTIDsql()
            : base()
        {
        }

        public SrcMSTIDsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsMSTIDsql : GroupSignalsSrc
        {
            public GroupSignalsMSTIDsql(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            //Строки для условия "по дате/времени"
            // начало
            protected override string DateTimeStartFormat
            {
                get { return DateTimeStart.AddHours(-6).AddSeconds(-1 * (int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss"); }
            }
            // окончание
            protected override string DateTimeCurIntervalEndFormat
            {
                //get { return DateTimeStart.AddHours(-6).AddSeconds((int)TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss"); }
                get { return DateTimeStart.AddHours(-6).ToString(@"yyyy/MM/dd HH:mm:ss"); }
            }

            public class SIGNALMSTIDsql : SIGNAL
            {
                public int m_id;

                public SIGNALMSTIDsql(int idMain, int id)
                    : base(idMain)
                {
                    m_id = id;
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALMSTIDsql((int)objs[0], (int)objs[2]);
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;
                string strIds = string.Empty;

                foreach (SIGNALMSTIDsql sgnl in m_arSignals)
                    strIds += sgnl.m_id + @",";
                //удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery = @"SELECT [ID], 1 AS [ID_TEC],"
                    + @" CASE WHEN ([Value] > -0.1 AND [Value] < 0.1) THEN 0 ELSE [Value] END AS [VALUE],"
                    + @" [last_changed_at] as [DATETIME],[tmdelta]"
                    + @" FROM [dbo].[v_STATISTICS_real_his]"
                        + @" WHERE"
                        + @" [last_changed_at] >='" + DateTimeStartFormat + @"'"
                        + @" AND [last_changed_at] <'" + DateTimeCurIntervalEndFormat + @"'"
                            + @" AND [ID] IN (" + strIds + @")"
                    ;
            }

            private int getIdMain (int id_mst)
            {
                int iRes = -1;

                foreach (SIGNALMSTIDsql sgnl in m_arSignals)
                    if (sgnl.m_id == id_mst)
                    {
                        iRes = sgnl.m_idMain;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            public override DataTable TableRecieved
            {
                get { return base.TableRecieved; }

                set
                {
                    //Требуется добавить идентификаторы 'id_main'
                    if (! (value.Columns.IndexOf (@"ID") < 0))
                    {
                        DataTable tblVal = value.Copy ();
                        tblVal.Columns.Add (@"ID_MST", typeof(int));

                        foreach (DataRow r in tblVal.Rows)
                        {
                            r[@"ID_MST"] = r[@"ID"];
                            r[@"ID"] = getIdMain((int)r[@"ID_MST"]);
                        }

                        base.TableRecieved = tblVal;
                    }
                    else
                    {
                        base.TableRecieved = value;
                    }
                }
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsMSTIDsql(this, objs);
        }

        public override void ClearValues()
        {
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1003;

            createObject(typeof(SrcMSTIDsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
