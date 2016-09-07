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
    public abstract class Pipe
    {
        public enum ID_EVENT : short
        {
            Unknown = -1
                , Start, Stop, Exit, Connect, Disconnect
            , Count
        }

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

        public static string MESSAGE_RECIEVED_OK = @"OK";
        public static Char DELIMETER_MESSAGE_KEYVALUEPAIR = '=';
        protected static string MESSAGE_UNRECOGNIZED_COMMAND = @"Non command";

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
    
    public class Server : Pipe
    {
        public enum ERROR { OVER_ATTEMPT = -2, ANY = -1, NO, DISCONNECT }

        /// <summary>
        /// Экземпляр главного распределяющего потока
        /// </summary>
        private MainStreamPipe m_mainStream;

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
        public void StartServer()
        {
            m_bStopThread = false;//инициализация переменной
            _thread.Start();//старт потока
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void StopServer()
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
                    m_mainStream.StopPipe();
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
            try {
                m_mainStream = new MainStreamPipe(NAME_MAINPIPE);//Новый экземпляр именованного канала, передаём его имя

                //Подписка на события
                m_mainStream.ReadMessage += new EventHandler(readMessage);
                m_mainStream.ResServ += new EventHandler(resServ);
                m_mainStream.ConnectClient += new EventHandler(event_connectClient);
                m_mainStream.DisConnectClient += new EventHandler(event_disConnectClient);

                m_mainStream.StartPipe();//Старт канала
            } catch (Exception e) {
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
            if(m_mainStream.dictServ.Count>0)
            m_mainStream.dictServ[idClient].WriteMessage(message);
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
            foreach (StreamPipe stream in m_mainStream.dictServ.Values)//Перебираем все подключенные клиенты
            {
                try
                {
                    stream.WriteMessage(ID_EVENT.Disconnect.ToString());//Отправляем сообщение
                }
                catch
                {
                        
                }
                stream.StopPipe();//Останавливаем канал
            }
            m_mainStream.dictServ.Clear();//Очищаем словарь с клиентами
        }


        /// <summary>
        /// Класс для работы с именованным каналом
        /// </summary>
        public class StreamPipe
        {
            /// <summary>
            /// Экземпляр канала
            /// </summary>
            protected NamedPipeServerStream m_pipeServer;

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
            public StreamPipe(string name)
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
                    m_pipeServer.BeginWaitForConnection(callbackConnection, null);
                }
                catch(Exception e)//Если таймаут превышен
                {
                    err = 1;
                }
            }

            private void callbackConnection(IAsyncResult res)
            {
                if (!(m_pipeServer == null)) {
                    m_pipeServer.EndWaitForConnection(res);

                    if (m_pipeServer.IsConnected == true) {
                        m_thread = new Thread(ThreadRead);//Новый поток работы канала
                        m_thread.Start();//Старт потока
                    } else
                        ;
                } else
                    ;
            }

            /// <summary>
            /// Запуск канала
            /// </summary>
            public void StartPipe()
            {
                //Инициализация переменных
                int err = 0;
                m_bStopThread = false;
                m_ErrServ = 0;
                    
                //Инициализация экземпляра канала
                m_pipeServer = new NamedPipeServerStream(m_NamePipe
                    , PipeDirection.InOut
                    , 1
                    , PipeTransmissionMode.Message
                    , PipeOptions.Asynchronous
                    , 1024 * 1024, 1024 * 1024);

                //Инициализация экземпляра класса по работе с каналом
                m_ss = new StreamString(m_pipeServer);

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
            public virtual void StopPipe()
            {
                m_bStopThread = true;

                try {
                    if (!(m_pipeServer == null))
                    {
                        if (m_pipeServer.IsConnected == true)//Если соединение установлено то
                            m_pipeServer.Disconnect();//Разрываем соединение
                        m_pipeServer.Close();//Закрываем канал
                        m_pipeServer.Dispose();//Уничтожаем ресурсы
                    }
                    else
                        Logging.Logg().Error(@"::StopPipe () - объект канала сервера =NULL", Logging.INDEX_MESSAGE.NOT_SET);
                    m_pipeServer = null;//Обнуляем

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
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"Pipes.Server::StopPipe () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Перезапуск канала
            /// </summary>
            public virtual void RestartPipe()
            {
                if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs("Клиент отключен...", m_NamePipe)); else ;
                m_pipeServer.Close();//Закрываем канал
                m_pipeServer = null;
                StartPipe();//Запускаем канал
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
                        Debug.WriteLine(e.Message);
                        m_ErrServ = ERROR.ANY; //Ошибка работы канала
                    }

                    if (m_ErrServ == 0)//если ошибок нет
                        if (m_pipeServer != null)
                        {
                            if (m_pipeServer.IsConnected == true)
                            {
                                if (errRead > 0)//Проверка на ошибки при чтении
                                {
                                    err++;
                                }
                                else
                                {
                                    err = 0;
                                    main_com = stat.Split(';');//разбор сложного сообщения
                                    //message = new string[0];
                                    if (main_com[0].Equals(ID_EVENT.Disconnect.ToString()) == true)//переход по служебным сообщениям
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
                if (!(m_pipeServer == null))
                    if (m_pipeServer.IsConnected == true)
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
                    int len;//длина сообщения
                    err = 0;
                    try
                    {
                        len = ioStream.ReadByte() * 256;
                        len += ioStream.ReadByte();
                        byte[] inBuffer = new byte[len];
                        ioStream.Read(inBuffer, 0, len);//Чтение массива байт из потока
                        return streamEncoding.GetString(inBuffer);//Возвращаем кодированный в юникод массив байт
                    }
                    catch
                    {
                        err = 1;
                        return string.Empty;
                    }
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
                    catch
                    {
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
        public class MainStreamPipe : StreamPipe
        {
            /// <summary>
            /// Словарь с клиентами
            /// </summary>
            public Dictionary<string, StreamPipe> dictServ;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="name">Имя канала</param>
            public MainStreamPipe(string name)
                : base(name)
            {
                //Инициализация словаря
                dictServ = new Dictionary<string, StreamPipe>();
            }

            /// <summary>
            /// Метод чтения данных из канала в потоке
            /// </summary>
            /// <param name="data"></param>
            protected override void ThreadRead(object data)
            {
                int err = 0;

                int errRead = -1;
                string stat = string.Empty;
                string[] main_com = null//Разбор сообщения
                    , message = null;

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
                        main_com = stat.Split(';');//Разбор сообщения
                        message = new string[0];
                        if (main_com[0].Equals (ID_EVENT.Disconnect.ToString()) == true)//Переход по основной команде
                        {
                            message = main_com[1].Split('=');//Получаем имя канала от клиента
                            dictServ[message[1]].StartPipe();//Запускаем канал по имени клиента
                            if (!(ConnectClient == null)) ConnectClient(this, new ConnectionClientEventArgs(message[1])); else ;
                            m_ErrServ = ERROR.DISCONNECT;//Завершаем цикл и перезапускаем основной канал
                        }
                        else
                            if (main_com[0].Equals (ID_EVENT.Connect.ToString()) == true)
                            {
                                message = main_com[1].Split(DELIMETER_MESSAGE_KEYVALUEPAIR);//Получаем имя клиента
                                string new_pipe = message[1] + "_" + (dictServ.Count + 1);//добавляем номер п\п
                                m_ss.WriteString(new_pipe);//отправляем клиенту имя канала для работы
                                StreamPipe stream = new StreamPipe(new_pipe);//Создаём новый канал для клиента
                                dictServ.Add(new_pipe, stream);//Добавляем в словарь клиента и поток
                                //Подписываемся на события
                                dictServ[new_pipe].ReadMessage += new EventHandler(readMessage);
                                dictServ[new_pipe].ResServ += new EventHandler(resServ);
                            }
                            else
                            {
                                //Не известна команда
                                m_pipeServer.Flush();
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

                    dictServ[(e as ResServEventArgs).IdServer].StopPipe();//Остановка канала конкретного клиента
                    dictServ.Remove((e as ResServEventArgs).IdServer);//Удаление этого клиента из словаря подключенных
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

                m_pipeServer.Close();//Закрытие распределяющего канала
                m_pipeServer = null;//Обнуление распределяющего канала
                StartPipe();//Создание нового распределяющего канала
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

    public class Client : Pipe
    {
        private static int ERROR_MAX = 50;

        public bool b_Active;

        /// <summary>
        /// Экземпляр канала клиента
        /// </summary>
        NamedPipeClientStream m_client;

        /// <summary>
        /// Экземпляр класса получения/посылки сообщений в канале
        /// </summary>
        StreamString m_ss;

        /// <summary>
        /// Состояние клиента
        /// </summary>
        public bool m_bIsConnected;

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

        public bool m_ErrServ = false;

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
            m_bIsConnected = false;
            //Генерация имени клиента из имени ПК и идентификатора процесса
            m_Name = Environment.MachineName + "_" + Process.GetCurrentProcess().Id;
        }

        /// <summary>
        /// Старт клиента
        /// </summary>
        public void StartClient()
        {
            int err = 0;
            m_bStopThread = false;
            m_pipeName = string.Empty;
            //Инициализация экземпляра клиента
            m_client = new NamedPipeClientStream(m_serverName, NAME_MAINPIPE, PipeDirection.InOut, PipeOptions.Asynchronous);
            //Инициализация объекта чтения и записи в поток(stream)
            m_ss = new StreamString(m_client);

            try
            {
                m_client.Connect(m_timeOut);//Попытка подключения к серверу
            }
            catch
            {
            }

            if (m_client.IsConnected == true)//Если подключен
            {
                m_bIsConnected = m_client.IsConnected;
                SendConnect();//Отправить сообщение серверу кто к нему подключился
                string ms = m_ss.ReadString(out err);//Должен вернуть имя канала к которому подключиться
                if (ms.Equals(MESSAGE_UNRECOGNIZED_COMMAND) == true)//Не понял команды и повторная попытка
                {
                    SendConnect();
                    ms = m_ss.ReadString(out err);
                }
                m_pipeName = ms;
                m_Name = m_pipeName;
                SendDisconnect();//Отключаемся от основного канала
                m_bIsConnected = m_client.IsConnected;

                m_client = null;

                //Инициализируем клиента с новым именем канала
                m_client = new NamedPipeClientStream(m_serverName, m_pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                m_ss = new StreamString(m_client);

                try
                {
                    m_client.Connect(m_timeOut); //Подключаемся
                }
                catch
                {
                }

                if (m_client.IsConnected == true)//Если был подключен
                {
                    m_bIsConnected = m_client.IsConnected;
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
            if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs("Соединение установлено", m_serverName)); else ;

            int err = 0;

            while (m_ErrServ == false)//Выполняется пока нет ошибок связи с сервером
            {
                string stat = string.Empty;
                int errRead = 0;

                if (m_client.IsConnected == true)//Если подключение есть
                {
                    try
                    {
                        stat = m_ss.ReadString(out errRead);//Читаем из канала
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        m_ErrServ = true;//Выходим из цикла
                    }
                }
                if (m_ErrServ == false)//Если ошибок нет
                {
                    if (errRead > 0)//Если при чтении была ошибка
                    {
                        err++;//Увеличиваем переменную
                    }
                    else
                    {
                        string[] main_com = stat.Split(';');//Разбор сообщения на наличие управляющей команды
                        string[] message = new string[0];
                        switch (main_com[0])
                        {
                            case "Disconnect"://Если есть дисконнект
                                if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_serverName)); else ;
                                m_ErrServ = true;//Завершаем поток
                                break;

                            default:
                                m_ErrServ = false;
                                //Вызов события
                                if (!(ReadMessage ==  null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_serverName)); else ;
                                break;
                        }
                    }

                    if (m_bStopThread == true)//Есла флаг изменился то прервать цикл
                    {
                        break;
                    }

                    if (err >= ERROR_MAX)//Если ошибок при чтении будет более 50 то
                    {
                        m_ErrServ = true; //Наличие ошибки
                        break;//Прерывание цикла
                    }
                }
            }
            if (m_ErrServ == true)//Если есть ошибка то
            {
                //Вызов события для перезапуска клиента
                if (!(ResServ == null)) ResServ(this, new ResServEventArgs(m_pipeName)); else ;
            }

            Thread.Sleep(1000);
        }

        /// <summary>
        /// Остановка клиента
        /// </summary>
        public void StopClient()
        {
            if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs("Соединение разорвано", m_serverName)); else ;


            m_client.Close();//Закрытие канала
            m_client.Dispose();//Освобождение ресурсов
            m_client = null;//Обнуление
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
            m_ss.WriteString("Connect;ClientName=" + m_Name);
        }

        /// <summary>
        /// Отправка сообщения о разрыве связи
        /// </summary>
        public void SendDisconnect()
        {
            if(m_client.IsConnected==true)
                m_ss.WriteString("Disconnect;ClientName=" + m_Name);
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
                int len;//длина сообщения
                err = 0;
                try
                {
                    len = ioStream.ReadByte() * 256;
                    len += ioStream.ReadByte();
                    byte[] inBuffer = new byte[len];
                    ioStream.Read(inBuffer, 0, len);//Чтение массива байт из потока
                    return streamEncoding.GetString(inBuffer);//Возвращаем кодированный в юникод массив байт
                }
                catch
                {
                    err = 1;
                    return string.Empty;
                }
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
