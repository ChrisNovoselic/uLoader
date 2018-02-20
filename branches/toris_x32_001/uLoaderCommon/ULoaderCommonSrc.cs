using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Threading;
using System.Diagnostics;

////using HClassLibrary;

using ELW;
using ELW.Library.Math;
using ELW.Library.Math.Exceptions;
using ELW.Library.Math.Expressions;
using ELW.Library.Math.Tools;
using ASUTP;
using ASUTP.PlugIn;
using ASUTP.Database;
using ASUTP.Core;
using ASUTP.Helper;

namespace uLoaderCommon
{
    public abstract class HHandlerDbULoaderSrc : HHandlerDbULoader, ILoaderSrc
    {
        /// <summary>
        /// Идентификатор ТЭЦ
        ///  (при наличии в файле конфигурации для группы источников)
        /// </summary>
        public int m_IdTEC;

        private int m_msecIntervalTimerActivate;
        private int m_msecCorrectTimerActivate;
        private
            //Вариант №1
            System.Threading.Timer
            ////Вариант №2
            //System.Timers.Timer
            ////Вариант №3
            //System.Windows.Threading.DispatcherTimer
                m_timerActivate;

        /// <summary>
        /// Перечисление - возможные значения для способа назначения метки даты/времени значениям (архивные, мгновенные)
        /// </summary>
        public enum UCHET : short { Unknown = -1,
            /// <summary>
            /// Источник - система учета сохраняет значения с меткой даты/времени значения окончания(архивные) интервала интегрирования
            /// </summary>
            Repos,
            /// <summary>
            /// Источник - система учета сохраняет значения с меткой даты/времени значения начала(мгновенные) интервала интегрирования
            /// </summary>
            Trice
        }

        protected UCHET _uchet;
        /// <summary>
        /// Смещение (в часах) в ~ от типа системы учета (по правилу назначения метки времени интегрированного значения за интервал)
        ///  , мгновенная (Trice) - начало интервала
        ///  , архивная (Repos) - окончание интервала
        ///   ; Использование правила ~ от основной цели функционирования системы учета: хранение ретроспективных значений/получение оперативных значений
        /// </summary>
        public int OffsetUchet
        {
            get
            {
                return _uchet == UCHET.Repos ? 0 : Mode == MODE_WORK.CUSTOMIZE ? -1 : 0;
            }
        }

        enum StatesMachine
        {
            CurrentTime
            , Values
        }

        public HHandlerDbULoaderSrc(UCHET uchet)
            : base()
        {
            _uchet = uchet;

            initialize();
        }

        public HHandlerDbULoaderSrc(PlugInULoader iPlugIn, UCHET uchet)
            : base(iPlugIn)
        {
            _uchet = uchet;

            initialize ();
        }

        private int initialize()
        {
            int iRes = 0;

            m_IdTEC = -1;

            m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;

            return iRes;
        }

        public override int Initialize(object[] pars)
        {
            int iRes = base.Initialize(pars);

            string[] arDictAddingKeys = new string[] { @"ID_TEC" };

            foreach (string key in arDictAddingKeys)
                if (m_dictAdding.ContainsKey(key) == true)
                    switch (key)
                    {
                        case @"ID_TEC":
                            m_IdTEC = Int32.Parse(m_dictAdding[key]);
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
                        case @"ID_TEC":
                            m_IdTEC = -1;
                            break;
                        default: ;
                            break;
                    }

            return iRes;
        }

        protected Dictionary<string, CompiledExpression> m_dictCompiledExpression;

        public override int Initialize(int id, object[] pars)
        {
            int iRes = -1;

            iRes = base.Initialize(id, pars);

            if (iRes == 0)
            {
                string[] arFormula = null;
                string fKey = string.Empty
                    , formula = string.Empty;

                try
                {
                    lock (m_lockStateGroupSignals)
                    {
                        if (m_dictGroupSignals.Keys.Contains(id) == false)
                            //Считать переданные параметры - параметрами сигналов
                            ; // выполнено в базовой ф-и

                        else
                            //Сигналы д.б. инициализированы
                            if (m_dictGroupSignals[id].Signals == null)
                                ; // выполнено в базовой ф-и
                            else
                                if (pars[0].GetType().IsArray == true)
                                    //Считать переданные параметры - параметрами сигналов
                                    // выполнено в базовой ф-и
                                    ;
                                else
                                {//Считать переданные параметры - параметрами группы сигналов
                                    // репарсинг 5-го параметра (описание формул для группы сигналов)
                                    if (((string)pars[5]).Equals(string.Empty) == false)
                                    {
                                        arFormula = ((string)pars[5]).Split(';');
                                        for (int i = 0; i < arFormula.Length; i++)
                                        {
                                            fKey = arFormula[i].Split('=')[0];

                                            if (m_dictCompiledExpression == null)
                                                m_dictCompiledExpression = new Dictionary<string, CompiledExpression>();
                                            else
                                                ;

                                            if (m_dictCompiledExpression.ContainsKey(fKey) == false)
                                            {
                                                formula = arFormula[i].Split('=')[1];
                                                m_dictCompiledExpression.Add(fKey, ToolsHelper.Compiler.Compile(ToolsHelper.Parser.Parse(formula)));
                                            }
                                            else
                                                ; // такая формула уже есть
                                        }
                                    }
                                    else
                                        ;
                                }
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"HHandlerDbULoader::Initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = -1;
                }

                return iRes;
            }
            else
                ;

            return iRes;
        }

        protected override object[] getConfirmStartAskedHost(int id)
        {
            object[] objRes = base.getConfirmStartAskedHost(id);

            objRes[3] = new object[]
                {
                    m_dictGroupSignals[id].Mode
                    , m_connSett.id
                    , ((m_dictAdding.ContainsKey(@"ID_TEC") == true) ? Int32.Parse(m_dictAdding[@"ID_TEC"]) : -1)
                };

            return objRes;
        }

        protected override GroupSignals.STATE State
        {
            get { return base.State; }

            set
            {
                base.State = value;

                MSecRemaindToActivate = MSecPeriodRequery;
            }
        }

        protected long MSecRemaindToActivate
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).MSecRemaindToActivate;
                else
                    throw new Exception(@"ULoaderCommon::MSecRemaindToActivate.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).MSecRemaindToActivate = value;
                else
                    throw new Exception(@"ULoaderCommon::MSecRemaindToActivate.set ...");
            }
        }

