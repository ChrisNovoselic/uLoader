using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms; //Control
using System.Drawing; //Point

using HClassLibrary;

namespace uLoader
{
    public partial class PanelConfig : PanelCommonDataHost
    {
        private class PanelSources : PanelCommonDataHost
        {
            /// <summary>
            /// Перечисление - индексы ПРЕДподготавливаемых параметров
            /// </summary>
            public enum INDEX_PREPARE_PARS { OBJ, KEY_OBJ, KEY_EVT, ID_OBJ_GROUP_SEL, ID_OBJ_ITEM_SEL, COUNT_INDEX_PREPARE_PARS }

            /// <summary>
            /// Вспомогательные константы для позиционирования объектов на панели
            /// </summary>            
            private static int COL_DIV = 1, ROW_DIV = 1
                , COL_COUNT = 4 / COL_DIV, ROW_COUNT = 1 / ROW_DIV;

            //Индексы панелей (группы источников, элементы в группе источников, группы сигналов, элементы в группе сигналов)
            public enum INDEX_PANEL
            {
                GROUP_SOURCES, SOURCES_OF_GROUP, GROUP_SIGNALS, SIGNALS_OF_GROUP
                    , COUNT_INDEX_PANEL
            };
            //Индексы групп элементов управления в панелях
            public enum INDEX_PANEL_CONTROL { GROUPBOX, LISTEDIT, TEXTBOX, BUTTON };
            //Ключи элементов управления
            public enum INDEX_CONTROL
            {
                GROUPBOX
                , LISTEDIT = GROUPBOX + INDEX_PANEL.COUNT_INDEX_PANEL
                , TEXTBOX = LISTEDIT + INDEX_PANEL.COUNT_INDEX_PANEL
                , BUTTON = TEXTBOX + INDEX_PANEL.COUNT_INDEX_PANEL
                , DGV_PARAMETER_SOURCE = BUTTON + INDEX_PANEL.COUNT_INDEX_PANEL
                , DGV_PARAMETER_SIGNAL
                , LABEL_DLLNAME_GROUPSOURCES
                , BUTTON_DLLNAME_GROUPSOURCES
                , RADIOBUTTON_SOURCE
                , RADIOBUTTON_DEST
                , BUTTON_UPDATE, BUTTON_SAVE
            };
            ////Заголовки строк параметров сединения - будут получены из файла конфигурации (PARS)
            //static string[] m_arHeaderRowParameterSources =
            //{
            //    @"ID", @"IP", @"PORT", @"DB_NAME", @"UID", @"PASSWORD"
            //};
            //Заголовки строк панелей
            static string[] m_arHeaderPanelListEdit =
            {
                @"Группы источников", @"Источники группы", @"Группы сигналов", @"Сигналы группы"
            };
            /// <summary>
            /// Словарь элементов управления
            /// </summary>
            public Dictionary<INDEX_CONTROL, Control> m_dictControl;
            /// <summary>
            /// Конструктор -
            /// </summary>
            public PanelSources() : base (COL_COUNT, ROW_COUNT)
            {
                InitializeComponent();
            }
            /// <summary>
            /// Конструктор (с параметром) -
            /// </summary>
            /// <param name="container">Объект-родитель по отношению к создаваемому объекту</param>
            public PanelSources(IContainer container)
                : base(COL_COUNT, ROW_COUNT)
            {
                container.Add(this);

                InitializeComponent();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly (cols, rows);
            }

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
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();

                //Для "безличного" обращения к элементу интерфейса
                TableLayoutPanel panelGroupBox = null;
                Control ctrl
                    , ctrlChild;

