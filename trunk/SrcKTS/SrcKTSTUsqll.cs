using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

namespace SrcKTS
{
    public class SrcKTSTUsql : HHandlerDbULoaderDatetimeSrc
    {
        public SrcKTSTUsql()
            : base(@"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        public SrcKTSTUsql(PlugInULoader iPlugIn)
            : base(iPlugIn, @"dd/MM/yyyy HH:mm:ss", MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }

        private class GroupSignalsKTSTUsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsKTSTUsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG
                return new SIGNALIdsql(this, (int)objs[0], /*(int)*/objs[2], bool.Parse((string)objs[3]));
            }

            /// <summary>
            /// Формирование запроса
            /// </summary>
            protected override void setQuery()
            {
                int idReq = HMath.GetRandomNumber()
                    , i = -1;
                string cmd =
                    string.Empty;
                //Формировать запрос
                i = 0;
                foreach (GroupSignalsKTSTUsql.SIGNALIdsql s in m_arSignals)
                {
                    if (s.IsFormula == false)
                    {
                        if (i == 0)
                            cmd = @"List";
                        else
                            if (i == 1)
                                cmd = @"ListAdd";
                            else
                                ; // оставить без изменений

                        m_strQuery += string.Format(@"exec dbo.ep_AskVTIdata @cmd='{0}'"
                            + @", @idVTI={1}"
                            + @", @TimeStart='{2}'"
                            + @", @TimeEnd='{3}'"
                            + @", @idReq={4};"
                            , cmd, s.m_iIdLocal, DateTimeBeginFormat, DateTimeEndFormat, idReq);

                        i++;
                    }
                    else
                        // формула
                        ;
                }

                // все поля idVTI, idReq, TimeIdx, TimeRTC, TimeSQL, idState, ValueFl, ValueInt, IsInteger, idUnit
                m_strQuery += string.Format(@"SELECT res.[idVTI], SUM(res.[ValueFl]) sum_ValueFl"
	                + @", res.[DATETIME]"
                    + @", COUNT(*) as [COUNT]"
                        + @" FROM ("
                            + @"SELECT [idVTI], [ValueFl]"
                                + @", DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime2(7))), 0), [TimeSQL]) / 60.) * 60, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime2(7))), 0)) as [DATETIME]"
                            + @" FROM [VTIdataList]"
                            + @" WHERE idREQ = {1}"
                            + @" GROUP BY"
                                + @" DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime2(7))), 0), [TimeSQL]) / 60.) * 60, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime2(7))), 0))"
                                + @", [idVTI], [ValueFl]"
                    + @") res"
                    + @" GROUP BY [idVTI], [DATETIME]"
                    + @" ORDER BY [idVTI], [DATETIME];", DateTimeBeginFormat, idReq);

                m_strQuery += string.Format(@"exec [dbo].ep_AskVTIdata @cmd='{0}'"
                    + @", @idReq={1};", @"Clear", idReq);
            }

            /// <summary>
            /// Возвратить основной идентификатор по косвенному(связанному) идентификатору
            /// </summary>
            /// <param name="id_link">Связанный идентификатор</param>
            /// <returns>Основной идентификатор (строка или целочисленное значение)</returns>
            protected override object getIdMain(object id_link)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Возвратить объект группы сигналов
        /// </summary>
        /// <param name="id">Идентификатор группы сишналов</param>
        /// <param name="objs">Параметры группы сигналов</param>
        /// <returns>Объект (вновь созданный) группы сигналов</returns>
        protected override GroupSignals createGroupSignals(int id, object[] objs)
        {
            return new GroupSignalsKTSTUsql(this, id, objs);
        }

        /// <summary>
        /// Преобразовать таблицу к известному(с заранее установленной структурой) виду
        /// </summary>
        /// <param name="table">Таблица с данными для преобразования</param>
        protected override void parseValues(DataTable table)
        {
            //base.parseValues (table);

            DataTable tblRes = new DataTable ();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblSumValue = -1F;

            tblRes.Columns.AddRange(new DataColumn [] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsSrc.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
            {
                if (sgnl.IsFormula == false)
                {
                    rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal, @"DATETIME");

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
                            //??? обработка всех последующих строк, а если строк > 2
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
                            // значения за разные интервалы интегрирования
                            //break
                            continue
                            ;
                    }
                    else
                        ; // неполные данные
                }
                else
                    // формула
                    ;
            }

            base.parseValues(tblRes);
        }
    }
}
