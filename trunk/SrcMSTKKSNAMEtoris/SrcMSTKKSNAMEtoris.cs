using System;
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

            protected override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNALMSTKKSNAMEsql((int)objs[0], (string)objs[2]);
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

            private void sendVar(string valueAddr)
            {
                //long error = 0;
                //string errorStr;

                //if (!torisConnected)
                //    return;
                //if (AddrIsAdvised(valueAddr))
                //    return;

                //MainForm.log.LogToFile("Подписка на сигнал " + valueAddr, true, true, true);

                //lock (lockingValuesAdvised)
                //{
                //    error = torisData.AdviseItems(valueAddr);

                //    if (error != 0)
                //    {
                //        switch (error)
                //        {
                //            case 1: errorStr = "ETORIS_NOTCONFIG"; break;
                //            case 2: errorStr = "ETORIS_INVALIDITEM"; break;
                //            case 3: errorStr = "ETORIS_INVALIDATTR"; break;
                //            case 4: errorStr = "ETORIS_INVALIDTYPE"; break;
                //            case 5: errorStr = "ETORIS_INVALIDHANDLE"; break;
                //            case 6: errorStr = "ETORIS_NOTREGISTER"; break;
                //            case 7: errorStr = "ETORIS_ALREADYADVISED"; break;
                //            case 8: errorStr = "ETORIS_INVALIDITEMTYPE"; break;
                //            case 9: errorStr = "ETORIS_SHUTDOWN"; break;
                //            default: errorStr = "Неизвестная ошибка " + error.ToString(); break;
                //        }

                //        MainForm.log.LogToFile("Ошибка подписки на сигнал " + valueAddr + ": " + errorStr, true, true, true);
                //        return;
                //    }

                //    torisValuesAddrAdvised.Add(valueAddr);
                //}

                //int quality, status;
                //int type = 3;
                //object value;
                //double timestamp;

                //error = torisData.ReadItem(valueAddr, ref type, out value, out timestamp, out quality, out status);
                //if (error != 0)
                //{
                //    MainForm.log.LogToFile("Ошибка вызова ReadItem для " + valueAddr + ": " + error.ToString(), true, true, true);
                //    return;
                //}

                //TorisRelationship_ItemNewValue(valueAddr, type, value, timestamp, quality, status);
            }
        }

        protected override HHandlerDbULoaderMSTTMSrc.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsMSTKKSNAMEtoris(this, objs);
        }

        public override void ClearValues()
        {
        }

        private void start ()
        {
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

        private void stop ()
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
        }

        public override void Start()
        {
            base.Start();

            start ();
        }

        public override void Start(int key)
        {
            base.Start(key);

            //???
            ;
        }

        public override void Stop()
        {
            stop ();

            base.Stop();
        }

        public override void Stop(int key)
        {
            //???
            ;

            base.Stop(key);
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
            _Id = 1004;

            createObject(typeof(SrcMSTKKSNAMEtoris));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
