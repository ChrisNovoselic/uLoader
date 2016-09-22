using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;

using HClassLibrary;

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

    public partial class FormMain : Form
    {
        public enum STATE_EXECUTE { NORMALIZE, MINIMIZE }
        /// <summary>
        /// Признак типа выполнения приложения в ~ от указанного аргумента/параметра в командной строке
        /// </summary>
        public static STATE_EXECUTE s_state_execute;
        /// <summary>
        /// Объект для визуализации процесса выполнения длительных операций
        /// </summary>
        private FormWait m_formWait;
        /// <summary>
        /// Объект обработки событий
        /// </summary>
        private HHandlerQueue m_handler;
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

        public FormMain()
        {
            string strNameFileINI = string.Empty;
            createHCmdArg(Environment.GetCommandLineArgs(), ref strNameFileINI);

            InitializeComponent();

            m_formWait = FormWait.This;

            //// настраиваемые параметры манагера состояний объектов
            //HHandlerQueue.MSEC_TIMERFUNC_UPDATE = 1006;
            //HHandlerQueue.MSEC_CONFIRM_WAIT = 6666;            
            m_handler = new HHandlerQueue(strNameFileINI);
            m_handler.EventInteraction += new DelegateObjectFunc (onEventInteraction);
            m_handler.Start(); m_handler.Activate(true);
            //m_handler.EventCrashed += new HHandlerQueue.EventHandlerCrashed(onCrashed);

            m_panelWork = new PanelWork(); m_panelWork.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelWork); m_panelWork.Start();
            m_panelConfig = new PanelConfig(); m_panelConfig.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelConfig); m_panelConfig.Start ();
            //m_handler.Push();
            m_panelCS = new PanelClientServer(new PanelClientServer.InteractionParameters (
                //@"NE2844, NE3336"
                //, @"MainPipe"
            )); m_panelCS.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelCS); m_panelCS.Start();
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

            switch (s_state_execute)
            {
                case STATE_EXECUTE.MINIMIZE:
                    Message msg = new Message();
                    msg.Msg = 0x112;
                    msg.WParam = (IntPtr)(0xF020);
                    WndProc(ref msg);
                    break;
                case STATE_EXECUTE.NORMALIZE:
                    this.OnMaximumSizeChanged(null);
                    break;
            }
        }

        /// <summary>
        /// Создание объекта-обработчика аргументов командной строки
        /// </summary>
        /// <param name="args">Массив аргументов командной строки</param>
        /// <returns>Объект-обработчик аргументов командной строки</returns>
        protected HCmd_Arg createHCmdArg(string[] args, ref string strNameFileINI)
        {
            return new handlerCmd(args, ref strNameFileINI);
        }

        /// <summary>
        /// Класс обработки "своих" команд
        /// </summary>
        public class handlerCmd : HCmd_Arg
        {
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="args">Массив аргументов командной строки</param>
            public handlerCmd(string[] args, ref string strNameFileINI)
                : base(args)
            {
               strNameFileINI = RunCmd();
            }


            /// <summary>
            /// обработка "своих" команд
            /// </summary>
            /// <param name="command"></param>
            private string RunCmd()
            {
                string strNameFileINI = string.Empty;

                foreach (KeyValuePair <string, string> pair in m_dictCmdArgs)
                    switch (pair.Key)
                    {
                        case "conf_ini":
                            strNameFileINI = pair.Value;
                            break;
                        case "minimize":
                            s_state_execute = STATE_EXECUTE.MINIMIZE;
                            break;
                        default:
                            //strNameFileINI = string.Empty;
                            break;
                    }

                return strNameFileINI;
            }
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
            m_handler.AutoStart();
        }

        private void interactionInitializeComlpeted(object par)
        {
            try
            {
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
                // сообщение приходит только в случае готовности панели "Взаимодействие"
                взаимодействиеToolStripMenuItem.Checked = (int)par == 0;

                //Проверить признак отображения вкладки "взаимодействие"
                if (взаимодействиеToolStripMenuItem.Checked == true)
                {
                    //Добавить вкладку
                    m_TabCtrl.AddTabPage(m_panelCS
                        , взаимодействиеToolStripMenuItem.Text
                        , (int)INDEX_TAB.INTERACTION
                        , HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
                }
                else
                    m_panelCS.Stop();
            } catch (Exception e) {
                //Console.WriteLine(e.Message);
                Logging.Logg().Exception(e, @"FormMain::interactionInitializeComlpeted () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }


            m_formWait.StopWaitForm();
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

            bool bStarted = false;
            PanelWork.STATE prevRemoteState = PanelWork.STATE.Unknown
                , curRemoteState = PanelWork.STATE.Unknown
                , curLocalState = PanelWork.STATE.Unknown;

            switch ((PanelClientServer.ID_EVENT)pars[0])
            {
                case PanelClientServer.ID_EVENT.State:
                    if (pars.Length > 2) {
                        prevRemoteState = (PanelWork.STATE)pars[1];
                        curRemoteState = (PanelWork.STATE)pars[2];

                        if ((prevRemoteState == PanelWork.STATE.Unknown)
                            //&& (curRemoteState == PanelWork.STATE.Unknown)
                            )
                            if (InvokeRequired == true)
                                BeginInvoke(new DelegateObjectFunc(interactionInitializeComlpeted), m_panelCS.Ready);
                            else
                                interactionInitializeComlpeted(m_panelCS.Ready);                            
                        else
                            ;
                        // в ~ от состояния взаимодействующей панели (старт с условием)
                        bStarted = m_panelCS.Ready == 0 ?
                            ((curRemoteState == PanelWork.STATE.Paused) || (curRemoteState == PanelWork.STATE.Unknown)) :
                                true;
                    }
                    else
                    // команда старт от взаимодействующего экземпляра
                        bStarted = true;

                    if (bStarted == true) {
                        if (InvokeRequired == true)
                            BeginInvoke(new DelegateFunc(autoStart));
                        else
                            autoStart();

                        curLocalState = PanelWork.STATE.Started;
                    }
                    else
                        curLocalState = PanelWork.STATE.Paused;
                    // указать панели состояние локального экземпляра (для передачи взаимодействующему)
                    m_panelCS.OnEvtDataRecievedHost(new object[] { (int)HHandlerQueue.StatesMachine.FORMMAIN_COMMAND_TO_INTERACTION, PanelClientServer.ID_EVENT.State, curLocalState });
                    break;
                case PanelClientServer.ID_EVENT.Start:
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
        private void FormMain_Load(object sender, EventArgs e)
        {
            m_formWait.StartWaitForm (Location, Size);            

            this.m_notifyIcon.Icon = this.Icon;
        }
        /// <summary>
        /// Обработчик события - нажатие на пиктограмму в области системных оповещений ОС
        /// </summary>
        /// <param name="sender">Объект - инициатор события - ???</param>
        /// <param name="e">Аргумент события</param>
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            m_notifyIcon.Visible = false;
        }
        /// <summary>
        /// Переопределение ф-и обработки оконных событий
        ///  переопрделение нажатия на кнопку свернуть
        /// </summary>
        /// <param name="m">Объект сообщения</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x112)
            {
                if (m.WParam.ToInt32() == 0xF020)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    m_notifyIcon.Visible = true;

                    return;
                }
            }
            else
                ;

            base.WndProc(ref m);
        }
        /// <summary>
        /// Обработчик события - закрытие формы
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (форма)</param>
        /// <param name="e">Аргумент события</param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_notifyIcon.Visible = false;
            m_panelWork.Stop();
            m_panelConfig.Stop();
            if (m_panelCS.Ready == 0)
                m_panelCS.Stop();
            else
                ;

            m_handler.Activate(false); m_handler.Stop();
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
            if (InvokeRequired == true)
                if (IsHandleCreated == true)
                    this.BeginInvoke(new DelegateObjectFunc(onEvtDataRecievedHost), obj);
                else
                    throw new Exception(@"::OnEvtDataRecievedHost () - IsHandleCreated==False");
            else
                onEvtDataRecievedHost(obj);

            base.OnEvtDataRecievedHost(obj);
        }

        protected abstract void onEvtDataRecievedHost(object obj);
    }
}
