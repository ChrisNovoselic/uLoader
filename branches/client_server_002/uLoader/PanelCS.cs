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
    public partial class PanelClienServer : PanelCommonDataHost
    {
        PanelCS m_panelClient,
            m_panelServer;

        string[] m_arServers;

        bool b_statPanelWork;

        System.Windows.Forms.Timer timer;

        public PanelClienServer(string[] arServerName)
            : base(1, 2)
        {
            b_statPanelWork = false;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(timer_Tick);

            m_arServers = arServerName;
            m_panelClient = new PanelCS(m_arServers, PanelCS.TypeApp.Client);
            m_panelClient.Dock = DockStyle.Fill;
            m_panelServer = new PanelCS(m_arServers, PanelCS.TypeApp.Server);
            m_panelServer.Dock = DockStyle.Fill;
            m_panelServer.SetStatEvent += new PanelCS.SetStatEventHandler(panelServerSetStat);
            m_panelClient.SetStatEvent += new PanelCS.SetStatEventHandler(panelServerSetStat);

            m_panelClient.StartWorkEvent += new PanelCS.StartEventHandler(panelStartWork);
            m_panelServer.StartWorkEvent += new PanelCS.StartEventHandler(panelStartWork);

            m_panelClient.StopWorkEvent += new PanelCS.StopEventHandler(panelStopWork);
            m_panelServer.StopWorkEvent += new PanelCS.StopEventHandler(panelStopWork);

            m_panelClient.ExitEvent += new PanelCS.ExitEventHandler(exitProg);
            m_panelServer.ExitEvent += new PanelCS.ExitEventHandler(exitProg);

            m_panelClient.DisconnectEvent += new PanelCS.DisconnectEventHandler(disconnect);
            m_panelServer.DisconnectEvent += new PanelCS.DisconnectEventHandler(disconnect);

            this.Controls.Add(m_panelServer, 0, 0);
            this.Controls.Add(m_panelClient, 0, 1);

        }

        public bool StatPanelWork
        {
            get
            {
                return b_statPanelWork;
            }
            set
            {
                b_statPanelWork = value;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (m_panelClient != null)
                if (m_panelClient.b_IsPanelWork == true)
                {
                    m_panelClient.StartedWork = b_statPanelWork;
                    m_panelServer.Enabled = false;
                    m_panelClient.Enabled = true;
                }
                else
                {
                    m_panelServer.StartedWork = b_statPanelWork;
                    m_panelServer.Enabled = true;
                    m_panelClient.Enabled = false;
                }
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (IsFirstActivated == true)
            {
                m_panelServer.Start();
                m_panelServer.Activate(active);
                m_panelClient.Start();
                m_panelClient.Activate(active);

                timer.Start();
            }

            return bRes;
        }

        /// <summary>
        /// Метод для завершения клиент/серверной части
        /// </summary>
        public void Close()
        {
            m_panelClient.Close();
            m_panelServer.Close();
        }

        private void panelServerSetStat(object sender, PanelCS.SetStatEventArgs e)
        {
            if (e.TypeApp == PanelCS.TypeApp.Client)
            {
                reConnClient();
            }
            if (e.TypeApp == PanelCS.TypeApp.Server)
            {
                //reConnClient();
            }
        }

        private void exitProg(object sender, EventArgs e)
        {
            if (ExitEvent != null)
            {
                ExitEvent(this, new EventArgs());
            }
        }

        private void panelStartWork(object sender, EventArgs e)
        {
            if (StartWorkEvent != null)
            {
                StartWorkEvent(this, new EventArgs());
            }
        }

        private void panelStopWork(object sender, EventArgs e)
        {
            if (StopWorkEvent != null)
            {
                StopWorkEvent(this, new EventArgs());
            }
        }

        private void reConnClient()
        {
            m_panelClient.Close();
            m_panelClient.Start();
            m_panelClient.StartPanel();
        }

        private void disconnect(object sender, EventArgs e)
        {
            if (DisconnectEvent != null)
            {
                DisconnectEvent(this, new EventArgs());
            }
        }

        #region События панели

        /// <summary>
        /// Тип делегата для обработки события
        /// </summary>
        public delegate void StartEventHandler(object obj, EventArgs e);

        /// <summary>
        /// Событие
        /// </summary>
        public StartEventHandler StartWorkEvent;


        /// <summary>
        /// Тип делегата для обработки события
        /// </summary>
        public delegate void StopEventHandler(object obj, EventArgs e);

        /// <summary>
        /// Событие
        /// </summary>
        public StopEventHandler StopWorkEvent;


        /// <summary>
        /// Тип делегата для обработки события
        /// </summary>
        public delegate void ExitEventHandler(object obj, EventArgs e);

        /// <summary>
        /// Событие
        /// </summary>
        public ExitEventHandler ExitEvent;

        /// <summary>
        /// Тип делегата для обработки события
        /// </summary>
        public delegate void DisconnectEventHandler(object obj, EventArgs e);

        /// <summary>
        /// Событие
        /// </summary>
        public DisconnectEventHandler DisconnectEvent;

        #endregion


        private partial class PanelCS : PanelCommonDataHost
        {

            #region Переменные и константы

            public bool b_IsPanelWork;

            /// <summary>
            /// Типы экземпляра
            /// </summary>
            public enum TypeApp { Unknown = -1, Client, Server };

            /// <summary>
            /// Тип сообщения
            /// </summary>
            enum TypeMes { Input, Output };

            private bool b_startedWork;

            /// <summary>
            /// Комманды к клиенту/серверу
            /// </summary>
            enum Command { GetDataTime, Start, Stop, ReConnect, GetName, GetStat, SetStat, Status, Exit };
            string[] arrCommand = new string[] { "GetDataTime", "Start", "Stop", "ReConnect", "GetName", "GetStat", "SetStat", "Status", "Exit" };

            bool m_b_isfirstActivate;

            System.Windows.Forms.Timer timerUpdateStatus;

            /// <summary>
            /// Экземпляр сервера
            /// </summary>
            Pipes.Server m_server;

            /// <summary>
            /// Экземпляр клиента
            /// </summary>
            Pipes.Client m_client;

            /// <summary>
            /// Количество попыток подключения
            /// </summary>
            int m_countRepCon;

            /// <summary>
            /// Таймаут подключения
            /// </summary>
            int m_timeOut;

            /// <summary>
            /// Массив имён серверов
            /// </summary>
            string[] m_servers;

            /// <summary>
            /// Тип экземпляра приложения
            /// </summary>
            TypeApp m_type_app;

            /// <summary>
            /// Делегат для добавления строки в DGV
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            delegate void delAddRow(object obj);
            /// <summary>
            /// Экземпляр делегата добавления строки в DGV
            /// </summary>
            delAddRow d_addRow;

            /// <summary>
            /// Делегат для добавления/удаления объекта в comboBox
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            delegate void delOperCB(string idClient, bool add);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в comboBox
            /// </summary>
            delOperCB d_operCB;

            /// <summary>
            /// Делегат для добавления/удаления объекта в comboBox
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            delegate void delReconn(string name_serv, bool stat = false);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в comboBox
            /// </summary>
            delReconn d_reconn;

            /// <summary>
            /// Делегат для добавления/удаления объекта в listView
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            delegate void delUpdateLV(string name_client, string status);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в listView
            /// </summary>
            delUpdateLV d_updateLV;

            /// <summary>
            /// Делегат для изменения label'a
            /// </summary>
            /// <param name="obj">bool</param>
            delegate void delLbl(bool start);
            /// <summary>
            /// Экземпляр делегата изменения label'a
            /// </summary>
            delLbl d_statLbl;

            /// <summary>
            /// Делегат для exit
            /// </summary>
            delegate void exit();
            /// <summary>
            /// Экземпляр делегата exit
            /// </summary>
            exit d_exit;

            /// <summary>
            /// Делегат для disconnect
            /// </summary>
            delegate void disconnect();
            /// <summary>
            /// Экземпляр делегата disconnect
            /// </summary>
            disconnect d_disconnect;

            /// <summary>
            /// Объект синхронизации
            /// </summary>
            private Object thisLock;

            /// <summary>
            /// Имя ПК
            /// </summary>
            string m_myName;

            /// <summary>
            /// Имя сервера(если экземпляр Client)
            /// </summary>
            string m_servName;

            Thread thread;
            #endregion

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="arServerName">Список серверов</param>
            public PanelCS(string[] arServerName, TypeApp type)
                : base(5, 20)
            {
                b_IsPanelWork = false;
                m_b_isfirstActivate = true;
                thisLock = new Object();
                m_type_app = type;
                d_exit = exit_program;
                StartedWork = false;
                d_disconnect = disconnect_client;

                InitializeComponent();

                rbCommand.CheckedChanged += new EventHandler(rbChecked);
                rbStatus.CheckedChanged += new EventHandler(rbChecked);


                m_countRepCon = 2;
                m_timeOut = 500;
                m_servers = arServerName;
            }

            public bool StartedWork
            {
                get
                {
                    return b_startedWork;
                }
                set
                {
                    b_startedWork = value;
                }

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

                // 
                // dgvMessage
                // 
                this.dgvMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvMessage.Name = "dgvMessage";
                this.dgvMessage.Size = new System.Drawing.Size(513, 150);
                this.dgvMessage.TabIndex = 0;
                this.dgvMessage.Dock = DockStyle.Fill;
                this.dgvMessage.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
                this.lblType.Text = m_type_app.ToString();
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

            }

            #endregion

            private System.Windows.Forms.DataGridView dgvMessage;
            private System.Windows.Forms.ListBox commandBox;
            private System.Windows.Forms.Button btnSendMessage;
            private System.Windows.Forms.ComboBox cbClients;
            private System.Windows.Forms.Label lblClients;
            private System.Windows.Forms.TextBox argCommand;
            private System.Windows.Forms.Label lblArg;
            private System.Windows.Forms.Label lblStat;
            private System.Windows.Forms.Label lblType;
            private System.Windows.Forms.TableLayoutPanel panelStatus;
            private System.Windows.Forms.TableLayoutPanel panelCommand;
            private System.Windows.Forms.RadioButton rbStatus;
            private System.Windows.Forms.RadioButton rbCommand;
            private System.Windows.Forms.ListView lvStatus;
            #endregion

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate(active);

                if (m_b_isfirstActivate == true)
                {
                    StartPanel();
                    m_b_isfirstActivate = false;
                }

                return bRes;
            }

            public void ReActivate()
            {
                StartPanel();
            }

            /// <summary>
            /// Запуск панели
            /// </summary>
            public void StartPanel()
            {
                //Инициализация делегатов
                d_addRow = addRowToDGV;
                d_operCB = operCBclient;
                d_reconn = reconnect;
                d_statLbl = setLbl;
                d_updateLV = lvStatusUpdate;
                //Размещение комманд в Control
                getCommandToList();
                //Инициализация экземпляра клиента/сервера
                switch (m_type_app)
                {
                    case TypeApp.Client:
                        initialize();
                        break;
                    case TypeApp.Server:
                        initialize("", true);
                        break;
                }
            }

            #region Создание клиента/сервера
            /// <summary>
            /// Инициализация объекта клиента/сервера
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения</param>
            /// <param name="is_serv">Если нужен сервер без запуска клиента то установить true</param>
            private void initialize(string name_serv = "", bool is_serv = false)
            {
                //Колонки таблицы со списком сообщений
                string[] columns = new string[] { "Дата/время", "Сообщение", "Источник", "Вход/исход" };
                cbClients.Enabled = false;
                argCommand.Enabled = false;

                m_server = null;
                m_client = null;
                //Тип экземпляра (клиент/сервер/неопределено)
                //m_type_app = TypeApp.Unknown;

                //Добавление колонок в DGV
                if (dgvMessage.Columns.Count == 0)
                    foreach (string col in columns)
                    {
                        dgvMessage.Columns.Add(col, col);
                    }

                //Проверка флага запуска сервера
                if (is_serv == false)
                    runStartClient(name_serv);

                if (is_serv == true)
                    runStartServer();
            }

            /// <summary>
            /// Запуск экземпляра клиента
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения, не передавать значение если нужно перебирать список</param>
            private void runStartClient(string name_serv = "")
            {
                if (m_type_app == TypeApp.Client)
                {
                    thread = new Thread(connectToServer);//Инициализация экземпляра потока
                    if (name_serv == "")
                        thread.Start(m_servers);//Старт потока со списком серверов из конструктора
                    else
                        thread.Start(new string[] { name_serv }); //Старт потока со списком серверов переданным в initialize
                    thread.Join();

                    if (m_client.b_IsConnected == true)
                        argCommand.Enabled = true;
                }
            }
            /// <summary>
            /// Подключение к серверу (метод отдельного потока)
            /// </summary>
            /// <param name="data"></param>
            private void connectToServer(object data)
            {
                string[] servers = (data as string[]);
                lock (thisLock)
                {
                    //Перебор серверов для подключения
                    foreach (string server in servers)
                    {
                        if (server != Environment.MachineName)
                        {
                            int i = 0;

                            m_client = new Pipes.Client(server, m_timeOut);//инициализация клиента

                            //Подписка на события клиента
                            m_client.ReadMessage += new Pipes.Client.ReadMessageEventHandler(newMessageToClient);
                            m_client.ResServ += new Pipes.Client.ResServEventHandler(resClient);
                            //несколько попыток подключения при неудаче
                            while (i < m_countRepCon)
                            {
                                //!!!! необходимо будет вставить условие на исключение собственного имени из перебора
                                m_client.StartClient();//Выполняем старт клиента и пытаемся подключиться к серверу
                                if (m_client.b_IsConnected == true)//Если соединение установлено то
                                {
                                    //m_type_app = TypeApp.Client;//Тип экземпляра устанавливаем Клиент
                                    m_myName = m_client.m_Name;//Устанавливаем собственное имя равное имени клиента
                                    m_client.WriteMessage(arrCommand[(int)Command.GetName]);//Запрашиваем имя сервера
                                    break;//Прерываем перебор серверов
                                }
                                else
                                    i++;
                            }
                            //Если не было установлено подключение то
                            if (m_client.b_IsConnected == false)
                            {
                                //отписываемся от событий
                                m_client.ReadMessage -= newMessageToClient;
                                m_client.ResServ -= resClient;
                                b_IsPanelWork = false;
                            }
                            else
                            {
                                b_IsPanelWork = true;
                            }
                            break;
                        }
                    }
                }
            }

            /// <summary>
            /// Запуск экземпляра сервера
            /// </summary>
            private void runStartServer()
            {
                lock (thisLock)
                {
                    if (m_type_app == TypeApp.Server)
                    {
                        if (thread != null)
                        {
                            if (thread.ThreadState == ThreadState.Running)
                            {
                                thread.Abort();//останавливем поток 
                            }

                            thread = null; //обнуляем его
                        }
                        thread = new Thread(initializeServer);//Инициализируем поток создания сервера
                        thread.Start();//Потока

                        //Включение компонентов формы для сервера
                        cbClients.Enabled = true;
                        argCommand.Enabled = true;
                        rbStatus.Enabled = true;
                        rbStatus.Checked = true;

                        timerUpdateStatus = new System.Windows.Forms.Timer();
                        timerUpdateStatus.Interval = 10000;
                        timerUpdateStatus.Tick += new EventHandler(timerUpdateStat_Tick);

                        timerUpdateStatus.Start();
                    }
                }
            }
            /// <summary>
            /// Запуск сервера (метод отдельного потока)
            /// </summary>
            /// <param name="data"></param>
            private void initializeServer(object data)
            {
                m_server = new Pipes.Server();//Создание экземпляра сервера
                //Подписка на события сервера
                m_server.ReadMessage += new Pipes.Server.ReadMessageEventHandler(newMessageToServer);
                m_server.ConnectClient += new Pipes.Server.ConnectClientEventHandler(addToComList);
                m_server.DisConnectClient += new Pipes.Server.DisConnectClientEventHandler(delFromComList);
                m_server.StartServer();//запуск экземпляра сервера
                //m_type_app = TypeApp.Server;//устанавливаем тип экземпляра приложения Сервер
                m_myName = m_server.Name;//Устанавливаем собственное имя равное имени сервера
            }
            #endregion

            #region Обработчики сервера
            private void addToComList(object sender, Pipes.Server.ConnectClientEventArgs e)
            {
                Invoke(d_operCB, new object[] { e.IdServer, true });
            }

            private void delFromComList(object sender, Pipes.Server.DisConnectClientEventArgs e)
            {
                Invoke(d_operCB, new object[] { e.IdServer, false });
                Invoke(d_disconnect);
            }

            private void newMessageToServer(object sender, Pipes.Server.ReadMessageEventArgs e)
            {
                addMessage(e.Value, e.IdServer, true);//Добавление сообщения и обработка
            }
            #endregion

            #region Обработчики клиента
            private void newMessageToClient(object sender, Pipes.Client.ReadMessageEventArgs e)
            {
                addMessage(e.Value, e.IdServer, true);//Добавление сообщения и обработка
                m_servName = e.IdServer;
            }

            private void resClient(object sender, Pipes.Client.ResServEventArgs e)
            {
                Invoke(d_reconn, new object[] { "", false });
            }
            #endregion

            #region Отправка сообщений
            /// <summary>
            /// Отправка сообщения из сервера
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="nameClient">Имя клиента</param>
            private void sendMessageFromServer(string message, string nameClient)
            {
                m_server.WriteMessage(nameClient, message);//Отправка
                addMessage(message, nameClient, false);//Добавление сообщения и обработка 
            }

            /// <summary>
            /// Отправка сообщения из клиента
            /// </summary>
            /// <param name="message"></param>
            private void sendMessageFromClient(string message)
            {
                m_client.WriteMessage(message);//Отправка сообщения
                addMessage(message, m_servName, false);//Добавление сообщения и обработка
            }
            #endregion

            private void rbChecked(object sender, EventArgs e)
            {
                if ((sender as RadioButton).Name == "rbStatus")
                {
                    if ((sender as RadioButton).Checked == true)
                    {
                        panelCommand.Visible = false;
                        panelStatus.Visible = true;
                    }
                }
                if ((sender as RadioButton).Name == "rbCommand")
                {
                    if ((sender as RadioButton).Checked == true)
                    {
                        panelStatus.Visible = false;
                        panelCommand.Visible = true;
                    }
                }
            }

            /// <summary>
            /// Обработчик кнопки Отправить
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnSendMessage_Click(object sender, EventArgs e)
            {
                switch (m_type_app)
                {
                    case TypeApp.Server:
                        if (commandBox.SelectedItem.ToString() == arrCommand[(int)Command.ReConnect])//Добавляет аргумент для реконнекта
                        {
                            sendMessageFromServer(commandBox.SelectedItem.ToString() + "=" + argCommand.Text, cbClients.Text);
                        }
                        else
                            sendMessageFromServer(commandBox.SelectedItem.ToString(), cbClients.Text);
                        break;

                    case TypeApp.Client:
                        sendMessageFromClient(commandBox.SelectedItem.ToString() + "=" + argCommand.Text);
                        break;
                }
                argCommand.Clear();
            }

            /// <summary>
            /// Обработка нового сообщения
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="name">Имя отправителя</param>
            /// <param name="in_mes">true если входящее</param>
            private void addMessage(string message, string name, bool in_mes)
            {
                TypeMes type_mes;
                if (in_mes == true)//лпределяем тип сообщения
                    type_mes = TypeMes.Input;//входящее
                else
                    type_mes = TypeMes.Output;//исходящее
                object rows = new object[] { DateTime.Now.ToString(), message, name, type_mes.ToString() };//новая строка сообщения
                Invoke(d_addRow, rows);//добавление новой строки сообщения в DGV


                string[] arrMessages = message.Split('=');//разбор сообщения
                string command_mes = arrMessages[0];//комманда
                string argument = string.Empty;//аргумент
                if (arrMessages.Length > 1)
                    argument = arrMessages[1];

                switch (type_mes)
                {
                    case TypeMes.Input://входящие

                        switch (m_type_app)
                        {
                            case TypeApp.Client://для клиента
                                if (command_mes == arrCommand[(int)Command.GetDataTime])//Обработка запроса даты
                                    sendMessageFromClient(DateTime.Now.ToString());
                                else
                                    if (command_mes == arrCommand[(int)Command.GetName])//Обработка запроса имени
                                        sendMessageFromClient("MyName=" + m_myName);
                                    else
                                        if (command_mes == arrCommand[(int)Command.GetStat])//Обработка запроса типа экземпляра
                                            sendMessageFromClient("Client");
                                        else
                                            if (command_mes == arrCommand[(int)Command.ReConnect])//Обработка запроса на переподключение
                                            {
                                                m_client.SendDisconnect();//отправка сообщения о разрыве соединения
                                                Invoke(d_reconn, new object[] { argument, false });
                                            }
                                            else
                                                if (command_mes == arrCommand[(int)Command.Start])//обработка запроса запуска
                                                {
                                                    Invoke(d_statLbl, true);
                                                    if (StartWorkEvent != null)
                                                        StartWorkEvent(this, new EventArgs());
                                                    else ;
                                                }
                                                else
                                                    if (command_mes == arrCommand[(int)Command.Stop])//обработка запроса остановки
                                                    {
                                                        Invoke(d_statLbl, false);
                                                        if (StopWorkEvent != null)
                                                            StopWorkEvent(this, new EventArgs());
                                                        else ;
                                                    }
                                                    else
                                                        if (command_mes == arrCommand[(int)Command.SetStat])//обработка запроса изменения типа экземпляра
                                                        {
                                                            if (argument == "Server")
                                                            {
                                                                m_client.SendDisconnect();
                                                                Invoke(d_reconn, new object[] { "", true });
                                                            }
                                                        }
                                                        else
                                                            if (command_mes == arrCommand[(int)Command.Status])//обработка запроса изменения типа экземпляра
                                                            {
                                                                sendMessageFromClient(arrCommand[(int)Command.Status] + "=" + "OK");
                                                            }
                                                            else
                                                                if (command_mes == arrCommand[(int)Command.Exit])
                                                                {
                                                                    Invoke(d_exit);
                                                                }
                                break;

                            case TypeApp.Server://входящие для сервера
                                if (command_mes == arrCommand[(int)Command.GetDataTime])//запрос даты
                                    sendMessageFromServer(DateTime.Now.ToString(), name);
                                else
                                    if (command_mes == arrCommand[(int)Command.GetName])//запрос имени
                                        sendMessageFromServer(m_myName, name);
                                    else
                                        if (command_mes == arrCommand[(int)Command.GetStat])//запрос типа текущего экземпляра
                                            sendMessageFromServer("Server", name);
                                        else
                                            if (command_mes == arrCommand[(int)Command.Start])//запрос запуска
                                            {
                                                Invoke(d_statLbl, true);
                                                if (StartWorkEvent != null)
                                                    StartWorkEvent(this, new EventArgs());
                                                else ;

                                            }
                                            else
                                                if (command_mes == arrCommand[(int)Command.Stop])//запрос остановки
                                                {
                                                    Invoke(d_statLbl, false);
                                                    if (StopWorkEvent != null)
                                                        StopWorkEvent(this, new EventArgs());
                                                    else ;
                                                }
                                                else
                                                    if (command_mes == arrCommand[(int)Command.Status])//обработка запроса изменения типа экземпляра
                                                    {
                                                        Invoke(d_updateLV, new object[] { name, argument });
                                                    }
                                                    else
                                                        if (command_mes == arrCommand[(int)Command.SetStat])//обработка запроса изменения типа экземпляра
                                                        {
                                                            if (argument == "Client")
                                                            {
                                                                m_server.SendDisconnect();
                                                                Invoke(d_reconn, new object[] { "", false });
                                                            }
                                                        }
                                                        else
                                                            if (command_mes == arrCommand[(int)Command.Exit])
                                                            {
                                                                Invoke(d_exit);
                                                            }
                                break;

                            default:
                                break;
                        }
                        break;

                    case TypeMes.Output://исходящие
                        string[] arMes = message.Split('=');
                        switch (arMes[0])
                        {
                            case "MyName":
                                switch (m_type_app)
                                {
                                    case TypeApp.Client:
                                        m_servName = arMes[1];//разбор клиентом ответного сообщения с именем сервера
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }

            private void timerUpdateStat_Tick(object sender, EventArgs e)
            {
                foreach (string client in cbClients.Items)
                {
                    sendMessageFromServer(arrCommand[(int)Command.Status], client);
                }
            }

            private void lvStatusUpdate(string client, string status)
            {
                foreach (ListViewItem item in lvStatus.Items)
                {
                    if (item.Text == client)
                    {
                        if (status == "OK")
                        {
                            item.SubItems[1].BackColor = Color.LimeGreen;
                            item.SubItems[2].Text = DateTime.Now.ToString();
                        }
                        else
                        {
                            item.SubItems[1].BackColor = Color.Red;
                            item.SubItems[1].Text = "Err";
                        }
                    }
                }
            }

            /// <summary>
            /// Добавление строки в DGV
            /// </summary>
            /// <param name="row">Объект в виде массива строк</param>
            private void addRowToDGV(object row)
            {
                dgvMessage.Rows.Add((row as object[]));//Добавление строки в DGV
            }

            /// <summary>
            /// Метод для завершения клиент/серверной части
            /// </summary>
            public void Close()
            {
                switch (m_type_app)
                {
                    case TypeApp.Client:
                        if (m_client != null)
                        {
                            if (m_client.b_Active == true)
                            {
                                m_client.SendDisconnect();//Отправка сообщения о разрыве соединения
                                m_client.StopClient();//Остановка клиента
                            }
                        }
                        break;
                    case TypeApp.Server:
                        if (m_server != null)
                        {
                            timerUpdateStatus.Stop();
                            m_server.SendDisconnect();//Отправка сообщения о разрыве соединения
                            m_server.StopServer();//Остановка сервера
                        }
                        break;
                }
            }

            /// <summary>
            /// Добавление списка комманд в ListBox
            /// </summary>
            private void getCommandToList()
            {
                foreach (string command in arrCommand)//Перебор списка комманд
                {
                    commandBox.Items.Add(command);
                }
            }

            /// <summary>
            /// Добавление/удаление слиентов из ComboBox
            /// </summary>
            /// <param name="idClient">ИД клиента</param>
            /// <param name="add">true если добавление</param>
            private void operCBclient(string idClient, bool add)
            {
                if (add == true)//если добавление то
                {
                    cbClients.Items.Add(idClient);//добавляем
                    lvStatus.Items.Add(idClient);
                    foreach (ListViewItem item in lvStatus.Items)
                    {
                        if (item.Text == idClient)
                        {
                            item.Name = idClient;
                            item.SubItems.Add("OK");
                            item.SubItems.Add(DateTime.Now.ToString());
                        }
                    }
                }
                else//иначе
                {
                    cbClients.Items.Remove(idClient);//удаляем клиента из списка и обновляем
                    cbClients.Text = string.Empty;
                    cbClients.Refresh();
                    lvStatus.Items.Find(idClient, false)[0].Remove();
                    lvStatus.Refresh();
                }
            }

            /// <summary>
            /// Метод перезапуска клиент/серверной части
            /// </summary>
            /// <param name="new_server">Пустая строка при полном перезапуске или имя сервера для подключения к нему</param>
            private void reconnect(string new_server = "", bool stat = false)
            {
                //Close();
                switch (m_type_app)
                {
                    case TypeApp.Client:
                        m_client.StopClient();//остановка клиента
                        b_IsPanelWork = false;
                        if (SetStatEvent != null)
                            SetStatEvent(this, new SetStatEventArgs(TypeApp.Server));
                        dgvMessage.Rows.Clear();
                        break;
                    case TypeApp.Server:
                        timerUpdateStatus.Stop();
                        cbClients.Items.Clear();
                        lvStatus.Items.Clear();
                        m_server.SendDisconnect();
                        //m_server.StopServer();//остановка сервера
                        b_IsPanelWork = false;
                        if (SetStatEvent != null)
                            SetStatEvent(this, new SetStatEventArgs(TypeApp.Client));
                        timerUpdateStatus.Start();
                        dgvMessage.Rows.Clear();
                        break;
                }

                if (new_server != "")
                    initialize(new_server, stat);//подключение к новому серверу или запуск сервера
            }

            /// <summary>
            /// Метод для изменения label'a
            /// </summary>
            /// <param name="start">Если запуск то true</param>
            private void setLbl(bool start)
            {
                b_startedWork = start;

                if (start == true)
                {
                    lblStat.BackColor = System.Drawing.Color.GreenYellow;
                    lblStat.Text = "Started";
                }
                else
                {
                    lblStat.BackColor = System.Drawing.Color.Red;
                    lblStat.Text = "Paused";
                }
            }

            private void exit_program()
            {
                if (ExitEvent != null)
                    ExitEvent(this, new EventArgs());
            }

            private void disconnect_client()
            {
                if (DisconnectEvent != null)
                    DisconnectEvent(this, new EventArgs());
            }

            #region События панели
            /// <summary>
            /// Тип делегата для обработки события
            /// </summary>
            public delegate void StartEventHandler(object obj, EventArgs e);

            /// <summary>
            /// Событие
            /// </summary>
            public StartEventHandler StartWorkEvent;


            /// <summary>
            /// Тип делегата для обработки события
            /// </summary>
            public delegate void StopEventHandler(object obj, EventArgs e);

            /// <summary>
            /// Событие
            /// </summary>
            public StopEventHandler StopWorkEvent;


            public class SetStatEventArgs
            {
                public TypeApp TypeApp;
                public SetStatEventArgs(TypeApp type)
                {
                    this.TypeApp = type;
                }
            }

            /// <summary>
            /// Тип делегата для обработки события
            /// </summary>
            public delegate void SetStatEventHandler(object obj, SetStatEventArgs e);

            /// <summary>
            /// Событие
            /// </summary>
            public SetStatEventHandler SetStatEvent;

            /// <summary>
            /// Тип делегата для обработки события
            /// </summary>
            public delegate void ExitEventHandler(object obj, EventArgs e);

            /// <summary>
            /// Событие
            /// </summary>
            public ExitEventHandler ExitEvent;

            /// <summary>
            /// Тип делегата для обработки события
            /// </summary>
            public delegate void DisconnectEventHandler(object obj, EventArgs e);

            /// <summary>
            /// Событие
            /// </summary>
            public DisconnectEventHandler DisconnectEvent;
            #endregion

        }
    }
}
