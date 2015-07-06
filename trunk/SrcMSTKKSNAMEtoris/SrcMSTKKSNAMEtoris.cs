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
        }

        protected override HULoader.GroupSignals createGroupSignals(object[] objs)
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
                m_torIsData.Disconnect();
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

        public override void Stop()
        {
            stop ();

            base.Stop();
        }

        private void torIsData_ItemNewValue(string kksname, int type, object value, double timestamp, int quality, int status)
        {
        }

        private void torIsData_ChangeStatus(int status)
        {
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