                ////Координаты, размеры для размещения элементов управления
                //Dictionary<INDEX_CONTROL, Position> dictPos = new Dictionary<INDEX_CONTROL,Position> ();
                ////Панели
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.PANEL * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.GROUP_SOURCES)
                //    , new Position (new Point (0, 0), new Size (0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.PANEL * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.SOURCES_OF_GROUP)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.PANEL * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.GROUP_SIGNALS)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.PANEL * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.SIGNALS_OF_GROUP)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                ////TextBox-ы
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.GROUP_SOURCES)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.SOURCES_OF_GROUP)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.GROUP_SIGNALS)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.SIGNALS_OF_GROUP)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                ////Кнопки
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.GROUP_SOURCES)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.SOURCES_OF_GROUP)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.GROUP_SIGNALS)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add((INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + INDEX_PANEL.SIGNALS_OF_GROUP)
                //    , new Position(new Point(0, 0), new Size(0, 0)));
                ////DGV_PARAMETER_SOURCES
                //dictPos.Add(INDEX_CONTROL.DGV_PARAMETER_SOURCES, new Position(new Point(0, 0), new Size(0, 0)));
                ////Переключатели
                //dictPos.Add(INDEX_CONTROL.RADIOBUTTON_SOURCE, new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add(INDEX_CONTROL.RADIOBUTTON_DEST, new Position(new Point(0, 0), new Size(0, 0)));
                ////Кнопки
                //dictPos.Add(INDEX_CONTROL.BUTTON_UPDATE, new Position(new Point(0, 0), new Size(0, 0)));
                //dictPos.Add(INDEX_CONTROL.BUTTON_SAVE, new Position(new Point(0, 0), new Size(0, 0)));

                m_dictControl = new Dictionary<INDEX_CONTROL, Control>();

                ////Создание объектов кнопок
                ////Загрузить
                //ctrl = new Button();
                //(ctrl as Button).Text = @"Загрузить";
                //(ctrl as Button).Dock = DockStyle.Fill;
                //m_dictControl.Add(INDEX_CONTROL.BUTTON_UPDATE, ctrl);
                ////Сохранить
                //ctrl = new Button();
                //(ctrl as Button).Text = @"Сохранить";
                //(ctrl as Button).Dock = DockStyle.Fill;
                //m_dictControl.Add(INDEX_CONTROL.BUTTON_SAVE, ctrl);
                int i = -1
                    , j = -1;
                //Создание ГроупБоксов для новых элементов панелей
                i = (int)INDEX_PANEL_CONTROL.GROUPBOX;
                for (j = 0; j < (int)INDEX_PANEL.COUNT_INDEX_PANEL; j++)
                {
                    ctrl = new GroupBox(); ctrl.Dock = DockStyle.Fill;
                    (ctrl as GroupBox).Text = m_arHeaderPanelListEdit[j];
                    m_dictControl.Add((INDEX_CONTROL)(i * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j), ctrl);
                }
                //Создание объектов 'TextBox' для новых элементов панелей
                i = (int)INDEX_PANEL_CONTROL.TEXTBOX;
                for (j = 0; j < (int)INDEX_PANEL.COUNT_INDEX_PANEL; j++)
                {
                    ctrl = new TextBox(); ctrl.Dock = DockStyle.Bottom;
                    m_dictControl.Add((INDEX_CONTROL)(i * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j), ctrl);
                }
                //Создание объектов кнопок для новых элементов панелей
                i = (int)INDEX_PANEL_CONTROL.BUTTON;
                for (j = 0; j < (int)INDEX_PANEL.COUNT_INDEX_PANEL; j++)
                {
                    ctrl = new Button(); (ctrl as Button).Text = @"+"; ctrl.Dock = DockStyle.Bottom;
                    m_dictControl.Add((INDEX_CONTROL)(i * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j), ctrl);
                }
                //Создание объектов панелей
                i = (int)INDEX_PANEL_CONTROL.LISTEDIT;
                for (j = 0; j < (int)INDEX_PANEL.COUNT_INDEX_PANEL; j++)
                {
                    ctrl = new DataGridViewConfigItem(
                        m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)] as TextBox
                        , m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)] as Button
                    );
                    m_dictControl.Add((INDEX_CONTROL)(i * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j), ctrl);

                    (ctrl as DataGridViewConfigItem).SelectionChanged += new EventHandler(PanelSources_ConfigItemSelectionChanged);
                    (ctrl as DataGridViewConfigItem).CellClick += new DataGridViewCellEventHandler(PanelSources_ConfigItemCellClick);
                }
                //Создание "подписи" - наименование библиотеки для GROUP_SOURCES
                ctrl = new Label();
                (ctrl as Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                ctrl.Dock = DockStyle.Bottom;
                m_dictControl.Add(INDEX_CONTROL.LABEL_DLLNAME_GROUPSOURCES, ctrl);
                //Создание кнопки - выбор файла-библиотеки для GROUP_SOURCES
                ctrl = new Button();
                (ctrl as Button).Text = @"...";
                ctrl.Dock = DockStyle.Bottom;
                m_dictControl.Add(INDEX_CONTROL.BUTTON_DLLNAME_GROUPSOURCES, ctrl);
                //Создание объекта для редактирования параметров соединения
                ctrl = new DataGridViewConfigItemProp();                
                //foreach (string headerRow in m_arHeaderRowParameterSources) - будут получены из файла конфигурации (PARS)
                //{
                //    (ctrl as DataGridView).Rows.Add(string.Empty);
                //    (ctrl as DataGridView).Rows[(ctrl as DataGridView).RowCount - 1].HeaderCell.Value = headerRow;
                //}
                m_dictControl.Add(INDEX_CONTROL.DGV_PARAMETER_SOURCE, ctrl);
                //Создание объекта для редактирования параметров сигнала
                ctrl = new DataGridViewConfigItemProp();
                m_dictControl.Add(INDEX_CONTROL.DGV_PARAMETER_SIGNAL, ctrl);

                initializeLayoutStyle (4, 1);

                //Размещение элементов
                this.SuspendLayout();

                ////Переключатели
                ////Источник
                //ctrl = m_dictControl[INDEX_CONTROL.RADIOBUTTON_SOURCE];
                //this.Controls.Add(ctrl, 0, 0);
                //this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 4);
                ////Назначение
                //ctrl = m_dictControl[INDEX_CONTROL.RADIOBUTTON_DEST];
                //this.Controls.Add(ctrl, 0, 4);
                //this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 4);
                ////Кнопки
                ////Загрузить
                //ctrl = m_dictControl[INDEX_CONTROL.BUTTON_UPDATE];
                //this.Controls.Add(ctrl, 0, 8);
                //this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 5);
                ////Сохранить
                //ctrl = m_dictControl[INDEX_CONTROL.BUTTON_SAVE];
                //this.Controls.Add(ctrl, 0, 13);
                //this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 5);
                
                //Группы источников                
                posGroupBox(INDEX_PANEL.GROUP_SOURCES, 0, 6);
                ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.GROUPBOX + (int)INDEX_PANEL.GROUP_SOURCES)];
                panelGroupBox = ctrl.Controls[0] as HPanelCommon;
                //Label - наименование библиотеки
                ctrlChild = m_dictControl[INDEX_CONTROL.LABEL_DLLNAME_GROUPSOURCES];
                panelGroupBox.Controls.Add(ctrlChild, 0, 7);
                panelGroupBox.SetColumnSpan(ctrlChild, 4); panelGroupBox.SetRowSpan(ctrlChild, 1);
                //Кнопка - наименование библиотеки
                ctrlChild = m_dictControl[INDEX_CONTROL.BUTTON_DLLNAME_GROUPSOURCES];
                panelGroupBox.Controls.Add(ctrlChild, 4, 7);
                panelGroupBox.SetColumnSpan(ctrlChild, 1); panelGroupBox.SetRowSpan(ctrlChild, 1);

                //Источники группы                
                posGroupBox(INDEX_PANEL.SOURCES_OF_GROUP, 1, 4);
                ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.GROUPBOX + (int)INDEX_PANEL.SOURCES_OF_GROUP)];
                panelGroupBox = ctrl.Controls[0] as HPanelCommon;
                //Параметры соединения
                ctrlChild = m_dictControl[INDEX_CONTROL.DGV_PARAMETER_SOURCE];
                panelGroupBox.Controls.Add(ctrlChild, 0, 5);
                panelGroupBox.SetColumnSpan(ctrlChild, 5); panelGroupBox.SetRowSpan(ctrlChild, 3);

                //Группы сигналов
                posGroupBox(INDEX_PANEL.GROUP_SIGNALS, 2, 7);

                //Сигналы группы
                posGroupBox(INDEX_PANEL.SIGNALS_OF_GROUP, 3, 4);
                ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.GROUPBOX + (int)INDEX_PANEL.SIGNALS_OF_GROUP)];
                panelGroupBox = ctrl.Controls[0] as HPanelCommon;
                //Параметры соединения
                ctrlChild = m_dictControl[INDEX_CONTROL.DGV_PARAMETER_SIGNAL];
                panelGroupBox.Controls.Add(ctrlChild, 0, 5);
                panelGroupBox.SetColumnSpan(ctrlChild, 5); panelGroupBox.SetRowSpan(ctrlChild, 3);

                //Применение размещения элементов
                this.ResumeLayout(false);
                this.PerformLayout();

                //ctrl.Dispose ();
            }
            #endregion

            private object []getPreparePars (DataGridViewConfigItem obj, int indx)
            {
                object[] arObjRes = new object[(int)INDEX_PREPARE_PARS.COUNT_INDEX_PREPARE_PARS];
                //Значения для идентификаторов "по умолчанию"
                arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] =
                arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] =
                    string.Empty;

                foreach (KeyValuePair <INDEX_CONTROL, Control> pair in m_dictControl)
                    if (pair.Value.Equals (obj) == true)
                    {
                        arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ] = (INDEX_PANEL)(pair.Key - (int)INDEX_CONTROL.LISTEDIT);

                        break;
                    }
                    else
                        ;

                switch ((INDEX_PANEL)arObjRes[(int)INDEX_PREPARE_PARS.KEY_OBJ])
                {
                    case INDEX_PANEL.GROUP_SOURCES:
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] = m_dictIds[INDEX_PANEL.GROUP_SOURCES][indx];
                        //arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] = "по умолчанию"
                        break;
                    case INDEX_PANEL.SOURCES_OF_GROUP:
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] = m_dictIds[INDEX_PANEL.GROUP_SOURCES][getConfigItem(INDEX_PANEL.GROUP_SOURCES).SelectedRows[0].Index];
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] = m_dictIds[INDEX_PANEL.SOURCES_OF_GROUP][indx];
                        break;
                    case INDEX_PANEL.GROUP_SIGNALS:
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] = m_dictIds[INDEX_PANEL.GROUP_SIGNALS][indx];
                        //arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] = "по умолчанию"
                        break;
                    case INDEX_PANEL.SIGNALS_OF_GROUP:
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] = m_dictIds[INDEX_PANEL.GROUP_SIGNALS][getConfigItem(INDEX_PANEL.GROUP_SIGNALS).SelectedRows[0].Index];
                        arObjRes[(int)INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] = m_dictIds[INDEX_PANEL.SIGNALS_OF_GROUP][indx];
                        break;
                    default:
                        break;
                }

                return arObjRes;
            }
            
            private void clearValues (INDEX_PANEL indxPanel)
            {
                DataGridViewConfigItem ctrl = getConfigItem(indxPanel);
                ctrl.Rows.Clear ();
            }

            private void clearValues(INDEX_CONTROL indxCtrl)
            {
                (m_dictControl[indxCtrl] as DataGridView).Rows.Clear();
            } 

            private void PanelSources_ConfigItemSelectionChanged(object obj, EventArgs ev)
            {
                //Проверить наличие возможности выбора строки
                if ((obj as DataGridView).SelectedRows.Count > 0)
                {
                    //Подготовить параметры для передачи "родительской" панели
                    object[] arPreparePars = getPreparePars(obj as DataGridViewConfigItem, (obj as DataGridView).SelectedRows[0].Index);

                    switch ((INDEX_PANEL)arPreparePars[(int)INDEX_PREPARE_PARS.KEY_OBJ])
                    {
                        case INDEX_PANEL.GROUP_SOURCES:
                            clearValues(INDEX_PANEL.SOURCES_OF_GROUP);
                            clearValues(INDEX_CONTROL.DGV_PARAMETER_SOURCE);
                            break;
                        case INDEX_PANEL.SOURCES_OF_GROUP:
                            clearValues(INDEX_CONTROL.DGV_PARAMETER_SOURCE);
                            break;
                        case INDEX_PANEL.GROUP_SIGNALS:
                            clearValues(INDEX_PANEL.SIGNALS_OF_GROUP);
                            clearValues(INDEX_CONTROL.DGV_PARAMETER_SIGNAL);
                            break;
                        case INDEX_PANEL.SIGNALS_OF_GROUP:
                            clearValues(INDEX_CONTROL.DGV_PARAMETER_SIGNAL);
                            break;
                        default:
                            break;
                    }

                    arPreparePars[(int)INDEX_PREPARE_PARS.OBJ] = obj;
                    arPreparePars[(int)INDEX_PREPARE_PARS.KEY_EVT] = KEY_EVENT.SELECTION_CHANGED;

                    //Отправить сообщение "родительской" панели (для дальнейшей ретрансляции)
                    DataAskedHost(arPreparePars);
                }
                else
                    ; //Нет выбранных строк
            }

            private void PanelSources_ConfigItemCellClick(object obj, DataGridViewCellEventArgs ev)
            {
            }

            void posGroupBox(INDEX_PANEL indx, int iColumn, int rowSpan)
            {
                TableLayoutPanel panelGroupBox = null;
                Control ctrl = null
                    , ctrlChild = null;

                //ГроупБокс
                ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.GROUPBOX + (int)indx)];
                this.Controls.Add(ctrl, iColumn, 0);
                panelGroupBox = new PanelCommonULoader(5, 8);
                ctrl.Controls.Add(panelGroupBox);
                //Панель
                ctrlChild = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.LISTEDIT + (int)indx)];
                panelGroupBox.Controls.Add(ctrlChild, 0, 0);
                panelGroupBox.SetColumnSpan(ctrlChild, 5); panelGroupBox.SetRowSpan(ctrlChild, rowSpan);
                //TextBox
                ctrlChild = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.TEXTBOX + (int)indx)];
                panelGroupBox.Controls.Add(ctrlChild, 0, rowSpan);
                panelGroupBox.SetColumnSpan(ctrlChild, 4); panelGroupBox.SetRowSpan(ctrlChild, 1);
                //Кнопка
                ctrlChild = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.BUTTON + (int)indx)];
                panelGroupBox.Controls.Add(ctrlChild, 4, rowSpan);
                panelGroupBox.SetColumnSpan(ctrlChild, 1); panelGroupBox.SetRowSpan(ctrlChild, 1);
            }

            /// <summary>
            /// Получить объект со списком групп (элементов групп)
            /// </summary>
            /// <param name="indxConfig">Индекс панели</param>
            /// <param name="indxPanel">Индекс типа объекта</param>
            /// <returns>Объект со списком групп</returns>
            private DataGridViewConfigItem getConfigItem(INDEX_PANEL indxPanel)
            {
                int indxCtrl;

                indxCtrl = (int)PanelSources.INDEX_PANEL_CONTROL.LISTEDIT * (int)PanelSources.INDEX_PANEL.COUNT_INDEX_PANEL + (int)indxPanel;
                return this.m_dictControl[(PanelSources.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem;
            }

            private Dictionary <INDEX_PANEL, string []> m_dictIds;

            public void FillConfigItem(INDEX_PANEL indxPanel, string[,] rows)
            {
                if (m_dictIds == null)
                    m_dictIds = new Dictionary<INDEX_PANEL, string[]>();
                else
                    ;

                int cnt = rows.GetLength (1)
                    , j = -1;

                if (m_dictIds.Keys.Contains(indxPanel) == false)
                    m_dictIds.Add(indxPanel, new string[cnt]);
                else
                    if (!(m_dictIds[indxPanel].Length == cnt))
                        m_dictIds[indxPanel] = new string[cnt];
                    else
                        ;

                DataGridViewConfigItem cfgItem = getConfigItem(indxPanel) as DataGridViewConfigItem;
                if (!(rows == null))
                {
                    j = 0;
                    for (j = 0; j < cnt; j++)
                    {
                        m_dictIds[indxPanel][j] = rows[0, j];
                        try
                        {
                            ////Вариант №1
                            //(cfgItem as DataGridView).Rows.Add(new object[] { rows[1, j], @"-" });
                            ////Вариант №2
                            //(cfgItem as DataGridView).Rows.Add(1);
                            //(cfgItem as DataGridView).Rows[j].Cells[0].Value = rows[1, j];
                            //(cfgItem as DataGridView).Rows[j].Cells[1].Value = @"-";
                            //Вариант №3
                            (cfgItem as DataGridViewConfigItem).AddRow(new object[] { rows[1, j], @"-" });
                        }
                        catch (ArgumentException e)
                        {
                            Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FillConfigItem (IndexConfig=" + IndexConfig + @") - indxPanel=" + indxPanel.ToString() + @" - ...");
                        }
                    }
                }
                else
                    ;
            }

            public void FillConfigItem(INDEX_PANEL indxPanel, string[] rows)
            {
                DataGridViewConfigItem cfgItem = getConfigItem(indxPanel);
                if (! (rows == null))
                    foreach (string row in rows)
                        cfgItem.Rows.Add(new object[] { row, @"-" });
                else
                    ;
            }

            private string IndexConfig
            {
                get { return ((INDEX_SRC)(Parent as PanelConfig).Controls.IndexOf(this)).ToString(); }
            }

            /// <summary>
            /// Получить объект со свойствами элемента группы
            /// </summary>
            /// <param name="indxConfig">Индекс панели (источник, назначение)</param>
            /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
            /// <returns>Объект со списком групп (элементов)</returns>
            private DataGridView getConfigItemProp(PanelSources.INDEX_PANEL indxPanel)
            {
                int indxCtrl = -1;

                switch (indxPanel)
                {
                    case PanelSources.INDEX_PANEL.SOURCES_OF_GROUP:
                        indxCtrl = (int)PanelSources.INDEX_CONTROL.DGV_PARAMETER_SOURCE;
                        break;
                    case PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP:
                        indxCtrl = (int)PanelSources.INDEX_CONTROL.DGV_PARAMETER_SIGNAL;
                        break;
                    case PanelSources.INDEX_PANEL.GROUP_SOURCES:
                    case PanelSources.INDEX_PANEL.GROUP_SIGNALS:
                    default:
                        throw new Exception(@"PanelSources::getConfigItemProp (" + IndexConfig + @", " + indxPanel.ToString() + @") - ...");
                }

                if (!(indxCtrl < 0))
                    return this.m_dictControl[(PanelSources.INDEX_CONTROL)indxCtrl] as DataGridView;
                else
                    return null;
            }

            /// <summary>
            /// Заполнить значениями объект с наименованиями параметров элементов групп (истоников, сигналов)
            /// </summary>
            /// <param name="indxConfig">Индекс панели конфигурации</param>
            /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
            /// <param name="rows">Массив строк для заполнения</param>
            public void FillConfigItemPars(PanelSources.INDEX_PANEL indxPanel, string[] rows)
            {
                //Получить объект для отображения строк
                DataGridView cfgItem = getConfigItemProp(indxPanel);

                //Проверить наличие строк для отображения
                if ((!(rows == null))
                    && (cfgItem.Columns.Count > 0))
                {
                    int i = 0;
                    foreach (string strHeader in rows)
                    {
                        //Проверить возможность отображения параметра
                        if (strHeader.Equals(string.Empty) == false)
                        {
                            cfgItem.Rows.Add(string.Empty); //Добавить строку
                            cfgItem.Rows[i++].HeaderCell.Value = strHeader; //Отобразить наименование параметра
                        }
                        else
                            //Исключение - пустых параметров не существует
                            throw new Exception(@"PanelConfig::fillConfigItemPars (" + IndexConfig + @", " + indxPanel.ToString() + @") - ...");
                    }
                }
                else
                    ;
            }

            /// <summary>
            /// Заполнить значениями объект со значениями параметров элементов групп (истоников, сигналов)
            /// </summary>
            /// <param name="indxConfig">Индекс панели конфигурации</param>
            /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
            /// <param name="rows">Массив строк для заполнения</param>
            public void FillConfigItemProp(PanelSources.INDEX_PANEL indxPanel, string[] rows)
            {
                DataGridView cfgItem = getConfigItemProp(indxPanel);

                if (!(rows == null))
                {
                    int i = 0;
                    foreach (string val in rows)
                    {
                        cfgItem.Rows[i++].Cells[0].Value = val;
                    }
                }
                else
                    ;
            }
        }

        //Количество столбцов, строк в "сетке" 'HPanelCommon'
        static int COL_COUNT = 1
            , ROW_COUNT = 2;

        //Массив объектов для контроля внесения изменений в элементах интерфейса
        static HMark []s_arMarkChanged;

        /// <summary>
        /// Конструктор -
        /// </summary>
        public PanelConfig()
            : base(COL_COUNT, ROW_COUNT)
        {
            InitializeComponent();

            initialize();
        }

        /// <summary>
        /// Конструктор (с парметром) - 
        /// </summary>
        /// <param name="container">Объект-родитель по отношению к создаваемому объекту</param>
        public PanelConfig(IContainer container)
            : base(container, COL_COUNT, ROW_COUNT)
        {
            container.Add(this);

            InitializeComponent();

            initialize();
        }

        /// <summary>
        /// Инициализация "пользовательских" данных 
        /// </summary>
        /// <returns>Результат выполнения инициализации</returns>
        private int initialize()
        {
            int iRes = 0;

            s_arMarkChanged = new HMark [(int)INDEX_SRC.COUNT_INDEX_SRC];
            s_arMarkChanged [(int)INDEX_SRC.SOURCE] = new HMark ();
            s_arMarkChanged[(int)INDEX_SRC.DEST] = new HMark();

            return iRes;
        }

        private void fillConfigItem(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[,] rows)
        {
            (this.Controls[(int)indxConfig] as PanelSources).FillConfigItem (indxPanel, rows);
        } 
        /// <summary>
        /// Заполнить значениями объект со списком групп (элементов групп) (истоников, сигналов)
        /// </summary>
        /// <param name="indxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillConfigItem(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            (this.Controls[(int)indxConfig] as PanelSources).FillConfigItem(indxPanel, rows);
        }
        /// <summary>
        /// Заполнить значениями объект с наименованиями параметров элементов групп (истоников, сигналов)
        /// </summary>
        /// <param name="indxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillConfigItemPars(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            (this.Controls[(int)indxConfig] as PanelSources).FillConfigItemPars(indxPanel, rows);
        }

        /// <summary>
        /// Заполнить значениями объект со значениями параметров элементов групп (истоников, сигналов)
        /// </summary>
        /// <param name="indxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillConfigItemProp(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            (this.Controls[(int)indxConfig] as PanelSources).FillConfigItemProp(indxPanel, rows);
        }

        /// <summary>
        /// Обработчик события получения данных по запросу (выполняется в текущем потоке)
        /// </summary>
        /// <param name="obj">Результат, полученный по запросу (массив 'object')</param>
        private void onEvtDataRecievedHost(object obj)
        {
            //Обработанное состояние 
            int state = Int32.Parse((obj as object[])[0].ToString());
            //Параметры (массив) в 1-ом элементе результата
            object par = (obj as object[])[1];            

            switch (state)
            {
                case (int)HHandlerQueue.StatesMachine.LIST_GROUP_SOURCES: //Заполнить на панели (источник, назаначение) - группы источников
                    fillConfigItem(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.GROUP_SOURCES, (par as object[])[(int)INDEX_SRC.SOURCE] as string[,]);
                    fillConfigItem(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.GROUP_SOURCES, (par as object[])[(int)INDEX_SRC.DEST] as string[,]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS: //Заполнить на панели (источник, назаначение) - элементы в группе источников
                    fillConfigItem(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.SOURCES_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_PARS: //Заполнить на панели источник - наименования параметров элементов в группе источников
                    fillConfigItemPars(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.SOURCES_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_PROP: //Заполнить на панели источник - параметры элементов в группе источников
                    fillConfigItemProp(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.SOURCES_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_GROUP_SIGNALS: //Заполнить на панели источник - группы сигналов
                    fillConfigItem(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.GROUP_SIGNALS, (par as object[])[(int)INDEX_SRC.SOURCE] as string[,]);
                    fillConfigItem(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.GROUP_SIGNALS, (par as object[])[(int)INDEX_SRC.DEST] as string[,]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS: //Заполнить на панели источник - элементы в группе сигналов
                    fillConfigItem(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS: //Заполнить на панели источник - наименования параметров элементов в группе сигналов
                    fillConfigItemPars(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP: //Заполнить на панели источник - параметры элементов в группе сигналов
                    fillConfigItemProp(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS: //Заполнить на панели (назаначение) - элементы в группе источников
                    fillConfigItem(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.SOURCES_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_PARS: //Заполнить на панели назначение - наименования параметров элементов в группе источников
                    fillConfigItemPars(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.SOURCES_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_PROP: //Заполнить на панели назначение - параметры элементов в группе источников
                    fillConfigItemProp(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.SOURCES_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS: //Заполнить на панели назначение - элементы в группе сигналов
                    fillConfigItem(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS: //Заполнить на панели назначение - наименования параметров элементов в группе сигналов
                    fillConfigItemPars(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP: //Заполнить на панели назначение - параметры элементов в группе сигналов
                    fillConfigItemProp(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP, (par as object[]) as string[]);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработчик события получения данных по запросу (выполняется в потоке получения результата)
        /// </summary>
        /// <param name="obj">Результат, полученный по запросу</param>
        public override void OnEvtDataRecievedHost(object obj)
        {
            if (InvokeRequired == true)
                if (IsHandleCreated == true)
                    this.BeginInvoke(new DelegateObjectFunc(onEvtDataRecievedHost), obj);
                else
                    throw new Exception(@"PanelConfig::OnEvtDataRecievedHost () - IsHandleCreated==" + IsHandleCreated);
            else
                onEvtDataRecievedHost (obj);

            base.OnEvtDataRecievedHost(obj);
        }

        /// <summary>
        /// Активацирует/деактивирует панель
        /// </summary>
        /// <param name="active">Признак операции: активация/деактивация</param>
        /// <returns>Результат выполнения: было ли изменено состояние</returns>
        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (IsFirstActivated == true)
                DataAskedHost(new object[] { new object [] { (int)HHandlerQueue.StatesMachine.LIST_GROUP_SOURCES /*, без параметров*/ }
                                        , new object [] { (int)HHandlerQueue.StatesMachine.LIST_GROUP_SIGNALS /*, без параметров*/ }
                                        });
            else
                ;

            return bRes;
        }

        /// <summary>
        /// Обработчик события 'EvtDataAskedHost' от панелей (источник, назначение)
        /// </summary>
        /// <param name="obj">Параметр для передачи-массив (0-панель, 1-индекс группы источников, 2-индекс группы сигналов)</param>
        private void OnEvtDataAskedPanelConfig_PanelSources (object par)
        {
            object []pars = (par as EventArgsDataHost).par[0] as object [];
            //Массив параметров для передачи
            object[] arObjToDataHost = new object [] { };
            //Событие для постановки в очередь обработки событий
            HHandlerQueue.StatesMachine state = HHandlerQueue.StatesMachine.UNKNOWN;
            //Определить панель-инициатор сообщения
            INDEX_SRC indxConfig= (INDEX_SRC)this.Controls.GetChildIndex(pars[(int)PanelSources.INDEX_PREPARE_PARS.OBJ] as PanelSources);

            switch ((KEY_EVENT)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_EVT])
            {
                case KEY_EVENT.SELECTION_CHANGED:
                    switch (indxConfig)
                    {
                        case INDEX_SRC.SOURCE:
                            switch ((PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ])
                            {
                                case PanelSources.INDEX_PANEL.GROUP_SOURCES:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список параметров соединения для источника из выбранной группы (строки)
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_PARS
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                            , new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //!!! Индекс группы элементов на панели
                                                        ,  pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                        };
                                    break;
                                case PanelSources.INDEX_PANEL.SOURCES_OF_GROUP:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_PROP //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)PanelSources.INDEX_PANEL.GROUP_SOURCES //!!! Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] // выбранная строка в списке элементов группы
                                                }
                                        };
                                    break;
                                case PanelSources.INDEX_PANEL.GROUP_SIGNALS:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список параметров соединения для источника из выбранной группы (строки)
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                            , new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //!!! Индекс группы элементов на панели
                                                        ,  pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                        };
                                    break;
                                case PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)PanelSources.INDEX_PANEL.GROUP_SIGNALS //!!! Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] // выбранная строка в списке элементов группы
                                                }
                                        };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case INDEX_SRC.DEST:
                            switch ((PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ])
                            {
                                case PanelSources.INDEX_PANEL.GROUP_SOURCES:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список параметров соединения для источника из выбранной группы (строки)
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_PARS
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                            , new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //!!! Индекс группы элементов на панели
                                                        ,  pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                        };
                                    break;
                                case PanelSources.INDEX_PANEL.SOURCES_OF_GROUP:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_PROP //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)PanelSources.INDEX_PANEL.GROUP_SOURCES //!!! Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] // выбранная строка в списке элементов группы
                                                }
                                        };
                                    break;
                                case PanelSources.INDEX_PANEL.GROUP_SIGNALS:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список параметров соединения для источника из выбранной группы (строки)
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                            , new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)(PanelSources.INDEX_PANEL)pars[(int)PanelSources.INDEX_PREPARE_PARS.KEY_OBJ] //!!! Индекс группы элементов на панели
                                                        ,  pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                }
                                        };
                                    break;
                                case PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP:
                                    arObjToDataHost = new object[] {
                                            new object [] //Список источников для выбранной строки
                                                {
                                                    HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP //Состояние для обработки
                                                        //с параметрами
                                                        , (int)indxConfig //Индекс панели
                                                        , (int)PanelSources.INDEX_PANEL.GROUP_SIGNALS //!!! Индекс группы элементов на панели
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_GROUP_SEL] // выбранная строка группы элементов
                                                        , pars[(int)PanelSources.INDEX_PREPARE_PARS.ID_OBJ_ITEM_SEL] // выбранная строка в списке элементов группы
                                                }
                                        };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case KEY_EVENT.CELL_CLICK:
                    switch (indxConfig)
                    {
                        case INDEX_SRC.SOURCE:
                            break;
                        case INDEX_SRC.DEST:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            //Ретрансляция для постановки в очередь
            DataAskedHost(arObjToDataHost);
        }
    }

    /// <summary>
    /// Обязательная часть реализации класса
    /// </summary>
    partial class PanelConfig
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

        //struct Position
        //{
        //    public Point loc; public Size sz;
        //    public Position(Point loc, Size sz) { this.loc = loc; this.sz = sz; }
        //};

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            //Размещение элементов
            this.SuspendLayout();

            this.Controls.Add(new PanelSources(), 0, 0);
            //(this.Controls[(int)INDEX_SRC.SOURCE] as PanelCommon). ;
            this.Controls.Add(new PanelSources(), 0, 1);
            //(this.Controls[(int)INDEX_SRC.DEST] as PanelCommon). ;

            //Применение размещения элементов
            this.ResumeLayout(false);
            this.PerformLayout();

            (this.Controls[0] as PanelSources).EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedPanelConfig_PanelSources);
            (this.Controls[1] as PanelSources).EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedPanelConfig_PanelSources);
        }

        #endregion
    }
}
