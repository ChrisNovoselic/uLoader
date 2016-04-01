using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using HClassLibrary;
using uLoaderCommon;

namespace SrcKTSTUDsql
{
    public class SrcKTSTUDsql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcKTSTUDsql()
            : base(@"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {

        }

        public SrcKTSTUDsql(IPlugIn iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        private class GroupSignalsKTSTUDsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsKTSTUDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            /// <summary>
            /// Установить содержание для запроса
            /// </summary>
            protected override void setQuery()
            {
                int idReq = HMath.GetRandomNumber()
                    , i = -1;
                string cmd = string.Empty;
                //перевод даты для суточного набора
                if (DateTimeStart != DateTimeBegin)
                    DateTimeBegin = (DateTimeBegin - DateTimeBegin.TimeOfDay).AddDays(PeriodMain.Days);
                else
                    DateTimeBegin = (DateTimeStart - DateTimeStart.TimeOfDay);
              
                //Формировать запрос
                i = 0;
                foreach (GroupSignalsKTSTUDsql.SIGNALIdsql s in m_arSignals)
                {
                    if (i == 0)
                        cmd = @"List";
                    else
                        if (i == 1)
                            cmd = @"ListAdd";
                        else ;

                    m_strQuery += @"exec e6work.dbo.ep_AskVTIdata @cmd='" + cmd + @"',"
                        + @"@idVTI=" + s.m_iIdLocal + @","
                        + @"@TimeStart='" + DateTimeBeginFormat + @"',"
                        + @"@TimeEnd='" + DateTimeEndFormat + @"',"
                        + @"@idReq=" + idReq
                        + @";";

                    i++;
                }

                m_strQuery += @"SELECT idVTI as [ID],idReq,TimeIdx,TimeRTC,TimeSQL as [DATETIME],idState,ValueFl as [VALUE],ValueInt,IsInteger,idUnit"
                        + @", DATEDIFF(HH, GETDATE(), GETUTCDATE()) as [UTC_OFFSET]"
                    + @" FROM e6work.dbo.VTIdataList"
                    + @" WHERE idReq=" + idReq
                    + @";";

                m_strQuery += @"exec e6work.dbo.ep_AskVTIdata @cmd='" + @"Clear" + @"',"
                    + @"@idReq=" + idReq
                    + @";";
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG
                return new SIGNALIdsql((int)objs[0], (int)objs[2], bool.Parse((string)objs[3]));
            }

            protected override object getIdMain(object id_link)
            {
                throw new NotImplementedException();
            }
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsKTSTUDsql(this, id, objs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        protected override void parseValues(System.Data.DataTable table)
        {
            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblSumValue = -1F;

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsKTSTUDsql.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
            {
                rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal, @"DATETIME");
                                    //вывод данных только при полных сутках
                if ((rowsSgnl.Length > 0)
                    && (rowsSgnl.Length % 48 == 0))
                {
                    dtValue = (DateTime)rowsSgnl[0][@"DATETIME"];
                    //Для обработки метки времени по UTC
                    dtValue = dtValue.AddHours((int)rowsSgnl[0][@"UTC_OFFSET"]).AddDays(PeriodMain.Days).AddMinutes(-30);
                    //Вычислить суммарное значение для сигнала
                    dblSumValue = 0F;
                    //cntRec = 0;
                    //??? обработка всех последующих строк, а если строк > 2
                    foreach (DataRow r in rowsSgnl)
                        dblSumValue += Convert.ToSingle(r[@"VALUE"].ToString());
                    // при необходимости найти среднее
                    if (sgnl.m_bAVG == true)
                        dblSumValue /= rowsSgnl.Length;
                    else
                        ;
                    // вставить строку
                    tblRes.Rows.Add(new object[] {
                            sgnl.m_idMain
                            , dtValue
                            , dblSumValue
                        });
                }
                else
                    continue
                    ;
                // не полные данные
            }
            base.parseValues(tblRes);
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1009;

            registerType(_Id, typeof(SrcKTSTUDsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
