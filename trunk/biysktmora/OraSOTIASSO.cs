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

        private static int SEC_INTERVAL_DEFAULT = 60;
        
        protected struct SIGNAL
        {
            public int m_id;
            public string m_NameTable;
            public SIGNAL (int id, string table)
            {
                this.m_id = id;
                this.m_NameTable = table;
            }
        }
        protected SIGNAL [] m_arSignals;

        enum StatesMachine {
            CurrentTime
            , Values
        }

        private DateTime m_dtStart
            , m_dtServer;
        private TimeSpan m_tmSpanPeriod;
        private long m_secInterval;
        public DataTable m_tableResults;
        public ConnectionSettings m_connSett;
        public string m_strQuery = string.Empty;

        public HBiyskTMOra()
        {
            m_dtStart =
            m_dtServer =
                DateTime.MinValue;

            m_tmSpanPeriod = new TimeSpan ((long)(SEC_INTERVAL_DEFAULT * Math.Pow (10, 7)));

            //Инициализировать массив сигналов
            m_arSignals = new SIGNAL[] { /*new SIGNAL(20049, @"TAG_000049")
                                        , new SIGNAL(20002, @"TAG_000047")
                                        , new SIGNAL(20003, @"TAG_000048")
                                        , new SIGNAL(20004, @"TAG_000049")
                                        , new SIGNAL(20005, @"TAG_000050")
                                        , new SIGNAL(20006, @"TAG_000051")
                                        , new SIGNAL(20007, @"TAG_000052")
                                        , new SIGNAL(20008, @"TAG_000053")*/
                                    };
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

        public int Initialize(object [] pars)
        {
            int iRes = 0;

            if (pars.Length > 0)
            {
                m_arSignals = new SIGNAL[pars.Length];

                for (int i = 0; i < pars.Length; i ++)
                {
                    m_arSignals[i] = new SIGNAL((int)(pars[i] as object[])[0], (pars[i] as object[])[1] as string);
                }
            }
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
                    (int)m_tmSpanPeriod.TotalSeconds
                    ;
            }
            else
                ;

            string strUnion = @" UNION "
                //Строки для условия "по дате/времени"
                , strStart = dtStart.ToString(@"yyyyMMdd HHmmss")
                , strEnd = dtStart.AddSeconds(secInterval).ToString(@"yyyyMMdd HHmmss");
            //Формировать зпрос
            foreach (SIGNAL s in m_arSignals)
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
            m_dictIdListeners.Add (0, new int [] { 0 });
            register(0, 0, m_connSett, string.Empty);
        }

        public override void Start()
        {
            StartDbInterfaces ();

            base.Start();
        }

        public override void ClearValues()
        {
            int iPrev = 0, iDel = 0, iCur = 0;
            if (! (m_tableResults == null))
            {
                iPrev = m_tableResults.Rows.Count;
                string strSel =
                    @"DATETIME<'" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' OR DATETIME>='" + m_dtStart.AddSeconds(m_tmSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    ;

                DataRow[] rowsDel = null;
                try { rowsDel = m_tableResults.Select(strSel); }
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
                            m_tableResults.Rows.Remove(r);

                        m_tableResults.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;

                iCur = m_tableResults.Rows.Count;

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
            if (m_tableResults.Columns.Count > 0)
            {//Только при наличии результата
                arSelWas = m_tableResults.Select(@"ID=" + id, @"DATETIME DESC");
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
            foreach (DataRow rRes in m_tableResults.Rows)
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

            for (int s = 0; s < m_arSignals.Length; s ++)
            {
                try
                {
                    //arSelIns = (table as DataTable).Select(string.Empty, @"ID, DATETIME");
                    arSelIns = (table as DataTable).Select(@"ID=" + m_arSignals[s].m_id, @"DATETIME");
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
                            setTMDelta(m_arSignals[s].m_id, (DateTime)arSelIns[i][@"DATETIME"]);
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

            if (m_dtStart == DateTime.MinValue)
            {
                m_dtStart = m_dtServer;
                m_dtStart = m_dtStart.AddSeconds(-1 * m_dtStart.Second);
            }
            else
                ;

            if ((m_dtServer - m_dtStart.AddSeconds (m_tmSpanPeriod.TotalSeconds)).TotalSeconds > 6)
                m_dtStart = m_dtStart.AddSeconds(m_tmSpanPeriod.TotalSeconds);
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
                    setQuery (m_dtStart);
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
                    if (m_tableResults == null)
                    {
                        m_tableResults = new DataTable();
                    }
                    else
                        ;

                    int iPrev = -1, iDupl = -1, iAdd = -1, iCur = -1;
                    iPrev = 0; iDupl = 0; iAdd = 0; iCur = 0;
                    iPrev = m_tableResults.Rows.Count;

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
                    m_tableResults.Merge(table);
                    iCur = m_tableResults.Rows.Count;
                    Console.WriteLine(@"Объединение таблицы-рез-та: [было=" + iPrev + @", дублирущих= " + iDupl + @", добавлено=" + iAdd + @", стало=" + iCur + @"]");
                    //DataTable tableChanged = results.GetChanges();
                    //if (! (tableChanged == null))
                    //    Console.WriteLine(@"Изменено строк: " + tableChanged.Rows.Count);
                    //else
                    //    Console.WriteLine(@"Изменено строк: " + 0);
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
                    target.Initialize(ev.par as object []);
                    break;
                case (int)ID_DATA_ASKED_HOST.START:
                    if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT) == true)
                        if (m_markDataHost.IsMarked((int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP) == true)
                            target.Start();
                        else
                            DataAskedHost((int)ID_DATA_ASKED_HOST.INIT_SIGNALS_OF_GROUP);
                    else
                        DataAskedHost((int)ID_DATA_ASKED_HOST.INIT_CONN_SETT);
                    break;
                case (int)ID_DATA_ASKED_HOST.STOP:
                    target.Stop();
                    break;
                default:
                    break;
            }
            
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
