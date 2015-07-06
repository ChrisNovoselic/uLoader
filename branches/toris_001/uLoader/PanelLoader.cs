using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using System.Windows.Forms;
using System.Data;

using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
    /// <summary>
    /// Перечисление - ключи событий для передачи событий "родительской" панели
    /// </summary>
    public enum KEY_EVENT { SELECTION_CHANGED, CELL_CLICK }

    public partial class PanelWork
    {
        private abstract partial class PanelLoader : PanelCommonDataHost
        {
            /// <summary>
            /// Перечисление - индексы ПРЕДподготавливаемых параметров
            /// </summary>
            public enum INDEX_PREPARE_PARS { OBJ, KEY_OBJ, KEY_EVT, ID_OBJ_SEL, DEPENDENCED_DATA, COUNT_INDEX_PREPARE_PARS }

            /// <summary>
            /// Перечисление - ключи элементов управления
            /// </summary>
            public enum KEY_CONTROLS { UNKNOWN = -1
                                        , DGV_GROUP_SOURCES, LABEL_DLLNAME_GROUPSOURCES, BUTTON_DLLNAME_GROUPSOURCES, CBX_SOURCE_OF_GROUP, TBX_GROUPSOURCES_ADDING
                                        , DGV_GROUP_SIGNALS
                                        , GROUP_BOX_GROUP_SIGNALS
                                        , RBUTTON_CUR_DATETIME, TBX_CUR_PERIOD, TBX_CUR_INTERVAL
                                        , RBUTTON_COSTUMIZE, CALENDAR_COSTUMIZE, TBX_COSTUMIZE_START_TIME, TBX_COSTUMIZE_PERIOD, TBX_COSTUMIZE_INTERVAL
                                        //, TBX_GROUPSIGNALS_ADDING
                                        , DGV_SIGNALS_OF_GROUP
                                        , COUNT_KEY_CONTROLS
                                        ,};

            public PanelLoader()
                : base (4, 1)
            {
                InitializeComponent();
            }

            public PanelLoader(IContainer container) : base (4, 1)
            {
                container.Add(this);

                InitializeComponent();
            }
        }

        private abstract partial class PanelLoader
        {
            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();

                //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                Control ctrl = null;
                HPanelCommon panelColumns;

                this.SuspendLayout ();

                //Панель источников данных
                panelColumns = new PanelCommonULoader(5, 8);
                this.Controls.Add(panelColumns, 0, 0);
                this.SetColumnSpan(panelColumns, 1); this.SetRowSpan(panelColumns, 1);
                //Группы источников
                ctrl = new DataGridView ();
                ctrl.Name = KEY_CONTROLS.DGV_GROUP_SOURCES.ToString ();                
                (ctrl as DataGridView).Columns.AddRange (
                    new DataGridViewColumn [] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewDisableButtonColumn ()
                    }
                );
                (ctrl as DataGridView).AllowUserToOrderColumns= false;
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                (ctrl as DataGridView).RowHeadersVisible = false;
                (ctrl as DataGridView).Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[0].HeaderText = @"Группы источников";
                (ctrl as DataGridView).Columns[1].Width = 37;
                (ctrl as DataGridView).Columns[1].HeaderText = @"Вкл./выкл.";
                //(ctrl as DataGridView).Columns[1].ReadOnly = true;
                //(ctrl as DataGridView).ReadOnly = true;
                (ctrl as DataGridView).RowsAdded += new DataGridViewRowsAddedEventHandler(panelLoader_WorkItemRowsAdded);
                (ctrl as DataGridView).CellClick += new DataGridViewCellEventHandler(panelLoader_WorkItemCellClick);
                (ctrl as DataGridView).SelectionChanged += new EventHandler(panelLoader_WorkItemSelectionChanged);
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 0);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 4);
                //Библиотека для загрузки
                ctrl = new Label();
                ctrl.Name = KEY_CONTROLS.LABEL_DLLNAME_GROUPSOURCES.ToString();
                (ctrl as Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                (ctrl as Label).FlatStyle = FlatStyle.Standard;
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 4);
                panelColumns.SetColumnSpan(ctrl, 4); panelColumns.SetRowSpan(ctrl, 1);
                //Кнопка для выгрузки/загрузки библиотеки
                ctrl = new Button();
                ctrl.Name = KEY_CONTROLS.BUTTON_DLLNAME_GROUPSOURCES.ToString();                
                (ctrl as Button).Text = @"<->";
                ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 4, 4);
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, 1);
                //Выбор текущего источника
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_SOURCE_OF_GROUP.ToString();
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                ctrl.Enabled = false;
                (ctrl as ComboBox).SelectedValueChanged += new EventHandler(panelLoader_SourceOfGroupSelectedValueChanged);
                ctrl.Dock = DockStyle.Bottom;
                panelColumns.Controls.Add(ctrl, 0, 5);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 1);
                //ГроупБокс для дополнительных параметров
                ctrl = new GroupBox();
                (ctrl as GroupBox).Text = @"Дополнительные параметры";
                //ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 6);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 2);
                //TextBox для редактирования дополнительных параметров
                ctrl.Controls.Add(new TextBox());
                ctrl.Controls[0].Name = KEY_CONTROLS.TBX_GROUPSOURCES_ADDING.ToString();
                (ctrl.Controls[0] as TextBox).Multiline = true;
                ctrl.Controls[0].Enabled = false;
                (ctrl.Controls[0] as TextBox).ScrollBars = ScrollBars.Both;
                ctrl.Controls[0].Leave += new EventHandler(panelLoader_GroupSourcesAddingLeave);
                ctrl.Controls[0].Dock = DockStyle.Fill;

                //Панель опроса
                panelColumns = new PanelCommonULoader(1, 16);
                this.Controls.Add(panelColumns, 1, 0);
                this.SetColumnSpan(panelColumns, 1); this.SetRowSpan(panelColumns, 1);
                //Группы сигналов
                ctrl = new DataGridView();
                ctrl.Name = KEY_CONTROLS.DGV_GROUP_SIGNALS.ToString ();                
                (ctrl as DataGridView).Columns.AddRange(
                    new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewDisableButtonColumn ()
                    }
                );
                (ctrl as DataGridView).AllowUserToOrderColumns = false;
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                (ctrl as DataGridView).RowHeadersVisible = false;
                (ctrl as DataGridView).Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[0].HeaderText = @"Группы сигналов";
                (ctrl as DataGridView).Columns[1].Width = 37;
                (ctrl as DataGridView).Columns[1].HeaderText = @"Вкл./выкл.";
                //(ctrl as DataGridView).Columns[1].ReadOnly = true;
                //(ctrl as DataGridView).ReadOnly = true;
                (ctrl as DataGridView).RowsAdded += new DataGridViewRowsAddedEventHandler(panelLoader_WorkItemRowsAdded);
                (ctrl as DataGridView).CellClick += new DataGridViewCellEventHandler(panelLoader_WorkItemCellClick);
                (ctrl as DataGridView).SelectionChanged += new EventHandler(panelLoader_WorkItemSelectionChanged);
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 0);
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, 4);
                //ГроупБокс... (в "своем" классе)

                //Панель рез-ов опроса
                panelColumns = new PanelCommonULoader(10, 8);
                this.Controls.Add(panelColumns, 2, 0);
                this.SetColumnSpan(panelColumns, 2); this.SetRowSpan(panelColumns, 1);
                ctrl = new DataGridView();
                ctrl.Name = KEY_CONTROLS.DGV_SIGNALS_OF_GROUP.ToString();
                (ctrl as DataGridView).Columns.AddRange(
                    new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                    }
                );
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).RowHeadersVisible = false;
                foreach (DataGridViewColumn col in (ctrl as DataGridView).Columns)
                    if ((ctrl as DataGridView).Columns.IndexOf(col) < (ctrl as DataGridView).Columns.Count - 1)
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    else
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[0].HeaderCell.Value = @"Сигнал";
                (ctrl as DataGridView).Columns[1].HeaderCell.Value = @"Кр./знач.";
                (ctrl as DataGridView).Columns[2].HeaderCell.Value = @"Дата/время кр./знач.";
                (ctrl as DataGridView).Columns[3].HeaderCell.Value = @"Кол-во";
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 0);
                panelColumns.SetColumnSpan(ctrl, 10); panelColumns.SetRowSpan(ctrl, 8);

                this.ResumeLayout (false);
                this.PerformLayout ();
            }

            #endregion

            private void panelLoader_WorkItemRowsAdded (object obj, EventArgs ev)
            {
                int cnt = (obj as DataGridView).Rows.Count;
                (obj as DataGridView).Rows[cnt - 1].Cells [1].Value = @"?";
                if ((obj as DataGridView).Rows[cnt - 1].Cells [1].GetType () == typeof (DataGridViewDisableButtonCell))
                    ((DataGridViewDisableButtonCell)(obj as DataGridView).Rows[cnt - 1].Cells [1]).Enabled = false;
                else
                    ;
            }            
            
            /// <summary>
            /// Найти целочисленный идентфикатор элемента управления
            /// </summary>
            /// <param name="ctrl">Элемент управления</param>
            /// <returns></returns>
            KEY_CONTROLS getKeyWorkingItem (Control ctrl)
            {
                KEY_CONTROLS keyRes = KEY_CONTROLS.UNKNOWN;
                
                //Цикл по всем известным целочисленным идентификаторам
                for (KEY_CONTROLS key = 0; key < KEY_CONTROLS.COUNT_KEY_CONTROLS; key++)
                {
                    //Проверить совпадение переменной цикла и идентификатора элемента управления
                    if (key.ToString().Trim().Equals((ctrl as Control).Name) == true)
                    {
                        //Запомнить идентификатор
                        keyRes = key;
                        //Прервать цикл
                        break;
                    }
                    else
                        ;
                }

                return keyRes;
            }            
            
            /// <summary>
            /// Подготовить параметры для передачи "родительской" панели
            ///  для последующего на их основе формирования события
            ///  ретрансляции и постановки в очередь обработки событий
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="indxRow">индекс "выделенной" строки объекта, инициировавшего событие</param>
            /// <returns>Массив параметров</returns>
            private object[] getPreparePars(DataGridView obj, int indxRow)
            {
                //0 - Ключ объекта
                //1 - Индекс "выделенной" строки в объекте с группами источников
                //2 - Индекс "выделенной" строки в объекте с группами сигналов (-1 , если объект-инициатор группа источников)
                object[] arObjRes = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];
                arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] = KEY_CONTROLS.COUNT_KEY_CONTROLS;
                arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] =
                arObjRes[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] =
                    -1;

                //Найти целочисленный идентфикатор элемента управления
                arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] = getKeyWorkingItem (obj);
                //Проверить рез-т поиска целочисленного идентфикатора
                if ((KEY_CONTROLS)arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] == KEY_CONTROLS.COUNT_KEY_CONTROLS)
                    throw new Exception(@"PanelLoader::panelLoader_WorkItemClick () - не найден ключ [" + (obj as Control).Name + @"] для элемента управления...");
                else
                    ;

                if ((KEY_CONTROLS)arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] == KEY_CONTROLS.DGV_GROUP_SOURCES)                    
                    arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] =
                        //(obj as DataGridView).Rows[indxRow].Cells[0].Value
                        m_dictGroupIds[KEY_CONTROLS.DGV_GROUP_SOURCES][indxRow]
                        ;
                    //Для группы сигналов индекс оставить значение по умолчанию (-1)...
                else
                    if ((KEY_CONTROLS)arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] == KEY_CONTROLS.DGV_GROUP_SIGNALS)
                    {
                        //Если есть "выбранная" группа сигналов (ее состояние пользователь "изменяет")
                        // , значит обязательно есть и группа источников (которой принадлежит "выбранная" группа сигналов)
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] =
                            //(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView).SelectedRows[0].Cells[0].Value
                            m_dictGroupIds[KEY_CONTROLS.DGV_GROUP_SOURCES][(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView).SelectedRows[0].Index]
                            ;
                        arObjRes[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] =
                            //(obj as DataGridView).Rows[indxRow].Cells[0].Value
                            m_dictGroupIds[KEY_CONTROLS.DGV_GROUP_SIGNALS][indxRow]
                            ;
                    }
                    else
                        throw new Exception(@"PanelLoader::panelLoader_WorkItemClick () - найденный ключ [" + (obj as Control).Name + @"] не м.б. использован в этой операции...");

                return arObjRes;
            }
            /// <summary>
            /// Обработчик события "нажатие кнопкой по ячеке объекта"
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент для сопровождения события</param>
            private void panelLoader_WorkItemCellClick(object obj, DataGridViewCellEventArgs ev)
            {
                //int indx...
                //Проверить индекс ячейки (кнопки только в 1-ом столбце)
                if (ev.ColumnIndex == 1)
                {
                    //Проверить способность ячейки изменять свое состояние
                    if (((obj as DataGridView).Rows[ev.RowIndex].Cells[ev.ColumnIndex].GetType () == typeof (DataGridViewDisableButtonCell))
                        // , если - да, то проверить "включенное" состояние
                        && (((obj as DataGridView).Rows[ev.RowIndex].Cells[ev.ColumnIndex] as DataGridViewDisableButtonCell).Enabled == true))
                    {
                        //Подготовить параметры для передачи "родительской" панели
                        object[] arPreparePars = getPreparePars(obj as DataGridView, ev.RowIndex);

                        arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                        arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.CELL_CLICK;

                        //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                        DataAskedHost(arPreparePars);
                    }
                    else
                        ;
                }
                else
                    ;
            }
            /// <summary>
            /// Обработчик события "изменение выбора"
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="ev"></param>
            private void panelLoader_WorkItemSelectionChanged(object obj, EventArgs ev)
            {
                //Проверить наличие возможности выбора строки
                if ((obj as DataGridView).SelectedRows.Count > 0)
                {
                    //Подготовить параметры для передачи "родительской" панели
                    object[] arPreparePars = getPreparePars(obj as DataGridView, (obj as DataGridView).SelectedRows[0].Index);

                    switch ((KEY_CONTROLS)arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ])
                    {
                        case KEY_CONTROLS.DGV_GROUP_SOURCES:
                            clearValues();
                            break;
                        case KEY_CONTROLS.DGV_GROUP_SIGNALS:
                            clearValues(KEY_CONTROLS.DGV_SIGNALS_OF_GROUP);
                            //if (this is PanelLoaderDest)
                            //    clearValues(KEY_CONTROLS.TBX_GROUPSIGNALS_ADDING);
                            //else ;
                            break;
                        default:
                            break;
                    }

                    arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                    arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                    //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                    DataAskedHost(arPreparePars);
                }
                else
                    ; //Нет выбранных строк
            }

            private void panelLoader_SourceOfGroupSelectedValueChanged(object obj, EventArgs ev)
            {
                //Подготовить параметры для передачи "родительской" панели
                object[] arPreparePars = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ] = KEY_CONTROLS.CBX_SOURCE_OF_GROUP;
                arPreparePars[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = m_dictGroupIds[KEY_CONTROLS.DGV_GROUP_SOURCES][(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView).SelectedRows[0].Index];
                arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = m_dictGroupIds[KEY_CONTROLS.CBX_SOURCE_OF_GROUP][(obj as ComboBox).SelectedIndex];

                arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                DataAskedHost(arPreparePars);
            }

            private void panelLoader_GroupSourcesAddingLeave(object obj, EventArgs ev)
            {
                //Подготовить параметры для передачи "родительской" панели
                object[] arPreparePars = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                string strDepencededData = string.Empty;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ] = getKeyWorkingItem (obj as Control);
                arPreparePars[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = m_dictGroupIds[KEY_CONTROLS.DGV_GROUP_SOURCES][(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView).SelectedRows[0].Index];
                foreach (string line in (obj as TextBox).Lines)
                    strDepencededData += line + FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL];
                if (strDepencededData.Length > (FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL].ToString().Length + 1))
                    strDepencededData = strDepencededData.Substring (0, strDepencededData.Length - FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL].ToString().Length);
                else
                    strDepencededData = string.Empty;
                arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = strDepencededData;

                arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                DataAskedHost(arPreparePars);
            }

            /// <summary>
            /// Словарь идентификаторв, отображаемых элементов
            /// </summary>
            private Dictionary<KEY_CONTROLS, string[]> m_dictGroupIds;
            /// <summary>
            /// Заполнить рабочий элемент - список групп источников, сигналов
            /// </summary>
            /// <param name="key"></param>
            /// <param name="rows"></param>
            public void FillWorkItem (KEY_CONTROLS key, string [,] rows)
            {
                if (m_dictGroupIds == null)
                    m_dictGroupIds = new Dictionary<KEY_CONTROLS,string[]> ();
                else
                    ;

                int cnt = rows.GetLength (1)
                    , j = -1;

                if (m_dictGroupIds.Keys.Contains (key) == false)
                    m_dictGroupIds.Add (key, new string [cnt]);
                else
                    if (! (m_dictGroupIds[key].Length == cnt))
                        m_dictGroupIds[key] = new string[cnt];
                    else
                        ;

                DataGridView workItem = GetWorkingItem(key) as DataGridView;
                if (!(rows == null))
                {
                    j = 0;
                    for (j = 0; j < cnt; j ++)
                    {
                        m_dictGroupIds[key][j] = rows[0, j];
                        workItem.Rows.Add(new object[] { rows[1, j] });
                    }
                }
                else
                    ;
            }
            /// <summary>
            /// Заполнить рабочий элемент - список источников 
            /// </summary>
            /// <param name="grpSrc">Объект с данными для заполнения</param>
            public void FillWorkItem(GROUP_SRC grpSrc)
            {
                Control workItem;
                PanelLoader.KEY_CONTROLS key;
                //Наименование библиотеки для загрузки данных
                key = PanelLoader.KEY_CONTROLS.LABEL_DLLNAME_GROUPSOURCES;
                workItem = GetWorkingItem(key);
                (workItem as Label).Text = grpSrc.m_strDLLName;
                //Список групп сигналов - инициирует заполнение списка сигналов группы
                key = PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS;
                int cnt = grpSrc.m_listGroupSignalsPars.Count
                    , j = -1;
                if (m_dictGroupIds.Keys.Contains (key) == false)
                    m_dictGroupIds.Add (key, new string [cnt]);
                else
                    m_dictGroupIds[key] = new string[cnt];
                workItem = GetWorkingItem(key);
                for (j = 0; j < cnt; j ++)
                {
                    m_dictGroupIds[key][j] = grpSrc.m_listGroupSignalsPars[j].m_strId;
                    (workItem as DataGridView).Rows.Add(new object[] { grpSrc.m_listGroupSignalsPars [j].m_strShrName });
                }
                //Список источников группы источников                
                key = PanelLoader.KEY_CONTROLS.CBX_SOURCE_OF_GROUP;
                workItem = GetWorkingItem(key);
                if (m_dictGroupIds.ContainsKey(key) == false)
                    m_dictGroupIds.Add(key, new string[grpSrc.m_dictConnSett.Count]);
                else
                    m_dictGroupIds[key] = new string[grpSrc.m_dictConnSett.Count];
                j = 0;
                foreach (KeyValuePair <string, ConnectionSettings> pair in grpSrc.m_dictConnSett)
                {
                    (workItem as ComboBox).Items.Add(pair.Value.name);
                    m_dictGroupIds[key][j ++] = pair.Key;
                }
                //Выбрать текущий источник
                if ((workItem as ComboBox).Items.Count > 0)
                    (workItem as ComboBox).SelectedIndex = uLoader.FormMain.FileINI.GetIDIndex(grpSrc.m_IDCurrentConnSett);
                else
                    ;

                //Дополнительные параметры
                //Список источников группы источников
                if (grpSrc.m_dictAdding.Count > 0)
                {
                    key = PanelLoader.KEY_CONTROLS.TBX_GROUPSOURCES_ADDING;
                    workItem = GetWorkingItem(key);
                    foreach (KeyValuePair <string, string> pair in grpSrc.m_dictAdding)
                        workItem.Text += pair.Key + FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE] + pair.Value + Environment.NewLine;
                    workItem.Text = workItem.Text.Substring (0, workItem.Text.Length - Environment.NewLine.Length);
                }
                else
                    ;
            }
            /// <summary>
            /// Заполнить рабочий элемент - список сигналов группы
            /// </summary>
            /// <param name="grpSrs"></param>
            public virtual void FillWorkItem(GROUP_SIGNALS_SRC grpSrc)
            {
                Control workItem;
                PanelLoader.KEY_CONTROLS key;

                //Список сигналов группы
                key = PanelLoader.KEY_CONTROLS.DGV_SIGNALS_OF_GROUP;
                workItem = GetWorkingItem(key);

                if (!(grpSrc.m_listSgnls == null))
                    foreach (SIGNAL_SRC sgnl in grpSrc.m_listSgnls)
                        (workItem as DataGridView).Rows.Add(new object[] { sgnl.m_arSPars[grpSrc.m_listSKeys.IndexOf(@"NAME_SHR")] });
                else
                    // группа сигналов == null
                    Logging.Logg().Warning(@"PanelLoader::FillWorkItem () - список сигналов == null...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Заполнить рабочий элемент - список групп 
            ///  + параметры режима опроса
            /// </summary>
            /// <param name="grpSrc">Объект с данными для заполнения</param>
            public virtual void FillWorkItem(GROUP_SIGNALS_PARS grpSgnlsPars)
            {
                Control workItem;
                PanelLoader.KEY_CONTROLS key;

                if (!(grpSgnlsPars == null))
                {
                    //Отобразить активный режим
                    // только, если панель - источник
                    if (this is PanelLoaderSource)
                    {
                        switch ((grpSgnlsPars as GROUP_SIGNALS_SRC_PARS).m_mode)
                        {
                            case MODE_WORK.CUR_INTERVAL:
                                key = PanelLoader.KEY_CONTROLS.RBUTTON_CUR_DATETIME;
                                break;
                            case MODE_WORK.COSTUMIZE:
                                key = PanelLoader.KEY_CONTROLS.RBUTTON_COSTUMIZE;
                                break;
                            default:
                                throw new Exception(@"PanelWork::fillWorkItem () - ...");
                        }
                        workItem = GetWorkingItem(key);
                        (workItem as RadioButton).Checked = true;
                    }
                    else
                        ;

                    if (this is PanelLoaderSource)
                    {
                        //??? Отобразить интервал опроса для режима 'CUR_INTERVAL'
                        key = PanelLoader.KEY_CONTROLS.TBX_CUR_PERIOD;
                        workItem = GetWorkingItem(key);

                        //Отобразить шаг опроса для режима 'CUR_INTERVAL'
                        key = PanelLoader.KEY_CONTROLS.TBX_CUR_INTERVAL;
                        workItem = GetWorkingItem(key);
                    }
                    else
                        ;

                    //Отобразить дата опроса для режима 'COSTUMIZE'

                    //Отобразить нач./время опроса для режима 'COSTUMIZE'

                    //Отобразить кон./время опроса для режима 'COSTUMIZE'

                    //Отобразить шаг опроса для режима 'COSTUMIZE'
                }
                else
                    // не найдена группа сигналов
                    Logging.Logg().Warning(@"PanelLoader::FillWorkItem () - не найдена группа сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Включить элементы управления в соответствии с состоянием объектов
            ///  , которые они обозначают
            /// </summary>
            /// <param name="key">Ключ элемента управления</param>
            /// <param name="states">Состояния объектов</param>
            /// <returns>Признак выполнения функции</returns>
            public int EnabledWorkItem(KEY_CONTROLS key, GroupSources.STATE []states)
            {
                int iRes = 0; //Результат выполнения функции

                Control ctrl = GetWorkingItem(key); //Элемент управления, содержащий записи об объектах
                DataGridViewDisableButtonCell btnCell = null; //Ячейка (кнопка) - состояние объекта
                string btnCellText = string.Empty; //Текст для кнопки
                //Индекс выбранной на текущий момент строки (объекта)
                int indxSel = (ctrl as DataGridView).SelectedRows.Count > 0 ? (ctrl as DataGridView).SelectedRows[0].Index : -1;
                bool bEnabled = false;

                if ((!(indxSel < 0))
                //    //&& ((ctrl as DataGridView).Rows.Count == states.Length)
                    )
                    for (int i = 0; i < states.Length; i++)
                    {
                        bEnabled = false;
                        //Получить объект "кнопка"
                        btnCell = ((ctrl as DataGridView).Rows[i].Cells[1] as DataGridViewDisableButtonCell);
                        //Изменить доступность кнопки
                        btnCell.Enabled = !(states[i] == GroupSources.STATE.UNAVAILABLE);
                        //Определить текстт на кнопке в соответствии с состоянием
                        if (btnCell.Enabled == true)
                        {//При "доступной" (для нажатия) кнопке
                            switch (states[i])
                            {
                                case GroupSources.STATE.STARTED: //При "стартованной" кнопке (хотя бы одна из групп "старт")
                                    btnCellText = @"<-"; // для возможности "остановить" такие группы
                                    break;
                                case GroupSources.STATE.STOPPED: //Прии 
                                    btnCellText = @"->";
                                    bEnabled = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                            btnCellText = @"?";
                        //Установить текст на кнопке в соответствии состоянием
                        btnCell.Value = btnCellText;
                        //Изменить состояние "зависимых" элементов интерфейса
                        if (i == indxSel)
                        {//Только для выбранной строки
                            switch (key)
                            {
                                case KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    //??? GetWorkingItem(KEY_CONTROLS.BUTTON_DLLNAME_GROUPSOURCES).Enabled =
                                    GetWorkingItem(KEY_CONTROLS.CBX_SOURCE_OF_GROUP).Enabled =
                                    GetWorkingItem(KEY_CONTROLS.TBX_GROUPSOURCES_ADDING).Enabled =
                                        bEnabled;
                                    break;
                                case KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    //??? GetWorkingItem(KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS).Enabled =
                                    //???    bEnabled;
                                    break;
                                default:
                                    throw new Exception(@"PanelLoader::EnabledWorkItem () - ...");
                            }
                        }
                        else
                            ;
                    }
                else
                //    //Нельзя назначить состояние несуществцющей строке
                //    // , и наоборот нельзя оставить без состояния существующую строку
                    ;

                return iRes;
            }
            /// <summary>
            /// Получить объект - дочерний элемент интерфейса
            /// </summary>
            /// <param name="indxConfig">Индекс панели</param>
            /// <param name="indxPanel">Индекс типа объекта</param>
            /// <returns>Объект со списком групп</returns>
            public Control GetWorkingItem(KEY_CONTROLS key)
            {
                Control ctrlRes = null;
                PanelLoader panelLdr;

                panelLdr = this; //this.Controls[(int)indxWork] as PanelLoader;
                //Вариант №1
                //int indxCtrl = (int)PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES;
                //dgvRes = panelLdr.m_dictControl[(PanelLoader.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem;
                //Вариант №2
                Control[] arCtrls = panelLdr.Controls.Find(key.ToString(), true);
                if (arCtrls.Length == 1)
                    ctrlRes = arCtrls[0];
                else
                    throw new Exception(@"PanelLoader::GetWorkingItem (" + (Parent as PanelWork).Controls.IndexOf (this).ToString() + @", " + key.ToString() + @") - не найден элемент управления...");

                return ctrlRes;
            }
            public string GetWorkingItemId(KEY_CONTROLS key)
            {
                Control ctrl = GetWorkingItem(key);
                int indxSel = -1;

                if (ctrl is DataGridView)
                {
                    indxSel = (ctrl as DataGridView).SelectedRows[0].Index;
                    
                    return m_dictGroupIds[key][indxSel];
                }
                else
                    throw new Exception(@"PanelLoader::GetWorkingItemValue () - функция предназначена только для обработки объектов 'DataGridView'...");
            }
            /// <summary>
            /// Возратить выбранное значение строки в 'DataGridView'
            /// </summary>
            /// <param name="key">Ключ элемента управления</param>
            /// <returns>Значение в выбранной строке</returns>
            public string GetWorkingItemValue(KEY_CONTROLS key)
            {
                Control ctrl = GetWorkingItem(key);

                if (ctrl is DataGridView)
                    return (ctrl as DataGridView).SelectedRows[0].Cells[0].Value.ToString().Trim();
                else
                    throw new Exception(@"PanelLoader::GetWorkingItemValue () - функция предназначена только для обработки объектов 'DataGridView'...");
            }
            /// <summary>
            /// Возратить значение указанной строки в 'DataGridView'
            /// </summary>
            /// <param name="key">Ключ элемента управления</param>
            /// <param name="indxSel">Индекс выбранной строки</param>
            /// <returns>Значение в указанной строке</returns>
            public string GetWorkingItemValue(KEY_CONTROLS key, int indxSel)
            {
                Control ctrl = GetWorkingItem(key);

                if (ctrl is DataGridView)
                    return (ctrl as DataGridView).Rows[indxSel].Cells[0].Value.ToString().Trim();
                else
                    throw new Exception(@"PanelLoader::GetWorkingItemValue () - функция предназначена только для обработки объектов 'DataGridView'...");
            }
            /// <summary>
            /// Очистить все элементы управления на панели
            /// </summary>
            /// <returns>Признак выполнения функции (0 - успех)</returns>
            private int clearValues ()
            {
                int iRes = 0;

                iRes = clearValues(KEY_CONTROLS.DGV_GROUP_SIGNALS);
                // перед выполнением очередной операции проверить результат выполнения предыдущей
                if (iRes == 0)
                {
                    iRes = clearValues(KEY_CONTROLS.CBX_SOURCE_OF_GROUP);

                    if (iRes == 0)
                    {
                        iRes = clearValues(KEY_CONTROLS.DGV_SIGNALS_OF_GROUP);

                        if (iRes == 0)
                        {
                            iRes = clearValues(KEY_CONTROLS.TBX_GROUPSOURCES_ADDING);
                            
                            //if (iRes == 0)
                            //    if (this is PanelLoaderDest)
                            //        iRes = clearValues(KEY_CONTROLS.TBX_GROUPSIGNALS_ADDING);
                            //    else ;
                            //else ;
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
                else
                    ;

                return iRes;
            }
            /// <summary>
            /// Очистить элемент управления на панели с указанным ключом
            /// </summary
            /// <param name="key">Ключ элемента управления</param>
            /// <returns>Признак выполнения функции (0 - успех)</returns>
            private int clearValues(KEY_CONTROLS key)
            {
                int iRes = 0;
                //Получить элемент управления
                Control ctrl = GetWorkingItem(key);
                //Проверить наличие элемента
                if (!(ctrl == null))
                    if (ctrl.GetType().Equals(typeof(DataGridView)) == true)
                    {//Очистиить как 'DataGridView'
                        (ctrl as DataGridView).Rows.Clear();
                    }
                    else
                        if (ctrl.GetType().Equals(typeof(ComboBox)) == true)
                        {//Очистиить как 'ComboBox'
                            (ctrl as ComboBox).Items.Clear ();
                        }
                        else
                            if (ctrl.GetType().Equals(typeof(TextBox)) == true)
                            {//Очистиить как 'TextBox'
                                (ctrl as TextBox).Text = string.Empty;
                            }
                            else
                                ;
                else
                    //Элемент управления не найден
                    iRes = -1;

                return iRes;
            }

            public override void Stop()
            {
                //DataGridView dgv = GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
                //foreach (DataGridViewRow row in dgv.Rows)
                //    if ((row.Cells [1] is DataGridViewDisableButtonCell))
                
                base.Stop();
            }

            public abstract int UpdateData(DataTable table);
        }

        private class PanelLoaderSource : PanelLoader
        {
            public PanelLoaderSource ()
            {
                InitializeComponent ();
            }

            private void InitializeComponent ()
            {
                Control ctrl;
                HPanelCommon panelColumns = this.Controls[1] as HPanelCommon;

                this.SuspendLayout();

                //ГроупБокс режима опроса
                ctrl = new GroupBox();
                ctrl.Name = KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS.ToString();
                (ctrl as GroupBox).Text = @"Режим опроса";
                ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, panelColumns.GetRow(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS)) + panelColumns.GetRowSpan(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS)));
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, panelColumns.RowCount - panelColumns.GetRowSpan(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS)));
                //Панель для ГроупБокса
                HPanelCommon panelGroupBox = new PanelCommonULoader(8, 8);
                panelGroupBox.Dock = DockStyle.Fill;
                ctrl.Controls.Add(panelGroupBox);
                //Текущая дата/время
                //РадиоБуттон (тек. дата/время)
                ctrl = new RadioButton();
                ctrl.Name = KEY_CONTROLS.RBUTTON_CUR_DATETIME.ToString();
                (ctrl as RadioButton).Text = @"Текущие дата/время";
                (ctrl as RadioButton).CheckedChanged += new EventHandler(panelLoaderSource_ModeGroupSignals_CheckedChanged);
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 0);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Описание для интервала
                ctrl = new Label();
                (ctrl as Label).Text = @"Период(ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 1);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //TextBox изменения интервала
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_CUR_PERIOD.ToString();
                ctrl.Dock = DockStyle.Bottom;
                panelGroupBox.Controls.Add(ctrl, 6, 1);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Описание для периода
                ctrl = new Label();
                (ctrl as Label).Text = @"Интервал (ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 2);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //TextBox изменения интервала
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_CUR_INTERVAL.ToString();
                ctrl.Dock = DockStyle.Bottom;
                panelGroupBox.Controls.Add(ctrl, 6, 2);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Выборочно
                //РадиоБуттон (выборочно)
                ctrl = new RadioButton();
                ctrl.Name = KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString();
                (ctrl as RadioButton).Text = @"Выборочно";
                (ctrl as RadioButton).CheckedChanged += new EventHandler(panelLoaderSource_ModeGroupSignals_CheckedChanged);
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 3);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Календарь
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.CALENDAR_COSTUMIZE.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 4);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Начало периода
                //Начало периода - описание
                ctrl = new Label();
                (ctrl as Label).Text = @"Начало (ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 5);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);                
                //Начало периода - значение
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_COSTUMIZE_START_TIME.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 6, 5);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Период
                //Период - описание
                ctrl = new Label();
                (ctrl as Label).Text = @"Период (ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 6);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //Период - значение
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_COSTUMIZE_PERIOD.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 6, 6);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Интервал
                //Интервал - описание
                ctrl = new Label();
                (ctrl as Label).Text = @"Интервал (ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 7);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //Интервал - значение
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_COSTUMIZE_INTERVAL.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 6, 7);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);

                this.ResumeLayout (false);
                this.PerformLayout ();
            }

            /// <summary>
            /// Включение/отключение элементов управлении на панели параметров опроса
            ///  при изменении режима опроса
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент для сопровождения события</param>
            private void panelLoaderSource_ModeGroupSignals_CheckedChanged(object obj, EventArgs ev)
            {
                RadioButton rBtn = obj as RadioButton;
                KEY_CONTROLS key;
                //Реагировать только на 'RadioButton' в состоянии 'Checked'
                if (rBtn.Checked == true)
                {
                    //Определить состояние группы элементов (COSTUMIZE)
                    bool bCostumizeEnabled =
                        rBtn.Name.Equals(KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString())
                        //false
                        ;

                    //if (rBtn.Name.Equals (KEY_CONTROLS.RBUTTON_CUR_DATETIME.ToString ()) == true)
                    //{
                    //}
                    //else
                    //    if (rBtn.Name.Equals (KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString ()) == true)
                    //    {
                    //         bCostumizeEnabled = true;
                    //    }
                    //    else
                    //        ;

                    key = KEY_CONTROLS.TBX_CUR_PERIOD;
                    Controls.Find(key.ToString(), true)[0].Enabled = !bCostumizeEnabled;
                    key = KEY_CONTROLS.TBX_CUR_INTERVAL;
                    Controls.Find(key.ToString(), true)[0].Enabled = !bCostumizeEnabled;

                    key = KEY_CONTROLS.CALENDAR_COSTUMIZE;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.TBX_COSTUMIZE_START_TIME;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.TBX_COSTUMIZE_PERIOD;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.TBX_COSTUMIZE_INTERVAL;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                }
                else
                    ; //Не "отмеченные" - игнорировать
            }

            public override int UpdateData(DataTable table)
            {
                int iRes = 0;

                Logging.Logg().Debug(@"PanelLoaderSources::UpdateData () - строк для отображения=" + table.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                if (table.Rows.Count > 0)
                {
                    DataGridView dgv = GetWorkingItem (KEY_CONTROLS.DGV_SIGNALS_OF_GROUP) as DataGridView;
                    DataRow []arSel = null;
                    foreach (DataGridViewRow dgvRow in dgv.Rows)
                    {
                        arSel = table.Select (@"NAME_SHR='" + dgvRow.Cells[0].Value + @"'");
                        if (arSel.Length == 1)
                        {
                            try
                            {
                                dgvRow.Cells[1].Value = arSel[0][@"VALUE"];
                                dgvRow.Cells[2].Value = arSel[0][@"DATETIME"];
                                dgvRow.Cells[3].Value = arSel[0][@"COUNT"];
                            }
                            catch (Exception e)
                            {
                                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"PanelLoaderSources::UpdateData () - ...");
                            }
                        }
                        else
                            //throw new Exception(@"PanelWork.PanelLoaderSource::UpdateData () - невозможно определить строку для отображения [NAME_SHR=" + dgvRow.Cells[0].Value + @"]...");
                        {
                            Logging.Logg().Error(@"PanelLoaderSources::UpdateData () - не найдена строка для NAME_SHR=" + dgvRow.Cells[0].Value + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                            dgvRow.Cells[1].Value =
                            dgvRow.Cells[2].Value =
                            dgvRow.Cells[3].Value =
                                string.Empty;
                        }
                    }
                }
                else
                    ;

                return iRes;
            }
        }

        private class PanelLoaderDest : PanelLoader
        {
            public PanelLoaderDest()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                Control ctrl;
                HPanelCommon panelColumns = this.Controls[1] as HPanelCommon;

                this.SuspendLayout();

                ctrl = GetWorkingItem (KEY_CONTROLS.DGV_GROUP_SIGNALS);
                panelColumns.SetRowSpan (ctrl, 8);

                //ГроупБокс управления очистки данных
                //ГроупБокс
                ctrl = new GroupBox();
                ctrl.Name = KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS.ToString();
                (ctrl as GroupBox).Text = @"Удалить значения";
                ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 8);
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, 8);
                //Панель для ГроупБокса
                HPanelCommon panelGroupBox = new PanelCommonULoader(8, 4);
                panelGroupBox.Dock = DockStyle.Fill;
                ctrl.Controls.Add(panelGroupBox);
                //Календарь
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.CALENDAR_COSTUMIZE.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 0);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Начало периода
                //Начало периода - описание
                ctrl = new Label();
                (ctrl as Label).Text = @"Начало (ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 1);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //Начало периода - значение
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_COSTUMIZE_START_TIME.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 6, 1);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Период
                //Период - описание
                ctrl = new Label();
                (ctrl as Label).Text = @"Период (ЧЧ:ММ)";
                //ctrl.Dock = DockStyle.Bottom;
                ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 2);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //Период - значение
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_COSTUMIZE_PERIOD.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 6, 2);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Кнопка - выполнить
                ctrl = new Button();
                ctrl.Name = @"BTN_CLEAR";
                (ctrl as Button).Text = @"Выполнить";
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 3);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);

                ////ГроупБокс дополнительных настроек для группы сигналов
                //ctrl = new GroupBox();
                //ctrl.Name = KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS.ToString() + @"-ADDING";
                //(ctrl as GroupBox).Text = @"Дополнительные параметры";
                ////ctrl.Enabled = false;
                //ctrl.Dock = DockStyle.Fill;
                //panelColumns.Controls.Add(ctrl, 0, 12);
                //panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, 4);
                //////TextBox для редактирования дополнительных параметров
                ////ctrl.Controls.Add(new TextBox ());
                ////ctrl.Controls[0].Name = KEY_CONTROLS.TBX_GROUPSIGNALS_ADDING.ToString ();
                ////(ctrl.Controls[0] as TextBox).Multiline = true;
                ////(ctrl.Controls[0] as TextBox).ScrollBars = ScrollBars.Both;
                ////ctrl.Controls[0].Enabled = false;
                ////ctrl.Controls[0].Dock = DockStyle.Fill;

                this.ResumeLayout(false);
                this.PerformLayout();
            }

            public override int UpdateData(DataTable table)
            {
                int iRes = 0;

                if (table.Rows.Count > 0)
                {
                    DataGridView dgv = GetWorkingItem (KEY_CONTROLS.DGV_SIGNALS_OF_GROUP) as DataGridView;
                    DataRow []arSel = null;
                    foreach (DataGridViewRow dgvRow in dgv.Rows)
                    {
                        arSel = table.Select (@"NAME_SHR='" + dgvRow.Cells[0].Value + @"'");
                        if (arSel.Length == 1)
                        {
                            dgvRow.Cells[1].Value = arSel[0][@"VALUE"];
                            dgvRow.Cells[2].Value = arSel[0][@"DATETIME"];
                            dgvRow.Cells[3].Value = arSel[0][@"COUNT"];
                        }
                        else
                            //throw new Exception(@"PanelWork.PanelLoaderSource::UpdateData () - невозможно определить строку для отображения [NAME_SHR=" + dgvRow.Cells[0].Value + @"]...");
                        {
                            dgvRow.Cells[1].Value =
                            dgvRow.Cells[2].Value =
                            dgvRow.Cells[3].Value =
                                string.Empty;
                        }
                    }
                }
                else
                    ;

                return iRes;
            }
        }
    }    
}
