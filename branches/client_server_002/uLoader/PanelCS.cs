using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;

using HClassLibrary;


namespace uLoader
{
    partial class PanelClientServer : HPanelCommonDataHost
    {
        public enum ID_EVENT : short
        {
            Unknown = -1
                , Start, Stop
                , State, Exit, Connect, Disconnect
            , Count
        }
        /// <summary>
        /// Типы экземпляра дочерней панели
        /// </summary>
        protected enum TypePanel
        {
            Unknown = -1
                , Client, Server
            , Count
        }
        /// <summary>
        /// Массив дочерних панелей
        /// </summary>
        private PanelCS[] m_arPanels;
        /// <summary>
        /// Признак состояния внешней панели (Работа)
        /// </summary>
        private PanelWork.STATE _stateLocalPanelWork;
        /// <summary>
        /// Тип включенной(активной) дочерней панели
        /// </summary>
        private TypePanel _typePanelEnabled;

        //private DelegateFunc eventTypePanelEnableInitialize;

        public PanelClientServer(InteractionParameters? pars = null)
            : base(1, 2)
        {
            m_semInteractionParameters = new Semaphore(0, 1);

            if ((!(pars == null))
                && (pars.GetValueOrDefault().Ready == true))
            {
                m_InteractionParameters = pars.GetValueOrDefault();
                start();
            }
            else
                ;

            eventStateLocalPanelWorkChanged += new DelegateFunc(onEventStateLocalPanelWorkChanged);
            //eventTypePanelEnableInitialize += new DelegateFunc (onEventTypePanelEnableInitialize);

            _stateLocalPanelWork = PanelWork.STATE.Unknown;
            _typePanelEnabled = TypePanel.Unknown;
        }

        public int Ready {
            get {
                return (m_InteractionParameters.Ready == true) ? (!(m_arPanels == null)) && ((m_arPanels[(int)TypePanel.Client].Ready == true)
                    && (m_arPanels[(int)TypePanel.Server].Ready == true)) ? 0 : 1 : -1;
            }
        }

