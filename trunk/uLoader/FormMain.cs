using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
    /// <summary>
    /// Перечисление для индексирования 'SEC_SRC_TYPES' (источник, назначение)
    /// </summary>
    enum INDEX_SRC
    {
        SOURCE,
        DEST
            , COUNT_INDEX_SRC
    };    

    public partial class FormMain : uLoaderCommon.FormMainBase
    {
        /// <summary>
        /// Объект для визуализации процесса выполнения длительных операций
        /// </summary>
        private FormWait m_formWait;
        /// <summary>
        /// Перечисление - индексы вкладок в главном окне приложения
        /// </summary>
        enum INDEX_TAB { WORK, INTERACTION, CONFIG, COUNT_INDEX_TAB };
        /// <summary>
        /// Панель с элементами управления - действия по выполнению целевых функций приложения
        /// </summary>
        private PanelWork m_panelWork;
        /// <summary>
        /// Панель с элементами управления - ClientServer
        /// </summary>
        private PanelClientServer m_panelCS;
        /// <summary>
        /// Панель с элементами управления - действия по конфигурации приложения
        /// </summary>
        private PanelConfig m_panelConfig;
        /// <summary>
        /// Упрощенный(без приведения к типу) доступ к объекту обработки очереди событий 
        /// </summary>
        private HHandlerQueue Handler { get { return m_handler as HHandlerQueue; } }

        #region Настройка логгирования
        private struct LOGGING_MESSAGE
        {
            public Logging.INDEX_MESSAGE IndexMessage;

            public bool Allowed;

            public string Description;
        }

        private enum LOGGING_ID {
            INIT
            , TABLE_RES
            , START_STOP
            , QUEUE
            , DEQUEUE_DEST
            , STATE_SRC
            , QUERY_SRC
            , REC_INSERT_DEST
        }

        private static LOGGING_MESSAGE [] _loggingMessageSetup = new LOGGING_MESSAGE [] // размер должен совпадать с размером перечисления
        {
            new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.A_001, Allowed = true, Description = "Инициализация гр.источников, гр.сигн." } // INIT
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.A_002, Allowed = false, Description = "Результат запроса(кол-во записей)" } // TABLE_RES
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.A_003, Allowed = true, Description = "Запуск/останов гр. источн., гр.сигн." } // START_STOP
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.D_001, Allowed = false, Description = "Постановка/снятие события в/из очередь(и)" } // QUEUE
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.D_005, Allowed = false, Description = "Снятие события из очереди назначения" } // DEQUEUE_DEST
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.D_003, Allowed = false, Description = "Изменение состояния(STATE_REQUEST) гр.сигн. источн." } // STATE_SRC
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.D_004, Allowed = false, Description = "Содержание запроса источника" } // QUERY_SRC
            , new LOGGING_MESSAGE() { IndexMessage = Logging.INDEX_MESSAGE.D_006, Allowed = false, Description = "Кол-во записей вставки в гр.сигн. назнач." } // REC_INSERT_DEST
        };

        private string fGetINIParametersOfID (int id)
        {
            return id < _loggingMessageSetup.Length ? _loggingMessageSetup[id].Allowed.ToString () : false.ToString ();
        }

        private void loggingLinkId ()
        {
            Logging.DelegateGetINIParametersOfID = new StringDelegateIntFunc (fGetINIParametersOfID);
            foreach (LOGGING_ID indx in Enum.GetValues (typeof (LOGGING_ID)))
                Logging.LinkId (_loggingMessageSetup[(int)indx].IndexMessage, (int)indx);
            Logging.UpdateMarkDebugLog ();
        }

        private void toolStripMenuItemFileLogging_CheckedChanged (object sender, EventArgs e)
        {
            _loggingMessageSetup [(int)(sender as System.Windows.Forms.ToolStripMenuItem).Tag].Allowed =
                (sender as System.Windows.Forms.ToolStripMenuItem).Checked;
            Logging.UpdateMarkDebugLog ();
        }
        #endregion

        /// <summary>
        /// Конструктор - основной (без аргументов)
        /// </summary>
        public FormMain ()
            : base (@"IconMainULoader")
        {
            //Logging.Logg().Debug(string.Format(@"FormMain::ctor() - вХод, IsNormalized={0} ...", HCmd_Arg.IsNormalized), Logging.INDEX_MESSAGE.NOT_SET);

            loggingLinkId ();

            InitializeComponent ();

            if (HCmd_Arg.IsNormalized == true)
                m_formWait = FormWait.This;
            else
                ;

            _statePanelWork = PanelWork.STATE.Unknown;

            //// настраиваемые параметры манагера состояний объектов
            //HHandlerQueue.MSEC_TIMERFUNC_UPDATE = 1006;
            //HHandlerQueue.MSEC_CONFIRM_WAIT = 6666;
            createHandlerQueue(typeof(HHandlerQueue));
            Handler.EventInteraction += new DelegateObjectFunc (onEventInteraction);
            m_handler.Start(); m_handler.Activate(true);
            //m_handler.EventCrashed += new HHandlerQueue.EventHandlerCrashed(onCrashed);

            m_panelWork = new PanelWork(); m_panelWork.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelWork);
            m_panelConfig = new PanelConfig(); m_panelConfig.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelConfig);
            //m_handler.Push();
            m_panelCS = new PanelClientServer(new PanelClientServer.InteractionParameters (
                //@"NE2844, NE3336"
                //, @"MainPipe"
            )); m_panelCS.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelCS);
            // автоматическое изменение состояния п.меню
            работаToolStripMenuItem.CheckOnClick =
            конфигурацияToolStripMenuItem.CheckOnClick =
            взаимодействиеToolStripMenuItem.CheckOnClick=
                 true;            
            // п.п.меню заблокированы - изменение состояния только программно
            //взаимодействиеToolStripMenuItem.CheckStateChanged += new EventHandler(взаимодействиеToolStripMenuItem_CheckStateChanged);
            //работаToolStripMenuItem.CheckStateChanged += new EventHandler(работаToolStripMenuItem_CheckStateChanged);
            конфигурацияToolStripMenuItem.CheckStateChanged += new EventHandler(конфигурацияToolStripMenuItem_CheckStateChanged);

            m_TabCtrl.EventHTabCtrlExClose += new HTabCtrlEx.DelegateHTabCtrlEx(onCloseTabPage);

            //Logging.Logg().Debug(string.Format(@"FormMain::ctor() - вЫХод ..."), Logging.INDEX_MESSAGE.NOT_SET);
        }

        void файлКонфигурацияЗагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void файлКонфигурацияСохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void autoStart()
        {
            if (!(m_statePanelWork == PanelWork.STATE.Started)) {
                Handler.AutoStart();

                m_statePanelWork = PanelWork.STATE.Started;
            } else
                ;
        }

        private void autoStop()
        {
            if (m_statePanelWork == PanelWork.STATE.Started) {
                Handler.AutoStop();

                m_statePanelWork = PanelWork.STATE.Paused;
            } else
                ;
        }

        private void interactionInitializeComlpeted(object par)
        {
            try
            {
                // сообщение приходит только в случае готовности панели "Взаимодействие"
                взаимодействиеToolStripMenuItem.Checked = (int)par == 0;

                //Проверить признак отображения вкладки "взаимодействие"
                if (взаимодействиеToolStripMenuItem.Checked == true) {
                    //Добавить вкладку
                    m_TabCtrl.AddTabPage(m_panelCS
                        , взаимодействиеToolStripMenuItem.Text
                        , (int)INDEX_TAB.INTERACTION
                        , HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
                } else
                    m_panelCS.Stop();
            } catch (Exception e) {
                //Console.WriteLine(e.Message);
                Logging.Logg().Exception(e, @"FormMain::interactionInitializeComlpeted () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (HCmd_Arg.IsNormalized == true)
                m_formWait.StopWaitForm();
            else
                ;
        }

        private PanelWork.STATE _statePanelWork;

        private PanelWork.STATE m_statePanelWork {
            get { return _statePanelWork; }

            set {
                if (!(_statePanelWork == value)) {
                // указать панели состояние локального экземпляра (для передачи взаимодействующему)
                    m_panelCS.OnEvtDataRecievedHost(new object[] { (int)HHandlerQueue.StatesMachine.FORMMAIN_COMMAND_TO_INTERACTION, PanelClientServer.ID_EVENT.State, value });

                    _statePanelWork = value;
                } else
                    ;
            }
        }

        /// <summary>
        /// Обработчик события StatesMachine.INTERACTION_EVENT
        /// </summary>
        /// <param name="obj">Параметры(аргументы) события</param>
        private void onEventInteraction(object obj)
        {
            //см. HandlerQueue::StateCheckResponse 'StatesMachine.INTERACTION_EVENT'
            //1-ый(0) - идентификатор сообщения(Pipes.Pipe.ID_EVENT)
            //2-ой(1) - дополнительная информация
            object[] pars = obj as object[];

            IAsyncResult iaRes = null;
            PanelWork.STATE curRemoteState = PanelWork.STATE.Unknown;
            Pipes.Pipe.Role roleActived = Pipes.Pipe.Role.Unknown;

            switch ((PanelClientServer.ID_EVENT)pars[0])
            {
                case PanelClientServer.ID_EVENT.State:
                    //??? проверить размерность массива
                    roleActived = (Pipes.Pipe.Role)pars[1];

                    if (pars.Length > 2) {// изменение состояния
                        curRemoteState = (PanelWork.STATE)pars[2];

                        // в ~ от состояния взаимодействующей панели (старт с условием)
                        if ((!(m_statePanelWork == PanelWork.STATE.Started))
                            && ((curRemoteState == PanelWork.STATE.Unknown)
                                || (curRemoteState == PanelWork.STATE.Paused)))
                            if (InvokeRequired == true) {
                                iaRes = BeginInvoke(new DelegateFunc(autoStart));
                                //EndInvoke(iaRes);
                            } else
                                autoStart();
                        else
                            if ((m_statePanelWork == PanelWork.STATE.Started)
                                && (curRemoteState == PanelWork.STATE.Started))
                                if (InvokeRequired == true) {
                                    iaRes = BeginInvoke(new DelegateFunc(autoStop));
                                    //EndInvoke(iaRes);
                                } else
                                    autoStop();
                            else
                                ;
                    } else
                        if (pars.Length > 1) {// 1-ая инициализация
                            if (m_panelCS.Ready > PanelClientServer.ERROR.CRITICAL) {
                                if (InvokeRequired == true) {
                                    iaRes = BeginInvoke(new DelegateObjectFunc(interactionInitializeComlpeted), m_panelCS.Ready);
                                    //EndInvoke(iaRes);
                                } else
                                    interactionInitializeComlpeted(m_panelCS.Ready);

                                if (!(roleActived == Pipes.Pipe.Role.Client))
                                    if (InvokeRequired == true) {
                                        iaRes = BeginInvoke(new DelegateFunc(autoStart));
                                        //EndInvoke(iaRes);
                                    } else
                                        autoStart();
                                else
                                    m_statePanelWork = PanelWork.STATE.Paused;
                            } else
                                BeginInvoke(new DelegateFunc(delegate { Close(); })); ;
                        } else
                            ;
                    break;
                case PanelClientServer.ID_EVENT.Start:
                // команда старт от взаимодействующего экземпляра
                    break;
                case PanelClientServer.ID_EVENT.Stop:
                    break;
                case PanelClientServer.ID_EVENT.Connect:
                    break;
                case PanelClientServer.ID_EVENT.Disconnect:
                    break;
                case PanelClientServer.ID_EVENT.Exit:
                    break;
                default:
                    break;
            }            
        }

        /// <summary>
        /// Обработка события окончания загрузки главной формы приложения
        /// </summary>
        /// <param name="sender">Объект, инийиирововший событие (форма)</param>
        /// <param name="e">Аргумент события</param>
        protected override void FormMain_Load(object sender, EventArgs e)
        {
            base.FormMain_Load(sender, e);

            m_panelWork.Start();
            m_panelConfig.Start();
            m_panelCS.Start();

            if (HCmd_Arg.IsNormalized == true)
                m_formWait.StartWaitForm(Location, Size);
            else
                ;

            //Проверить признак отображения вкладки "работа"
            if (работаToolStripMenuItem.Checked == true)
            {
                //Добавить вкладку
                m_TabCtrl.AddTabPage(m_panelWork
                    , работаToolStripMenuItem.Text
                    , (int)INDEX_TAB.WORK
                    , HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
                //Запомнить "предыдущий" выбор
                m_TabCtrl.PrevSelectedIndex = 0;
            }
            else
                ;

            //??? почемы вызов не в базовом классе
            initFormMainSizing();
        }

        /// <summary>
        /// Обработчик события - закрытие формы
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (форма)</param>
        /// <param name="e">Аргумент события</param>
        protected override void FormMain_Closing(object sender, FormClosingEventArgs e)
        {
            m_panelWork.Stop();
            m_panelConfig.Stop();
            if (m_panelCS.Ready == 0)
                m_panelCS.Stop();
            else
                ;

            base.FormMain_Closing(sender, e);
        }

        /// <summary>
        /// Обработчик события - закрытие вкладки
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (менеджер вкладок)</param>
        /// <param name="ev">Аргумент события</param>
        private void onCloseTabPage(object obj, HTabCtrlExEventArgs ev)
        {
            switch (ev.Id)
            {
                case 1: //Работа
                    MessageBox.Show (this, "Вкладка \"Работа\" не может быть закрыта!", @"Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case 2: //Конфигурация
                    конфигурацияToolStripMenuItem.PerformClick();
                    break;
                case 3: //Взаимодействие
                    MessageBox.Show(this, "Вкладка \"Взаимодействие\" не может быть закрыта!", @"Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                default:
                    break;
            }
        }

        // п.п.меню заблокированы - изменение состояния только программно
        //private void работаToolStripMenuItem_CheckStateChanged(object obj, EventArgs ev)
        //{
        //}

        //private void взаимодействиеToolStripMenuItem_CheckStateChanged(object obj, EventArgs ev)
        //{
        //}

        /// <summary>
        /// Обработчик события - изменения свойства - предыдущий индекс выбранной вкладки
        /// </summary>
        /// <param name="indx">Новое значение для индекса</param>
        private void TabCtrl_OnPrevSelectedIndexChanged(int indx)
        {
            Logging.Logg().Action(@"Смена вкладки: активная - " + m_TabCtrl.SelectedTab.Text, Logging.INDEX_MESSAGE.NOT_SET);

            HPanelCommon panelCommon;
            //Проверить наличие вкладки для деактивации перед активации выбранной пользователем
            
            if (!(m_TabCtrl.PrevSelectedIndex < 0)
                && (m_TabCtrl.PrevSelectedIndex < m_TabCtrl.TabCount))
            {
                //if (m_TabCtrl.PrevSelectedIndex != 1)
                //{
                    // деактивировать предыдущую вкладку
                    panelCommon = (m_TabCtrl.TabPages[m_TabCtrl.PrevSelectedIndex].Controls[0] as HPanelCommon);
                    panelCommon.Activate(false);
                //}
            }
            else
                ;
            //if (m_TabCtrl.SelectedIndex != 1)
            //{
                // активировать выбранную вкладку
                panelCommon = (m_TabCtrl.TabPages[m_TabCtrl.SelectedIndex].Controls[0] as HPanelCommon);
                panelCommon.Activate(true);
            //}
        }

        /// <summary>
        /// Обработчик события - изменения выбора вкладки
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void TabCtrl_OnSelectedIndexChanged(object obj, EventArgs ev)
        {
            (obj as HTabCtrlEx).PrevSelectedIndex = (obj as HTabCtrlEx).SelectedIndex;
        }

        /// <summary>
        /// Обработчик события - изменение состояния выбора п. меню "Конфигурация"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        private void конфигурацияToolStripMenuItem_CheckStateChanged(object obj, EventArgs ev)
        {
            string strNameMenuItem = (obj as ToolStripMenuItem).Text;

            if (конфигурацияToolStripMenuItem.Checked == true)
            {
                m_TabCtrl.AddTabPage(m_panelConfig
                    , strNameMenuItem
                    , (int)INDEX_TAB.CONFIG
                    , HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
            }
            else
            {
                m_panelConfig.Activate(false);
                m_TabCtrl.RemoveTabPage(); //strNameMenuItem                
            }

            activateMenuItemConfig(конфигурацияToolStripMenuItem.Checked);
        }

        /// <summary>
        /// Иизменить доступность п. меню "Файл - конфигурация"
        /// </summary>
        /// <param name="activate">Признак доступности п. меню</param>
        private void activateMenuItemConfig(bool activate)
        {
            this.файлКонфигурацияToolStripMenuItem.Enabled = activate;
        }

        /// <summary>
        /// Обработчик события - выбор п. меню "О программе"
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент события</param>
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout fAbout = new FormAbout())
            {
                fAbout.ShowDialog();
            }
        }

        /// <summary>
        /// Обработчик события приема сообщений от вкладки (панели) "Работа"
        /// </summary>
        /// <param name="obj">Массив объектов сообщения</param>
        private void OnEvtDataAskedFormMain_PanelWork(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            //ev.id - здесь всегда = -1
            m_handler.Push(m_panelWork, ev.par as object[]);
        }

        /// <summary>
        /// Обработчик события приема сообщений от вкладки (панели) "Конфигурация"
        /// </summary>
        /// <param name="obj">Массив объектов сообщения</param>
        private void OnEvtDataAskedFormMain_PanelConfig(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            //ev.id - здесь всегда = -1
            m_handler.Push(m_panelConfig, ev.par as object[]);
        }

        /// <summary>
        /// Обработчик события приема сообщений от вкладки (панели) "Конфигурация"
        /// </summary>
        /// <param name="obj">Массив объектов сообщения</param>
        private void OnEvtDataAskedFormMain_PanelCS(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            //ev.id - здесь всегда = -1
            m_handler.Push(m_panelCS, ev.par as object[]);
        }
    }

    public abstract class HPanelCommonDataHost : PanelCommonDataHost
    {
        public HPanelCommonDataHost(int cntCol, int cntRow)
            : base(cntCol, cntRow)
        {
        }

        public HPanelCommonDataHost(IContainer container, int cntCol, int cntRow)
            : base(container, cntCol, cntRow)
        {
        }
        /// <summary>
        /// Обработчик события получения данных по запросу (выполняется в потоке получения результата)
        /// </summary>
        /// <param name="obj">Результат, полученный по запросу</param>
        public override void OnEvtDataRecievedHost(object obj)
        {
            try {
                if (InvokeRequired == true)
                    //if (IsHandleCreated == true)
                        Invoke(new DelegateObjectFunc(onEvtDataRecievedHost), obj);
                    //else
                    //    throw new Exception(@"::OnEvtDataRecievedHost () - IsHandleCreated==False");
                else
                    onEvtDataRecievedHost(obj);
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"FormMain::OnEvtDataRecievedHost () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            base.OnEvtDataRecievedHost(obj);
        }

        protected abstract void onEvtDataRecievedHost(object obj);
    }
}
