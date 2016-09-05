using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Diagnostics;
using System.IO.Pipes;


namespace uLoader
{
    class Pipes
    {
        public class Server
        {
            /// <summary>
            /// Поток работы сервера
            /// </summary>
            Thread m_server;
            
            /// <summary>
            /// Флаг для остановки потока сервера
            /// </summary>
            bool m_bStopServer;

            /// <summary>
            /// Экземпляр главного распределяющего потока
            /// </summary>
            MainStreamPipe m_mainStream;

            /// <summary>
            /// Имя сервера
            /// </summary>
            public string Name;

            /// <summary>
            /// Конструктор
            /// </summary>
            public Server()
            {
                Name = Environment.MachineName;
                //Новый экземпляр потока для сервера
                m_server = new Thread(ServerThread);
            }

            /// <summary>
            /// Старт сервера
            /// </summary>
            public void StartServer()
            {
                m_bStopServer = false;//инициализация переменной
                m_server.Start();//старт потока
            }

            /// <summary>
            /// Остановка сервера
            /// </summary>
            public void StopServer()
            {
                m_bStopServer = true;//Изменение зн-я флага для завершения работы потока

                if (m_server != null)
                {
                    if (m_server.Join(1000) == false)//Ожидаем секунду, Если не завершился
                    {
                        m_server.Join(200);
                        m_server.Interrupt();
                        if(m_server.ThreadState == System.Threading.ThreadState.Running)
                                m_server.Abort();
                        m_server = null;
                    }
                }


                if (m_mainStream != null)
                    m_mainStream.StopPipe();

                
            }

            /// <summary>
            /// Метод для потока сервера
            /// </summary>
            /// <param name="data"></param>
            private void ServerThread(object data)
            {
                m_mainStream = new MainStreamPipe("MainPipe");//Новый экземпляр именованного канала, передаём его имя
                
                //Подписка на события
                m_mainStream.ReadMessage += new EventHandler(readMessage);
                m_mainStream.ResServ += new EventHandler(resServ);
                m_mainStream.ConnectClient += new EventHandler(event_connectClient);
                m_mainStream.DisConnectClient += new EventHandler(event_disConnectClient);
                
                m_mainStream.StartPipe();//Старт канала
            }

            /// <summary>
            /// Обрабочик получения нового сообщения от клиента
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void readMessage(object sender, EventArgs e)
            {
                if (!(ReadMessage == null)) ReadMessage(this, (e as StreamPipe.ReadMessageEventArgs)); else ;
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
                if (!(ConnectClient == null)) ConnectClient(this, e); else ;
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
                        stream.WriteMessage("Disconnect");//Отправляем сообщение
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
                public bool m_ErrServ = false;

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
                protected bool m_b_stopThread;

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
                        m_pipeServer.WaitForConnection();//Ожидание подключения
                    }
                    catch(Exception e)//Если таймаут превышен
                    {
                        err = 1;
                    }
                }

                /// <summary>
                /// Запуск канала
                /// </summary>
                public void StartPipe()
                {
                    //Инициализация переменных
                    int err = 0;
                    m_b_stopThread = false;
                    m_ErrServ = false;
                    
                    //Инициализация экземпляра канала
                    m_pipeServer = new NamedPipeServerStream(m_NamePipe, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1024 * 1024, 1024 * 1024);
                    
                    //Инициализация экземпляра класса по работе с каналом
                    m_ss = new StreamString(m_pipeServer);

                    waitConnect(out err);//Ожидание подключения

                    if (err == 0)//Если оштбок нет 
                    {
                        m_thread = new Thread(ThreadRead);//Новый поток работы канала
                        m_thread.Start();//Старт потока
                    }
                }
                