        ///// <summary>
        ///// Старт группы сигналов с указанным идентификаторм
        ///// </summary>
        ///// <param name="id">Идентификатор группы сигналов</param>
        //public override void Start(int id)
        //{
        //    base.Start (id);

        //    //changeState (m_dictGroupSignals[id].State);
        //    changeState(id);
        //}

        public override void Start()
        {
            base.Start();

            startTimerActivate();
        }

        public override void Stop()
        {
            stopTimerActivate();
            // "забыть" формулы
            if (!(m_dictCompiledExpression == null))
                m_dictCompiledExpression.Clear();
            else
                ;

            base.Stop();
        }

        private int startTimerActivate()
        {
            int iRes = 0;

            stopTimerActivate();
            m_timerActivate =
                //Вариант №1
                new System.Threading.Timer(fTimerActivate, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite)
                ////Вариант №2
                //new System.Timers.Timer (m_msecIntervalTimerActivate);
                ;
            //Вариант №1
            m_timerActivate.Change(0, System.Threading.Timeout.Infinite);
            ////Вариант №2
            //m_timerActivate.Elapsed += new System.Timers.ElapsedEventHandler(timerActivate_OnElapsed);
            //m_timerActivate.Start ();

            return iRes;
        }

        private int stopTimerActivate()
        {
            int iRes = 0;

            if (!(m_timerActivate == null))
            {
                //Вариант №1
                m_timerActivate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                ////Вариант №2
                //m_timerActivate.Stop ();
                //m_timerActivate.Close ();
                m_timerActivate.Dispose();
                m_timerActivate = null;
            }
            else
                ;

            return iRes;
        }
        //Вариант №1
        /// <summary>
        /// Обработчик события таймера 'System.Threading.Timer'
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        private void fTimerActivate(object obj)
        {
            changeState();

            if (!(m_timerActivate == null))
                m_timerActivate.Change(m_msecIntervalTimerActivate, System.Threading.Timeout.Infinite);
            else
                ;
        }
        ////Вариант №2
        ///// <summary>
        /////  Обработчик события таймера 'System.Timers.Timer'
        ///// </summary>
        ///// <param name="obj">Объект, инициировавший событие</param>
        ///// <param name="ev">Аргумент события</param>
        //private void timerActivate_OnElapsed (object obj, System.Timers.ElapsedEventArgs ev)
        //{
        //    changeState(GroupSignals.STATE.TIMER);
        //}
        /// <summary>
        /// Класс для описания группы сигналов источника
        /// </summary>
        protected abstract class GroupSignalsSrc : GroupSignals
        {
            protected class SIGNALBiyskTMoraSrc : SIGNAL
            {
                public string m_NameTable;

                public SIGNALBiyskTMoraSrc(GroupSignals parent, int idMain, object nameTable)
                    : base(parent, idMain, nameTable)
                {
                    this.m_NameTable = IsFormula == false ? (string)nameTable : string.Empty;
                }
            }

            public class SIGNALMSTKKSNAMEsql : SIGNAL
            {
                public string m_kks_name;

                public SIGNALMSTKKSNAMEsql(GroupSignals parent, int idMain, object kks_name)
                    : base(parent, idMain, kks_name)
                {
                    m_kks_name = IsFormula == false ? (string)kks_name : string.Empty;
                }
            }

            public class SIGNALKTSTUsql : SIGNALIdsql
            {
                public int Derivative;

                public SIGNALKTSTUsql(GroupSignals parent, int idMain, object idLocal, bool bAVG, int derivative)
                    : base(parent, idMain, idLocal, bAVG)
                {
                    Derivative = derivative;
                }
            }

            public class SIGNALIdsql : SIGNAL
            {
                public int m_iIdLocal;
                public bool m_bAVG;

                public SIGNALIdsql(GroupSignals parent, int idMain, object idLocal, bool bAVG)
                    : base(parent, idMain, idLocal)
                {
                    m_iIdLocal = IsFormula == false ? (int)idLocal : -1;
                    m_bAVG = bAVG;
                }
            }

            public class SIGNALVzletKKSNAMEsql : SIGNAL
            {
                public string m_kks_name;
                public bool m_bAVG;

                public SIGNALVzletKKSNAMEsql(GroupSignals parent, int idMain, object kks_name, bool bAVG)
                    : base(parent, idMain, kks_name)
                {
                    m_kks_name = IsFormula == false ? (string)kks_name : string.Empty;
                    m_bAVG = bAVG;
                }
            }

            private long m_msecRemaindToActivate;
            /// <summary>
            /// Интервал (милисекунды) - оставшееся время до очередной активации запроса
            /// </summary>
            public long MSecRemaindToActivate { get { return m_msecRemaindToActivate; } set { m_msecRemaindToActivate = value; } }

            private DataTable m_tableRec;
            /// <summary>
            /// Таблица результат выполнения запроса
            /// </summary>
            public override DataTable TableRecieved { get { return m_tableRec; } set { m_tableRec = value; } }

            private int m_iRowCountRecieved;
            /// <summary>
            /// Количество строк в таблице-результате выполнения запроса
            /// </summary>
            public int RowCountRecieved
            {
                get { return m_iRowCountRecieved; }

                set
                {
                    ////Вариант №1
                    //if (isUpdateQuery (value) == true)
                    //    setQuery ();
                    //else
                    //    ;
                    ////Вариант №2 - признак обновления содержания запроса перед его выполнением ()
                    //m_bIsUpdateQuery = isUpdateQuery(value);
                    //Вариант №2 - 
                    if (isUpdateQuery(value) == true)
                        m_strQuery = string.Empty;
                    else
                        ;

                    m_iRowCountRecieved = value;
                }
            }

            public override void Stop()
            {
                base.Stop();

                m_msecRemaindToActivate = 0;
                m_iRowCountRecieved = -1;
            }

