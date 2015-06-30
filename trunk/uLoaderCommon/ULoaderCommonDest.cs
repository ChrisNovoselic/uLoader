using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

using HClassLibrary;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderDest : HHandlerDbULoader
    {
        public string m_strNameTable;

        enum StatesMachine
        {
            Unknown = -1
            ,
            CurrentTime
                ,
            Values
                , Insert
        }

        public HHandlerDbULoaderDest()
            : base()
        {
        }

        public HHandlerDbULoaderDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            m_strNameTable = m_dictAdding[@"NAME_TABLE"];

            return iRes;
        }

        public abstract class GroupSignalsDest : GroupSignals
        {
            public enum INDEX_DATATABLE_RES
            {
                PREVIOUS,
                CURRENT
                    , COUNT_INDEX_DATATABLE_RES
            }

            protected class SIGNALDest : GroupSignals.SIGNAL
            {
                public int m_idLink;

                public SIGNALDest(int idMain, int idLink)
                    : base(idMain)
                {
                    this.m_idLink = idLink;
                }
            }

            protected int dequeue()
            {
                int iRes = 0
                    , cntPrev = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count
                    , cntCur = cntPrev;

                //Проверка признака сравнения текущей и предыдущих таблиц + наличия очереди перед вызовом
                lock (this)
                {
                    if (m_queueTableRec.Count > 0)
                    {
                        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] = m_queueTableRec.Dequeue();
                        cntCur = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count;
                    }
                    else
                        ;
                }

                //Пустую таблицу НЕ копировать, чтобы предотвратить потерю информацию в предыдущей
                // , пустая таблица - признак перехода через границу интервала опроса
                if (m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count > 0)
                    m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                else
                    ;

                string msg = @"HHandlerDbULoaderDest.GroupSignalsDest::dequeue () - DEQUEUE!"
                        + @" [ID=" + ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInBase)._Id + @", key=" + (_parent as HHandlerDbULoaderDest).m_IdGroupSignalsCurrent
                        + @"] queue.Count=" + m_queueTableRec.Count
                        + @", строк_было=" + cntPrev
                        + @", строк_стало=" + cntCur
                        ;
                        
                Console.WriteLine(msg);
                Logging.Logg().Debug(msg + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

                return iRes;
            }

            /// <summary>
            /// х таблиц результата от источника
            /// </summary>
            private Queue<DataTable> m_queueTableRec;
            /// <summary>
            /// Признак наличия необработанных таблиц результата от источника
            /// </summary>
            public bool IsQueue { get { return m_queueTableRec.Count > 1; } }
            private DataTable[] m_arTableRec;
            //private DataTable m_tableRecPrev;
            public override DataTable TableRecieved
            {
                get
                {
                    lock (this)
                    {
                        return
                            m_queueTableRec.Count > 0 ? m_queueTableRec.Peek() : m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT];
                            ;
                    }
                }

                set
                {
                    lock (this)
                    {                        
                        //Добавить элемент в очередь
                        m_queueTableRec.Enqueue (value);
                    }

                    //Logging.Logg().Debug(@"HHandlerDbULoaderDest::TableRecieved.set - " + @"ENQUEUE!"
                    //    + @" [ID=" + ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInBase)._Id + @", key=" + (_parent as HHandlerDbULoaderDest).m_IdGroupSignalsCurrent
                    //    + @"] queue.Count=" + m_queueTableRec.Count
                    //    + @"..."
                    //    , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Таблица с результатами - предыдущая (обработанная, для использования в вызове 'GetInsertQuery')
            /// </summary>
            protected DataTable TableRecievedPrev { get { return m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS]; } }

            /// <summary>
            /// Состояние группы сигналов
            /// </summary>
            public override STATE State
            {
                get { return base.State; }

                set
                {
                    base.State = value;

                    //При установке состояния 'STOP' - очистить (сбросить) очередь
                    if ((value == STATE.STOP)
                        || (value == STATE.UNKNOWN))
                    {                        
                        m_queueTableRec.Clear();
                    }
                    else
                        ;
                }
            }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="parent">Объект-владелец (для последующего обращения к его членам-данным)</param>
            /// <param name="pars">Параметры группы сигналов</param>
            public GroupSignalsDest(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
                ////Вариант - очередь
                //m_queueTableRec = new Queue<DataTable> ();
                //Вариант - массив
                m_arTableRec = new DataTable [(int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES];
                for (INDEX_DATATABLE_RES indx = INDEX_DATATABLE_RES.PREVIOUS; indx < INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES; indx ++)
                    m_arTableRec [(int)indx] = new DataTable();

                m_queueTableRec = new Queue<DataTable>();
                //m_stackTableRec = new Stack<DataTable>();
            }
            /// <summary>
            /// Получить таблицу для вставки значений в целевую БД
            /// </summary>
            /// <param name="table">Ссылка на таблицу</param>
            /// <returns>Таблица для вставки</returns>
            protected abstract DataTable getTableIns(ref DataTable table);
            /// <summary>
            /// Получить идентификатор
            /// </summary>
            /// <param name="idLink">Идентификатор, устанавливающий связь</param>
            /// <returns>Идентификатор для поля в таблице для вставки</returns>
            protected abstract object getIdToInsert(int idLink);
            /// <summary>
            /// Получить строку с запросом на вставку значений
            /// </summary>
            /// <param name="tblRes">Таблица, использующуюся для формирования строки запроса</param>
            /// <returns>Строка с запросом на вставку значений</returns>
            protected abstract string getInsertValuesQuery(DataTable tblRes);
            /// <summary>
            /// Получить строку с запросом на вставку значений
            /// </summary>
            /// <returns>Строка с запросом на вставку значений</returns>
            public string GetInsertValuesQuery()
            {
                string strRes = string.Empty;

                DataTable tblRes = getTableRes();

                if (!(tblRes == null))
                {
                    if (tblRes.Rows.Count > 0)
                        strRes = getInsertValuesQuery(tblRes);
                    else
                        ;

                    Logging.Logg().Debug(@"Строк для вставки [ID=" + ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInBase)._Id + @", key=" + (_parent as HHandlerDbULoaderDest).m_IdGroupSignalsCurrent + @"]: " + tblRes.Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    ;

                return
                    //string.Empty
                    strRes
                    ;
            }

            protected abstract DataTable getTableRes();
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
                        throw new Exception(@"HHandlerDbULoaderDest::StateRequest () - state=" + state.ToString() + @"...");
                    break;
                case StatesMachine.Values:
                    break;
                case StatesMachine.Insert:
                    string query = (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsDest).GetInsertValuesQuery();

                    //Logging.Logg().Debug(@"HHandlerDbULoaderDest:StateRequest () ::" + ((StatesMachine)state).ToString() + @" - "
                    //        + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                    //        + @"query=" + query + @"..."
                    //        , Logging.INDEX_MESSAGE.NOT_SET);

                    if (query.Equals(string.Empty) == false)
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
                    //string msg = @"statidsql::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
                    //    + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                    //    + @"DATETIME=" + m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff") + @"...";
                    //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                    //Console.WriteLine (msg);
                    break;
                case StatesMachine.Values:
                    Logging.Logg().Action(@"statidsql::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
                        + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] "
                        + @"Ok ...", Logging.INDEX_MESSAGE.NOT_SET);
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
            Logging.Logg().Error(@"HHandlerDbULoaderDest::StateErrors (state=" + ((StatesMachine)state).ToString() + @", req=" + req + @", res=" + res + @") - "
                + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]"
                + @"..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(@"HHandlerDbULoaderDest::StateWarnings (state" + ((StatesMachine)state).ToString() + @", req=" + req + @", res=" + res + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Постановка в очередь обработки событий вставку записей из таблицы
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов</param>
        /// <param name="tableIn">Таблица, содержащая записи для вставки</param>
        /// <param name="pars">Массив допю/параметров</param>
        /// <returns>Результат постановки в очередьь обработки событий</returns>
        public virtual int Insert(int id, DataTable tableIn, object []pars)
        {
            int iRes = 0;
            //string msg = string.Empty;

            lock (m_lockStateGroupSignals)
            {
                if ((! (m_dictGroupSignals == null))
                    && (m_dictGroupSignals.Keys.Contains(id) == true)
                    && (m_dictGroupSignals[id].IsStarted == true))
                {
                    m_dictGroupSignals[id].TableRecieved = tableIn.Copy();

                    push(id);
                    //msg = @"PUSH";

                    if ((m_dictGroupSignals[id] as GroupSignalsDest).IsQueue == true)
                    {
                        push(id);
                        //msg += @"..PUSH!";
                    }
                    else
                        ;
                }
                else
                    ;
            }

            //Logging.Logg().Debug(@"HHandlerDbULoaderDest::Insert () - " + msg + @" ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + id + @", от [ID_SOURCE=" + pars[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);

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
    }

    public abstract class HHandlerDbULoaderStatTMDest : HHandlerDbULoaderDest
    {
        public string m_strIdTEC;

        public HHandlerDbULoaderStatTMDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            m_strIdTEC = m_dictAdding[@"ID_TEC"];

            return iRes;
        }

        public abstract class GroupSignalsStatTMDest : GroupSignalsDest
        {
            public GroupSignalsStatTMDest(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }            

            protected override DataTable getTableRes()
            {
                DataTable tblRec
                    , tblDiff
                    , tblRes = new DataTable ();

                lock (this)
                {
                    tblRec = TableRecieved.Copy();
                }

                tblDiff = clearDupValues(TableRecievedPrev.Copy(), tblRec);
                tblRes = getTableIns(ref tblDiff);

                dequeue();

                return tblRes;
            }

            protected override DataTable getTableIns(ref DataTable table)
            {
                return table;
            }

            protected class TableInsTMDelta : object
            {
                private DataTable m_tblPrevRecieved;

                public DataTable Result(DataTable tblCur, DataTable tblPrev, GroupSignalsDest.SIGNAL[] arSignals)
                {
                    DataTable tableRes = new DataTable();
                    DataRow[] arSelIns = null;
                    DataRow rowCur = null
                        , rowAdd
                        , rowPrev = null;
                    int idSgnl = -1
                        , tmDelta = -1;
                    bool bConsoleDebug = false;

                    m_tblPrevRecieved = tblPrev.Copy();

                    if ((tblCur.Columns.Count > 2)
                        && ((!(tblCur.Columns.IndexOf(@"ID") < 0)) && (!(tblCur.Columns.IndexOf(@"DATETIME") < 0))))
                    {
                        tblCur.Columns.Add(@"tmdelta", typeof(int));
                        tableRes = tblCur.Clone();

                        for (int s = 0; s < arSignals.Length; s++)
                        {
                            try
                            {
                                idSgnl = (arSignals[s] as HHandlerDbULoaderDest.GroupSignalsDest.SIGNALDest).m_idLink;

                                //arSelIns = (tblCur as DataTable).Select(string.Empty, @"ID, DATETIME");
                                arSelIns = (tblCur as DataTable).Select(@"ID=" + idSgnl, @"DATETIME");
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
                                        tableRes.ImportRow(arSelIns[i]);
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
                                                if (col.ColumnName.Equals(@"tmdelta") == true)
                                                    //Для "нового" столбца - найденное значение
                                                    rowAdd[col.ColumnName] = tmDelta;
                                                else
                                                    //"Старые" значения
                                                    rowAdd[col.ColumnName] = rowPrev[col.ColumnName];
                                            }

                                            if (bConsoleDebug == true)
                                                Console.WriteLine(@"Установлен для ID=" + idSgnl + @", DATETIME=" + ((DateTime)rowAdd[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + rowAdd[@"tmdelta"]);
                                            else
                                                ;
                                        }
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        //Определить смещение "соседних" значений сигнала
                                        long iTMDelta = (((DateTime)arSelIns[i][@"DATETIME"]).Ticks - ((DateTime)arSelIns[i - 1][@"DATETIME"]).Ticks) / TimeSpan.TicksPerMillisecond;
                                        rowPrev[@"tmdelta"] = (int)iTMDelta;
                                        if (bConsoleDebug == true)
                                            Console.WriteLine(@", tmdelta=" + rowPrev[@"tmdelta"]);
                                        else
                                            ;
                                    }

                                    if (bConsoleDebug == true)
                                        if (!(rowCur == null))
                                            Console.Write(@"ID=" + rowCur[@"ID"] + @", DATETIME=" + ((DateTime)rowCur[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                                        else
                                            Console.Write(@"ID=" + arSelIns[i][@"ID"] + @", DATETIME=" + ((DateTime)arSelIns[i][@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                                    else
                                        ;

                                    rowPrev = rowCur;
                                }
                            else
                                ; //arSelIns == null

                            //Корректировать вывод
                            if (bConsoleDebug == true)
                                if (arSelIns.Length > 0)
                                    Console.WriteLine();
                                else ;
                            else
                                ;
                        } //Цикл по сигналам...
                    }
                    else
                        ; //Отсутствуют необходимые столбцы (т.е. у таблицы нет структуры)

                    tableRes.AcceptChanges();

                    return tableRes;
                }

                private DataRow setTMDelta(int id, DateTime dtCurrent, out int tmDelta)
                {
                    tmDelta = -1;
                    DataRow rowRes = null;
                    DataRow[] arSelWas = null;

                    //Проверить наличие столбцов в результ./таблице (признак получения рез-та)
                    if (m_tblPrevRecieved.Columns.Count > 0)
                    {//Только при наличии результата
                        arSelWas = m_tblPrevRecieved.Select(@"ID=" + id, @"DATETIME DESC");
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
        }
    }

    public abstract class HHandlerDbULoaderStatTMMSTDest : HHandlerDbULoaderStatTMDest
    {
        public HHandlerDbULoaderStatTMMSTDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMMSTDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        public abstract class GroupSignalsStatTMMSTDest : GroupSignalsStatTMDest
        {
            public GroupSignalsStatTMMSTDest(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_SRC_SGNL
                return new SIGNALDest((int)objs[0], (int)objs[1]);
            }

            protected override object getIdToInsert(int idLink)
            {
                int iRes = -1;

                foreach (SIGNALDest sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        iRes = 0;

                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                    throw new Exception(@"GroupSignlasStatTMIDDest::getIdToInsert (idLink=" + idLink + @") - ...");
                else
                    ;

                return iRes;
            }
        }
    }

    public abstract class HHandlerDbULoaderStatTMIDDest : HHandlerDbULoaderStatTMDest
    {
        public HHandlerDbULoaderStatTMIDDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMIDDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        protected abstract class GroupSignalsStatTMIDDest : GroupSignalsStatTMDest
        {
            public GroupSignalsStatTMIDDest(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected class SIGNALStatIDsql : GroupSignalsDest.SIGNALDest
            {
                public int m_idStat;

                public SIGNALStatIDsql(int idMain, int idLink, int idStat)
                    : base(idMain, idLink)
                {
                    m_idStat = idStat;
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_SRC_SGNL, ID_STAT
                return new SIGNALStatIDsql((int)objs[0], (int)objs[1], (int)objs[3]);
            }

            protected override object getIdToInsert(int idLink)
            {
                int iRes = -1;

                foreach (SIGNALStatIDsql sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        iRes = sgnl.m_idStat;

                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                    throw new Exception(@"GroupSignlasStatTMIDDest::getIdToInsert (idLink=" + idLink + @") - ...");
                else
                    ;

                return iRes;
            }
        }
    }

    public abstract class HHandlerDbULoaderStatTMKKSNAMEDest : HHandlerDbULoaderStatTMDest
    {
        public string m_strIdSource
            , m_strIdSrvTM;

        public HHandlerDbULoaderStatTMKKSNAMEDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMKKSNAMEDest(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private string getIdSrvTM(int id)
        {
            return ((id % 10) - 1).ToString();
        }
        
        //public override int Initialize(object[] pars)
        //{
        //    int iRes = base.Initialize(pars);

        //    m_strIdSrvTM = 

        //    return iRes;
        //}

        public override int Insert(int id, DataTable tableIn, object[] pars)
        {
            int iRes = base.Insert(id, tableIn, pars);

            m_strIdSource = ((int)pars[0]).ToString();
            m_strIdSrvTM = getIdSrvTM((int)pars[0]);

            return iRes;
        }

        protected abstract class GroupSignalsStatTMKKSNAMEDest : GroupSignalsStatTMDest
        {
            public GroupSignalsStatTMKKSNAMEDest(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            protected class SIGNALStatKKSNAMEsql : GroupSignalsDest.SIGNALDest
            {
                public string m_strStatKKSName;

                public SIGNALStatKKSNAMEsql(int idMain, int idLink, string statKKSName)
                    : base(idMain, idLink)
                {
                    m_strStatKKSName = statKKSName;
                }
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_SRC_SGNL, KKSNAME_STAT
                return new SIGNALStatKKSNAMEsql((int)objs[0], (int)objs[1], (string)objs[4]);
            }

            protected override object getIdToInsert(int idLink)
            {
                string strRes = string.Empty;

                foreach (SIGNALStatKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        strRes = sgnl.m_strStatKKSName;

                        break;
                    }
                    else
                        ;

                if (strRes.Equals (string.Empty) == true)
                    throw new Exception(@"GroupSignlasStatTMKKSNAMEDest::getIdToInsert (idLink=" + idLink + @") - ...");
                else
                    ;

                return strRes;
            }
        }
    }

    public class PlugInULoaderDest : PlugInULoader
    {
        public PlugInULoaderDest()
            : base()
        {
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            HHandlerDbULoaderDest target = _object as HHandlerDbULoaderDest;

            switch (ev.id)
            {
                case (int)ID_DATA_ASKED_HOST.TO_INSERT:
                    target.Insert((int)(ev.par as object[])[0], (ev.par as object[])[1] as DataTable, (ev.par as object[])[2] as object[]);
                    break;
                default:
                    break;
            }

            base.OnEvtDataRecievedHost(obj);
        }
    }

}
