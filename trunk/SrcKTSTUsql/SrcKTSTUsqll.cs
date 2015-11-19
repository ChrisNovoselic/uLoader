using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

namespace SrcKTSTUsql
{
    public class SrcKTSTUsql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcKTSTUsql()
            : base(@"dd/MM/yyyy HH:mm:ss")
        {
        }

        public SrcKTSTUsql(IPlugIn iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss")
        {
        }

        private class GroupSignalsKTSTUsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsKTSTUsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN
                return new SIGNALKTSTUsql((int)objs[0], (int)objs[2], bool.Parse((string)objs[3]));
            }

            protected override void setQuery()
            {
                int idReq = HMath.GetRandomNumber ()
                    , i = -1;
                string cmd =
                    string.Empty;
                //Формировать запрос
                i = 0;
                foreach (GroupSignalsKTSTUsql.SIGNALKTSTUsql s in m_arSignals)
                {
                    if (i == 0)
                        cmd = @"List";
                    else
                        if (i == 1)
                            cmd = @"ListAdd";
                        else
                            ; // оставить без изменений

                    m_strQuery += @"exec e6work.dbo.ep_AskVTIdata @cmd='" + cmd + @"',"
                        + @"@idVTI=" + s.m_iIdKTS + @","
                        + @"@TimeStart='" + DateTimeBeginFormat + @"',"
                        + @"@TimeEnd='" + DateTimeEndFormat + @"',"
                        + @"@idReq=" + idReq
                        + @";";

                    i ++;
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
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsKTSTUsql(this, id, objs);
        }

        protected override void parseValues(System.Data.DataTable table)
        {
            //base.parseValues (table);

            DataTable tblRes = new DataTable ();
            DataRow[] rowsSgnl = null;
            //DataRow rowIns = null;
            DateTime dtValue;
            double dblSumValue = -1F;
            //int cntRec = -1;

            tblRes.Columns.AddRange(new DataColumn [] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsKTSTUsql.SIGNALKTSTUsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
            {
                rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdKTS, @"DATETIME");

                if ((rowsSgnl.Length > 0)
                    && (rowsSgnl.Length % 2 == 0))
                {
                    dtValue = (DateTime)rowsSgnl[0][@"DATETIME"];
                    //У 1-го значения минуты д.б. = 30
                    if (dtValue.Minute == 30)
                    {
                        //Для обработки метки времени по UTC
                        dtValue = dtValue.AddHours((int)rowsSgnl[0][@"UTC_OFFSET"]).AddMinutes(30);
                        //Вычислить суммарное значение для сигнала
                        dblSumValue = 0F;
                        //cntRec = 0;
                        foreach (DataRow r in rowsSgnl)
                        {
                            dblSumValue += (double)r[@"VALUE"];
                            //cntRec++;
                        }
                        // при необходимости найти среднее
                        if (sgnl.m_bAVG == true)
                            //dblSumValue /= cntRec;
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
                        break; // значения за разные интервалы интегрирования
                }
                else
                    ; // не полные данные
            }

            RowCountRecieved = tblRes.Rows.Count;

            TableRecieved = tblRes;
        }

        /// <summary>
        /// Актулизировать дату/время начала опроса
        /// </summary>
        /// <returns>Признак изменения даты/времени начала опроса</returns>
        protected override int actualizeDateTimeBegin()
        {
            int iRes = 0;
            if (Mode == MODE_WORK.CUR_INTERVAL)
                //Проверить признак 1-го запуска (в режиме CUR_INTERVAL)
                if (DateTimeBegin == DateTime.MinValue)
                {
                    if (s_modeCurInterval == MODE_CURINTERVAL.CAUSE_PERIOD)
                    {
                        //Выравнивание по "час"
                        DateTimeBegin = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
                        DateTimeBegin = DateTimeBegin.AddSeconds(-1 * (m_dtServer.Minute * 60 + m_dtServer.Second));
                    }
                    else
                        if (s_modeCurInterval == MODE_CURINTERVAL.CAUSE_NOT)
                            DateTimeBegin = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds);
                        else
                            ;

                    iRes = 1;
                }
                else
                    //Переход на очередной интервал (повторный опрос)
                    switch (s_modeCurInterval)
                    {
                        case MODE_CURINTERVAL.CAUSE_PERIOD:
                            //Проверить необходимость изменения даты/времени
                            if ((m_dtServer - DateTimeBegin.AddSeconds(PeriodLocal.TotalSeconds)).TotalMilliseconds > MSecIntervalLocal)
                            {
                                DateTimeBegin = DateTimeBegin.AddSeconds(PeriodLocal.TotalSeconds);
                                //CountMSecInterval++;
                                //Установить признак перехода
                                iRes = 1;
                            }
                            else
                                ;
                            break;
                        case MODE_CURINTERVAL.CAUSE_NOT:
                            DateTimeBegin = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds);
                            //Установить признак перехода
                            iRes = 1;
                            break;
                        default:
                            break;
                    }
            else
                if (Mode == MODE_WORK.COSTUMIZE)
                {
                    //Проверить признак 1-го запуска (в режиме COSTUMIZE)
                    if (DateTimeBegin == DateTime.MinValue)
                    {
                        //Проверить указано ли дата/время начала опроса
                        if (DateTimeStart == DateTime.MinValue)
                            //Не указано - опросить ближайший к текущей дате/времени период
                            DateTimeStart = m_dtServer.AddMilliseconds(-1 * PeriodLocal.TotalMilliseconds);
                        else
                            ;

                        DateTimeBegin = DateTimeStart;
                    }
                    else
                        //Повторный опрос
                        DateTimeBegin = DateTimeBegin.AddMilliseconds(PeriodLocal.TotalMilliseconds);

                    iRes = 1;
                }
                else
                    throw new Exception(@"HHandlerDbULoaderDatetimeSrc::actualizeDateTimeStart () - неизвестный режим ...");

            Logging.Logg().Debug(@"HHandlerDbULoader::actualizeDateTimeStart () - "
                                + @"[" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]"
                                + @", m_dtServer=" + m_dtServer.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", DateTimeBegin=" + DateTimeBegin.ToString(@"dd.MM.yyyy HH:mm.ss.fff")
                                + @", iRes=" + iRes
                                + @"...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1002;

            createObject(typeof(SrcKTSTUsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
