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
    public partial class FormMain : uLoaderCommon.FormMainBase//, IDataHost
    {
        /// <summary>
        /// Перечисление - идентификаторы(Tag) некоторых элементов интерфейса
        /// </summary>
        private enum INDEX_CONTROL { CBX_READ_SESSION_START, CBX_READ_SESSION_STOP }
        /// <summary>
        /// Объект для приема пакетов по UDP
        /// </summary>
        private UDPListener m_udpListener;
        ///// <summary>
        ///// Событие, инициирующее постановку события в очередь (обработки событий)
        ///// </summary>
        //public event DelegateObjectFunc EvtDataAskedHost;
        /// <summary>
        /// Объект - обработчик очереди событий
        /// </summary>
        private HHandlerQueue Handler { get { return m_handler as HHandlerQueue; } }
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public FormMain() : base ()
        {
            int indxRow = -1;

            InitializeComponent();
            // указать идентификаторы некорым элементам интерфейса
            m_cbxReadSessionStart.Tag = INDEX_CONTROL.CBX_READ_SESSION_START;
            m_cbxReadSessionStop.Tag = INDEX_CONTROL.CBX_READ_SESSION_STOP;
            // инициализировать строками статические представления - Сессия
            indxRow = m_dgvSession.Rows.Add();
            m_dgvSession.Rows[indxRow].HeaderCell.Value = @"Крайний пакет";
            indxRow = m_dgvSession.Rows.Add();
            m_dgvSession.Rows[indxRow].HeaderCell.Value = @"Длительность сессии";
            indxRow = m_dgvSession.Rows.Add();
            m_dgvSession.Rows[indxRow].HeaderCell.Value = @"Принятых пакетов";
            // инициализировать строками статические представления - Параметры источников для сохранения значений
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
            Handler.EvtToFormMain += new DelegateObjectFunc (onHandlerQueue);

            m_udpListener = new UDPListener();
            m_udpListener.EvtDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);
            evtUDPListenerDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);

            initFormMainSizing();
        }

        private void onHandlerQueue(object obj)
        {
            //имитацияпакетодинToolStripMenuItem.Enabled =
            //имитацияпакетциклToolStripMenuItem.Enabled =
            //    bConnected;
        }

        private event DelegateObjectFunc evtUDPListenerDataAskedHost;

        private void udpListener_OnEvtDataAskedHost(object obj)
        {
            m_handler.Push(m_udpListener, new object[] { obj });
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_udpListener.Stop();

            Close();
        }

        protected override void FormMain_Load(object sender, EventArgs e)
        {
            base.FormMain_Load(sender, e);

            m_handler.Start(); m_handler.Activate(true);
            m_udpListener.Start();
        }
        /// <summary>
        /// Обработчик события - Старт-Стоп для сессии (прием сообщений из UDP-канала)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void cbxSession_Click(object sender, EventArgs e)
        {
            // запросить изменение состояния
            evtUDPListenerDataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE, !m_cbxReadSessionStart.Checked } });
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
        ///// <summary>
        ///// Инициировать событие для очереди обработки событий
        /////  (поставить событие в очередь для обработки)
        ///// </summary>
        ///// <param name="par"></param>
        //public void DataAskedHost(object par)
        //{
        //    EvtDataAskedHost?.Invoke(par);
        //}
        ///// <summary>
        ///// Обработчик события - получено сообщение от очереди обработки событий
        ///// </summary>
        ///// <param name="res">Параметры сообщения</param>
        //public void OnEvtDataRecievedHost(object res)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
