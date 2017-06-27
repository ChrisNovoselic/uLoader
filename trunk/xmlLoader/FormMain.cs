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
        public enum INDEX_CONTROL { UNKNOWN = -1
            , CBX_READ_SESSION_START, CBX_READ_SESSION_STOP
            , DGV_PACKAGE_LIST, DGV_DATASET_LIST
            , TABCONTROL_VIEW_PACKAGE, TABCONTROL_DEST
            , TABPAGE_VIEW_PACKAGE_XML, TABPAGE_VIEW_PACKAGE_TREE, TABPAGE_VIEW_PACKAGE_TABLE_VALUE, TABPAGE_VIEW_PACKAGE_TABLE_PARAMETER
            , TABPAGE_VIEW_DATASET_TABLE_VALUE, TABPAGE_VIEW_DATASET_TABLE_PARAMETER
        }
        /// <summary>
        /// Набор признаков для логгированния тех или иных (LOGGING_ID) событий
        /// </summary>
        private HMark m_markLogging;

        public enum LOGGING_ID {
            PACKAGE_RECIEVED_EVENT
            , PACKAGE_RECIEVED_CONTENT
            , GROUP_PARAMETER_NOT_UPADTE            
                //, COUNT
            ,
        }

        //private static string[] s_arLoggingName
        //    //= {
        //    //    LOGGING_ID.GROUP_PARAMETER_NOT_UPADTE.ToString()
        //    //}
        //;

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

                topNode.Expand();
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

        private delegate DataGridViewRow DelegateStatisticIndexItemFunc (PackageHandlerQueue.STATISTIC.INDEX_ITEM tag);
        /// <summary>
        /// Обновить значения в представлении для отображения статистических данных
        /// </summary>
        private void updateDataGridViewStatistic()
        {
            DataGridView dgv = m_dgvStatistic;
            PackageHandlerQueue.STATISTIC.ITEM item;
            DataGridViewRow row;
            int iRow = -1;

            DelegateStatisticIndexItemFunc getRow = delegate (PackageHandlerQueue.STATISTIC.INDEX_ITEM tag) {
                var rows = from r in dgv.Rows.Cast<DataGridViewRow>() where (PackageHandlerQueue.STATISTIC.INDEX_ITEM)r.Tag == tag select r;

                if (rows.Count() > 0)
                    return rows.ElementAt(0);
                else
                    return null;
            };

            if (PackageHandlerQueue.s_Statistic.IsEmpty == false)
                foreach (PackageHandlerQueue.STATISTIC.INDEX_ITEM iItem in Enum.GetValues(typeof(PackageHandlerQueue.STATISTIC.INDEX_ITEM))) {
                    item = PackageHandlerQueue.s_Statistic.ElementAt(iItem);

                    if (item.visibled == true) {
                        row = getRow(iItem);

                        if (row == null) {
                            iRow = dgv.Rows.Add(new object[] { string.Empty });
                            dgv.Rows[iRow].Tag = iItem;
                            dgv.Rows[iRow].HeaderCell.Value = item.desc;
                            row = dgv.Rows[iRow];
                        }
                        else
                            ;

                        row.Cells[0].Value = (!(item.value == null)) ? item.value.ToString() : string.Empty;
                    } else
                        ;
                }
            else
                ;
        }
        /// <summary>
        /// Обновить значения в представлении с элементами пакетов/наборов
        /// </summary>
        /// <param name="dgv">Представление в котором требуется обновить значения</param>
        /// <param name="items">Список(актуальный) элементов для отображения (возможно, повторяет уже отображенные)</param>
        private void updateDataGridViewItem(DataGridView dgv, IEnumerable<VIEW_ITEM> items, EventHandler handler_SelectionChanged)
        {
            int indxRow = -1
                , cntViewPackageItem = -1;
            List<int> listItemIndexToAdding = new List<int>();
            DataGridViewRow rowAdding;

            try {
                if (!(items == null)) {
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
                            dgv.Rows[indxRow].Cells[(int)VIEW_ITEM.INDEX.DATETIME_COMPLTETED].Value = item.Values[(int)VIEW_ITEM.INDEX.DATETIME_COMPLTETED];
                        }
                    }
                    // отменить регистрацию обработчика события - чтобы при удалении 2-х и более строк
                    // , обработчик не вызывался бы столько раз
                    // , сколько будет удалено строк
                    if (dgv.RowCount + listItemIndexToAdding.Count > cntViewPackageItem)
                        dgv.SelectionChanged -= handler_SelectionChanged;
                    else
                        ;
                    // удалить, при необходимости, строки
                    while (dgv.RowCount + listItemIndexToAdding.Count > cntViewPackageItem) {
                    // требуется удаление строк
                        // сортировать строки с меткой меткой даты/времени получения XML-пакета от минимальной к максимальной
                        var rowsOrdering = from row in (dgv.Rows.Cast<DataGridViewRow>().ToList()) orderby (DateTime)row.Tag /*descending*/ select row;
                        if ((dgv.RowCount + listItemIndexToAdding.Count - cntViewPackageItem) == 1)
                        // перед крайним удалением воостанавливаем обработчик
                            dgv.SelectionChanged += handler_SelectionChanged;
                        else
                            ;
                        //  , затем удалить 1-ый элемент из списка
                        dgv.Rows.Remove(rowsOrdering.ElementAt(0));
                    }                    
                    // добавить строки в представление
                    foreach (int indx in listItemIndexToAdding) {
                        // создать строку (если строку добавить сразу, то значение Tag будет присвоено после события 'SelectionChanged' - нельхя определить идентификатор строки)
                        rowAdding = new DataGridViewRow();
                        rowAdding.CreateCells(dgv);
                        if (rowAdding.SetValues(items.ElementAt(indx).Values) == true) {
                            // установить идентификатор строки
                            rowAdding.Tag = items.ElementAt(indx).Values[(int)VIEW_ITEM.INDEX.DATETIME_RECIEVED];
                            // добавить строку
                            indxRow = dgv.Rows.Add(rowAdding);
                        } else
                            ;
                    }
                } else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"..."), Logging.INDEX_MESSAGE.NOT_SET);
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
        public FormMain() : base (@"IconMainxmlLoader")
        {
            int indxRow = -1;

            m_markLogging = new HMark(0);
            //s_arLoggingName = new string[Enum.GetValues(typeof(LOGGING_ID)).Length];
            //foreach (LOGGING_ID id in Enum.GetValues(typeof(LOGGING_ID)))
            //    s_arLoggingName[(int)id] = id.ToString();

            Logging.DelegateGetINIParametersOfID = getLogParametersOfID;
            //Logging.LinkId(Logging.INDEX_MESSAGE.D_001, 0);
            Logging.LinkId(Logging.INDEX_MESSAGE.D_002, (int)LOGGING_ID.PACKAGE_RECIEVED_EVENT);
            Logging.LinkId(Logging.INDEX_MESSAGE.D_003, (int)LOGGING_ID.PACKAGE_RECIEVED_CONTENT);
            Logging.LinkId(Logging.INDEX_MESSAGE.D_004, (int)LOGGING_ID.GROUP_PARAMETER_NOT_UPADTE);

            //this.Icon = this.Icon = ((System.Drawing.Icon)(new System.ComponentModel.ComponentResourceManager(typeof(FormMain)).GetObject("IconMainxmlLoader")));

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
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.AUTO_START;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"Авто-старт";
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.SERVER;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"Сервер";
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.NPORT;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"№ порт";
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.INSTANCE;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"Экземпляр";
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.DB_NAME;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"Имя БД";
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.UID;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"Пользователь";
            indxRow = m_dgvDestDetail.Rows.Add(); m_dgvDestDetail.Rows[indxRow].Tag = WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.PSWD;
            m_dgvDestDetail.Rows[indxRow].HeaderCell.Value = @"Пароль";

            createHandlerQueue (typeof(HHandlerQueue));
            (m_handler as HHandlerQueue).EvtToFormMain += new DelegateObjectFunc (onHandlerMainQueue);

            m_udpListener = new UDPListener();
            m_udpListener.EvtDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);
            evtUDPListenerDataAskedHost += new DelegateObjectFunc (udpListener_OnEvtDataAskedHost);

            m_handlerPackage = new PackageHandlerQueue();
            m_handlerPackage.EvtToFormMain += onHandlerPackageQueue;

            m_handlerWriter = new WriterHandlerQueue();
            m_handlerWriter.EvtToFormMain += onHandlerWriterQueue;

            m_timerUpdate = new System.Threading.Timer(timerUpdate_OnCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            m_dgvPackageList.AllowUserToResizeRows = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvPackageList.SelectionChanged += dgvPackageList_SelectionChanged;
            m_dgvPackageList.CellFormatting += dgvListItem_CellFormatting;
            m_dgvViewPackageTableValue.AllowUserToResizeColumns = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvViewPackageTableValue.AllowUserToResizeRows = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvViewPackageTableValue.DataSourceChanged += dgvViewDataTable_DataSourceChanged;
            //m_dgvViewPackageTableParameter.DataSourceChanged += dgvViewDataTable_DataSourceChanged;

            m_tabControlViewPackage.Tag = INDEX_CONTROL.TABCONTROL_VIEW_PACKAGE;
            m_tabControlViewPackage.Selecting += tabControl_Selecting;
            m_tabControlViewPackage.Selected += tabControl_Selected;
            m_tpageViewPackageXml.Tag = INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_XML;
            m_tpageViewPackageTree.Tag = INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TREE;
            m_tpageViewPackageTableValue.Tag = INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TABLE_VALUE;
            ((Control)m_tpageViewPackageTableValue).Enabled = true;

            m_dgvDestDatasetList.AllowUserToResizeRows = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestDatasetList.SelectionChanged += dgvDatasetList_SelectionChanged;
            m_dgvDestDatasetList.CellFormatting += dgvListItem_CellFormatting;
            m_dgvDestValue.AllowUserToResizeColumns = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestValue.AllowUserToResizeRows = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestValue.RowHeadersVisible = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestValue.DataSourceChanged += dgvViewDataTable_DataSourceChanged;
            m_dgvDestParameter.AllowUserToResizeColumns = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestParameter.AllowUserToResizeRows = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestParameter.AllowUserToAddRows = false; //??? 
            m_dgvDestParameter.RowHeadersVisible = false; //??? почему не установлено в 'InitializeComponent'
            m_dgvDestParameter.DataSourceChanged += dgvViewDataTable_DataSourceChanged;

            m_tabControlDest.Tag = INDEX_CONTROL.TABCONTROL_DEST;
            m_tabControlDest.Selecting += tabControl_Selecting;
            m_tabControlDest.Selected += tabControl_Selected;
            m_tpageDestValue.Tag = INDEX_CONTROL.TABPAGE_VIEW_DATASET_TABLE_VALUE;
            ((Control)m_tpageDestParameter).Enabled = true;
            m_tpageDestParameter.Tag = INDEX_CONTROL.TABPAGE_VIEW_DATASET_TABLE_PARAMETER;
        }

        private string getLogParametersOfID(int id)
        {
            return m_markLogging.IsMarked(id).ToString();
        }

        private void dgvListItem_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            DataGridViewCell cell;

            if (e.ColumnIndex == (int)VIEW_ITEM.INDEX.DATETIME_COMPLTETED) {
                cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (cell.Value is DateTime)
                    cell.Value = ((DateTime)cell.Value).Equals(DateTime.MinValue) == true ? @"Пропуск" :
                        ((DateTime)cell.Value).Equals(DateTime.MaxValue) == true ? @"Ошибка" :
                             cell.Value;
                else
                    ;
            } else
                ;
        }

        private void dgvViewDataTable_DataSourceChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn column in (sender as DataGridView).Columns)
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void dgvDestList_SelectionChanged(object sender, EventArgs e)
        {
            if (m_dgvDestList.SelectedRows.Count > 0)
                m_handler.Push(null, new object[] {
                    new object [] {
                        new object [] { HHandlerQueue.StatesMachine.DEST_DETAIL, (int)m_dgvDestList.SelectedRows[0].Tag }
                    }
                });
            else
                ;
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            INDEX_CONTROL indxTypeItemList = (INDEX_CONTROL)(sender as Control).Tag == INDEX_CONTROL.TABCONTROL_VIEW_PACKAGE ?
                INDEX_CONTROL.DGV_PACKAGE_LIST :
                    (INDEX_CONTROL)(sender as Control).Tag == INDEX_CONTROL.TABCONTROL_DEST ?
                        INDEX_CONTROL.DGV_DATASET_LIST : INDEX_CONTROL.UNKNOWN;

            if ((INDEX_CONTROL)e.TabPage.Tag == INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TREE)
                m_treeViewPackage.Nodes.Clear();
            else ;

            if (!(indxTypeItemList == INDEX_CONTROL.UNKNOWN))
            // отправить запрос на получение контента выбранного пакета
                pushItemContent(indxTypeItemList, (INDEX_CONTROL)e.TabPage.Tag);
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod()
                    , string.Format(@"не найден идентификатор для представления, соответствующий {0}...", ((INDEX_CONTROL)(sender as Control).Tag).ToString())
                    , Logging.INDEX_MESSAGE.NOT_SET);
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
            //??? зачем очищать, если TABPAGE не отображается (проверить?)
            if ((INDEX_CONTROL)m_tabControlViewPackage.SelectedTab.Tag == INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TREE)
                m_treeViewPackage.Nodes.Clear();
            else ;

            // отправить запрос на получение контента выбранного пакета
            pushItemContent(INDEX_CONTROL.DGV_PACKAGE_LIST, (INDEX_CONTROL)m_tabControlViewPackage.SelectedTab.Tag);
        }

        private void dgvDatasetList_SelectionChanged(object sender, EventArgs e)
        {
            // отправить запрос на получение контента выбранного пакета
            pushItemContent(INDEX_CONTROL.DGV_DATASET_LIST, (INDEX_CONTROL)m_tabControlDest.SelectedTab.Tag);
        }
        /// <summary>
        /// ??? Общий метод - отправить запрос на получение контента выбранного элемента
        /// </summary>
        /// <param name="indxItem">Индекс(таг) элемента управления</param>
        private void pushItemContent(INDEX_CONTROL indxItem, INDEX_CONTROL tagSelectedTab)
        {
            DataGridView dgv = null;
            IHHandlerQueue handlerQueue = null;
            int state = -1;

            switch(indxItem) {
                case INDEX_CONTROL.DGV_PACKAGE_LIST:
                    state = (int)PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT;
                    dgv = m_dgvPackageList;
                    handlerQueue = m_handlerPackage;
                    break;
                case INDEX_CONTROL.DGV_DATASET_LIST:
                    state = (int)WriterHandlerQueue.StatesMachine.DATASET_CONTENT;
                    dgv = m_dgvDestDatasetList;
                    handlerQueue = m_handlerWriter;
                    break;
                default:
                    break;
            }

            if ((!(dgv == null))
                && (!(handlerQueue == null))
                && (!(state < 0))) {
                //if (!(tabParent == null))
                    if (dgv.SelectedRows.Count == 1)
                        handlerQueue.Push(null, new object[] {
                            new object[] {
                                new object [] {
                                  state, dgv.SelectedRows[0].Tag, tagSelectedTab, dgv.SelectedRows[0].Index, dgv.SelectedRows[0].State
                                }
                            }
                        });
                    else
                        if (dgv.SelectedRows.Count > 1)
                            throw new Exception(string.Format(@"Строк выбрано {0} больше, чем указано в свойствах {1}...", dgv.SelectedRows.Count, indxItem.ToString()));
                        else
                            ;
                //else
                //    throw new Exception(string.Format(@"Среди владельцев объекта {0} нет элементов с требуемым типом...", indxItem.ToString()));
            } else
                throw new Exception(string.Format(@"Аргумент {0} указывает на несуществующий элемент управления...", indxItem.ToString()));
        }

        public struct VIEW_ITEM
        {
            public enum INDEX : int { COUNT_RECORD, DATETIME_RECIEVED, DATETIME_COMPLTETED }

            private static Type []_types = { typeof(int), typeof(DateTime), typeof(DateTime) };

            public object[] Values;
        }

        private void onHandlerPackageQueue(object obj)
        {
            PackageHandlerQueue.StatesMachine state = (PackageHandlerQueue.StatesMachine)(obj as object[])[0];

            // анонимный метод(функция) для выполнения операций с элементами интерфейса
            DelegateObjectFunc handlerPackageQueue = delegate (object arg)
            {
                object[] dataSet;

                switch (state) {
                    //case PackageHandlerQueue.StatesMachine.SETUP:
                    //    m_handler.Push(m_handlerPackage, new object[] {
                    //    });
                    //    break;
                    case PackageHandlerQueue.StatesMachine.LIST_PACKAGE:
                        updateDataGridViewItem (m_dgvPackageList, (IEnumerable<VIEW_ITEM>)(arg as object[])[1], dgvPackageList_SelectionChanged);
                        break;
                    case PackageHandlerQueue.StatesMachine.PACKAGE_CONTENT:
                        switch ((INDEX_CONTROL)m_tabControlViewPackage.SelectedTab.Tag) {
                            case INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_XML:
                                if (!((arg as object[])[1] == null))
                                    m_tbxViewPackage.Text = ((XmlDocument)(arg as object[])[1]).InnerXml;
                                else
                                    m_tbxViewPackage.Text = string.Empty;
                                break;
                            case INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TREE:
                                m_treeViewPackage.UpdateData((ListXmlTree)(arg as object[])[1]);
                                break;
                            case INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TABLE_VALUE:
                                m_dgvViewPackageTableValue.DataSource = (DataTable)(arg as object[])[1];
                                break;
                            //case INDEX_CONTROL.TABPAGE_VIEW_PACKAGE_TABLE_PARAMETER:
                            //    m_dgvViewPackageTableParameter.DataSource = (DataTable)(arg as object[])[1];
                            //    break;
                            default:
                                throw new Exception(@"Неизвестный тип панели для представления содержимого пакета");
                        }
                        break;
                    case PackageHandlerQueue.StatesMachine.STATISTIC:
                        updateDataGridViewStatistic();
                        break;
                    case PackageHandlerQueue.StatesMachine.TIMER_TABLERES:
                        if ((arg as object[]).Length == 2) {
                            if (((arg as object[])[1] is Array)) {
                                dataSet = (arg as object[])[1] as object[];

                                if (dataSet.Length == 3)
                                    m_handlerWriter.Push(null, new object[] {
                                        new object[] {
                                            new object[] { // формат DATA_SET
                                                WriterHandlerQueue.StatesMachine.NEW
                                                , (DateTime)dataSet[0]
                                                , (DataTable)dataSet[1]
                                                , (DataTable)dataSet[2]
                                            }
                                        }
                                    })
                                    ;
                                else
                                // не соблюдается формат DATA_SET
                                    ;
                            } else
                            // нет пакетов для сохранения
                                ; 
                        } else
                            Logging.Logg().Error(MethodBase.GetCurrentMethod()
                                , string.Format(@"некорректное кол-во аргументов state={0}", state)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                    case PackageHandlerQueue.StatesMachine.NEW: //??? не м.б. получен, иначе фиксировать ошибку
                    default: //??? неизвестный идентикатор, фиксировать ошибку
                        break;
                }
            };

            switch (state) {
                case PackageHandlerQueue.StatesMachine.MESSAGE_TO_STATUSSTRIP:
                    m_statusStripMain.Message((FormMain.StatusStrip.STATE)(obj as object[])[1], (string)(obj as object[])[2]);
                    break;
                default:
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
                    break;
            }
        }

        private void onHandlerWriterQueue(object obj)
        {
            WriterHandlerQueue.StatesMachine state = (WriterHandlerQueue.StatesMachine)(obj as object[])[0];

            // анонимный метод(функция) для выполнения операций с элементами интерфейса
            DelegateObjectFunc handlerDataSetQueue = delegate (object arg)
            {
                switch (state) {
                    case WriterHandlerQueue.StatesMachine.LIST_DATASET:
                        updateDataGridViewItem (m_dgvDestDatasetList, (IEnumerable<VIEW_ITEM>)(arg as object[])[1], dgvDatasetList_SelectionChanged);
                        break;
                    case WriterHandlerQueue.StatesMachine.DATASET_CONTENT:
                        //arg as object[])[1]
                        switch ((INDEX_CONTROL)m_tabControlDest.SelectedTab.Tag) {
                            case INDEX_CONTROL.TABPAGE_VIEW_DATASET_TABLE_VALUE:
                                m_dgvDestValue.DataSource = (DataTable)(arg as object[])[1];
                                break;
                            case INDEX_CONTROL.TABPAGE_VIEW_DATASET_TABLE_PARAMETER:
                                m_dgvDestParameter.DataSource = (DataTable)(arg as object[])[1];
                                break;
                            default:
                                throw new Exception(@"Неизвестный тип панели для представления содержимого пакета");
                        }
                        break;
                    case WriterHandlerQueue.StatesMachine.NEW: //??? не м.б. получен, иначе фиксировать ошибку
                    default: //??? неизвестный идентикатор, фиксировать ошибку
                        break;
                }
            };

            switch (state) {
                case WriterHandlerQueue.StatesMachine.MESSAGE_TO_STATUSSTRIP:
                    m_statusStripMain.Message((FormMain.StatusStrip.STATE)(obj as object[])[1], (string)(obj as object[])[2]);
                    break;
                default:
                    if (InvokeRequired == true)
                    // выполнить анонимный метод ы контексте формы
                        this.BeginInvoke(handlerDataSetQueue, obj);
                    else
                        handlerDataSetQueue(obj);
                    break;
            }
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
            Action<bool, bool> connectedChanged = delegate (bool bConnected, bool bDebugTurn) {
                if (bDebugTurn == true)
                    имитацияпакетодинToolStripMenuItem.Enabled =
                    имитацияпакетциклToolStripMenuItem.Enabled =
                        bConnected;
                else
                    сервисToolStripMenuItem.Visible = false;

                m_cbxReadSessionStart.Checked =
                    bConnected;
                m_cbxReadSessionStop.Checked =
                    !m_cbxReadSessionStart.Checked;

                if (bDebugTurn == true)
                    имитацияпакетциклToolStripMenuItemPerformClick();
                else
                    ;

                m_handlerPackage.Activate(m_cbxReadSessionStart.Checked);
            };

            Logging.Logg().Debug(MethodBase.GetCurrentMethod()
                , string.Format(@"получено сообщение state={0} от обработчика очереди событий", state.ToString())
                , Logging.INDEX_MESSAGE.D_002);
            
            switch (state) {
                case HHandlerQueue.StatesMachine.MESSAGE_TO_STATUSSTRIP:
                    m_statusStripMain.Message((FormMain.StatusStrip.STATE)(obj as object[])[1], (string)(obj as object[])[2]);
                    break;
                case HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGED:
                    // выполнить анонимный метод
                    Invoke(new Action<bool, bool> (connectedChanged), (obj as object[])[1], (obj as object[])[2]);
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
                case HHandlerQueue.StatesMachine.DEST_DETAIL:
                    BeginInvoke(new DelegateObjectFunc(fillDestDetail), (obj as object[])[1]);
                    break;
                case HHandlerQueue.StatesMachine.LOGGING_SET:
                    foreach (LOGGING_ID id in Enum.GetValues(typeof(LOGGING_ID)))                        
                        m_markLogging.Set((int)id, ((obj as object[])[1] as bool[])[(int)id]);

                    Logging.UpdateMarkDebugLog();
                    break;
                case HHandlerQueue.StatesMachine.OPTION_PACKAGE:
                    BeginInvoke(new DelegateObjectFunc(setOptionPackage), (obj as object[])[1]);

                    m_handlerPackage.Push(null, new object[] {
                        new object[] {
                            new object [] {
                                PackageHandlerQueue.StatesMachine.OPTION, (obj as object[])[1]
                            }
                        }
                    });
                    break;
                case HHandlerQueue.StatesMachine.OPTION_DEST:
                    BeginInvoke(new DelegateObjectFunc(setOptionDataSet), (obj as object[])[1]);

                    m_handlerWriter.Push(null, new object[] {
                        new object[] {
                            new object [] {
                                WriterHandlerQueue.StatesMachine.OPTION, (obj as object[])[1]
                            }
                        }
                    });
                    break;
                case HHandlerQueue.StatesMachine.TIMER_UPDATE:
                    // запустить таймер на обновление информации
                    m_timerUpdate.Change((SEC_TIMER_UPDATE = (ushort)(obj as object[])[1]) * 1000, System.Threading.Timeout.Infinite);
                    break;
                default:
                    break;
            }            
        }
        /// <summary>
        /// Установить значения настраиваемых параметров
        /// </summary>
        /// <param name="obj">Объект со значениями настраиваемых параметров</param>
        private void setOptionPackage(object obj)
        {
            PackageHandlerQueue.OPTION optionPackage = (PackageHandlerQueue.OPTION)obj;

            m_nudnPackageDisplayCount.Value = optionPackage.COUNT_VIEW_ITEM;
            m_nudnPackageDisplayCount.ValueChanged += nudnPackageDisplayCount_ValueChanged;

            m_nudnPackageHistoryAlong.Value = (int)optionPackage.TS_HISTORY_ALONG.TotalHours;
            m_nudnPackageHistoryAlong.ValueChanged += nudnPackageHistoryAlong_ValueChanged;
        }

        private void nudnPackageDisplayCount_ValueChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void nudnPackageHistoryAlong_ValueChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void setOptionDataSet(object obj)
        {
            WriterHandlerQueue.OPTION optionDataSet = (WriterHandlerQueue.OPTION)obj;

            m_nudnDestDisplayCount.Value = optionDataSet.COUNT_VIEW_ITEM;
            m_nudnDestDisplayCount.ValueChanged += nudnDestDisplayCount_ValueChanged;
        }

        private void nudnDestDisplayCount_ValueChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
        ///// <summary>
        ///// Список объектов с параметрами соединения с источниками данных
        /////  (хранение списка не обязательно, но при этом не требует постоянного опроса при выборе)
        ///// </summary>
        //private List<ConnectionSettings> _listDestConnSett;

        private void fillDestList(object obj)
        {
            DataGridViewRow rowAdding;
            bool bPressed = false;

            //??? почему не выполнено в 'InitializeComponent'
            ((from column in m_dgvDestList.Columns.Cast<DataGridViewColumn>() where column.GetType().Equals(typeof(DataGridViewPressedButtonColumn)) == true select column).ElementAt(0) as DataGridViewPressedButtonColumn).PressChanged +=
                dgvDestList_PressChanged;

            List<WriterHandlerQueue.ConnectionSettings> listDestConnSett = obj as List<WriterHandlerQueue.ConnectionSettings>;

            foreach(WriterHandlerQueue.ConnectionSettings conSett in listDestConnSett) {
                rowAdding = new DataGridViewRow();
                rowAdding.CreateCells(m_dgvDestList);
                if (rowAdding.SetValues(new object[] { conSett.name, @"->" }) == true) {
                    rowAdding.Tag = conSett.id;

                    m_dgvDestList.Rows.Add(rowAdding);

                    bPressed = (bool)conSett.Items[(int)WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.AUTO_START];

                    if (bPressed == true) {
                        (rowAdding.Cells[1] as DataGridViewPressedButtonCell).Pressed =
                            bPressed;
                        
                    } else
                        ;
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

        private void fillDestDetail(object obj)
        {
            WriterHandlerQueue.ConnectionSettings connSett = obj as WriterHandlerQueue.ConnectionSettings;

            foreach (DataGridViewRow row in m_dgvDestDetail.Rows)
                switch ((WriterHandlerQueue.ConnectionSettings.INDEX_ITEM)row.Tag) {
                    case WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.AUTO_START:
                        row.Cells[0].Value = connSett.Items[(int)WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.AUTO_START];
                        break;
                    case WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.SERVER:
                        row.Cells[0].Value = connSett.server;
                        break;
                    case WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.NPORT:
                        row.Cells[0].Value = connSett.port;
                        break;
                    case WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.DB_NAME:
                        row.Cells[0].Value = connSett.dbName;
                        break;
                    case WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.UID:
                        row.Cells[0].Value = connSett.userName;
                        break;
                    case WriterHandlerQueue.ConnectionSettings.INDEX_ITEM.PSWD:
                        row.Cells[0].Value = connSett.password;
                        break;
                    default:
                        break;
                }
        }

        private void dgvDestList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (((sender as DataGridView).Columns[e.ColumnIndex] is DataGridViewPressedButtonColumn)
                && (!(e.RowIndex < 0))) {
                ((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewPressedButtonCell).Pressed =
                    !((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewPressedButtonCell).Pressed;
            } else
                ;

            (sender as DataGridView).Rows[e.RowIndex].Selected = true;
        }

        private void dgvDestList_PressChanged(int param)
        {
            m_handlerWriter.Push(null, new object[] {
                new object[] {
                    new object[] { WriterHandlerQueue.StatesMachine.CONNSET_USE_CHANGED , (int)m_dgvDestList.Rows[param].Tag }
                }
            });
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

            m_statusStripMain.Start();

            // запуск, активация основного обработчика очереди событий
            m_handler.Start(); m_handler.Activate(true);
            // запуск, активация обработчика очереди событий при записи значений в БД
            m_handlerWriter.Start(); m_handlerWriter.Activate(true);
            // запуск, активация обработчика очереди событий разбора пакетов
            m_handlerPackage.Start(); //m_handlerPackage.Activate(true);
            // запуск объекта прослушивателя XML-пакетов (НЕ опрос)
            m_udpListener.Start();

            // запросить список параметров соединения с БД
            m_handler.Push(null, new object[] {
                new object[] {
                    new object[] { HHandlerQueue.StatesMachine.LOGGING_SET }
                    , new object[] { HHandlerQueue.StatesMachine.OPTION_PACKAGE }
                    , new object[] { HHandlerQueue.StatesMachine.OPTION_DEST }
                    , new object[] { HHandlerQueue.StatesMachine.LIST_DEST }
                    , new object[] { HHandlerQueue.StatesMachine.TIMER_UPDATE }
                }
            });

            // запросить(команда) изменение состояния
            if ((m_handler as HHandlerQueue).AutoStart == true)
                evtUDPListenerDataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE, !m_cbxReadSessionStart.Checked } });
            else
                ;

            //??? почемы вызов не в базовом классе
            initFormMainSizing();
        }

        protected override void FormMain_Closing(object sender, FormClosingEventArgs e)
        {
            m_statusStripMain.Stop();

            base.FormMain_Closing(sender, e);
        }
        /// <summary>
        /// Обработчик события - Старт-Стоп для сессии (прием сообщений из UDP-канала)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - п. меню</param>
        /// <param name="e">Аргумент события</param>
        private void cbxSession_Click(object sender, EventArgs e)
        {
            bool bStartChecked = m_cbxReadSessionStart.Checked;

            //if (bStartChecked == имитацияпакетциклToolStripMenuItem.Checked) {
            //// команда на в(от)ключение - проверить в(ы)ключен ли отладочный цикл генерации серии пакетов
            //// отладочная серия одинаковое состояние с 'UDPListener.connect' - в(от)ключить
            //    имитацияпакетциклToolStripMenuItemPerformClick();
            //} else
            //    ;

            // запросить(команда) изменение состояния
            evtUDPListenerDataAskedHost(new object[] { new object[] { HHandlerQueue.StatesMachine.UDP_CONNECTED_CHANGE, !bStartChecked } });
        }

        private void имитацияпакетциклToolStripMenuItemPerformClick()
        {
            bool bPrevMenuItemEnabled = имитацияпакетциклToolStripMenuItem.Enabled;

            if (bPrevMenuItemEnabled == false)
                имитацияпакетциклToolStripMenuItem.Enabled = true;
            else
                ;

            имитацияпакетциклToolStripMenuItem.PerformClick();

            if (!(имитацияпакетциклToolStripMenuItem.Enabled = bPrevMenuItemEnabled))
                имитацияпакетциклToolStripMenuItem.Enabled = bPrevMenuItemEnabled;
            else
                ;
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

        private int IdCurrentConnSett { get { return m_dgvDestList.SelectedRows.Count > 0 ? (int)m_dgvDestList.SelectedRows[0].Tag : -1; } }

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
                    new object[] { WriterHandlerQueue.StatesMachine.LIST_DATASET, IdCurrentConnSett }
                    //, new object[] { WriterHandlerQueue.StatesMachine.STATISTIC } //??? Статистика не размещена (недостаток места)
                }
            });
        }
    }
}
