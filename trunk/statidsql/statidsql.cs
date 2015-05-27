using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

namespace statidsql
{
    public class statidsql : HHandlerDbULoader
    {
        DataTable [] m_arTableResults;

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
            private enum INDEX_DATATABLE_RES
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

            private DataTable []m_arTableRes;
            public override DataTable TableResults
            {
                get { return m_arTableRes[(int)INDEX_DATATABLE_RES.CURRENT]; }

                set
                {
                    m_arTableRes[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRes[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                    m_arTableRes[(int)INDEX_DATATABLE_RES.CURRENT] = value.Copy();
                }
            }

            public GroupSignalsStatIdSQL(object[] pars)
                : base(pars)
            {
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALStatIdSQL((int)objs[0], (int)objs[1], (int)objs[3]);
            }
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;
            
            switch ((StatesMachine)state)
            {
                case StatesMachine.CurrentTime:
                    GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    break;
                case StatesMachine.Insert:
                    string query = GetInsertValuesQuery ();
                    if (query.Equals (string.Empty) == false)
                        Request(m_dictIdListeners[m_IdGroupSignalsCurrent][0], query);
                    else
                        ;
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
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        public int Insert (int id, DataTable tableIn)
        {
            int iRes = 0;

            TableResults = tableIn.Copy ();

            if (IsStarted == true)
            {
                //AddState ((int)StatesMachine.Insert);
                AddState((int)StatesMachine.CurrentTime);

                Run(@"statidsql::Insert () - ...");
            }
            else
                ;

            return iRes;
        }

        private string GetInsertValuesQuery ()
        {
            string strRes = string.Empty;

            return strRes;
        }

        private DataTable getTableIns(ref DataTable table)
        {
            DataTable tableRes = new DataTable();
            DataRow[] arSelIns = null;

            table.Columns.Add(@"tmdelta", typeof(int));

            for (int s = 0; s < Signals.Length; s++)
            {
                try
                {
                    //arSelIns = (table as DataTable).Select(string.Empty, @"ID, DATETIME");
                    arSelIns = (table as DataTable).Select(@"ID=" + (Signals[s] as GroupSignalsStatIdSQL.SIGNALStatIdSQL).m_idLink, @"DATETIME");
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::getTableIns () - ...");
                }

                if (!(arSelIns == null))
                    for (int i = 0; i < arSelIns.Length; i++)
                    {
                        tableRes.ImportRow(arSelIns[i]);

                        //Проверитьт № итерации
                        if (i == 0)
                        {//Только при прохождении 1-ой итерации цикла
                            //iTMDelta = 
                            setTMDelta((Signals[s] as GroupSignalsStatIdSQL.SIGNALStatIdSQL).m_idLink, (DateTime)arSelIns[i][@"DATETIME"]);
                        }
                        else
                        {
                            //Определить смещение "соседних" значений сигнала
                            int iTMDelta = (int)((DateTime)arSelIns[i][@"DATETIME"] - (DateTime)arSelIns[i - 1][@"DATETIME"]).TotalMilliseconds;
                            arSelIns[i - 1][@"tmdelta"] = iTMDelta;
                            Console.WriteLine(@", tmdelta=" + arSelIns[i - 1][@"tmdelta"]);
                        }

                        Console.Write(@"ID=" + arSelIns[i][@"ID"] + @", DATETIME=" + ((DateTime)arSelIns[i][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                    }
                //Корректировать вывод
                if (arSelIns.Length > 0)
                    Console.WriteLine();
                else ;
            } //Цикл по сигналам...

            //tableRes.AcceptChanges();

            return tableRes;
        }

        private int clearDupValues(ref DataTable table)
        {
            int iRes = 0
                , cnt = -1;

            DataRow[] arSel;
            foreach (DataRow rRes in TableResults.Rows)
            {
                arSel = (table as DataTable).Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                iRes += arSel.Length;
                foreach (DataRow rDel in arSel)
                    (table as DataTable).Rows.Remove(rDel);
                table.AcceptChanges();
            }

            //!!! См. ВНИМАТЕЛЬНО файл конфигурации - ИДЕНТИФИКАТОРЫ д.б. уникальные
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

            return iRes;
        }

        private int setTMDelta(int id, DateTime dtCurrent)
        {
            int iRes = -1;
            DataRow[] arSelWas = null;

            //Проверить наличие столбцов в результ./таблице (признак получения рез-та)
            if (TableResults.Columns.Count > 0)
            {//Только при наличии результата
                arSelWas = TableResults.Select(@"ID=" + id, @"DATETIME DESC");
                //Проверить результат для конкретного сигнала
                if ((!(arSelWas == null)) && (arSelWas.Length > 0))
                {//Только при наличии рез-та по конкретному сигналу
                    iRes = (int)(dtCurrent - (DateTime)arSelWas[0][@"DATETIME"]).TotalMilliseconds;
                    arSelWas[0][@"tmdelta"] = iRes;

                    Console.WriteLine(@"Установлен для ID=" + id + @", DATETIME=" + ((DateTime)arSelWas[0][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + iRes);
                }
                else
                    ; //Без полученного рез-та для конкретного сигнала - невозможно
            }
            else
                ; //Без полученного рез-та - невозможно

            return iRes;
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState ((int)StatesMachine.Insert);

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
