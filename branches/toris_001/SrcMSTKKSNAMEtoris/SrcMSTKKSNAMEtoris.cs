using System;
using System.Collections.Generic; //Dictionary
using System.Data; //DataTable
using System.Threading; //Timer

using TORISLib;

using HClassLibrary;
using uLoaderCommon;

namespace SrcMSTKKSNAMEtoris
{
    public class SrcMSTKKSNAMEtoris
    {
        /// <summary>
        /// Ссылка на объект "связи" с клиентом
        /// </summary>
        protected IPlugIn _iPlugin;
        /// <summary>
        /// Объект для получения значений от сервера приложений
        /// </summary>
        TORISLib.TorISData m_torIsData;
        /// <summary>
        /// Объект таймера дл активации групп сигналов
        /// </summary>
        System.Threading.Timer m_timerActivate;
        /// <summary>
        /// Интервал проверки выполнения условия активациия группы сигналов
        /// </summary>
        private int m_msecIntervalTimerActivate;

        object m_lockStateGroupSignals;

        Dictionary <int, GroupSignalsMSTKKSNAMEtoris> m_dictGroupSignals;

        public SrcMSTKKSNAMEtoris()
            : base()
        {
            initialize ();
        }

        public SrcMSTKKSNAMEtoris(IPlugIn iPlugIn)
        {
            _iPlugin = iPlugIn;

            initialize();
        }

        private void initialize ()
        {
            m_msecIntervalTimerActivate = (int)uLoaderCommon.DATETIME.MSEC_INTERVAL_TIMER_ACTIVATE;
        }

        protected class GroupSignalsMSTKKSNAMEtoris
        {
            /// <summary>
            /// Сылка на объект владельца текущего объекта
            /// </summary>
            protected SrcMSTKKSNAMEtoris _parent;
            /// <summary>
            /// Перечисление возможных слстояний для группы сигналов
            /// </summary>
            public enum STATE { UNKNOWN = -1, STOP, SLEEP, TIMER, QUEUE, ACTIVE }
            private STATE m_state;
            /// <summary>
            /// Состояние группы сигналов
            /// </summary>
            public virtual STATE State { get { return m_state; } set { m_state = value; } }
            
            protected SIGNAL[] m_arSignals;
            
            /// <summary>
            /// Класс для объекта СИГНАЛ
            /// </summary>
            public class SIGNAL
            {
                /// <summary>
                /// Идентификатор сигнала, уникальный в границах приложения
                /// </summary>
                public int m_idMain;

                public string m_kks_name;

                /// <summary>
                /// Конструктор - основной (с параметром)
                /// </summary>
                /// <param name="idMain">Идентификатор сигнала, уникальный в границах приложения</param>
                public SIGNAL(int idMain, string kks_name)
                {
                    this.m_idMain = idMain;
                    this.m_kks_name = kks_name;
                }
            }

            public GroupSignalsMSTKKSNAMEtoris(SrcMSTKKSNAMEtoris parent, object[] pars)
            {
                _parent = parent;
            }

            protected SIGNAL createSignal(object[] objs)
            {
                //ID_MAIN, KKSNAME
                return new SIGNAL((int)objs[0], (string)objs[2]);
            }

            private int getIdMain (string id_mst)
            {
                int iRes = -1;

                foreach (SIGNAL sgnl in m_arSignals)
                    if (sgnl.m_kks_name == id_mst)
                    {
                        iRes = sgnl.m_idMain;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            private DataTable m_tableRecieved;
            public DataTable TableRecieved
            {
                get { return m_tableRecieved; }

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

                        m_tableRecieved = tblVal;
                    }
                    else
                    {
                        m_tableRecieved = value;
                    }
                }
            }
        }

        protected GroupSignalsMSTKKSNAMEtoris createGroupSignals(object[] objs)
        {
            return new GroupSignalsMSTKKSNAMEtoris(this, objs);
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

        public void Start()
        {
            start ();
        }

        public void Stop()
        {
            stop ();            
        }

        private void torIsData_ItemNewValue(string kksname, int type, object value, double timestamp, int quality, int status)
        {
        }

        private void torIsData_ChangeStatus(int status)
        {
        }

        /// <summary>
        /// Функция обратного вызова таймера
        ///  для активации группы сигналов
        /// </summary>
        /// <param name="obj">аргумент...</param>
        private void fTimerActivate(object obj)
        {
            lock (m_lockStateGroupSignals)
            {
                //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [ID=" + (_iPlugin as PlugInBase)._Id + @"]" + @" 'QUEUE' не найдено...", Logging.INDEX_MESSAGE.NOT_SET);
                //Перевести в состояние "активное" ("ожидание") группы сигналов
                foreach (KeyValuePair<int, GroupSignalsMSTKKSNAMEtoris> pair in m_dictGroupSignals)
                    if (pair.Value.State == GroupSignalsMSTKKSNAMEtoris.STATE.TIMER)
                    {
                        (pair.Value as GroupSignalsMSTKKSNAMEtoris).MSecRemaindToActivate -= m_msecIntervalTimerActivate;

                        //Logging.Logg().Debug(@"HHandlerDbULoader::fTimerActivate () - [ID=" + (_iPlugin as PlugInBase)._Id + @", key=" + pair.Key + @"] - MSecRemaindToActivate=" + pair.Value.MSecRemaindToActivate + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                        if ((pair.Value as GroupSignalsMSTKKSNAMEtoris).MSecRemaindToActivate < 0)
                        {
                            pair.Value.State = GroupSignalsMSTKKSNAMEtoris.STATE.QUEUE;

                            push(pair.Key);
                        }
                        else
                            ;
                    }
                    else
                        ;
            }

            if (!(m_timerActivate == null))
                m_timerActivate.Change(m_msecIntervalTimerActivate, System.Threading.Timeout.Infinite);
            else
                ;
        }

        /// <summary>
        /// Запустить таймер
        ///  для активации групп сигналов
        /// </summary>
        /// <returns></returns>
        private int startTimerActivate()
        {
            int iRes = 0;

            stopTimerActivate();
            m_timerActivate = new System.Threading.Timer(fTimerActivate, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            m_timerActivate.Change(0, System.Threading.Timeout.Infinite);

            return iRes;
        }
        /// <summary>
        /// Остановить таймер активации групп сигналов
        /// </summary>
        /// <returns></returns>
        private int stopTimerActivate()
        {
            int iRes = 0;

            if (!(m_timerActivate == null))
            {
                m_timerActivate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerActivate.Dispose();
                m_timerActivate = null;
            }
            else
                ;

            return iRes;
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
