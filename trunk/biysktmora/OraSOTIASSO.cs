using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection

using HClassLibrary;
//using StatisticCommon;

namespace biysktmora
{    
    public class HBiyskTMOra : HHandlerDb
    {
        protected IPlugIn _iPlugin;

        private static int SEC_SPANPERIOD_DEFAULT = 60;
        private static int MSEC_INTERVAL_DEFAULT = 6666;

        private class GroupSignals
        {
            public struct SIGNAL
            {
                public int m_id;
                public string m_NameTable;

                public SIGNAL(int id, string table)
                {
                    this.m_id = id;
                    this.m_NameTable = table;
                }
            }

            private SIGNAL[] m_arSignals;
            public SIGNAL[] Signals { get { return m_arSignals; } }

            private DateTime m_dtStart;
            public DateTime DateTimeStart { get { return m_dtStart; } }
            private TimeSpan m_tmSpanPeriod;
            public TimeSpan TimeSpanPeriod { get { return m_tmSpanPeriod; } }
            private long m_msecInterval;
            public long MSecInterval { get { return m_msecInterval; } }

            private DataTable m_tableResults;
            public DataTable TableResults { get { return m_tableResults; } }

            public GroupSignals(object [] pars)
            {
                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                m_dtStart = DateTime.MinValue;
                m_tmSpanPeriod = new TimeSpan((long)(SEC_SPANPERIOD_DEFAULT * Math.Pow(10, 7)));
                m_msecInterval = MSEC_INTERVAL_DEFAULT;
                //Инициализировать массив сигналов
                if (pars.Length > 0)
                {
                    m_arSignals = new SIGNAL[pars.Length];                

                    for (int i = 0; i < pars.Length; i ++)
                        m_arSignals[i] = new SIGNAL((int)(pars[i] as object[])[0], (pars[i] as object[])[1] as string);
                }
                else
                    ;
                
                m_arSignals = new SIGNAL[]
                                    { /*new SIGNAL(20049, @"TAG_000049")
                                        , new SIGNAL(20002, @"TAG_000047")
                                        , new SIGNAL(20003, @"TAG_000048")
                                        , new SIGNAL(20004, @"TAG_000049")
                                        , new SIGNAL(20005, @"TAG_000050")
                                        , new SIGNAL(20006, @"TAG_000051")
                                        , new SIGNAL(20007, @"TAG_000052")
                                        , new SIGNAL(20008, @"TAG_000053")*/
                                    };
            }
        }

        private Dictionary<int, GroupSignals> m_dictGroupSignals;