            protected string m_strQuery;
            /// <summary>
            /// Строка для запроса
            /// </summary>
            public string Query
            {
                get
                {
                    if (string.IsNullOrEmpty(m_strQuery) == true)
                        try {
                            setQuery();
                        } catch (Exception e) {
                            Logging.Logg().Exception(e
                                , string.Format("HHandlerDbULoaderSrc.GroupSignalsSrc.Query - setQuery() - [ID={0}:{1}, key={2}] - ..."
                                    , (_parent as HHandlerDbULoaderSrc)._iPlugin._Id, (_parent as HHandlerDbULoaderSrc)._iPlugin.KeySingleton, m_Id)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                        ;

                    return m_strQuery;
                }

                /*set { m_strQuery = value; }*/
            }
            /// <summary>
            ///  Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="parent">Объект-владелей группы сигналов источника</param>
            /// <param name="pars">Параметры для инициализации группы сигналов источника</param>
            public GroupSignalsSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                m_msecRemaindToActivate = 0;
                m_iRowCountRecieved = -1;
            }
            /// <summary>
            /// Возвратить признак необходимости обновления содержания запроса
            ///  (составляющая дата/время)
            /// </summary>
            /// <param name="cntRec">Количество записей в крайней (по времени) полуенной таблице-рез-те</param>
            /// <returns>Признак необходимости обновления содержания запроса</returns>
            protected virtual bool isUpdateQuery(int cntRec)
            {
                return true;
            }
            /// <summary>
            /// Установить содержание для запроса
            /// </summary>
            protected abstract void setQuery();
            /// <summary>
            /// Возвратить основной идентификатор по вспомогательному
            ///  , указыаются в файле конфигурации для каждого сигнала отдельно
            /// </summary>
            /// <param name="id_link">Вспомогательный идентификатор</param>
            /// <returns>Основной идентификатор</returns>
            protected abstract object getIdMain(object id_link);
        }
        /// <summary>
        /// Добавить все известные состояния для обработки
        /// </summary>
        /// <returns>Результат выполнения</returns>
        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            return iRes;
        }

