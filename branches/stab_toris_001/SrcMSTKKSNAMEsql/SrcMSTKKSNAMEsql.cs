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

namespace SrcMSTKKSNAMEsql
{
    public class SrcMSTKKSNAMEsql : HHandlerDbULoaderMSTTMSrc
    {
        public SrcMSTKKSNAMEsql()
            : base()
        {
        }

        public SrcMSTKKSNAMEsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsMSTKKSNAMEsql : GroupSignalsMSTTMSrc
        {
            public GroupSignalsMSTKKSNAMEsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNALMSTKKSNAMEsql((int)objs[0], (string)objs[2]);
            }

            protected override void setQuery()
            {
                m_strQuery = string.Empty;
                string strIds = string.Empty;

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    strIds += @"'" + sgnl.m_kks_name + @"',";
                //удалить "лишнюю" запятую
                strIds = strIds.Substring(0, strIds.Length - 1);

                m_strQuery =
                    ////Вариант №1
                    //@"SELECT [KKS_NAME] as [ID], 1 AS [ID_TEC],"
                    //+ @" CASE WHEN ([Value] > -0.1 AND [Value] < 0.1) THEN 0 ELSE [Value] END AS [VALUE],"
                    //+ @" [last_changed_at] as [DATETIME],[tmdelta]"
                    //+ @" FROM [dbo].[v_STATISTICS_real_his_KKS]"
                    //    + @" WHERE"
                    //    + @" [last_changed_at] >='" + DateTimeStartFormat + @"'"
                    //    + @" AND [last_changed_at] <'" + DateTimeCurIntervalEndFormat + @"'"
                    //        + @" AND [KKS_NAME] IN (" + strIds + @")"
                    //Вариант №2
                    @"SELECT [PARAM].[NAME] as [ID], [PARAM].[ID] as [ID_MST]"
                        + @",CASE WHEN ([Value] > -0.1 AND [Value] < 0.1) THEN 0 ELSE [Value] END AS [VALUE]"
                        + @",[last_changed_at] as [DATETIME],[tmdelta]"
                        + @" FROM [v_STATISTICS_real_his] as [DATA]"
                        + @" INNER JOIN (SELECT [NAME], [ID] FROM [reals_rv] WHERE [NAME] IN (" + strIds + @")) AS [PARAM] ON [PARAM].[ID] = [DATA].[ID]"
                        + @" WHERE [DATA].[ID] IN (SELECT [id] FROM [reals_rv] WHERE [NAME] IN (" + strIds + @"))"
                        + @" AND [last_changed_at] >='" + DateTimeBeginFormat + @"'"
                        + @" AND [last_changed_at] <'" + DateTimeEndFormat + @"'"
                    ;
            }

            private int getIdMain (string id_mst)
            {
                int iRes = -1;

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.m_kks_name == id_mst)
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
                    if (! (value == null))
                        //Требуется добавить идентификаторы 'id_main'
                        if (! (value.Columns.IndexOf (@"ID") < 0))
                        {
                            DataTable tblVal = value.Copy ();
                            tblVal.Columns.Add (@"KKSNAME_MST", typeof(string));
                            //tblVal.Columns.Add(@"ID_MST", typeof(int));

                            foreach (DataRow r in tblVal.Rows)
                            {
                                r[@"KKSNAME_MST"] = r[@"ID"];
                                //r[@"ID_MST"] = getIdMST((string)r[@"KKSNAME_MST"]);

                                r[@"ID"] = getIdMain((string)r[@"KKSNAME_MST"]);
                            }

                            base.TableRecieved = tblVal;
                        }
                        else
                            base.TableRecieved = value;
                    else
                        base.TableRecieved = value;
                }
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsMSTKKSNAMEsql(this, id, objs);
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1004;

            createObject(typeof(SrcMSTKKSNAMEsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
