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

namespace SrcBiyskTMora
{
    public class SrcBiyskTMora : HHandlerDbULoaderSrc //HHandlerDbULoader
    {
        public SrcBiyskTMora()
            : base()
        {
        }

        public SrcBiyskTMora(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsBiyskTMora : GroupSignalsSrc
        {
            public GroupSignalsBiyskTMora(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            private class SIGNALBiyskTMora : SIGNAL
            {
                public string m_NameTable;

                public SIGNALBiyskTMora(int idMain, string nameTable) : base (idMain)
                {
                    this.m_NameTable = nameTable;
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALBiyskTMora((int)objs[0], objs[2] as string);
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;

                string strUnion = @" UNION ";

                //Формировать зпрос
                foreach (GroupSignalsBiyskTMora.SIGNALBiyskTMora s in m_arSignals)
                {
                    m_strQuery += @"SELECT " + s.m_idMain + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.m_NameTable
                        + @" WHERE"
                        + @" DATETIME >=" + @" to_timestamp('" + DateTimeStartFormat + @"', 'yyyymmdd hh24missFF9')"
                        + @" AND DATETIME <" + @" to_timestamp('" + DateTimeCurIntervalEndFormat + @"', 'yyyymmdd hh24missFF9')"
                        + strUnion
                    ;
                }

                //Удалить "лишний" UNION
                m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
                ////Установить сортировку
                //m_strQuery += @" ORDER BY DATETIME DESC";

                //Logging.Logg().Debug(@"GroupSignalsBiystTMOra::setQuery() - m_strQuery=" + m_strQuery + @"]..."
                //        , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsBiyskTMora(this, objs);
        }

        public override void ClearValues()
        {
            int iPrev = 0, iDel = 0, iCur = 0;
            if (!(TableRecieved == null))
            {
                iPrev = TableRecieved.Rows.Count;
                string strSel =
                    @"DATETIME<'" + DateTimeStart.ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"' OR DATETIME>='" + DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'"
                    //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    ;

                DataRow[] rowsDel = null;
                try { rowsDel = TableRecieved.Select(strSel); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ClearValues () - ...");
                }

                if (!(rowsDel == null))
                {
                    iDel = rowsDel.Length;
                    if (rowsDel.Length > 0)
                    {
                        foreach (DataRow r in rowsDel)
                            TableRecieved.Rows.Remove(r);
                        //??? Обязательно ли...
                        TableRecieved.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;

                iCur = TableRecieved.Rows.Count;

                Console.WriteLine(@"Обновление рез-та [ID=" + m_IdGroupSignalsCurrent + @"]: " + @"(было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @")");
            }
            else
                ;
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1001;

            createObject(typeof(SrcBiyskTMora));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