        private void changeState()
        {
            lock (m_lockStateGroupSignals)
            {
                //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @"]" + @" 'QUEUE' не найдено...", Logging.INDEX_MESSAGE.NOT_SET);
                //Перевести в состояние "активное" ("ожидание") группы сигналов
                foreach (KeyValuePair<int, GroupSignals> pair in m_dictGroupSignals)
                    if ((pair.Value.State == GroupSignals.STATE.TIMER)
                        || (pair.Value.State == GroupSignals.STATE.SLEEP))
                    {
                        (pair.Value as GroupSignalsSrc).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [" + PlugInId + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                        if ((pair.Value as GroupSignalsSrc).MSecRemaindToActivate < 0)
                        {
                            pair.Value.State = GroupSignals.STATE.QUEUE;
                            //Debug.Print("Timer end " + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                            push(pair.Key);
                        }
                        else
                            ;
                    }
                    else
                        ;
            }
        }
        /// <summary>
        /// Количество строк в таблице-результате для текущей (обрабатываемой) группы
        /// </summary>
        protected int RowCountRecieved
        {
            get
            {
                return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved;
            }

            set
            {
                (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).RowCountRecieved = value;
            }
        }
        /// <summary>
        /// Строка запроса для текущей (обрабатываемой) группы
        /// </summary>
        private string _query
        {
            get
            {
                return Equals(m_dictGroupSignals, null) == false
                    ? m_dictGroupSignals.ContainsKey(IdGroupSignalsCurrent) == true
                        ? (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsSrc).Query
                            : string.Empty // словарь не содержит группы сигналов с идентификатором
                                : string.Empty; // словарь не инициализирован
            }
        }

        protected virtual void calculateValues(ref DataTable tblRes)
        {
            double dblRes = -1F;
            DataRow[] rowsSgnl = null;
            List<VariableValue> variables = null;
            DateTime dtValueRecieved
                , dtValue = DateTime.MinValue;

            foreach (GroupSignals.SIGNAL sgnl in m_dictGroupSignals[IdGroupSignalsCurrent].Signals)
            {
                if (sgnl.IsFormula == true)
                {
                    // подготовить переменные для расчета
                    variables = new List<VariableValue>();

                    for (int i = 0; i < sgnl.m_listIdArgs.Count; i ++)
                    {
                        rowsSgnl = tblRes.Select(@"ID=" + sgnl.m_listIdArgs[i]);

                        if (rowsSgnl.Length == 1)
                        {
                            dtValueRecieved = (DateTime)rowsSgnl[0][@"DATETIME"];
                            if (dtValue.Equals(DateTime.MinValue) == true)
                                dtValue = dtValueRecieved;
                            else
                                if (dtValue > dtValueRecieved)
                                    dtValue = dtValueRecieved;
                                else
                                    ;

                            variables.Add(new VariableValue((float)rowsSgnl[0][@"VALUE"], @"a" + i.ToString()));
                        }
                        else
                            ;
                    }
                    // проверить качество подготовленных для расчета переменных
                    if ((variables.Count == sgnl.m_listIdArgs.Count)
                        && (dtValue.Equals(DateTime.MinValue) == false))
                    {
                        try
                        {
                            dblRes = ToolsHelper.Calculator.Calculate(m_dictCompiledExpression[sgnl.m_fKey], variables);

                            // вставить строку
                            tblRes.Rows.Add(new object[] {
                                sgnl.m_idMain
                                , dtValue
                                , dblRes
                            });
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"HHandlerDbULoaderSrc::calculateValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                        ; // не получено ни одного значения ни для одного из аргументов формулы
                }
                else
                    ;
            }
        }

        protected virtual void parseValues(DataTable table)
        {
            // расчитать формулы
            calculateValues(ref table);

            RowCountRecieved = table.Rows.Count;

            if (Equals(TableRecieved, null) == true)
                TableRecieved = new DataTable();
            else
                ;

            table.Rows.Cast<DataRow>().ToList().ForEach(row => {
                row ["DATETIME"] = ToUtcTime ((DateTime)row ["DATETIME"]).AddHours(-1 * OffsetUchet);
            });

            TableRecieved = GroupSignals.DataTableDuplicate.Clear(table);
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            string msg = string.Empty
                , query = string.Empty;

            try {
                msg = string.Format("HHandlerDbULoaderSrc::StateRequest (state={3}) - [ID={0}:{1}, key={2}] - "
                    , _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent, (StatesMachine)state);

                if ((!(IdGroupSignalsCurrent < 0))
                    && (m_dictIdListeners.ContainsKey(IdGroupSignalsCurrent) == true)
                    && (m_dictIdListeners[IdGroupSignalsCurrent].Length > 0)) {
                    switch (state) {
                        case (int)StatesMachine.CurrentTime:
                            query = GetCurrentTimeQuery (DbTSQLInterface.getTypeDB (m_connSett.port));
                            //GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_connSett.port), m_dictIdListeners[IdGroupSignalsCurrent][0]);
                            Request (m_dictIdListeners [IdGroupSignalsCurrent] [0], query);
                            break;
                        case (int)StatesMachine.Values:
                            if (RowCountRecieved == -1)
                                RowCountRecieved = 0;
                            else
                                ;

                            query = _query;
                            break;
                        default:
                            break;
                    }

                    Request (m_dictIdListeners [IdGroupSignalsCurrent] [0], query);

                    // журналирование обработки состояния для группы сигналов источника
                    Logging.Logg().Debug(string.Format(@"{0} ...", msg)
                        , Logging.INDEX_MESSAGE.D_003);
                    // более детальная информация
                    Logging.Logg ().Debug (string.Format (@"{0} ...", string.Format (@"{0} query={1} ...", msg, m_datetimeServer.Equals(DateTime.MinValue) == false ? query : @"не известно тек./время сервера"))
                        , Logging.INDEX_MESSAGE.D_004);
                } else
                    throw new Exception(string.Format(@"{0} ...", msg));
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

            DataTable table = obj as DataTable;
            string msg = string.Empty;

            try {
                msg = string.Format("HHandlerDbULoaderSrc::StateResponse (state={3}) - [ID={0}:{1}, key={2}] - "
                    , _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent, (StatesMachine)state);

                switch (state) {
                    case (int)StatesMachine.CurrentTime:
                        if ((table.Rows.Count == 1)
                            && (table.Columns.Count == 2)) {
                            m_datetimeServer.Value = (DateTime)(table as DataTable).Rows [0] [0];
                            // смещение ОТ УТЦ ДО времени сервера
                            m_datetimeServer.BaseUTCOffset = (DateTime)(table as DataTable).Rows [0] [0] - (DateTime)(table as DataTable).Rows [0] [1];
                        } else
                            ;
                        break;
                    case (int)StatesMachine.Values:
                        parseValues(table);
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e
                    , string.Format(@"{0} ...", msg)
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine (msg);

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            HHandler.INDEX_WAITHANDLE_REASON resReason = INDEX_WAITHANDLE_REASON.SUCCESS;

            string unknownErr = @"Неизвестная ошибка"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"дату/время СУБД";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"значения в БД";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals(unknownErr) == false)
            {
                msgErr = @"Не удалось получить " + msgErr;
            }
            else
                ;

            Logging.Logg().Error(string.Format(@"[ID={0}:{1}, key={2}] - {3}...", _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent, msgErr), Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine(@"Ошибка. " + msgErr);

            if (!(_iPlugin == null))
                (_iPlugin as PlugInBase).DataAskedHost(new object[] { -1, (int)ID_DATA_ASKED_HOST.ERROR, IdGroupSignalsCurrent, state, msgErr }); //-1 неизвестный идентификатор типа (класса)объекта
            else
                ;

            return resReason;
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

        protected override bool isPush(int curCount)
        {
            return curCount == 0;
        }

        protected override object[] getDataAskedHost()
        {
            object[] arObjToSend = base.getDataAskedHost();

            arObjToSend[arObjToSend.Length - 1] = new object[] {
                Mode // MODE_WORK
                , m_connSett.id //IdSourceConnSett
                , m_IdTEC
            };

            return arObjToSend;
        }
    }

    public abstract class HHandlerDbULoaderDatetimeSrc : HHandlerDbULoaderSrc
    {
        public HTimeSpan m_tsCurIntervalOffset;

        protected virtual string m_strDateTimeDBFormat { get; set; }

        public enum INDEX_MODE_CURINTERVAL { CAUSE, NEXTSTEP, COUNT };
        /// <summary>
        /// Перечисление - идентификаторы режимов вычисления даты/времени начала опроса
        /// </summary>
        public enum MODE_CURINTERVAL
        {
            CAUSE_PERIOD_MINUTE
            , CAUSE_PERIOD_HOUR
            , CAUSE_PERIOD_DAY /*округление до ПЕРИОД, ожидание полного набора записей за ПЕРИОД*/
            , CAUSE_NOT /*текущее время сервера*/
                , NEXTSTEP_HALF_PERIOD, NEXTSTEP_FULL_PERIOD /*Шаг при изменении даты/времени очередного опроса*/
        };
        public MODE_CURINTERVAL[] m_modeCurIntervals;

        public HHandlerDbULoaderDatetimeSrc(UCHET uchet, string dtDBFormat, params MODE_CURINTERVAL[] modeCurIntervals)
            : base(uchet)
        {
            m_strDateTimeDBFormat = dtDBFormat;
            _uchet = uchet;
            m_modeCurIntervals = new MODE_CURINTERVAL[(int)INDEX_MODE_CURINTERVAL.COUNT];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP];
        }

        public HHandlerDbULoaderDatetimeSrc(PlugInULoader iPlugIn, UCHET uchet, string dtDBFormat, params MODE_CURINTERVAL[] modeCurIntervals)
            : base(iPlugIn, uchet)
        {
            m_strDateTimeDBFormat = dtDBFormat;
            m_modeCurIntervals = new MODE_CURINTERVAL[(int)INDEX_MODE_CURINTERVAL.COUNT];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE];
            m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] = modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.NEXTSTEP];
        }
        /// <summary>
        /// Класс для описания группы сигналов источника
        /// </summary>
        protected abstract class GroupSignalsDatetimeSrc : GroupSignalsSrc
        {
            //TODO: необходимо распространить учет на все библиотеки (пока только для КТС Энергия+)
            // для АСУ ТП эн./бл.№1 результат запроса '.Ended'
            protected MODE_DATA_DATETIME _modeDateTime = MODE_DATA_DATETIME.Ended;

            private DateTime _datetimeStart;
            /// <summary>
            /// Дата/время главного периода опроса
            /// </summary>
            public DateTime DateTimeStart
            {
                get { return _datetimeStart; }

                set { _datetimeStart = value; }
            }

            public void SetDatetimeBegin (DateTime stamp)
            {
                _datetimeBegin = stamp;
            }

            public bool IsCompleted
            {
                get
                {
                    string debMes = $@"::IsCompleted.get Start={DateTimeStart}, Customize={IntervalCustomize} _begin={_datetimeBegin}, Diff.Seconds={((_datetimeBegin + PeriodMain) - (DateTimeStart + IntervalCustomize)).TotalSeconds}";

                    if (DateTimeStart.Equals (DateTime.MinValue) == true)
                        throw new Exception (debMes);
                    else
                        Logging.Logg ().Debug (debMes, Logging.INDEX_MESSAGE.NOT_SET);

                    return
                    //??? учитывать ли '_uchet' (_datetimeBegin или DateTimeBegin)
                        ((_datetimeBegin + PeriodMain) - (DateTimeStart + IntervalCustomize)).TotalSeconds > 0;
                }
            }

            public bool IsNextPeriodRequired
            {
                get
                {
                    return (_datetimeBegin - _datetimeBegin.Add (PeriodMain)) > PeriodRequery;
                }
            }

            public void AddPeriodMain ()
            {
                _datetimeBegin += PeriodMain;
            }

            private DateTime _datetimeBegin;
            /// <summary>
            /// Дата/время промежуточного периода опрса
            /// </summary>
            public DateTime DateTimeBegin
            {
                get
                {
                    return (_datetimeBegin.Equals(DateTime.MinValue) == false)
                        && (Equals (_parent, null) == false)
                        ? _datetimeBegin.AddHours((_parent as HHandlerDbULoaderDatetimeSrc).OffsetUchet)
                            : DateTime.MinValue;
                }

                set
                {
                    _datetimeBegin = value;
                }
            }

            public GroupSignalsDatetimeSrc(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                //Инициализация "временнЫх" значений
                // конкретные значения м.б. получены при "старте" 
                _datetimeStart =
                _datetimeBegin =
                    DateTime.MinValue;
            }

            public override void Stop()
            {
                base.Stop();

                DateTimeStart =
                DateTimeBegin =
                    DateTime.MinValue;
            }
            /// <summary>
            /// Строка для условия "по дате/времени", в формате заданном в конструкторе класса
            ///  - начало
            /// </summary>
            protected virtual string DateTimeBeginFormat
            {
                get
                {
                    string strRes = string.Empty;

                    strRes = DateTimeBegin
                        //.AddMilliseconds(1 * msecDiff)
                        .ToString((_parent as HHandlerDbULoaderDatetimeSrc).m_strDateTimeDBFormat, CultureInfo.InvariantCulture);

                    return strRes;
                }
            }
            /// <summary>
            /// Строка для условия "по дате/времени", в формате заданном в конструкторе класса
            ///  - окончание
            /// </summary>
            protected virtual string DateTimeEndFormat
            {
                get
                {
                    //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

                    return DateTimeBegin
                        //.AddMilliseconds(1 * msecDiff)
                        .AddMilliseconds((long)(PeriodMain.TotalMilliseconds)).ToString((_parent as HHandlerDbULoaderDatetimeSrc).m_strDateTimeDBFormat, CultureInfo.InvariantCulture);
                }
            }

            protected override bool isUpdateQuery(int cntRec)
            {
                bool bRes = false;

                switch (Mode)
                {
                    case MODE_WORK.CUR_INTERVAL:
                        if (((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_DAY)
                            || ((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_HOUR)
                            || ((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_PERIOD_MINUTE))
                            bRes = ((RowCountRecieved < 0) || (RowCountRecieved == cntRec)) && (EvtActualizeDateTimeBegin?.Invoke() == 1);
                        else
                            if ((_parent as HHandlerDbULoaderDatetimeSrc).m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE] == MODE_CURINTERVAL.CAUSE_NOT)
                                bRes = EvtActualizeDateTimeBegin?.Invoke() == 1;
                            else
                                ; //throw new Exception (@"Неизвестный режим...")
                        break;
                    case MODE_WORK.CUSTOMIZE:
                        bRes = EvtActualizeDateTimeBegin?.Invoke() == 1;
                        break;
                    default:
                        ; //throw new Exception (@"Неизвестный режим...")
                        break;
                }

                return bRes;
            }
            /// <summary>
            /// Событие для инициирования процесса актуализации даты/времени объекта
            /// </summary>
            private event IntDelegateFunc EvtActualizeDateTimeBegin;
            /// <summary>
            /// Установить метод для актуализации начальной даты/времени для опроса
            /// </summary>
            /// <param name="fActualize">Функция-делегат для актуализации начальной даты/времени для опроса</param>
            public void SetDelegateActualizeDateTimeBegin(IntDelegateFunc fActualize)
            {
                //Установить обработчик только один раз
                if (Equals(EvtActualizeDateTimeBegin, null) == true)
                {
                    EvtActualizeDateTimeBegin += fActualize;
                }
                else
                    ;

                ////??? - вызов при инициализации (сброс каждый раз...)
                //RowCountRecieved = -1;
            }
        }

        public override int Initialize(int id, object[] pars)
        {
            int iRes = base.Initialize(id, pars);

            //Повторная проверка назначения массива параметров
            try
            {
                if (m_dictGroupSignals.Keys.Contains(id) == true)
                    //Сигналы д.б. инициализированы
                    if (m_dictGroupSignals[id].Signals == null)
                        ;
                    else
                        if (pars[0].GetType().IsArray == true)
                            ;
                        else
                        {//Считать переданные параметры - параметрами группы сигналов
                            lock (m_lockStateGroupSignals)
                            {
                                if (m_dictGroupSignals[id].Mode == MODE_WORK.CUSTOMIZE)
                                    if ((!(((DateTime)pars[1] == null)))
                                        && (!(((DateTime)pars[1] == DateTime.MinValue))))
                                    {
                                        ((GroupSignalsDatetimeSrc)m_dictGroupSignals[id]).DateTimeStart = (DateTime)pars[1];
                                        //m_dictGroupSignals[id].MSecIntervalLocal *= 1000; //Т.к. для реж. 'COSTUMIZE' - секунды
                                    }
                                    else
                                        ;
                                else
                                    ;
                            }

                            //Logging.Logg().Debug(@"HHandlerDbULoaderDatetimeSrc::Initialize () - параметры группы сигналов [" + PlugInId + @", key=" + id + @"]...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"HHandlerDbULoaderDatetimeSrc::Initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                iRes = -1;
            }

            if (m_dictGroupSignals.Keys.Contains(id) == true)
                (m_dictGroupSignals[id] as GroupSignalsDatetimeSrc).SetDelegateActualizeDateTimeBegin(actualizeDateTimeBegin);
            else
                ;

            // = Convert.ToInt32(m_dictAdding[@"CUR_INTERVAL_OFFSET"]);
            m_tsCurIntervalOffset = HTimeSpan.Zero;
            if (m_dictAdding.ContainsKey(@"CUR_INTERVAL_OFFSET") == true)
                m_tsCurIntervalOffset = new HTimeSpan(m_dictAdding[@"CUR_INTERVAL_OFFSET"]);
            else
                ;

            return iRes;
        }

        //protected long CountMSecInterval
        //{
        //    get
        //    {
        //        if (!(IdGroupSignalsCurrent < 0))
        //            return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).CountMSecInterval;
        //        else
        //            throw new Exception(@"ULoaderCommon::CountMSecInterval.get ...");
        //    }

        //    set
        //    {
        //        if (!(IdGroupSignalsCurrent < 0))
        //            (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).CountMSecInterval = value;
        //        else
        //            throw new Exception(@"ULoaderCommon::CountMSecInterval.set ...");
        //    }
        //}

        /// <summary>
        /// Дата/время начала опроса для текущей (обрабатываемой) группы
        /// </summary>
        protected DateTime DateTimeStart
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeStart
                        //+ TimeSpan.FromMilliseconds (CountMSecInterval * MSecInterval)
                        ;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.get ...");
            }

            set
            {
                if (!(IdGroupSignalsCurrent < 0))
                    (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeStart = value;
                else
                    throw new Exception(@"ULoaderCommon::DateTimeStart.set ...");
            }
        }

        protected void SetDatetimeBegin (DateTime stamp)
        {
            if (!(IdGroupSignalsCurrent < 0))
                (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).SetDatetimeBegin (stamp);
            else
                throw new Exception ($@"ULoaderCommonSrc::SetDatetimeBegin (stamp={stamp}) - ...");
        }

        private bool IsCompleted
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).IsCompleted;
                else
                    throw new Exception (@"ULoaderCommonSrc::IsComlpeted.get - ...");
            }
        }

        private bool IsNextPeriodRequired
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).IsNextPeriodRequired;
                else
                    throw new Exception (@"ULoaderCommonSrc::IsNextPeriodRequired.get - ...");
            }
        }

        private void NextPeriodMain ()
        {
            if (!(IdGroupSignalsCurrent < 0))
                (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).AddPeriodMain();
            else
                throw new Exception (@"ULoaderCommonSrc::AddPeriodMain () - ...");
        }

        /// <summary>
        /// Дата/время начала опроса для текущей (обрабатываемой) группы
        /// </summary>
        protected DateTime DateTimeBegin
        {
            get
            {
                if (!(IdGroupSignalsCurrent < 0))
                    return (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeBegin
                        //+ TimeSpan.FromMilliseconds (CountMSecInterval * MSecInterval)
                        ;
                else
                    throw new Exception(@"ULoaderCommonSrc::DateTimeBegin.get ...");
            }

            //set
            //{
            //    if (!(IdGroupSignalsCurrent < 0))
            //        (m_dictGroupSignals [IdGroupSignalsCurrent] as GroupSignalsDatetimeSrc).DateTimeBegin = value;
            //    else
            //        throw new Exception (@"ULoaderCommonSrc::DateTimeBegin.set ...");
            //}
        }

        private TimeSpan StepIncrement
        {
            get
            {
                int denum = 1; //??? по умолчанию 'FULL_PERIOD'

                if (m_modeCurIntervals [(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] == HHandlerDbULoaderDatetimeSrc.MODE_CURINTERVAL.NEXTSTEP_FULL_PERIOD)
                // использовать полный период
                    denum = 1;
                else if (m_modeCurIntervals [(int)INDEX_MODE_CURINTERVAL.NEXTSTEP] == HHandlerDbULoaderDatetimeSrc.MODE_CURINTERVAL.NEXTSTEP_HALF_PERIOD)
                // использовать полу/период
                    denum = 2;
                else
                    ;

                return TimeSpan.FromMilliseconds (PeriodMain.TotalMilliseconds / denum);
            }
        }
        
        /// <summary>
        /// Актулизировать дату/время начала опроса
        /// </summary>
        /// <returns>Признак изменения даты/времени начала опроса</returns>
        protected virtual int actualizeDateTimeBegin()
        {
            int iRes = 0;

            if (Mode == MODE_WORK.CUR_INTERVAL)
                ////"сутки"
                //if (PeriodMain.Days == pday)
                //{
                //    DateTimeBegin = (m_dtServer - m_dtServer.TimeOfDay);
                //    DateTimeStart = m_dtServer;
                //    iRes = 1;
                //}
                //else
                //{
                    //Проверить признак 1-го запуска (в режиме CUR_INTERVAL)
                    if (DateTimeBegin == DateTime.MinValue) {
                        SetDatetimeBegin(
                            m_datetimeServer.Utc.Start (m_modeCurIntervals [(int)INDEX_MODE_CURINTERVAL.CAUSE], StepIncrement)
                                .Add (OffsetDataToQuery));

                        //Установить признак перехода
                        iRes = 1;
                    } else
                    //Переход на очередной интервал (повторный опрос)
                        switch (m_modeCurIntervals[(int)INDEX_MODE_CURINTERVAL.CAUSE]) {
                            case MODE_CURINTERVAL.CAUSE_PERIOD_HOUR:
                            case MODE_CURINTERVAL.CAUSE_PERIOD_MINUTE:
                            //Проверить необходимость изменения даты/времени
                                if (IsNextPeriodRequired == true) {
                                    NextPeriodMain();
                                    //CountMSecInterval++;
                                    //Установить признак перехода
                                    iRes = 1;
                                }
                                else
                                    ;
                                break;
                            case MODE_CURINTERVAL.CAUSE_NOT:
                                SetDatetimeBegin(
                                    m_datetimeServer.Utc.Start (StepIncrement)
                                        .Add (OffsetDataToQuery));
                                //Установить признак перехода
                                iRes = 1;
                                break;
                            default:
                                break;
                        }
                //}
            else
                if (Mode == MODE_WORK.CUSTOMIZE) {
                    //Проверить признак 1-го запуска (в режиме COSTUMIZE)
                    if (DateTimeBegin == DateTime.MinValue) {
                        //Проверить указана ли дата/время начала опроса
                        if (DateTimeStart == DateTime.MinValue)
                        //Не указано - опросить ближайший к текущей дате/времени период
                            DateTimeStart =
                                m_datetimeServer.Utc.Start(
                                        m_modeCurIntervals [(int)INDEX_MODE_CURINTERVAL.CAUSE]
                                        , StepIncrement)
                                    .Add(OffsetDataToQuery);
                        else
                        //Указано - ничего не делать
                            //DateTimeStart =
                            //    DateTimeStart.Round(PeriodMain, MidpointRounding.AwayFromZero)
                                ;

                        SetDatetimeBegin(DateTimeStart);
                    } else
                    //Повторный опрос
                        NextPeriodMain();

                    iRes = 1;
                }
                else
                    throw new Exception(@"HHandlerDbULoaderDatetimeSrc::actualizeDateTimeStart () - неизвестный режим ...");

            Logging.Logg().Debug(@"HHandlerDbULoader::actualizeDateTimeStart () - "
                    + string.Format(@"[ID={0}:{1}, key={2}]", _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent)
                    + $", m_dtServer(UTC)={m_datetimeServer.Utc.ToString(@"dd.MM.yyyy HH:mm.ss.fff")}"
                    + $", DateTimeBegin={DateTimeBegin.ToString(@"dd.MM.yyyy HH:mm.ss.fff")}"
                    + $", iRes={iRes}"
                    + @"..."
                , Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        protected override int IdGroupSignalsCurrent
        {
            get
            {
                return base.IdGroupSignalsCurrent;
            }

            set
            {
                if ((!(base.IdGroupSignalsCurrent == value))
                    && (value == -1))
                    if (State == GroupSignals.STATE.SLEEP)
                        if (IsCompleted == false)
                        ////Продолжать сбор - поставить в очередь для обработки идентификатор группы сигналов
                            //push(IdGroupSignalsCurrent)
                            ;
                        else
                        //Остановить сбор данных
                            new Thread (new ParameterizedThreadStart (new DelegateObjectFunc (stop))).Start (IdGroupSignalsCurrent);
                    else
                        ;
                else
                    ;

                base.IdGroupSignalsCurrent = value;
            }
        }

        private void stop(object id)
        {
            Stop((int)id, ID_HEAD_ASKED_HOST.GET);
        }
    }

    //public abstract class HHandlerDbULoaderMSTTMSrc : HHandlerDbULoaderDatetimeSrc
    //{
    //    public TimeSpan m_tsCurIntervalOffset
    //        , m_tsUTCOffset;

    //    public HHandlerDbULoaderMSTTMSrc()
    //        : base(@"yyyy/MM/dd HH:mm:ss")
    //    {
    //    }

    //    public HHandlerDbULoaderMSTTMSrc(PlugInULoader iPlugIn)
    //        : base(iPlugIn, @"yyyy/MM/dd HH:mm:ss")
    //    {
    //    }

    //    public override int Initialize(object[] pars)
    //    {
    //        int iRes = base.Initialize(pars);

    //        if (m_dictAdding.ContainsKey(@"CUR_INTERVAL_OFFSET") == true)
    //            m_iCurIntervalShift = Convert.ToInt32(m_dictAdding[@"CUR_INTERVAL_OFFSET"]);
    //        else
    //            ;

    //        return iRes;
    //    }

    //    protected abstract class GroupSignalsMSTTMSrc : GroupSignalsDatetimeSrc
    //    {
    //        public GroupSignalsMSTTMSrc(HHandlerDbULoader parent, int id, object[] pars)
    //            : base(parent, id, pars)
    //        {
    //        }
    //        /// <summary>
    //        /// Строки для условия "по дате/времени" - начало
    //        /// </summary>
    //        protected override string DateTimeBeginFormat
    //        {
    //            get
    //            {
    //                string strRes = string.Empty;
    //                long msecDiff = -1;

    //                switch (Mode)
    //                {
    //                    case MODE_WORK.CUR_INTERVAL:
    //                        msecDiff = (long)((_parent as HHandlerDbULoaderMSTTMSrc).m_tsCurIntervalOffset.TotalMilliseconds);
    //                        break;
    //                    case MODE_WORK.COSTUMIZE:
    //                        msecDiff = 0; //MSecInterval;
    //                        break;
    //                    default:
    //                        break;
    //                }

    //                strRes = DateTimeBegin.AddSeconds(m_tsUTCOffset.TotalSeconds).AddMilliseconds(msecDiff).ToString((_parent as HHandlerDbULoaderMSTTMSrc).m_strDateTimeDBFormat);
    //                //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

    //                return strRes;
    //            }
    //        }
    //        /// <summary>
    //        /// Строки для условия "по дате/времени" - окончание
    //        /// </summary>
    //        protected override string DateTimeEndFormat
    //        {
    //            get
    //            {
    //                string strRes = string.Empty;
    //                long msecDiff = -1;

    //                switch (Mode)
    //                {
    //                    case MODE_WORK.CUR_INTERVAL:
    //                        msecDiff = 0;
    //                        break;
    //                    case MODE_WORK.COSTUMIZE:
    //                        msecDiff = (int)PeriodLocal.TotalMilliseconds;
    //                        break;
    //                    default:
    //                        break;
    //                }

    //                strRes = DateTimeBegin.AddHours(-6).AddMilliseconds(msecDiff).ToString((_parent as HHandlerDbULoaderMSTTMSrc).m_strDateTimeDBFormat);
    //                //Console.WriteLine(@"DateTimeBegin=" + DateTimeBeginFormat + @"; DateTimeEndFormat=" + strRes);

    //                return strRes;
    //            }
    //        }
    //    }
    //}

    public abstract class HHandlerDbULoaderMSTIDsql : HHandlerDbULoaderDatetimeSrc
    {
        protected enum MODE_TABLE_STATES : short {
            HOUR, DEC, MINUTE
        }

        protected MODE_TABLE_STATES _modeTableStates;

        protected int DataRowCountFlush
        {
            get
            {
                TimeSpan periodMain = PeriodMain;

                if (periodMain.TotalHours % 1 > 0)
                    throw new Exception ("SrcMSTASUTPIDT5tg1sql.DataRowCountFlush::get - некорректно установлен базовый период...");
                else
                    ;

                return _modeTableStates == MODE_TABLE_STATES.HOUR ? 1 //* (int)periodMain.TotalHours
                    : _modeTableStates == MODE_TABLE_STATES.DEC ? 6 //* (int)periodMain.TotalHours
                        : _modeTableStates == MODE_TABLE_STATES.MINUTE ? 60 //* (int)periodMain.TotalHours
                            : 0; // throw Divide by zero
            }
        }

        protected string NameTableSource
        {
            get
            {
                int prefix = -1;

                prefix = _modeTableStates == MODE_TABLE_STATES.HOUR ? 2
                    : _modeTableStates == MODE_TABLE_STATES.DEC ? 1
                        : _modeTableStates == MODE_TABLE_STATES.MINUTE ? 0
                            : -2; // throw object-table not found

                return $"states_real_his_{prefix}";
            }
        }

        protected string NameDatePart
        {
            get
            {
                string strRes = string.Empty;

                switch (_modeTableStates) {
                    case MODE_TABLE_STATES.MINUTE:
                    case MODE_TABLE_STATES.DEC:
                        strRes = "MINUTE";
                        break;
                    case MODE_TABLE_STATES.HOUR:
                        strRes = "HOUR";
                        break;
                }

                return strRes;
            }
        }

        protected enum MODE_WHERE_DATETIME : short {
            UNKNOWN = -1,
            BETWEEN_N_N, BETWEEN_Y_N, BETWEEN_N_Y, // использовать 'BETWEEN(UNKNOWN)' - ВКЛючить значения для левой, правой границам, IN_IN_0_0, IN_IN_1_0, IN_IN_0_1 // принудительное указание ВКЛючения значений для левой, правой границам
            EX_EX_N_N, EX_EX_Y_N, EX_EX_N_Y, // принудительное указание ИСКЛючения значений для левой, правой границам
            IN_EX_N_N, IN_EX_Y_N, IN_EX_N_Y // принудительное указание ВКЛючения значений для левой, ИСКЛючения значений для правой границам
                 , COUNT
        }

        protected MODE_WHERE_DATETIME _modeWhereDatetime;

        public HHandlerDbULoaderMSTIDsql(params MODE_CURINTERVAL[] modeCurIntervals)
            : base(UCHET.Trice, @"yyyyMMdd HH:mm:ss", modeCurIntervals)
        {
        }

        public HHandlerDbULoaderMSTIDsql(PlugInULoader iPlugIn, params MODE_CURINTERVAL[] modeCurIntervals)
            : base(iPlugIn, UCHET.Trice, @"yyyyMMdd HH:mm:ss", modeCurIntervals)
        {
        }

        public override void ClearValues()
        {
        }

        protected abstract class GroupSignalsMSTIDsql : GroupSignalsDatetimeSrc
        {
            public GroupSignalsMSTIDsql(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
            }

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, ID_LOCAL, AVG
                return new SIGNALIdsql(this, (int)objs[0], /*(int)*/objs[2], bool.Parse((string)objs[3]));
            }

            protected override object getIdMain(object id_mst)
            {
                int iRes = -1;

                foreach (SIGNALIdsql sgnl in m_arSignals)
                    if (sgnl.m_iIdLocal == (int)id_mst)
                    {
                        iRes = sgnl.m_idMain;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            protected string WhereDatetime
            {
                get
                {
                    string strRes = string.Empty;

                    string [] comparison = new string [2];

                    // определить содержание 'where'
                    switch ((_parent as HHandlerDbULoaderMSTIDsql)._modeWhereDatetime) {
                        case MODE_WHERE_DATETIME.BETWEEN_N_N:
                        //case MODE_WHERE_DATETIME.EX_EX_Y_Y:
                        case MODE_WHERE_DATETIME.IN_EX_N_Y:
                            comparison [0] = ">";
                            comparison [1] = "<";
                            break;
                        case MODE_WHERE_DATETIME.BETWEEN_N_Y:
                        case MODE_WHERE_DATETIME.EX_EX_Y_N:
                        case MODE_WHERE_DATETIME.IN_EX_N_N:
                            comparison [0] = ">";
                            comparison [1] = "<=";
                            break;
                        case MODE_WHERE_DATETIME.BETWEEN_Y_N:
                        case MODE_WHERE_DATETIME.EX_EX_N_Y:
                            //case MODE_WHERE_DATETIME.IN_EX_Y_Y:
                            comparison [0] = ">=";
                            comparison [1] = "<";
                            break;
                        case MODE_WHERE_DATETIME.UNKNOWN:
                        //case MODE_WHERE_DATETIME.BETWEEN_Y_Y:
                        case MODE_WHERE_DATETIME.IN_EX_Y_N:
                        //case MODE_WHERE_DATETIME.EX_IN_N_Y:
                        //case MODE_WHERE_DATETIME.IN_IN_Y_Y:
                        case MODE_WHERE_DATETIME.EX_EX_N_N:
                        default:
                            break;
                    }

                    if ((string.IsNullOrEmpty (comparison [0]) == false)
                        && (string.IsNullOrEmpty (comparison [1]) == false))
                        strRes = string.Format (@" [last_changed_at] {2} CAST('{0}' as datetime)"
                            + @" AND [last_changed_at] {3} CAST('{1}' as datetime)"
                                , DateTimeBeginFormat
                                , DateTimeEndFormat
                                , comparison [0]
                                , comparison [1]
                            );
                    else
                        strRes = string.Format (@" BETWEEN CAST('{0}' as datetime)"
                            + @" AND CAST('{1}' as datetime)"
                                , DateTimeBeginFormat
                                , DateTimeEndFormat
                            );

                    return strRes;
                }
            }
        }
    }
}
