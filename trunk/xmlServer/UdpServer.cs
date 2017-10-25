using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using uLoaderCommon;

namespace xmlServer
{
    class HCmd_Arg : ASUTP.Helper.HCmd_Arg
    {
        private MAIN_ARG[] _args;

        public int Length { get { return m_dictCmdArgs.Count; } }

        public object ElementAt(string name)
        {
            object objRes = null;

            foreach(MAIN_ARG arg in _args)
                if (arg.m_Name.Equals(name) == true) {
                    objRes = arg.m_Value;

                    break;
                } else
                    ;

            return objRes;
        }

        public string[] List { get; }

        private struct MAIN_ARG
        {
            public static string delimMainArg = string.Format(@"{0}", ' ');

            public static string prefixMainArg = string.Format(@"{0}", @"/");

            public string m_Name;

            public object m_defaultValue;

            public object m_Value;

            public int m_State;
        }

        public HCmd_Arg() : base(Environment.GetCommandLineArgs())
        {
            _args = new MAIN_ARG[] {
                new MAIN_ARG() { m_Name = @"dest", m_defaultValue = new IPAddress(new byte[] { 127, 0, 0, 1 }), m_State = -1, m_Value = null }
                , new MAIN_ARG() { m_Name = @"period", m_defaultValue = TimeSpan.FromSeconds(3), m_State = -1, m_Value = null  }
            };

            for (int i = 0; i < _args.Length; i++)
                try {
                    if (m_dictCmdArgs.ContainsKey(_args[i].m_Name) == true) {
                        switch (_args[i].m_Name) {
                            case @"dest":
                                _args[i].m_Value = IPAddress.Parse(m_dictCmdArgs[_args[i].m_Name]);
                                break;
                            case @"period":
                                _args[i].m_Value = new HTimeSpan(m_dictCmdArgs[_args[i].m_Name]).Value;
                                break;
                            default:
                                break;
                        }
                        // присваивание по значению
                        _args[i].m_State = 0;
                    } else {
                        _args[i].m_Value = _args[i].m_defaultValue;
                        // присваивание по умолчанию
                        _args[i].m_State = 1;
                    }
                } catch (Exception e) {
                    Console.WriteLine(string.Format(@"Исключение при обработке аргумента {1}{0}{2}", Environment.NewLine, _args[i].m_Name, e.Message));

                    _args[i].m_Value = _args[i].m_defaultValue;
                    // присваивание по исключение
                    _args[i].m_State = -2;
                }
        }
    }

    class UdpServer
    {
        private int _counter;

        private TimeSpan m_tsIntervalSeries;

        private XmlDocument m_xmlDoc;

        private UdpClient m_Server;

        private const int PORT = 1052;

        private Timer m_timerSend;

        public UdpServer(IPAddress ip, TimeSpan tsIntervalSeries)
        {
            _counter = 0;
            m_tsIntervalSeries = tsIntervalSeries;

            string folder = @"config"
                , name = @"template"
                , ver = @"1.3.1"
                , ext = @"xml"
                , xmlPath = string.Format(@"{0}\{1}-{2}.{3}", folder, name, ver, ext);

            Console.WriteLine(string.Format(@"Создание объекта [IP={0}, порт={1}, период={2}, шаблон={3}]..."
                , ip.ToString()
                , PORT
                , tsIntervalSeries.ToString()                
                , xmlPath
            ));

            m_xmlDoc = new XmlDocument();
            m_xmlDoc.Load(xmlPath);

            m_Server = new UdpClient();
            m_Server.Connect(new IPEndPoint(ip, PORT));
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
