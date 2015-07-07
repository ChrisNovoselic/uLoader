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
        TORISLib.TorISData m_torIsData;

        public SrcMSTKKSNAMEtoris()
            : base()
        {
            m_torIsData = new TorISData ();
        }

        public SrcMSTKKSNAMEtoris(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsMSTKKSNAMEtoris : GroupSignalsMSTTMSrc
        {
            public GroupSignalsMSTKKSNAMEtoris(HHandlerDbULoader parent, object[] pars)
                : base(parent, pars)
            {
            }

            public event DelegateObjectFunc EvtAdviseItem;
            public event DelegateStringFunc EvtUnadviseItem;

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNALMSTKKSNAMEsql((int)objs[0], (string)objs[2]);
            }

            public void AdviseItems (int grpKey)
            {
                object[] parsToEvt = new object[] { grpKey, string.Empty };

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
                    {
                        base.TableRecieved = value;
                    }
                }
            }
        }

        protected override HHandlerDbULoaderMSTTMSrc.GroupSignals createGroupSignals(object[] objs)
        {
            GroupSignalsMSTKKSNAMEtoris grpRes = new GroupSignalsMSTKKSNAMEtoris(this, objs);

            grpRes.EvtAdviseItem += new DelegateObjectFunc(groupSignals_OnEvtAdviseItem);
            grpRes.EvtUnadviseItem += new DelegateStringFunc(groupSignals_OnEvtUnadviseItem);
            
            return grpRes;
        }

        public override void ClearValues()
        {
        }

        public override void Start()
        {
            base.Start();

            try
            {
                m_torIsData.Connect();

                m_torIsData.ItemNewValue += new _ITorISDataEvents_ItemNewValueEventHandler(torIsData_ItemNewValue);
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

            ((GroupSignalsMSTKKSNAMEtoris)m_dictGroupSignals[key]).AdviseItems(key);
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
                if (m_dictGroupSignals.ContainsKey (key) == true)
                    ((GroupSignalsMSTKKSNAMEtoris)m_dictGroupSignals[key]).UnadviseItems();
                else
                    ;

                base.Stop(key);
            }
            else
                ;
        }
        /// <summary>
        /// Старт зависимых потоков
        /// </summary>
        protected new void startThreadDepended()
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
        protected new void register(int id, int indx, ConnectionSettings connSett, string name)
        {
            //???
            ;
        }

        private object lockAdvisedItems;
        private Dictionary <string, int> m_dictSignalsAdvised;

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

        private void groupSignals_OnEvtUnadviseItem(string kks_name)
        {
            int err = -1
                , idGrpSgnls = -1;
            string strErr = string.Empty
                , strIds = string.Empty;

            lock (lockAdvisedItems)
            {
                if (m_dictSignalsAdvised.ContainsKey(kks_name) == true)
                    return;
                else
                    ;

                idGrpSgnls = m_dictSignalsAdvised[kks_name];
                strIds = @" [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + idGrpSgnls + @"]: ";

                err = m_torIsData.UnadviseItem(kks_name);

                if (! (err == 0))
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

                Logging.Logg().Action(@"Отписка на сигнал" + strIds + kks_name, Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void torIsData_ItemNewValue(string kksname, int type, object value, double timestamp, int quality, int status)
        {
            //string errorStr;

            //if (type != 3)
            //{
            //    MainForm.log.LogToFile("Получено значение для " + item + " с неправильным типом: " + type.ToString(), true, true, true);
            //    return;
            //}

            //if (!AddrIsAdvised(item))
            //{
            //    MainForm.log.LogToFile("Получено значение с неправильным адресом " + item, true, true, true);
            //    return;
            //}

            //if (quality != 0)
            //{
            //    switch (quality)
            //    {
            //        case 1: errorStr = "Недостоверный ответ от КП"; break;
            //        case 2: errorStr = "Нет связи с КП"; break;
            //        case 3: errorStr = "Аппаратная ошибка"; break;
            //        case 4: errorStr = "Ошибка конфигурации"; break;
            //        case 5: errorStr = "Performance overflow"; break;
            //        case 6: errorStr = "Software error"; break;
            //        case 7: errorStr = "Потеря связи с ЦППС"; break;
            //        case 8: errorStr = "Ошибка протокола при ответе от КП"; break;
            //        case 9: errorStr = "Логически неверный ответ от КП"; break;
            //        default: errorStr = "Неизвестная ошибка " + quality.ToString(); break;
            //    }
            //    MainForm.log.LogToFile("Получено значение для  " + item + " с ошибкой: " + errorStr, true, true, true);
            //    return;
            //}

            //DataGridValues d = new DataGridValues();
            //d.receivedTimestamp = DateTime.Now;
            //d.name = item;
            //d.type = type;
            //d.value = (float)value;
            //d.quality = quality;
            //d.status = status;
            //if (timestamp != 0)
            //    d.timestamp = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
            //else
            //    d.timestamp = d.receivedTimestamp;
            //d.saved = false;
            //d.id = cacheNextIndex++;

            //lock (lockingCache)
            //{
            //    cacheToDbValues c;
            //    c.values = d;
            //    cache.Add(c);
            //    if (cache.Count == cacheMaxSize)
            //        cache.RemoveAt(0);
            //}

            //if (MainForm.showSignals)
            //    delegateDataGridInsert(d);
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
