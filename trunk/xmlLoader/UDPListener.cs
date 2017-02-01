using HClassLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using uLoaderCommon;

namespace xmlLoader
{
    public class UDPListener : IDataHost
    {
        private class UdpClientAsync : IDisposable
        {
            private readonly IPAddress _hostIpAddress;
            private readonly int _port;
            private readonly Action<UdpReceiveResult> _processor;
            private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
            private CancellationTokenSource _tokenSource = new CancellationTokenSource();
            private CancellationTokenRegistration _tokenReg;
            private UdpClient _udpClient;

            public UdpClientAsync(IPEndPoint server, Action<UdpReceiveResult> processor) : this (server.Address, server.Port, processor)
            {
            }

            public UdpClientAsync(IPAddress hostIpAddress, int port, Action<UdpReceiveResult> processor)
            {
                _hostIpAddress = hostIpAddress;
                _port = port;
                _processor = processor;


            }

            public Task ReceiveAsync()
            {
                // note: there is a race condition here in case of concurrent calls 
                if (_tokenSource != null && _udpClient == null)
                {
                    try
                    {
                        _udpClient = new UdpClient();
                        _udpClient.Connect(_hostIpAddress, _port);
                        _tokenReg = _tokenSource.Token.Register(() => _udpClient.Close());
                        BeginReceive();
                    }
                    catch (Exception ex)
                    {
                        _tcs.SetException(ex);
                        throw;
                    }
                }
                return _tcs.Task;
            }

            public void Stop()
            {
                var cts = Interlocked.Exchange(ref _tokenSource, null);
                if (cts != null)
                {
                    cts.Cancel();
                    if (_tcs != null && _udpClient != null)
                        _tcs.Task.Wait();
                    _tokenReg.Dispose();
                    cts.Dispose();
                }
            }

            public void Dispose()
            {
                Stop();
                if (_udpClient != null)
                {
                    ((IDisposable)_udpClient).Dispose();
                    _udpClient = null;
                }
                GC.SuppressFinalize(this);
            }

            private void BeginReceive()
            {
                var iar = _udpClient.BeginReceive(HandleMessage, null);
                if (iar.CompletedSynchronously)
                    HandleMessage(iar); // if "always" completed sync => stack overflow
            }

            private void HandleMessage(IAsyncResult iar)
            {
                try
                {
                    IPEndPoint remoteEP = null;
                    Byte[] buffer = _udpClient.EndReceive(iar, ref remoteEP);
                    _processor(new UdpReceiveResult(buffer, remoteEP));
                    BeginReceive(); // do the next one
                }
                catch (ObjectDisposedException)
                {
                    // we were canceled, i.e. completed normally
                    _tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    // we failed.
                    _tcs.TrySetException(ex);
                }
            }
        }

        private IPEndPoint m_Server;

        [Flags]
        private enum STATE : short {
            NotSet
            , CONNECT = 0x1
            , DEBUG = 0x2
            , XML_VERSION = 0x4
            , XML_TEMPLATE = 0x8
        }

        private struct SETUP_DEBUG
        {
            public bool m_Turn;

            public TimeSpan m_tsIntervalSeries;
        }

        private SETUP_DEBUG m_SetupDebug;

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

        //private BackgroundWorker m_threadRecived;

        private static XmlDocument s_packageTemplate;

        private Timer m_debugTmerSeries;

        private static string _versionXMLPackage;

        public UDPListener()
        {
            m_debugTmerSeries = new Timer(fDebugTimerSeries_CallBack, null, Timeout.Infinite, Timeout.Infinite);

            //m_threadRecived = new BackgroundWorker();
            //m_threadRecived.WorkerSupportsCancellation = true;
            //m_threadRecived.DoWork += fThreadRecived_DoWork;

            m_Server = null;
            _versionXMLPackage = string.Empty;
        }

        //private void fThreadRecived_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    IPEndPoint remEP = null;
        //    byte[] resRecieved;
            
        //    using (UdpClient udpClient = new UdpClient(m_Server))
        //    {
        //        while (true) {
        //            resRecieved = udpClient.Receive(ref remEP);
        //        }
        //    };
        //}

        /// <summary>
        /// Реализация интерфейса 'IDataHost' - событие для отправления сообщения
        /// </summary>
        public event DelegateObjectFunc EvtDataAskedHost;
        /// <summary>
        /// Событие для изменения 
        /// </summary>
        private Action evtStateChanged;

        private Action evtConnectedChanged;

