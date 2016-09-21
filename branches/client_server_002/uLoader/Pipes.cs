using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Diagnostics;
using System.IO.Pipes;

using HClassLibrary;

namespace uLoader.Pipes
{
    public interface IPipe
    {
        void Start();

        void Stop();
    }

    public abstract class Pipe
    {
        /// <summary>
        /// Комманды к клиенту/серверу
        /// </summary>
        public enum COMMAND
        {
            Unknown = -1
                , Connect, Disconnect
                , DateTime, Name, PipeRole /*Server|Client*/, AppState /*Started|Paused*/
                , Start, Stop, ReConnect, Exit
            , Count
        };

        protected object _stream;

        /// <summary>
        /// Класс для описания аргумента события
        /// </summary>
        public class ConnectionClientEventArgs : EventArgs
        {
            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public string IdServer;

            public ConnectionClientEventArgs(string id_serv)
            {
                IdServer = id_serv;
            }
        }

        /// <summary>
        /// Класс для описания аргумента события
        /// </summary>
        public class ReadMessageEventArgs : ConnectionClientEventArgs
        {
            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public string Value;

            public ReadMessageEventArgs(string value, string id_serv)
                : base(id_serv)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Класс для описания аргумента события
        /// </summary>
        public class ResServEventArgs : ConnectionClientEventArgs
        {
            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public bool Value;

            public ResServEventArgs(string id_serv)
                : base(id_serv)
            {
                Value = true;
            }
        }

        protected static string NAME_MAINPIPE = @"MainPipe";

        public static string /*MESSAGE_RECIEVED_OK = @"OK",*/
            MESSAGE_CLIENT_OFFLINE = "Клиент отключен..."
            , MESSAGE_UNRECOGNIZED_COMMAND = @"Non command";

        public static Char
            //DELIMETER_MESSAGE_COMMAND = ';',
            DELIMETER_MESSAGE_KEYVALUEPAIR = '='
            ;

        protected static int MAX_ATTEMPT_READ_PIPE = 50;
        /// <summary>
        /// Период ожидания завершения потока(милисекунды), повышающий и понижающий коэффициенты
        /// </summary>
        public enum TIMEOUT { NORMAL = 1000, RAISING = 5, REDUSING = 5 };
        /// <summary>
        /// Имя сервера
        /// </summary>
        public string m_Name;
        /// <summary>
        /// Экземпляр потока клиента для чтения сообщений
        /// </summary>
        protected Thread _thread;
        /// <summary>
        /// Флаг для остановки потока
        /// </summary>
        protected bool m_bStopThread;

        protected abstract void fThread(object data);
    }

    public class Server : Pipe, IPipe
    {
        public enum ERROR { OVER_ATTEMPT = -2, ANY = -1, NO, DISCONNECT }

        /// <summary>
        /// Экземпляр главного распределяющего потока
        /// </summary>
        private MainPipeStream m_mainStream { get { return _stream as MainPipeStream; } }

        /// <summary>
        /// Конструктор
        /// </summary>
        public Server()
        {
            m_Name = Environment.MachineName;
            //Новый экземпляр потока для сервера
            _thread = new Thread(fThread);
        }

