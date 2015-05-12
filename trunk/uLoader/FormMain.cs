using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace uLoader
{
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

            m_panelWork = new PanelWork(); m_panelWork.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelWork);
            m_panelConfig = new PanelConfig(); m_panelConfig.EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedFormMain_PanelConfig);

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

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (работаToolStripMenuItem.Checked == true)
            {
                m_TabCtrl.AddTabPage(работаToolStripMenuItem.Text, 1, HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED);
                m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add(m_panelWork);
                m_TabCtrl.PrevSelectedIndex = 0;
            }
            else
                ;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_handler.Activate(false); m_handler.Stop();
        }

        private void onCloseTabPage(object obj, HTabCtrlExEventArgs ev)
        {
            switch (ev.Id)
            {
                case 1: //Работа
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
            PanelCommon panelCommon;

            if (! (tabCtrl.PrevSelectedIndex < 0))
            {
                panelCommon = (tabCtrl.TabPages[tabCtrl.PrevSelectedIndex].Controls[0] as PanelCommon);
                panelCommon.Activate (false);                
            }
            else
                ;

            panelCommon = (tabCtrl.TabPages[tabCtrl.SelectedIndex].Controls[0] as PanelCommon);
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
                m_TabCtrl.RemoveTabPage(strNameMenuItem);

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
        }

        private void OnEvtDataAskedFormMain_PanelConfig(object obj)
        {
            m_handler.Push(m_panelConfig, ((obj as EventArgsDataHost).par as object[])[0] as int []);
        }
    }
}
