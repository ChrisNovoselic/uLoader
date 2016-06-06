using System;
using System.Collections.Generic;
using System.Data; //DataTable
using System.Diagnostics;

using TORISLib;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMST
{
    public class SrcMSTKKSNAMEtoris : HHandlerDbULoaderDatetimeSrc
    {
        //private class TorISData : TORISLib.TorISData
        //{
        //}

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
            : base(string.Empty, MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.HALF_PERIOD)
        {
            initialize();
        }

        public SrcMSTKKSNAMEtoris(PlugInULoader iPlugIn)
            //??? аргументы лишние, кроме 1-го
            : base(iPlugIn, string.Empty, MODE_CURINTERVAL.CAUSE_NOT, MODE_CURINTERVAL.HALF_PERIOD)
        {
            initialize();
        }

        private void initialize()
        {
            m_torIsData = new TorISData();

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

            static int s_repeatPrevValue_interval = 22;
            static int s_repeatPrevValue_interval_offset = 4;


            public GroupSignalsMSTKKSNAMEtoris(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                DataColumn[] arColl = new DataColumn[] {
                                                new DataColumn (@"ID", typeof (string))
                                                , new DataColumn (@"VALUE", typeof (double))
                                                , new DataColumn (@"DATETIME", typeof (DateTime))
                                            };

                m_tableTorIs = new DataTable ();

                m_tableTorIs.Columns.AddRange (arColl);
                
                m_TablePrevValue = new DataTable();
                m_TablePrevValue = m_tableTorIs.Copy();

                for (int i = 0; i < m_arSignals.Length; i++)
                {
                    m_TablePrevValue.Rows.Add(new object[] { (m_arSignals[i] as uLoaderCommon.HHandlerDbULoaderSrc.GroupSignalsSrc.SIGNALMSTKKSNAMEsql).m_kks_name.ToString(), Convert.ToDouble(0.ToString("F2")), DateTime.MinValue}); 
                }

                RepeatSignal += new RepeatSignalEventHandler(repeat_value);

                m_lockData = new object();
            }

            public event DelegateObjectFunc EvtAdviseItem;
            public event DelegateStringFunc EvtUnadviseItem;

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNALMSTKKSNAMEsql((int)objs[0], (string)objs[2]);
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

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    parsToEvt[1] += sgnl.m_kks_name + @",";
                
                if (((string)parsToEvt[1]).Equals(string.Empty) == false)
                {// только, если есть сигналы для подписки
                    // исключить лишнюю запятую в списке
                    parsToEvt[1] = ((string)parsToEvt[1]).Substring(0, ((string)parsToEvt[1]).Length - 1);
                    // иницициировать событие - подписки
                    EvtAdviseItem(parsToEvt);
                }
                else
                    ;
            }
            /// <summary>
            /// Отменить регистрацию сигнала(-ов) (отписаться) в OPC-сервере
            /// </summary>
            public void UnadviseItems()
            {
                string parsToEvt = string.Empty;
                
                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    parsToEvt += sgnl.m_kks_name + @",";

                if (parsToEvt.Equals(string.Empty) == false)
                {// только, если есть сигналы для отписки
                    // исключить лишнюю запятую в списке
                    parsToEvt = parsToEvt.Substring(0, parsToEvt.Length - 1);
                    // иницициировать событие - отписки
                    EvtUnadviseItem(parsToEvt);
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
                    DataTable table = base.TableRecieved;
                    
                    if (table != null)
                        foreach (DataRow r in m_TablePrevValue.Rows)//Перебор таблицы с последними значениями сигналов
                        {
                            if (Convert.ToDateTime(r[2]) <= DateTime.UtcNow.AddSeconds(-s_repeatPrevValue_interval))
                                if (RepeatSignal != null)
                                    RepeatSignal(this, new RepeatSignalEventArgs("zero_count"
                                                        , new object()
                                                        ));
                        }

                    return base.TableRecieved;
                }

                set
                {
                    //Требуется добавить идентификаторы 'id_main'
                    if ((!(value == null)) && (!(value.Columns.IndexOf(@"ID") < 0)))
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
                            if ((DateTime)r[2] <= DateTime.UtcNow.AddSeconds(-(s_repeatPrevValue_interval + s_repeatPrevValue_interval_offset)) & (DateTime)r[2] != DateTime.MinValue)//если метка времени последнего значения меньше текущего времени со смещением в период обновления
                            {
                                if (RepeatSignal != null)
                                {
                                    RepeatSignal(this, new RepeatSignalEventArgs(r[0].ToString()
                                                        , r[1]
                                                        ));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "SrcMSTKKSNAMEtoris.GroupSignalsMSTKKSNAMEtoris.ItemNewValue - Ошибка перебора m_TablePrevValue при kksname = zero_count", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
                else
                {
                    lock (this)
                    {
                        try
                        {
                            foreach (DataRow r in m_TablePrevValue.Rows)
                            {
                                if (r[0].ToString().Trim() == kksname)
                                {
                                    r[1] = value;
                                    if (status == -1991)
                                        r[2] = Convert.ToDateTime(r[2]).AddSeconds(s_repeatPrevValue_interval);
                                    else
                                        r[2] = dtVal;
                                }
                                if ((DateTime)r[2] <= DateTime.UtcNow.AddSeconds(-(s_repeatPrevValue_interval + s_repeatPrevValue_interval_offset)) & (DateTime)r[2] != DateTime.MinValue)//если метка времени последнего значения меньше текущего времени со смещением в период обновления
                                {
                                    if (RepeatSignal != null)
                                        RepeatSignal(this, new RepeatSignalEventArgs(r[0].ToString()
                                                            , r[1]
                                                            ));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "SrcMSTKKSNAMEtoris.GroupSignalsMSTKKSNAMEtoris.ItemNewValue - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }

                        //Debug.Print("Добавление строки " + kksname + ", " + value.ToString() + ", " + dtVal.ToString());

                    }

                    lock (m_lockData)
                    {
                        //Logging.Logg().Action("StateCheckResponse:m_tableTorIs.Rows.Add()", Logging.INDEX_MESSAGE.NOT_SET);
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
                        @"DATETIME<'" + DateTimeBegin.AddSeconds(-(PeriodLocal.TotalSeconds + 15)).ToUniversalTime().ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"' OR DATETIME>='" + DateTimeBegin.AddSeconds(PeriodLocal.TotalSeconds + 5).ToUniversalTime().ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'"
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
            /// Событие - повторная запись значения
            /// </summary>
            public RepeatSignalEventHandler RepeatSignal;


        }

        protected override HHandlerDbULoaderDatetimeSrc.GroupSignals createGroupSignals(int id, object[] objs)
        {
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
            base.Start();

            try
            {
                m_torIsData.Connect();

                m_torIsData.ItemNewValue += new _ITorISDataEvents_ItemNewValueEventHandler(torIsData_ItemNewValue);
                m_torIsData.ChangeAttributeValue += new _ITorISDataEvents_ChangeAttributeValueEventHandler(torIsData_ChangeAttributeValue);
                m_torIsData.ChangeStatus += new _ITorISDataEvents_ChangeStatusEventHandler(torIsData_ChangeStatus);
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
                ((GroupSignalsMSTKKSNAMEtoris)m_dictGroupSignals[key]).AdviseItems();
            }
        }

        public new void Stop()
        {
            try
            {
                m_torIsData.ItemNewValue -= torIsData_ItemNewValue;
                m_torIsData.ChangeStatus -= torIsData_ChangeStatus;

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
                , strIds = @" [" + PlugInId + @", key=" + idGrpSgnls + @"]: ";
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

            foreach (string kks_name in kks_names)
            {
                if (m_dictSignalsAdvised.ContainsKey(kks_name) == true)
                    return;
                else
                    ;

                err = m_torIsData.AdviseItems(kks_name);

                if (!(err == 0))
                {
                    switch (err)
                    {
                        case 1: strErr = "ETORIS_NOTCONFIG"; break;
                        case 2: strErr = "ETORIS_INVALIDITEM"; break;
                        case 3: strErr = "ETORIS_INVALIDATTR"; break;
                        case 4: strErr = "ETORIS_INVALIDTYPE"; break;
                        case 5: strErr = "ETORIS_INVALIDHANDLE"; break;
                        case 6: strErr = "ETORIS_NOTREGISTER"; break;
                        case 7: strErr = "ETORIS_ALREADYADVISED"; break;
                        case 8: strErr = "ETORIS_INVALIDITEMTYPE"; break;
                        case 9: strErr = "ETORIS_SHUTDOWN"; break;
                        default: strErr = "Неизвестная ошибка " + strErr.ToString(); break;
                    }

                    Logging.Logg().Error(@"Ошибка подписки на сигнал" + strIds + kks_name + " - " + strErr, Logging.INDEX_MESSAGE.NOT_SET);
                    continue;
                }
                else
                    ;

                m_dictSignalsAdvised.Add(kks_name, idGrpSgnls);

                //Logging.Logg ().Action (@"Подписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);            

                err = m_torIsData.ReadItem(kks_name, ref type, out value, out timestamp, out quality, out status);
                if (! (err == 0))
                {
                    Logging.Logg().Error(@"Ошибка вызова ReadItem для" + strIds + kks_name + " - " + err.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                    continue;
                }
                else
                    ;

                torIsData_ItemSetValue (kks_name, type, value, timestamp, quality, status);
            }
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

        private void groupSignals_OnEvtUnadviseItem(string kks_name)
        {
            int err = -1
                , idGrpSgnls = -1;
            string strErr = string.Empty
                , strIds = string.Empty;

            if (m_dictSignalsAdvised.ContainsKey(kks_name) == false)
                return;
            else
                ;

            idGrpSgnls = m_dictSignalsAdvised[kks_name];
            strIds = @" [" + PlugInId + @", key=" + idGrpSgnls + @"]: ";

            try
            {
                err = m_torIsData.UnadviseItem(kks_name);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "TORISLib.TorISDataClass.UnadviseItem(String item) - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (!(err == 0))
            {
                switch (err)
                {
                    default:
                        break;
                }

                Logging.Logg().Error(@"Ошибка отписки на сигнал" + strIds + kks_name + " - " + strErr, Logging.INDEX_MESSAGE.NOT_SET);
                return;
            }
            else
                ;

            m_dictSignalsAdvised.Remove(kks_name);

            //Logging.Logg().Action(@"Отписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void torIsData_ItemSetValue(string kksname, int type, object value, double timestamp, int quality, int status)
        {
            string strErr = string.Empty;
            int idGrpSgnls = -1;

            if (type != 3)
                {
                    strErr = "SrcMSTKKSNAMEtoris::groupSignals_OnEvtUnadviseItem () - некорректный тип для: " + kksname + @", тип=" + type.ToString() + @" ...";
                    Logging.Logg().Error(strErr, Logging.INDEX_MESSAGE.NOT_SET);

                    return;
                }
                else
                    ;

                idGrpSgnls = getIdGroupSignals(kksname);
                if (idGrpSgnls < 0)
                {
                    strErr = "SrcMSTKKSNAMEtoris::groupSignals_OnEvtUnadviseItem () - неиспользуемый сигнал: " + kksname + @" ...";
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
                    strErr = "SrcMSTKKSNAMEtoris::groupSignals_OnEvtUnadviseItem () - сигнал: " + kksname + " с ошибкой: " + strErr;
                    Logging.Logg().Error(strErr, Logging.INDEX_MESSAGE.NOT_SET);

                    return;
                }

                (m_dictGroupSignals[idGrpSgnls] as GroupSignalsMSTKKSNAMEtoris).ItemSetValue(kksname, value, timestamp, quality, status);
        }

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
                    m_dtServer = DateTime.Now;
                    DateTimeBegin = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
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
