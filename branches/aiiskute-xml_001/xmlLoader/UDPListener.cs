﻿using HClassLibrary;
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
        private const int SEC_INTERVAL_SERIES_EVENT_PACKAGE_RECIEVED = 3;

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

        private Timer m_debugTmerSeries;

        private static string _versionXMLPackage;

        public UDPListener()
        {
            m_debugTmerSeries = new Timer(debugTimerSeries_CallBack, null, Timeout.Infinite, Timeout.Infinite);

            m_iNPort = -1;
            _versionXMLPackage = string.Empty;
        }

        public event DelegateObjectFunc EvtDataAskedHost;

        private DelegateFunc evtStateChanged;

        private DelegateFunc evtConnectedChanged;

        public void Start()
        {
            int iErr = 0;

            if ((!(m_iNPort > 0))
                && (_versionXMLPackage.Equals(string.Empty) == true))
            // запросить номер порта, шаблон пакета(м номером версии)
                DataAskedHost(new object[] {
                    new object[] { HHandlerQueue.StatesMachine.NUDP_LISTENER }
                    , new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION }
                });
            else
                iErr --; //??? - ошибка (повторный старт)

            if (evtStateChanged == null)
                evtStateChanged += onEvtStateChanged;
            else
                iErr--; //??? - ошибка (повторный старт)

            if (evtConnectedChanged == null)
                evtConnectedChanged += onEvtConnectedChanged;
            else
                iErr--; //??? - ошибка (повторный старт)

            if (iErr < 0)
                throw new Exception(@"Повторный старт объекта-прослушивателя XML-пакетов...");
            else
                ;
        }
        /// <summary>
        /// Обработчик события - изменение состояния объекта
        /// </summary>
        private void onEvtStateChanged()
        {
            IsConnected = ((state & UDPListener.STATE.CONNECT) == UDPListener.STATE.CONNECT)
                && ((state & UDPListener.STATE.XML_TEMPLATE) == UDPListener.STATE.XML_TEMPLATE);
        }
        /// <summary>
        /// Обработчик события - изменение состояния соединения (установлено/разорвано)
        /// </summary>
        private void onEvtConnectedChanged()
        {
            DataAskedHost(new object[] { new object [] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGED
                , IsConnected
            }});
        }

        public void Stop()
        {
            m_debugTmerSeries.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            m_debugTmerSeries.Dispose();
        }
        /// <summary>
        /// Метод отладки - инициировать очередную итерацию генерации серии пакетов
        /// </summary>
        public void DebugGenerateEventPackageRecieved()
        {
            m_debugTmerSeries.Change(0, Timeout.Infinite);
        }
        /// <summary>
        /// Метод отладки - запуск генерации серии пакетов
        /// </summary>
        /// <param name="secInterval">Интервал(сек) между очередными итерациями генерации серии пакетов</param>
        public void DebugStartSeriesEventPackageRecieved(int secInterval = SEC_INTERVAL_SERIES_EVENT_PACKAGE_RECIEVED)
        {
            string debugMsg = @"СТАРТ генерации серии XML-пакетов";

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            m_debugTmerSeries.Change(0, secInterval * 1000);
        }

        public void DebugStopSeriesEventPackageRecieved()
        {
            string debugMsg = @"СТОП генерации серии XML-пакетов";

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            m_debugTmerSeries.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void debugTimerSeries_CallBack(object obj)
        {
            string debugMsg = @"сгенерирован XML-пакет";
            XmlDocument xmlDoc;

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            // сформировать XML-пакет - заменить метки даты/времени, значения
            xmlDoc = debugGeneratePackageRecieved();

            // отправить XML-пакет
            DataAskedHost(new object[] { new object[] {
                HHandlerQueue.StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED
                , DateTime.UtcNow
                , xmlDoc }
            });
        }

        private XmlDocument debugGeneratePackageRecieved()
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