        public void Start()
        {
            int iErr = 0;

            if ((m_Server == null)
                || (_versionXMLPackage.Equals(string.Empty) == true))
            // запросить номер порта, шаблон пакета(м номером версии)
                DataAskedHost(new object[] {
                    new object[] { HHandlerQueue.StatesMachine.UDP_LISTENER }
                    , new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION }
                    , new object[] { HHandlerQueue.StatesMachine.UDP_DEBUG }
                    //, new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_TEMPLATE } после получения версии
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
                && ((state & UDPListener.STATE.XML_VERSION) == UDPListener.STATE.XML_VERSION)
                && ((state & UDPListener.STATE.XML_TEMPLATE) == UDPListener.STATE.XML_TEMPLATE)
                && ((state & UDPListener.STATE.DEBUG) == UDPListener.STATE.DEBUG);
        }
        /// <summary>
        /// Обработчик события - изменение состояния соединения (установлено/разорвано)
        /// </summary>
        private void onEvtConnectedChanged()
        {
            DataAskedHost(new object[] { new object [] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGED
                , IsConnected
                , m_SetupDebug.m_Turn
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
            if (m_SetupDebug.m_Turn == true)
                m_debugTmerSeries.Change(0, Timeout.Infinite);
            else
                Logging.Logg().Warning(MethodBase.GetCurrentMethod(), @"Генерация Xml-пакета отменена - настройки отладки...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Метод отладки - запуск генерации серии пакетов
        /// </summary>
        /// <param name="secInterval">Интервал(сек) между очередными итерациями генерации серии пакетов</param>
        public void DebugStartSeriesEventPackageRecieved()
        {
            string debugMsg = @"СТАРТ генерации серии XML-пакетов";

            debugMsg += string.Format(@"{0}{1}", debugMsg, m_SetupDebug.m_Turn == true ? string.Empty : @" - отменена...");

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            if (m_SetupDebug.m_Turn == true)
                m_debugTmerSeries.Change(0, (int)m_SetupDebug.m_tsIntervalSeries.TotalMilliseconds);
            else
                ;
        }
        /// <summary>
        /// Метод отладки - останов процесса генерации серии пакетов
        /// </summary>
        public void DebugStopSeriesEventPackageRecieved()
        {
            string debugMsg = @"СТОП генерации серии XML-пакетов";

            debugMsg += string.Format(@"{0}{1}", debugMsg, m_SetupDebug.m_Turn == true ? string.Empty : @" - отменена...");

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));

            m_debugTmerSeries.Change(Timeout.Infinite, Timeout.Infinite);
        }
        /// <summary>
        /// Метод обратного вызова для таймера генерации серии пакетов
        /// </summary>
        /// <param name="obj">Аргумент при вызове метода</param>
        private void fDebugTimerSeries_CallBack(object obj)
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
        /// <summary>
        /// Генерировать Xml-пакет из шаблона в режиме отладке
        /// </summary>
        /// <returns>Объект один из серии пакетов</returns>
        private XmlDocument debugGeneratePackageRecieved()
        {
            return CopyXmlDocument(s_packageTemplate);
        }
        /// <summary>
        /// Копировать XML-документ
        /// </summary>
        /// <param name="source">Источник для копирования</param>
        /// <returns>Копия аргумента</returns>
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
        /// <summary>
        /// Копировать XML-документ
        /// </summary>
        /// <param name="source">Источник для копирования</param>
        /// <returns>Копия аргумента</returns>
        public static XmlDocument GenerateXmlDocument(XmlDocument source)
        {
            string[] patterns = { @"TIME", @"DATETIME", @"VALUE_FLOAT", @"VALUE_INT" };
            string occurence = string.Empty;
            int indx = -1;

            XmlDocument xmlDocRes = CopyXmlDocument(source);

            foreach (string pattern in patterns) {
                occurence = string.Format(@"?{0}?", pattern);

                indx = 0;
                while (true) {
                    indx = xmlDocRes.InnerXml.IndexOf(occurence, indx);

                    if (!(indx < 0)) {
                        switch (pattern) {
                            case @"TIME":
                                xmlDocRes.InnerXml = xmlDocRes.InnerXml.Replace(occurence, DateTime.UtcNow.ToString(@"HH:mm:ss.fff"));
                                break;
                            case @"DATETIME":
                                xmlDocRes.InnerXml = xmlDocRes.InnerXml.Replace(occurence, DateTime.UtcNow.ToString());
                                break;
                            case @"VALUE_FLOAT":
                                xmlDocRes.InnerXml = xmlDocRes.InnerXml.Remove(indx, occurence.Length);
                                xmlDocRes.InnerXml = xmlDocRes.InnerXml.Insert(indx, string.Format(@"{0:F2}", HMath.GetRandomNumber(5, 655)));
                                break;
                            case @"VALUE_INT":
                                xmlDocRes.InnerXml = xmlDocRes.InnerXml.Remove(indx, occurence.Length);
                                xmlDocRes.InnerXml = xmlDocRes.InnerXml.Insert(indx, string.Format(@"{0}", HMath.GetRandomNumber(656, 998)));
                                break;
                            default:
                                break;
                        }
                    } else
                        break;
                }
            }

            return xmlDocRes;
        }
        /// <summary>
        /// Реализация интерфейса 'IDataHost' - отправить сообщение
        /// </summary>
        /// <param name="par">Объект для передачи</param>
        public void DataAskedHost(object par)
        {
            EvtDataAskedHost?.Invoke(par);
        }
        /// <summary>
        /// Реализация интерфейса 'IDataHost' - принять сообщение
        /// </summary>
        /// <param name="res">Объект с принятым сообщением</param>
        public void OnEvtDataRecievedHost(object res)
        {
            HHandlerQueue.StatesMachine stateMashine = (HHandlerQueue.StatesMachine)(res as object[])[0];

            try {
                switch (stateMashine) {
                    case HHandlerQueue.StatesMachine.UDP_DEBUG:
                        m_SetupDebug.m_Turn = bool.Parse((string)((res as object[])[1] as object[])[0]);
                        m_SetupDebug.m_tsIntervalSeries = new HTimeSpan((string)((res as object[])[1] as object[])[1]).Value;

                        state |= STATE.DEBUG;
                        break;
                    case HHandlerQueue.StatesMachine.UDP_LISTENER:
                        m_Server = new IPEndPoint(IPAddress.Parse((string)((res as object[])[1] as object[])[0])
                            , (int)((res as object[])[1] as object[])[1]);
                        break;
                    case HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE:
                        if ((bool)(res as object[])[1] == true)
                            connect();
                        else
                            disconnect();
                        break;
                    case HHandlerQueue.StatesMachine.XML_PACKAGE_VERSION:
                        _versionXMLPackage = (string)(res as object[])[1];

                        DataAskedHost(new object[] {
                            new object[] { HHandlerQueue.StatesMachine.XML_PACKAGE_TEMPLATE, _versionXMLPackage }
                        });

                        state |= STATE.XML_VERSION;
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

        private UdpClientAsync m_udpClient;
        /// <summary>
        /// Установить соединение
        /// </summary>
        /// <returns>Результат выполнения метода</returns>
        private int connect()
        {
            int iRes = 0;

            if (((m_Server.Address.Equals(IPAddress.None) == false)
                    && (m_Server.Address.Equals(IPAddress.Any) == false))
                && (m_Server.Port > 0)) {
                m_udpClient = new UdpClientAsync(m_Server, recieve_callBack);

                m_udpClient.
                    //BeginReceive(new AsyncCallback(recieve_callBack), new object[] { m_udpClient, m_Server })
                    ReceiveAsync()
                    ;

                state |= STATE.CONNECT;
            } else
                ;

            return iRes;
        }

        private void recieve_callBack(UdpReceiveResult res)
        {
        }

        private void recieve_callBack(IAsyncResult iar)
        {
            Byte[] receiveBytes = null;
            string strReceived = string.Empty;
            XmlDocument xmlDoc;

            UdpClient udpClient;
            IPEndPoint server;

            lock (this) {
                udpClient = (iar.AsyncState as object[])[0] as UdpClient;
                server = (iar.AsyncState as object[])[1] as IPEndPoint;

                if (!(udpClient.Client == null)) {
                    receiveBytes = udpClient.EndReceive(iar, ref server);
                    strReceived = Encoding.ASCII.GetString(receiveBytes);

                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(strReceived);

                    // отправить XML-пакет
                    DataAskedHost(new object[] { new object[] {
                        HHandlerQueue.StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED
                        , DateTime.UtcNow
                        , xmlDoc }
                    });

                    m_udpClient.
                        //BeginReceive(new AsyncCallback(recieve_callBack), new object[] { udpClient, server })
                        ReceiveAsync()
                        ;
                } else
                    ;
            }
        }
        /// <summary>
        /// Разорвать соединение с сервером - источником XML-пакетом
        /// </summary>
        private void disconnect()
        {
            if (!(m_udpClient == null)) {
                m_udpClient.Dispose();
                m_udpClient = null;
            } else
                ;

            state -= STATE.CONNECT;
        }
    }
}
