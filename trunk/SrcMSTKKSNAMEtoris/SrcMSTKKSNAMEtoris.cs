using System;
using System.Collections.Generic;
using System.Data; //DataTable

using TORISLib;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMSTKKSNAMEtoris
{
    public class SrcMSTKKSNAMEtoris : HHandlerDbULoaderMSTTMSrc
    {
        enum StatesMachine
        {
            Values
            ,
        }

        TORISLib.TorISData m_torIsData;

        public SrcMSTKKSNAMEtoris()
            : base()
        {
            initialize();
        }

        public SrcMSTKKSNAMEtoris(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
            initialize();
        }

        private void initialize()
        {
            m_torIsData = new TorISData();

            lockAdvisedItems = new object();
        }

        private class GroupSignalsMSTKKSNAMEtoris : GroupSignalsMSTTMSrc
        {
            public DataTable m_tableTorIs;
            //public DataTable TableTorIs { get { return m_tableTorIs; } }

            public GroupSignalsMSTKKSNAMEtoris(HHandlerDbULoader parent, int id, object[] pars)
                : base(parent, id, pars)
            {
                m_tableTorIs = new DataTable ();
                m_tableTorIs.Columns.AddRange (new DataColumn [] {
                                                new DataColumn (@"ID", typeof (string))
                                                , new DataColumn (@"VALUE", typeof (double))
                                                , new DataColumn (@"DATETIME", typeof (DateTime))
                                            });
            }

            public event DelegateObjectFunc EvtAdviseItem;
            public event DelegateStringFunc EvtUnadviseItem;

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNALMSTKKSNAMEsql((int)objs[0], (string)objs[2]);
            }

            public void AdviseItems ()
            {
                object[] parsToEvt = new object[] { m_Id, string.Empty };

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                {
                    parsToEvt [1] = sgnl.m_kks_name;
                    EvtAdviseItem(parsToEvt);
                }
            }

            public void UnadviseItems()
            {
                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    EvtUnadviseItem(sgnl.m_kks_name);
            }

            protected override void setQuery()
            {
                ;
            }

            private int getIdMain (string id_mst)
            {
                int iRes = -1;

                foreach (SIGNALMSTKKSNAMEsql sgnl in m_arSignals)
                    if (sgnl.m_kks_name == id_mst)
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
                get { return base.TableRecieved; }

                set
                {
                    //Требуется добавить идентификаторы 'id_main'
                    if (! (value.Columns.IndexOf (@"ID") < 0))
                    {
                        DataTable tblVal = value.Copy ();
                        tblVal.Columns.Add (@"KKSNAME_MST", typeof(string));
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
                        base.TableRecieved = value;
                }
            }

            public void ItemNewValue(string kksname, object value, double timestamp, int quality, int status)
            {
                //string strIds = @" [ID=" + ((_parent as HHandlerDbULoader)._iPlugin as PlugInBase)._Id + @", key=" + m_Id + @"]: ";

                DateTime dtVal = 
                    //new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp)
                    DateTime.Now
                    //DateTime.FromOADate(timestamp)
                    //new DateTime(1899, 12, 30).AddDays(timestamp)
                    ;

                lock (this)
                {
                    m_tableTorIs.Rows.Add(new object[] { kksname, value, dtVal });
                }

                Console.WriteLine(@"Получено значение для сигнала:" + kksname + @"(" + value + @", " + dtVal.ToString (@"dd.MM.yyyy HH:mm:ss.fff") + @")");
            }

            public void ClearValues ()
            {
                int iPrev = 0, iDel = 0, iCur = 0;

                lock (this)
                {
                    iPrev = m_tableTorIs.Rows.Count;
                    string strSel =
                        @"DATETIME<'" + DateTimeStart.ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"' OR DATETIME>='" + DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'"
                        //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                        ;

                    DataRow[] rowsDel = null;
                    try { rowsDel = m_tableTorIs.Select(strSel); }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ClearValues () - ...");
                    }

                    if (!(rowsDel == null))
                    {
                        iDel = rowsDel.Length;
                        if (rowsDel.Length > 0)
                        {
                            foreach (DataRow r in rowsDel)
                                m_tableTorIs.Rows.Remove(r);
                            //??? Обязательно ли...
                            m_tableTorIs.AcceptChanges();
                        }
                        else
                            ;
                    }
                    else
                        ;

                    iCur = m_tableTorIs.Rows.Count;
                }

                Console.WriteLine(@"Обновление рез-та [ID=" + m_Id + @"]: " + @"(было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @")");
            }
        }

        protected override HHandlerDbULoaderMSTTMSrc.GroupSignals createGroupSignals(int id, object[] objs)
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
                (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsMSTKKSNAMEtoris).ClearValues ();
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
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"SrcMSTKKSNAMEtoris::start () - ...");
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
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"SrcMSTKKSNAMEtoris::stop () - ...");
            }

            ((HHandler)this).Stop();
        }

        public override void Stop(int key)
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

                base.Stop(key);
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

        private object lockAdvisedItems;
        private Dictionary <string, int> m_dictSignalsAdvised;
        /// <summary>
        /// Подписаться на сигнал
        /// </summary>
        /// <param name="pars">Параметр (массив: идентификатор группы сигналов + KKS_NAME сигнала)</param>
        private void groupSignals_OnEvtAdviseItem (object pars)
        {
            long err = -1;
            int idGrpSgnls = (int) (pars as object [])[0];
            string kks_name = (string) (pars as object [])[1]
                , strErr = string.Empty
                , strIds = @" [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + idGrpSgnls + @"]: ";

            if (IsStarted == false)
                return;
            else
                ;

            if (lockAdvisedItems == null)
                lockAdvisedItems = new object ();
            else
                ;

            if (m_dictSignalsAdvised == null)
                m_dictSignalsAdvised = new Dictionary<string,int> ();
            else
                ;

            lock (lockAdvisedItems)
            {
                if (m_dictSignalsAdvised.ContainsKey(kks_name) == true)
                    return;
                else
                    ;

                err = m_torIsData.AdviseItems(kks_name);

                if (! (err == 0))
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
                    return;
                }
                else
                    ;

                m_dictSignalsAdvised.Add(kks_name, idGrpSgnls);
            }

            Logging.Logg ().Action (@"Подписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);

            int quality, status;
            int type = 3;
            object value;
            double timestamp;

            err = m_torIsData.ReadItem(kks_name, ref type, out value, out timestamp, out quality, out status);
            if (! (err == 0))
            {
                Logging.Logg().Error(@"Ошибка вызова ReadItem для" + strIds + kks_name + " - " + err.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                return;
            }
            else
                ;

            torIsData_ItemNewValue (kks_name, type, value, timestamp, quality, status);
        }

        private int getIdGroupSignals(string kksname)
        {
            int iRes = -1;
            
            lock (lockAdvisedItems)
            {
                if (m_dictSignalsAdvised.ContainsKey (kksname) == true)
                    iRes = m_dictSignalsAdvised[kksname];
                else
                    ;
            }

            return iRes;
        }

        private void groupSignals_OnEvtUnadviseItem(string kks_name)
        {
            int err = -1
                , idGrpSgnls = -1;
            string strErr = string.Empty
                , strIds = string.Empty;

            lock (lockAdvisedItems)
            {
                if (m_dictSignalsAdvised.ContainsKey(kks_name) == false)
                    return;
                else
                    ;

                idGrpSgnls = m_dictSignalsAdvised[kks_name];
                strIds = @" [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + idGrpSgnls + @"]: ";

                err = m_torIsData.UnadviseItem(kks_name);

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
            }

            Logging.Logg().Action(@"Отписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void torIsData_ItemNewValue(string kksname, int type, object value, double timestamp, int quality, int status)
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

            (m_dictGroupSignals[idGrpSgnls] as GroupSignalsMSTKKSNAMEtoris).ItemNewValue(kksname, value, timestamp, quality, status);
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

            table = (m_dictGroupSignals[m_IdGroupSignalsCurrent] as GroupSignalsMSTKKSNAMEtoris).m_tableTorIs.Copy();

            return iRes;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.Values:
                    m_dtServer = DateTime.Now;
                    DateTimeStart = m_dtServer.AddMilliseconds(-1 * (m_dtServer.Second * 1000 + m_dtServer.Millisecond));
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
            //    + @"[ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"] ";

            try
            {
                switch (state)
                {
                    case (int)StatesMachine.Values:
                        //msg =+ @"Ok ...";
                        Logging.Logg().Debug(@"Получено строк [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + m_IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);
                        if (TableRecieved == null)
                        {
                            TableRecieved = new DataTable();
                        }
                        else
                            ;

                        TableRecieved = GroupSignals.clearDupValues(table);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HHandlerDbULoader::StateResponse (::" + ((StatesMachine)state).ToString() + @") - ...");
            }

            //Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
            //Console.WriteLine (msg);

            return iRes;
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1005;

            createObject(typeof(SrcMSTKKSNAMEtoris));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
