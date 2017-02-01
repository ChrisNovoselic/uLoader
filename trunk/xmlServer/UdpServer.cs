using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace xmlServer
{
    class UdpServer
    {
        private const int SEC_INTERVAL_SERIES = 3;

        private int _counter;

        private TimeSpan m_tsIntervalSeries;

        private XmlDocument m_xmlDoc;

        private UdpClient m_Server;

        private Timer m_timerSend;

        public UdpServer(TimeSpan tsIntervalSeries)
        {
            _counter = 0;

            string folder = @"config"
                , name = @"template"
                , ver = @"1.3.1"
                , ext = @"xml";

            if (tsIntervalSeries.Equals(TimeSpan.Zero) == false)
                m_tsIntervalSeries = tsIntervalSeries;
            else
                m_tsIntervalSeries = TimeSpan.FromSeconds(SEC_INTERVAL_SERIES);

            m_xmlDoc = new XmlDocument();
            m_xmlDoc.Load(string.Format(@"{0}\{1}-{2}.{3}", folder, name, ver, ext));

            m_Server = new UdpClient();
            m_Server.Connect(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 1052));
            m_timerSend = new Timer(new TimerCallback(fTimer_callBack), new object[] { m_Server }, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            m_timerSend.Change(0, (int)m_tsIntervalSeries.TotalMilliseconds);
        }

        public void Stop()
        {
            m_timerSend.Change(Timeout.Infinite, Timeout.Infinite);
            m_timerSend.Dispose();

            m_Server.Close();
        }

        private void fTimer_callBack(object obj)
        {
            UdpClient server = (obj as object[])[0] as UdpClient;
            XmlDocument xmlDoc = xmlLoader.UDPListener.GenerateXmlDocument(m_xmlDoc);

            Console.Write(string.Format(@"{0}Таймер: {1:HH.mm.ss.fff}, сервер: непринятые пакеты={2}"
                , _counter > 0 ? Environment.NewLine : string.Empty
                , DateTime.UtcNow
                , server.Available > 0 ? true : false));

            server.Send(Encoding.ASCII.GetBytes(xmlDoc.InnerXml), xmlDoc.InnerXml.Length);

            _counter++;
        }
    }
}