                /// <summary>
                /// Остановка канала
                /// </summary>
                public virtual void StopPipe()
                {
                    m_b_stopThread = true;
                    if (m_pipeServer.IsConnected == true)//Если соединение установлено то
                        m_pipeServer.Disconnect();//Разрываем соединение
                    m_pipeServer.Close();//Закрываем канал
                    m_pipeServer.Dispose();//Уничтожаем ресурсы
                    m_pipeServer = null;//Обнуляем

                    if (m_thread != null)
                    {
                        if (m_thread.Join(5000) == false)//Ожидаем секунду, Если не завершился
                        {
                            try
                            {
                                m_thread.Abort();//То завершаем принудительно
                            }
                            catch (ThreadAbortException e)
                            {
                                m_thread.Join(1000);
                                m_thread.Interrupt();
                                m_thread = null;
                                Thread.ResetAbort();
                            }
                        }
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
                    while (m_ErrServ == false)//Ошибок при чтении
                    {
                        int errRead = 0;

                        string stat = string.Empty;
                        try
                        {
                            stat = m_ss.ReadString(out errRead);//Чтение из канала
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                            m_ErrServ = true;//Ошибка работы канала
                        }

                        if (m_ErrServ == false)//если ошибок нет
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
                                        string[] main_com = stat.Split(';');//разбор сложного сообщения
                                        string[] message = new string[0];
                                        switch (main_com[0])//переход по служебным сообщениям
                                        {
                                            case "Disconnect":
                                                message = main_com[1].Split('=');
                                                m_ErrServ = true;
                                                if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_NamePipe)); else ;//Вызов события передачи сообщения
                                                break;

                                            default:
                                                if (!(ReadMessage == null)) ReadMessage(this, new ReadMessageEventArgs(stat, m_NamePipe)); else ;//Вызов события передачи сообщения
                                                break;
                                        }
                                    }
                                    if (m_b_stopThread == true)//Был ли изменен флаг остановки потока
                                    {
                                        break;//Прерываем цикл
                                    }
                                    if (err >= 50)
                                        m_ErrServ = true;
                                }
                            }
                            else
                            {
                                break;//прерываем цикл
                            }
                    }
                    if (m_ErrServ == true)//Если есть ошибка то перезапускаем канал
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
                    if (m_pipeServer.IsConnected == true)
                        m_ss.WriteString(message);
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
                /// Класс для описания аргумента события
                /// </summary>
                public class ReadMessageEventArgs : EventArgs
                {
                    /// <summary>
                    /// Значение изменяемого параметра
                    /// </summary>
                    public string Value;

                    public string IdServer;

                    public ReadMessageEventArgs(string value, string id_serv)
                    {
                        Value = value;
                        IdServer = id_serv;
                    }
                }

                /// <summary>
                /// Событие
                /// </summary>
                public EventHandler ReadMessage;

                /// <summary>
                /// Класс для описания аргумента события
                /// </summary>
                public class ResServEventArgs : EventArgs
                {
                    /// <summary>
                    /// Значение изменяемого параметра
                    /// </summary>
                    public bool Value;
                    public string IdServer;

                    public ResServEventArgs(string id_serv)
                        : base()
                    {
                        Value = true;
                        IdServer = id_serv;
                    }
                }

                /// <summary>
                /// Событие
                /// </summary>
                public EventHandler ResServ;
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
                    while (err < 50)
                    {
                        int errRead = 0;

                        string stat = m_ss.ReadString(out errRead);//Чтение из канала

                        if (errRead > 0)
                        {
                            err++;
                        }
                        else
                        {
                            string[] main_com = stat.Split(';');//Разбор сообщения
                            string[] message = new string[0];
                            switch (main_com[0])//Переход по основной команде
                            {
                                case "Disconnect":
                                    message = main_com[1].Split('=');//Получаем имя канала от клиента
                                    dictServ[message[1]].StartPipe();//Запускаем канал по имени клиента
                                    if (!(ConnectClient == null)) ConnectClient(this, new ConnectClientEventArgs(message[1])); else ;
                                    m_ErrServ = true;//Завершаем цикл и перезапускаем основной канал
                                    break;

                                case "Connect":
                                    message = main_com[1].Split('=');//Получаем имя клиента
                                    string new_pipe = message[1] + "_" + (dictServ.Count + 1);//добавляем номер п\п
                                    m_ss.WriteString(new_pipe);//отправляем клиенту имя канала для работы
                                    StreamPipe stream = new StreamPipe(new_pipe);//Создаём новый канал для клиента
                                    dictServ.Add(new_pipe, stream);//Добавляем в словарь клиента и поток
                                    //Подписываемся на события
                                    dictServ[new_pipe].ReadMessage += new EventHandler(readMessage);
                                    dictServ[new_pipe].ResServ += new EventHandler(resServ);
                                    break;

                                default:
                                    //Не известна команда
                                    m_pipeServer.Flush();
                                    m_ss.WriteString("Non command");
                                    break;
                            }
                            if (m_ErrServ == true)
                                break;
                        }
                    }
                    m_ErrServ = true;
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
                        if (DisConnectClient == null) DisConnectClient(this, new DisConnectClientEventArgs((e as ResServEventArgs).IdServer)); else ;

                        dictServ[(e as ResServEventArgs).IdServer].StopPipe();//Остановка канала конкретного клиента
                        dictServ.Remove((e as ResServEventArgs).IdServer);//Удаление этого клиента из словаря подключенных
                    }
                }

                /// <summary>
                /// Перезапуск канала
                /// </summary>
                public override void RestartPipe()
                {
                    m_b_stopThread = true; //Флаг остановки работы потока распределяющего канала

                    if (m_thread != null)
                    {
                        if (m_thread.Join(1000) == false)//Ожидаем секунду, Если не завершился
                        {
                            try
                            {
                                m_thread.Abort();//То завершаем принудительно
                            }
                            catch (ThreadAbortException e)
                            {
                                m_thread.Join(200);
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
                /// Класс для описания аргумента события
                /// </summary>
                public class ConnectClientEventArgs : EventArgs
                {
                    /// <summary>
                    /// Значение изменяемого параметра
                    /// </summary>
                    public string IdServer;

                    public ConnectClientEventArgs(string id_serv)
                    {
                        IdServer = id_serv;
                    }
                }

                /// <summary>
                /// Событие
                /// </summary>
                public EventHandler ConnectClient;

                /// <summary>
                /// Класс для описания аргумента события
                /// </summary>
                public class DisConnectClientEventArgs : EventArgs
                {
                    /// <summary>
                    /// Значение изменяемого параметра
                    /// </summary>
                    public string IdServer;

                    public DisConnectClientEventArgs(string id_serv)
                    {
                        IdServer = id_serv;
                    }
                }

                /// <summary>
                /// Событие
                /// </summary>
                public EventHandler DisConnectClient;
            }            

            /// <summary>
            /// Класс для описания аргумента события
            /// </summary>
            public class ReadMessageEventArgs : EventArgs
            {
                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public string Value;

                public string IdServer;

                public ReadMessageEventArgs(string value, string id_serv)
                {
                    Value = value;
                    IdServer = id_serv;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public EventHandler ReadMessage;

            /// <summary>
            /// Класс для описания аргумента события
            /// </summary>
            public class ConnectClientEventArgs : EventArgs
            {
                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public string IdServer;

                public ConnectClientEventArgs(string id_serv)
                {
                    IdServer = id_serv;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public event EventHandler ConnectClient;

            /// <summary>
            /// Класс для описания аргумента события
            /// </summary>
            public class DisConnectClientEventArgs : EventArgs
            {
                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public string IdServer;

                public DisConnectClientEventArgs(string id_serv)
                {
                    IdServer = id_serv;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public EventHandler DisConnectClient;
        }

        public class Client
        {
            private static int ERROR_MAX = 50;

            public bool b_Active;

            /// <summary>
            /// Экземпляр канала клиента
            /// </summary>
            NamedPipeClientStream m_client;

            /// <summary>
            /// Флаг для остановки потока
            /// </summary>
            bool m_bStopThread;

            /// <summary>
            /// Экземпляр класса получения/посылки сообщений в канале
            /// </summary>
            StreamString m_ss;

            /// <summary>
            /// Состояние клиента
            /// </summary>
            public bool m_bIsConnected;

            /// <summary>
            /// Имя клиента
            /// </summary>
            public string m_Name;

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
            /// Экземпляр потока клиента для чтения сообщений
            /// </summary>
            Thread thread;

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
                m_client = new NamedPipeClientStream(m_serverName, "MainPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
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
                    if (ms == "Non command")//Не понял команды и повторная попытка
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
                        thread = new Thread(ThreadRead);//Инициализация нового потока для работы канала
                        thread.Start();//Запуск потока
                        b_Active = true;
                    }
                }
            }

            /// <summary>
            /// Метод потока для чтения сообщений в канале
            /// </summary>
            /// <param name="data"></param>
            protected virtual void ThreadRead(object data)
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

                if (thread != null)
                {
                    bool stat = thread.Join(1000);//Ожидаем секунду
                    if (stat == false)//Если не завершился
                    {
                        try
                        {
                            thread.Abort();//То завершаем принудительно
                        }
                        catch (ThreadAbortException e)
                        {
                            thread.Join(200);
                            thread.Interrupt();
                            thread = null;
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
            /// Класс для описания аргумента события
            /// </summary>
            public class ReadMessageEventArgs : EventArgs
            {
                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public string Value;

                public string IdServer;

                public ReadMessageEventArgs(string value, string id_serv)
                {
                    Value = value;
                    IdServer = id_serv;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public EventHandler ReadMessage;

            /// <summary>
            /// Класс для описания аргумента события
            /// </summary>
            public class ResServEventArgs : EventArgs
            {
                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public bool Value;
                public string IdServer;

                public ResServEventArgs(string id_serv)
                    : base()
                {
                    Value = true;
                    IdServer = id_serv;
                }
            }

            /// <summary>
            /// Событие
            /// </summary>
            public EventHandler ResServ;
        }
    }
}
