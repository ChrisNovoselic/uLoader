﻿using System;
using System.Collections.Generic;
using System.Data; //DataTable
using System.Diagnostics;

using TORISLib;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP.Helper;
using ASUTP.PlugIn;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP;

namespace SrcMST
{
    public class SrcMSTKKSNAMEtoris : HHandlerDbULoaderDatetimeSrc
    {
        private enum ERROR {
            UNKNOWN = 0
            , ETORIS_NOTCONFIG
            , ETORIS_INVALIDITEM
            , ETORIS_INVALIDATTR
            , ETORIS_INVALIDTYPE
            , ETORIS_INVALIDHANDLE
            , ETORIS_NOTREGISTER
            , ETORIS_ALREADYADVISED
            , ETORIS_INVALIDITEMTYPE
            , ETORIS_SHUTDOWN
            ,
        }

        private enum CONNECT_RESULT
        {
            SUCCESS = 0
            , ODIN
            , DVA
            , TRI
        }

        enum StatesMachine
        {
            Values
            ,
        }

        TORISLib.TorISData
        //TorISData
            m_torIsData
            ;

        public SrcMSTKKSNAMEtoris()
            //??? аргументы лишние, кроме 1-го
            : base(UCHET.Trice, string.Empty, MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.NEXTSTEP_HALF_PERIOD)
        {
            initialize();
        }

        public SrcMSTKKSNAMEtoris(PlugInULoader iPlugIn)
            //??? аргументы лишние, кроме 1-го
            : base(iPlugIn, UCHET.Trice, string.Empty, MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.NEXTSTEP_HALF_PERIOD)
        {
            initialize();
        }

        private void initialize()
        {
            try {
                m_torIsData = (TorISData)System.Runtime.InteropServices.Marshal.GetActiveObject("TorIS.TorISData");
            } catch (Exception e) {
                try {
                    m_torIsData = new TorISData();
                } catch { }
            }

            lockAdvisedItems = new object();
        }

        private class GroupSignalsMSTKKSNAMEtoris : GroupSignalsDatetimeSrc
        {
            /// <summary>
            /// Объект для синхронизации изменения очереди событий
            /// </summary>
            public object m_lockData;

            public DataTable m_tableTorIs;
            //public DataTable TableTorIs { get { return m_tableTorIs; } }

            DataTable m_TablePrevValue;

            static int SEC_TO_REPEAT_BY_ZERO = 22
                , SEC_TO_REPEAT_BY_ZERO_OFFSET = 4;

            public GroupSignalsMSTKKSNAMEtoris(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                m_tableTorIs = new DataTable ();
                m_tableTorIs.Columns.AddRange (new DataColumn[] {
                    new DataColumn (@"ID", typeof (string))
                    , new DataColumn (@"VALUE", typeof (double))
                    , new DataColumn (@"DATETIME", typeof (DateTime))
                });

                m_TablePrevValue = m_tableTorIs.Copy();

                for (int i = 0; i < m_arSignals.Length; i++)
                    m_TablePrevValue.Rows.Add(new object[] {
                        // ??? формула
                        (m_arSignals[i] as uLoaderCommon.HHandlerDbULoaderSrc.GroupSignalsSrc.SIGNALMSTKKSNAMEsql).m_kks_name.ToString()
                        , Convert.ToDouble(0.ToString("F2"))
                        , DateTime.MinValue
                    });

                RepeatSignal += new RepeatSignalEventHandler(repeat_value);

                m_lockData = new object();
            }

