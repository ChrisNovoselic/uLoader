using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestStatIDsql
{
    public class DestStatIDsql : HHandlerDbULoaderStatTMDest
    {
        private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO"
            , s_strIdTEC = @"6";

        public DestStatIDsql()
            : base()
        {
        }

        public DestStatIDsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatIdsql : GroupSignalsStatTMDest
        {
            public GroupSignalsStatIdsql(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }
            
            protected class SIGNALStatIdsql : GroupSignalsDest.SIGNALDest
            {
                public int m_idStat;

                public SIGNALStatIdsql(int idMain, int idLink, int idStat)
                    : base(idMain, idLink)
                {
                    this.m_idStat = idStat;
                }
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALStatIdsql((int)objs[0], (int)objs[1], (int)objs[3]);
            }

            protected override object getIdToInsert(int idLink)
            {
                int iRes = -1;

                foreach (SIGNALStatIdsql sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        iRes = sgnl.m_idStat;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                strRes = @"INSERT INTO [dbo].[" + s_strNameDestTable + @"] ("
                    + @"[ID]"
                    + @",[ID_TEC]"
                    + @",[Value]"
                    + @",[last_changed_at]"
                    + @",[tmdelta]"
                    + @",[INSERT_DATETIME]"
                        + @") VALUES";

                foreach (DataRow row in tblRes.Rows)
                {
                    strRow = @"(";

                    strRow += getIdToInsert(Int32.Parse(row[@"ID"].ToString().Trim())) + @",";
                    strRow += s_strIdTEC + @",";
                    strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                    strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(-6).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"',";
                    strRow += row[@"tmdelta"] + @",";
                    strRow += @"GETDATE()";

                    strRow += @"),";

                    strRes += strRow;
                }
                //Лишняя ','
                strRes = strRes.Substring(0, strRes.Length - 1);

                return
                    //string.Empty
                    strRes
                    ;
            }
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            string key = string.Empty
                , val = string.Empty;
            if (pars.Length > 1)
            {
                for (int i = 1; i < pars.Length; i ++)
                {
                    if (pars[i] is string)
                    {
                        key = ((string)pars[i]).Split (FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[0];
                        val = ((string)pars[i]).Split (FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[1];
                        if (key.Equals (@"NAME_TABLE") == true)
                            s_strNameDestTable = val;
                        else
                            if (key.Equals (@"ID_TEC") == true)
                                s_strIdTEC = val;
                            else
                                ;
                    }
                    else
                        ;
                }
            }
            else
                ;

            return iRes;
        }

        protected override GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsStatIdsql(this, objs);
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2001;

            createObject(typeof(DestStatIDsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
