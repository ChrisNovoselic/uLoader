﻿using System;
using System.Windows.Forms;

using HClassLibrary;

namespace uLoader
{
    /// <summary>
    /// Перечисление для индексирования 'SEC_SRC_TYPES' (источник, назначение)
    /// </summary>
    public enum INDEX_SRC
    {
        SOURCE,
        DEST
            , COUNT_INDEX_SRC
    };

    public partial class FormMain : Form
    {
        private FormWait m_formWait;
        
        private HHandlerQueue m_handler;

        enum INDEX_TAB { WORK, CONFIG, COUNT_INDEX_TAB };

        private PanelWork m_panelWork;
        private PanelConfig m_panelConfig;        

        public FormMain()
        {
            string strNameFileINI = string.Empty;
            createHCmdArg(Environment.GetCommandLineArgs(), ref strNameFileINI);

            InitializeComponent();

            m_formWait = FormWait.This;

            m_handler = new HHandlerQueue(strNameFileINI);
            m_handler.Start();
            m_handler.Activate(true);

            m_panelWork = new PanelWork(); m_panelWork.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelWork); m_panelWork.Start();
            m_panelConfig = new PanelConfig(); m_panelConfig.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelConfig); m_panelConfig.Start ();

            работаToolStripMenuItem.CheckOnClick =
            конфигурацияToolStripMenuItem.CheckOnClick =
                 true;

            работаToolStripMenuItem.CheckStateChanged += new EventHandler(работаToolStripMenuItem_CheckStateChanged);
            конфигурацияToolStripMenuItem.CheckStateChanged += new EventHandler(конфигурацияToolStripMenuItem_CheckStateChanged);

            m_TabCtrl.EventHTabCtrlExClose += new HTabCtrlEx.DelegateHTabCtrlEx(onCloseTabPage);
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
                switch (cmd)
                {
                    case "conf_ini":
                        strNameFileINI = param;
                        break;
                    default:
                        strNameFileINI = "";
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

        /// <summary>
        /// Обработка события окончания загрузки главной формы приложения
        /// </summary>
        /// <param name="sender">Объект, инийиирововший событие (форма)</param>
        /// <param name="e">Аргументы события</param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            m_formWait.StartWaitForm (Location, Size);

            m_handler.AutoStart ();

            //Проверить признак отображения вкладки "работа"
            if (работаToolStripMenuItem.Checked == true)
            {
                //Добавить вкладку
                m_TabCtrl.AddTabPage(работаToolStripMenuItem.Text, 1, HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
                m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add(m_panelWork);
                //Запомнить "предыдущий" выбор
                m_TabCtrl.PrevSelectedIndex = 0;
            }
            else
                ;

            m_formWait.StopWaitForm ();

            this.m_notifyIcon.Icon = this.Icon;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            m_notifyIcon.Visible = false;
        }

        // Перехват нажатия на кнопку свернуть
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

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_panelWork.Stop();
            m_panelConfig.Stop();
            
            m_handler.Activate(false); m_handler.Stop();
        }

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
                default:
                    break;
            }
        }

        private void работаToolStripMenuItem_CheckStateChanged(object obj, EventArgs ev)
        {
        }        

        private void TabCtrl_OnPrevSelectedIndexChanged(int indx)
        {
            Logging.Logg().Action(@"Смена вкладки: активная - " + m_TabCtrl.SelectedTab.Text, Logging.INDEX_MESSAGE.NOT_SET);

            HPanelCommon panelCommon;
            //Проверить наличие вкладки для деактивации перед активации выбранной пользователем
            if (!(m_TabCtrl.PrevSelectedIndex < 0)
                && (m_TabCtrl.PrevSelectedIndex < m_TabCtrl.TabCount))
            {
                // деактивировать предыдущую вкладку
                panelCommon = (m_TabCtrl.TabPages[m_TabCtrl.PrevSelectedIndex].Controls[0] as HPanelCommon);
                panelCommon.Activate(false);
            }
            else
                ;
            // активировать выбранную вкладку
            panelCommon = (m_TabCtrl.TabPages[m_TabCtrl.SelectedIndex].Controls[0] as HPanelCommon);
            panelCommon.Activate(true);
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
                m_TabCtrl.AddTabPage(strNameMenuItem, 2, HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
                m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add(m_panelConfig);
            }
            else
            {
                m_panelConfig.Activate(false);
                m_TabCtrl.RemoveTabPage(strNameMenuItem);
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
    }
}
