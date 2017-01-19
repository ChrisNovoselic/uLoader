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

        private static XmlDocument s_packageTemplate;

        private static string PackageTemplate { get; }

        private Timer m_timerSeries;

        private static string _versionXMLPackage;

        public UDPListener()
        {
            m_timerSeries = new Timer(timerSeries_CallBack, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event DelegateObjectFunc EvtDataAskedHost;

        private event DelegateFunc evtStateChanged;

        public void Start()
        {
            // запросить номер порта, шаблон пакета(м номером версии)
            DataAskedHost(new object[] {
                new object[] { HHandlerQueue.StatesMachine.NUDP_LISTENER }
                , new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION }
            });

            evtStateChanged += onEvtStateChanged;
        }

        private void onEvtStateChanged()
        {
            DataAskedHost (new object[] { new object [] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGED
                , ((state & UDPListener.STATE.CONNECT) == UDPListener.STATE.CONNECT)
                    && ((state & UDPListener.STATE.XML_TEMPLATE) == UDPListener.STATE.XML_TEMPLATE)
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

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            XmlDocument xmlDoc;

            // сформировать XML-пакет

            // отправить XML-пакет
            DataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED, PackageTemplate } });
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
                        connect((int)(res as object[])[1]);
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

        private int connect(int iPort)
        {
            int iRes = 0;

            if (iRes == 0)
                state |= STATE.CONNECT
                    ;
            else
                ;

            return iRes;
        }
    }
}
