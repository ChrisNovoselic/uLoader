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

namespace SrcBiysk
{
    public class SrcBiyskTMora : HHandlerDbULoaderDatetimeSrc
    {
        public SrcBiyskTMora()
            : base(@"yyyyMMdd HHmmss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.HALF_PERIOD)
        {
        }

        public SrcBiyskTMora(PlugInULoader iPlugIn)
            : base(iPlugIn, @"yyyyMMdd HHmmss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.HALF_PERIOD)
        {
        }

        private class GroupSignalsBiyskTMora : GroupSignalsDatetimeSrc
        {
            public GroupSignalsBiyskTMora(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;

                string strUnion = @" UNION ";
                long secUTCOffsetToData = m_msecUTCOffsetToData / 1000;

                //Формировать зпрос
                foreach (GroupSignalsBiyskTMora.SIGNALBiyskTMoraSrc s in m_arSignals)
                    if (s.IsFormula == false)
                        m_strQuery += @"SELECT " + s.m_idMain + @" as ID, VALUE, QUALITY"
                                + @", DATETIME + numtodsinterval(" + secUTCOffsetToData + @",'second') as DATETIME"
                            + @" FROM ARCH_SIGNALS." + s.m_NameTable
                            + @" WHERE"
                            + @" DATETIME >=" + @" to_timestamp('" + DateTimeBeginFormat + @"', 'yyyymmdd hh24missFF9')"
                            + @" AND DATETIME <" + @" to_timestamp('" + DateTimeEndFormat + @"', 'yyyymmdd hh24missFF9')"
                            + strUnion
                        ;
                    else
                        ; // формула

                // удалить "лишний" UNION
                m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
                ////Установить сортировку
                //m_strQuery += @" ORDER BY DATETIME DESC";

                //Logging.Logg().Debug(@"GroupSignalsBiystTMOra::setQuery() - DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + DateTimeEndFormat, Logging.INDEX_MESSAGE.NOT_SET);
                //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + DateTimeEndFormat);

                //Logging.Logg().Debug(@"GroupSignalsBiystTMOra::setQuery() - m_strQuery=" + m_strQuery + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                //Console.WriteLine(@"m_strQuery=" + m_strQuery);
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, TAG
                return new SIGNALBiyskTMoraSrc(this, (int)objs[0], objs[2] as string);
            }

            protected override object getIdMain(object id_link)
            {
                throw new NotImplementedException();
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsBiyskTMora(this, id, objs);
        }
    }
}