        /// <summary>
        /// Обработчик приема команд от дочерних панелей
        /// </summary>
        /// <param name="sender">Панель, отправившая событие</param>
        /// <param name="ev">Аргумент события</param>
        private void panelOnCommandEvent(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            // 1-ый(0) параметр - объект-вкладка
            // 2-ой(1) - тип приложения (TypePanel)
            // 3-ий(2) - тип сообщения (TypeMes)
            // 4-ый(3) - идентификатор события (Pipes.Pipe.COMMAND)
            object[] pars = (ev.par[0] as object[])[0] as object[];
            //Определить: 1) внутреннее сообщение или 2) для передачи в родительскую форму
            // по кол-ву параметров (короткие сообщения - внутренние)
            bool bRedirect = pars.Length > 2;

            try
            {
                if (bRedirect == true) {
                    switch ((Pipes.Pipe.COMMAND)pars[3]) {
                        case Pipes.Pipe.COMMAND.Start:
                            DataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.INTERACTION_EVENT
                                , ID_EVENT.Start }
                            });
                            break;
                        case Pipes.Pipe.COMMAND.Stop:
                            DataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.INTERACTION_EVENT
                                , ID_EVENT.Stop }
                            });
                            break;
                        case Pipes.Pipe.COMMAND.AppState:
                            DataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.INTERACTION_EVENT
                                , ID_EVENT.State
                                , pars.Length > 4 ? pars[4] : null }
                            });
                            break;
                        default:
                            break;
                    }                    
                }
                else
                    // внутреннее сообщение
                    if (((TypePanel)pars[1]) == TypePanel.Client) //e.TypeApp == PanelCS.TypePanel.Client
                        BeginInvoke(new DelegateFunc(reConnClient));
                    else //e.TypeApp == PanelCS.TypePanel.Server
                        ; // ничего не делаем
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"PanelClientServer::panelOnCommandEvent () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private InteractionParameters m_InteractionParameters;

        /// <summary>
        /// Обработчик события получения данных по запросу (выполняется в текущем потоке)
        /// </summary>
        /// <param name="obj">Результат, полученный по запросу (массив 'object')</param>
        protected override void onEvtDataRecievedHost(object obj)
        {
            //Обработанное состояние 
            HHandlerQueue.StatesMachine state = (HHandlerQueue.StatesMachine)Int32.Parse((obj as object[])[0].ToString());
            //Параметры (массив) в 1-ом элементе результата
            object par = (obj as object[])[1];

            int iRes = -1;

            //InteractionParameters interactionPars;

            switch (state)
            {
                case HHandlerQueue.StatesMachine.GET_INTERACTION_PARAMETERS:
                    if (!(par == null))
                    {
                        //start(par);
                        //BeginInvoke(new DelegateObjectFunc (start), par);
                        //eventRecievedInteractionParameters(par);

                        //TypePanelEnabled = m_arPanels[(int)TypePanel.Client].Enabled == true ? TypePanel.Client :
                        //    m_arPanels[(int)TypePanel.Server].Enabled == true ? TypePanel.Server :
                        //        TypePanel.Unknown;

                        m_InteractionParameters = (InteractionParameters)par;
                        m_semInteractionParameters.Release(1);
                    }
                    else
                        ;
                    break;
                case HHandlerQueue.StatesMachine.FORMMAIN_COMMAND_TO_INTERACTION:
                    break;
                default:
                    break;
            }
        }

        private void start(/*object obj*/)
        {
            InteractionParameters pars = m_InteractionParameters;

            m_arPanels = new PanelCS[(int)TypePanel.Count];
            m_arPanels[(int)TypePanel.Client] = new PanelClient(pars.m_arNameServers);
            m_arPanels[(int)TypePanel.Client].EvtDataAskedHost += new DelegateObjectFunc(panelOnCommandEvent);
            m_arPanels[(int)TypePanel.Client].Start();
            //m_panelClient.Dock = DockStyle.Fill; уже Fill
            m_arPanels[(int)TypePanel.Server] = new PanelServer(pars.m_arNameServers);
            m_arPanels[(int)TypePanel.Server].EvtDataAskedHost += new DelegateObjectFunc(panelOnCommandEvent);
            m_arPanels[(int)TypePanel.Server].Start();
            //m_panelServer.Dock = DockStyle.Fill; уже Fill

            //EventStartedWorkChanged += new DelegateFunc(onEventStartedWorkChanged);

            this.Controls.Add(m_arPanels[(int)TypePanel.Server], 0, 0);
            this.Controls.Add(m_arPanels[(int)TypePanel.Client], 0, 1);
        }
        /// <summary>
        /// Внутреннее событие - для изменения содержания подписи - состояния внешнй (рабочей) панели
        /// </summary>
        private event DelegateFunc eventStateLocalPanelWorkChanged;

        public PanelWork.STATE StateLocalPanelWork
        {
            get {
                return _stateLocalPanelWork;
            }

            set {
                if (!(_stateLocalPanelWork == value)) {
                    _stateLocalPanelWork = value;
                    eventStateLocalPanelWorkChanged();
                } else
                    ;
            }
        }

        private PanelWork.STATE _stateRemotePanelWork;

        private void onEventStateLocalPanelWorkChanged()
        {
            if (!(m_arPanels == null))
                m_arPanels[(int)_typePanelEnabled].UpdatePanelWorkState();
            else
                ;
        }

        private Semaphore m_semInteractionParameters;

        public override void Start()
        {
            base.Start();

            DataAskedHost(new object[] {
                new object[] {
                    HHandlerQueue.StatesMachine.GET_INTERACTION_PARAMETERS
                }
            });

            // ждать ответа
            m_semInteractionParameters.WaitOne();
            // проверить корректность инициализации
            if (!(Ready < 0)) {
                start();
                //BeginInvoke(new DelegateObjectFunc (start), par);
                //eventRecievedInteractionParameters(par);

                if ((m_arPanels[(int)TypePanel.Client] as PanelClient).IsConnected == true)
                    _typePanelEnabled = TypePanel.Client;
                else
                    _typePanelEnabled = TypePanel.Server;

                m_arPanels[(int)_typePanelEnabled].Enabled = true;
            }
            else
                ;
            // проверить тип активной панели
            if (_typePanelEnabled == TypePanel.Server)
            // если сервер, значит значит статус спрашивать не у кого (считаем, что рабочая панель ни у одного экземпляра не активна)
                DataAskedHost(new object[] {
                    new object[] { 
                        HHandlerQueue.StatesMachine.INTERACTION_EVENT, ID_EVENT.State
                        , PanelWork.STATE.Unknown // состояние взаимодействующего экземпляра
                } });
            else
            // если клиент, ожидать сообщения о статусе от сервера
                ;
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true) {
                m_arPanels[(int)TypePanel.Server].Activate(active);
                m_arPanels[(int)TypePanel.Client].Activate(active);
            } else
                ;

            return bRes;
        }

        /// <summary>
        /// Метод для завершения клиент/серверной части
        /// </summary>
        public override void Stop()
        {
            if (!(m_arPanels == null)) {
                m_arPanels[(int)TypePanel.Client].Stop();
                m_arPanels[(int)TypePanel.Server].Stop();
            }
            else
                ;

            base.Stop();
        }
        /// <summary>
        /// Попытка нового подключения к серверу
        /// </summary>
        private void reConnClient()
        {
            // очевидно, что _typePanelEnabled == TypePanel.Client
            TypePanel prevTypePanelEnabled = _typePanelEnabled;
            bool bIsConnected = false;

            try {
                // остановить текущего клиента
                m_arPanels[(int)TypePanel.Client].Activate(false); m_arPanels[(int)TypePanel.Client].Stop();
                // запустить нового
                m_arPanels[(int)TypePanel.Client].Start();
                bIsConnected = (m_arPanels[(int)TypePanel.Client] as PanelClient).IsConnected;
                // проверить результат подключения
                if (bIsConnected == true)
                    // роль (клиент) осталась без изменений
                    m_arPanels[(int)TypePanel.Client].Activate(true);
                else
                    // роль изменилась
                    _typePanelEnabled = TypePanel.Server;
                // проверить изменение роли
                if (!(prevTypePanelEnabled == _typePanelEnabled)) {
                // роль изменилась - приложение становится сервером
                    m_arPanels[(int)prevTypePanelEnabled].Enabled = false;
                    m_arPanels[(int)_typePanelEnabled].Enabled = true;
                } else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelClientServer::reConnClient () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private class PanelClient : PanelCS
        {
            /// <summary>
            /// Экземпляр клиента
            /// </summary>
            private Pipes.Client _client { get { return _pipe == null ? null : _pipe as Pipes.Client; } }

            public PanelClient(string[] arServerName)
                : base(arServerName, TypePanel.Client)
            {
            }

            public bool IsConnected { get { return (!(_client == null)) && (_client.IsConnected == true); } }
            /// <summary>
            /// Запуск экземпляра клиента
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения, не передавать значение если нужно перебирать список</param>
            protected override void runStart(string name_serv)
            {
                _pipe = null;

                thread = new Thread(connectToServer);//Инициализация экземпляра потока
                if (name_serv.Equals(string.Empty) == true)
                    thread.Start(m_servers);//Старт потока со списком серверов из конструктора
                else
                    thread.Start(new string[] { name_serv }); //Старт потока со списком серверов переданным в initialize
                thread.Join();
                // ожидать создания клиента
                if ((!(_client == null))
                    && (_client.IsConnected == true))
                    argCommand.Enabled = true;
                else
                    ;
            }
            /// <summary>
            /// Установить соответствие текущей машины и свойства переданного в аргументе
            /// </summary>
            /// <param name="host">Наименоввание(IP-адрес) рабочей станции в сети</param>
            /// <returns>Признак соответствия</returns>
            private static bool isEqualeHost(string host)
            {
                bool bRes = host.Equals(Environment.MachineName);

                if (bRes == false)
                    bRes = !(System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.ToList().Find(
                        adr => { return adr.ToString().Equals(host); }) == null);
                else
                    ;

                return bRes;
            }
            /// <summary>
            /// Подключение к серверу (метод отдельного потока)
            /// </summary>
            /// <param name="data">Массив имен серверов</param>
            private void connectToServer(object data)
            {
                string[] servers = (data as string[]);
                int iAttempt = -1;

                lock (thisLock)
                {
                    //Перебор серверов для подключения
                    foreach (string server in servers)
                    {
                        if (isEqualeHost(server) == false) {
                            iAttempt = 0;

                            _pipe = new Pipes.Client(server, MS_TIMEOUT_CONNECT_TO_SERVER);//инициализация клиента

                            //Подписка на события клиента
                            _client.ReadMessage += new EventHandler(recievedMessage);
                            _client.ResServ += new EventHandler(resClient);
                            //несколько попыток подключения при неудаче
                            while (iAttempt < MAX_ATTEMPT_CONNECT)
                            {
                                //!!!! необходимо будет вставить условие на исключение собственного имени из перебора
                                _client.StartClient();//Выполняем старт клиента и пытаемся подключиться к серверу
                                if (_client.IsConnected == true)//Если соединение установлено то
                                {
                                    //m_type_app = TypePanel.Client;//Тип экземпляра устанавливаем Клиент
                                    m_myName = _client.m_Name;//Устанавливаем собственное имя равное имени клиента
                                    _client.WriteMessage(Pipes.Pipe.COMMAND.Name.ToString());//Запрашиваем имя сервера
                                    //Прерываем попытки подключения
                                    break;
                                }
                                else
                                    iAttempt++;
                            }
                            //Проверить было ли установлено соединение
                            if (_client.IsConnected == false)
                            {// подключениене не было установлено
                                //отписываемся от событий
                                _client.ReadMessage -= recievedMessage;
                                _client.ResServ -= resClient;
                            }
                            else
                            {
                                // прерываем перебор серверов
                                break;
                            }

                        }
                    }
                }
            }
            /// <summary>
            /// Отправка сообщения из клиента
            /// </summary>
            /// <param name="message">Сообщение для отправки</param>
            protected override void sendMessage(string message, string notUses)
            {
                sendMessage(message);
            }

            private void sendMessage(string message)
            {
                Logging.Logg().Debug(string.Format(@"PanelClientServer.PanelClient::sendMessage (text={0}) - ...", message), Logging.INDEX_MESSAGE.NOT_SET);

                _client.WriteMessage(message);//Отправка сообщения
                addMessage(new Pipes.Pipe.ReadMessageEventArgs(message, m_servName), TypeMes.Output);//Добавление сообщения и обработка
            }

            protected override int addMessage(string com_mes, string arg, string name)
            {
                int iRes = base.addMessage(com_mes, arg, name);

                if (iRes == 0)
                {
                    Logging.Logg().Debug(string.Format(@"PanelClientServer.PanelClient::addMessage (COMMAND={0}, ARG={1}, name={2}) - ...", com_mes, arg, name), Logging.INDEX_MESSAGE.NOT_SET);

                    //Pipes.Pipe.COMMAND com = Pipes.Pipe.COMMAND.Unknown;

                    //for (com = Pipes.Pipe.COMMAND.Unknown; com < (Pipes.Pipe.COMMAND.Count - 1); com++)
                    //    if (com_mes.Equals((com + 1).ToString()) == true)
                    //    {
                    //        switch (com)
                    //        {
                    //            case Pipes.Pipe.COMMAND.DateTime:
                    //                sendMessage(com_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + DateTime.Now.ToString());
                    //                break;
                    //            case Pipes.Pipe.COMMAND.GetName:
                    //                sendMessage(KEY_MYNAME + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + m_myName);
                    //                break;
                    //            case Pipes.Pipe.COMMAND.GetStat:
                    //                sendMessage(TypePanel.Client.ToString());
                    //                break;
                    //            case Pipes.Pipe.COMMAND.ReConnect:
                    //                _client.SendDisconnect();//отправка сообщения о разрыве соединения
                    //                Invoke(d_reconn, new object[] { arg, false });
                    //                break;
                    //            case Pipes.Pipe.COMMAND.Start:
                    //                Invoke(d_statLbl, true);
                    //                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, ID_EVENT.Start } });
                    //                break;
                    //            case Pipes.Pipe.COMMAND.Stop:
                    //                Invoke(d_statLbl, false);
                    //                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, ID_EVENT.Stop } });
                    //                break;
                    //            case Pipes.Pipe.COMMAND.SetStat:
                    //                if (arg.Equals(TypePanel.Server.ToString()) == true)
                    //                {
                    //                    _client.SendDisconnect();
                    //                    Invoke(d_reconn, new object[] { string.Empty, true });
                    //                }
                    //                else
                    //                    ;
                    //                break;
                    //            case Pipes.Pipe.COMMAND.Status:
                    //                sendMessage(com_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + Pipes.Pipe.MESSAGE_RECIEVED_OK);
                    //                break;
                    //            case Pipes.Pipe.COMMAND.Exit:
                    //                Invoke(d_exit);
                    //                break;
                    //            case Pipes.Pipe.COMMAND.Disconnect:
                    //            // обработка не требуется
                    //            default:
                    //                break;
                    //        }

                    //        break; // прервать сравнение следующих команд
                    //    }
                    //    else
                    //        ;
                    //// проверить найдена ли команда
                    //if (com > (Pipes.Pipe.COMMAND.Count - 1))
                    //    // команда не найдена
                    //    //??? в команде присутствует некоманда
                    //    if ((com_mes.Equals(Pipes.Client.MESSAGE_CONNECT_TO_SERVER_OK) == true)
                    //        || (com_mes.Equals(Pipes.Client.MESSAGE_DISCONNECT) == true)
                    //        || (com_mes.Equals(Pipes.Pipe.COMMAND.Disconnect.ToString()) == true)
                    //        || (com_mes.Equals(m_servName) == true)
                    //        )
                    //        // обработка не требуется
                    //        ;
                    //    else
                    //        throw new Exception(string.Format(@"PanelClient::addMessage (COMMAND={0}) - неизвестная команда...", com_mes));
                    //else
                    //    ; // команда найдена И/ИЛИ обработана

                    if (com_mes.Equals(Pipes.Pipe.COMMAND.DateTime.ToString()) == true)//Обработка запроса даты
                        sendMessage(com_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + DateTime.Now.ToString());
                    else
                        if (com_mes.Equals(Pipes.Pipe.COMMAND.Name.ToString()) == true)//Обработка запроса имени
                        //??? требуется проверка наличия аргумента. М.б. это пришел ответ с запрашенным именем
                            sendMessage(com_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + m_myName);
                        else
                            if (com_mes.Equals(Pipes.Pipe.COMMAND.PipeRole.ToString()) == true)//Обработка запроса типа экземпляра
                            //??? требуется проверка наличия аргумента. М.б. это пришел ответ с запрошенной ролью
                                sendMessage(TypePanel.Client.ToString());
                            else
                                if (com_mes.Equals(Pipes.Pipe.COMMAND.ReConnect.ToString()) == true)//Обработка запроса на переподключение
                                {
                                    _client.SendDisconnect();//отправка сообщения о разрыве соединения
                                    Invoke(d_reconn, new object[] { arg, false });
                                }
                                else
                                    if (com_mes.Equals(Pipes.Pipe.COMMAND.Start.ToString()) == true)//обработка запроса запуска
                                    {
                                        Invoke(d_statLbl, true);
                                        DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, Pipes.Pipe.COMMAND.Start } });
                                    }
                                    else
                                        if (com_mes.Equals(Pipes.Pipe.COMMAND.Stop.ToString()) == true)//обработка запроса остановки
                                        {
                                            Invoke(d_statLbl, false);
                                            DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, Pipes.Pipe.COMMAND.Stop } });
                                        }
                                        else
                                            if (com_mes.Equals(Pipes.Pipe.COMMAND.PipeRole.ToString()) == true)//обработка запроса изменения типа экземпляра
                                            {
                                                if (arg.Equals(TypePanel.Server.ToString()) == true)
                                                {
                                                    _client.SendDisconnect();
                                                    Invoke(d_reconn, new object[] { string.Empty, true });
                                                }
                                            }
                                            else
                                                if (com_mes.Equals(Pipes.Pipe.COMMAND.AppState.ToString()) == true)//обработка запроса извещения о состоянии экземпляра
                                                {
                                                    // запомнить текущее состояние взаимодействующего экземпляра
                                                    stateRemotePanelWork = (PanelWork.STATE)arg[0];
                                                    // отправить информацию о своем состоянии
                                                    sendMessage(com_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + stateLocalPanelWork/*Pipes.Pipe.MESSAGE_RECIEVED_OK*/);                                                    
                                                }
                                                else
                                                    if (com_mes.Equals(Pipes.Pipe.COMMAND.Exit.ToString()) == true)
                                                    {
                                                        Invoke(d_exit);
                                                    }
                                                    else
                                                        //??? в команде присутствует некоманда
                                                        if ((com_mes.Equals(Pipes.Client.MESSAGE_CONNECT_TO_SERVER_OK) == true)
                                                            || (com_mes.Equals(Pipes.Client.MESSAGE_DISCONNECT) == true)
                                                            || (com_mes.Equals(Pipes.Pipe.COMMAND.Disconnect.ToString()) == true)
                                                            || (com_mes.Equals(m_servName) == true))
                                                            // обработка не требуется
                                                            ;
                                                        else
                                                        {
                                                            iRes = -2;
                                                            throw new Exception(string.Format(@"PanelClient::addMessage (COMMAND={0}) - неизвестная команда...", com_mes));
                                                        }
                }
                else
                    //??? двойная ошибка - пустая команда, проверка производилась выше
                    ;

                return iRes;
            }

            protected override void recievedMessage(object sender, EventArgs e)
            {
                base.recievedMessage(sender, e);

                m_servName = (e as Pipes.Pipe.ReadMessageEventArgs).IdServer;
            }

            private void resClient(object sender, EventArgs e)
            {//Pipes.Client.ResServeventArgs
                Invoke(d_reconn, string.Empty);
            }

            public override void Stop()
            {
                if (_client != null)
                {
                    if (_client.b_Active == true)
                    {
                        _client.SendDisconnect();//Отправка сообщения о разрыве соединения
                        _client.StopClient();//Остановка клиента
                    }
                }

                base.Stop();
            }

            protected override void reconnect(string new_server = "")
            {
                Logging.Logg().Debug(string.Format(@"PanelClientServer.PanelClient::reconnect (new_server={0}) - ...", new_server), Logging.INDEX_MESSAGE.NOT_SET);

                _client.StopClient();//остановка клиента

                base.reconnect(new_server);
            }
        }

        private class PanelServer : PanelCS
        {
            /// <summary>
            /// Экземпляр сервера
            /// </summary>
            private Pipes.Server _server { get { return _pipe == null ? null : _pipe as Pipes.Server; } }

            public PanelServer(string[] arServerName)
                : base(arServerName, TypePanel.Server)
            {
                d_disconnect = disconnect_client;
            }

            private System.Windows.Forms.Timer timerUpdateStatus;

            /// <summary>
            /// Запуск экземпляра сервера
            /// </summary>
            protected override void runStart(string notUse)
            {
                _pipe = null;

                lock (thisLock)
                {
                    if (thread != null)
                    {
                        if (thread.ThreadState == ThreadState.Running)
                        {
                            thread.Abort();//останавливем поток 
                        }

                        thread = null; //обнуляем его
                    }
                    thread = new Thread(initialize);//Инициализируем поток создания сервера
                    //??? подождать завершения - определить результат
                    thread.Start();//Потока

                    //Включение компонентов формы для сервера
                    cbClients.Enabled = true;
                    argCommand.Enabled = true;
                    rbStatus.Enabled = true;
                    rbStatus.Checked = true;

                    timerUpdateStatus = new System.Windows.Forms.Timer();
                    timerUpdateStatus.Interval = MS_TIMER_UPDATE_STATUS;
                    timerUpdateStatus.Tick += new EventHandler(timerUpdateStat_Tick);

                    timerUpdateStatus.Start();
                }
            }

            public override void Start()
            {
                base.Start();
            }
            /// <summary>
            /// Запуск сервера (метод отдельного потока)
            /// </summary>
            /// <param name="data"></param>
            private void initialize(object data)
            {
                _pipe = new Pipes.Server();//Создание экземпляра сервера
                //Подписка на события сервера
                _server.ReadMessage += new EventHandler(recievedMessage);
                _server.ConnectClient += new EventHandler(addToComList);
                _server.DisConnectClient += new EventHandler(delFromComList);
                _server.StartServer();//запуск экземпляра сервера
                //m_type_app = TypePanel.Server;//устанавливаем тип экземпляра приложения Сервер
                m_myName = _server.m_Name;//Устанавливаем собственное имя равное имени сервера
            }
            /// <summary>
            /// Экземпляр делегата disconnect
            /// </summary>
            private DelegateFunc d_disconnect;

            private void disconnect_client()
            {
                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, ID_EVENT.Disconnect } });
            }

            protected override void delFromComList(object sender, EventArgs e)
            {
                base.delFromComList(sender, e);
                Invoke(d_disconnect);
            }

            /// <summary>
            /// Отправка сообщения из сервера
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="nameClient">Имя клиента</param>
            protected override void sendMessage(string message, string nameClient)
            {
                _server.WriteMessage(nameClient, message);//Отправка
                addMessage(new Pipes.Pipe.ReadMessageEventArgs(message, nameClient), TypeMes.Output);//Добавление сообщения и обработка 
            }

            protected override int addMessage(string com_mes, string arg, string name)
            {
                int iRes = base.addMessage(com_mes, arg, name);

                if (iRes == 0)
                {
                    Logging.Logg().Debug(string.Format(@"PanelClientServer.PanelServer::addMessage (COMMAND={0}, ARG={1}, name={2}) - ...", com_mes, arg, name), Logging.INDEX_MESSAGE.NOT_SET);

                    Pipes.Pipe.COMMAND com = Pipes.Pipe.COMMAND.Unknown;

                    for (com = Pipes.Pipe.COMMAND.Unknown; com < (Pipes.Pipe.COMMAND.Count - 1); com++)
                        if (com_mes.Equals((com + 1).ToString()) == true)
                        {
                            switch (com)
                            {
                                case Pipes.Pipe.COMMAND.DateTime:
                                    sendMessage(com.ToString() + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + DateTime.Now.ToString(), name);
                                    break;
                                case Pipes.Pipe.COMMAND.Name:
                                    sendMessage(m_myName, name);
                                    break;
                                case Pipes.Pipe.COMMAND.PipeRole:
                                    if (arg.Equals(string.Empty) == true)
                                    // ответ на запрос роли
                                        sendMessage(TypePanel.Server.ToString(), name);
                                    else
                                    // обработка команды на изменение роли
                                        if (arg.Equals(TypePanel.Client.ToString()) == true) {
                                            _server.SendDisconnect();
                                            Invoke(d_reconn, new object[] { string.Empty, false });
                                        }
                                        else
                                            ;
                                    break;
                                case Pipes.Pipe.COMMAND.Start:
                                    Invoke(d_statLbl, true);
                                    DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, Pipes.Pipe.COMMAND.Start } });
                                    break;
                                case Pipes.Pipe.COMMAND.Stop: // прием команды 
                                    Invoke(d_statLbl, false);
                                    DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, Pipes.Pipe.COMMAND.Stop } });
                                    break;
                                case Pipes.Pipe.COMMAND.AppState: // прием состояния взаимодействующего приложения
                                    // отображение его на панели
                                    Invoke(d_updateLV, new object[] { name, arg });
                                    break;
                                case Pipes.Pipe.COMMAND.Exit: // прием команды на завершение приложения
                                    Invoke(d_exit);
                                    break;
                                case Pipes.Pipe.COMMAND.Connect:
                                case Pipes.Pipe.COMMAND.Disconnect:
                                // обработка не требуется
                                default:
                                    break;
                            }

                            break;
                        }
                        else
                            ;
                    // проверить найдена ли команда
                    if (com > (Pipes.Pipe.COMMAND.Count - 1))
                        // команда не найдена                        
                        throw new Exception(string.Format(@"PanelServer::addMessage (COMMAND={0}) - неизвестная команда...", com_mes));
                    else
                        ; // команда найдена И/ИЛИ обработана

                    //if (com_mes.Equals(Pipes.Pipe.COMMAND.DateTime.ToString()) == true)//запрос даты
                    //    sendMessage(com_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + DateTime.Now.ToString(), name);
                    //else
                    //    if (com_mes.Equals(Pipes.Pipe.COMMAND.GetName.ToString()) == true)//запрос имени
                    //        sendMessage(m_myName, name);
                    //    else
                    //        if (com_mes.Equals(Pipes.Pipe.COMMAND.GetStat.ToString()) == true)//запрос типа текущего экземпляра
                    //            sendMessage(TypePanel.Server.ToString(), name);
                    //        else
                    //            if (com_mes.Equals(Pipes.Pipe.COMMAND.Start.ToString()) == true)//запрос запуска
                    //            {
                    //                Invoke(d_statLbl, true);
                    //                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, ID_EVENT.Start } });
                    //            }
                    //            else
                    //                if (com_mes.Equals(Pipes.Pipe.COMMAND.Stop.ToString()) == true)//запрос остановки
                    //                {
                    //                    Invoke(d_statLbl, false);
                    //                    DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, ID_EVENT.Stop } });
                    //                }
                    //                else
                    //                    if (com_mes.Equals(Pipes.Pipe.COMMAND.Status.ToString()) == true)//обработка запроса изменения типа экземпляра
                    //                    {
                    //                        Invoke(d_updateLV, new object[] { name, arg });
                    //                    }
                    //                    else
                    //                        if (com_mes.Equals(Pipes.Pipe.COMMAND.SetStat.ToString()) == true)//обработка запроса изменения типа экземпляра
                    //                        {
                    //                            if (arg.Equals(TypePanel.Client.ToString()) == true)
                    //                            {
                    //                                _server.SendDisconnect();
                    //                                Invoke(d_reconn, new object[] { string.Empty, false });
                    //                            }
                    //                        }
                    //                        else
                    //                            if (com_mes.Equals(Pipes.Pipe.COMMAND.Exit.ToString()) == true)
                    //                                Invoke(d_exit);
                    //                            else
                    //                                if ((com_mes.Equals(Pipes.Pipe.COMMAND.Connect.ToString()) == true)
                    //                                    || (com_mes.Equals(Pipes.Pipe.COMMAND.Disconnect.ToString()) == true))
                    //                                    // ничего не делать
                    //                                    ;
                    //                                else
                    //                                {
                    //                                    // неизвестная команда
                    //                                    iRes = -2;
                    //                                    throw new Exception(string.Format(@"PanelServer::addMessage (COMMAND={0}) - неизвестная команда...", com_mes));
                    //                                }
                }
                else
                    // пустая команда
                    ;

                return iRes;
            }
            /// <summary>
            /// Метод обработки события таймера
            ///  отправка сообщений клиентам (опрос статуса)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - таймер</param>
            /// <param name="e">Аргумент события</param>
            private void timerUpdateStat_Tick(object sender, EventArgs ev)
            {
                try
                {
                    foreach (string client in cbClients.Items)
                        sendMessage(Pipes.Pipe.COMMAND.AppState.ToString() + +Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + stateLocalPanelWork, client);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::timerUpdateStat_Tick () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            protected void addToComList(object sender, EventArgs e)
            {
                Invoke(d_operCB, new object[] { (e as Pipes.Server.ConnectionClientEventArgs).IdServer, true });
            }

            public override void Stop()
            {
                if (_server != null)
                {
                    timerUpdateStatus.Stop();
                    _server.SendDisconnect();//Отправка сообщения о разрыве соединения
                    _server.StopServer();//Остановка сервера
                }

                base.Stop();
            }

            protected override void reconnect(string new_server = "")
            {
                timerUpdateStatus.Stop();
                cbClients.Items.Clear();
                lvStatus.Items.Clear();
                _server.SendDisconnect();
                //m_server.StopServer();//остановка сервера
                timerUpdateStatus.Start();

                base.reconnect(new_server);
            }
        }

        private abstract partial class PanelCS : PanelCommonDataHost
        {
            #region Переменные и константы

            //protected const string KEY_MYNAME = @"MyName";
            protected static int MS_TIMER_UPDATE_STATUS = 10000;

            private enum INDEX_COLUMN_MESSAGES : short
            {
                UNKNOWN = -1
                    , DATETIME, MESSAGE, SOURCE,
                TYPE_IO
                    , COUNT
            }
            //Колонки таблицы со списком сообщений
            private string[] m_arTextColumnMessages = new string[(int)INDEX_COLUMN_MESSAGES.COUNT] { "Дата/время", "Сообщение", "Источник", "Вход/исход" };

            /// <summary>
            /// Количество попыток подключения
            /// </summary>
            protected static int MAX_ATTEMPT_CONNECT = 2;
            /// <summary>
            /// Таймаут подключения
            /// </summary>
            protected static int MS_TIMEOUT_CONNECT_TO_SERVER = 500;

            protected Pipes.Pipe _pipe;

            public bool Ready
            {
                get
                {
                    return
                        //(!(m_client == null))
                        //    && (!(m_server == null))
                        m_servers.Length > 1
                            ;
                }
            }

            /// <summary>
            /// Тип сообщения
            /// </summary>
            protected enum TypeMes { Input, Output };

            /// <summary>
            /// Массив имён серверов
            /// </summary>
            protected string[] m_servers;

            /// <summary>
            /// Тип экземпляра приложения
            /// </summary>
            protected TypePanel m_type_panel;

            /// <summary>
            /// Экземпляр делегата добавления строки в DGV
            /// </summary>
            private DelegateFunc d_addRow;

            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в comboBox
            /// </summary>
            protected DelegateStrBoolFunc d_operCB;

            /// <summary>
            /// Делегат для добавления/удаления объекта в listView
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            protected delegate void DelegateStrStrFunc(string name_client, string status);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в listView
            /// </summary>
            protected DelegateStrStrFunc d_updateLV;

            /// <summary>
            /// Делегат для добавления/удаления объекта в comboBox
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            protected delegate void DelegateStrBoolFunc(string name_serv, bool stat = false);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в comboBox
            /// </summary>
            protected DelegateStringFunc d_reconn;

            /// <summary>
            /// Экземпляр делегата изменения label'a
            /// </summary>
            protected DelegateFunc d_statLbl;

            /// <summary>
            /// Экземпляр делегата exit
            /// </summary>
            protected DelegateFunc d_exit;

            /// <summary>
            /// Объект синхронизации
            /// </summary>
            protected Object thisLock;

            /// <summary>
            /// Имя сервера(если экземпляр Client)
            /// </summary>
            protected string m_servName;

            /// <summary>
            /// Имя ПК
            /// </summary>
            protected string m_myName;

            protected Thread thread;
            #endregion

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="arServerName">Список серверов</param>
            public PanelCS(string[] arServerName, TypePanel type)
                : base(5, 20)
            {
                thisLock = new Object();
                m_type_panel = type;
                d_exit = exit_program;

                m_listRowDGVAdding = new ListDGVMessage();

                try
                {
                    InitializeComponent();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelCS::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                rbCommand.CheckedChanged += new EventHandler(rbChecked);
                rbStatus.CheckedChanged += new EventHandler(rbChecked);

                m_servers = arServerName;
            }

            /// <summary>
            /// Признак состояния внешней панели (Работа)
            /// </summary>
            protected PanelWork.STATE stateLocalPanelWork
            {
                get
                {
                    return (Parent as PanelClientServer).StateLocalPanelWork;
                }
            }
            /// <summary>
            /// Состояние удаленной рабочей панели экземпляра
            /// </summary>
            private PanelWork.STATE _stateRemotePanelWork;

            protected PanelWork.STATE stateRemotePanelWork {
                get { return _stateRemotePanelWork; }

                set {
                    if (!(_stateRemotePanelWork == value))
                    // оповестить родительскую панель об изменении состояния
                    //  взаимодействующего экземпляра приложения
                        DataAskedHost(new object[] {
                            new object[] { this
                                , m_type_panel
                                , TypeMes.Input
                                , ID_EVENT.State //??? Pipes.Pipe.COMMAND.Start
                                , value // состояние взаимодействующего экземпляра
                        } });
                    else
                        ;

                    _stateRemotePanelWork = value;
                }
            }

            public void UpdatePanelWorkState()
            {
                if (IsHandleCreated == true)
                    if (InvokeRequired == true)
                        BeginInvoke(d_statLbl);
                    else
                        d_statLbl();
                else
                    ;
            }

            #region Initialize
            /// <summary> 
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary> 
            /// Обязательный метод для поддержки конструктора - не изменяйте 
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                string[] column_name = { "ClientName", "Status", "LastUpdate" };
                this.dgvMessage = new System.Windows.Forms.DataGridView();
                this.commandBox = new System.Windows.Forms.ListBox();
                this.btnSendMessage = new System.Windows.Forms.Button();
                this.cbClients = new System.Windows.Forms.ComboBox();
                this.lblClients = new System.Windows.Forms.Label();
                this.argCommand = new System.Windows.Forms.TextBox();
                this.lblArg = new System.Windows.Forms.Label();
                this.lblStat = new System.Windows.Forms.Label();
                this.lblType = new System.Windows.Forms.Label();
                this.panelStatus = new System.Windows.Forms.TableLayoutPanel();
                this.panelCommand = new System.Windows.Forms.TableLayoutPanel();
                this.rbStatus = new System.Windows.Forms.RadioButton();
                this.rbCommand = new System.Windows.Forms.RadioButton();
                this.lvStatus = new System.Windows.Forms.ListView();
                ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).BeginInit();

                this.SuspendLayout();
                // 
                // lvStatus
                // 
                this.lvStatus.Name = "lvStatus";
                this.lvStatus.TabIndex = 0;
                this.lvStatus.Dock = DockStyle.Fill;
                this.lvStatus.Columns.AddRange(new ColumnHeader[] { new ColumnHeader(), new ColumnHeader(), new ColumnHeader(), });

                this.lvStatus.FullRowSelect = true;
                this.lvStatus.MultiSelect = false;
                this.lvStatus.Size = new System.Drawing.Size(277, 245);
                this.lvStatus.UseCompatibleStateImageBehavior = false;
                this.lvStatus.View = System.Windows.Forms.View.Details;
                foreach (ColumnHeader column in this.lvStatus.Columns)
                {
                    column.Text = column_name[column.Index];
                    column.Name = column_name[column.Index];
                    column.Width = 90;
                }
                this.lvStatus.GotFocus += new EventHandler(lvStatus_OnGotFocus);

                // 
                // dgvMessage
                // 
                this.dgvMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvMessage.Name = "dgvMessage";
                this.dgvMessage.Size = new System.Drawing.Size(513, 150);
                this.dgvMessage.TabIndex = 0;
                this.dgvMessage.Dock = DockStyle.Fill;
                this.dgvMessage.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.dgvMessage.AllowUserToAddRows = false;
                // 
                // commandBox
                // 
                this.commandBox.FormattingEnabled = true;
                this.commandBox.Name = "commandBox";
                //this.commandBox.Size = new System.Drawing.Size(221, 277);
                this.commandBox.TabIndex = 1;
                this.commandBox.Dock = DockStyle.Fill;
                // 
                // btnSendMessage
                // 
                this.btnSendMessage.AutoSize = true;
                this.btnSendMessage.Name = "btnSendMessage";
                //this.btnSendMessage.Size = new System.Drawing.Size(75, 23);
                this.btnSendMessage.TabIndex = 2;
                this.btnSendMessage.Text = "Отправить";
                this.btnSendMessage.UseVisualStyleBackColor = true;
                this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
                // 
                // cbClients
                // 
                this.cbClients.FormattingEnabled = true;
                this.cbClients.Name = "cbClients";
                this.cbClients.Size = new System.Drawing.Size(131, 21);
                this.cbClients.TabIndex = 3;
                this.cbClients.Dock = DockStyle.Fill;
                // 
                // lblClients
                // 
                this.lblClients.AutoSize = true;
                this.lblClients.Name = "lblClients";
                this.lblClients.Size = new System.Drawing.Size(92, 13);
                this.lblClients.TabIndex = 4;
                this.lblClients.Text = "Клиенты/сервер";
                // 
                // lblType
                // 
                this.lblType.AutoSize = true;
                this.lblType.Name = "lblType";
                this.lblType.Size = new System.Drawing.Size(92, 13);
                this.lblType.TabIndex = 8;
                this.lblType.Text = m_type_panel.ToString();
                this.lblType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                // 
                // argCommand
                // 
                this.argCommand.AutoSize = true;
                this.argCommand.Name = "argCommand";
                //this.argCommand.Size = new System.Drawing.Size(131, 20);
                this.argCommand.TabIndex = 5;
                this.argCommand.Dock = DockStyle.Fill;
                // 
                // lblArg
                // 
                this.lblArg.AutoSize = true;
                this.lblArg.Name = "lblArg";
                this.lblArg.TabIndex = 6;
                this.lblArg.Text = "Аргумент";
                // 
                // lblStat
                // 
                this.lblStat.AutoSize = true;
                this.lblStat.BackColor = System.Drawing.Color.Red;
                this.lblStat.Name = "lblStat";
                this.lblStat.TabIndex = 7;
                this.lblStat.Text = "Paused";
                // 
                // rbStatus
                // 
                this.rbStatus.AutoSize = true;
                this.rbStatus.Name = "rbStatus";
                this.rbStatus.TabIndex = 9;
                this.rbStatus.Text = "Статус";
                this.rbStatus.Enabled = false;
                // 
                // rbCommand
                // 
                this.rbCommand.AutoSize = true;
                this.rbCommand.Name = "rbCommand";
                this.rbCommand.TabIndex = 10;
                this.rbCommand.Text = "Команды";
                this.rbCommand.Checked = true;
                // 
                // panelCommand
                // 
                //this.panelCommand.AutoSize = true;
                this.panelCommand.Name = "panelCommand";
                this.panelCommand.TabIndex = 10;
                this.panelCommand.RowCount = 19;
                this.panelCommand.Dock = DockStyle.Fill;
                // 
                // panelStatus
                // 
                //this.panelCommand.AutoSize = true;
                this.panelStatus.Name = "panelStatus";
                this.panelStatus.TabIndex = 10;
                this.panelStatus.RowCount = 19;
                this.panelStatus.Dock = DockStyle.Fill;
                this.panelStatus.Visible = false;

                // 
                // PanelCS
                //
                this.Controls.Add(this.rbStatus, 0, 0);
                this.SetRowSpan(this.rbStatus, 2);
                this.SetColumnSpan(this.rbStatus, 1);

                this.Controls.Add(this.rbCommand, 1, 0);
                this.SetRowSpan(this.rbCommand, 2);
                this.SetColumnSpan(this.rbCommand, 1);


                this.Controls.Add(this.panelCommand, 0, 2);
                this.SetRowSpan(this.panelCommand, 18);
                this.SetColumnSpan(this.panelCommand, 2);

                this.Controls.Add(this.panelStatus, 0, 2);
                this.SetRowSpan(this.panelStatus, 18);
                this.SetColumnSpan(this.panelStatus, 2);

                #region Command
                panelCommand.Controls.Add(this.lblStat, 0, 0);

                panelCommand.Controls.Add(this.lblClients, 0, 1);
                panelCommand.Controls.Add(this.cbClients, 0, 2);
                panelCommand.SetRowSpan(this.cbClients, 2);

                panelCommand.Controls.Add(this.lblArg, 0, 4);
                panelCommand.Controls.Add(this.argCommand, 0, 5);
                panelCommand.SetRowSpan(this.argCommand, 2);

                panelCommand.Controls.Add(this.commandBox, 0, 7);
                panelCommand.SetRowSpan(this.commandBox, 10);

                panelCommand.Controls.Add(this.btnSendMessage, 0, 17);
                panelCommand.SetRowSpan(this.btnSendMessage, 2);
                #endregion

                #region Status
                panelStatus.Controls.Add(this.lvStatus, 0, 0);
                panelStatus.SetRowSpan(this.lvStatus, 19);
                #endregion

                this.Controls.Add(this.lblType, 2, 0);

                this.Controls.Add(this.dgvMessage, 2, 1);
                this.SetRowSpan(dgvMessage, 19);
                this.SetColumnSpan(dgvMessage, 3);

                this.Name = "PanelCS";
                this.Size = new System.Drawing.Size(519, 441);
                ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).EndInit();
                this.ResumeLayout(false);
                this.PerformLayout();

                Enabled = false;
            }

            private void lvStatus_OnGotFocus(object sender, EventArgs e)
            {
                throw new NotImplementedException();
            }

            #endregion

            private System.Windows.Forms.DataGridView dgvMessage;
            private System.Windows.Forms.ListBox commandBox;
            private System.Windows.Forms.Button btnSendMessage;
            protected System.Windows.Forms.ComboBox cbClients;
            private System.Windows.Forms.Label lblClients;
            protected System.Windows.Forms.TextBox argCommand;
            private System.Windows.Forms.Label lblArg;
            private System.Windows.Forms.Label lblStat;
            private System.Windows.Forms.Label lblType;
            private System.Windows.Forms.TableLayoutPanel panelStatus;
            private System.Windows.Forms.TableLayoutPanel panelCommand;
            protected System.Windows.Forms.RadioButton rbStatus;
            protected System.Windows.Forms.RadioButton rbCommand;
            protected System.Windows.Forms.ListView lvStatus;
            #endregion

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            //public override bool Activate(bool active)
            //{
            //    bool bRes = base.Activate(active);

            //    //if (m_bIsFirstActivate == true)
            //    if ((bRes == true)
            //        && (IsFirstActivated == true))
            //    {
            //    }
            //    else
            //        ;

            //    return bRes;
            //}

            /// <summary>
            /// Запуск панели
            /// </summary>
            public override void Start()
            {
                base.Start();

                //Инициализация делегатов
                d_addRow = addRowToDGV;
                d_operCB = operCBclient;
                d_reconn = reconnect;
                d_statLbl = setLbl;
                d_updateLV = lvStatusUpdate;
                //Размещение комманд в Control
                getCommandToList();

                //Инициализация экземпляра клиента/сервера
                initialize(string.Empty);
            }

            #region Создание клиента/сервера
            /// <summary>
            /// Инициализация объекта клиента/сервера
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения</param>
            /// <param name="is_serv">Если нужен сервер без запуска клиента то установить true</param>
            private void initialize(string name_serv)
            {
                cbClients.Enabled = false;
                argCommand.Enabled = false;

                //Тип экземпляра (клиент/сервер/неопределено)
                //m_type_app = TypePanel.Unknown;

                //Добавление колонок в DGV
                if (dgvMessage.Columns.Count == 0)
                    foreach (string col in m_arTextColumnMessages)
                        dgvMessage.Columns.Add(col, col);
                else
                    ;

                //Проверка флага запуска сервера
                runStart(name_serv);
            }

            /// <summary>
            /// Запуск экземпляра клиента
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения, не передавать значение если нужно перебирать список</param>
            protected abstract void runStart(string name_serv);

            protected virtual void recievedMessage(object sender, EventArgs e)
            {
                addMessage(e as Pipes.Pipe.ReadMessageEventArgs, TypeMes.Input);//Добавление сообщения и обработка
            }

            #endregion

            #region Обработчики сервера

            protected virtual void delFromComList(object sender, EventArgs e)
            {
                Invoke(d_operCB, new object[] { (e as Pipes.Server.ConnectionClientEventArgs).IdServer, false });
            }

            #endregion

            #region Обработчики клиента

            #endregion

            #region Отправка сообщений
            /// <summary>
            /// Отправка сообщения от клиента/сервера
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="nameClient">Имя клиента</param>
            protected abstract void sendMessage(string message, string nameClient);
            #endregion

            private void rbChecked(object sender, EventArgs ev)
            {
                try
                {
                    if ((sender as RadioButton).Name == "rbStatus")
                    {
                        if ((sender as RadioButton).Checked == true)
                        {
                            panelCommand.Visible = false;
                            panelStatus.Visible = true;
                        }
                    }
                    else
                        ;

                    if ((sender as RadioButton).Name == "rbCommand")
                    {
                        if ((sender as RadioButton).Checked == true)
                        {
                            panelStatus.Visible = false;
                            panelCommand.Visible = true;
                        }
                    }
                    else
                        ;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::::rbChecked () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Обработчик кнопки Отправить
            /// </summary>
            /// <param name="sender">Объект - инициатор события - кнопка</param>
            /// <param name="e">Аргумент события (пустое,  не используется)</param>
            private void btnSendMessage_Click(object sender, EventArgs e)
            {
                switch (m_type_panel)
                {
                    case TypePanel.Server:
                        if (commandBox.SelectedItem.ToString() == Pipes.Pipe.COMMAND.ReConnect.ToString())//Добавляет аргумент для реконнекта
                        {
                            sendMessage(commandBox.SelectedItem.ToString() + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + argCommand.Text, cbClients.Text);
                        }
                        else
                            sendMessage(commandBox.SelectedItem.ToString(), cbClients.Text);
                        break;

                    case TypePanel.Client:
                        sendMessage(commandBox.SelectedItem.ToString() + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + argCommand.Text, string.Empty);
                        break;
                }
                argCommand.Clear();
            }
            /// <summary>
            /// Список сообщений (буфер) для отображения в элементе упарвления "Список сообщений"
            /// </summary>
            private ListDGVMessage m_listRowDGVAdding;

            protected virtual int addMessage(string com_mes, string arg, string name)
            {
                int iRes = 0;

                if (com_mes.Trim().Equals(string.Empty) == true) {
                    iRes = -1;

                    Logging.Logg().Error(string.Format(@"PanelClientServer.PanelCS::addMessage (com_mes={0}, arg={1}, name={2}) - пустая команда..."
                            , com_mes, arg, name)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    ;

                return iRes;
            }
            /// <summary>
            /// Класс для хранения списка сообщений
            /// </summary>
            private class ListDGVMessage : List<object>
            {
                /// <summary>
                /// Максимальное кол-во сообщений, остальные (при переполнении) удаляются
                /// </summary>
                private int MAX_COUNT_MESSAGE = 16;
                /// <summary>
                /// Структура для хранения объектов одного сообщения
                /// </summary>
                private struct DGVMessage
                {
                    public string m_dtNow
                        , m_message
                        , m_idServer
                        , m_typeMes;
                    /// <summary>
                    /// Конструктор - основной (с параметрами)
                    /// </summary>
                    /// <param name="mes">Сообщение</param>
                    /// <param name="idServer">Источник сообщения</param>
                    /// <param name="typeMes">Тип сообщения (вход/исход)</param>
                    public DGVMessage(string mes, string idServer, TypeMes typeMes)
                    {
                        m_dtNow = DateTime.Now.ToString();
                        m_message = mes;
                        m_idServer = idServer;
                        m_typeMes = typeMes.ToString();
                    }
                    /// <summary>
                    /// Подготовить сообщение к отображению
                    /// </summary>
                    /// <returns>Массив объектов, пригодный к добавлению в виде строки для 'DataGridView'</returns>
                    public object[] ToPrint()
                    {
                        return new object[] { m_dtNow, m_message, m_idServer, m_typeMes };
                    }
                }

                public int New(string mes, string idServer, TypeMes typeMes)
                {
                    this.Add(new DGVMessage(mes, idServer, typeMes));
                    // ограничение кол-ва элементов
                    while (Count > MAX_COUNT_MESSAGE)
                    // удалить самый старый
                        RemoveAt(0);

                    return this.Count;
                }
                /// <summary>
                /// Подготовить сообщение к отображению
                /// </summary>
                /// <param name="indx">Индекс сообщения</param>
                /// <returns>Массив объектов, пригодный к добавлению в виде строки для 'DataGridView'</returns>
                public object[] ToPrint(int indx)
                {
                    return ((DGVMessage)this[indx]).ToPrint();
                }
            }

            /// <summary>
            /// Обработка нового сообщения
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="name">Имя отправителя</param>
            /// <param name="in_mes">true если входящее</param>
            protected void addMessage(Pipes.Pipe.ReadMessageEventArgs mes, TypeMes type_mes)
            {
                string[] arrMessages = mes.Value.Split(Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR); // разбор сообщения
                string command_mes = arrMessages[0] // комманда
                    , argument = string.Empty; // аргумент

                if (arrMessages.Length > 1)
                    argument = arrMessages[1];
                else
                    ;

                try {
                    if (command_mes.Trim().Equals(string.Empty) == false) {
                        // добавление строки в ОЗУ (вкладка может не отображаться, а сообщения сохраняются)
                        m_listRowDGVAdding.New(mes.Value, mes.IdServer, type_mes);

                        if (IsHandleCreated == true)
                            Invoke(d_addRow); // добавление новой строки сообщения в DGV
                        else
                            ;

                        switch (type_mes)
                        {
                            case TypeMes.Input://входящие
                                addMessage(command_mes, argument, mes.IdServer);
                                break;
                            case TypeMes.Output://исходящие
                                if (command_mes.Equals(Pipes.Pipe.COMMAND.Name.ToString()) == true)
                                {
                                    switch (m_type_panel)
                                    {
                                        case TypePanel.Client:
                                            m_servName = argument;//разбор клиентом ответного сообщения с именем сервера
                                            break;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        //??? пустая команда
                        Logging.Logg().Error(string.Format(@"PanelClientServer.PanelCS::addMessage (message={}, name={1}, type_mes={2}) - пустая команда..."
                                , mes.Value, mes.IdServer, type_mes)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e
                        , string.Format(@"PanelClientServer.PanelCS::addMessage (mes={0}, name={1}, typeMes={2}) - ...", mes.Value, mes.IdServer, type_mes)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавление строки в DGV
            /// </summary>
            private void addRowToDGV()
            {
                try {
                    // добавить все сообщения накопленные в "буфере"
                    while (m_listRowDGVAdding.Count > 0) {
                        dgvMessage.Rows.Add(m_listRowDGVAdding.ToPrint(0));//Добавление строки в DGV
                        // удалить сообщение из буфера
                        m_listRowDGVAdding.RemoveAt(0);
                    }
                    // удалить лишние строки из представления
                    while (dgvMessage.RowCount > dgvMessage.DisplayedRowCount(false))
                        dgvMessage.Rows.RemoveAt(0);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"PanelClientServer.PanelCS::addRowToDGV () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод для завершения клиент/серверной части
            /// </summary>
            public override void Stop()
            {
                base.Stop();
            }

            /// <summary>
            /// Добавление списка комманд в ListBox
            /// </summary>
            private void getCommandToList()
            {
                for (Pipes.Pipe.COMMAND command = Pipes.Pipe.COMMAND.Unknown; command < (Pipes.Pipe.COMMAND.Count - 1); command++)//Перебор списка комманд
                {
                    commandBox.Items.Add((command + 1).ToString());
                }
            }

            /// <summary>
            /// Добавление/удаление клиентов из ComboBox
            /// </summary>
            /// <param name="idClient">ИД клиента</param>
            /// <param name="add">true если добавление</param>
            private void operCBclient(string idClient, bool add)
            {
                try {
                    if (add == true)//если добавление то
                    {
                        cbClients.Items.Add(idClient);//добавляем
                        lvStatus.Items.Add(idClient);
                        foreach (ListViewItem item in lvStatus.Items)
                        {
                            if (item.Text == idClient)
                            {
                                item.Name = idClient;
                                item.SubItems.Add(stateRemotePanelWork.ToString());
                                item.SubItems.Add(DateTime.Now.ToString());
                            }
                        }
                    } else//иначе
                    {
                        cbClients.Items.Remove(idClient);//удаляем клиента из списка и обновляем
                        cbClients.Text = string.Empty;
                        cbClients.Refresh();
                        lvStatus.Items.Find(idClient, false)[0].Remove();
                        lvStatus.Refresh();
                    }
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::operCBclient", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод перезапуска клиент/серверной части
            /// </summary>
            /// <param name="new_server">Пустая строка при полном перезапуске или имя сервера для подключения к нему</param>
            protected virtual void reconnect(string new_server = "")
            {
                dgvMessage.Rows.Clear();

                // внутренее сообщение
                DataAskedHost(new object[] { new object[] { this, m_type_panel } });

                //if (new_server.Equals(string.Empty) == false)
                //    initialize(new_server);//подключение к новому серверу или запуск сервера
            }

            /// <summary>
            /// Метод для изменения label'a состояния внешней панели (Работа)
            /// </summary>
            private void setLbl()
            {
                Color clr = Enabled == true ? Color.DarkGray :
                    (stateLocalPanelWork == PanelWork.STATE.Started) ? Color.GreenYellow :
                        (stateLocalPanelWork == PanelWork.STATE.Paused) ? Color.Red :
                            Color.LightGray;

                try {
                    lblStat.BackColor = clr;
                    lblStat.Text = stateLocalPanelWork.ToString();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::setLbl () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод обновления
            /// </summary>
            /// <param name="client">Наименование клиента</param>
            /// <param name="status">Текст ответа</param>
            private void lvStatusUpdate(string client, string state)
            {
                Color clr = Color.Empty;
                string text = string.Empty;
                int col = -1;

                try {
                    foreach (ListViewItem item in lvStatus.Items)
                        if (item.Text.Equals(client) == true) {
                            if (state.Equals(PanelWork.STATE.Started) == true) {
                                clr = Color.LimeGreen;
                                text = DateTime.Now.ToString();
                                col = 2;
                            } else
                                if (state.Equals(PanelWork.STATE.Paused) == true) {
                                    clr = Color.Red;
                                    text = "Err";
                                    col = 1;
                                } else
                                    ;

                            item.SubItems[1].BackColor = clr;
                            item.SubItems[col].Text = text;
                        } else
                            ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"PanelClientServer.PanelCS::lvStatusUpdate (client={0}, status={1}) - ...", client, state), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            private void exit_program()
            {
                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, ID_EVENT.Exit } });
            }
        }
        /// <summary>
        /// Структура для хранения параметров взаимодействия экземпляров приложения
        /// </summary>
        public struct InteractionParameters
        {
            private enum INDEX_PARAMETER {
                UNKNOWN = -1
                , WS, MAIN_PIPE
            , COUNT }
            /// <summary>
            /// Список взаимодействущих серверов на которых выполняются взаимодействующие экземпляры
            /// </summary>
            public string[] m_arNameServers;
            /// <summary>
            /// Наименование главного канала для реализации взаимодействия
            /// </summary>
            public string m_NameMainPipe;
            /// <summary>
            /// Констрктор - основной (с парметрами)
            /// </summary>
            /// <param name="ini">Строка из конфигурационного файла с параметрами</param>
            public InteractionParameters(string ini)
            {
                m_arNameServers = new string[] { };
                m_NameMainPipe = string.Empty;

                List<string> listNameServers = new List<string> ();
                string[] arPars = null
                    , values = null;

                try {
                    // проверить наличие параметров
                    if (ini.Equals(string.Empty) == false) {
                    // параметры в строке есть
                        // получить все параметры
                        arPars = ini.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL]);
                        // цикл по всем переданным параметрам и их значеням
                        foreach (string key_val in arPars)
                            // цикл по всем известным параметрам
                            for (INDEX_PARAMETER par = (INDEX_PARAMETER.UNKNOWN + 1); par < INDEX_PARAMETER.COUNT; par++)
                                if (key_val.IndexOf(par.ToString()) == 0) {
                                // переданная пара парметра ключ:значения распознана
                                    // разделить пару ключ:значение
                                    values = key_val.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE]/*, StringSplitOptions.RemoveEmptyEntries*/);
                                    // проверить успешность разделения пары ключ:значение
                                    if (values.Length == 2)
                                        // проверить наличие значения
                                        if (values[1].Equals(string.Empty) == false)
                                            // в ~ от парамера, установить для него значение
                                            switch (par) {
                                                case INDEX_PARAMETER.WS:
                                                    listNameServers.Add(values[1]);
                                                    break;
                                                case INDEX_PARAMETER.MAIN_PIPE:
                                                    m_NameMainPipe = values[1];
                                                    break;
                                                default:
                                                    break;
                                            }
                                        else
                                            Logging.Logg().Warning(string.Format(@"PanelClientServer.InteractionParameters::ctor (ключ={0}) - значение=не_установлено", par.ToString())
                                                , Logging.INDEX_MESSAGE.NOT_SET);
                                    else
                                        throw new Exception(string.Format(@"PanelClientServer.InteractionParameters::ctor (ключ={0}) - пара ключ:значение не распознана", par.ToString()));
                                    // прервать обработку внутреннего цикла, т.к. в текущей паре параметра ключ:значение, 2-го параметра не можыть быть
                                    break;
                                } else
                                    ; // параметр не найден - перйти к следующему

                        m_arNameServers = listNameServers.ToArray();
                    }
                    else
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelClientServer.InteractionParameters::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public bool Ready { get { return ((!(m_arNameServers == null)) && (m_arNameServers.Length > 1)) && m_NameMainPipe.Equals(string.Empty) == false; } }
        }
    }
}
