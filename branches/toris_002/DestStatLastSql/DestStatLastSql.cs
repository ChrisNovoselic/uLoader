using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace DestStatLastSql
{
    public class DestStatLastSql : HHandlerDbULoaderStatTMKKSNAMEDest
    {
        //private static string s_strNameDestTable = @"ALL_PARAM_SOTIASSO"
        //    , s_strIdTEC = @"6";
        /// <summary>
        /// Конструктор - вспомогательный (статическая сборка)
        /// </summary>
        public DestStatLastSql()
            : base()
        {
        }
        /// <summary>
        /// Конструктор - основной (динамическая загрузка)
        /// </summary>
        /// <param name="iPlugIn">Объект для связи с "родительским" приложением</param>
        public DestStatLastSql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatLastSql : GroupSignalsStatTMKKSNAMEDest
        {
            public GroupSignalsStatLastSql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            //protected override object getIdToInsert(int idLink)
            //{
            //    throw new NotImplementedException();
            //}

            protected override string getTargetValuesQuery()
            {
                string strRes = string.Empty
                    , strRow = string.Empty;
                Type typeVal = m_DupTables.TableDistinct.Columns[@"VALUE"].DataType;
                int idSrvTM = (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).GetIdSrvTM(m_IdSourceConnSett);

                //Logging.Logg().Debug(@"GroupSignalsStatKKSNAMEsql::getInsertValuesQuery () - Type of results DateTable column[VALUE]=" + tblRes.Columns[@"Value"].DataType.AssemblyQualifiedName + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                strRow = @"UPDATE [" + (_parent as HHandlerDbULoaderDest).m_strNameTable + @"]"
                                    + @"SET ";

                foreach (DataRow row in m_DupTables.TableDistinct.Rows)
                {
                    strRes += strRow;

                    strRes += @"[VALUE]='";
                    strRes += ((double)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture);

                    //if (typeVal.Equals(typeof(decimal)) == true)
                    //    strRes += ((decimal)row[@"VALUE"]).ToString("F7", CultureInfo.InvariantCulture);
                    //else
                    //    strRes += row[@"VALUE"];
                    strRes += @"',";
                    strRes += @"[DATETIME]='" + ((DateTime)row[@"DATETIME"]).ToString(s_strFormatDbDateTime) + @"'" + @",";
                    strRes += @"[UPDATE_DATETIME]=GETDATE()";

                    strRes += @" WHERE [ID_SIGNAL]='" + row[@"KKSNAME_MST"] + @"';";
                }

                return
                    //string.Empty
                    strRes
                    ;
            }

            protected override string getExistsValuesQuery()
            {
                return string.Empty;
            }
        }

        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsStatLastSql(this, id, objs);
        }
    }

    public class PlugIn : PlugInULoaderDest
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2008;

            registerType(_Id, typeof(DestStatLastSql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
