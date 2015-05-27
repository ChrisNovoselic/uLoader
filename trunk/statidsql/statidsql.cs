using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using HClassLibrary;

namespace statidsql
{
    public class statidsql : HHandlerDb
    {
        protected IPlugIn _iPlugin;

        private Dictionary<int, GroupSignals> m_dictGroupSignals;

        private GroupSignals.SIGNAL[] Signals { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].Signals; } }
        public DataTable TableResults { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults; } set { m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults = value; } }

        private DateTime m_dtServer;
        private object m_lockStateGroupSignals;

        private int m_IdGroupSignalsCurrent;

        DataTable [] m_arTableResults;

        enum StatesMachine
        {
            Unknown = -1
            , CurrentTime
            , Values
            , Insert
        }

        public ConnectionSettings m_connSett;

        private class GroupSignals
        {
            private enum INDEX_DATATABLE_RES
            {
                PREVIOUS,
                CURRENT
                    , COUNT_INDEX_DATATABLE_RES
            }
            
            public enum STATE { UNKNOWN = -1, SLEEP, TIMER, QUEUE, ACTIVE }
            private STATE m_state;
            public STATE State { get { return m_state; } set { m_state = value; } }

            public struct SIGNAL
            {
                public int m_idMain
                    , m_idLink
                    , m_idStat;

                public SIGNAL(int idMain, int idLink, int idStat)
                {
                    this.m_idMain = idMain;
                    this.m_idLink = idLink;
                    this.m_idStat = idStat;
                }
            }

            private uLoaderCommon.MODE_WORK m_mode;
            public uLoaderCommon.MODE_WORK Mode { get { return m_mode; } set { m_mode = value; } }

            private DataTable []m_arTableRes;
            public DataTable TableResults
            {
                get { return m_arTableRes[(int)INDEX_DATATABLE_RES.CURRENT]; }

                set
                {
                    m_arTableRes[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRes[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                    m_arTableRes[(int)INDEX_DATATABLE_RES.CURRENT] = value.Copy();
                }
            }

            public GroupSignals(object[] pars)
            {
                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart = DateTime.MinValue;
                m_tmSpanPeriod = new TimeSpan((long)((int)uLoaderCommon.DATETIME.SEC_SPANPERIOD_DEFAULT * Math.Pow(10, 7)));
                m_msecInterval = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_DEFAULT;

                m_arTableRes = new DataTable[(int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES];
                for (int i = 0; i < m_arTableRes.Length; i++)
                    m_arTableRes[i] = new DataTable();

                //Инициализировать массив сигналов
                if (pars.Length > 0)
                {
                    m_arSignals = new SIGNAL[pars.Length];

                    for (int i = 0; i < pars.Length; i++)
                        m_arSignals[i] = new SIGNAL((int)(pars[i] as object[])[0]
                                                    , (int)(pars[i] as object[])[1]
                                                    , (int)(pars[i] as object[])[3]);
                }
                else
                    ;
            }

            private SIGNAL[] m_arSignals;
            public SIGNAL[] Signals { get { return m_arSignals; } }

            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } set { m_dtStart = value; } }
            private TimeSpan m_tmSpanPeriod;
            public TimeSpan TimeSpanPeriod { get { return m_tmSpanPeriod; } set { m_tmSpanPeriod = value; } }
            private long m_msecInterval;
            public long MSecInterval { get { return m_msecInterval; } set { m_msecInterval = value; } }

            public static STATE GetMode(uLoaderCommon.MODE_WORK mode, STATE prevState)
            {
                GroupSignals.STATE stateRes = GroupSignals.STATE.UNKNOWN;
                if (mode == uLoaderCommon.MODE_WORK.CUR_INTERVAL)
                    switch (prevState)
                    {
                        case STATE.ACTIVE:
                        case STATE.UNKNOWN:
                            stateRes = GroupSignals.STATE.TIMER;
                            break;
                        default:
                            stateRes = GroupSignals.STATE.SLEEP;
                            break;
                    }
                else
                    if (mode == uLoaderCommon.MODE_WORK.COSTUMIZE)
                        stateRes = GroupSignals.STATE.SLEEP;
                    else
                        //??? throw new Exception
                        ;

                return stateRes;
            }
        }

        public statidsql()
        {
            m_dtServer = DateTime.MinValue;
            //m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;

            m_IdGroupSignalsCurrent =
                0
                //-1 //???
                ;
            m_dictGroupSignals = new Dictionary<int, GroupSignals>();

            m_lockStateGroupSignals = new object();

            //m_lockQueue = new object();
            //m_queueIdGroupSignals = new Queue<int>();
            //threadQueueIsWorking = -1;
        }

        public statidsql(IPlugIn iPlugIn)
            : this()
        {
            this._iPlugin = iPlugIn;
        }
        
        public override void Start()
        {
            base.Start();

            StartDbInterfaces();
        }
        
        public override void StartDbInterfaces()
        {
            foreach (int id in m_dictGroupSignals.Keys)
                register(id, 0, m_connSett, string.Empty);
        }

        protected override void register(int id, int indx, ConnectionSettings connSett, string name)
        {
            bool bReq = true;

            if (m_dictIdListeners.ContainsKey(id) == false)
                m_dictIdListeners.Add(id, new int[] { -1 });
            else
                if (!(m_dictIdListeners[id][indx] < 0))
                    bReq = false;
                else
                    ;

            if (bReq == true)
                base.register(id, indx, connSett, name);
            else
                ;
        }

        public int Initialize(ConnectionSettings connSett)
        {
            int iRes = 0;

            m_connSett = new ConnectionSettings(connSett);

            return iRes;
        }

        public int Initialize(int id, object[] pars)
        {
            int iRes = 0;

            if (m_dictGroupSignals.Keys.Contains(id) == false)
                //Считать переданные параметры - параметрами сигналов
                m_dictGroupSignals.Add(id, new GroupSignals(pars));
            else
            {//Считать переданные параметры - параметрами группы сигналов
                if (m_dictGroupSignals[id].Signals == null)
                    iRes = -1;
                else
                {
                    lock (m_lockStateGroupSignals)
                    {
                        m_dictGroupSignals[id].Mode = (uLoaderCommon.MODE_WORK)pars[0];
                        m_dictGroupSignals[id].State = GroupSignals.GetMode(m_dictGroupSignals[id].Mode, GroupSignals.STATE.UNKNOWN);
                        //m_dictGroupSignals[id].DateTimeStart = (DateTime)pars[1];
                        //m_dictGroupSignals[id].TimeSpanPeriod = TimeSpan.FromSeconds((double)pars[2]);
                        //m_dictGroupSignals[id].MSecInterval = (int)pars[3];
                    }
                }
            }

            return iRes;
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }

        protected override int StateCheckResponse(int state, out bool error, out object outobj)
        {
            return response(out error, out outobj);
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

        public override void Stop()
        {
            base.Stop();
        }

        public void Stop(int id)
        {
            Stop();
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
                    arSelIns = (table as DataTable).Select(@"ID=" + Signals[s].m_idLink, @"DATETIME");
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
                            setTMDelta(Signals[s].m_idLink, (DateTime)arSelIns[i][@"DATETIME"]);
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
    }

    public class PlugIn : HHPlugIn
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
                case (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT:
                    target.Initialize(ev.par[0] as ConnectionSettings);
                    break;
                case (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP:
                    target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]);
                    break;
                case (int)ID_DATA_ASKED_HOST.START:
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT) == true)
                    {
                        if (target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object[]) == 0)
                        {
                            target.Start();
                            target.Activate(true);
                        }
                        else
                            DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP, (int)(ev.par as object[])[0] });
                    }
                    else
                        DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT });
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Stop((int)(ev.par as object[])[0]);
                    break;
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
