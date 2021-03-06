﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestTechSiteLastsql
{
    public class DestTechSiteLastsql : HHandlerDbULoaderStatTMKKSNAMEDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO"
        //    , s_strIdTEC = @"6";
        /// <summary>
        /// Конструктор - вспомогательный (статическая сборка)
        /// </summary>
        public DestTechSiteLastsql()
            : base()
        {
        }
        /// <summary>
        /// Конструктор - основной (динамическая загрузка)
        /// </summary>
        /// <param name="iPlugIn">Объект для связи с "родительским" приложением</param>
        public DestTechSiteLastsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsTechSiteLastsql : GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsTechSiteLastsql(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            //protected override object getIdToInsert(int idLink)
            //{
            //    throw new NotImplementedException();
            //}

            protected override string getInsertValuesQuery(DataTable tblRes)
            {
                string strRes = string.Empty
                    , strRow = string.Empty;
                Type typeVal = tblRes.Columns[@"VALUE"].DataType;

                //Logging.Logg().Debug(@"GroupSignalsStatKKSNAMEsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                strRow = @"UPDATE [" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"]"
                                    + @"SET [ID_SRV_TM]=" + (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).m_strIdSrvTM + @",";

                foreach (DataRow row in tblRes.Rows)
                {
                    strRes += strRow;

                    strRes += @"[VALUE]='";
                    if (typeVal.Equals(typeof (decimal)) == true)
                        strRes += ((decimal)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                    else
                        strRes += row[@"VALUE"];
                    strRes +=  @"',";
                    strRes += @"[DATETIME]='" + ((DateTime)row[@"DATETIME"]).AddHours(-6).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"'" + @",";
                    strRes += @"[UPDATE_DATETIME]=GETDATE()";

                    strRes += @" WHERE [KKS_NAME]='" + getIdToInsert(Int32.Parse(row[@"ID"].ToString().Trim())) + @"';";
                }

                return
                    //string.Empty
                    strRes
                    ;
            }
        }

        protected override GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsTechSiteLastsql(this, objs);
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2005;

            createObject(typeof(DestTechSiteLastsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
