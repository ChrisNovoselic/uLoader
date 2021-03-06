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

namespace SrcBiyskDiscrLastora
{
    public class SrcBiyskDiscrLastora : HHandlerDbULoaderSrc
    {
        public SrcBiyskDiscrLastora()
            : base()
        {
        }

        public SrcBiyskDiscrLastora(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsBiyskDiscrLastora : GroupSignalsSrc
        {
            public GroupSignalsBiyskDiscrLastora(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
                m_strQuery = string.Empty;

                string strUnion = @",";

                if (m_arSignals.Length > 0)
                {
                    m_strQuery += @"SELECT TAGNAME as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS.ARCHIVE_TS WHERE TAGNAME IN (";

                    //Формировать зпрос
                    foreach (GroupSignalsSrc.SIGNALBiyskTMoraSrc s in m_arSignals)
                    {
                        m_strQuery += @"'" + s.m_NameTable + @"'" + strUnion;
                    }

                    //Удалить "лишний" UNION
                    m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);

                    m_strQuery += @")";
                }
                else
                    ;
            }

            protected override void setQuery()
            {
                ;
            }

            protected object getIdMain(string tag)
            {
                int iRes = -1;

                foreach (SIGNAL s in m_arSignals)
                    if ((s as SIGNALBiyskTMoraSrc).m_NameTable.Equals(tag) == true)
                    {
                        iRes = s.m_idMain;

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
                    DataRow []arSel;
                    DataRow rowAdd;
                    
                    //Требуется добавить идентификаторы 'id_main'
                    if (!(value.Columns.IndexOf(@"ID") < 0))
                    {
                        DataTable tblVal = value.Clone();
                        tblVal.Columns.Remove (@"ID");
                        tblVal.Columns.Add(@"ID", typeof(Int32));

                        tblVal.Columns.Add(@"CNT", typeof(Int32));

                        foreach (SIGNAL s in m_arSignals)
                        {
                            arSel = value.Select(@"ID='" + (s as SIGNALBiyskTMoraSrc).m_NameTable + @"'", @"DATETIME DESC");

                            if (arSel.Length > 0)
                            {
                                rowAdd = tblVal.Rows.Add();
                                rowAdd[@"ID"] = getIdMain (arSel[0][@"ID"].ToString());
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
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsBiyskDiscrLastora(this, objs);
        }

        public override void ClearValues()
        {
            TableRecieved.Clear ();
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1006;

            createObject(typeof(SrcBiyskDiscrLastora));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
