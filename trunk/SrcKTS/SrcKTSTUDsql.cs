using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using uLoaderCommon;

namespace SrcKTS
{
    public class SrcKTSTUDsql : HHandlerDbULoaderDatetimeSrc
    {
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public SrcKTSTUDsql()
            : base(@"yyyyMMdd HH:mm:ss", MODE_CURINTERVAL.CAUSE_PERIOD_DAY, MODE_CURINTERVAL.FULL_PERIOD)
        {

        }
        /// <summary>
        /// Конструктор - основной (для создания из динамически подгружаемой библиотеки)
        /// </summary>
        /// <param name="iPlugIn">Объект для обмена сообщенями с основной программой</param>
        public SrcKTSTUDsql(PlugInULoader iPlugIn)
            : base(iPlugIn, @"yyyyMMdd HH:mm:ss", MODE_CURINTERVAL.CAUSE_PERIOD_DAY, MODE_CURINTERVAL.FULL_PERIOD)
        {
        }
        /// <summary>
        /// Класс для описания группы сигналов
        /// </summary>
        private class GroupSignalsKTSTUDsql : GroupSignalsDatetimeSrc
        {
            /// <summary>
            /// Конструктор основной (с параметрами)
            /// </summary>
            /// <param name="parent">Ссылка на объект-владелец</param>
            /// <param name="id">Идентификатор группы сигналов</param>
            /// <param name="pars">Свойства группы сигналов</param>
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
                string cmd =
                    string.Empty;

                //Формировать запрос
                i = 0;
                foreach (GroupSignalsKTSTUDsql.SIGNALIdsql s in m_arSignals)
                    if (s.IsFormula == false) {
                        m_strQuery += string.Format(@"exec dbo.ep_AskVTIdata @cmd='{0}'"
                            + @", @idVTI={1}"
                            + @", @TimeStart='{2}'"
                            + @", @TimeEnd='{3}'"
                            + @", @idReq={4};"
                            , i++ == 0 ? @"List" : @"ListAdd", s.m_iIdLocal, DateTimeBeginFormat, DateTimeEndFormat, idReq);
                    } else
                        // формула
                        ;

                // все поля idVTI, idReq, TimeIdx, TimeRTC, TimeSQL, idState, ValueFl, ValueInt, IsInteger, idUnit
                m_strQuery += string.Format(@"SELECT res.[idVTI] as [ID], SUM(res.[ValueFl]) as [VALUE]"
                    + @", DATEADD(DAY, {1}, res.[DATETIME]) as [DATETIME]"
                    //+ @", {2} as [UTC_OFFSET]" //DATEDIFF(HOUR, GETDATE(), GETUTCDATE()) as [UTC_OFFSET]
                    + @", COUNT(*) as [COUNT]"
                        + @" FROM ("
                            + @"SELECT [idVTI], [ValueFl]"
                                + @", DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime)), 0), [TimeSQL]) / 60.) * 60, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime)), 0)) as [DATETIME]"
                            + @" FROM [VTIdataList]"
                            + @" WHERE idREQ = {2}"
                            + @" GROUP BY [IdResult], [idVTI], [ValueFl]"
                                + @", DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime)), 0), [TimeSQL]) / 60.) * 60, DATEADD(DAY, DATEDIFF(DAY, 0, CAST('{0}' as datetime)), 0))"
                    + @") res"
                    + @" GROUP BY [idVTI], [DATETIME]"
                    + @" ORDER BY [idVTI], [DATETIME];"
                    , DateTimeBeginFormat, _modeDateTime == MODE_DATA_DATETIME.Begined ? -1 : _modeDateTime == MODE_DATA_DATETIME.Ended ? 0 : 0, idReq);

                m_strQuery += string.Format(@"exec [dbo].ep_AskVTIdata @cmd='{0}'"
                    + @", @idReq={1};", @"Clear", idReq);
            }
            /// <summary>
            /// Создать объект для сигнала с параметрами
            /// </summary>
            /// <param name="objs">Параметры/характеристики сигнала</param>
            /// <returns>Объект созданного сигнала</returns>
            protected override SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG, DERIVATIVE
                return new SIGNALKTSTUsql(this, (int)objs[0], /*(int)*/objs[2], bool.Parse((string)objs[3]), (int)objs[4]);
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
            return new GroupSignalsKTSTUDsql(this, id, objs);
        }
        /// <summary>
        /// Преобразовать таблицу к известному(с заранее установленной структурой) виду
        /// </summary>
        /// <param name="table">Таблица с данными для преобразования</param>
        protected override void parseValues(DataTable table)
        {
            DataTable tblRes = new DataTable();
            DataRow[] rowsSgnl = null;
            DateTime dtValue;
            double dblSumValue = -1F;
            List<DateTime> listNotCompletedDatetimeValues = new List<DateTime>();

            tblRes.Columns.AddRange(new DataColumn[] {
                new DataColumn (@"ID", typeof (int))
                , new DataColumn (@"DATETIME", typeof (DateTime))
                , new DataColumn (@"VALUE", typeof (float))
            });

            foreach (GroupSignalsSrc.SIGNALIdsql sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
                if (sgnl.IsFormula == false) {
                    // получить все строки для сигнала
                    rowsSgnl = table.Select(@"ID=" + sgnl.m_iIdLocal, @"DATETIME");

                    if (rowsSgnl.Length > 0) {
                        //Для каждого сигнала предполагаем, что все данные в наличии(полные)
                        listNotCompletedDatetimeValues.Clear();
                        // необходимо присвоить некоторое начальное значение (по нему определим - есть ли хотя бы одно значение для сигнала)
                        dtValue = DateTime.MinValue;
                        //Для каждого сигнала суммировать начинать с "0"
                        dblSumValue = 0F;
                        //cntRec = 0;
                        //??? обработка всех последующих строк, а если строк > 2
                        foreach (DataRow r in rowsSgnl) {
                            dtValue = ((DateTime)r[@"DATETIME"]).AddHours(m_tsOffsetUTCToData.Value.Hours); //(int)rowsSgnl[0][@"UTC_OFFSET"]

                            if ((int)rowsSgnl[0][@"COUNT"] == 2) // должно быть 2 полу-часовых значения
                                dblSumValue += (double)r[@"VALUE"] / (((sgnl as GroupSignalsSrc.SIGNALKTSTUsql).Derivative == 0) ? 1 : ((sgnl as GroupSignalsSrc.SIGNALKTSTUsql).Derivative == 1) ? 2 : 1);
                            else
                            // отметить, что за этот час значение не было получено
                                listNotCompletedDatetimeValues.Add(dtValue);
                        }

                        if ((dtValue > DateTime.MinValue)
                            && (listNotCompletedDatetimeValues.Count == 0))
                            // вставить строку
                            tblRes.Rows.Add(new object[] {
                                sgnl.m_idMain
                                , dtValue
                                , sgnl.m_bAVG == false ?
                                    dblSumValue : // оставить как есть (сумма)
                                        dblSumValue / (rowsSgnl.Length * 2) // при необходимости найти среднее
                            });
                        else
                            ; // неполные данные
                    } else
                        ; // нет данных
                } else
                    // формула
                    ;

            // вызов базового метода
            base.parseValues(tblRes);
        }
    }
}
