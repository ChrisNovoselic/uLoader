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
using System.Xml;

namespace xmlLoader
{
    public partial class FormMain : uLoaderCommon.FormMainBase//, IDataHost
    {
        private static ushort SEC_TIMER_UPDATE = 1;
        /// <summary>
        /// Перечисление - идентификаторы(Tag) некоторых элементов интерфейса
        /// </summary>
        private enum INDEX_CONTROL { CBX_READ_SESSION_START, CBX_READ_SESSION_STOP }

        private class DataGridViewStatistic : DataGridView
        {
            private DataGridViewRow getRow(PackageHandlerQueue.STATISTIC.INDEX_ITEM tag)
            {
                var rows = from row in Rows.Cast<DataGridViewRow>() where (PackageHandlerQueue.STATISTIC.INDEX_ITEM)row.Tag == tag select row;

                if (rows.Count() > 0)
                    return rows.ElementAt(0);
                else
                    return null;
            }

            public void UpdateData()
            {
                PackageHandlerQueue.STATISTIC.ITEM item;
                DataGridViewRow row;
                int iRow = -1;

                if (PackageHandlerQueue.s_Statistic.IsEmpty == false)
                    foreach (PackageHandlerQueue.STATISTIC.INDEX_ITEM iItem in Enum.GetValues(typeof(PackageHandlerQueue.STATISTIC.INDEX_ITEM))) {
                        item = PackageHandlerQueue.s_Statistic.ElementAt(iItem);

                        if (item.visibled == true) {
                            row = getRow(iItem);

                            if (row == null) {
                                iRow = Rows.Add(new object[] { string.Empty });
                                Rows[iRow].Tag = iItem;
                                Rows[iRow].HeaderCell.Value = item.desc;
                                row = Rows[iRow];
                            } else
                                ;

                            row.Cells[0].Value = (!(item.value == null)) ? item.value.ToString() : string.Empty;
                        } else
                            ;
                    }
                else
                    ;
            }
        }
        /// <summary>
        /// Класс для представления списка полученных пакетов
        /// </summary>
        private class DataGridViewPackageList : DataGridView
        {
            /// <summary>
            /// Обновить значения в представлении
            /// </summary>
            /// <param name="items">Список объектов с информацией о пакетах</param>
            public void UpdateData(IEnumerable<VIEW_PACKAGE_ITEM>items)
            {
                int indxRow = -1
                    , cntViewPackageItem = -1;
                List<int> listItemIndxToAdding = new List<int>();
                DataGridViewRow rowAdding;
                // кол-во пакетов указано в 'PackageHandlerQueue.COUNT_VIEW_PACKAGE_ITEM'                
                cntViewPackageItem = items.Count();
                // поиск строк для добавления в текущее представление
                foreach (VIEW_PACKAGE_ITEM item in items) {
                    indxRow = -1;
                    // по всем строкам поиск пакета
                    foreach (DataGridViewRow row in Rows)
                        if (row.Tag.Equals(item.Values[(int)VIEW_PACKAGE_ITEM.INDEX.DATETIME_RECIEVED]) == true) {
                            indxRow = Rows.IndexOf(row);
                            // найдено соответствие пакет - строка
                            break;
                        } else
                            ;
                    // проверить найден ли пакет
                    if (indxRow < 0) {
                    // пакет не был найден - для добавления
                        listItemIndxToAdding.Add(items.ToList().IndexOf(item));
                    } else
                        ;
                }

                while (RowCount + listItemIndxToAdding.Count > cntViewPackageItem) {
                    // требуется удаление строк
                    // сортировать строки с меткой меткой даты/времени получения XML-пакета от минимальной к максимальной
                    var rowsOrdering = from row in (Rows.Cast<DataGridViewRow>().ToList()) orderby (DateTime)row.Tag /*descending*/ select row;
                    //  , затем удалить 1-ый элемент из списка
                    Rows.Remove(rowsOrdering.ElementAt(0));
                }
                // добавить строки в представление
                foreach (int indx in listItemIndxToAdding) {
                    // создать строку (если строку добавить сразу, то значение Tag будет присвоено после события 'SelectionChanged' - нельхя определить идентификатор строки)
                    rowAdding = new DataGridViewRow();
                    rowAdding.CreateCells(this);
                    if (rowAdding.SetValues(items.ElementAt(indx).Values) == true) {
                        // установить идентификатор строки
                        rowAdding.Tag = items.ElementAt(indx).Values[(int)VIEW_PACKAGE_ITEM.INDEX.DATETIME_RECIEVED];
                        // добавить строку
                        indxRow = Rows.Add(rowAdding);
                    } else
                        ;
                }
            }
        }
        /// <summary>
        /// Объект для приема пакетов по UDP
        /// </summary>
        private UDPListener m_udpListener;
        /// <summary>
        /// Объект для обработки пакетов
        /// </summary>
        private PackageHandlerQueue m_handlerPackage;
        /// <summary>
        /// Объект для записи значений в целевую БД
        /// </summary>
        private WriterHandlerQueue m_handlerWriter;
        /// <summary>
        /// Таймер для отправления запросов на получение информации для обновления данных на форме
        /// </summary>
        private System.Threading.Timer m_timerUpdate;
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
            //// инициализировать строками статические представления - Сессия
            //indxRow = m_dgvStatistic.Rows.Add();
            //m_dgvStatistic.Rows[indxRow].HeaderCell.Value = @"Крайний пакет";
            //indxRow = m_dgvStatistic.Rows.Add();
            //m_dgvStatistic.Rows[indxRow].HeaderCell.Value = @"Длительность сессии";
            //indxRow = m_dgvStatistic.Rows.Add();
            //m_dgvStatistic.Rows[indxRow].HeaderCell.Value = @"Принятых пакетов";
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
            (m_handler as HHandlerQueue).EvtToFormMain += new DelegateObjectFunc (onHandlerMainQueue);

