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
    public partial class PanelCS : PanelCommonDataHost
    {
        #region Переменные и константы
        /// <summary>
        /// Типы экземпляра
        /// </summary>
        enum TypeApp { Unknown=-1, Client, Server };

        /// <summary>
        /// Тип сообщения
        /// </summary>
        enum TypeMes { Input, Output };

        /// <summary>
        /// Комманды к клиенту/серверу
        /// </summary>
        enum Command { GetDataTime, Start, Stop, ReConnect, GetName, GetStat, SetStat };
        string[] arrCommand = new string[] {"GetDataTime", "Start", "Stop", "ReConnect", "GetName", "GetStat", "SetStat" };

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
        delegate void delReconn(string name_serv, bool stat=false);
        /// <summary>
        /// Экземпляр делегата добавления/удаления объекта в comboBox
        /// </summary>
        delReconn d_reconn;

        /// <summary>
        /// Делегат для изменения label'a
        /// </summary>
        /// <param name="obj">Строка в виде массива</param>
        delegate void delLbl(bool start);
        /// <summary>
        /// Экземпляр делегата изменения label'a
        /// </summary>
        delLbl d_statLbl;

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
        public PanelCS(string[] arServerName)
            : base(3, 40)
        {
            thisLock = new Object();
            InitializeComponent();

            m_type_app = TypeApp.Unknown;
            m_countRepCon = 2;
            m_timeOut = 500;
            m_servers = arServerName; 
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
            this.dgvMessage = new System.Windows.Forms.DataGridView();
            this.commandBox = new System.Windows.Forms.ListBox();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.cbClients = new System.Windows.Forms.ComboBox();
            this.lblClients = new System.Windows.Forms.Label();
            this.argCommand = new System.Windows.Forms.TextBox();
            this.lblArg = new System.Windows.Forms.Label();
            this.lblStat = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dgvMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMessage.Location = new System.Drawing.Point(3, 288);
            this.dgvMessage.Name = "dataGridView1";
            this.dgvMessage.Size = new System.Drawing.Size(513, 150);
            this.dgvMessage.TabIndex = 0;
            this.dgvMessage.Dock = DockStyle.Fill;
            this.dgvMessage.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // 
            // listBox1
            // 
            this.commandBox.FormattingEnabled = true;
            this.commandBox.Location = new System.Drawing.Point(4, 4);
            this.commandBox.Name = "listBox1";
            this.commandBox.Size = new System.Drawing.Size(221, 277);
            this.commandBox.TabIndex = 1;
            this.commandBox.Dock = DockStyle.Fill;
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Location = new System.Drawing.Point(231, 97);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(75, 23);
            this.btnSendMessage.TabIndex = 2;
            this.btnSendMessage.Text = "Отправить";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            //this.btnSendMessage.Dock = DockStyle.Fill;
            // 
            // comboBox1
            // 
            this.cbClients.FormattingEnabled = true;
            this.cbClients.Location = new System.Drawing.Point(231, 28);
            this.cbClients.Name = "comboBox1";
            this.cbClients.Size = new System.Drawing.Size(131, 21);
            this.cbClients.TabIndex = 3;
            this.cbClients.Dock = DockStyle.Fill;
            // 
            // label1
            // 
            this.lblClients.AutoSize = true;
            this.lblClients.Location = new System.Drawing.Point(232, 9);
            this.lblClients.Name = "label1";
            this.lblClients.Size = new System.Drawing.Size(92, 13);
            this.lblClients.TabIndex = 4;
            this.lblClients.Text = "Клиенты/сервер";
            //this.lblClients.Dock = DockStyle.Fill;
            // 
            // textBox1
            // 
            this.argCommand.Location = new System.Drawing.Point(231, 71);
            this.argCommand.Name = "textBox1";
            this.argCommand.Size = new System.Drawing.Size(131, 20);
            this.argCommand.TabIndex = 5;
            this.argCommand.Dock = DockStyle.Fill;
            // 
            // label2
            // 
            this.lblArg.AutoSize = true;
            this.lblArg.Location = new System.Drawing.Point(232, 52);
            this.lblArg.Name = "label2";
            this.lblArg.Size = new System.Drawing.Size(55, 13);
            this.lblArg.TabIndex = 6;
            this.lblArg.Text = "Аргумент";
            //this.lblArg.Dock = DockStyle.Fill;
            // 
            // label3
            // 
            this.lblStat.AutoSize = true;
            this.lblStat.BackColor = System.Drawing.Color.Red;
            this.lblStat.Location = new System.Drawing.Point(232, 143);
            this.lblStat.Name = "label3";
            this.lblStat.Size = new System.Drawing.Size(43, 13);
            this.lblStat.TabIndex = 7;
            this.lblStat.Text = "Paused";
            //this.lblStat.Dock = DockStyle.Fill;
            // 
            // PanelCS
            // 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblStat,0,0);
            this.SetRowSpan(lblStat, 2);

            this.Controls.Add(this.lblClients,0,2);
            this.SetRowSpan(lblClients, 2);
            this.Controls.Add(this.cbClients,0,4);
            this.SetRowSpan(cbClients, 2);

            this.Controls.Add(this.lblArg, 0, 6);
            this.SetRowSpan(lblArg, 2);
            this.Controls.Add(this.argCommand, 0, 8);
            this.SetRowSpan(argCommand, 2);

            this.Controls.Add(this.commandBox, 0, 10);
            this.SetRowSpan(commandBox, 28);

            this.Controls.Add(this.btnSendMessage,0,38);
            this.SetRowSpan(btnSendMessage, 2);

            this.Controls.Add(this.dgvMessage,1,0);
            this.SetRowSpan(dgvMessage, 40);
            this.SetColumnSpan(dgvMessage, 2);

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
        #endregion

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly();
        }
        
        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (IsFirstActivated == true)
            {
                StartPanel();
            }

            return bRes;
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
            //Размещение комманд в Control
            getCommandToList();
            //Инициализация экземпляра клиента/сервера
            initialize();
        }
         
        #region Создание клиента/сервера
        /// <summary>
        /// Инициализация объекта клиента/сервера
        /// </summary>
        /// <param name="name_serv">Имя сервера для подключения</param>
        /// <param name="is_serv">Если нужен сервер без запуска клиента то установить true</param>
        private void initialize(string name_serv = "", bool is_serv=false)
        {
            //Колонки таблицы со списком сообщений
            string[] columns = new string[]{"Дата/время","Сообщение","Источник","Вход/исход" };
            cbClients.Enabled = false;
            argCommand.Enabled = false;

            m_server = null;
            m_client = null;
            //Тип экземпляра (клиент/сервер/неопределено)
            m_type_app = TypeApp.Unknown;

            //Добавление колонок в DGV
            if(dgvMessage.Columns.Count==0)
                foreach (string col in columns)
                {
                    dgvMessage.Columns.Add(col, col);
                }

            //Проверка флага запуска сервера
            if(is_serv==false)
                runStartClient(name_serv);

            runStartServer();
        }

        /// <summary>
        /// Запуск экземпляра клиента
        /// </summary>
        /// <param name="name_serv">Имя сервера для подключения, не передавать значение если нужно перебирать список</param>
        private void runStartClient(string name_serv = "")
        {
            thread = new Thread(connectToServer);//Инициализация экземпляра потока
            if (name_serv == "")
                thread.Start(m_servers);//Старт потока со списком серверов из конструктора
            else
                thread.Start(new string[] { name_serv }); //Старт потока со списком серверов переданным в initialize
            thread.Join();
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
                            m_type_app = TypeApp.Client;//Тип экземпляра устанавливаем Клиент
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
                    }
                    else
                        break;
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
                if (m_type_app == TypeApp.Unknown)
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
            m_type_app = TypeApp.Server;//устанавливаем тип экземпляра приложения Сервер
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
            m_servName=e.IdServer;
        }

        private void resClient(object sender, Pipes.Client.ResServEventArgs e)
        {
            Invoke(d_reconn,new object[]{"",false});
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
                    sendMessageFromClient(commandBox.SelectedItem.ToString());
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
            switch(type_mes)
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
                                                if (StartWork != null)
                                                    StartWork(this, new EventArgs());
                                                else ;
                                            }
                                            else
                                                if (command_mes == arrCommand[(int)Command.Stop])//обработка запроса остановки
                                                {
                                                    Invoke(d_statLbl, false);
                                                    if (StopWork != null)
                                                        StopWork(this, new EventArgs());
                                                    else ;
                                                }
                                                else
                                                    if (command_mes == arrCommand[(int)Command.SetStat])//обработка запроса изменения типа экземпляра
                                                    {
                                                        m_client.SendDisconnect();
                                                        Invoke(d_reconn, new object[] {"", true });
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
                                            if (StartWork != null)
                                                StartWork(this, new EventArgs());
                                            else ;
                                                 
                                        }
                                        else
                                            if (command_mes == arrCommand[(int)Command.Stop])//запрос остановки
                                            {
                                                Invoke(d_statLbl, false);
                                                if (StopWork != null)
                                                    StopWork(this, new EventArgs());
                                                else ;
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
                    m_client.SendDisconnect();//Отправка сообщения о разрыве соединения
                    m_client.StopClient();//Остановка клиента
                    break;
                case TypeApp.Server:
                    m_server.SendDisconnect();//Отправка сообщения о разрыве соединения
                    m_server.StopServer();//Остановка сервера
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
            }
            else//иначе
            {
                cbClients.Items.Remove(idClient);//удаляем клиента из списка и обновляем
                cbClients.Text = string.Empty;
                cbClients.Refresh();
            }
        }

        /// <summary>
        /// Метод перезапуска клиент/серверной части
        /// </summary>
        /// <param name="new_server">Пустая строка при полном перезапуске или имя сервера для подключения к нему</param>
        private void reconnect(string new_server="", bool stat = false)
        {
            //Close();
            switch (m_type_app)
            {
                case TypeApp.Client:
                    m_client.StopClient();//остановка клиента
                    break;
                case TypeApp.Server:
                    m_server.StopServer();//остановка сервера
                    break;
            }
            
            initialize(new_server,stat);//подключение к новому серверу или запуск сервера
        }

        /// <summary>
        /// Метод для изменения label'a
        /// </summary>
        /// <param name="start">Если запуск то true</param>
        private void setLbl(bool start)
        {
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

        #region События панели
        /// <summary>
        /// Тип делегата для обработки события
        /// </summary>
        public delegate void StartEventHandler(object obj, EventArgs e);

        /// <summary>
        /// Событие
        /// </summary>
        public StartEventHandler StartWork;

        
        /// <summary>
        /// Тип делегата для обработки события
        /// </summary>
        public delegate void StopEventHandler(object obj, EventArgs e);

        /// <summary>
        /// Событие
        /// </summary>
        public StopEventHandler StopWork;
        #endregion

    }
}
