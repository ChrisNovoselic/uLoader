using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;

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
        private HHandlerQueue m_handler;

        enum INDEX_TAB { WORK, CONFIG, COUNT_INDEX_TAB };

        private PanelWork m_panelWork;
        private PanelConfig m_panelConfig;        

        public FormMain()
        {
            InitializeComponent();

            m_handler = new HHandlerQueue();
            m_handler.Start(); m_handler.Activate(true);

            m_panelWork = new PanelWork(); m_panelWork.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelWork); m_panelWork.Start();
            m_panelConfig = new PanelConfig(); m_panelConfig.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelConfig); m_panelConfig.Start ();

            работаToolStripMenuItem.CheckOnClick =
            конфигурацияToolStripMenuItem.CheckOnClick =
                 true;

            работаToolStripMenuItem.CheckStateChanged += new EventHandler(работаToolStripMenuItem_CheckStateChanged);
            конфигурацияToolStripMenuItem.CheckStateChanged += new EventHandler(конфигурацияToolStripMenuItem_CheckStateChanged);

            m_TabCtrl.OnClose += new HTabCtrlEx.DelegateOnHTabCtrlEx(onCloseTabPage);
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

        private void TabCtrl_OnPrevSelectedIndexChanged(object obj, EventArgs ev)
        {
            Logging.Logg().Action(@"Смена вкладки: активная - " + (obj as HTabCtrlEx).SelectedTab.Text, Logging.INDEX_MESSAGE.NOT_SET);

            HTabCtrlEx tabCtrl = obj as HTabCtrlEx;
            HPanelCommon panelCommon;

            if (!(tabCtrl.PrevSelectedIndex < 0)
                && (tabCtrl.PrevSelectedIndex < tabCtrl.TabCount))
            {
                panelCommon = (tabCtrl.TabPages[tabCtrl.PrevSelectedIndex].Controls[0] as HPanelCommon);
                panelCommon.Activate(false);
            }
            else
                ;

            panelCommon = (tabCtrl.TabPages[tabCtrl.SelectedIndex].Controls[0] as HPanelCommon);
            panelCommon.Activate(true);
        }

        private void TabCtrl_OnSelectedIndexChanged(object obj, EventArgs ev)
        {
            (obj as HTabCtrlEx).PrevSelectedIndex = (obj as HTabCtrlEx).SelectedIndex;
        }

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

        private void activateMenuItemConfig(bool activate)
        {
            this.файлКонфигурацияToolStripMenuItem.Enabled = activate;
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout fAbout = new FormAbout())
            {
                fAbout.ShowDialog();
            }
        }

        private void OnEvtDataAskedFormMain_PanelWork(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            //ev.id - здесь всегда = -1
            m_handler.Push(m_panelWork, ev.par as object[]);
        }

        private void OnEvtDataAskedFormMain_PanelConfig(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            //ev.id - здесь всегда = -1
            m_handler.Push(m_panelConfig, ev.par as object[]);
        }
    }
}
