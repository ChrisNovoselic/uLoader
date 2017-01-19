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

        private static string s_packageTemplate;

        private static string PackageTemplate { get; }

        private Timer m_timerSeries;

        private static string _versionXMLPackage;

        public UDPListener()
        {
            m_timerSeries = new Timer(timerSeries_CallBack, null, Timeout.Infinite, Timeout.Infinite);            
        }

        public event DelegateObjectFunc EvtDataAskedHost;

        public event DelegateObjectFunc EvtStateChanged;

        public void Start()
        {
            // запросить номер порта, шаблон пакета(м номером версии)
            DataAskedHost(new object[] {
                new object[] { HHandlerQueue.StatesMachine.NUDP_LISTENER }
                , new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION }
            });
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
            DataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.NUDP_LISTENER, PackageTemplate } });
        }

        public void DataAskedHost(object par)
        {
            EvtDataAskedHost?.Invoke(par);
        }

        public void OnEvtDataRecievedHost(object res)
        {
            HHandlerQueue.StatesMachine state = (HHandlerQueue.StatesMachine)(res as object[])[0];

            switch (state) {
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
                    s_packageTemplate = (string)(res as object[])[1];
                    break;
                default:
                    break;
            }
        }

        private int connect(int iPort)
        {
            int iRes = 0;

            return iRes;
        }
    }
}