            public event DelegateObjectFunc EvtAdviseItem;
            public event DelegateStringFunc EvtUnadviseItem;

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNALMSTKKSNAMEsql(this, (int)objs[0], (string)objs[2]);
            }
            /// <summary>
            /// Зарегистрировать сигнал(-ы) (подписаться) в OPC-сервере
            /// </summary>
            public void AdviseItems ()
            {
                // сформировать объект - аргумент события
                // [0] - идентификатор группы сигналов
                // , [1] - перечень сигналов для регистрации сигнала(-ов) (подписка)
                object[] parsToEvt = new object[] { m_Id, string.Empty };

                try {
                    foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                        if (sgnl.IsFormula == false)
                            parsToEvt[1] += sgnl.m_kks_name + @",";
                        else
                            ; // формула
                
                    if (((string)parsToEvt[1]).Equals(string.Empty) == false)
                    {// только, если есть сигналы для подписки
                        // исключить лишнюю запятую в списке
                        parsToEvt[1] = ((string)parsToEvt[1]).Substring(0, ((string)parsToEvt[1]).Length - 1);
                        // иницициировать событие - подписки
                        EvtAdviseItem(parsToEvt);
                    }
                    else
                        ;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"GroupSignalsMSTKKSNAMEtoris.AdviseItems () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            /// <summary>
            /// Отменить регистрацию сигнала(-ов) (отписаться) в OPC-сервере
            /// </summary>
            public void UnadviseItems()
            {
                string parsToEvt = string.Empty;
                IAsyncResult iar = null;

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.IsFormula == false)
                        parsToEvt += sgnl.m_kks_name + @",";
                    else
                        ; // формула

                if (parsToEvt.Equals(string.Empty) == false)
                {// только, если есть сигналы для отписки
                    // исключить лишнюю запятую в списке
                    parsToEvt = parsToEvt.Substring(0, parsToEvt.Length - 1);
                    // иницициировать событие - отписки
                    iar = EvtUnadviseItem.BeginInvoke(parsToEvt, null, null);
                    // ожидать окончания обработки события (вар.№1)
                    iar.AsyncWaitHandle.WaitOne();
                    //// ожидать окончания обработки события (вар.№2)
                    //EvtUnadviseItem.EndInvoke(iar);
                }
                else
                    ;
            }

            protected override void setQuery()
            {
                ;
            }

            protected override object getIdMain (object id_mst)
            {
                int iRes = -1;

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.m_kks_name.Equals((string)id_mst) == true)
                    {
                        iRes = sgnl.m_idMain;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            public override DataTable TableRecieved
            {
                get
                {
                    try {
                        DataTable table = base.TableRecieved;

                        if (Equals (table, null) == false)
                            //Перебор таблицы с последними значениями сигналов
                            foreach (DataRow r in m_TablePrevValue.Rows)
                                if (((DateTime.UtcNow - (DateTime)r [2]).TotalSeconds < SEC_TO_REPEAT_BY_ZERO))
                                    RepeatSignal?.Invoke (this, new RepeatSignalEventArgs ("zero_count", new object ()));
                                else
                                    ;
                        else
                            ;
                    } catch (Exception e) {
                        Logging.Logg ().Exception (e, $@"GroupSignalsMSTKKSNAMEtoris.TableRecieved::get - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return base.TableRecieved;
                }

                set
                {
                    try {
                        //Требуется добавить идентификаторы 'id_main'
                        if ((Equals(value, null) == false)
                            && (!(value.Columns.IndexOf(@"ID") < 0)))
                        {
                            DataTable tblVal = value.Copy();
                            tblVal.Columns.Add(@"KKSNAME_MST", typeof(string));
                            //tblVal.Columns.Add(@"ID_MST", typeof(int));

                            foreach (DataRow r in tblVal.Rows)
                            {
                                r[@"KKSNAME_MST"] = r[@"ID"];
                                //r[@"ID_MST"] = getIdMST((string)r[@"KKSNAME_MST"]);

                                r[@"ID"] = getIdMain((string)r[@"KKSNAME_MST"]);
                            }

                            base.TableRecieved = tblVal;
                        }
                        else
                        {
                            base.TableRecieved = value;
                        }
                    } catch (Exception e) {
                        Logging.Logg ().Exception (e, $@"GroupSignalsMSTKKSNAMEtoris.TableRecieved::set - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }

            public void ItemSetValue(string kksname, object value, double timestamp, int quality, int status)
            {
                DateTime dtVal = DateTime.UtcNow;

                if (kksname == "zero_count")
                {
                    try
                    {
                        foreach (DataRow r in m_TablePrevValue.Rows)
                        {
                            if ((((DateTime)r [2]).Equals (DateTime.MinValue) == false)
                                && ((DateTime.UtcNow  - (DateTime)r [2]).TotalSeconds < (SEC_TO_REPEAT_BY_ZERO + SEC_TO_REPEAT_BY_ZERO_OFFSET)))
                            //если метка времени последнего значения меньше текущего времени со смещением в период обновления
                                RepeatSignal?.Invoke (this, new RepeatSignalEventArgs (r [0].ToString (), r [1]));
                            else
                                ;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "SrcMSTKKSNAMEtoris.GroupSignalsMSTKKSNAMEtoris.ItemNewValue - Ошибка перебора m_TablePrevValue при kksname = zero_count", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
                else
                {
                    //lock (this) {
                        try
                        {
                            if (Equals(m_TablePrevValue, null) == false) {
                                //Logging.Logg ().Debug ($@"::ItemSetValue () - строк было <{m_TablePrevValue.Rows.Count}>...", Logging.INDEX_MESSAGE.NOT_SET);

                                foreach (DataRow r in m_TablePrevValue.Rows)
                                {
                                    if (r [0].ToString ().Trim () == kksname) {
                                        r [1] = value;
                                        if (status == -1991)
                                            r [2] = Convert.ToDateTime (r [2]).AddSeconds (SEC_TO_REPEAT_BY_ZERO);
                                        else
                                            r [2] = dtVal;
                                    } else
                                        ;

                                    if ((((DateTime)r [2]).Equals (DateTime.MinValue) == false)
                                        && ((DateTime.UtcNow  - (DateTime)r [2]).TotalSeconds < (SEC_TO_REPEAT_BY_ZERO + SEC_TO_REPEAT_BY_ZERO_OFFSET)))
                                    //если метка времени последнего значения меньше текущего времени со смещением в период обновления
                                        RepeatSignal?.Invoke (this, new RepeatSignalEventArgs (r [0].ToString (), r [1]));
                                    else
                                        ;
                                }
                            } else
                                //Logging.Logg ().Error ($@"::ItemSetValue () - предыдущая таблица пуста...", Logging.INDEX_MESSAGE.NOT_SET)
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "SrcMSTKKSNAMEtoris.GroupSignalsMSTKKSNAMEtoris.ItemNewValue - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    //}

                    lock (m_lockData)
                    {
                        //Logging.Logg().Debug($@"::ItemSetValue () - добавить строку: kks-code <{kksname}>, метка <{dtVal}> значение <{value}> ...", Logging.INDEX_MESSAGE.NOT_SET);
                        m_tableTorIs.Rows.Add(new object[] { kksname, value, dtVal });
                    }

                    //Console.WriteLine(@"Получено значение для сигнала:" + kksname + @"(" + value + @", " + dtVal.ToString(@"dd.MM.yyyy HH:mm:ss.fff") + @")");
                }
            }

            public void ClearValues ()
            {
                int iPrev = 0, iDel = 0, iCur = 0;

                lock (m_lockData)
                {
                    iPrev = m_tableTorIs.Rows.Count;
                    string strSel =
                        @"DATETIME<'" + DateTimeBegin.AddSeconds(-(PeriodMain.TotalSeconds + 15)).ToUniversalTime().ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"' OR DATETIME>='" + DateTimeBegin.AddSeconds(PeriodMain.TotalSeconds + 5).ToUniversalTime().ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'"
                        //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                        ;

                    DataRow[] rowsDel = null;
                    try { rowsDel = m_tableTorIs.Select(strSel); }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"SrcMSTKKSNAMEtoris.GroupSignalsMSTKKSNAMEtoris::ClearValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    if (!(rowsDel == null))
                    {
                        iDel = rowsDel.Length;
                        if (rowsDel.Length > 0)
                        {
                            foreach (DataRow r in rowsDel)
                            {
                                //Debug.Print("Удалено значение для сигнала:" + r[0].ToString() + "(" + r[1].ToString() + "," + r[2].ToString() + ") " +DateTime.Now.ToString());
                                //Logging.Logg().Action("StateCheckResponse:m_tableTorIs.Rows.Remove()", Logging.INDEX_MESSAGE.NOT_SET);
                                m_tableTorIs.Rows.Remove(r);

                            }
                            //Debug.Print(strSel);
                            //m_tableTorIs = returnTable(m_tableTorIs);
                            //??? Обязательно ли...
                            //Logging.Logg().Action("StateCheckResponse:m_tableTorIs.AcceptChanges()", Logging.INDEX_MESSAGE.NOT_SET);
                            m_tableTorIs.AcceptChanges();
                        }
                        else
                            ;
                    }
                    else
                        ;

                    //m_tableTorIs = returnTable(m_tableTorIs);

                    iCur = m_tableTorIs.Rows.Count;
                }

                //Console.WriteLine(@"Обновление рез-та [ID=" + m_Id + @"]: " + @"(было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @")");
            }

            private void repeat_value(object sender, RepeatSignalEventArgs e)
            {
                ItemSetValue(e.m_kks_name, e.m_Value, 0, 0, -1991);
            }
            
            /// <summary>
            /// Класс для описания аргумента события - повторная запись значения
            /// </summary>
            public class RepeatSignalEventArgs : EventArgs
            {
                /// <summary>
                /// Имя изменяемого параметра
                /// </summary>
                public string m_kks_name;

                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public object m_Value;

                public RepeatSignalEventArgs()
                    : base()
                {
                    m_kks_name = string.Empty;
                    m_Value = string.Empty;

                }

                public RepeatSignalEventArgs(string kks_name, object value)
                    : this()
                {
                    m_kks_name = kks_name;
                    m_Value = value;
                }
            }

            /// <summary>
            /// Тип делегата для обработки события - повторная запись значения
            /// </summary>
            public delegate void RepeatSignalEventHandler(object obj, RepeatSignalEventArgs e);

            /// <summary>
            /// Делегат - повторная запись значения
            /// </summary>
            public RepeatSignalEventHandler RepeatSignal;
        }

        protected override HHandlerDbULoaderDatetimeSrc.GroupSignals createGroupSignals(int id, object[] objs)
        {
            Logging.Logg ().Debug ($@"::createGroupSignals (id={id}, args.Length={objs.Length}) - ...", Logging.INDEX_MESSAGE.NOT_SET);

            GroupSignalsMSTKKSNAMEtoris grpRes = new GroupSignalsMSTKKSNAMEtoris(this, id, objs);

            grpRes.EvtAdviseItem += new DelegateObjectFunc(groupSignals_OnEvtAdviseItem);
            grpRes.EvtUnadviseItem += new DelegateStringFunc(groupSignals_OnEvtUnadviseItem);
            
            return grpRes;
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.Values);

            return iRes;
        }

        public override void ClearValues()
        {
            if (!(TableRecieved == null))
            {
                (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsMSTKKSNAMEtoris).ClearValues ();
            }
            else
                ;
        }

        public override void Start()
        {
            int iResConnect = -1;

            base.Start();

            try
            {
                iResConnect = m_torIsData.Connect();

                switch ((CONNECT_RESULT)iResConnect) {
                    case CONNECT_RESULT.SUCCESS:
                        break;
                    case CONNECT_RESULT.ODIN:
                        break;
                    case CONNECT_RESULT.DVA:
                        break;
                    case CONNECT_RESULT.TRI:
                        break;
                    default:
                        break;
                }

                m_torIsData.ItemNewValue += new _ITorISDataEvents_ItemNewValueEventHandler(torIsData_ItemNewValue);
                //m_torIsData.ChangeAttributeValue += new _ITorISDataEvents_ChangeAttributeValueEventHandler(torIsData_ChangeAttributeValue);
                m_torIsData.ChangeStatus += new _ITorISDataEvents_ChangeStatusEventHandler(torIsData_ChangeStatus);

                Logging.Logg ().Action (string.Format (@"SrcMST.SrcMSTKKSNAMEToris::Start() - [ID={0}:{1}, key={2}], результат ={3}..."
                        , _iPlugin._Id, _iPlugin.KeySingleton, IdGroupSignalsCurrent, iResConnect)
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"SrcMSTKKSNAMEtoris::Start () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        public override void Start(int key)
        {
            base.Start(key);

            lock (lockAdvisedItems)
            {
                try {
                    ((GroupSignalsMSTKKSNAMEtoris)m_dictGroupSignals [key]).AdviseItems ();
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"::Start (key={key}) - [ID={_iPlugin._Id}:{_iPlugin.KeySingleton}, key={IdGroupSignalsCurrent}] ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        public new void Stop()
        {
            try
            {
                m_torIsData.ItemNewValue -= new _ITorISDataEvents_ItemNewValueEventHandler (torIsData_ItemNewValue);
                //m_torIsData.ChangeAttributeValue -= new _ITorISDataEvents_ChangeAttributeValueEventHandler (torIsData_ChangeAttributeValue);
                m_torIsData.ChangeStatus -= new _ITorISDataEvents_ChangeStatusEventHandler (torIsData_ChangeStatus);

                m_torIsData.Disconnect();

                m_torIsData = null;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"SrcMSTKKSNAMEtoris::Stop () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            ((HHandler)this).Stop();
        }

        public override void Stop(int key, ID_HEAD_ASKED_HOST direct)
        {
            if (IsStarted == true)
            {
                lock (lockAdvisedItems)
                {
                    if (m_dictGroupSignals.ContainsKey (key) == true)
                        ((GroupSignalsMSTKKSNAMEtoris)m_dictGroupSignals[key]).UnadviseItems();
                    else
                        ;
                }

                base.Stop(key, direct);
            }
            else
                ;
        }
        /// <summary>
        /// Старт зависимых потоков
        /// </summary>
        protected override void startThreadDepended()
        {
            startThreadQueue();
        }
        /// <summary>
        /// Регистрация источника информации
        /// </summary>
        /// <param name="id">Ключ в словаре с идентификаторами соединений</param>
        /// <param name="indx">Индекс в массиве - элементе словаря с идентификаторами соединений</param>
        /// <param name="connSett">Параметры соединения с источником информации</param>
        /// <param name="name">Наименование соединения</param>
        protected override void register(int id, int indx, ConnectionSettings connSett, string name)
        {
            //???
            ;
        }
        /// <summary>
        /// Объект синхронизации для доступа к словарю с зарегистрированными сигналами
        /// </summary>
        private object lockAdvisedItems;
        /// <summary>
        /// Словарь с зарегистрированными сигналами
        /// </summary>
        private Dictionary <string, int> m_dictSignalsAdvised;
        /// <summary>
        /// Подписаться на сигнал
        /// </summary>
        /// <param name="pars">Параметр (массив: идентификатор группы сигналов + KKS_NAME сигнала)</param>
        private void groupSignals_OnEvtAdviseItem (object pars)
        {
            long err = -1;
            int idGrpSgnls = (int) (pars as object [])[0];
            string []kks_names = ((string)(pars as object [])[1]).Split (new string [] {@","}, StringSplitOptions.RemoveEmptyEntries); 
            string strErr = string.Empty
                , strIds = string.Format(@" [ID={0}:{1}, key={2}]: ", _iPlugin._Id, _iPlugin.KeySingleton, idGrpSgnls);
            // переменные для вызова 'm_torIsData.ReadItem'
            int quality, status;
            int type = 3;
            object value;
            double timestamp;

            if (IsStarted == false)
                return;
            else
                ;

            if (m_dictSignalsAdvised == null)
                m_dictSignalsAdvised = new Dictionary<string,int> ();
            else
                ;

            Logging.Logg ().Action ($@"::groupSignals_OnEvtAdviseItem () - начало подписки  {strIds}, сигналов <{kks_names.Length}>...", Logging.INDEX_MESSAGE.NOT_SET);

            foreach (string kks_name in kks_names) {
                try {
                    if (m_dictSignalsAdvised.ContainsKey(kks_name) == true)
                        return;
                    else
                        ;

                    err = m_torIsData.AdviseItems(kks_name);

                    if (!(err == 0))
                    {
                        switch ((ERROR)err)
                        {
                            case ERROR.ETORIS_NOTCONFIG:
                            case ERROR.ETORIS_INVALIDITEM:
                            case ERROR.ETORIS_INVALIDATTR:
                            case ERROR.ETORIS_INVALIDTYPE:
                            case ERROR.ETORIS_INVALIDHANDLE:
                            case ERROR.ETORIS_NOTREGISTER:
                            case ERROR.ETORIS_ALREADYADVISED:
                            case ERROR.ETORIS_INVALIDITEMTYPE:
                            case ERROR.ETORIS_SHUTDOWN:
                                strErr = ((ERROR)err).ToString ();
                                break;
                            default:
                                strErr = "Неизвестная ошибка " + strErr.ToString();
                                break;
                        }

                        Logging.Logg().Error(@"Ошибка подписки на сигнал" + strIds + kks_name + " - " + strErr, Logging.INDEX_MESSAGE.NOT_SET);
                        continue;
                    }
                    else
                        ;

                    m_dictSignalsAdvised.Add(kks_name, idGrpSgnls);

                    Logging.Logg ().Action (@"Подписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);

                    err = m_torIsData.ReadItem(kks_name, ref type, out value, out timestamp, out quality, out status);
                    if (! (err == 0)) {
                        Logging.Logg().Error(@"Ошибка вызова ReadItem для" + strIds + kks_name + " - " + err.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                        continue;
                    }
                    else
                        ;

                    torIsData_ItemSetValue (kks_name, type, value, timestamp, quality, status);
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"[{strIds}, kks-code={kks_name}]", Logging.INDEX_MESSAGE.NOT_SET);
                }
            } // for

            Logging.Logg ().Action ($@"::groupSignals_OnEvtAdviseItem () - подписка завершена {strIds}, сигналов <{kks_names.Length}>...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private int getIdGroupSignals(string kksname)
        {
            int iRes = -1;
            
            //lock (lockAdvisedItems)
            //{
                if (m_dictSignalsAdvised.ContainsKey (kksname) == true)
                    iRes = m_dictSignalsAdvised[kksname];
                else
                    ;
            //}

            return iRes;
        }

        private void groupSignals_OnEvtUnadviseItem(string par)
        {
            int err = -1
                , idGrpSgnls = -1
                , i = -1;
            string[] kks_names = par.Split(new string [] { @"," }, StringSplitOptions.RemoveEmptyEntries);
            int[] arResUnadvised = new int[kks_names.Length]; //Рез-т выполнения метода отписки от сигнала (-2 - ничего не делали, -1 - исключение, 0 - Ок, 1 - ошибка при выполнении метода)
            string strErr = string.Empty
                , strIds = string.Empty;

            i = -1;
            foreach (string kks_name in kks_names)
            {
                i ++;
                // для каждого сигнала предполагаем лучший вариант - отписка без ошибки
                err = 0;

                if (m_dictSignalsAdvised.ContainsKey(kks_name) == false)
                {
                    arResUnadvised[i] = -2; //ничего не делали

                    continue;
                }
                else
                    ;
                // необходимо только в случае возникновения исключения или ошибки
                idGrpSgnls = m_dictSignalsAdvised[kks_name];
                strIds = string.Format(@" [ID={0}:{1}, key={2}]", _iPlugin._Id, _iPlugin.KeySingleton, idGrpSgnls);

                try
                {
                    err = m_torIsData.UnadviseItem(kks_name);
                }
                catch (Exception e)
                {
                    arResUnadvised[i] = -1; // исключение при выполнении метода

                    Logging.Logg().Exception(e, "TORISLib.TorISDataClass.UnadviseItem (" + strIds + @", KKS_NAME=" + kks_name + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(err == 0))
                {
                    arResUnadvised[i] = 1; // ошибка - как рез-т выполнения метода

                    switch (err)
                    {
                        default:
                            break;
                    }

                    Logging.Logg().Error(@"Ошибка отмены регистрации обработки событий для сигнала " + strIds + @": " + kks_name + " - " + strErr, Logging.INDEX_MESSAGE.NOT_SET);
                    continue;
                }
                else
                    ;

                m_dictSignalsAdvised.Remove(kks_name);

                //Logging.Logg().Action(@"Отписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void torIsData_ItemSetValue(string kksname, int type, object value, double timestamp, int quality, int status)
        {
            string strErr = string.Empty;
            int idGrpSgnls = -1;

            //Logging.Logg ().Debug ($@"::torIsData_ItemSetValue (kks-code={kksname}, type={type}, timestamp={timestamp}, quality={quality}, status={status}) - ..."
            //    , Logging.INDEX_MESSAGE.NOT_SET);

            if (type != 3)
            {
                strErr = "SrcMSTKKSNAMEtoris::torIsData_ItemSetValue () - некорректный тип для: " + kksname + @", тип=" + type.ToString() + @" ...";
                Logging.Logg().Error(strErr, Logging.INDEX_MESSAGE.NOT_SET);

                return;
            }
            else
                ;

            if (quality != 0)
            {
                switch (quality)
                {
                    case 1: strErr = "недостоверный ответ от КП"; break;
                    case 2: strErr = "нет связи с КП"; break;
                    case 3: strErr = "аппаратная ошибка"; break;
                    case 4: strErr = "ошибка конфигурации"; break;
                    case 5: strErr = "performance overflow"; break;
                    case 6: strErr = "software error"; break;
                    case 7: strErr = "потеря связи с ЦППС"; break;
                    case 8: strErr = "ошибка протокола при ответе от КП"; break;
                    case 9: strErr = "логически неверный ответ от КП"; break;
                    default: strErr = "неизвестная ошибка " + quality.ToString(); break;
                }
                strErr = "SrcMSTKKSNAMEtoris::torIsData_ItemSetValue () - сигнал: " + kksname + " с ошибкой: " + strErr;
                Logging.Logg().Error(strErr, Logging.INDEX_MESSAGE.NOT_SET);

                return;
            }

            idGrpSgnls = getIdGroupSignals (kksname);
            if (idGrpSgnls < 0) {
                strErr = "SrcMSTKKSNAMEtoris::torIsData_ItemSetValue () - неиспользуемый сигнал: " + kksname + @" ...";
                Logging.Logg ().Error (strErr, Logging.INDEX_MESSAGE.NOT_SET);

                return;
            } else
                ;

            Logging.Logg ().Debug ($@"::torIsData_ItemSetValue (kks-code={kksname}, type={type}, timestamp={timestamp}, quality={quality}, status={status}) - ..."
                , Logging.INDEX_MESSAGE.NOT_SET);

            (m_dictGroupSignals[idGrpSgnls] as GroupSignalsMSTKKSNAMEtoris).ItemSetValue(kksname, value, timestamp, quality, status);
        }
        /// <summary>
        /// Обработчик события - появление нового значения в OPC-сервере МСТ для сигнала
        /// </summary>
        /// <param name="kksname">ККС-наименование сигнала для которого получено новое значение</param>
        /// <param name="type">Тип ...</param>
        /// <param name="value">Новое значение</param>
        /// <param name="timestamp">Метка времени (всегда = метке времени запуска на выполнение OPC-сервера МСТ)</param>
        /// <param name="quality">Качество полученного нового значения</param>
        /// <param name="status">Состояние ...</param>
        private void torIsData_ItemNewValue(string kksname, int type, object value, double timestamp, int quality, int status)
        {
            lock (lockAdvisedItems)
            {
                torIsData_ItemSetValue(kksname, type, value, timestamp, quality, status);
            }
        }

        private void torIsData_ChangeAttributeValue(string item, string name, int type, object value)
        {
        }
        /// <summary>
        /// Изменение состояние OPC-сервера МСТ
        /// </summary>
        /// <param name="newStatus">Пррзнак нового статуса</param>
        private void torIsData_ChangeStatus(int newStatus)
        {
            switch (newStatus)
            {
                case 3: //Торис-сервер завершил работу
                    break;
                default: //Неизвестный статус
                    break;
            }
        }
        /// <summary>
        /// Проверить наличие ответа на запрос к источнику данных
        /// </summary>
        /// <param name="state">Состояние</param>
        /// <param name="error">Признак ошибки</param>
        /// <param name="table">Таблица - результат запроса</param>
        /// <returns>Результат проверки наличия ответа на запрос</returns>
        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            int iRes = 0;
            error = false;
            table = null;
            lock ((m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsMSTKKSNAMEtoris).m_lockData)
            {
                //Logging.Logg().Action("StateCheckResponse:m_tableTorIs.Copy()", Logging.INDEX_MESSAGE.NOT_SET);
                try
                {
                    table = (m_dictGroupSignals[IdGroupSignalsCurrent] as GroupSignalsMSTKKSNAMEtoris).m_tableTorIs.Copy();
                    
                }
                catch (Exception e)
                {
                    Logging.Logg().Warning("StateCheckResponse:m_tableTorIs.Copy() " + e.Message, Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            return iRes;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.Values:
                    // toris-instance выполняется только на сервере
                    m_datetimeServer.Value = DateTime.Now;
                    m_datetimeServer.BaseUTCOffset = DateTime.Now - DateTime.UtcNow;
                    // округлить до минут
                    SetDatetimeBegin(DateTimeBegin.Round (TimeSpan.FromMinutes (1), MidpointRounding.AwayFromZero));
                    //DateTimeBegin = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
                    //Запрос на выборку данных не требуется
                    ClearValues ();
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
            //string msg = @"HHandlerDbULoaderDest::StateResponse () ::" + ((StatesMachine)state).ToString() + @" - "
            //    + @"[" + PlugInId + @", key=" + m_IdGroupSignalsCurrent + @"] ";

            try
            {
                switch (state)
                {
                    case (int)StatesMachine.Values:
                        //msg =+ @"Ok ...";
                        //Logging.Logg().Debug(@"Получено строк [" + PlugInId + @", key=" + IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);
                        if (TableRecieved == null)
                        {
                            TableRecieved = new DataTable();
                        }
                        else
                            ;

                        TableRecieved = GroupSignals.DataTableDuplicate.Clear(table);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"HHandlerDbULoader::StateResponse (::" + ((StatesMachine)state).ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine (msg);

            return iRes;
        }
    }
}
