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
    public enum KEY_EVENT { SELECTION_CHANGED, CELL_CLICK, BTN_DLL_RELOAD, BTN_CLEAR_CLICK }

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
                                        , RBUTTON_CUR_DATETIME
                                        , RBUTTON_COSTUMIZE
                                        , CALENDAR_START_DATE, MTBX_START_TIME, MTBX_PERIOD_MAIN, MTBX_PERIOD_LOCAL, TBX_INTERVAL
                                        , BTN_CLEAR
                                        //, TBX_GROUPSIGNALS_ADDING
                                        , DGV_SIGNALS_OF_GROUP
                                            , COUNT_KEY_CONTROLS
                                        ,};

            enum DGV_GROUP_SIGNALS_COL_INDEX { AUTO_START, SHR_NAME, TURN_ONOFF, COUNT_DGV_GROUP_SIGNALS_COL_INDEX }

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
                (ctrl as Button).Click += new EventHandler(panelLoader_SourceOfGroupDLLReload);
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
                        new DataGridViewCheckBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
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
                (ctrl as DataGridView).Columns[(int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[(int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START].HeaderText = @"АвтоСт.";
                //(ctrl as DataGridView).Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[(int)DGV_GROUP_SIGNALS_COL_INDEX.SHR_NAME].Width = 107;
                (ctrl as DataGridView).Columns[(int)DGV_GROUP_SIGNALS_COL_INDEX.SHR_NAME].HeaderText = @"Группы сигналов";
                (ctrl as DataGridView).Columns[(int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF].Width = 37;
                (ctrl as DataGridView).Columns[(int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF].HeaderText = @"Вкл./выкл.";
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
                int cnt = (obj as DataGridView).Rows.Count
                    , indxCol = -1;
                indxCol = (obj as DataGridView).ColumnCount == (int)DGV_GROUP_SIGNALS_COL_INDEX.COUNT_DGV_GROUP_SIGNALS_COL_INDEX ? (int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF : (int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF - 1;
                (obj as DataGridView).Rows[cnt - 1].Cells[indxCol].Value = @"?";
                if ((obj as DataGridView).Rows[cnt - 1].Cells[indxCol].GetType() == typeof(DataGridViewDisableButtonCell))
                    ((DataGridViewDisableButtonCell)(obj as DataGridView).Rows[cnt - 1].Cells[indxCol]).Enabled = false;
                else
                    ;
            }            
            /// <summary>
            /// Найти целочисленный идентфикатор элемента управления
            /// </summary>
            /// <param name="ctrl">Элемент управления</param>
            /// <returns></returns>
            protected KEY_CONTROLS getKeyWorkingItem (Control ctrl)
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
            
            private object[] getPrepareGroupSignalChangedPars(DataGridView obj, int indxRow)
            {
                object[] arObjRes = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                MODE_WORK modeWork = MODE_WORK.UNKNOWN;
                GROUP_SIGNALS_PARS objDepenceded;
                if (this is PanelLoaderSource)
                {
                    objDepenceded = new GROUP_SIGNALS_SRC_PARS();

                    modeWork = (GetWorkingItem(KEY_CONTROLS.RBUTTON_CUR_DATETIME) as RadioButton).Checked == true ? MODE_WORK.CUR_INTERVAL :
                        (GetWorkingItem(KEY_CONTROLS.RBUTTON_COSTUMIZE) as RadioButton).Checked == true ? MODE_WORK.COSTUMIZE : MODE_WORK.UNKNOWN;
                    (objDepenceded as GROUP_SIGNALS_SRC_PARS).m_mode = modeWork;
                }
                else
                    if (this is PanelLoaderDest)
                    {
                        objDepenceded = new GROUP_SIGNALS_DEST_PARS();

                        modeWork = MODE_WORK.COSTUMIZE;
                    }
                    else
                        throw new Exception(@"PanelLoader::getPrepareGroupSignalChangedPars () - неизвестный тип панели ...");

                objDepenceded.m_iAutoStart = ((bool)((obj.Rows[indxRow].Cells[(int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START]).Value) == true) ? 1 : 0;

                objDepenceded.m_arWorkIntervals[(int)modeWork].m_dtStart = (GetWorkingItem(KEY_CONTROLS.CALENDAR_START_DATE) as DateTimePicker).Value.Date;
                objDepenceded.m_arWorkIntervals[(int)modeWork].m_dtStart += fromMaskedTextBox(KEY_CONTROLS.MTBX_START_TIME).Value;
                objDepenceded.m_arWorkIntervals[(int)modeWork].m_tsPeriodMain =
                    fromMaskedTextBox(KEY_CONTROLS.MTBX_PERIOD_MAIN);
                objDepenceded.m_arWorkIntervals[(int)modeWork].m_tsPeriodLocal =
                    this is PanelLoaderSource ? fromMaskedTextBox(KEY_CONTROLS.MTBX_PERIOD_LOCAL) : HTimeSpan.NotValue;
                objDepenceded.m_arWorkIntervals[(int)modeWork].m_tsIntervalLocal.Text =
                    this is PanelLoaderSource ? (GetWorkingItem(KEY_CONTROLS.TBX_INTERVAL) as TextBox).Text : @"-ms1";

                arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] = KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS; // обязательно для switch
                arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES);
                objDepenceded.m_strId = getGroupId(KEY_CONTROLS.DGV_GROUP_SIGNALS); //Уточнить идентификатор для группы сигналов
                arObjRes[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = objDepenceded; //

                arObjRes[(int)INDEX_PREPARE_PARS.OBJ] = this;
                arObjRes[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                return arObjRes;
            }
            /// <summary>
            /// Подготовить параметры для передачи "родительской" панели
            ///  для последующего на их основе формирования события
            ///  ретрансляции и постановки в очередь обработки событий
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="indxRow">индекс "выделенной" строки объекта, инициировавшего событие</param>
            /// <returns>Массив параметров</returns>
            private object[] getPrepareCommonPars(DataGridView obj, int indxRow)
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
                            getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES)
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
                int indxCol = (obj as DataGridView).ColumnCount == (int)DGV_GROUP_SIGNALS_COL_INDEX.COUNT_DGV_GROUP_SIGNALS_COL_INDEX ? (int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF : (int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF - 1;
                //Параметры-аргументы события для дальнейшей обработки
                object[] arPreparePars = null;
                //Ключ элемента управления
                KEY_CONTROLS key = getKeyWorkingItem (obj as Control);
                //Проверить индекс ячейки
                if (ev.ColumnIndex == (int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START)
                    //Признак "авто/запуск" группы сигналов
                    if (key == KEY_CONTROLS.DGV_GROUP_SIGNALS)
                    {//Только для группы сигналов
                        DataGridView dgv = GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS) as DataGridView;
                        arPreparePars = getPrepareGroupSignalChangedPars(dgv, dgv.SelectedRows[0].Index);

                        ////Вариант №1
                        //int iAutoStart = (arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] as GROUP_SIGNALS_PARS).m_iAutoStart;
                        //if (iAutoStart == 0)
                        //    iAutoStart = 1;
                        //else
                        //    if (iAutoStart == 1)
                        //        iAutoStart = 0;
                        //    else
                        //        throw new Exception(@"PanelLoader::panelLoader_WorkItemCellClick () - неизвестное состояние автозапуска для key=" + key + @"...");
                        //(arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] as GROUP_SIGNALS_PARS).m_iAutoStart = iAutoStart;

                        ////Вариант №2
                        //(arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] as GROUP_SIGNALS_PARS).m_iAutoStart =
                        //    ((bool)(((obj as DataGridView).Rows[ev.RowIndex].Cells[(int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START]).Value) == true) ? 0 : 1;

                        //Console.WriteLine(@"PanelLoader::panelLoader_WorkItemCellClick () - iAutoStart=" + (arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] as GROUP_SIGNALS_PARS).m_iAutoStart + @"...");

                        //Вариант №3
                        (arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] as GROUP_SIGNALS_PARS).m_iAutoStart = 2;

                        //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                        DataAskedHost(arPreparePars);
                    }
                    else
                        ;
                else
                    if (ev.ColumnIndex == indxCol)
                    {//Кнопки только в КРАЙНем столбце
                        //Проверить способность ячейки изменять свое состояние-ошибка??
                        if (((obj as DataGridView).Rows[ev.RowIndex].Cells[ev.ColumnIndex].GetType () == typeof (DataGridViewDisableButtonCell))
                            // , если - да, то проверить "включенное" состояние
                            && (((obj as DataGridView).Rows[ev.RowIndex].Cells[ev.ColumnIndex] as DataGridViewDisableButtonCell).Enabled == true))
                        {
                            //Подготовить параметры для передачи "родительской" панели
                            arPreparePars = getPrepareCommonPars(obj as DataGridView, ev.RowIndex);

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
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент при возникновении события</param>
            private void panelLoader_WorkItemSelectionChanged(object obj, EventArgs ev)
            {
                //Проверить наличие возможности выбора строки
                if ((obj as DataGridView).SelectedRows.Count > 0)
                {
                    //Подготовить параметры для передачи "родительской" панели
                    object[] arPreparePars = getPrepareCommonPars(obj as DataGridView, (obj as DataGridView).SelectedRows[0].Index);

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
                            GetWorkingItem(KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS).Enabled =
                                false;
                            break;
                        default:
                            break;
                    }

                    arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                    arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                    if (arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA].Equals(string.Empty) == false)
                        //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                        DataAskedHost(arPreparePars);
                    else
                        ;
                }
                else
                    ; //Нет выбранных строк
            }
            /// <summary>
            /// Обработчик события - изменение значения
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент при возникновении события</param>
            private void panelLoader_SourceOfGroupSelectedValueChanged(object obj, EventArgs ev)
            {
                //Подготовить параметры для передачи "родительской" панели
                object[] arPreparePars = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ] = KEY_CONTROLS.CBX_SOURCE_OF_GROUP;
                arPreparePars[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES);
                arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = m_dictGroupIds[KEY_CONTROLS.CBX_SOURCE_OF_GROUP][(obj as ComboBox).SelectedIndex];

                arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                if (arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA].Equals (string.Empty) == false)
                    //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                    DataAskedHost(arPreparePars);
                else
                    ;
            }
            /// <summary>
            /// Обработчик события - нажатие на кнопку "Выгрузить/загрузить ДЛЛ"
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void panelLoader_SourceOfGroupDLLReload(object obj, EventArgs ev)
            {
                //Подготовить параметры для передачи "родительской" панели
                object[] arPreparePars = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ] = KEY_CONTROLS.BUTTON_DLLNAME_GROUPSOURCES;
                arPreparePars[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES);
                //arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = ;

                arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.BTN_DLL_RELOAD;

                //if (arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA].Equals(string.Empty) == false)
                    //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                    DataAskedHost(arPreparePars);
                //else ;
            }
            /// <summary>
            /// Преобразовать значение 'MaskedTextBox'
            /// </summary>
            /// <param name="key">Идентификатор элемента управления со значением для преобразования</param>
            /// <returns>Преобразованное значение</returns>
            protected HTimeSpan fromMaskedTextBox (KEY_CONTROLS key)
            {
                HTimeSpan tsRes = HTimeSpan.NotValue;
                string []vals;
                //(ctrl as MaskedTextBox).Culture.NumberFormat.NumberDecimalSeparator
                MaskedTextBox ctrl = GetWorkingItem(key) as MaskedTextBox;

                vals = ctrl.Text.Split(new char[] { ctrl.Culture.NumberFormat.NumberDecimalSeparator[0], ':' });

                switch (vals.Length)
                {
                    case 1:
                        tsRes = new HTimeSpan(vals[0]);//00:00??dd1 or hh24
                        break;
                    case 2:
                        tsRes = HTimeSpan.FromMinutes(Int32.Parse(vals[0]) * 60 + Int32.Parse(vals[1]));//00:00??dd1 or hh24
                        break;
                    case 3:
                        tsRes = HTimeSpan.FromMinutes(Int32.Parse(vals[0]) * 24 * 60 + Int32.Parse(vals[1]) * 60 + Int32.Parse(vals[2]));//00:00??dd1 or hh24
                        break;
                }

                return tsRes;
            }
            /// <summary>
            /// Обработчик события - потеря фокусса ввода элемента управления 'KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS'
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент при возникновении события</param>
            protected void panelLoader_grpBoxGroupSignalsLeave (object obj, EventArgs ev)
            {
                object[] arPreparePars = null;

                DataGridView dgv = GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS) as DataGridView;
                arPreparePars = getPrepareGroupSignalChangedPars(dgv, dgv.SelectedRows[0].Index);

                DataAskedHost(arPreparePars);
            }

            protected string getGroupId (KEY_CONTROLS key)
            {
                return m_dictGroupIds[key][(GetWorkingItem(key) as DataGridView).SelectedRows[0].Index];
            }

            private void panelLoader_GroupSourcesAddingLeave(object obj, EventArgs ev)
            {
                //Подготовить параметры для передачи "родительской" панели
                object[] arPreparePars = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                string strDepencededData = string.Empty;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ] = getKeyWorkingItem (obj as Control);
                arPreparePars[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES);
                foreach (string line in (obj as TextBox).Lines)
                    strDepencededData += line + FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL];
                if (strDepencededData.Length > (FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL].ToString().Length + 1))
                    strDepencededData = strDepencededData.Substring (0, strDepencededData.Length - FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL].ToString().Length);
                else
                    strDepencededData = string.Empty;

                if (strDepencededData.Equals (string.Empty) == false)
                {
                    arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = strDepencededData;

                    arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                    arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                    //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                    DataAskedHost(arPreparePars);
                }
                else
                    ;
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
                    //Вставить значения с индексами: (int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START, (int)DGV_GROUP_SIGNALS_COL_INDEX.SHR_NAME
                    (workItem as DataGridView).Rows.Add(new object[] { grpSrc.m_listGroupSignalsPars [j].m_iAutoStart == 1, grpSrc.m_listGroupSignalsPars [j].m_strShrName });
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
                MODE_WORK modeWork = MODE_WORK.UNKNOWN;
                bool bAutoUpdateDatetimePars = false;

                if (!(grpSgnlsPars == null))
                {
                    //Отобразить активный режим
                    // только, если панель - источник
                    if (this is PanelLoaderSource)
                    {
                        modeWork = (grpSgnlsPars as GROUP_SIGNALS_SRC_PARS).m_mode;

                        switch (modeWork)
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
                        bAutoUpdateDatetimePars = ! (workItem as RadioButton).Checked;
                        (workItem as RadioButton).Checked = true;
                    }
                    else
                        modeWork = MODE_WORK.COSTUMIZE;

                    if (bAutoUpdateDatetimePars == false)
                        FillDatetimePars (grpSgnlsPars.m_arWorkIntervals[(int)modeWork]);
                    else
                        ;                    
                }
                else
                    // не найдена группа сигналов
                    Logging.Logg().Warning(@"PanelLoader::FillWorkItem () - не найдена группа сигналов...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            public void FillDatetimePars (DATETIME_WORK pars)
            {
                Control workItem;
                PanelLoader.KEY_CONTROLS key;

                //Отобразить дата опроса для режима 'COSTUMIZE'
                key = PanelLoader.KEY_CONTROLS.CALENDAR_START_DATE;
                workItem = GetWorkingItem(key);
                (workItem as DateTimePicker).Value = pars.m_dtStart; //DateTime.Now;

                //Отобразить нач./время опроса для режима 'COSTUMIZE'
                key = PanelLoader.KEY_CONTROLS.MTBX_START_TIME;
                workItem = GetWorkingItem(key);
                (workItem as MaskedTextBox).Text = pars.m_dtStart.Hour.ToString(@"00")
                    + pars.m_dtStart.Minute.ToString(@"00")
                    ;

                //??? Отобразить период опроса (основной)
                key = PanelLoader.KEY_CONTROLS.MTBX_PERIOD_MAIN;
                workItem = GetWorkingItem(key);
                (workItem as MaskedTextBox).Text = pars.m_tsPeriodMain.Value.Days.ToString(@"00")
                    + pars.m_tsPeriodMain.Value.Hours.ToString(@"00")
                    + pars.m_tsPeriodMain.Value.Minutes.ToString(@"00")
                    //+ @":" + grpSgnlsPars.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriod.Seconds
                    ;

                if (this is PanelLoaderSource)
                {
                    //??? Отобразить период опроса (локальный)
                    key = PanelLoader.KEY_CONTROLS.MTBX_PERIOD_LOCAL;
                    workItem = GetWorkingItem(key);
                    (workItem as MaskedTextBox).Text = pars.m_tsPeriodMain.Value.Days.ToString(@"00")
                        + pars.m_tsPeriodLocal.Value.Hours.ToString(@"00")
                        + pars.m_tsPeriodLocal.Value.Minutes.ToString(@"00")
                        ;

                    //Отобразить шаг опроса для режима 'COSTUMIZE'
                    key = PanelLoader.KEY_CONTROLS.TBX_INTERVAL;
                    workItem = GetWorkingItem(key);
                    (workItem as TextBox).Text = pars.m_tsIntervalLocal.ToString();
                }
                else
                    ; //Не 'Source'
            }
            /// <summary>
            /// Обновить состояние кнопки управления группой (источников, сигналов)
            /// </summary>
            /// <param name="ctrl">Элемент управления на котором размещается обновляемая кнопка</param>
            /// <param name="indx">Индекс строки в которой размещена кнопка</param>
            /// <param name="state">Состояние для кнопки</param>
            /// <returns>Признак разрешающий/запрещающий редактирование доп./параметров</returns>
            private bool updateBtnCell (Control ctrl, int indx, GroupSources.STATE state)
            {
                bool bRes = false;
                int indxCell = -1;
                //Получить объект "кнопка"
                indxCell = (ctrl as DataGridView).ColumnCount == (int)DGV_GROUP_SIGNALS_COL_INDEX.COUNT_DGV_GROUP_SIGNALS_COL_INDEX ? (int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF : (int)DGV_GROUP_SIGNALS_COL_INDEX.TURN_ONOFF - 1; 
                DataGridViewDisableButtonCell btnCell = ((ctrl as DataGridView).Rows[indx].Cells[indxCell] as DataGridViewDisableButtonCell); //Ячейка (кнопка) - состояние объекта
                string btnCellText = string.Empty; //Текст для кнопки
                //Изменить доступность кнопки
                btnCell.Enabled = !(state == GroupSources.STATE.UNAVAILABLE);
                //Определить текстт на кнопке в соответствии с состоянием
                if (btnCell.Enabled == true)
                {//При "доступной" (для нажатия) кнопке
                    switch (state)
                    {
                        case GroupSources.STATE.STARTED: //При "стартованной" кнопке (хотя бы одна из групп "старт")
                            btnCellText = @"<-"; // для возможности "остановить" такие группы
                            break;
                        case GroupSources.STATE.STOPPED: //Прии 
                            btnCellText = @"->";
                            bRes = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                    btnCellText = @"?";
                //Установить текст на кнопке в соответствии состоянием
                btnCell.Value = btnCellText;
                // когда кнопка доступна(включена), а группа остановлена = True
                return bRes;
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

                bool bEnabled = false;
                Control ctrl = GetWorkingItem(key); //Элемент управления, содержащий записи об объектах

                //Индекс выбранной на текущий момент строки (объекта)
                int indxSel = (ctrl as DataGridView).SelectedRows.Count > 0 ? (ctrl as DataGridView).SelectedRows[0].Index : -1;

                if ((!(indxSel < 0))
                //    //&& ((ctrl as DataGridView).Rows.Count == states.Length)
                    )
                    for (int i = 0; i < states.Length; i++)
                    {
                        bEnabled = updateBtnCell(ctrl, i, states[i]);
                        //Изменить состояние "зависимых" элементов интерфейса
                        if (i == indxSel)
                        {//Только для выбранной строки
                            switch (key)
                            {
                                case KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    GetWorkingItem(KEY_CONTROLS.BUTTON_DLLNAME_GROUPSOURCES).Enabled =
#if _SEPARATE_APPDOMAIN
                                        ! ((states[i] == GroupSources.STATE.UNKNOWN) || (states[i] == GroupSources.STATE.UNAVAILABLE))
#else
                                        false
#endif
                                        ;
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
            /// Включить элементы управления в соответствии с состоянием объектов (и-или параметров группы сигналов)
            /// </summary>
            /// <param name="key">Ключ элемента управления</param>
            /// <param name="args">Массив аргументов для каждой из групп сигналов (STATE, bEnableTools)</param>
            /// <returns></returns>
            public int EnabledWorkItem(KEY_CONTROLS key, object []args)
            {
                int iRes = 0; //Результат выполнения функции

                bool bEnabled = false;
                Control ctrl = GetWorkingItem(key); //Элемент управления, содержащий записи об объектах

                //Индекс выбранной на текущий момент строки (объекта)
                int indxSel = (ctrl as DataGridView).SelectedRows.Count > 0 ? (ctrl as DataGridView).SelectedRows[0].Index : -1;

                if ((!(indxSel < 0))
                    //    //&& ((ctrl as DataGridView).Rows.Count == states.Length)
                    )
                    for (int i = 0; i < args.Length; i++)
                    {
                        bEnabled = updateBtnCell(ctrl, i, (GroupSources.STATE)(args[i] as object[])[0]) // 0 - состояние группы
                            && (bool)(args[i] as object[])[1]; // 1 - признак вкл/выкл параметров настроек для группы (источников, сигналов)
                        //Изменить состояние "зависимых" элементов интерфейса
                        if (i == indxSel)
                        {//Только для выбранной строки
                            switch (key)
                            {
                                case KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    ;
                                    break;
                                case KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    GetWorkingItem(KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS).Enabled =
                                        bEnabled;
                                    // 2 - признак авто-старт для группы (ТОЛЬКО сигналов)
                                    ((ctrl as DataGridView).Rows[indxSel].Cells [(int)DGV_GROUP_SIGNALS_COL_INDEX.AUTO_START] as DataGridViewCheckBoxCell).Value = (int)((args[i] as object[])[2]) == 1;
                                    break;
                                default:
                                    throw new Exception(@"PanelLoader::EnabledWorkItem () - ...");
                            }
                        }
                        else
                            ;
                    }
                else
                    //Нельзя назначить состояние несуществцющей строке
                    // , и наоборот нельзя оставить без состояния существующую строку
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
            /// <summary>
            /// Очистить представление
            /// </summary>
            public void UpdateData()
            {
                DataGridView dgv = GetWorkingItem(KEY_CONTROLS.DGV_SIGNALS_OF_GROUP) as DataGridView;
                foreach (DataGridViewRow dgvRow in dgv.Rows)
                    dgvRow.Cells[1].Value =
                    dgvRow.Cells[2].Value =
                    dgvRow.Cells[3].Value =
                        string.Empty;
            }

            //public abstract int UpdateData(DataTable table);
            /// <summary>
            /// Отобразить полученные данные в представлении
            /// </summary>
            /// <param name="table">Таблица - результат запроса для отображения</param>
            /// <returns>результат выполнения функции (не используется)</returns>
            public virtual int UpdateData(DataTable table)
            {
                int iRes = 0;
                DataGridView dgv = null;
                MaskedTextBox mtbxStartTime = null;
                DataRow[] arSel = null;
                DateTime datetimeValue = DateTime.MinValue
                    , datetimeMinLastUpdate = DateTime.MinValue;

                Logging.Logg().Debug(@"PanelLoaderSources::UpdateData () - строк для отображения=" + table.Rows.Count + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                if (table.Rows.Count > 0) {
                    dgv = GetWorkingItem(KEY_CONTROLS.DGV_SIGNALS_OF_GROUP) as DataGridView;

                    foreach (DataGridViewRow dgvRow in dgv.Rows) {
                        arSel = table.Select(@"NAME_SHR='" + dgvRow.Cells[0].Value + @"'");
                        if (arSel.Length == 1) {
                            try {
                                datetimeValue = (DateTime)arSel[0][@"DATETIME"];

                                if ((datetimeMinLastUpdate - datetimeValue).TotalSeconds < 0)
                                    datetimeMinLastUpdate = (DateTime)arSel[0][@"DATETIME"];
                                else
                                    ;

                                dgvRow.Cells[1].Value = arSel[0][@"VALUE"];
                                dgvRow.Cells[2].Value = datetimeValue.Equals(DateTime.MinValue) == false ? datetimeValue.ToString(@"dd.MM.yyyy HH:mm:ss.fff") : string.Empty;
                                dgvRow.Cells[3].Value = arSel[0][@"COUNT"];
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, @"PanelLoaderSources::UpdateData () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        } else {
                            //throw new Exception(@"PanelWork.PanelLoaderSource::UpdateData () - невозможно определить строку для отображения [NAME_SHR=" + dgvRow.Cells[0].Value + @"]...");
                            Logging.Logg().Error(@"PanelLoaderSources::UpdateData () - не найдена строка для NAME_SHR=" + dgvRow.Cells[0].Value + @"...", Logging.INDEX_MESSAGE.NOT_SET);

                            dgvRow.Cells[1].Value =
                            dgvRow.Cells[2].Value =
                            dgvRow.Cells[3].Value =
                                string.Empty;
                        }
                    }

                    mtbxStartTime = GetWorkingItem(KEY_CONTROLS.MTBX_START_TIME) as MaskedTextBox;
                    mtbxStartTime.Text = datetimeMinLastUpdate.ToString(@"HH::mm");
                } else
                    ;

                return iRes;
            }
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

                ctrl = GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS);
                panelColumns.SetRowSpan(ctrl, 6);

                //ГроупБокс режима опроса
                ctrl = new GroupBox();
                ctrl.Name = KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS.ToString();
                (ctrl as GroupBox).Text = @"Режим опроса";
                ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                ctrl.Leave += new EventHandler(panelLoader_grpBoxGroupSignalsLeave);
                panelColumns.Controls.Add(ctrl, 0, panelColumns.GetRow(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS)) + panelColumns.GetRowSpan(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS)));
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, panelColumns.RowCount - panelColumns.GetRowSpan(GetWorkingItem(KEY_CONTROLS.DGV_GROUP_SIGNALS)));
                //Панель для ГроупБокса
                HPanelCommon panelGroupBox = new PanelCommonULoader(9, 7);
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
                //РадиоБуттон (выборочно)
                ctrl = new RadioButton();
                ctrl.Name = KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString();
                (ctrl as RadioButton).Text = @"Выборочно";
                (ctrl as RadioButton).CheckedChanged += new EventHandler(panelLoaderSource_ModeGroupSignals_CheckedChanged);
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 1);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Календарь
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.CALENDAR_START_DATE.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 2);
                panelGroupBox.SetColumnSpan(ctrl, 9); panelGroupBox.SetRowSpan(ctrl, 1);
                //Начало периода
                //Начало периода - описание
                ctrl = new Label();
                (ctrl as Label).Text = @"Начало (ЧЧ:ММ)";
                ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 3);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //Начало периода - значение
                ctrl = new MaskedTextBox();
                ctrl.Name = KEY_CONTROLS.MTBX_START_TIME.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as MaskedTextBox).TextAlign = HorizontalAlignment.Right;
                (ctrl as MaskedTextBox).Mask = @"00:00";
                panelGroupBox.Controls.Add(ctrl, 6, 3);
                panelGroupBox.SetColumnSpan(ctrl, 3); panelGroupBox.SetRowSpan(ctrl, 1);
                //Период
                //Описание для главного периода
                ctrl = new Label();
                (ctrl as Label).Text = @"Период(ДД.ЧЧ:ММ)";
                ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 4);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //TextBox изменения главного периода
                ctrl = new MaskedTextBox();
                ctrl.Name = KEY_CONTROLS.MTBX_PERIOD_MAIN.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as MaskedTextBox).TextAlign = HorizontalAlignment.Right;
                (ctrl as MaskedTextBox).Mask =
                    //string.Format(@"00{0}00:00", (ctrl as MaskedTextBox).Culture.NumberFormat.NumberDecimalSeparator[0])
                    @"00.00:00"
                    ;
                panelGroupBox.Controls.Add(ctrl, 6, 4);
                panelGroupBox.SetColumnSpan(ctrl, 3); panelGroupBox.SetRowSpan(ctrl, 1);

                //Описание для локального периода
                ctrl = new Label();
                (ctrl as Label).Text = @"Период(ДД.ЧЧ:ММ)";
                ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 5);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //TextBox изменения локального периода
                ctrl = new MaskedTextBox();
                ctrl.Name = KEY_CONTROLS.MTBX_PERIOD_LOCAL.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as MaskedTextBox).TextAlign = HorizontalAlignment.Right;
                (ctrl as MaskedTextBox).Mask =
                    //string.Format(@"00{0}00:00", (ctrl as MaskedTextBox).Culture.NumberFormat.NumberDecimalSeparator[0])
                    @"00.00:00"
                    ;
                panelGroupBox.Controls.Add(ctrl, 6, 5);
                panelGroupBox.SetColumnSpan(ctrl, 3); panelGroupBox.SetRowSpan(ctrl, 1);
                //Интервал
                ctrl = new Label();
                (ctrl as Label).Text = @"Интервал (код)";
                ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 6);
                panelGroupBox.SetColumnSpan(ctrl, 6); panelGroupBox.SetRowSpan(ctrl, 1);
                //TextBox изменения интервала
                ctrl = new /*Masked*/TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_INTERVAL.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as TextBox).TextAlign = HorizontalAlignment.Right;
                //(ctrl as MaskedTextBox).Mask = @"00:00";
                panelGroupBox.Controls.Add(ctrl, 6, 6);
                panelGroupBox.SetColumnSpan(ctrl, 3); panelGroupBox.SetRowSpan(ctrl, 1);

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
                Control ctrl;
                KEY_CONTROLS key = KEY_CONTROLS.UNKNOWN;
                MODE_WORK modeWork = MODE_WORK.UNKNOWN;

                //Реагировать только на 'RadioButton' в состоянии 'Checked'
                if (rBtn.Checked == true)
                {
                    //Определить состояние группы элементов (COSTUMIZE)
                    bool bCostumizeEnabled =
                        rBtn.Name.Equals(KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString())
                        //false
                        ;

                    modeWork = bCostumizeEnabled == true ? MODE_WORK.COSTUMIZE : MODE_WORK.CUR_INTERVAL;

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

                    key = KEY_CONTROLS.CALENDAR_START_DATE; ctrl = Controls.Find(key.ToString(), true)[0];
                    ctrl.Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.MTBX_START_TIME; ctrl = Controls.Find(key.ToString(), true)[0];
                    ctrl.Enabled = bCostumizeEnabled;

                    //key = KEY_CONTROLS.MTBX_PERIOD_MAIN; ctrl = Controls.Find(key.ToString(), true)[0];
                    //ctrl.Enabled = !bCostumizeEnabled;
                    key = KEY_CONTROLS.MTBX_PERIOD_LOCAL; ctrl = Controls.Find(key.ToString(), true)[0];
                    ctrl.Enabled = bCostumizeEnabled;
                    //key = KEY_CONTROLS.TBX_INTERVAL; ctrl = Controls.Find(key.ToString(), true)[0];
                    //ctrl.Enabled = !bCostumizeEnabled;

                    object[] arObjRes = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];
                    arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] = getKeyWorkingItem(rBtn); // обязательно для switch
                    arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES);
                    arObjRes[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = new object[] { getGroupId(KEY_CONTROLS.DGV_GROUP_SIGNALS) //Уточнить идентификатор для группы сигналов
                        , modeWork }; // для выбранного режима

                    arObjRes[(int)INDEX_PREPARE_PARS.OBJ] = this;
                    arObjRes[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                    //Отправить запрос на обновление параметров  группы сигналов "родительской" панели (для ретрансляции)
                    DataAskedHost(arObjRes);
                }
                else
                    ; //Не "отмеченные" - игнорировать
            }            
        }
        /// <summary>
        /// Класс для описания элемента управления - панель с группами источников (назначения), группами сигналов (назначения)
        /// </summary>
        private class PanelLoaderDest : PanelLoader
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
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
                ctrl.Leave += new EventHandler(panelLoader_grpBoxGroupSignalsLeave);
                panelColumns.Controls.Add(ctrl, 0, 8);
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, 8);
                //Панель для ГроупБокса
                HPanelCommon panelGroupBox = new PanelCommonULoader(8, 4);
                panelGroupBox.Dock = DockStyle.Fill;
                ctrl.Controls.Add(panelGroupBox);
                //Календарь
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.CALENDAR_START_DATE.ToString();
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
                ctrl = new MaskedTextBox();
                ctrl.Name = KEY_CONTROLS.MTBX_START_TIME.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as MaskedTextBox).Mask = @"00:00";
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
                ctrl = new MaskedTextBox();
                ctrl.Name = KEY_CONTROLS.MTBX_PERIOD_MAIN.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as MaskedTextBox).Mask = @"00:00";
                panelGroupBox.Controls.Add(ctrl, 6, 2);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Кнопка - выполнить
                ctrl = new Button();
                ctrl.Name = KEY_CONTROLS.BTN_CLEAR.ToString();
                (ctrl as Button).Click += new EventHandler(panelLoaderDest_btnClearCLick);
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
            ///// <summary>
            ///// Отобразить (обновить) значения на панели для выбранной группы источников, группы сигналов
            ///// </summary>
            ///// <param name="table">Таблица - рез=т запроса</param>
            ///// <returns>Рез-т выполнения функции (всегда=0, не используется)</returns>
            //public override int UpdateData(DataTable table)
            //{
            //    int iRes = 0;

            //    if (table.Rows.Count > 0)
            //    {
            //        DataGridView dgv = GetWorkingItem (KEY_CONTROLS.DGV_SIGNALS_OF_GROUP) as DataGridView;
            //        DataRow []arSel = null;
            //        foreach (DataGridViewRow dgvRow in dgv.Rows)
            //        {
            //            arSel = table.Select (@"NAME_SHR='" + dgvRow.Cells[0].Value + @"'");
            //            if (arSel.Length == 1)
            //            {
            //                dgvRow.Cells[1].Value = arSel[0][@"VALUE"];
            //                dgvRow.Cells[2].Value = arSel[0][@"DATETIME"];
            //                dgvRow.Cells[3].Value = arSel[0][@"COUNT"];
            //            }
            //            else
            //                //throw new Exception(@"PanelWork.PanelLoaderSource::UpdateData () - невозможно определить строку для отображения [NAME_SHR=" + dgvRow.Cells[0].Value + @"]...");
            //            {
            //                dgvRow.Cells[1].Value =
            //                dgvRow.Cells[2].Value =
            //                dgvRow.Cells[3].Value =
            //                    string.Empty;
            //            }
            //        }
            //    }
            //    else
            //        ;

            //    return iRes;
            //}

            private void panelLoaderDest_btnClearCLick (object obj, EventArgs ev)
            {
                //Подготовить параметры для передачи "родительской" панели
                object[] arPreparePars = new object [(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];

                //Найти целочисленный идентфикатор элемента управления
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ] = KEY_CONTROLS.DGV_GROUP_SIGNALS; //getKeyWorkingItem(obj as Control);
                arPreparePars[(int)INDEX_PREPARE_PARS.ID_OBJ_SEL] = new object[] { getGroupId(KEY_CONTROLS.DGV_GROUP_SOURCES), getGroupId(KEY_CONTROLS.DGV_GROUP_SIGNALS) };
                arPreparePars[(int)INDEX_PREPARE_PARS.DEPENDENCED_DATA] = new object [] {
                    (GetWorkingItem(KEY_CONTROLS.CALENDAR_START_DATE) as DateTimePicker).Value.Date
                    , fromMaskedTextBox (KEY_CONTROLS.MTBX_START_TIME)
                    , fromMaskedTextBox (KEY_CONTROLS.MTBX_PERIOD_MAIN)
                };                

                arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = this;
                arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.BTN_CLEAR_CLICK;

                //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                DataAskedHost(arPreparePars);
            }
        }
    }    
}
