using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

using HClassLibrary;
using uLoaderCommon;

namespace statidsql
{
    public class statidsql : HHandlerDbULoader
    {
        private static string m_strNameDestTable = @"ALL_PARAM_SOTIASSO-TEST";

        enum StatesMachine
        {
            Unknown = -1
            , CurrentTime
            , Values
            , Insert
        }

        public statidsql()
            : base()
        {
        }

        public statidsql(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsStatIdSQL : GroupSignals
        {
            public enum INDEX_DATATABLE_RES
            {
                PREVIOUS,
                CURRENT
                    , COUNT_INDEX_DATATABLE_RES
            }

            public class SIGNALStatIdSQL : GroupSignals.SIGNAL
            {
                public int m_idLink
                    , m_idStat;

                public SIGNALStatIdSQL(int idMain, int idLink, int idStat) : base (idMain)
                {
                    this.m_idLink = idLink;
                    this.m_idStat = idStat;
                }
            }

            private DataTable []m_arTableRec;
            public override DataTable TableRecieved
            {
                get
                {
                    lock (this)
                    {
                        return m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT];
                    }
                }

                set
                {
                    lock (this)
                    {
                        if (value.Rows.Count > 0)
                        {
                            if (m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count > 0)
                                m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                            else
                                ;
                            m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] = value.Copy();
                        }
                        else
                            ;
                    }
                }
            }

