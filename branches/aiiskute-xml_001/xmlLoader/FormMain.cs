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
using System.Windows.Forms.VisualStyles;
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

        public class ListXmlTree : List<object>
        {
            public string Tag;

            public List<KeyValuePair<string, string>> Attributes;
        }

        private class TreeViewPackage : TreeView
        {
            public void UpdateData(ListXmlTree listXmlTree)
            {
                TreeNode topNode = Nodes.Add(listXmlTree.Tag);

                foreach (ListXmlTree itemXmlTree in listXmlTree)
                    addNodes(itemXmlTree, topNode.Nodes);
            }

            private void addNodes(ListXmlTree listXmlTree, TreeNodeCollection nodes)
            {
                TreeNode newNode;

                for (int i = 0; i < listXmlTree.Count; i++) {
                    newNode = nodes.Add((listXmlTree[i] as ListXmlTree).Tag);

                    addNodes(listXmlTree[i] as ListXmlTree, newNode.Nodes);
                }
            }
        }

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

        private void updateDataGridViewItem(DataGridView dgv, IEnumerable<VIEW_ITEM> items)
        {
            int indxRow = -1
                    , cntViewPackageItem = -1;
                List<int> listItemIndexToAdding = new List<int>();
                DataGridViewRow rowAdding;
                // кол-во пакетов указано в 'PackageHandlerQueue.COUNT_VIEW_PACKAGE_ITEM' 
                cntViewPackageItem = items.Count();
                // поиск строк для добавления в текущее представление
                foreach (VIEW_ITEM item in items) {
                    indxRow = -1;
                    // по всем строкам поиск пакета
                    foreach (DataGridViewRow row in dgv.Rows)
                        if (row.Tag.Equals(item.Values[(int)VIEW_ITEM.INDEX.DATETIME_RECIEVED]) == true) {
                            indxRow = dgv.Rows.IndexOf(row);
                            // найдено соответствие пакет - строка
                            break;
                        } else
                            ;
                    // проверить найден ли пакет
                    if (indxRow < 0)
                        // пакет не был найден - для добавления
                        listItemIndexToAdding.Add(items.ToList().IndexOf(item));
                    else {
                        dgv.Rows[indxRow].Cells[(int)VIEW_ITEM.INDEX.DATETIME_COMPLTETED].Value =
                            item.Values[(int)VIEW_ITEM.INDEX.DATETIME_COMPLTETED];
                    }
                }

                while (dgv.RowCount + listItemIndexToAdding.Count > cntViewPackageItem) {
                    // требуется удаление строк
                    // сортировать строки с меткой меткой даты/времени получения XML-пакета от минимальной к максимальной
                    var rowsOrdering = from row in (dgv.Rows.Cast<DataGridViewRow>().ToList()) orderby (DateTime)row.Tag /*descending*/ select row;
                    //  , затем удалить 1-ый элемент из списка
                    dgv.Rows.Remove(rowsOrdering.ElementAt(0));
                }
                // добавить строки в представление
                foreach (int indx in listItemIndexToAdding) {
                    // создать строку (если строку добавить сразу, то значение Tag будет присвоено после события 'SelectionChanged' - нельхя определить идентификатор строки)
                    rowAdding = new DataGridViewRow();
                    rowAdding.CreateCells(m_dgvDestDatabaseListAction);
                    if (rowAdding.SetValues(items.ElementAt(indx).Values) == true) {
                        // установить идентификатор строки
                        rowAdding.Tag = items.ElementAt(indx).Values[(int)VIEW_ITEM.INDEX.DATETIME_RECIEVED];
                        // добавить строку
                        indxRow = dgv.Rows.Add(rowAdding);
                    } else
                        ;
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
            indxRow = m_dgvDestSetting.Rows.Add(); m_dgvDestSetting.Rows[indxRow].Tag = FileINI.INDEX_CONNECTION_SETTING.IP;
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Сервер";
            indxRow = m_dgvDestSetting.Rows.Add(); m_dgvDestSetting.Rows[indxRow].Tag = FileINI.INDEX_CONNECTION_SETTING.NPORT;
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"№ порт";
            indxRow = m_dgvDestSetting.Rows.Add(); m_dgvDestSetting.Rows[indxRow].Tag = FileINI.INDEX_CONNECTION_SETTING.INSTANCE;
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Экземпляр";
            indxRow = m_dgvDestSetting.Rows.Add(); m_dgvDestSetting.Rows[indxRow].Tag = FileINI.INDEX_CONNECTION_SETTING.DB_NAME;
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Имя БД";
            indxRow = m_dgvDestSetting.Rows.Add(); m_dgvDestSetting.Rows[indxRow].Tag = FileINI.INDEX_CONNECTION_SETTING.UID;
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Пользователь";
            indxRow = m_dgvDestSetting.Rows.Add(); m_dgvDestSetting.Rows[indxRow].Tag = FileINI.INDEX_CONNECTION_SETTING.PSWD;
            m_dgvDestSetting.Rows[indxRow].HeaderCell.Value = @"Пароль";

            createHandlerQueue (typeof(HHandlerQueue));
            (m_handler as HHandlerQueue).EvtToFormMain += new DelegateObjectFunc (onHandlerMainQueue);

            m_udpListener = new UDPListener();
            m_udpListener.EvtDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);
            evtUDPListenerDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);

            m_handlerPackage = new PackageHandlerQueue();
            m_handlerPackage.EvtToFormMain += onHandlerPackageQueue;

            m_handlerWriter = new WriterHandlerQueue();
            m_handlerWriter.EvtToFormMain += onHandlerWriterQueue;

            //??? почемы вызов не в базовом классе
            initFormMainSizing();

            m_timerUpdate = new System.Threading.Timer(timerUpdate_OnCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            m_dgvPackageList.SelectionChanged += dgvPackageList_SelectionChanged;            

            m_tabControlViewPackage.Selecting += tabControl_Selecting;
            m_tabControlViewPackage.Selected += tabControlViewPackage_Selected;
            m_tpageViewPackageXml.Tag = INDEX_TABPAGE_VIEW_PACKAGE.XML;
            m_tpageViewPackageTree.Tag = INDEX_TABPAGE_VIEW_PACKAGE.TREE;
            m_tpageViewPackageTableValue.Tag = INDEX_TABPAGE_VIEW_PACKAGE.TABLE_VALUE;
        }

        private void dgvDestList_SelectionChanged(object sender, EventArgs e)
        {
            int indxDest = -1;

            if (m_dgvDestList.SelectedRows.Count > 0) {
                indxDest = m_dgvDestList.SelectedRows[0].Index;

                foreach (DataGridViewRow row in m_dgvDestSetting.Rows)
                    switch ((FileINI.INDEX_CONNECTION_SETTING)row.Tag) {
                        case FileINI.INDEX_CONNECTION_SETTING.IP:
                            row.Cells[0].Value = _listDestConnSett[indxDest].server;
                            break;
                        case FileINI.INDEX_CONNECTION_SETTING.NPORT:
                            row.Cells[0].Value = _listDestConnSett[indxDest].port;
                            break;
                        case FileINI.INDEX_CONNECTION_SETTING.DB_NAME:
                            row.Cells[0].Value = _listDestConnSett[indxDest].dbName;
                            break;
                        case FileINI.INDEX_CONNECTION_SETTING.UID:
                            row.Cells[0].Value = _listDestConnSett[indxDest].userName;
                            break;
                        case FileINI.INDEX_CONNECTION_SETTING.PSWD:
                            row.Cells[0].Value = _listDestConnSett[indxDest].password;
                            break;
                        default:
                            break;
                    }
            } else
                ;            
        }

        public enum INDEX_TABPAGE_VIEW_PACKAGE { XML, TREE, TABLE_VALUE, TABLE_PARAMETER }

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
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // отменить активацию вкладки
            e.Cancel = !((Control)e.TabPage).Enabled;
        }
        /// <summary>
        /// Обработчик события - изменение выбранной строки в представленни со списком полученных пакетов
        /// </summary>
        /// <param name="sender">бъект, инициировавший событие(представление)</param>
        /// <param name="e">Аргумент события</param>
        private void dgvPackageList_SelectionChanged(object sender, EventArgs e)
        {
            m_treeViewPackage.Nodes.Clear();

            // отправить запрос на получение контента выбранного пакета
            pushPackageContent();
        }
        /// <summary>
        /// Отправить запрос на получение контента выбранного пакета
        /// </summary>
        private void pushPackageContent()
        {
            if (m_dgvPackageList.SelectedRows.Count == 1)
                m_handlerPackage.Push(null, new object[] {
                    new object[] {
                        new object [] {
                            PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT, m_dgvPackageList.SelectedRows[0].Tag, m_tabControlViewPackage.SelectedTab.Tag
                        }
                    }
                });
            else
                if (m_dgvPackageList.SelectedRows.Count > 1)
                    throw new Exception(@"Строк выбрано больше, чем указано в свойствах...");
                else
                    ;
        }

        public struct VIEW_ITEM
        {
            public enum INDEX : int { COUNT_RECORD, DATETIME_RECIEVED, DATETIME_COMPLTETED }

            private static Type []_types = { typeof(int), typeof(DateTime), typeof(DateTime) };

            public object[] Values;
        }

        //private void handlerPackageQueue (object arg) {
        //    PackageHandlerQueue.StatesMachine state = (PackageHandlerQueue.StatesMachine)(arg as object[])[0];

        //    switch (state) {
        //        case PackageHandlerQueue.StatesMachine.LIST_PACKAGE:
        //            m_dgvPackageList.UpdateData((IEnumerable<VIEW_PACKAGE_ITEM>)(arg as object[])[1]);
        //            break;
        //        case PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT:
        //            //m_tabControlViewPackage.Update();
        //            switch ((INDEX_TABPAGE_VIEW_PACKAGE)m_tabControlViewPackage.SelectedTab.Tag) {
        //                case INDEX_TABPAGE_VIEW_PACKAGE.XML:
        //                    m_tbxViewPackage.Text = ((XmlDocument)(arg as object[])[1]).InnerXml;
        //                    break;
        //                case INDEX_TABPAGE_VIEW_PACKAGE.TREE:
        //                    m_treeViewPackage.UpdateData((ListXmlTree)(arg as object[])[1]);
        //                    break;
        //                default:
        //                    throw new Exception(@"Неизвестный тип панели для представления содержимого пакета");
        //            }
        //            break;
        //        case PackageHandlerQueue.StatesMachine.STATISTIC:
        //            m_dgvStatistic.UpdateData();
        //            break;
        //        case PackageHandlerQueue.StatesMachine.TIMER_TABLERES:
        //            if ((arg as object[]).Length == 4)
        //                //m_handlerWriter.Push(null, new object[] {
        //                //    new object[] {
        //                //        new object[] {
        //                //            WriterHandlerQueue.StatesMachine.DATASET_CONTENT
        //                //            , (DateTime)(arg as object[])[1]
        //                //            , (DataTable)(arg as object[])[2]
        //                //            , (DataTable)(arg as object[])[3]
        //                //        }
        //                //    }
        //                //})
        //                ;
        //            else
        //                Logging.Logg().Warning(MethodBase.GetCurrentMethod()
        //                    , string.Format(@"некорректное кол-во аргументов state={0}", state)
        //                    , Logging.INDEX_MESSAGE.NOT_SET);
        //            break;
        //        case PackageHandlerQueue.StatesMachine.NEW: //??? не м.б. получен, иначе фиксировать ошибку
        //        default: //??? неизвестный идентикатор, фиксировать ошибку
        //            break;
        //    }
        //}

        private void onHandlerPackageQueue(object obj)
        {
            // анонимный метод(функция) для выполнения операций с элементами интерфейса
            DelegateObjectFunc handlerPackageQueue = delegate (object arg)
            {
                PackageHandlerQueue.StatesMachine state = (PackageHandlerQueue.StatesMachine)(arg as object[])[0];
                object[] dataSet;

                switch (state) {
                    case PackageHandlerQueue.StatesMachine.LIST_PACKAGE:
                        updateDataGridViewItem (m_dgvPackageList, (IEnumerable<VIEW_ITEM>)(arg as object[])[1]);
                        break;
                    case PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT:
                        //m_tabControlViewPackage.Update();
                        switch ((INDEX_TABPAGE_VIEW_PACKAGE)m_tabControlViewPackage.SelectedTab.Tag)
                        {
                            case INDEX_TABPAGE_VIEW_PACKAGE.XML:
                                m_tbxViewPackage.Text = ((XmlDocument)(arg as object[])[1]).InnerXml;
                                break;
                            case INDEX_TABPAGE_VIEW_PACKAGE.TREE:
                                m_treeViewPackage.UpdateData((ListXmlTree)(arg as object[])[1]);
                                break;
                            default:
                                throw new Exception(@"Неизвестный тип панели для представления содержимого пакета");
                        }
                        break;
                    case PackageHandlerQueue.StatesMachine.STATISTIC:
                        m_dgvStatistic.UpdateData();
                        break;
                    case PackageHandlerQueue.StatesMachine.TIMER_TABLERES:
                        if (((arg as object[]).Length == 2)
                            && ((arg as object[])[1] is Array)) {
                            dataSet = (arg as object[])[1] as object[];

                            if (dataSet.Length == 3)
                                m_handlerWriter.Push(null, new object[] {
                                    new object[] {
                                        new object[] {
                                            WriterHandlerQueue.StatesMachine.NEW
                                            , (DateTime)dataSet[0]
                                            , (DataTable)dataSet[1]
                                            , (DataTable)dataSet[2]
                                        }
                                    }
                                })
                                ;
                        } else
                            Logging.Logg().Warning(MethodBase.GetCurrentMethod()
                                , string.Format(@"некорректное кол-во аргументов state={0}", state)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                    case PackageHandlerQueue.StatesMachine.NEW: //??? не м.б. получен, иначе фиксировать ошибку
                    default: //??? неизвестный идентикатор, фиксировать ошибку
                        break;
                }
            };

            if (InvokeRequired == true)
                // выполнить анонимный метод ы контексте формы
                this.BeginInvoke(
                        //new DelegateObjectFunc (
                        handlerPackageQueue
                    //)
                    , obj)
                ;
            else
                handlerPackageQueue(obj);
        }

        private void onHandlerWriterQueue(object obj)
        {
            // анонимный метод(функция) для выполнения операций с элементами интерфейса
            DelegateObjectFunc handlerDataSetQueue = delegate (object arg)
            {
                WriterHandlerQueue.StatesMachine state = (WriterHandlerQueue.StatesMachine)(arg as object[])[0];

                switch (state) {
                    case WriterHandlerQueue.StatesMachine.LIST_DATASET:
                        updateDataGridViewItem (m_dgvDestDatabaseListAction, (IEnumerable<VIEW_ITEM>)(arg as object[])[1]);
                        break;                    
                    case WriterHandlerQueue.StatesMachine.NEW: //??? не м.б. получен, иначе фиксировать ошибку
                    default: //??? неизвестный идентикатор, фиксировать ошибку
                        break;
                }
            };

            if (InvokeRequired == true)
                // выполнить анонимный метод ы контексте формы
                this.BeginInvoke(handlerDataSetQueue, obj);
            else
                handlerDataSetQueue(obj);
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
                case HHandlerQueue.StatesMachine.LIST_DEST:
                    BeginInvoke(new DelegateObjectFunc(fillDestList), (obj as object[])[1]);
                    // запуск, активация обработчика очереди событий при записи значений в БД                    
                    m_handlerWriter.Push(null, new object [] {
                        new object[] {
                            new object [] {
                                WriterHandlerQueue.StatesMachine.LIST_DEST, (obj as object[])[1]
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

        private List<ConnectionSettings> _listDestConnSett;

        private void fillDestList(object obj)
        {
            DataGridViewRow rowAdding;

            _listDestConnSett = obj as List<ConnectionSettings>;

            foreach(ConnectionSettings conSett in _listDestConnSett) {
                rowAdding = new DataGridViewRow();
                rowAdding.CreateCells(m_dgvDestList);
                if (rowAdding.SetValues(new object[] { conSett.name, @"->" }) == true) {
                    rowAdding.Tag = conSett.id;

                    m_dgvDestList.Rows.Add(rowAdding);
                } else
                    ;
            }

            m_dgvDestList.SelectionChanged += dgvDestList_SelectionChanged;
            m_dgvDestList.CellContentClick += dgvDestList_CellContentClick;

            if (m_dgvDestList.RowCount > 0)
                m_dgvDestList.Rows[0].Selected = true;
            else
                ;
        }

        private void dgvDestList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (((sender as DataGridView).Columns[e.ColumnIndex] is DataGridViewPressedButtonColumn)
                && (!(e.RowIndex < 0))) {
                ((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewPressedButtonCell).Pressed =
                    !((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewPressedButtonCell).Pressed;
            } else
                ;
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
            Text = (m_handler as HHandlerQueue).FormMainText;

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

            // запросить список параметров соединения с БД
            m_handler.Push(null, new object[] {
                new object[] {
                    new object[] { HHandlerQueue.StatesMachine.LIST_DEST }
                }
            });
        }
        /// <summary>
        /// Обработчик события - Старт-Стоп для сессии (прием сообщений из UDP-канала)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void cbxSession_Click(object sender, EventArgs e)
        {
            bool bStartChecked = m_cbxReadSessionStart.Checked;

            if (bStartChecked == true)
                // команда на отключение - проверить включен ли отладочный цикл генерации серии пакетов
                if (имитацияпакетциклToolStripMenuItem.Checked == true)
                // серия также в работе - отключить
                    имитацияпакетциклToolStripMenuItem.PerformClick();
                else
                    ;
            else
                ;

            // запросить(команда) изменение состояния
            evtUDPListenerDataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE, !bStartChecked } });
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

            m_handlerWriter.Push(null, new object[] {
                new object[] {
                    new object[] { WriterHandlerQueue.StatesMachine.LIST_DATASET }
                    //, new object[] { WriterHandlerQueue.StatesMachine.STATISTIC } //??? Статистика не размещена (недостаток места)
                }
            });
        }
    }

    public class DataGridViewPressedButtonCell : DataGridViewButtonCell
    {
        private bool pressedValue;

        public bool Pressed
        {
            get { return pressedValue; }

            set { pressedValue = value; Value = value == true ? @"<-" : @"->"; }
        }

        // Override the Clone method so that the Enabled property is copied.
        public override object Clone()
        {
            DataGridViewPressedButtonCell cell = (DataGridViewPressedButtonCell)base.Clone();

            cell.Pressed = this.Pressed;

            return cell;
        }

        // By default, enable the button cell.
        public DataGridViewPressedButtonCell()
        {
            this.pressedValue = false;
        }

        protected override void Paint(Graphics graphics,
                                    Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
                                    DataGridViewElementStates elementState, object value,
                                    object formattedValue, string errorText,
                                    DataGridViewCellStyle cellStyle,
                                    DataGridViewAdvancedBorderStyle advancedBorderStyle,
                                    DataGridViewPaintParts paintParts)
        {
            // The button cell is disabled, so paint the border,  
            // background, and disabled button for the cell.
            if (this.pressedValue)
            {
                // Draw the cell background, if specified.
                if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
                {
                    SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);

                    graphics.FillRectangle(cellBackground, cellBounds);

                    cellBackground.Dispose();
                }

                // Draw the cell borders, if specified.
                if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
                {
                    PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
                }
                else
                    ;

                // Calculate the area in which to draw the button.
                Rectangle buttonArea = cellBounds;
                Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);

                buttonArea.X += buttonAdjustment.X;
                buttonArea.Y += buttonAdjustment.Y;

                buttonArea.Height -= buttonAdjustment.Height;
                buttonArea.Width -= buttonAdjustment.Width;

                // Draw the disabled button.                
                ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Pressed);

                // Draw the disabled button text. 
                if (this.FormattedValue is String)
                {
                    TextRenderer.DrawText(graphics, (string)this.FormattedValue, this.DataGridView.Font, buttonArea, SystemColors.ControlText);
                }
                else
                    ;
            }
            else
            {
                // The button cell is enabled, so let the base class 
                // handle the painting.
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            }
        }

        //protected override void OnClick(DataGridViewCellEventArgs e)
        //{
        //    if (Enabled == true)
        //        base.OnClick(e);
        //    else
        //        ;
        //}
    }

    public class DataGridViewPressedButtonColumn : DataGridViewButtonColumn
    {
        public DataGridViewPressedButtonColumn()
        {
            this.CellTemplate = new DataGridViewPressedButtonCell();
        }
    }
}
