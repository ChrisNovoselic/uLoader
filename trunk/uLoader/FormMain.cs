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
        private FileINI m_fileINI;
        
        enum INDEX_TAB { WORK, CONFIG, COUNT_INDEX_TAB };

        private PanelWork m_panelWork;
        private PanelConfig m_panelConfig;        

        public FormMain()
        {
            InitializeComponent();

            m_fileINI = new FileINI ();
            string val = m_fileINI.GetMainValueOfKey (@"Position");
            val = m_fileINI.GetSecValueOfKey(@"GDst-GSgnls0", @"Sgnl1");
            val = m_fileINI.GetSecValueOfKey(@"GSrc-GSgnls0", @"Sgnl1");
            val = m_fileINI.GetSecValueOfKey(@"GDst-GD1", @"D1");

            m_panelWork = new PanelWork ();
            m_panelConfig = new PanelConfig ();

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
            }
            else
                ;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {            
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

        private void TabCtrl_OnSelectedIndexChanged(object obj, EventArgs ev)
        {
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
    }
}
