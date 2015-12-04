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

namespace SrcBiyskTMLastora
{
    public class SrcBiyskTMLastora : HHandlerDbULoaderSrc
    {
        public SrcBiyskTMLastora()
            : base()
        {
        }

        public SrcBiyskTMLastora(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsBiyskTMLastora : GroupSignalsSrc
        {
            public GroupSignalsBiyskTMLastora(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;

                string strUnion = @" UNION ALL ";

                //Формировать зпрос
                foreach (GroupSignalsSrc.SIGNALBiyskTMoraSrc s in m_arSignals)
                {
                    m_strQuery += @"SELECT " + s.m_idMain + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.m_NameTable
                        + @" WHERE"
                        + @" DATETIME > " + @"to_timestamp('" + (_parent as SrcBiyskTMLastora).m_dtServer.AddMinutes(-1).ToString(@"yyyyMMdd HHmmss") + @"', 'yyyymmdd hh24missFF9')" //@" SYSTIMESTAMP - interval '1' minute"
                        //+ @" ORDER BY DATETIME DESC"
                        + strUnion
                    ;
                }

                //Удалить "лишний" UNION
                m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
            }

            public override DataTable TableRecieved
            {
                get { return base.TableRecieved; }

                set
                {
                    DataRow []arSel;
                    DataRow rowAdd;
                    
                    //Требуется добавить идентификаторы 'id_main'
                    if ((! (value == null)) && (!(value.Columns.IndexOf(@"ID") < 0)))
                    {
                        DataTable tblVal = value.Clone();

                        tblVal.Columns.Add(@"CNT", typeof(Int32));

                        foreach (SIGNAL s in m_arSignals)
                        {
                            arSel = value.Select(@"ID=" + s.m_idMain, @"DATETIME DESC");

                            if (arSel.Length > 0)
                            {
                                rowAdd = tblVal.Rows.Add();
                                rowAdd[@"ID"] = arSel[0][@"ID"];
                                rowAdd[@"VALUE"] = arSel[0][@"VALUE"];
                                rowAdd[@"DATETIME"] = arSel[0][@"DATETIME"];
                                rowAdd[@"CNT"] = arSel.Length;
                            }
                            else
                                ;
                            
                        }

                        tblVal.AcceptChanges();

                        base.TableRecieved = tblVal;
                    }
                    else
                    {
                        base.TableRecieved = value;
                    }
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, TAG
                return new SIGNALBiyskTMoraSrc((int)objs[0], objs[2] as string);
            }

            protected override object getIdMain(object id_link)
            {
                throw new NotImplementedException();
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsBiyskTMLastora(this, id, objs);
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1005;

            createObject(typeof(SrcBiyskTMLastora));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