        private GroupSignals.SIGNAL[] Signals { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].Signals; } }

        private TimeSpan TimeSpanPeriod { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].TimeSpanPeriod; } }
        private DateTime DateTimeStart { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].DateTimeStart; } }
        private long MSecInterval { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].MSecInterval; } }

        public DataTable TableResults { get { return m_dictGroupSignals[m_IdGroupSignalsCurrent].TableResults; } }

        enum StatesMachine {
            CurrentTime
            , Values
        }

        private DateTime m_dtServer;        
        public ConnectionSettings m_connSett;
        public string m_strQuery = string.Empty;
        private int m_IdGroupSignalsCurrent;

        public HBiyskTMOra()
        {
            m_dtServer = DateTime.MinValue;

            m_IdGroupSignalsCurrent = 0;

            m_dictGroupSignals = new Dictionary<int, GroupSignals>();
        }

        public HBiyskTMOra(IPlugIn iPlugIn) : this ()
        {
            this._iPlugin = iPlugIn;
        }

        public int Initialize(ConnectionSettings connSett)
        {
            int iRes = 0;

            m_connSett = new ConnectionSettings (connSett);

            return iRes;
        }

        public int Initialize(int id, object [] pars)
        {
            int iRes = 0;

            if (m_dictGroupSignals.Keys.Contains(id) == false)
                m_dictGroupSignals.Add(id, new GroupSignals(pars));
            else
                ;

            return iRes;
        }

        private void setQuery(DateTime dtStart, int secInterval = -1)
        {
            m_strQuery = string.Empty;            

            if (secInterval  < 0)
            {
                secInterval =
                    //SEC_INTERVAL_DEFAULT
                    (int)TimeSpanPeriod.TotalSeconds
                    ;
            }
            else
                ;

            string strUnion = @" UNION "
                //Строки для условия "по дате/времени"
                , strStart = dtStart.ToString(@"yyyyMMdd HHmmss")
                , strEnd = dtStart.AddSeconds(secInterval).ToString(@"yyyyMMdd HHmmss");
            //Формировать зпрос
            foreach (GroupSignals.SIGNAL s in Signals)
            {
                m_strQuery += @"SELECT " + s.m_id + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.m_NameTable + @" WHERE DATETIME BETWEEN"
                + @" to_timestamp('" + strStart + @"', 'yyyymmdd hh24miss')" + @" AND"
                + @" to_timestamp('" + strEnd + @"', 'yyyymmdd hh24miss')"
                + strUnion
                ;
            }

            //Удалить "лишний" UNION
            m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
            ////Установить сортировку
            //m_strQuery += @" ORDER BY DATETIME DESC";
        }

        public void ChangeState ()
        {
            ClearStates ();

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            Run(@"HBiyskTMOra::ChangeState ()");
        }

        public override void StartDbInterfaces ()
        {            
            foreach (int id in m_dictGroupSignals.Keys)
                register(id, 0, m_connSett, string.Empty);
        }

        protected override void register(int id, int indx, ConnectionSettings connSett, string name)
        {
            if (m_dictIdListeners.ContainsKey (id) == false)
                m_dictIdListeners.Add(id, new int[] { -1 });
            else
                ;

            base.register(id, indx, connSett, name);
        }

        public override void Start()
        {
            base.Start();

            StartDbInterfaces ();            
        }

        public override void ClearValues()
        {
            int iPrev = 0, iDel = 0, iCur = 0;
            if (! (TableResults == null))
            {
                iPrev = TableResults.Rows.Count;
                string strSel =
                    @"DATETIME<'" + DateTimeStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' OR DATETIME>='" + DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    ;

                DataRow[] rowsDel = null;
                try { rowsDel = TableResults.Select(strSel); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ClearValues () - ...");
                }

                if (! (rowsDel == null))
                {
                    iDel = rowsDel.Length;
                    if (rowsDel.Length > 0)
                    {
                        foreach (DataRow r in rowsDel)
                            TableResults.Rows.Remove(r);
                        //??? Обязательно ли...
                        TableResults.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;

                iCur = TableResults.Rows.Count;

                Console.WriteLine(@"Обновление рез-та: [было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @"]");
            }
            else
                ;
        }

        private int setTMDelta (int id, DateTime dtCurrent)
        {
            int iRes = -1;
            DataRow []arSelWas = null;
            
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

        private int clearDupValues (ref DataTable table)
        {
            int iRes = 0;

            DataRow[] arSel;
            foreach (DataRow rRes in TableResults.Rows)
            {
                arSel = (table as DataTable).Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                iRes += arSel.Length;
                foreach (DataRow rDel in arSel)
                    (table as DataTable).Rows.Remove(rDel);
                //table.AcceptChanges ();
            }

            return iRes;
        }

        private DataTable getTableIns(ref DataTable table)
        {
            DataTable tableRes = new DataTable();
            DataRow []arSelIns = null;

            table.Columns.Add (@"tmdelta", typeof (int));

            for (int s = 0; s < Signals.Length; s ++)
            {
                try
                {
                    //arSelIns = (table as DataTable).Select(string.Empty, @"ID, DATETIME");
                    arSelIns = (table as DataTable).Select(@"ID=" + Signals[s].m_id, @"DATETIME");
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::getTableIns () - ...");
                }

                if (! (arSelIns == null))
                    for (int i = 0; i < arSelIns.Length; i++)
                    {
                        tableRes.ImportRow(arSelIns[i]);                           

                        //Проверитьт № итерации
                        if (i == 0)
                        {//Только при прохождении 1-ой итерации цикла
                            //iTMDelta = 
                            setTMDelta(Signals[s].m_id, (DateTime)arSelIns[i][@"DATETIME"]);
                        }
                        else
                        {
                            //Определить смещение "соседних" значений сигнала
                            arSelIns[i - 1][@"tmdelta"] = (int)((DateTime)arSelIns[i][@"DATETIME"] - (DateTime)arSelIns[i - 1][@"DATETIME"]).TotalMilliseconds;
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

        private int actualizeDatetimeStart ()
        {
            int iRes = 0;

            if (DateTimeStart == DateTime.MinValue)
            {
                DateTimeStart = m_dtServer;
                DateTimeStart = DateTimeStart.AddSeconds(-1 * DateTimeStart.Second);
            }
            else
                ;

            if ((m_dtServer - DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds)).TotalSeconds > 6)
                DateTimeStart = DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds);
            else
                ;

            return iRes;
        }

        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            return response(out error, out table);
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.Oracle, m_dictIdListeners[0][0]);
                    break;
                case (int)StatesMachine.Values:                    
                    actualizeDatetimeStart ();
                    ClearValues();
                    setQuery(DateTimeStart);
                    Request (m_dictIdListeners[0][0], m_strQuery);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            DataTable table = obj as DataTable;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    m_dtServer = (DateTime)(table as DataTable).Rows[0][0];
                    Console.WriteLine(m_dtServer.ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                    break;
                case (int)StatesMachine.Values:
                    Console.WriteLine(@"Получено строк: " + (table as DataTable).Rows.Count);
                    if (TableResults == null)
                    {
                        TableResults = new DataTable();
                    }
                    else
                        ;

                    int iPrev = -1, iDupl = -1, iAdd = -1, iCur = -1;
                    iPrev = 0; iDupl = 0; iAdd = 0; iCur = 0;
                    iPrev = TableResults.Rows.Count;

                    //if (results.Rows.Count == 0)
                    //{
                    //    results = table.Copy ();
                    //}
                    //else
                    //    ;

                    //Удалить из таблицы записи, метки времени в которых, совпадают с метками времени в таблице-рез-те предыдущего опроса
                    iDupl = clearDupValues (ref table);

                    //Сформировать таблицу с "новыми" данными
                    DataTable tableIns = getTableIns (ref table);
                    tableIns.Columns.Add (@"tmdelta", typeof (int));

                    //foreach (DataRow r in tableIns.Rows)
                    //{
                    //    Console.WriteLine (@"ID=" + r[@"ID"] + @", DATETIME=" + ((DateTime)r[@"DATETIME"]).ToString (@"dd.MM.yyyy HH:mm:ss.fff"));
                    //}

                    //table.Columns.Add(@"tmdelta", Type.GetType("Int32"));

                    iAdd = table.Rows.Count;
                    TableResults.Merge(table);
                    iCur = TableResults.Rows.Count;
                    Console.WriteLine(@"Объединение таблицы-рез-та: [было=" + iPrev + @", дублирущих= " + iDupl + @", добавлено=" + iAdd + @", стало=" + iCur + @"]");
                    //DataTable tableChanged = results.GetChanges();
                    //if (! (tableChanged == null))
                    //    Console.WriteLine(@"Изменено строк: " + tableChanged.Rows.Count);
                    //else
                    //    Console.WriteLine(@"Изменено строк: " + 0);

                    (_iPlugin as HHPlugIn).DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.TABLE_RES, 0, table });
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, int request, int result)
        {
            string unknownErr = @"Неизвестная ошибка"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получения даты/времени сервера-источника";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"получения значений источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals (unknownErr) == false)
                msgErr = @"Ошибка " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);

            (_iPlugin as HHPlugIn).DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.ERROR, 0, state, msgErr });
        }

        protected override void StateWarnings(int state, int request, int result)
        {
            string unknownErr = @"Неизвестное предупреждение"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получении даты/времени сервера-источника";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"получении значений источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals(unknownErr) == false)
                msgErr = @"Предупреждение при " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);
        }
    }

    public class PlugIn : HHPlugIn
    {
        //private Dictionary <int, HMark> m_dictMarkDataHost;

        public PlugIn()
            : base()
        {
            _Id = 1001;

            createObject(typeof(HBiyskTMOra));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            HBiyskTMOra target = _object as HBiyskTMOra;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.INIT_CONN_SETT:
                    ConnectionSettings connSett = ev.par [0] as ConnectionSettings;
                    target.Initialize(new ConnectionSettings (
                        connSett.name
                        , connSett.server
                        , connSett.port
                        , connSett.dbName
                        , connSett.userName
                        , connSett.password
                    ));
                    break;
                case (int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP:
                    target.Initialize((int)(ev.par as object[])[0], (ev.par as object[])[1] as object []);
                    break;
                case (int)ID_DATA_ASKED_HOST.START:
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT) == true)
                    {
                        target.Start();
                        target.Activate(true);
                    }
                    else
                        DataAskedHost((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT);
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Activate(false);
                    target.Stop();
                    break;
                default:
                    break;
            }
            
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
