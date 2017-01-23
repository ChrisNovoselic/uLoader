using System;
using HClassLibrary;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Linq;

namespace xmlLoader
{
    public class PackageHandlerQueue : HClassLibrary.HHandlerQueue
    {
        private static ushort SEC_INTERVAL_OUTDATED = 20;

        /// <summary>
        /// Перечисление - возможные состояния для обработки
        /// </summary>
        public enum StatesMachine
        {
            UNKNOWN = -1
            , NEW // получен новый пакет
            , LIST_PACKAGE // запрос для получения списка пакетов
            , PACKAGE_CONTENT // запрос для получения пакета
            , TIMER_OUTDATED // событие устаревания XML-пакета
            , STATISTIC
        }

        public struct STATISTIC
        {
            public int m_PackageCount;

            public int m_PackageParsedCount;

            public DateTime m_dtLastPackageRecieved;

            public int m_LengthLastPackageRecieved;
        }

        private struct RECORD
        {
            public string m_NameGroup;

            public string m_NameParameter;

            public float m_value;
        }

        public struct PACKAGE
        {
            public enum STATE : short { UNKNOWN = -1
                , NEW, PARSING, PARSED, ERROR
            };

            public DateTime m_dtScore;

            public DateTime m_dtSend;

            public STATE m_state;

            public XmlDocument m_xmlSource;

            public DataTable m_tableParameters;

            public DataTable m_tableValues;

            private int parseNode(XmlNode node)
            {
                int iRes = -1;

                float value = -1F;

                if (node.HasChildNodes == true)
                    if (node.ChildNodes.Count > 0)
                        foreach (XmlNode childNode in node.ChildNodes)
                            parseNode(childNode);
                    else
                        return 1;
                else {
                    try {
                        if (!(node.Attributes == null)) {
                            if (string.IsNullOrEmpty(node.Attributes.GetNamedItem("value").Value) == false)
                                if (float.TryParse(node.Attributes.GetNamedItem("value").Value, out value) == false)
                                    value = -1F;
                                else
                                    ; //значние получено
                            else
                                value = -1F;

                            m_tableValues.Rows.Add(new object[] {
                                node.ParentNode.Name
                                , node.Name
                                , value
                            });

                            iRes = 0;
                        } else
                            ; //???
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, @"PackageHandlerQueue.PACKAGE::parseNode () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }                    

                return iRes;
            }

            public PACKAGE(DateTime dtScore, XmlDocument xmlSource)
            {
                m_dtScore = dtScore;
                m_dtSend = DateTime.MinValue;

                m_state = STATE.NEW;

                m_xmlSource = UDPListener.CopyXmlDocument(xmlSource);

                m_tableParameters = new DataTable();
                m_tableParameters.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"GROUP", typeof(string))
                    , new DataColumn(@"PARAMETER", typeof(string))
                });
                m_tableValues = new DataTable();
                m_tableValues.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"GROUP", typeof(string))
                    , new DataColumn(@"PARAMETER", typeof(string))
                    , new DataColumn(@"VALUE", typeof(float))
                });

                m_state = STATE.PARSING;

                foreach (XmlNode node in m_xmlSource.ChildNodes) {
                    if (node.NodeType == XmlNodeType.Element) {
                        parseNode(node);
                    } else
                        ;
                }

                m_state = STATE.PARSED; //ERROR
            }
        }

        private List<PACKAGE> _listPackage;

        private System.Threading.Timer m_timerOutdated;

        public event DelegateObjectFunc EvtToFormMain;

        public PackageHandlerQueue(short secIntervalOutdated = -1)
        {
            if (secIntervalOutdated > 0)
                SEC_INTERVAL_OUTDATED = (ushort)secIntervalOutdated;
            else
                ;

            _listPackage = new List<PACKAGE>();
            m_timerOutdated = new System.Threading.Timer(timerOutdated_onCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true) {
                m_timerOutdated.Change(active == true ? SEC_INTERVAL_OUTDATED * 1000 : System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            } else
                ;

            return bRes;
        }

        private void timerOutdated_onCallback(object obj)
        {
            Push(null, new object[] {
                new object[] {
                    new object[] {
                        StatesMachine.TIMER_OUTDATED
                    }
                }
            });

            m_timerOutdated.Change(SEC_INTERVAL_OUTDATED * 1000, System.Threading.Timeout.Infinite);
        }

        private List<FormMain.VIEW_PACKAGE_ITEM> listViewPackageItem {
            get {
                List<FormMain.VIEW_PACKAGE_ITEM> listRes = new List<FormMain.VIEW_PACKAGE_ITEM>();

                (from package in _listPackage select new FormMain.VIEW_PACKAGE_ITEM {
                    Values = new object[] {
                        package.m_tableValues.Rows.Count
                        , package.m_dtScore
                        , package.m_dtSend
                    }
                }).ToList().ForEach(item => listRes.Add(item));

                return listRes;
            }
        }
        /// <summary>
        /// Подготовить объект для отправки адресату по его запросу
        /// </summary>
        /// <param name="s">Событие - идентификатор запрашиваемой информации/операции,действия</param>
        /// <param name="error">Признак выполнения операции/действия по запросу</param>
        /// <param name="outobj">Объект для отправления адресату как результат запроса</param>
        /// <returns>Признак выполнения метода (дополнительный)</returns>
        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;
            StatesMachine state = (StatesMachine)s;
            string debugMsg = string.Empty;

            error = true;
            outobj = null;

            ItemQueue itemQueue = null;

            try {
                switch (state) {
                    case StatesMachine.NEW: // новый пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        _listPackage.Add(new PACKAGE((DateTime)itemQueue.Pars[0], (XmlDocument)itemQueue.Pars[1]));
                        break;
                    case StatesMachine.LIST_PACKAGE: // список пакетов
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = listViewPackageItem;
                        break;
                    case StatesMachine.PACKAGE_CONTENT: // пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = from package in _listPackage where package.m_dtScore == (DateTime)itemQueue.Pars[0] select package;
                        break;
                    case StatesMachine.STATISTIC: // статистика
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = new STATISTIC();
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"HHandlerQueue::StateCheckResponse (state=" + state.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

                error = true;
                iRes = -1 * (int)state;
            }

            return iRes;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            switch ((StatesMachine)state) {
                default:
                    break;
            }

            Logging.Logg().Error(@"HHandlerQueue::StateErrors () - не обработана ошибка [" + ((StatesMachine)state).ToString() + @", REQ=" + req + @", RES=" + res + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);

            return HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch ((StatesMachine)state) {
                case StatesMachine.NEW: // 
                case StatesMachine.LIST_PACKAGE: //
                case StatesMachine.PACKAGE_CONTENT: //
                case StatesMachine.STATISTIC: //
                    // не требуют запроса
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            switch ((StatesMachine)state) {
                case StatesMachine.NEW: // 
                    //Ответа не требуется/не требуют обработки результата
                    break;
                case StatesMachine.LIST_PACKAGE: // 
                case StatesMachine.PACKAGE_CONTENT: // 
                case StatesMachine.STATISTIC: // 
                    if ((!(itemQueue == null))
                        //&& (!(itemQueue.m_dataHostRecieved == null)) FormMain не реализует интерфейс 'IDataHost'
                        )
                        //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj })
                        EvtToFormMain(new object [] { state, obj })
                        ;
                    else
                        ;
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(@"HHandlerQueue::StateWarnings () - не обработано предупреждение [" + ((StatesMachine)state).ToString() + @", REQ=" + req + @", RES=" + res + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