            //Для 'GetInsertQuery'
            public DataTable TableRecievedPrev { get { return m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS]; } }
            public DataTable TableRecievedCur { get { return m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT]; } }

            public GroupSignalsStatIdSQL(object[] pars)
                : base(pars)
            {
                m_arTableRec = new DataTable[(int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES];
                for (int i = 0; i < (int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES; i ++)
                    m_arTableRec[i] = new DataTable();
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALStatIdSQL((int)objs[0], (int)objs[1], (int)objs[3]);
            }

            private DataTable getTableIns(ref DataTable table)
            {
                DataTable tableRes = new DataTable();
                DataRow[] arSelIns = null;
                DataRow rowCur = null
                    , rowAdd
                    , rowPrev = null;
                int idSgnl = -1
                    , tmDelta = -1;

                if ((table.Columns.Count > 2)
                    && ((!(table.Columns.IndexOf (@"ID") < 0)) && (!(table.Columns.IndexOf (@"DATETIME") < 0))))
                {
                    table.Columns.Add(@"tmdelta", typeof(int));
                    tableRes = table.Clone ();

                    for (int s = 0; s < Signals.Length; s++)
                    {
                        try
                        {
                            idSgnl = (Signals[s] as GroupSignalsStatIdSQL.SIGNALStatIdSQL).m_idLink;
                            
                            //arSelIns = (table as DataTable).Select(string.Empty, @"ID, DATETIME");
                            arSelIns = (table as DataTable).Select(@"ID=" + idSgnl, @"DATETIME");
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"statidsql::getTableIns () - ...");
                        }

                        if (!(arSelIns == null))
                            for (int i = 0; i < arSelIns.Length; i++)
                            {
                                if (i < (arSelIns.Length - 1))
                                {
                                    tableRes.ImportRow (arSelIns[i]);
                                    rowCur = tableRes.Rows[tableRes.Rows.Count - 1];
                                }
                                else
                                    //Не вставлять без известной 'tmdelta'
                                    rowCur = null;

                                //Проверитьт № итерации
                                if (i == 0)
                                {//Только при прохождении 1-ой итерации цикла
                                    tmDelta = -1;
                                    //Определить 'tmdelta' для записи из предыдущего опроса
                                    rowAdd = null;
                                    rowPrev = setTMDelta(idSgnl, (DateTime)arSelIns[i][@"DATETIME"], out tmDelta);

                                    if ((!(rowPrev == null))
                                        && (tmDelta > 0))
                                    {
                                        //Добавить из предыдущего опроса
                                        rowAdd = tableRes.Rows.Add();
                                        //Скопировать все значения
                                        foreach (DataColumn col in tableRes.Columns)
                                        {
                                            if (col.ColumnName.Equals (@"tmdelta") == true)
                                                //Для "нового" столбца - найденное значение
                                                rowAdd[col.ColumnName] = tmDelta;
                                            else
                                                //"Старые" значения
                                                rowAdd[col.ColumnName] = rowPrev[col.ColumnName];
                                        }

                                        Console.WriteLine(@"Установлен для ID=" + idSgnl + @", DATETIME=" + ((DateTime)rowAdd[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + rowAdd[@"tmdelta"]);
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    //Определить смещение "соседних" значений сигнала
                                    long iTMDelta = (((DateTime)arSelIns[i][@"DATETIME"]).Ticks - ((DateTime)arSelIns[i - 1][@"DATETIME"]).Ticks) / TimeSpan.TicksPerMillisecond;                                    
                                    rowPrev[@"tmdelta"] = (int)iTMDelta;
                                    Console.WriteLine(@", tmdelta=" + rowPrev[@"tmdelta"]);
                                }

                                if (! (rowCur == null))
                                    Console.Write(@"ID=" + rowCur[@"ID"] + @", DATETIME=" + ((DateTime)rowCur[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                                else
                                    Console.Write(@"ID=" + arSelIns[i][@"ID"] + @", DATETIME=" + ((DateTime)arSelIns[i][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));

                                rowPrev = rowCur;
                            }
                        else
                            ; //arSelIns == null

                        //Корректировать вывод
                        if (arSelIns.Length > 0)
                            Console.WriteLine();
                        else ;
                    } //Цикл по сигналам...
                }
                else
                    ; //Отсутствуют необходимые столбцы (т.е. у таблицы нет структуры)

                tableRes.AcceptChanges();

                return tableRes;
            }

            private int getIdStat (int idLink)
            {
                int iRes = -1;

                foreach (SIGNALStatIdSQL sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        iRes = sgnl.m_idStat;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            public string GetInsertValuesQuery ()
            {
                string strRes = string.Empty;

                DataTable tblRes = getTableRes ();

                if ((!(tblRes == null)) 
                    && (tblRes.Rows.Count > 0))
                {
                    string strRow = string.Empty;

                    strRes = @"INSERT INTO [dbo].[" + m_strNameDestTable + @"] ("
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

                       strRow += getIdStat (Int32.Parse (row[@"ID"].ToString().Trim())) + @",";
                       strRow += @"6" + @",";
                       strRow += ((decimal)row[@"VALUE"]).ToString("F3", CultureInfo.InvariantCulture) + @",";
                       strRow += @"'" + ((DateTime)row[@"DATETIME"]).AddHours(-6).ToString (@"yyyyMMdd HH:mm:ss.fff") + @"',";
                       strRow += row[@"tmdelta"] + @",";
                       strRow += @"GETDATE()";

                       strRow += @"),";

                       strRes += strRow;
                    }
                    //Лишняя ','
                    strRes = strRes.Substring (0, strRes.Length - 1);
                }
                else
                    ;

                //Console.WriteLine (@"Запрос на вставку [" + );

                return
                    //string.Empty
                    strRes
                    ;
            }

            private DataTable getTableRes ()
            {
                DataTable tblDiff = clearDupValues ()
                    , tblRes = getTableIns (ref tblDiff);

                return tblRes;
            }

            private DataTable clearDupValues()
            {
                int iDup = 0;

                DataTable tblPrev = TableRecievedPrev.Copy()
                    , tblRes = TableRecieved.Copy();

                if (((!(tblRes.Columns.IndexOf(@"ID") < 0)) && (!(tblRes.Columns.IndexOf(@"DATETIME") < 0)))
                    && (tblRes.Rows.Count > 0))
                {
                    DataRow[] arSel;
                    foreach (DataRow rRes in tblPrev.Rows)
                    {
                        arSel = (tblRes as DataTable).Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                        iDup += arSel.Length;
                        foreach (DataRow rDel in arSel)
                            (tblRes as DataTable).Rows.Remove(rDel);
                        tblRes.AcceptChanges();
                    }

                    //!!! См. ВНИМАТЕЛЬНО файл конфигурации - ИДЕНТИФИКАТОРЫ д.б. уникальные
                    //int cnt = -1;
                    //List <int>listDel = new List<int>();
                    //foreach (DataRow rRes in table.Rows)
                    //{
                    //    arSel = (table as DataTable).Select(@"ID=" + rRes[@"ID"]
                    //        + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                    //    if (arSel.Length > 1)
                    //    {
                    //        iRes ++;
                    //        //Logging.Logg().Error(@"HBiyskTMOra::clearDupValues () - "
                    //        //    + @"ID=" + rRes[@"ID"]
                    //        //    + @", " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'"
                    //        //    + @", " + @"QUALITY[" + arSel[0][@"QUALITY"] + @"," + arSel[1][@"QUALITY"] + @"]"
                    //        //, Logging.INDEX_MESSAGE.NOT_SET);
                    //        cnt = listDel.Count + arSel.Length;
                    //        foreach (DataRow rDel in arSel)
                    //            if (listDel.Count < (cnt - 1))
                    //                listDel.Add(table.Rows.IndexOf(rDel));
                    //            else
                    //                break;
                    //    }
                    //    else
                    //        ;
                    //}

                    //foreach (int indx in listDel)
                    //    //(table as DataTable).Rows.Remove(rDel);
                    //    (table as DataTable).Rows.RemoveAt(indx);
                    //table.AcceptChanges();
                }
                else
                    ;

                return tblRes;
            }

            private DataRow setTMDelta(int id, DateTime dtCurrent, out int tmDelta)
            {
                tmDelta = -1;
                DataRow rowRes = null;
                DataRow[] arSelWas = null;

                //Проверить наличие столбцов в результ./таблице (признак получения рез-та)
                if (TableRecievedPrev.Columns.Count > 0)
                {//Только при наличии результата
                    arSelWas = TableRecievedPrev.Select(@"ID=" + id, @"DATETIME DESC");
                    //Проверить результат для конкретного сигнала
                    if ((!(arSelWas == null)) && (arSelWas.Length > 0))
                    {//Только при наличии рез-та по конкретному сигналу
                        rowRes = arSelWas[0];
                        tmDelta = (int)(dtCurrent - (DateTime)arSelWas[0][@"DATETIME"]).TotalMilliseconds;

                        //arSelWas[0][@"tmdelta"] = iRes;
                        //Console.WriteLine(@"Установлен для ID=" + id + @", DATETIME=" + ((DateTime)arSelWas[0][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + iRes);
                    }
                    else
                        ; //Без полученного рез-та для конкретного сигнала - невозможно
                }
                else
                    ; //Без полученного рез-та - невозможно

                return rowRes;
            }
        }

        public override void ClearValues()
        {
            //TableResults = new DataTable ();
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;
            
            switch ((StatesMachine)state)
            {
                case StatesMachine.CurrentTime:
                    if (!(m_IdGroupSignalsCurrent < 0))
                        GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    else
                        throw new Exception(@"statdidsql::StateRequest () - state=" + state.ToString() + @"...");
                    break;
                case StatesMachine.Values:
                    break;
                case StatesMachine.Insert:
                    string query = (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsStatIdSQL).GetInsertValuesQuery ();
                    if (query.Equals (string.Empty) == false)
                        Request(m_dictIdListeners[m_IdGroupSignalsCurrent][0], query);
                    else
                        Logging.Logg().Error(@"statidsql::StateRequest () ::" + ((StatesMachine)state).ToString() + @" - "
                            + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                            + @"query=Empty" + @"..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;

            switch ((StatesMachine)state)
            {
                case StatesMachine.CurrentTime:
                    m_dtServer = (DateTime)(obj as DataTable).Rows[0][0];
                    Logging.Logg().Error(@"statidsql::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
                        + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                        + @"DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff") + @"..."
                        , Logging.INDEX_MESSAGE.NOT_SET);
                    break;
                case StatesMachine.Values:
                    break;
                case StatesMachine.Insert:
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, int req, int res)
        {
            Logging.Logg().Error(@"statidsql::StateErrors (state=" + ((StatesMachine)state).ToString () + @", req=" + req + @", res=" + res + @") - "
                + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]"
                + @"..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(@"statidsql::StateWarnings (state" + ((StatesMachine)state).ToString() + @", req=" + req + @", res=" + res + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        public int Insert (int id, DataTable tableIn)
        {
            int iRes = 0;

            lock (m_lockStateGroupSignals)
            {
                if (m_dictGroupSignals[id].IsStarted == true)
                {
                    m_dictGroupSignals[id].TableRecieved = tableIn.Copy();

                    enqueue(id);
                }
                else
                    ;
            }

            return iRes;
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            //AddState((int)StatesMachine.Values);
            AddState((int)StatesMachine.Insert);

            return iRes;
        }

        protected override GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsStatIdSQL(objs);
        }
    }

    public class PlugIn : PlugInULoader
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 2001;

            createObject(typeof(statidsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            statidsql target = _object as statidsql;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.TO_INSERT:
                    target.Insert((int)(ev.par as object[])[0], (ev.par as object[])[1] as DataTable);
                    break;
                default:                    
                    break;
            }

            base.OnEvtDataRecievedHost(obj);
        }
    }
}