        /// <summary>
        /// Старт сервера
        /// </summary>
        public void Start()
        {
            m_bStopThread = false;//инициализация переменной
            _thread.Start();//старт потока
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void Stop()
        {
            m_bStopThread = true;//Изменение зн-я флага для завершения работы потока

            try
            {
                if (_thread != null)
                {
                    if (_thread.Join((int)TIMEOUT.NORMAL) == false)//Ожидаем секунду, Если не завершился
                    {
                        _thread.Join((int)((int)TIMEOUT.NORMAL / (float)TIMEOUT.REDUSING));
                        _thread.Interrupt();
                        if (_thread.ThreadState == System.Threading.ThreadState.Running)
                            _thread.Abort();
                        _thread = null;
                    }
                }

                if (m_mainStream != null)
                    m_mainStream.Stop();
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"Pipes.Server::StopServer () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Метод для потока сервера
        /// </summary>
        /// <param name="data">Параметр при старте потоковой функции</param>
        protected override void fThread(object data)
        {
            try
            {
                _stream = new MainPipeStream(NAME_MAINPIPE);//Новый экземпляр именованного канала, передаём его имя

                //Подписка на события
                m_mainStream.ReadMessage += new EventHandler(readMessage);
                m_mainStream.ResServ += new EventHandler(resServ);
                m_mainStream.ConnectClient += new EventHandler(event_connectClient);
                m_mainStream.DisConnectClient += new EventHandler(event_disConnectClient);

                m_mainStream.Start();//Старт канала
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"Pipes.Server::ServerThread () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Обрабочик получения нового сообщения от клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readMessage(object sender, EventArgs e)
        {
            if (!(ReadMessage == null)) ReadMessage(this, (e as Pipes.Pipe.ReadMessageEventArgs)); else ;
        }

        /// <summary>
        /// Отправка сообщения клиенту
        /// </summary>
        /// <param name="idClient">Идентификатор клиента(имяПК_ИДпроцесса)</param>
        /// <param name="message">Сообщение</param>
        public void WriteMessage(string idClient, string message)
        {
            m_mainStream.WriteMessage(idClient, message);
        }

        /// <summary>
        /// Обработчик перезапуска главного канала
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resServ(object sender, EventArgs e)
        {//StreamPipe.ResServEventArgs
            m_mainStream.RestartPipe();
        }

        /// <summary>
        /// Обработчик события получения нового подключенного клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void event_connectClient(object sender, EventArgs e)
        {//MainStreamPipe.ConnectClientEventArgs
            if (!(ConnectClient == null)) ConnectClient(this, e as Pipes.Server.ConnectionClientEventArgs); else ;
        }

        /// <summary>
        /// Обработчик события отключенного клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void event_disConnectClient(object sender, EventArgs e)
        {
            if (!(DisConnectClient == null)) DisConnectClient(this, e); else ;
        }

        /// <summary>
        /// Разорвать соединения со всеми клиентами
        /// </summary>
        public void SendDisconnect()
        {
            m_mainStream.SendDisconnect();
        }


        /// <summary>
        /// Класс для работы с именованным каналом
        /// </summary>
        private class PipeStream
        {
            /// <summary>
            /// Экземпляр канала
            /// </summary>
            protected NamedPipeServerStream m_pipeStream;

            /// <summary>
            /// Экземпляр класса по чтению и записи данных в канале
            /// </summary>
            protected StreamString m_ss;

            /// <summary>
            /// Флаг ошибки работы сервера
            /// </summary>
            protected ERROR m_ErrServ = 0;

            /// <summary>
            /// Имя канала
            /// </summary>
            public string m_NamePipe;

            /// <summary>
            /// Экземпляр потока
            /// </summary>
            protected Thread m_thread;

            /// <summary>
            /// Флаг для остановки работы потока
            /// </summary>
            protected bool m_bStopThread;

            /// <summary>
            /// Конструктор основной
            /// </summary>
            /// <param name="name">Имя канала</param>
            public PipeStream(string name)
            {
                m_NamePipe = name;
            }

            /// <summary>
            /// Запуск ожидания подключения
            /// </summary>
            protected void waitConnect(out int err)
            {
                err = 0;
                try
                {
                    //m_pipeServer.WaitForConnection();//Ожидание подключения
                    m_pipeStream.BeginWaitForConnection(callbackConnection, null);
                }
                catch (Exception e)//Если таймаут превышен
                {
                    err = 1;
                }
            }

            private void callbackConnection(IAsyncResult res)
            {
                if (!(m_pipeStream == null))
                {
                    m_pipeStream.EndWaitForConnection(res);

                    if (m_pipeStream.IsConnected == true)
                    {
                        m_thread = new Thread(ThreadRead);//Новый поток работы канала
                        m_thread.Start();//Старт потока
                    }
                    else
                        ;
                }
                else
                    ;
            }

            /// <summary>
            /// Запуск канала
            /// </summary>
            public void Start()
            {
                //Инициализация переменных
                int err = 0;
                m_bStopThread = false;
                m_ErrServ = 0;

                //Инициализация экземпляра канала
                m_pipeStream = new NamedPipeServerStream(m_NamePipe
                    , PipeDirection.InOut
                    , 1
                    , PipeTransmissionMode.Message
                    , PipeOptions.Asynchronous
                    , 1024 * 1024, 1024 * 1024);

                //Инициализация экземпляра класса по работе с каналом
                m_ss = new StreamString(m_pipeStream);

                waitConnect(out err);//Ожидание подключения

                //if (err == 0)//Если ошибок нет, соединение установлено
                //{
                //    m_thread = new Thread(ThreadRead);//Новый поток работы канала
                //    m_thread.Start();//Старт потока
                //}
            }

            /// <summary>
            /// Остановка канала
            /// </summary>
            public virtual void Stop()
            {
                m_bStopThread = true;

                try
                {
                    if (!(m_pipeStream == null))
                    {
                        if (m_pipeStream.IsConnected == true)//Если соединение установлено то
                            m_pipeStream.Disconnect();//Разрываем соединение
                        m_pipeStream.Close();//Закрываем канал
                        m_pipeStream.Dispose();//Уничтожаем ресурсы
                    }
                    else
                        Logging.Logg().Error(@"::StopPipe () - объект канала сервера =NULL", Logging.INDEX_MESSAGE.NOT_SET);
                    m_pipeStream = null;//Обнуляем

                    if (m_thread != null)
                    {
                        if (m_thread.Join((int)((int)TIMEOUT.NORMAL * (int)TIMEOUT.RAISING)) == false)//Ожидаем секунду, Если не завершился
                        {
                            try
                            {
                                m_thread.Abort();//То завершаем принудительно
                            }
                            catch (ThreadAbortException e)
                            {
                                m_thread.Join((int)((int)TIMEOUT.NORMAL * (float)TIMEOUT.RAISING / (int)TIMEOUT.REDUSING));
                                m_thread.Interrupt();
                                m_thread = null;
                                Thread.ResetAbort();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"Pipes.Server::StopPipe () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Перезапуск канала
            /// </summary>
            public virtual void RestartPipe()
            {
                if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(MESSAGE_CLIENT_OFFLINE, m_NamePipe)); else ;
                m_pipeStream.Close();//Закрываем канал
                m_pipeStream = null;
                Start();//Запускаем канал
            }

            /// <summary>
            /// Метод чтения данных из канала в потоке
            /// </summary>
            /// <param name="data"></param>
            protected virtual void ThreadRead(object data)
            {
                int err = 0;

                string[] main_com = null
                    //???, message = null
                    ;
                int errRead = 0;
                string stat = string.Empty;

                while (m_ErrServ == 0)//Ошибок при чтении
                {
                    errRead = 0;

                    stat = string.Empty;
                    try
                    {
                        stat = m_ss.ReadString(out errRead);//Чтение из канала
                    }
                    catch (Exception e)
                    {
                        m_ErrServ = ERROR.ANY; //Ошибка работы канала

                        Logging.Logg().Exception(e, @"Pipes.Server.streamPipe::ThreadRead () - ошибка работы канала...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    if (m_ErrServ == 0)//если ошибок нет
                        if (m_pipeStream != null)
                        {
                            if (m_pipeStream.IsConnected == true)
                            {
                                if (errRead > 0)//Проверка на ошибки при чтении
                                {
                                    err++;
                                }
                                else
                                {
                                    err = 0;
                                    //main_com = stat.Split(DELIMETER_MESSAGE_COMMAND);//разбор сложного сообщения
                                    main_com = stat.Split(DELIMETER_MESSAGE_KEYVALUEPAIR);//разбор сложного сообщения
                                    //message = new string[0];
                                    if (main_com[0].Equals(COMMAND.Disconnect.ToString()) == true)//переход по служебным сообщениям
                                    {
                                        //message = main_com[1].Split(DELIMETER_MESSAGE_KEYVALUEPAIR);
                                        m_ErrServ = ERROR.DISCONNECT;
                                    }
                                    else
                                        ;

                                    if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_NamePipe)); else ;//Вызов события передачи сообщения
                                }
                                if (m_bStopThread == true)//Был ли изменен флаг остановки потока
                                {
                                    break;//Прерываем цикл
                                }
                                if (!(err < MAX_ATTEMPT_READ_PIPE))
                                    m_ErrServ = ERROR.OVER_ATTEMPT;
                            }
                        }
                        else
                        {
                            break;//прерываем цикл
                        }
                }
                if (!(m_ErrServ == ERROR.NO))//Если есть ошибка то перезапускаем канал
                {
                    if (!(ResServ == null)) ResServ(this, new ResServEventArgs(m_NamePipe)); else ;//Вызов события о уничтожении канала
                }
            }

            /// <summary>
            /// Метод отправки сообщения клиенту
            /// </summary>
            /// <param name="message"></param>
            public void WriteMessage(string message)
            {
                if (!(m_pipeStream == null))
                    if (m_pipeStream.IsConnected == true)
                        m_ss.WriteString(message);
                    else
                        ;
                else
                    Logging.Logg().Error(string.Format(@"Pipes.Server.StreamPipe::WriteMessage (msg={0}) - канал сервера=NULL", message), Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Класс для отправки/посылки сообщений
            /// </summary>
            protected class StreamString
            {
                private NamedPipeServerStream ioStream;//Поток
                private UnicodeEncoding streamEncoding;//кодирование данных

                /// <summary>
                /// Конструктор
                /// </summary>
                /// <param name="ioStream">Поток из которого читаем данные</param>
                public StreamString(NamedPipeServerStream ioStream)
                {
                    this.ioStream = ioStream;
                    streamEncoding = new UnicodeEncoding();
                }

                /// <summary>
                /// Чтение строки
                /// </summary>
                /// <param name="err">Ошибка</param>
                /// <returns>Возвращает текст сообщения</returns>
                public string ReadString(out int err)
                {
                    err = 0;
                    string strRes = string.Empty;

                    int len = -1;//длина сообщения
                    byte[] inBuffer = null;

                    try
                    {
                        if ((ioStream.CanRead == true)
                            //&& (ioStream.CanSeek == true)
                            )
                        {
                            len = ioStream.ReadByte() * 256;

                            if (!(len < 0))
                            {
                                len += ioStream.ReadByte();
                                inBuffer = new byte[len];
                                ioStream.Read(inBuffer, 0, len);//Чтение массива байт из потока

                                strRes = streamEncoding.GetString(inBuffer);//Возвращаем кодированный в юникод массив байт
                            }
                            else
                            {// неизвестная длина
                                err = -3;
                            }
                        }
                        else
                        {// не может быть прочитан
                            err = -2;
                        }
                    }
                    catch (Exception e)
                    {
                        err = -1;

                        Logging.Logg().Exception(e, @"Pipes.Server.StreamPipe.StreamString::ReadString () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return strRes;
                }

                /// <summary>
                /// Запись строки
                /// </summary>
                /// <param name="outString">Сообщение</param>
                /// <returns>Длина сообщения</returns>
                public int WriteString(string outString)
                {
                    byte[] outBuffer = streamEncoding.GetBytes(outString);
                    int len = outBuffer.Length;
                    if (len > UInt16.MaxValue)
                    {
                        len = (int)UInt16.MaxValue;
                    }
                    try
                    {
                        ioStream.WriteByte((byte)(len / 256));
                        ioStream.WriteByte((byte)(len & 255));
                        ioStream.Write(outBuffer, 0, len);
                        ioStream.Flush();
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"Pipes.Server.StreamPipe.StreamString::WriteString () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return outBuffer.Length + 2;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public EventHandler ReadMessage
                , ResServ;
        }

        /// <summary>
        /// Класс для основного(распределяющего канала)
        /// </summary>
        private class MainPipeStream : PipeStream
        {
            /// <summary>
            /// Объект для блокировки доступа к словарю с потоками клиентов
            /// </summary>
            private object m_lockPipeStreamClient;

            /// <summary>
            /// Словарь с клиентами
            /// </summary>
            private Dictionary<string, PipeStream> m_dictPipeStreamClient;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="name">Имя канала</param>
            public MainPipeStream(string name)
                : base(name)
            {
                m_lockPipeStreamClient = new object();
                //Инициализация словаря
                m_dictPipeStreamClient = new Dictionary<string, PipeStream>();
            }

            /// <summary>
            /// Метод чтения данных из канала в потоке
            /// </summary>
            /// <param name="data"></param>
            protected override void ThreadRead(object data)
            {
                int err = 0;

                int errRead = -1;
                string stat = string.Empty
                    , new_pipe = string.Empty;
                string[] main_com = null//Разбор сообщения
                    //, message = null
                    ;

                while (err < 50)
                {
                    errRead = 0;

                    stat = m_ss.ReadString(out errRead);//Чтение из канала

                    if (errRead > 0)
                    {
                        err++;
                    }
                    else
                    {
                        //main_com = stat.Split(DELIMETER_MESSAGE_COMMAND);//Разбор сообщения
                        main_com = stat.Split(DELIMETER_MESSAGE_KEYVALUEPAIR);//Разбор сообщения
                        //message = new string[0];
                        if (main_com[0].Equals(COMMAND.Disconnect.ToString()) == true)//Переход по основной команде
                        {
                            //message = main_com[1].Split(DELIMETER_MESSAGE_KEYVALUEPAIR);//Получаем имя канала от клиента
                            //dictServ[message[1]].StartPipe();//Запускаем канал по имени клиента
                            m_dictPipeStreamClient[main_com[1]].Start();//Запускаем канал по имени клиента
                            if (!(ConnectClient == null)) ConnectClient(this, new ConnectionClientEventArgs(main_com[1])); else ;
                            m_ErrServ = ERROR.DISCONNECT;//Завершаем цикл и перезапускаем основной канал
                        }
                        else
                            if (main_com[0].Equals(COMMAND.Connect.ToString()) == true)
                            {
                                //message = main_com[1].Split(DELIMETER_MESSAGE_KEYVALUEPAIR);//Получаем имя клиента
                                new_pipe = main_com[1] + "_" + (m_dictPipeStreamClient.Count + 1);//добавляем номер п\п
                                m_ss.WriteString(new_pipe);//отправляем клиенту имя канала для работы
                                PipeStream stream = new PipeStream(new_pipe);//Создаём новый канал для клиента
                                m_dictPipeStreamClient.Add(new_pipe, stream);//Добавляем в словарь клиента и поток
                                //Подписываемся на события
                                m_dictPipeStreamClient[new_pipe].ReadMessage += new EventHandler(readMessage);
                                m_dictPipeStreamClient[new_pipe].ResServ += new EventHandler(resServ);
                            }
                            else
                            {
                                //Не известна команда
                                m_pipeStream.Flush();
                                m_ss.WriteString(MESSAGE_UNRECOGNIZED_COMMAND);
                            }

                        if (!(m_ErrServ == ERROR.NO))
                            break;
                    }
                }
                m_ErrServ = ERROR.ANY;
                if (!(ResServ == null)) ResServ(this, new ResServEventArgs(m_NamePipe)); else ;
            }

            /// <summary>
            /// Обработчик события получения нового сообщения из канала
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void readMessage(object sender, EventArgs e)
            {
                if (!(ReadMessage == null)) ReadMessage(this, e as ReadMessageEventArgs); else ;
            }

            /// <summary>
            /// Обработчик события отключения клиента
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void resServ(object sender, EventArgs e)
            {
                if ((e as ResServEventArgs).Value == true)
                {
                    if (!(DisConnectClient == null))
                        DisConnectClient(this, new ConnectionClientEventArgs((e as ResServEventArgs).IdServer));
                    else ;

                    lock (m_lockPipeStreamClient)
                    {
                        m_dictPipeStreamClient[(e as ResServEventArgs).IdServer].Stop();//Остановка канала конкретного клиента
                        m_dictPipeStreamClient.Remove((e as ResServEventArgs).IdServer);//Удаление этого клиента из словаря подключенных
                    }
                }
            }

            /// <summary>
            /// Перезапуск канала
            /// </summary>
            public override void RestartPipe()
            {
                m_bStopThread = true; //Флаг остановки работы потока распределяющего канала

                if (m_thread != null)
                {
                    if (m_thread.Join((int)TIMEOUT.NORMAL) == false)//Ожидаем секунду, Если не завершился
                    {
                        try
                        {
                            m_thread.Abort();//То завершаем принудительно
                        }
                        catch (ThreadAbortException e)
                        {
                            m_thread.Join((int)((int)TIMEOUT.NORMAL / (float)TIMEOUT.REDUSING));
                            m_thread.Interrupt();
                            m_thread = null;
                            Thread.ResetAbort();
                        }
                    }
                }

                m_pipeStream.Close();//Закрытие распределяющего канала
                m_pipeStream = null;//Обнуление распределяющего канала
                Start();//Создание нового распределяющего канала
            }

            public void SendDisconnect()
            {
                lock (m_lockPipeStreamClient)
                {
                    foreach (PipeStream stream in m_dictPipeStreamClient.Values)//Перебираем все подключенные клиенты
                    {
                        try
                        {
                            stream.WriteMessage(COMMAND.Disconnect.ToString());//Отправляем сообщение
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"Pipes.Server::SendDisconnect () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        stream.Stop();//Останавливаем канал
                    }

                    m_dictPipeStreamClient.Clear();//Очищаем словарь с клиентами
                }
            }

            public void WriteMessage(string idClient, string message)
            {
                lock (m_lockPipeStreamClient)
                {
                    if (m_dictPipeStreamClient.Count > 0)
                        m_dictPipeStreamClient[idClient].WriteMessage(message);
                    else
                        ;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public EventHandler ConnectClient
                , DisConnectClient;
        }

        /// <summary>
        /// Событие
        /// </summary>
        public EventHandler ReadMessage;

        /// <summary>
        /// Событие
        /// </summary>
        public event EventHandler ConnectClient
            , DisConnectClient;
    }

    public class Client : Pipe, IPipe
    {
        public static string MESSAGE_CONNECT_TO_SERVER_OK = @"Соединение установлено"
            , MESSAGE_DISCONNECT = "Соединение разорвано";

        private static int ERROR_MAX = 50;

        public bool b_Active;

        /// <summary>
        /// Экземпляр канала клиента
        /// </summary>
        NamedPipeClientStream m_stream { get { return _stream as NamedPipeClientStream; } }

        /// <summary>
        /// Состояние потока для соединения
        /// </summary>
        public bool IsConnected { get { return _stream == null ? false : (_stream as PipeStream).IsConnected; } }

        /// <summary>
        /// Экземпляр класса получения/посылки сообщений в канале
        /// </summary>
        StreamString m_ss;

        /// <summary>
        /// Имя сервера
        /// </summary>
        string m_serverName;

        /// <summary>
        /// Имя канала
        /// </summary>
        string m_pipeName;

        /// <summary>
        /// Таймаут ожидания подключения
        /// </summary>
        int m_timeOut;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="pcName">Имя ПК</param>
        /// <param name="timeOut">Таймаут попытки подключения</param>
        public Client(string pcName, int timeOut)
        {
            //Инициализация переменных
            m_timeOut = timeOut;
            m_serverName = pcName;
            //Генерация имени клиента из имени ПК и идентификатора процесса
            m_Name = Environment.MachineName + "_" + Process.GetCurrentProcess().Id;
        }

        private void connectToServer()
        {
            try
            {
                m_stream.Connect(m_timeOut);//Попытка подключения к серверу
            }
            catch { }
        }

        /// <summary>
        /// Старт клиента
        /// </summary>
        public void Start()
        {
            int err = 0;
            string ms = string.Empty;

            m_bStopThread = false;
            m_pipeName = string.Empty;
            //Инициализация экземпляра клиента
            _stream = new NamedPipeClientStream(m_serverName, NAME_MAINPIPE, PipeDirection.InOut, PipeOptions.Asynchronous);
            //Инициализация объекта чтения и записи в поток(stream)
            m_ss = new StreamString(m_stream);

            connectToServer();

            if (m_stream.IsConnected == true)//Если подключен
            {
                SendConnect();//Отправить сообщение серверу кто к нему подключился
                ms = m_ss.ReadString(out err);//Должен вернуть имя канала к которому подключиться

                //if (err == 0) {
                if (ms.Equals(MESSAGE_UNRECOGNIZED_COMMAND) == true)//Не понял команды и повторная попытка
                {
                    SendConnect();
                    ms = m_ss.ReadString(out err);
                }
                m_pipeName =
                m_Name =
                    ms;
                SendDisconnect();//Отключаемся от основного канала
                //} else ;

                _stream = null;

                //Инициализируем клиента с новым именем канала
                _stream = new NamedPipeClientStream(m_serverName, m_pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                m_ss = new StreamString(m_stream);

                connectToServer();

                if (m_stream.IsConnected == true)//Если был подключен
                {
                    SendConnect();//Отправлям сообщение о подключениии
                    _thread = new Thread(fThread);//Инициализация нового потока для работы канала
                    _thread.Start();//Запуск потока
                    b_Active = true;
                }
            }
        }

        /// <summary>
        /// Метод потока для чтения сообщений в канале
        /// </summary>
        /// <param name="data"></param>
        protected override void fThread(object data)
        {
            if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(MESSAGE_CONNECT_TO_SERVER_OK, m_serverName)); else ;

            int cntError = 0;
            bool bAbort = false;
            string[] main_com = null;

            while (bAbort == false)//Выполняется пока нет ошибок связи с сервером
            {
                string stat = string.Empty;
                int errRead = 0;

                if (m_stream.IsConnected == true)//Если подключение есть
                {
                    try
                    {
                        stat = m_ss.ReadString(out errRead);//Читаем из канала
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"Pipes.Client::fThread () - ....", Logging.INDEX_MESSAGE.NOT_SET);
                        bAbort = true;//Выходим из цикла
                    }
                }
                if (bAbort == false)//Если ошибок нет
                {
                    if (errRead < 0)//Если при чтении была ошибка
                    {
                        cntError++;//Увеличиваем переменную
                    }
                    else
                    {
                        //main_com = stat.Split(DELIMETER_MESSAGE_COMMAND); //Разбор сообщения на наличие управляющей команды
                        main_com = stat.Split(DELIMETER_MESSAGE_KEYVALUEPAIR);
                        //string[] message = new string[0];
                        if (main_com[0].Equals(COMMAND.Disconnect.ToString()) == true)
                        {//Если есть дисконнект
                            if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_serverName)); else ;
                            bAbort = true;//Завершаем поток
                        }
                        else
                        {
                            bAbort = false;
                            //Вызов события
                            if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_serverName)); else ;
                        }
                    }

                    if (m_bStopThread == true)//Есла флаг изменился то прервать цикл
                    {
                        // нормальное завршение по команде из-вне
                        break;
                    }

                    if (cntError > ERROR_MAX)//Если ошибок при чтении будет более 50 то
                    {
                        bAbort = true; //Наличие ошибки И прерываие цикла
                    }
                }
            }

            if (bAbort == true)//Если есть ошибка то
            {
                //Вызов события для перезапуска клиента
                if (!(ResServ == null)) ResServ(this, new ResServEventArgs(m_pipeName)); else ;
            }

            Thread.Sleep(1000);
        }

        /// <summary>
        /// Остановка клиента
        /// </summary>
        public void Stop()
        {
            if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(MESSAGE_DISCONNECT, m_serverName)); else ;

            m_stream.Close();//Закрытие канала
            m_stream.Dispose();//Освобождение ресурсов
            _stream = null;//Обнуление
            b_Active = false;

            m_bStopThread = true;//Меняем флаг для того чтобы завершить работу потока

            if (_thread != null)
            {
                bool stat = _thread.Join((int)Pipe.TIMEOUT.NORMAL);//Ожидаем секунду
                if (stat == false)//Если не завершился
                {
                    try
                    {
                        _thread.Abort();//То завершаем принудительно
                    }
                    catch (ThreadAbortException e)
                    {
                        _thread.Join((int)((int)Pipe.TIMEOUT.NORMAL / (int)Pipe.TIMEOUT.REDUSING));
                        _thread.Interrupt();
                        _thread = null;
                        Thread.ResetAbort();
                    }
                }
            }
        }

        /// <summary>
        /// Отправка сообщения о установлении связи
        /// </summary>
        public void SendConnect()
        {
            //m_ss.WriteString(COMMAND.Connect.ToString() + DELIMETER_MESSAGE_COMMAND + "ClientName=" + m_Name);
            m_ss.WriteString(COMMAND.Connect.ToString() + DELIMETER_MESSAGE_KEYVALUEPAIR + m_Name);
        }

        /// <summary>
        /// Отправка сообщения о разрыве связи
        /// </summary>
        public void SendDisconnect()
        {
            if (m_stream.IsConnected == true)
                //m_ss.WriteString(COMMAND.Disconnect.ToString() + DELIMETER_MESSAGE_COMMAND + "ClientName=" + m_Name)
                m_ss.WriteString(COMMAND.Disconnect.ToString() + DELIMETER_MESSAGE_KEYVALUEPAIR + m_Name)
                ;
            else
                ;
        }

        /// <summary>
        /// Отправка сообщения серверу
        /// </summary>
        /// <param name="message">Сообщение</param>
        public void WriteMessage(string message)
        {
            m_ss.WriteString(message);
        }

        /// <summary>
        /// Класс для отправки/посылки сообщений
        /// </summary>
        protected class StreamString
        {
            private Stream ioStream;//Поток
            private UnicodeEncoding streamEncoding;//Кодирование

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="ioStream">Поток из которого читаем данные</param>
            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            /// <summary>
            /// Чтение строки
            /// </summary>
            /// <param name="err">Ошибка</param>
            /// <returns>Возвращает текст сообщения</returns>
            public string ReadString(out int err)
            {
                err = 0;
                string strRes = string.Empty;

                int len = -1;//длина сообщения
                byte[] inBuffer = null;

                try
                {
                    if ((ioStream.CanRead == true)
                        //&& (ioStream.CanSeek == true)
                        )
                    {
                        len = ioStream.ReadByte() * 256; //??? зачем '* 256'                        

                        if (!(len < 0))
                        {
                            len += ioStream.ReadByte();
                            inBuffer = new byte[len];
                            ioStream.Read(inBuffer, 0, len);//Чтение массива байт из потока

                            strRes = streamEncoding.GetString(inBuffer);//Возвращаем кодированный в юникод массив байт
                        }
                        else
                        {// неизвестная длина
                            err = -3;
                        }
                    }
                    else
                        // не может быть прочитан
                        err = -2;
                }
                catch (Exception e)
                {
                    err = -1;

                    Logging.Logg().Exception(e, @"Pipes.Server.StreamPipe.StreamString::ReadString () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                Logging.Logg().Debug(string.Format(@"Pipes.Client.StreamString::ReadString () - len= {0}, err = {1}", len, err), Logging.INDEX_MESSAGE.NOT_SET);

                return strRes;
            }

            /// <summary>
            /// Запись строки
            /// </summary>
            /// <param name="outString">Сообщение</param>
            /// <returns>Длина сообщения</returns>
            public int WriteString(string outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }

        /// <summary>
        /// Событие
        /// </summary>
        public EventHandler ReadMessage
            , ResServ;
    }
}