            m_udpListener = new UDPListener();
            m_udpListener.EvtDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);
            evtUDPListenerDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);

            m_handlerPackage = new PackageHandlerQueue();
            m_handlerPackage.EvtToFormMain += onHandlerPackageQueue;

            m_handlerWriter = new WriterHandlerQueue();
            //m_handlerWriter.EvtToFormMain += onHandlerPackageQueue;

            //??? почемы вызов не в базовом классе
            initFormMainSizing();

            m_timerUpdate = new System.Threading.Timer(timerUpdate_OnCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            m_dgvPackageList.SelectionChanged += dgvPackageList_SelectionChanged;
                        
            m_tabControlViewPackage.Selecting += tabControlViewPackage_Selecting;
            m_tabControlViewPackage.Selected += tabControlViewPackage_Selected;
            m_tpageViewPackageXml.Tag = INDEX_TABPAGE_VIEW_PACKAGE.XML;
            m_tpageViewPackageTree.Tag = INDEX_TABPAGE_VIEW_PACKAGE.TREE;
        }

        public enum INDEX_TABPAGE_VIEW_PACKAGE { XML, TREE }

        private void tabControlViewPackage_Selected(object sender, TabControlEventArgs e)
        {
            // отправить запрос на получение контента выбранного пакета
            pushPackageContent();
        }
        /// <summary>
        /// Обработчик события - перед изменением активной вкладки
        ///  (для блокировки вкладки - временно)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        private void tabControlViewPackage_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // отменить активацию вкладки
            e.Cancel = ((Control)sender).Enabled;
        }
        /// <summary>
        /// Обработчик события - изменение выбранной строки в представленни со списком полученных пакетов
        /// </summary>
        /// <param name="sender">бъект, инициировавший событие(представление)</param>
        /// <param name="e">Аргумент события</param>
        private void dgvPackageList_SelectionChanged(object sender, EventArgs e)
        {
            // отправить запрос на получение контента выбранного пакета
            pushPackageContent();
        }
        /// <summary>
        /// Отправить запрос на получение контента выбранного пакета
        /// </summary>
        private void pushPackageContent()
        {
            m_handlerPackage.Push(null, new object[] {
                new object[] {
                    new object [] {
                        PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT, m_dgvPackageList.SelectedRows[0].Tag, m_tabControlViewPackage.SelectedTab.Tag
                    }
                }
            });
        }

        public struct VIEW_PACKAGE_ITEM
        {
            public enum INDEX : int { COUNT_RECORD, DATETIME_RECIEVED, DATETIME_SENDED }

            private static Type []_types = { typeof(int), typeof(DateTime), typeof(DateTime) };

            public object[] Values;
        }

        private void onHandlerPackageQueue(object obj)
        {
            // анонимный метод(функция) для выполнения операций с элементами интерфейса
            DelegateObjectFunc handlerPackageQueue = delegate(object arg) {
                PackageHandlerQueue.StatesMachine state = (PackageHandlerQueue.StatesMachine)(arg as object[])[0];

                switch (state) {
                    case PackageHandlerQueue.StatesMachine.LIST_PACKAGE:
                        m_dgvPackageList.UpdateData((IEnumerable<VIEW_PACKAGE_ITEM>)(arg as object[])[1]);
                        break;
                    case PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT:
                        //m_tabControlViewPackage.Update();
                        switch ((INDEX_TABPAGE_VIEW_PACKAGE)m_tabControlViewPackage.SelectedTab.Tag) {
                            case INDEX_TABPAGE_VIEW_PACKAGE.XML:
                                m_tbxViewPackage.Text = ((XmlDocument)(arg as object[])[1]).InnerXml;
                                break;
                            case INDEX_TABPAGE_VIEW_PACKAGE.TREE:
                                break;
                            default:
                                throw new Exception(@"НЕизвестный тип панели для представления содержимого пакета");
                        }
                        break;
                    case PackageHandlerQueue.StatesMachine.STATISTIC:
                        m_dgvStatistic.UpdateData();
                        break;
                    case PackageHandlerQueue.StatesMachine.NEW: //??? не м.б. получен, иначе фиксировать ошибку
                    default: //??? неизвестный идентикатор, фиксировать ошибку
                        break;
                }
            };
            // выполнить анонимный метод ы контексте формы
            Invoke(handlerPackageQueue, obj);
        }
        /// <summary>
        /// Обработчик события - сообщение от обработчика очереди событий
        ///  (обратная связь с обработчиком очереди событий)
        /// </summary>
        /// <param name="obj">Объект - параметр(аргумент) события</param>
        private void onHandlerMainQueue(object obj)
        {
            HHandlerQueue.StatesMachine state = (HHandlerQueue.StatesMachine)(obj as object[])[0];

            // анонимная функция для выполнения в контексте формы
            DelegateBoolFunc connectedChanged = delegate (bool bConnected) {
                имитацияпакетодинToolStripMenuItem.Enabled =
                имитацияпакетциклToolStripMenuItem.Enabled =
                m_cbxReadSessionStart.Checked =
                    bConnected;
                m_cbxReadSessionStop.Checked =
                    !m_cbxReadSessionStart.Checked;
            };

            Logging.Logg().Debug(MethodBase.GetCurrentMethod()
                , string.Format(@"получено сообщение state={0} от обработчика очереди событий", state.ToString())
                , Logging.INDEX_MESSAGE.NOT_SET);
            
            switch (state) {
                case HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGED:
                    // выполнить анонимный метод
                    Invoke(new DelegateBoolFunc (connectedChanged), (bool)(obj as object[])[1]);
                    break;
                case HHandlerQueue.StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED:
                    m_handlerPackage.Push(null, new object [] {
                        new object[] {
                            new object [] {
                                PackageHandlerQueue.StatesMachine.NEW, (obj as object[])[1], (obj as object[])[2]
                            }
                        }
                    });
                    break;
                default:
                    break;
            }            
        }
        /// <summary>
        /// Событие для постановки в очередь обработки событий сообщение для объекта-прослушивателя пакетов
        /// </summary>
        private event DelegateObjectFunc evtUDPListenerDataAskedHost;
        /// <summary>
        /// Обработчик события - получение сообщения от объекта-прослушивателя пакетов
        /// </summary>
        /// <param name="obj">Объект-массив с аргументами сообзения</param>
        private void udpListener_OnEvtDataAskedHost(object obj)
        {
            // поставить в очередь обработки событий
            m_handler.Push(m_udpListener, new object[] { obj });
        }
        /// <summary>
        /// Обработчик события - выбор п. главного меню Выход
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент события</param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // останов объектов в обратном (см. старт) порядке
            m_timerUpdate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            m_timerUpdate.Dispose();

            m_udpListener.Stop();
            m_handlerPackage.Activate(false); m_handlerPackage.Stop();
            m_handlerWriter.Activate(false); m_handlerWriter.Stop();

            m_handler.Activate(false); m_handler.Stop();

            Close();
        }
        /// <summary>
        /// Обработчик события - загрузка главной формы завершена
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (this)</param>
        /// <param name="e">Аргумент события</param>
        protected override void FormMain_Load(object sender, EventArgs e)
        {
            base.FormMain_Load(sender, e);
            // запуск, активация основного обработчика очереди событий
            m_handler.Start(); m_handler.Activate(true);
            // запуск, активация обработчика очереди событий при записи значений в БД
            m_handlerWriter.Start(); m_handlerWriter.Activate(true);
            // запуск, активация обработчика очереди событий разбора пакетов
            m_handlerPackage.Start(); m_handlerPackage.Activate(true);
            // запуск объекта прослушивателя XML-пакетов (НЕ опрос)
            m_udpListener.Start();
            // запустить таймер на обновление информации
            m_timerUpdate.Change(SEC_TIMER_UPDATE * 1000, System.Threading.Timeout.Infinite);

            // запросить(команда) изменение состояния
            if ((m_handler as HHandlerQueue).AutoStart == true)
                evtUDPListenerDataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE, !m_cbxReadSessionStart.Checked } });
            else
                ;
        }
        /// <summary>
        /// Обработчик события - Старт-Стоп для сессии (прием сообщений из UDP-канала)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void cbxSession_Click(object sender, EventArgs e)
        {
            // запросить(команда) изменение состояния
            evtUDPListenerDataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE, !m_cbxReadSessionStart.Checked } });
        }
        /// <summary>
        /// Обработчик события - выбор п. меню "Отладака-Чтение-Пакет-Один"
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void имитацияпакетодинToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_udpListener.DebugGenerateEventPackageRecieved();
        }
        /// <summary>
        /// Обработчик события - выбор п. меню "Отладака-Чтение-Пакет-Цикл"
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void имитацияпакетциклToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked == true)
                m_udpListener.DebugStartSeriesEventPackageRecieved();
            else
                m_udpListener.DebugStopSeriesEventPackageRecieved();
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

        private void timerUpdate_OnCallback(object obj)
        {
            m_timerUpdate.Change(SEC_TIMER_UPDATE * 1000, System.Threading.Timeout.Infinite);

            m_handlerPackage.Push(null, new object[] {
                new object[] {
                    new object[] { PackageHandlerQueue.StatesMachine.LIST_PACKAGE }
                    , new object[] { PackageHandlerQueue.StatesMachine.STATISTIC }
                }
            });

            m_handlerPackage.Push(null, new object[] {
                new object[] {
                    new object[] { WriterHandlerQueue.StatesMachine.LIST_DATASET }
                    , new object[] { WriterHandlerQueue.StatesMachine.STATISTIC }
                }
            });
        }
    }
}
