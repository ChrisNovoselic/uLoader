using HClassLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace xmlLoader
{
    public partial class FormMain : uLoaderCommon.FormMainBase
    {
        private enum INDEX_CONTROL { CBX_READ_SESSION_START, CBX_READ_SESSION_STOP }

        private UDPListener m_udpListener;

        private HHandlerQueue Handler { get { return m_handler as HHandlerQueue; } }

        public FormMain() : base ()
        {
            int indxRow = -1;

            InitializeComponent();

            m_cbxReadSessionStart.Tag = INDEX_CONTROL.CBX_READ_SESSION_START;
            m_cbxReadSessionStop.Tag = INDEX_CONTROL.CBX_READ_SESSION_STOP;

            indxRow = m_dgvSession.Rows.Add();
            m_dgvSession.Rows[indxRow].HeaderCell.Value = @"Крайний пакет";
            indxRow = m_dgvSession.Rows.Add();
            m_dgvSession.Rows[indxRow].HeaderCell.Value = @"Длительность сессии";
            indxRow = m_dgvSession.Rows.Add();
            m_dgvSession.Rows[indxRow].HeaderCell.Value = @"Принятых пакетов";

            // Папамеры источников для сохранения значений
            indxRow = m_dgvDestSetting.Rows.Add();
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Сервер";
            indxRow = m_dgvDestSetting.Rows.Add();
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Экземпляр";
            indxRow = m_dgvDestSetting.Rows.Add();
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Имя БД";
            indxRow = m_dgvDestSetting.Rows.Add();
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Пользователь";
            indxRow = m_dgvDestSetting.Rows.Add();
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Пароль";

            createHandlerQueue (typeof(HHandlerQueue));

            m_udpListener = new UDPListener();
            m_udpListener.EvtDataAskedHost += udpListener_OnEvtDataAskedHost;

            initFormMainSizing();
        }

        private void udpListener_OnEvtDataAskedHost(object obj)
        {
            m_handler.Push(m_udpListener, new object[] { obj });
        }

        private void udpListener_OnEventPackageRecieved(object obj)
        {
            string debugMsg = @"получен XML-пакет";

            Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
            Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_udpListener.Stop();
            m_handler.Stop();

            Close();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            m_handler.Start();
            m_udpListener.Start();
        }
        /// <summary>
        /// Обработчик события - Старт-Стоп для сессии (прием сообщений из UDP-канала)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void cbxSession_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbxMaster = (CheckBox)sender
                , cbxSlave = null;

            switch ((INDEX_CONTROL)((Control)sender).Tag) {
                case INDEX_CONTROL.CBX_READ_SESSION_START:
                    cbxSlave = m_cbxReadSessionStop;
                    break;
                case INDEX_CONTROL.CBX_READ_SESSION_STOP:
                    cbxSlave = m_cbxReadSessionStart;
                    break;
                default:
                    throw new Exception(@"FormMain::cbxSession_CheckedChanged () - неизвестный элемент управления...");
                    break;
            }

            cbxSlave.CheckedChanged -= cbxSession_CheckedChanged;
            cbxSlave.Checked = !cbxMaster.Checked;
            cbxSlave.CheckedChanged += new EventHandler(cbxSession_CheckedChanged);
        }
        /// <summary>
        /// Обработчик события - выбор п. меню "Отладака-Чтение-Пакет-Один"
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void имитацияпакетодинToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_udpListener.GenerateEventPackageRecieved();
        }
        /// <summary>
        /// Обработчик события - выбор п. меню "Отладака-Чтение-Пакет-Цикл"
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void имитацияпакетциклToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked == true)
                m_udpListener.StartSeriesEventPackageRecieved();
            else
                m_udpListener.StopSeriesEventPackageRecieved();
        }
    }
}
