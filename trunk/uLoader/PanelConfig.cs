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
    public partial class PanelConfig : PanelCommon
    {
        private class PanelSources : PanelCommon
        {
            static int COL_DIV = 1, ROW_DIV = 1
                , COL_COUNT = 4 / COL_DIV, ROW_COUNT = 1 / ROW_DIV;

            //Индексы панелей
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
                , DGV_PARAMETER_SOURCES = BUTTON + INDEX_PANEL.COUNT_INDEX_PANEL
                , LABEL_DLLNAME_GROUPSOURCES
                , BUTTON_DLLNAME_GROUPSOURCES
                , RADIOBUTTON_SOURCE
                , RADIOBUTTON_DEST
                , BUTTON_UPDATE, BUTTON_SAVE
            };
            //Заголовки строк параметров сединения
            static string[] m_arHeaderRowParameterSources =
            {
                @"ID", @"IP", @"PORT", @"DB_NAME", @"UID", @"PASSWORD"
            };
            //Заголовки строк панелей
            static string[] m_arHeaderPanelListEdit =
            {
                @"Группы источников", @"Источники группы", @"Группы сигналов", @"Сигналы группы"
            };
            //Словарь элементов управления
            public Dictionary<INDEX_CONTROL, Control> m_dictControl;

            public PanelSources() : base (COL_COUNT, ROW_COUNT)
            {
                InitializeComponent();
            }

            public PanelSources(IContainer container)
                : base(COL_COUNT, ROW_COUNT)
            {
                container.Add(this);

                InitializeComponent();
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
                    m_dictControl.Add((INDEX_CONTROL)(i * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)
                        , new DataGridViewConfigItem(
                        m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)] as TextBox
                        , m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)] as Button
                    ));
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
                ctrl = new DataGridView();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DataGridView).Columns.Add(@"Value", @"Значение");
                (ctrl as DataGridView).Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                //(ctrl as DataGridView).Columns[0].Frozen = true;
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).RowHeadersWidth = 76;
                foreach (string headerRow in m_arHeaderRowParameterSources)
                {
                    (ctrl as DataGridView).Rows.Add(string.Empty);
                    (ctrl as DataGridView).Rows[(ctrl as DataGridView).RowCount - 1].HeaderCell.Value = headerRow;
                }
                m_dictControl.Add(INDEX_CONTROL.DGV_PARAMETER_SOURCES, ctrl);

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
                panelGroupBox = ctrl.Controls[0] as PanelCommon;
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
                panelGroupBox = ctrl.Controls[0] as PanelCommon;
                //Параметры соединения
                ctrlChild = m_dictControl[INDEX_CONTROL.DGV_PARAMETER_SOURCES];
                panelGroupBox.Controls.Add(ctrlChild, 0, 5);
                panelGroupBox.SetColumnSpan(ctrlChild, 5); panelGroupBox.SetRowSpan(ctrlChild, 3);

                //Группы сигналов
                posGroupBox(INDEX_PANEL.GROUP_SIGNALS, 2, 7);

                //Сигналы группы
                posGroupBox(INDEX_PANEL.SIGNALS_OF_GROUP, 3, 7);

                //Применение размещения элементов
                this.ResumeLayout(false);
                this.PerformLayout();
            }
            #endregion

            void posGroupBox(INDEX_PANEL indx, int iColumn, int rowSpan)
            {
                TableLayoutPanel panelGroupBox = null;
                Control ctrl = null
                    , ctrlChild = null;

                //ГроупБокс
                ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.GROUPBOX + (int)indx)];
                this.Controls.Add(ctrl, iColumn, 0);
                panelGroupBox = new PanelCommon(5, 8);
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
        }        

        static int COL_COUNT = 1
            , ROW_COUNT = 2;

        //Индексы...
        enum INDEX_CONFIG
        {
            SOURCE,
            DEST
            , COUNT_INDEX_CONFIG
        };        

        public PanelConfig()
            : base(COL_COUNT, ROW_COUNT)
        {
            InitializeComponent();

            initialize();
        }

        public PanelConfig(IContainer container)
            : base(container, COL_COUNT, ROW_COUNT)
        {
            container.Add(this);

            InitializeComponent();

            initialize();
        }

        private int initialize()
        {
            int iRes = 0;

            return iRes;
        }

        private void fillConfigItem(INDEX_CONFIG indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            PanelSources panelSrc;
            int indxCtrl;
            DataGridViewConfigItem cfgItem;

            panelSrc = this.Controls[(int)indxConfig] as PanelSources;
            indxCtrl = (int)PanelSources.INDEX_PANEL_CONTROL.LISTEDIT * (int)PanelSources.INDEX_PANEL.COUNT_INDEX_PANEL + (int)indxPanel;
            cfgItem = panelSrc.m_dictControl[(PanelSources.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem;
            foreach (string row in rows)
                cfgItem.Rows.Add(new object[] { row, @"-" });
        }

        private void onEvtDataRecievedHost(object obj)
        {
            int state = Int32.Parse((obj as object[])[0].ToString());
            object par = (obj as object[])[1];

            

            switch (state)
            {
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCES:
                    fillConfigItem(INDEX_CONFIG.SOURCE, PanelSources.INDEX_PANEL.GROUP_SOURCES, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNALS:
                    fillConfigItem(INDEX_CONFIG.SOURCE, PanelSources.INDEX_PANEL.GROUP_SIGNALS, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES:
                    fillConfigItem(INDEX_CONFIG.DEST, PanelSources.INDEX_PANEL.GROUP_SOURCES, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNALS:
                    fillConfigItem(INDEX_CONFIG.DEST, PanelSources.INDEX_PANEL.GROUP_SIGNALS, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                    break;
                default:
                    break;
            }
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            this.BeginInvoke(new DelegateObjectFunc(onEvtDataRecievedHost), obj);            

            base.OnEvtDataRecievedHost(obj);
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (IsFirstActivated == true)
                DataAskedHost(new int[] { (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCES
                                        , (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNALS
                                        , (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES
                                        , (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNALS
                                        });
            else
                ;

            return bRes;
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
            this.Controls.Add(new PanelSources(), 0, 1);

            //Применение размещения элементов
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
