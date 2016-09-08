﻿using System;
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
    public partial class PanelClientServer : HPanelCommonDataHost
    {
        /// <summary>
        /// Типы экземпляра дочерней панели
        /// </summary>
        protected enum TypePanel { Unknown = -1, Client, Server
            , Count }
        /// <summary>
        /// Массив дочерних панелей
        /// </summary>
        private PanelCS []m_arPanels;

        public enum TypePanelWorkState { Unknown = -1, Paused, Started
            , Count }
        /// <summary>
        /// Признак состояния внешней панели (Работа)
        /// </summary>
        private TypePanelWorkState m_PanelWorkState;
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

            EventPanelWorkStateChanged += new DelegateFunc (onEventPanelWorkStateChanged);
            //eventTypePanelEnableInitialize += new DelegateFunc (onEventTypePanelEnableInitialize);

            m_PanelWorkState = TypePanelWorkState.Unknown;
            _typePanelEnabled = TypePanel.Unknown;
        }

        private int Ready {
            get { return (m_arPanels[(int)TypePanel.Client].Ready == true) && (m_arPanels[(int)TypePanel.Server].Ready == true) ? 0 : -1; }
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
            // 2-ой(1) - тип приложения (TypeApp)
            // 3-ий(2) - тип сообщения (TypeMes)
            // 4-ый(3) - идентификатор события (ID_EVENT)
            object[] pars = (ev.par[0] as object[])[0] as object [];
            //Определить внутреннее сообщение или для передачи в родительскую форму
            // по кол-ву параметров (короткие сообщения - внутренние)
            bool bRedirect = pars.Length > 2;

            if (bRedirect == true)
                DataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.INTERACTION_EVENT, (Pipes.Pipe.ID_EVENT)pars[3] } });
            else
                // внутреннее сообщение
                if (((TypePanel)pars[1]) == TypePanel.Client) //e.TypeApp == PanelCS.TypePanel.Client
                    BeginInvoke(new DelegateFunc (reConnClient));
                else //e.TypeApp == PanelCS.TypePanel.Server
                    ; // ничего не делаем
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
                        m_semInteractionParameters.Release (1);
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
            m_arPanels[(int)TypePanel.Client] = new PanelCS(pars.m_arNameServers, TypePanel.Client);
            m_arPanels[(int)TypePanel.Client].EvtDataAskedHost += new DelegateObjectFunc(panelOnCommandEvent);
            m_arPanels[(int)TypePanel.Client].Start();
            //m_panelClient.Dock = DockStyle.Fill; уже Fill
            m_arPanels[(int)TypePanel.Server] = new PanelCS(pars.m_arNameServers, TypePanel.Server);
            m_arPanels[(int)TypePanel.Server].EvtDataAskedHost += new DelegateObjectFunc(panelOnCommandEvent);
            m_arPanels[(int)TypePanel.Server].Start();
            //m_panelServer.Dock = DockStyle.Fill; уже Fill

            //EventStartedWorkChanged += new DelegateFunc(onEventStartedWorkChanged);

            this.Controls.Add(m_arPanels[(int)TypePanel.Server], 0, 0);
            this.Controls.Add(m_arPanels[(int)TypePanel.Client], 0, 1);
        }

        public event DelegateFunc EventPanelWorkStateChanged;

        public TypePanelWorkState PanelWorkState
        {
            get
            {
                return m_PanelWorkState;
            }
            set
            {
                if (!(m_PanelWorkState == value))
                {
                    m_PanelWorkState = value;
                    EventPanelWorkStateChanged();
                }
                else
                    ;
            }
        }

        private void onEventPanelWorkStateChanged()
        {
            m_arPanels[(int)_typePanelEnabled].UpdatePanelWorkState();
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

            start();
            //BeginInvoke(new DelegateObjectFunc (start), par);
            //eventRecievedInteractionParameters(par);

            if (m_arPanels[(int)TypePanel.Client].ClientIsConnected == true) {
                _typePanelEnabled = TypePanel.Client;
            } else {
                _typePanelEnabled = TypePanel.Server;
            }

            m_arPanels[(int)_typePanelEnabled].Enabled = true;

            DataAskedHost(new object[] {
                new object[] {
                    HHandlerQueue.StatesMachine.INTERACTION_EVENT, Pipes.Pipe.ID_EVENT.Start
                    , Ready // признак успеха опаерации инициализации-старта
            } });
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true)
            {
                m_arPanels[(int)TypePanel.Server].Activate(active);
                m_arPanels[(int)TypePanel.Client].Activate(active);
            }
            else
                ;

            return bRes;
        }

        /// <summary>
        /// Метод для завершения клиент/серверной части
        /// </summary>
        public override void Stop()
        {
            m_arPanels[(int)TypePanel.Client].Stop();
            m_arPanels[(int)TypePanel.Server].Stop();

            base.Stop();
        }

        private void reConnClient()
        {
            m_arPanels[(int)TypePanel.Client].Activate(false); m_arPanels[(int)TypePanel.Client].Stop();
            m_arPanels[(int)TypePanel.Client].Start();
            m_arPanels[(int)TypePanel.Client].Activate(true);
        }

        private partial class PanelCS : PanelCommonDataHost
        {
            #region Переменные и константы

            private const string KEY_MYNAME = @"MyName";
            private static int MS_TIMER_UPDATE_STATUS = 10000;

            private enum INDEX_COLUMN_MESSAGES : short { UNKNOWN = -1
                , DATETIME, MESSAGE, SOURCE, TYPE_IO
                , COUNT
            }
            //Колонки таблицы со списком сообщений
            private string[] m_arTextColumnMessages = new string[(int)INDEX_COLUMN_MESSAGES.COUNT] { "Дата/время", "Сообщение", "Источник", "Вход/исход" };

            /// <summary>
            /// Количество попыток подключения
            /// </summary>
            private static int MAX_ATTEMPT_CONNECT = 2;
            /// <summary>
            /// Таймаут подключения
            /// </summary>
            private static int MS_TIMEOUT_CONNECT_TO_SERVER = 500;
            ///// <summary>
            ///// Признак состояния внешней панели ("Работа")
            ///// </summary>
            //private bool m_bStartedWork;
            ///// <summary>
            ///// Признак активности текущего объекта(панели)
            ///// </summary>
            //public bool m_bIsPanelWork;

            //private bool m_bIsFirstActivate;

            public bool Ready {
                get {
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
            enum TypeMes { Input, Output };

            /// <summary>
            /// Комманды к клиенту/серверу
            /// </summary>
            private enum COMMAND { Unknown = -1
                , GetDataTime, Start, Stop, ReConnect, GetName, GetStat, SetStat, Status, Exit
                , Count
            };
            //string[] arrCommand = new string[] { "GetDataTime", "Start", "Stop", "ReConnect", "GetName", "GetStat", "SetStat", "Status", "Exit" };            

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
            /// Массив имён серверов
            /// </summary>
            string[] m_servers;

            /// <summary>
            /// Тип экземпляра приложения
            /// </summary>
            private TypePanel m_type_panel;

            /// <summary>
            /// Экземпляр делегата добавления строки в DGV
            /// </summary>
            private DelegateObjectFunc d_addRow;

            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в comboBox
            /// </summary>
            private DelegateStrBoolFunc d_operCB;

            /// <summary>
            /// Делегат для добавления/удаления объекта в comboBox
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            delegate void DelegateStrBoolFunc(string name_serv, bool stat = false);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в comboBox
            /// </summary>
            private DelegateStringFunc d_reconn;

            /// <summary>
            /// Делегат для добавления/удаления объекта в listView
            /// </summary>
            /// <param name="obj">Строка в виде массива</param>
            delegate void DelegateStrStrFunc(string name_client, string status);
            /// <summary>
            /// Экземпляр делегата добавления/удаления объекта в listView
            /// </summary>
            private DelegateStrStrFunc d_updateLV;

            /// <summary>
            /// Экземпляр делегата изменения label'a
            /// </summary>
            private DelegateFunc d_statLbl;

            /// <summary>
            /// Экземпляр делегата exit
            /// </summary>
            private DelegateFunc d_exit;

            /// <summary>
            /// Экземпляр делегата disconnect
            /// </summary>
            private DelegateFunc d_disconnect;

            /// <summary>
            /// Объект синхронизации
            /// </summary>
            private Object thisLock;

            /// <summary>
            /// Имя ПК
            /// </summary>
            private string m_myName;

            /// <summary>
            /// Имя сервера(если экземпляр Client)
            /// </summary>
            private string m_servName;

            private Thread thread;
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
                d_disconnect = disconnect_client;

                try {
                    InitializeComponent();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelCS::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                rbCommand.CheckedChanged += new EventHandler(rbChecked);
                rbStatus.CheckedChanged += new EventHandler(rbChecked);

                m_servers = arServerName;
            }

            /// <summary>
            /// Признак состояния внешней панели (Работа)
            /// </summary>
            public TypePanelWorkState PanelWorkState
            {
                get
                {
                    return (Parent as PanelClientServer).PanelWorkState;
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

                //if (m_bIsFirstActivate == true)
                if ((bRes == true)
                    && (IsFirstActivated == true))
                {
                }
                else
                    ;

                return bRes;
            }

            //public void ReActivate()
            //{
            //    StartPanel();
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
                initialize(string.Empty
                    , m_type_panel);
            }

            public bool ClientIsConnected { get { return (!(m_client == null)) && (m_client.m_bIsConnected == true); } }

            #region Создание клиента/сервера
            /// <summary>
            /// Инициализация объекта клиента/сервера
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения</param>
            /// <param name="is_serv">Если нужен сервер без запуска клиента то установить true</param>
            private void initialize(string name_serv, TypePanel runPipe)
            {
                cbClients.Enabled = false;
                argCommand.Enabled = false;

                m_server = null;
                m_client = null;
                //Тип экземпляра (клиент/сервер/неопределено)
                //m_type_app = TypePanel.Unknown;

                //Добавление колонок в DGV
                if (dgvMessage.Columns.Count == 0)
                    foreach (string col in m_arTextColumnMessages)
                        dgvMessage.Columns.Add(col, col);
                else
                    ;

                //Проверка флага запуска сервера
                switch (runPipe)
                {
                    case TypePanel.Client:
                        runStartClient(name_serv);
                        break;
                    case TypePanel.Server:
                        runStartServer();
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Запуск экземпляра клиента
            /// </summary>
            /// <param name="name_serv">Имя сервера для подключения, не передавать значение если нужно перебирать список</param>
            private void runStartClient(string name_serv = @"")
            {
                if (m_type_panel == TypePanel.Client)
                {
                    thread = new Thread(connectToServer);//Инициализация экземпляра потока
                    if (name_serv.Equals(string.Empty) == true)
                        thread.Start(m_servers);//Старт потока со списком серверов из конструктора
                    else
                        thread.Start(new string[] { name_serv }); //Старт потока со списком серверов переданным в initialize
                    thread.Join();

                    if (m_client.m_bIsConnected == true)
                        argCommand.Enabled = true;
                }
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
                        if (server.Equals(Environment.MachineName) == false)
                        {
                            iAttempt = 0;

                            m_client = new Pipes.Client(server, MS_TIMEOUT_CONNECT_TO_SERVER);//инициализация клиента

                            //Подписка на события клиента
                            m_client.ReadMessage += new EventHandler(newMessageToClient);
                            m_client.ResServ += new EventHandler(resClient);
                            //несколько попыток подключения при неудаче
                            while (iAttempt < MAX_ATTEMPT_CONNECT)
                            {
                                //!!!! необходимо будет вставить условие на исключение собственного имени из перебора
                                m_client.StartClient();//Выполняем старт клиента и пытаемся подключиться к серверу
                                if (m_client.m_bIsConnected == true)//Если соединение установлено то
                                {
                                    //m_type_app = TypePanel.Client;//Тип экземпляра устанавливаем Клиент
                                    m_myName = m_client.m_Name;//Устанавливаем собственное имя равное имени клиента
                                    m_client.WriteMessage(COMMAND.GetName.ToString());//Запрашиваем имя сервера
                                    //Прерываем попытки подключения
                                    break;
                                }
                                else
                                    iAttempt++;
                            }
                            //Проверить было ли установлено соединение
                            if (m_client.m_bIsConnected == false)
                            {// подключениене не было установлено
                                //отписываемся от событий
                                m_client.ReadMessage -= newMessageToClient;
                                m_client.ResServ -= resClient;
                            } else {
                            // прерываем перебор серверов
                                break;
                            }
                            
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
                    if (m_type_panel == TypePanel.Server)
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
            }
            /// <summary>
            /// Запуск сервера (метод отдельного потока)
            /// </summary>
            /// <param name="data"></param>
            private void initializeServer(object data)
            {
                m_server = new Pipes.Server();//Создание экземпляра сервера
                //Подписка на события сервера
                m_server.ReadMessage += new EventHandler(newMessageToServer);
                m_server.ConnectClient += new EventHandler(addToComList);
                m_server.DisConnectClient += new EventHandler(delFromComList);
                m_server.StartServer();//запуск экземпляра сервера
                //m_type_app = TypePanel.Server;//устанавливаем тип экземпляра приложения Сервер
                m_myName = m_server.m_Name;//Устанавливаем собственное имя равное имени сервера
            }
            #endregion

            #region Обработчики сервера
            private void addToComList(object sender, EventArgs e)
            {
                Invoke(d_operCB, new object[] { (e as Pipes.Server.ConnectionClientEventArgs).IdServer, true });
            }

            private void delFromComList(object sender, EventArgs e)
            {
                Invoke(d_operCB, new object[] { (e as Pipes.Server.ConnectionClientEventArgs).IdServer, false });
                Invoke(d_disconnect);
            }

            private void newMessageToServer(object sender, EventArgs e)
            {
                addMessage((e as Pipes.Pipe.ReadMessageEventArgs).Value
                    , (e as Pipes.Pipe.ReadMessageEventArgs).IdServer
                    , TypeMes.Input);//Добавление сообщения и обработка
            }
            #endregion

            #region Обработчики клиента
            private void newMessageToClient(object sender, EventArgs e)
            {
                addMessage((e as Pipes.Client.ReadMessageEventArgs).Value, (e as Pipes.Client.ReadMessageEventArgs).IdServer, TypeMes.Input);//Добавление сообщения и обработка
                m_servName = (e as Pipes.Client.ReadMessageEventArgs).IdServer;
            }

            private void resClient(object sender, EventArgs e)
            {//Pipes.Client.ResServeventArgs
                Invoke(d_reconn, string.Empty);
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
                addMessage(message, nameClient, TypeMes.Output);//Добавление сообщения и обработка 
            }

            /// <summary>
            /// Отправка сообщения из клиента
            /// </summary>
            /// <param name="message">Сообщение для отправки</param>
            private void sendMessageFromClient(string message)
            {
                m_client.WriteMessage(message);//Отправка сообщения
                addMessage(message, m_servName, TypeMes.Output);//Добавление сообщения и обработка
            }
            #endregion

            private void rbChecked(object sender, EventArgs ev)
            {
                try {
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
                } catch (Exception e) {
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
                        if (commandBox.SelectedItem.ToString() == COMMAND.ReConnect.ToString ())//Добавляет аргумент для реконнекта
                        {
                            sendMessageFromServer(commandBox.SelectedItem.ToString() + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + argCommand.Text, cbClients.Text);
                        }
                        else
                            sendMessageFromServer(commandBox.SelectedItem.ToString(), cbClients.Text);
                        break;

                    case TypePanel.Client:
                        sendMessageFromClient(commandBox.SelectedItem.ToString() + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + argCommand.Text);
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
            private void addMessage(string message, string name, TypeMes type_mes)
            {
                object rows = null; // новая строка сообщения

                string[] arrMessages = message.Split(Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR); // разбор сообщения
                string command_mes = arrMessages[0]; // комманда
                string argument = string.Empty; // аргумент
                if (arrMessages.Length > 1)
                    argument = arrMessages[1];

                try {
                    rows = new object[] { DateTime.Now.ToString(), message, name, type_mes.ToString() }; // новая строка сообщения
                    Invoke(d_addRow, rows); // добавление новой строки сообщения в DGV

                    switch (type_mes)
                    {
                        case TypeMes.Input://входящие
                            switch (m_type_panel)
                            {
                                case TypePanel.Client://для клиента
                                    if (command_mes.Equals(COMMAND.GetDataTime.ToString()) == true)//Обработка запроса даты
                                        sendMessageFromClient(DateTime.Now.ToString());
                                    else
                                        if (command_mes.Equals(COMMAND.GetName.ToString()) == true)//Обработка запроса имени
                                            sendMessageFromClient(KEY_MYNAME + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + m_myName);
                                        else
                                            if (command_mes.Equals(COMMAND.GetStat.ToString()) == true)//Обработка запроса типа экземпляра
                                                sendMessageFromClient(TypePanel.Client.ToString());
                                            else
                                                if (command_mes.Equals(COMMAND.ReConnect.ToString()) == true)//Обработка запроса на переподключение
                                                {
                                                    m_client.SendDisconnect();//отправка сообщения о разрыве соединения
                                                    Invoke(d_reconn, new object[] { argument, false });
                                                }
                                                else
                                                    if (command_mes.Equals(COMMAND.Start.ToString()) == true)//обработка запроса запуска
                                                    {
                                                        Invoke(d_statLbl, true);
                                                        DataAskedHost(new object[] { new object[] { this, m_type_panel, type_mes, Pipes.Pipe.ID_EVENT.Start } });
                                                    }
                                                    else
                                                        if (command_mes.Equals(COMMAND.Stop.ToString()) == true)//обработка запроса остановки
                                                        {
                                                            Invoke(d_statLbl, false);
                                                            DataAskedHost(new object[] { new object[] { this, m_type_panel, type_mes, Pipes.Pipe.ID_EVENT.Stop } });
                                                        }
                                                        else
                                                            if (command_mes.Equals(COMMAND.SetStat.ToString()) == true)//обработка запроса изменения типа экземпляра
                                                            {
                                                                if (argument.Equals(TypePanel.Server.ToString()) == true)
                                                                {
                                                                    m_client.SendDisconnect();
                                                                    Invoke(d_reconn, new object[] { string.Empty, true });
                                                                }
                                                            }
                                                            else
                                                                if (command_mes.Equals(COMMAND.Status.ToString()) == true)//обработка запроса изменения типа экземпляра
                                                                {
                                                                    sendMessageFromClient(command_mes + Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR + Pipes.Pipe.MESSAGE_RECIEVED_OK);
                                                                }
                                                                else
                                                                    if (command_mes.Equals(COMMAND.Exit.ToString()) == true)
                                                                    {
                                                                        Invoke(d_exit);
                                                                    }
                                    break;

                                case TypePanel.Server://входящие для сервера
                                    if (command_mes.Equals(COMMAND.GetDataTime.ToString()) == true)//запрос даты
                                        sendMessageFromServer(DateTime.Now.ToString(), name);
                                    else
                                        if (command_mes.Equals(COMMAND.GetName.ToString()) == true)//запрос имени
                                            sendMessageFromServer(m_myName, name);
                                        else
                                            if (command_mes.Equals(COMMAND.GetStat.ToString()) == true)//запрос типа текущего экземпляра
                                                sendMessageFromServer(TypePanel.Server.ToString(), name);
                                            else
                                                if (command_mes.Equals(COMMAND.Start.ToString()) == true)//запрос запуска
                                                {
                                                    Invoke(d_statLbl, true);
                                                    DataAskedHost(new object[] { new object[] { this, m_type_panel, type_mes, Pipes.Pipe.ID_EVENT.Start } });
                                                }
                                                else
                                                    if (command_mes.Equals(COMMAND.Stop.ToString()) == true)//запрос остановки
                                                    {
                                                        Invoke(d_statLbl, false);
                                                        DataAskedHost(new object[] { new object[] { this, m_type_panel, type_mes, Pipes.Pipe.ID_EVENT.Stop } });
                                                    }
                                                    else
                                                        if (command_mes.Equals(COMMAND.Status.ToString()) == true)//обработка запроса изменения типа экземпляра
                                                        {
                                                            Invoke(d_updateLV, new object[] { name, argument });
                                                        }
                                                        else
                                                            if (command_mes.Equals(COMMAND.SetStat.ToString()) == true)//обработка запроса изменения типа экземпляра
                                                            {
                                                                if (argument.Equals(TypePanel.Client.ToString()) == true)
                                                                {
                                                                    m_server.SendDisconnect();
                                                                    Invoke(d_reconn, new object[] { "", false });
                                                                }
                                                            }
                                                            else
                                                                if (command_mes.Equals(COMMAND.Exit.ToString()) == true)
                                                                    Invoke(d_exit);
                                                                else
                                                                    ; // неизвестная команда
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case TypeMes.Output://исходящие
                            string[] arMes = message.Split(Pipes.Pipe.DELIMETER_MESSAGE_KEYVALUEPAIR);
                            switch (arMes[0])
                            {
                                case KEY_MYNAME:
                                    switch (m_type_panel)
                                    {
                                        case TypePanel.Client:
                                            m_servName = arMes[1];//разбор клиентом ответного сообщения с именем сервера
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                } catch (Exception e) {
                    Logging.Logg().Exception(e
                        , string.Format(@"PanelClientServer.PanelCS::addMaessage (mes={0}, name={1}, typeMes={2}) - ...", message, name, type_mes)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод обработки события таймера
            ///  отправка сообщений клиентам (опрос статуса)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - таймер</param>
            /// <param name="e">Аргумент события</param>
            private void timerUpdateStat_Tick(object sender, EventArgs ev)
            {
                try {
                    foreach (string client in cbClients.Items)
                        sendMessageFromServer(COMMAND.Status.ToString (), client);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::timerUpdateStat_Tick () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод обновления
            /// </summary>
            /// <param name="client">Наименование клиента</param>
            /// <param name="status">Текст ответа</param>
            private void lvStatusUpdate(string client, string status)
            {
                Color clr = Color.Empty;
                string text = string.Empty;
                int col = -1;

                try {
                    foreach (ListViewItem item in lvStatus.Items)
                        if (item.Text.Equals(client) == true)
                        {
                            if (status.Equals(Pipes.Pipe.MESSAGE_RECIEVED_OK) == true)
                            {
                                clr = Color.LimeGreen;
                                text = DateTime.Now.ToString();
                                col = 2;
                            }
                            else
                            {
                                clr = Color.Red;
                                text = "Err";
                                col = 1;
                            }

                            item.SubItems[1].BackColor = clr;
                            item.SubItems[col].Text = text;
                        }
                        else
                            ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format (@"PanelClientServer.PanelCS::lvStatusUpdate (client={0}, status={1}) - ...", client, status), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавление строки в DGV
            /// </summary>
            /// <param name="row">Объект в виде массива строк</param>
            private void addRowToDGV(object row)
            {
                try {
                    dgvMessage.Rows.Add((row as object[]));//Добавление строки в DGV
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"PanelClientServer.PanelCS::addRowToDGV () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод для завершения клиент/серверной части
            /// </summary>
            public override void Stop()
            {
                switch (m_type_panel)
                {
                    case TypePanel.Client:
                        if (m_client != null)
                        {
                            if (m_client.b_Active == true)
                            {
                                m_client.SendDisconnect();//Отправка сообщения о разрыве соединения
                                m_client.StopClient();//Остановка клиента
                            }
                        }
                        break;
                    case TypePanel.Server:
                        if (m_server != null)
                        {
                            timerUpdateStatus.Stop();
                            m_server.SendDisconnect();//Отправка сообщения о разрыве соединения
                            m_server.StopServer();//Остановка сервера
                        }
                        break;
                }

                base.Stop();
            }

            /// <summary>
            /// Добавление списка комманд в ListBox
            /// </summary>
            private void getCommandToList()
            {
                for (COMMAND command = COMMAND.Unknown; command < (COMMAND.Count - 1); command ++)//Перебор списка комманд
                {
                    commandBox.Items.Add((command + 1).ToString());
                }
            }

            /// <summary>
            /// Добавление/удаление слиентов из ComboBox
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
                                item.SubItems.Add(Pipes.Pipe.MESSAGE_RECIEVED_OK);
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
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::operCBclient", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Метод перезапуска клиент/серверной части
            /// </summary>
            /// <param name="new_server">Пустая строка при полном перезапуске или имя сервера для подключения к нему</param>
            private void reconnect(string new_server = "")
            {
                switch (m_type_panel)
                {
                    case TypePanel.Client:
                        m_client.StopClient();//остановка клиента                        
                        break;
                    case TypePanel.Server:
                        timerUpdateStatus.Stop();
                        cbClients.Items.Clear();
                        lvStatus.Items.Clear();
                        m_server.SendDisconnect();
                        //m_server.StopServer();//остановка сервера
                        timerUpdateStatus.Start();                        
                        break;
                    default:
                        break;
                }

                dgvMessage.Rows.Clear();

                DataAskedHost(new object[] { new object[] { this, m_type_panel } });

                if (new_server.Equals(string.Empty) == false)
                    initialize(new_server, m_type_panel);//подключение к новому серверу или запуск сервера
            }

            /// <summary>
            /// Метод для изменения label'a состояния внешней панели (Работа)
            /// </summary>
            private void setLbl()
            {
                Color clr = Enabled == true ? Color.DarkGray :
                    (PanelWorkState == TypePanelWorkState.Started) ? Color.GreenYellow :
                        (PanelWorkState == TypePanelWorkState.Paused) ? Color.Red :
                            Color.LightGray;

                try {
                    lblStat.BackColor = clr;
                    lblStat.Text = PanelWorkState.ToString();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelClientServer.PanelCS::setLbl () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            private void exit_program()
            {
                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, Pipes.Pipe.ID_EVENT.Exit } });
            }

            private void disconnect_client()
            {
                DataAskedHost(new object[] { new object[] { this, m_type_panel, TypeMes.Input, Pipes.Pipe.ID_EVENT.Disconnect } });
            }
        }

        public struct InteractionParameters
        {
            public string[] m_arNameServers;

            public string m_NameMainPipe;

            public InteractionParameters(string strServerNames, string strNameMainPipe)
            {
                m_arNameServers = strServerNames.Split(',');

                m_NameMainPipe = strNameMainPipe;
            }

            public bool Ready { get { return ((!(m_arNameServers == null)) && (m_arNameServers.Length > 1)) && m_NameMainPipe.Equals(string.Empty) == false; } }
        }
    }
}
