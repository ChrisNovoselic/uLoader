﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

////using HClassLibrary;
using ASUTP;
using ASUTP.Database;
using ASUTP.PlugIn;
using ASUTP.Helper;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderDest : HHandlerDbULoader, ILoader
    {
        private static string[] s_arDictAddingKeys = { @"NAME_TABLE" };
        
        private const int MAX_QUEUECOUNT_OF_KEY = 16;

        public string m_strNameTable;

        enum StatesMachine
        {
            Unknown = -1
            , CurrentTime
            , Values
            , Insert
        }

        public HHandlerDbULoaderDest()
            : base()
        {
        }

        public HHandlerDbULoaderDest(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            foreach (string key in s_arDictAddingKeys)
                if (m_dictAdding.ContainsKey(key) == true)
                    switch (key)
                    {
                        case @"NAME_TABLE":
                            m_strNameTable = m_dictAdding[key];
                            break;
                        default:
                            iRes = 1; // необрабатываемый параметр
                            break;
                    }                    
                else
                    switch (key)
                    {
                        case @"NAME_TABLE":
                            iRes = -1;
                            break;
                        default:
                            break;
                    }

            return iRes;
        }

        public abstract class GroupSignalsDest : GroupSignals
        {
            protected MODE_WORK m_modeSource;
            protected int m_IdSourceConnSett
                , m_IdSourceTEC;
            
            public enum INDEX_DATATABLE_RES
            {
                PREVIOUS,
                CURRENT
                    , COUNT_INDEX_DATATABLE_RES
            }
            /// <summary>
            /// Класс сигнала в составе группы сигналов группы источников(назначение)
            /// </summary>
            protected class SIGNALDest : GroupSignals.SIGNAL
            {
                /// <summary>
                /// Идентификатор(локальный) сигнала в группе сигналов группы источников(источник)
                /// </summary>
                public int m_idLink;

                public SIGNALDest(GroupSignals parent, int idMain, int idLink)
                    : base(parent, idMain, string.Empty) // 2-й параметр может указывать на формулу, но в 'Dest' формулы пока не предусмотрены
                {
                    this.m_idLink = idLink;
                }
            }

            protected class SIGNALIDsql : GroupSignalsDest.SIGNALDest
            {
                public int m_idTarget;

                public SIGNALIDsql(GroupSignals parent, int idMain, int idLink, int idTarget)
                    : base(parent, idMain, idLink)
                {
                    m_idTarget = idTarget;
                }
            }

            protected class SIGNALStatKKSNAMEsql : GroupSignalsDest.SIGNALDest
            {
                public string m_strStatKKSName;

                public SIGNALStatKKSNAMEsql(GroupSignals parent, int idMain, int idLink, string statKKSName)
                    : base(parent, idMain, idLink)
                {
                    m_strStatKKSName = statKKSName;
                }
            }

            public void InitSource(params object []pars)
            {
                m_modeSource = (MODE_WORK)pars[0];
                m_IdSourceConnSett = (int)pars[1];
                m_IdSourceTEC = (int)pars[2];
            }

            protected static string s_strFormatDbDateTime/*ToInsert*/ = @"yyyyMMdd HH:mm:ss.fffffff"
                //, s_strFormatDbDateTimeToSelect = @"yyyyMMdd HH:mm:ss"
                ;

            public int Dequeue()
            {
                int iRes = -1
                    , cntPrev = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] == null ? -1 : m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count
                    , cntCur = cntPrev;

                //Проверка признака сравнения текущей и предыдущих таблиц + наличия очереди перед вызовом
                lock (this)
                {
                    if (m_queueTableRec.Count > 0)
                    {
                        m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] = m_queueTableRec.Dequeue();
                        if (!(m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT] == null))
                        {
                            cntCur = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count;

                            iRes = 0;
                        }
                        else
                            ;
                    }
                    else
                        ;
                }

                if (!(iRes < 0))
                    //Пустую таблицу НЕ копировать, чтобы предотвратить потерю информацию в предыдущей
                    // , пустая таблица - признак перехода через границу интервала опроса
                    if (m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Rows.Count > 0)
                        m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT].Copy();
                    else
                        iRes = 1;
                else
                    ;
                //Очистить предыдущую таблицу, чтобы при очередной итерации
                // опросить целевую таблицу на наличие значений
                // т.к. в режиме 'MODE_WORK.COSTUMIZE' интервалы времени не пересекаются
                // , и записи предыдущей таблицы становятся не актуальными
                if (m_modeSource == MODE_WORK.CUSTOMIZE)
                    Clear();
                else
                    ;

                string msg = @"HHandlerDbULoaderDest.GroupSignalsDest::dequeue () - DEQUEUE!"
                        + string.Format(@" [ID={0}:{1}, key={2}]"
                            , ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInBase)._Id
                            , ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInULoader).KeySingleton
                            , (_parent as HHandlerDbULoaderDest).IdGroupSignalsCurrent)
                        + @" queue.Count=" + (_parent as HHandlerDbULoaderDest).QueueCount
                        + @", queueTableRec.Count=" + m_queueTableRec.Count
                        + @", строк_было=" + cntPrev
                        + @", строк_стало=" + cntCur
                        ;

                //Console.WriteLine(msg);
                Logging.Logg().Debug(msg + @" ...", Logging.INDEX_MESSAGE.D_005);

                return iRes;
            }
            /// <summary>
            /// Очередь таблиц результата от источника
            ///  , очередная таблица при обработке попадает в 'm_arTableRec'
            /// </summary>
            private Queue<DataTable> m_queueTableRec;
            /// <summary>
            /// Количество таблиц со значениями в очереди для обработки
            /// </summary>
            public int QueueCountTableRec { get { return m_queueTableRec.Count; } }
            /// <summary>
            /// Массив с текущей и предыдущей таблицами полученными от источника
            /// </summary>
            private DataTable[] m_arTableRec;
            /// <summary>
            /// Текущая (крайняя полученная) таблица со значениями от источника
            /// </summary>
            public override DataTable TableRecieved
            {
                get
                {
                    lock (this)
                    {
                        return
                            m_queueTableRec.Count > 0 ?
                                m_queueTableRec.Peek() :
                                m_arTableRec[(int)INDEX_DATATABLE_RES.CURRENT]
                                ;
                    } 
                }

                set
                {
                    lock (this)
                    {
                        if (!(value == null))
                        {
                            //Добавить элемент в очередь
                            m_queueTableRec.Enqueue (value);
                        }
                        else
                            ;
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
            public DataTable TableRecievedPrev
            {
                get {
                    return m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS];
                }

                set {
                    m_arTableRec[(int)INDEX_DATATABLE_RES.PREVIOUS] = value;
                }
            }
            /// <summary>
            /// Объект для сравнения предыдущей и текущей таблиц
            /// </summary>
            protected DataTableDuplicate m_DupTables;
            ///// <summary>
            ///// Дата/время начала интервала (минимальное значение), за который получен набор значений для вставки в целевую таблицу
            ///// </summary>
            //public DateTimeRange DateTimeRangeRecieved;
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
            public GroupSignalsDest(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                ////Вариант - очередь
                //m_queueTableRec = new Queue<DataTable> ();
                //Вариант - массив
                m_arTableRec = new DataTable [(int)INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES];
                for (INDEX_DATATABLE_RES indx = INDEX_DATATABLE_RES.PREVIOUS; indx < INDEX_DATATABLE_RES.COUNT_INDEX_DATATABLE_RES; indx ++)
                    m_arTableRec [(int)indx] = new DataTable();

                m_queueTableRec = new Queue<DataTable>();

                //DateTimeRangeRecieved = new DateTimeRange ();

                m_DupTables = createDupTables();
            }
            /// <summary>
            /// Возвратить идентификатор для вставки строки в целевую таблицу
            /// </summary>
            /// <param name="idLink">Идентификатор источника сигнала</param>
            /// <returns>Идентификатор сигнала в целевой таблице</returns>
            protected virtual object getIdTarget(int idLink)
            {
                int iRes = -1;

                foreach (SIGNALDest sgnl in m_arSignals)
                    if (sgnl.m_idLink == idLink)
                    {
                        iRes = sgnl.m_idMain;

                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                    Logging.Logg().Warning(@"GroupSignlasDest::getIdTarget (idLink=" + idLink + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;

                return iRes;
            }

            protected virtual object getIdLink(object idTarget)
            {
                int iRes = -1;

                foreach (SIGNALDest sgnl in m_arSignals)
                    if (sgnl.m_idMain == (int)idTarget)
                    {
                        iRes = sgnl.m_idLink;

                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                    Logging.Logg().Warning(@"GroupSignlasDest::getIdLink (idTarget=" + (int)idTarget + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;

                return iRes;
            }

            //protected virtual void setTableRes()
            //{
            //    //Сравнить (удалить дублирующие записи) предыдущую и текущую таблицы
            //    m_DupTables.Clear(TableRecievedPrev, TableRecieved);
            //}
            protected abstract void setTableRes();
            /// <summary>
            /// Получить строку с запросом на вставку значений
            /// </summary>
            /// <param name="tblRes">Таблица, использующуюся для формирования строки запроса</param>
            /// <returns>Строка с запросом на вставку значений</returns>
            protected abstract string getTargetValuesQuery();
            /// <summary>
            /// Получить строку с запросом текущих записей в целевой таблице
            /// </summary>
            /// <returns>Строка с запросом</returns>
            protected abstract string getExistsValuesQuery();
            /// <summary>
            /// Строка с запросом на вставку значений
            /// </summary>
            /// <returns>Строка с запросом на вставку значений</returns>
            public string TargetValuesQuery
            {
                get
                {
                    string strRes = string.Empty
                        , strIds = string.Format(@"[ID={0}:{1}, key={2}]"
                            , ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInBase)._Id
                            , ((_parent as HHandlerDbULoaderDest)._iPlugin as PlugInULoader).KeySingleton
                            , (_parent as HHandlerDbULoaderDest).IdGroupSignalsCurrent);

                    try
                    {
                        lock (this) {
                            setTableRes();
                        }

                        if (m_DupTables.IsDeterminate == true)
                            strRes = getTargetValuesQuery();
                        else
                            ;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"HHandlerDbULoaderDest::GroupSignlsDest () - " + strIds + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    Logging.Logg().Debug(string.Format(@"Строк для вставки {0}: {1}..."
                            , strIds
                            , (((!(m_DupTables == null)) && (!(m_DupTables.TableDistinct == null))) ? m_DupTables.TableDistinct.Rows.Count.ToString() : @"не известно"))
                        , Logging.INDEX_MESSAGE.D_006);

                    return
                        //string.Empty
                        strRes
                        ;
                    }
            }

            public string ExistsValuesQuery
            {
                get
                {
                    string strRes = string.Empty;

                    strRes = getExistsValuesQuery();

                    return strRes;
                }
            }

            public override void Stop()
            {
                //Установить признак 
                TableRecievedPrev.Rows.Clear();
                //DateTimeRangeRecieved.Clear ();

                base.Stop();
            }

            public void Clear ()
            {
                TableRecievedPrev.Rows.Clear();
            }

            protected virtual DataTableDuplicate createDupTables()
            {
                return new DataTableDuplicate();
            }
            /// <summary>
            /// Заменить в таблице, переданной в качестве аргумента, значения поля [ID]
            /// </summary>
            /// <param name="tablePrev">Таблица в которой происходит замена значений поля [ID]</param>
            public void Convert(DataTable tablePrev)
            {
                try
                {
                    if (tablePrev.Columns.Count > 0)
                        if (!(tablePrev.Columns.IndexOf(@"ID") < 0))
                        {
                            foreach (DataRow r in tablePrev.Rows)
                                r[@"ID"] = getIdLink(r[@"ID"]);

                            TableRecievedPrev = tablePrev;
                        }
                        else
                        {
                            // отсутствует необходимое поле "ID"
                            Logging.Logg().Warning(@"HHandlerDbULoader.GroupSignals::Convert (IdGroupSgls=" + m_Id + @") - отсутствует необходимое поле [ID]...", Logging.INDEX_MESSAGE.NOT_SET);

                            string strFieldsNames = string.Empty;
                            foreach (DataColumn c in tablePrev.Columns)
                                strFieldsNames += c.ColumnName + @", ";

                            strFieldsNames = strFieldsNames.Substring(0, strFieldsNames.Length - 2);

                            Logging.Logg().Debug(@"Все столбцы в таблице: " + strFieldsNames, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                        ; //??? нет ни одного столбца
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"GroupsignalsDest::Convert (IdGroupSgnls=" + m_Id + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            string query = string.Empty
                , msg = string.Empty;
            StatesMachine stateMachine = StatesMachine.Unknown;

            try {
                msg = string.Format("HHandlerDbULoaderDest::StateRequest (state={3}) - [ID={0}:{1}, key={2}] - "
                    , _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent, (StatesMachine)state);

                if (!(IdGroupSignalsCurrent < 0)) {
                    stateMachine = (StatesMachine)state;

                    switch (stateMachine) {
                        case StatesMachine.CurrentTime:
                            query = GetCurrentTimeQuery (DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL);
                            break;
                        case StatesMachine.Values:
                            query = (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDest).ExistsValuesQuery;
                            break;
                        case StatesMachine.Insert:
                            query = (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDest).TargetValuesQuery;
                            break;
                        default:
                            break;
                    }

                    iRes = string.IsNullOrEmpty (query) == true ? 0 : 1;

                    Request (m_dictIdListeners [IdGroupSignalsCurrent] [0], query);

                    // детальная информация по запросу
                    Logging.Logg ().Debug (string.Format (@"{0} ...", string.Format (@"{0} query={1} ...", msg, m_datetimeServer.Equals (DateTime.MinValue) == false ? query : @"не известно тек./время сервера"))
                        , Logging.INDEX_MESSAGE.D_004);

                    if (isLastState (state) == true)
                        (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDest).Dequeue ();
                    else
                        ;
                } else
                    Logging.Logg().Warning (string.Format (@"{0} ...", msg), Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg().Exception(e
                    , string.Format(@"{0} ...", msg)
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;

            string msg = string.Empty;
            DataTable table;

            try {
                msg = string.Format("HHandlerDbULoaderDest::StateResponse (state={3}) - [ID={0}:{1}, key={2}] - "
                    , _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent, (StatesMachine)state);

                table  = (obj as DataTable);

                switch ((StatesMachine)state) {
                    case StatesMachine.CurrentTime:
                        if ((table.Rows.Count == 1)
                            && (table.Columns.Count == 1)) {
                            m_datetimeServer.Value = (DateTime)table.Rows [0] [0];
                            //msg = string.Format(@"{0} DATETIME={1} ...", msg, m_dtServer.ToString(@"dd.MM.yyyy HH.mm.ss.fff"));
                            //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                            //Console.WriteLine (msg);
                            m_datetimeServer.BaseUTCOffset = (DateTime)table.Rows [0] [0] - (DateTime)table.Rows [0] [1];
                        } else
                            ;
                        (obj as DataTable).Columns.Clear();
                        break;
                    case StatesMachine.Values:
                        // ??? обработать результат запроса на получение текущих значений
                        (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDest).Convert(table);
                        break;
                    case StatesMachine.Insert:
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e
                    , string.Format(@"{0} ...", msg)
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            HHandler.INDEX_WAITHANDLE_REASON resReason = INDEX_WAITHANDLE_REASON.SUCCESS;
            
            Logging.Logg().Error(@"HHandlerDbULoaderDest::StateErrors (state=" + ((StatesMachine)state).ToString() + @", req=" + req + @", res=" + res + @") - "
                + string.Format(@"[ID={0}:{1}, key={2}]", _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent)
                + @"..."
                , Logging.INDEX_MESSAGE.NOT_SET);

            return resReason;
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
        /// <param name="pars">Массив доп./параметров</param>
        /// <returns>Результат постановки в очередьь обработки событий</returns>
        public virtual int Insert(int id, DataTable tableIn, object []pars)
        {
            int iRes = 0
                , keyQueueCount = -1;
            ////string msg = string.Empty;

            lock (m_lockStateGroupSignals)
            {
                if ((! (m_dictGroupSignals == null))
                    && (m_dictGroupSignals.Keys.Contains(id) == true)
                    && (m_dictGroupSignals[id].IsStarted == true))
                {
                    //0 - режим работы, 1 - иднтификатор источника значений, 2 - идентификатор ТЭЦ (при наличии)
                    (m_dictGroupSignals[id] as GroupSignalsDest).InitSource(pars[0], pars[1], pars[2]);
                    m_dictGroupSignals[id].TableRecieved = tableIn.Copy();

                    keyQueueCount = push(id);
                    //msg = @"PUSH";

                    //if ((m_dictGroupSignals[id] as GroupSignalsDest).IsQueueTableRec == true)
                    if (keyQueueCount < (m_dictGroupSignals[id] as GroupSignalsDest).QueueCountTableRec)
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

            //Logging.Logg().Debug(@"HHandlerDbULoaderDest::Insert () - " + msg + @" " + PlugInId + @", key=" + id + @", от [ID_SOURCE=" + pars[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }
        /// <summary>
        /// Очистить усорвно-постоянные значения
        ///  при останове связанной группы сигналов источника
        /// </summary>
        /// <param name="id">Идентификатор группы сигналов назначения</param>
        public void Clear(int id)
        {
            (m_dictGroupSignals[id] as GroupSignalsDest).Clear ();
        }

        //protected GroupSignalsDest.DateTimeRange DateTimeRangeRecieved
        //{
        //    get
        //    {
        //        if (!(IdGroupSignalsCurrent < 0))
        //            return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDest).DateTimeRangeRecieved;
        //        else
        //            throw new Exception(@"ULoaderCommonDest::DateTimeRangeRecieved.get ...");
        //    }
        //}

        protected virtual DataTable TableRecievedPrev
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDest).TableRecievedPrev;
                else
                    throw new Exception(@"ULoaderCommonDest::TableRecievedPrev.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDest).TableRecievedPrev = value;
                else
                    throw new Exception(@"ULoaderCommonDest::TableRecievedPrev.get ...");
            }
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            if (TableRecievedPrev.Rows.Count == 0)
                AddState((int)StatesMachine.Values);
            else
                ;
            AddState((int)StatesMachine.Insert);

            return iRes;
        }

        protected override bool isPush(int curCount)
        {
            return curCount < MAX_QUEUECOUNT_OF_KEY;
        }
    }

    public abstract class HHandlerDbULoaderStatTMDest : HHandlerDbULoaderDest
    {
        //public string m_strIdTEC;

        public HHandlerDbULoaderStatTMDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMDest(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        //public override int Initialize(object[] pars)
        //{
        //    int iRes = base.Initialize(pars);

        //    m_strIdTEC = m_dictAdding[@"ID_TEC"];

        //    return iRes;
        //}

        public abstract class GroupSignalsStatTMDest : GroupSignalsDest
        {
            public GroupSignalsStatTMDest(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected class DataTableDuplicateTMDelta : DataTableDuplicate
            {
                private DataTable m_tblPrevRecieved;

                public DataTableDuplicateTMDelta()
                {
                }

                public void Convert(DataTable tblPrev, GroupSignalsDest.SIGNAL[] arSignals)
                {
                    DataTable tblCurrent;
                    List<string> listReqNameColumn = new List<string> () { @"ID", @"DATETIME" };

                    DataRow [] arSelIns = null;
                    DataRow rowCur = null
                        , rowAdd
                        , rowPrev = null;
                    int cntReqNameColumn = -1
                        , idSgnl = -1
                        , tmDelta = -1;
                    bool bConsoleDebug = false;

                    if (Equals (TableDistinct, null) == false) {
                        if (TableDistinct.Columns.Count > 2) {
                            // проверить наличие обязательных при обработке столбцов
                            cntReqNameColumn = 0;
                            listReqNameColumn.ForEach (reqName => {
                                cntReqNameColumn += TableDistinct.Columns.Contains (reqName) == true ? 1 : 0;
                            });

                            if (cntReqNameColumn == listReqNameColumn.Count) {
                                if (Equals (tblPrev, null) == false) {
                                    if ((Equals (arSignals, null) == false)
                                        && (arSignals.Length > 0)) {
                                        tblCurrent = TableDistinct.Copy ();
                                        TableDistinct = null;

                                        m_tblPrevRecieved = tblPrev.Copy ();

                                        if (tblCurrent.Columns.IndexOf (@"tmdelta") < 0)
                                            tblCurrent.Columns.Add (@"tmdelta", typeof (int));
                                        else
                                            ;
                                        TableDistinct = tblCurrent.Clone ();

                                        for (int s = 0; s < arSignals.Length; s++) {
                                            try {
                                                idSgnl = (arSignals [s] as HHandlerDbULoaderDest.GroupSignalsDest.SIGNALDest).m_idLink;

                                                //arSelIns = (tblCur as DataTable).Select(string.Empty, @"ID, DATETIME");
                                                arSelIns = (tblCurrent as DataTable).Select (@"ID=" + idSgnl, @"DATETIME");
                                            } catch (Exception e) {
                                                Logging.Logg ().Exception (e, @"statidsql::getTableIns () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                            }

                                            if (!(arSelIns == null))
                                                for (int i = 0; i < arSelIns.Length; i++) {
                                                    if (i < (arSelIns.Length - 1)) {
                                                        TableDistinct.ImportRow (arSelIns [i]);
                                                        rowCur = TableDistinct.Rows [TableDistinct.Rows.Count - 1];
                                                    } else
                                                        //Не вставлять без известной 'tmdelta'
                                                        rowCur = null;

                                                    //Проверитьт № итерации
                                                    if (i == 0) {//Только при прохождении 1-ой итерации цикла
                                                        tmDelta = -1;
                                                        //Определить 'tmdelta' для записи из предыдущего опроса
                                                        rowAdd = null;
                                                        rowPrev = setTMDelta (idSgnl, (DateTime)arSelIns [i] [@"DATETIME"], out tmDelta);

                                                        if ((!(rowPrev == null))
                                                            && (tmDelta > 0)) {
                                                            //Добавить из предыдущего опроса
                                                            rowAdd = TableDistinct.Rows.Add ();
                                                            //Скопировать все значения
                                                            foreach (DataColumn col in TableDistinct.Columns) {
                                                                if (col.ColumnName.Equals (@"tmdelta") == true)
                                                                    //Для "нового" столбца - найденное значение
                                                                    rowAdd [col.ColumnName] = tmDelta;
                                                                else
                                                                    //"Старые" значения
                                                                    rowAdd [col.ColumnName] = rowPrev [col.ColumnName];
                                                            }

                                                            if (bConsoleDebug == true)
                                                                //Console.WriteLine(@"Установлен для ID=" + idSgnl + @", DATETIME=" + ((DateTime)rowAdd[@"DATETIME"]).ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @" tmdelta=" + rowAdd[@"tmdelta"])
                                                                ;
                                                            else
                                                                ;
                                                        } else
                                                            ;
                                                    } else {
                                                        //Определить смещение "соседних" значений сигнала
                                                        long iTMDelta = (((DateTime)arSelIns [i] [@"DATETIME"]).Ticks - ((DateTime)arSelIns [i - 1] [@"DATETIME"]).Ticks) / TimeSpan.TicksPerMillisecond;
                                                        rowPrev [@"tmdelta"] = (int)iTMDelta;
                                                        if (bConsoleDebug == true)
                                                            //Console.WriteLine(@", tmdelta=" + rowPrev[@"tmdelta"])
                                                            ;
                                                        else
                                                            ;
                                                    }

                                                    if (bConsoleDebug == true)
                                                        if (!(rowCur == null))
                                                            Console.Write (@"ID=" + rowCur [@"ID"] + @", DATETIME=" + ((DateTime)rowCur [@"DATETIME"]).ToString (@"dd.MM.yyyy HH:mm:ss.fff"));
                                                        else
                                                            Console.Write (@"ID=" + arSelIns [i] [@"ID"] + @", DATETIME=" + ((DateTime)arSelIns [i] [@"DATETIME"]).ToString (@"dd.MM.yyyy HH:mm:ss.fff"));
                                                    else
                                                        ;

                                                    rowPrev = rowCur;
                                                } else
                                                //arSelIns == null
                                                    ;

                                            //Корректировать вывод
                                            if (bConsoleDebug == true)
                                                if (arSelIns.Length > 0)
                                                    Console.WriteLine ();
                                                else
                                                    ;
                                            else
                                                ;
                                        } //Цикл по сигналам...
                                    } else
                                        ;
                                } else
                                    Logging.Logg().Error(string.Format("Отсутствует таблица с предыдущими значенями (из аргумента)..."), Logging.INDEX_MESSAGE.NOT_SET);

                                TableDistinct.AcceptChanges ();
                            } else
                                Logging.Logg ().Error ($"Отсутствуют необходимые столбцы: {string.Join(",", TableDistinct.Columns.OfType<DataColumn>().Select(col => col.ColumnName).ToArray ())}..."
                                    , Logging.INDEX_MESSAGE.NOT_SET);
                        } else
                            Logging.Logg ().Error ($"Отсутствует структура в исходной/целевой таблицы, кл-во_столбцов={TableDistinct.Columns.Count})...", Logging.INDEX_MESSAGE.NOT_SET);
                    } else
                        ;
                }

                private DataRow setTMDelta(int id, DateTime dtCurrent, out int tmDelta)
                {
                    tmDelta = -1;
                    DataRow rowRes = null;
                    DataRow[] arSelWas = null;

                    //Проверить наличие столбцов в результ./таблице (признак получения рез-та)
                    if ((!(m_tblPrevRecieved.Columns.IndexOf(@"ID") < 0))
                        && (!(m_tblPrevRecieved.Columns.IndexOf(@"DATETIME") < 0)))
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

            protected override void setTableRes()
            {
                // 26.07.2017 пересено на вызов функции
                //lock (this) {
                    ////Заполнить таблицы с повторяющимися/уникальными записями
                    //base.setTableRes();
                    //Сравнить (удалить дублирующие записи) предыдущую и текущую таблицы
                    m_DupTables.Clear(TableRecievedPrev, TableRecieved);
                    //добаить поле [tmdelta]
                    (m_DupTables as DataTableDuplicateTMDelta).Convert(TableRecievedPrev, Signals);
                //}
            }

            protected override DataTableDuplicate createDupTables()
            {
                return new DataTableDuplicateTMDelta();
            }
        }
    }

    //public abstract class HHandlerDbULoaderStatTMMSTDest : HHandlerDbULoaderStatTMDest
    //{
    //    public HHandlerDbULoaderStatTMMSTDest()
    //        : base()
    //    {
    //    }

    //    public HHandlerDbULoaderStatTMMSTDest(PlugInULoader iPlugIn)
    //        : base(iPlugIn)
    //    {
    //    }

    //    public abstract class GroupSignalsStatTMMSTDest : GroupSignalsStatTMDest
    //    {
    //        public GroupSignalsStatTMMSTDest(HHandlerDbULoader parent, int id, object[] pars)
    //            : base(parent, id, pars)
    //        {
    //        }

    //        protected override GroupSignals.SIGNAL createSignal(object[] objs)
    //        {
    //            //ID_MAIN, ID_SRC_SGNL
    //            return new SIGNALDest((int)objs[0], (int)objs[1]);
    //        }            
    //    }
    //}

    public abstract class HHandlerDbULoaderIDDest : HHandlerDbULoaderDest
    {
        public HHandlerDbULoaderIDDest()
            : base()
        {
        }

        public HHandlerDbULoaderIDDest(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        protected abstract class GroupSignalsIDDest : GroupSignalsDest
        {
            public GroupSignalsIDDest(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_SRC_SGNL, ID_STAT
                return new SIGNALIDsql(this, (int)objs[0], (int)objs[1], (int)objs[3]);
            }

            protected override void setTableRes()
            {
                //Сравнить (удалить дублирующие записи) предыдущую и текущую таблицы
                m_DupTables.Clear(TableRecievedPrev, TableRecieved);
            }

            protected override object getIdTarget(int idLink)
            {
                List<int> listRes = new List<int> ();

                (m_arSignals).ToList ().ForEach(sgnl => { if ((sgnl as SIGNALIDsql).m_idLink == idLink) listRes.Add((sgnl as SIGNALIDsql).m_idTarget); else; });

                if (listRes.Count == 0)
                    Logging.Logg().Warning(string.Format(@"{0}::getIdTarget (idLink={1}) - ...", GetType().Name, idLink), Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;

                return listRes;
            }

            protected override object getIdLink(object idTarget)
            {
                int iRes = -1;

                foreach (SIGNALIDsql sgnl in m_arSignals)
                    if (sgnl.m_idTarget == (int)idTarget)
                    {
                        iRes = sgnl.m_idLink;

                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                    throw new Exception(@"GroupSignlasIDDest::getIdLink (idTarget=" + (int)idTarget + @") - ...");
                else
                    ;

                return iRes;
            }
        }
    }

    public abstract class HHandlerDbULoaderStatTMKKSNAMEDest : HHandlerDbULoaderStatTMDest
    {
        //public int m_IdSrvTM;

        public HHandlerDbULoaderStatTMKKSNAMEDest()
            : base()
        {
        }

        public HHandlerDbULoaderStatTMKKSNAMEDest(PlugInULoader iPlugIn)
            : base(iPlugIn)
        {
        }

        public int GetIdSrvTM(int id)
        {
            return ((id % 10) - 1);
        }
        
        //public override int Initialize(object[] pars)
        //{
        //    int iRes = base.Initialize(pars);

        //    m_IdSrvTM = getIdSrvTM(m_IdSourceTEC);

        //    return iRes;
        //}

        protected abstract class GroupSignalsStatTMKKSNAMEDest : GroupSignalsStatTMDest
        {
            public GroupSignalsStatTMKKSNAMEDest(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                int indxKKS_NAMEStat = -1;
                //ID_MAIN, ID_SRC_SGNL, KKSNAME_STAT (??? всегда крайнее)
                indxKKS_NAMEStat = objs.Length - 1;
                return new SIGNALStatKKSNAMEsql(this, (int)objs[0], (int)objs[1], (string)objs[indxKKS_NAMEStat]); //14.03.2016 было - 4
            }

            //protected override void setTableRes()
            //{
            //    //Заполнить таблицы с повторяющимися/уникальными записями
            //    base.setTableRes();
            //    //добавить поле [tmdelta]
            //    (m_DupTables as DataTableDuplicateTMDelta).Convert(TableRecievedPrev, Signals);
            //}

            protected override object getIdTarget(int idLink)
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

                //if (strRes.Equals (string.Empty) == true)
                //    Logging.Logg().Warning(@"GroupSignalsStatTMKKSNAMEDest::getIdTarget (id=" + (_parent as HHandlerDbULoaderStatTMKKSNAMEDest).PlugInId
                //        + @", key=" + m_Id
                //        + @", idLink=" + idLink + this.m_Id + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                //else
                //    ;

                return strRes;
            }

            protected override object getIdLink(object idTarget)
            {
                int iRes = -1;

                foreach (SIGNALStatKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.m_strStatKKSName == (string)idTarget)
                    {
                        iRes = sgnl.m_idLink;

                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                    throw new Exception(@"GroupSignlasStatTMKKSNAMEDest::getIdLink (idTarget=" + (string)idTarget + @") - ...");
                else
                    ;

                return iRes;
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
            HHandlerDbULoaderDest target = _objects[KeySingleton] as HHandlerDbULoaderDest;

            switch (ev.id_detail)
            {
                case (int)ID_DATA_ASKED_HOST.TO_INSERT:
                    target.Insert((int)(ev.par as object[])[0], (ev.par as object[])[1] as DataTable, (ev.par as object[])[2] as object[]);
                    break;
                //case (int)ID_DATA_ASKED_HOST.TO_START:
                //    target.InitSource(ev.par);
                //    break;
                case (int)ID_DATA_ASKED_HOST.TO_STOP:
                    target.Clear((int)(ev.par as object[])[0]); 
                    break;
                default:
                    break;
            }

            base.OnEvtDataRecievedHost(obj);
        }
    }

}
