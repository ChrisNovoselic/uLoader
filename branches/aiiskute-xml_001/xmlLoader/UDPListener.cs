using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace xmlLoader
{
    class UDPListener : IDataHost
    {
        private const int SEC_INTERVAL_SERIES_EVENT_PACKAGE_RECIEVED = 10;

        private static int m_iNPort;

        [Flags]
        private enum STATE : short {
            NotSet
            , CONNECT = 0x1
            , XML_TEMPLATE = 0x2
        }

        private STATE _state;

        private STATE state {
            get { return _state; }

            set {
                _state = value;

                evtStateChanged?.Invoke();
            }
        }

        private bool _connected;

        private bool IsConnected
        {
            get { return _connected; }

            set {
                if (!(_connected == value)) {
                    _connected = value;

                    evtConnectedChanged.Invoke();
                } else
                    ;
            }
        }

        private static XmlDocument s_packageTemplate;

        private Timer m_timerSeries;

        private static string _versionXMLPackage;

        public UDPListener()
        {
            m_timerSeries = new Timer(timerSeries_CallBack, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event DelegateObjectFunc EvtDataAskedHost;

        private event DelegateFunc evtStateChanged;

        private event DelegateFunc evtConnectedChanged;

        public void Start()
        {
            // запросить номер порта, шаблон пакета(м номером версии)
            DataAskedHost(new object[] {
                new object[] { HHandlerQueue.StatesMachine.NUDP_LISTENER }
                , new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION }
            });

            evtStateChanged += onEvtStateChanged;
            evtConnectedChanged += onEvtConnectedChanged;
        }

        private void onEvtStateChanged()
        {
            IsConnected = ((state & UDPListener.STATE.CONNECT) == UDPListener.STATE.CONNECT)
                && ((state & UDPListener.STATE.XML_TEMPLATE) == UDPListener.STATE.XML_TEMPLATE);
        }

        private void onEvtConnectedChanged()
        {
            DataAskedHost(new object[] { new object [] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGED
                , IsConnected
            }});
        }

        public void Stop()
        {
        }

        public void GenerateEventPackageRecieved()
        {
            m_timerSeries.Change(0, Timeout.Infinite);
        }

        public void StartSeriesEventPackageRecieved(int secInterval = SEC_INTERVAL_SERIES_EVENT_PACKAGE_RECIEVED)
        {
            string debugMsg = @"СТАРТ генерации серии XML-пакетов";

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            m_timerSeries.Change(0, secInterval * 1000);
        }

        public void StopSeriesEventPackageRecieved()
        {
            string debugMsg = @"СТОП генерации серии XML-пакетов";

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            m_timerSeries.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void timerSeries_CallBack(object obj)
        {
            string debugMsg = @"сгенерирован XML-пакет";
            XmlDocument xmlDoc;

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            // сформировать XML-пакет - заменить метки даты/времени, значения
            xmlDoc = generatePackageRecieved();

            // отправить XML-пакет
            DataAskedHost(new object[] { new object[] {
                HHandlerQueue.StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED
                , DateTime.UtcNow
                , xmlDoc }
            });
        }

        private XmlDocument generatePackageRecieved()
        {
            return CopyXmlDocument(s_packageTemplate);
        }

        public static XmlDocument CopyXmlDocument(XmlDocument source)
        {
            XmlDocument xmlDocRes = new XmlDocument();

            foreach (XmlNode node in source.ChildNodes) {
                //if (!(node.NodeType == XmlNodeType.XmlDeclaration))
                xmlDocRes.AppendChild(xmlDocRes.ImportNode(node, true));
                //else
                //    ;

                //newNode.InnerXml = node.InnerXml;
            }

            return xmlDocRes;
        }

        public void DataAskedHost(object par)
        {
            EvtDataAskedHost?.Invoke(par);
        }

        public void OnEvtDataRecievedHost(object res)
        {
            HHandlerQueue.StatesMachine stateMashine = (HHandlerQueue.StatesMachine)(res as object[])[0];

            try {
                switch (stateMashine) {
                    case HHandlerQueue.StatesMachine.NUDP_LISTENER:
                        m_iNPort = (int)(res as object[])[1];
                        break;
                    case HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE:
                        if ((bool)(res as object[])[1] == true)
                            connect();
                        else
                            disconnect();
                        break;
                    case HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION:
                        _versionXMLPackage = (string)(res as object[])[1];
                        // запросить номер порта, шаблон пакета(м номером версии)
                        DataAskedHost(new object[] {
                            new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_TEMPLATE, _versionXMLPackage }
                        });
                        break;
                    case HHandlerQueue.StatesMachine.XML_PACKAGE_TEMPLATE:
                        s_packageTemplate = (XmlDocument)(res as object[])[1];

                        state |= STATE.XML_TEMPLATE;
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e
                    , string.Format(@"Обработка принятого сообщения по интерфесу 'IDataHost' событие={0}...", state.ToString())
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private int connect()
        {
            int iRes = 0;

            if (m_iNPort > 0)
                state |= STATE.CONNECT
                    ;
            else
                ;

            return iRes;
        }

        private void disconnect()
        {
            state -= STATE.CONNECT;
        }
    }
}
