﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestStatIDsql
{
    public class DestStatIDsql : HHandlerDbULoaderStatTMMSTDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO"
        //    , s_strIdTEC = @"6";
        /// <summary>
        /// Конструктор - вспомогательный (статическая сборка)
        /// </summary>
        public DestStatIDsql()
            : base()
        {
        }
        /// <summary>
        /// Конструктор - основной (динамическая загрузка)
        /// </summary>
        /// <param name="iPlugIn">Объект для связи с "родительским" приложением</param>
        public DestStatIDsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatIDsql : GroupSignalsStatTMMSTDest
        {
            public GroupSignalsStatIDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;

                //Logging.Logg().Debug(@"GroupSignalsStatIDsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                strRes = @"INSERT INTO [dbo].[" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"] ("
                    + @"[ID]"
                    + @",[ID_TEC]"
                    + @",[Value]"
                    + @",[last_changed_at]"
                    + @",[tmdelta]"
                    + @",[INSERT_DATETIME]"
                        + @") VALUES";

                foreach (DataRow row in tblRes.Rows)
                {
                    if (((int)getIdToInsert(Int32.Parse(row[@"ID"].ToString().Trim()))) == 0)
                    {
                        strRow = @"(";

                        strRow += row[@"ID_MST"] + @",";
                        strRow += (_parent as HHandlerDbULoaderStatTMDest).m_strIdTEC + @",";
                        strRow += ((double)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                        strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(0).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"',";
                        strRow += row[@"tmdelta"] + @",";
                        strRow += @"GETDATE()";

                        strRow += @"),";

                        strRes += strRow;
                    }
                    else
                        ; // не найдено соответствие с Id источника
                }
                //Лишняя ','
                strRes = strRes.Substring(0, strRes.Length - 1);

                return
                    //string.Empty
                    strRes
                    ;
            }
        }

        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsStatIDsql(this, id, objs);
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
