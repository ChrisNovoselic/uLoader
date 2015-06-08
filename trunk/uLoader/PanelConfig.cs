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
        private class PanelSources : HPanelCommon
        {
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

        /// <summary>
        /// Получить объект со списком групп (элементов групп)
        /// </summary>
        /// <param name="indxConfig">Индекс панели</param>
        /// <param name="indxPanel">Индекс типа объекта</param>
        /// <returns>Объект со списком групп</returns>
        private DataGridViewConfigItem getConfigItem(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel)
        {
            PanelSources panelSrc;
            int indxCtrl;            

            panelSrc = this.Controls[(int)indxConfig] as PanelSources;
            indxCtrl = (int)PanelSources.INDEX_PANEL_CONTROL.LISTEDIT * (int)PanelSources.INDEX_PANEL.COUNT_INDEX_PANEL + (int)indxPanel;
            return panelSrc.m_dictControl[(PanelSources.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem;
        }

        /// <summary>
        /// Получить объект со свойствами элемента группы
        /// </summary>
        /// <param name="indxConfig">Индекс панели (источник, назначение)</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <returns>Объект со списком групп (элементов)</returns>
        private DataGridView getConfigItemProp(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel)
        {
            PanelSources panelSrc;
            int indxCtrl = -1;

            panelSrc = this.Controls[(int)indxConfig] as PanelSources;

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
                    throw new Exception(@"PanelConfig::getConfigItemProp (" + indxConfig.ToString () + @", " + indxPanel.ToString () + @") - ...");
            }

            if (!(indxCtrl < 0))
                return panelSrc.m_dictControl[(PanelSources.INDEX_CONTROL)indxCtrl] as DataGridView;
            else
                return null;
        }
        private void fillConfigItem(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[,] rows)
        {
            DataGridViewConfigItem cfgItem = getConfigItem(indxConfig, indxPanel);
            if (!(rows == null))
                foreach (string row in rows)
                    cfgItem.Rows.Add(new object[] { row, @"-" });
            else
                ;
        } 
        /// <summary>
        /// Заполнить значениями объект со списком групп (элементов групп) (истоников, сигналов)
        /// </summary>
        /// <param name="indxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillConfigItem(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            DataGridViewConfigItem cfgItem = getConfigItem(indxConfig, indxPanel);
            if (! (rows == null))
                foreach (string row in rows)
                    cfgItem.Rows.Add(new object[] { row, @"-" });
            else
                ;
        }
        /// <summary>
        /// Заполнить значениями объект с наименованиями параметров элементов групп (истоников, сигналов)
        /// </summary>
        /// <param name="indxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillConfigItemPars(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            //Получить объект для отображения строк
            DataGridView cfgItem = getConfigItemProp(indxConfig, indxPanel);

            //Проверить наличие строк для отображения
            if ((! (rows == null))
                && (cfgItem.Columns.Count > 0))
            {
                int i = 0;
                foreach (string strHeader in rows)
                {
                    //Проверить возможность отображения параметра
                    if (strHeader.Equals (string.Empty) == false)
                    {
                        cfgItem.Rows.Add (string.Empty); //Добавить строку
                        cfgItem.Rows[i++].HeaderCell.Value = strHeader; //Отобразить наименование параметра
                    }
                    else
                        //Исключение - пустых параметров не существует
                        throw new Exception(@"PanelConfig::fillConfigItemPars (" + indxConfig.ToString () + @", " + indxPanel.ToString () + @") - ...");
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
        private void fillConfigItemProp(INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel, string[] rows)
        {
            DataGridView cfgItem = getConfigItemProp(indxConfig, indxPanel);

            if (! (rows == null))
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
                    fillConfigItem(INDEX_SRC.SOURCE, PanelSources.INDEX_PANEL.GROUP_SIGNALS, (par as object[])[(int)INDEX_SRC.SOURCE] as string[]);
                    fillConfigItem(INDEX_SRC.DEST, PanelSources.INDEX_PANEL.GROUP_SIGNALS, (par as object[])[(int)INDEX_SRC.DEST] as string[]);
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
        /// Назначить обработчик события при изменении выбора в элементе интерфейса (DataGridViewConfigItem)
        /// </summary>
        /// <param name="dgvConfigItem">Элемент интерфейса</param>
        /// <param name="indxConfig">Индекс панели</param>
        /// <param name="indxPanel">Индекс группы элементов на панели</param>
        private void setHandler_dgvConfigItemSelectionChanged (DataGridViewConfigItem dgvConfigItem, INDEX_SRC indxConfig, PanelSources.INDEX_PANEL indxPanel)
        {
            //Обработчик для "безличного" обращения
            EventHandler delegateHandler = null;

            switch (indxConfig)
            {
                case INDEX_SRC.SOURCE: //Индекс панели конфигурации - источник
                    switch (indxPanel)
                    {
                        case PanelSources.INDEX_PANEL.GROUP_SOURCES: //Индекс панели групп источников
                            delegateHandler = new EventHandler (panelConfig_dgvConfigItemSrcGroupSourcesSelectionChanged);
                            break;
                        case PanelSources.INDEX_PANEL.SOURCES_OF_GROUP: //Индекс панели элемента группы источников
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemSrcSourcesOfGroupSelectionChanged);
                            break;
                        case PanelSources.INDEX_PANEL.GROUP_SIGNALS: //Индекс панели групп сигналов
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemSrcGroupSignalsSelectionChanged);
                            break;
                        case PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP: //Индекс панели элемента группы сигналов
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemSrcSignalsOfGroupSelectionChanged);
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_SRC.DEST: //Индекс панели конфигурации - назначение
                    switch (indxPanel)
                    {
                        case PanelSources.INDEX_PANEL.GROUP_SOURCES: //Индекс панели групп источников
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemDestGroupSourcesSelectionChanged);
                            break;
                        case PanelSources.INDEX_PANEL.SOURCES_OF_GROUP: //Индекс панели элемента группы источников
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemDestSourcesOfGroupSelectionChanged);
                            break;
                        case PanelSources.INDEX_PANEL.GROUP_SIGNALS: //Индекс панели групп сигналов
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemDestGroupSignalsSelectionChanged);
                            break;
                        case PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP: //Индекс панели элемента группы сигналов
                            delegateHandler = new EventHandler(panelConfig_dgvConfigItemDestSignalsOfGroupSelectionChanged);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            if (! (delegateHandler == null))
                dgvConfigItem.SelectionChanged += delegateHandler;
            else
                ;
        }

        /// <summary>
        /// Универсальный "отправитель" запроса на получение данных (строк) для отображения
        ///  при возникновении события 'SelectionChanged'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие 'SelectionChanged'</param>
        /// <param name="ev">Аргументы события</param>
        /// <param name="infxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanelToClear">Индекс панели, напрямую зависимой от инициировавшей событие (для удаления всех элементов)</param>
        /// <param name="indxPanelSelected">Индекс</param>
        /// <param name="statePars">Состояние для обработки (параметры элементов группы)</param>
        /// <param name="stateItems">Состояние для обработки (элементы группы)</param>
        private void panelConfig_dgvConfigGroupSelectionChanged(object obj, EventArgs ev
            , INDEX_SRC infxConfig
            , PanelSources.INDEX_PANEL indxPanelToClear
            , PanelSources.INDEX_PANEL indxPanelSelected
            , HHandlerQueue.StatesMachine statePars
            , HHandlerQueue.StatesMachine stateItems)
        {
            //Очистить список с источниками
            getConfigItem(infxConfig, indxPanelToClear).Rows.Clear();
            //Очистить список с параметрами соединения
            getConfigItemProp(infxConfig, indxPanelToClear).Rows.Clear();

            int indxItemSelected = (int)getConfigItem (infxConfig, indxPanelSelected).SelectedRows [0].Index;
            //Запросить
            DataAskedHost(new object[] {
                                    new object [] //Список параметров соединения для источника из выбранной группы (строки)
                                        {
                                            statePars
                                                //с параметрами
                                                , (int)infxConfig //Индекс панели
                                                , (int)indxPanelSelected //Индекс группы элементов на панели
                                                , indxItemSelected // выбранная строка группы элементов
                                        }
                                    , new object [] //Список источников для выбранной строки
                                        {
                                            (int)stateItems //Состояние для обработки
                                                //с параметрами
                                                , (int)infxConfig //Индекс панели
                                                , (int)indxPanelSelected //Индекс группы элементов на панели
                                                ,  indxItemSelected // выбранная строка группы элементов
                                        }
                                }
                            );
        }

        /// <summary>
        /// Универсальный "отправитель" запроса на получение данных (строк) для отображения
        ///  при возникновении события 'SelectionChanged'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие 'SelectionChanged'</param>
        /// <param name="ev">Аргументы события</param>
        /// <param name="indxConfig">Индекс панели конфигурации</param>
        /// <param name="indxPanelGroup">Индекс группы элементов</param>
        /// <param name="indxPanelItem">Индекс элементов группы</param>
        /// <param name="stateProp">Состояние для обработки</param>
        private void panelConfig_dgvConfigItemSelectionChanged(object obj, EventArgs ev
            , INDEX_SRC indxConfig
            , PanelSources.INDEX_PANEL indxPanelGroup
            , PanelSources.INDEX_PANEL indxPanelItem
            , HHandlerQueue.StatesMachine stateProp)
        {
            int indxPanelGroupSel = -1
                , indxPanelItemSel = -1;

            //Получить объект с группами
            DataGridViewConfigItem cfgItem = getConfigItem (indxConfig, indxPanelGroup);
            //Проверить наличие выбора
            if (! (cfgItem.SelectedRows.Count == 1))
                //Не выполнять без выбранной строки
                return;
            else
                indxPanelGroupSel = cfgItem.SelectedRows[0].Index;

            //Получить объект с элементами
            cfgItem = getConfigItem(indxConfig, indxPanelItem);
            //Проверить наличие выбора
            if (!(cfgItem.SelectedRows.Count == 1))
                //Не выполнять без выбранной строки
                return;
            else
                indxPanelItemSel = cfgItem.SelectedRows[0].Index;
            
            //Запросить
            DataAskedHost(new object[] {
                                    new object [] //Список источников для выбранной строки
                                        {
                                            (int)stateProp //Состояние для обработки
                                                //с параметрами
                                                , (int)indxConfig //Индекс панели
                                                , (int)indxPanelGroup //Индекс группы элементов на панели
                                                , indxPanelGroupSel // выбранная строка группы элементов
                                                , indxPanelItemSel // выбранная строка в списке элементов группы
                                        }
                                }
                            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель источник - группы источников
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemSrcGroupSourcesSelectionChanged (object obj, EventArgs ev)
        {
            panelConfig_dgvConfigGroupSelectionChanged(obj, ev
                , INDEX_SRC.SOURCE
                , PanelSources.INDEX_PANEL.SOURCES_OF_GROUP
                , PanelSources.INDEX_PANEL.GROUP_SOURCES
                , HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_PARS
                , HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS
            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель источник - элемент в группе источников
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemSrcSourcesOfGroupSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigItemSelectionChanged (obj, ev
                , INDEX_SRC.SOURCE
                , PanelSources.INDEX_PANEL.GROUP_SOURCES
                , PanelSources.INDEX_PANEL.SOURCES_OF_GROUP
                , HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCE_PROP
            );            
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель источник - группы сигналов
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemSrcGroupSignalsSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigGroupSelectionChanged(obj, ev
                , INDEX_SRC.SOURCE
                , PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP
                , PanelSources.INDEX_PANEL.GROUP_SIGNALS //??? Передать во-вне НЕИЗВЕСТНый идентификатор
                , HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS
                , HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS
            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель источник - элемент в группе сигналов
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemSrcSignalsOfGroupSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigItemSelectionChanged(obj, ev
                , INDEX_SRC.SOURCE
                , PanelSources.INDEX_PANEL.GROUP_SIGNALS //??? Передать во-вне НЕИЗВЕСТНый идентификатор
                , PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP
                , HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP
            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель назначение - группы источников
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemDestGroupSourcesSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigGroupSelectionChanged(obj, ev
                , INDEX_SRC.DEST
                , PanelSources.INDEX_PANEL.SOURCES_OF_GROUP
                , PanelSources.INDEX_PANEL.GROUP_SOURCES
                , HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_PARS
                , HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS
            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель назначение - элемент в группе источников
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemDestSourcesOfGroupSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigItemSelectionChanged(obj, ev
                , INDEX_SRC.DEST
                , PanelSources.INDEX_PANEL.GROUP_SOURCES
                , PanelSources.INDEX_PANEL.SOURCES_OF_GROUP
                , HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCE_PROP
            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель назначение - группы сигналов
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemDestGroupSignalsSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigGroupSelectionChanged(obj, ev
                , INDEX_SRC.DEST
                , PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP
                , PanelSources.INDEX_PANEL.GROUP_SIGNALS //??? Передать во-вне НЕИЗВЕСТНый идентификатор
                , HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS
                , HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS
            );
        }

        /// <summary>
        /// Обработчик события 'SelectionChanged' Панель назначение - элемент в группе сигналов
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргументы события</param>
        private void panelConfig_dgvConfigItemDestSignalsOfGroupSelectionChanged(object obj, EventArgs ev)
        {
            panelConfig_dgvConfigItemSelectionChanged(obj, ev
                , INDEX_SRC.DEST
                , PanelSources.INDEX_PANEL.GROUP_SIGNALS //??? Передать во-вне НЕИЗВЕСТНый идентификатор
                , PanelSources.INDEX_PANEL.SIGNALS_OF_GROUP
                , HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP
            );
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

            PanelSources panelSrc;
            int indxCtrl;

            for (INDEX_SRC indxConfig = INDEX_SRC.SOURCE; indxConfig < INDEX_SRC.COUNT_INDEX_SRC; indxConfig ++)
            {
                panelSrc = this.Controls[(int)indxConfig] as PanelSources;

                for (PanelSources.INDEX_PANEL indxPanel  = PanelSources.INDEX_PANEL.GROUP_SOURCES; indxPanel < PanelSources.INDEX_PANEL.COUNT_INDEX_PANEL; indxPanel ++)
                {                    
                    indxCtrl = (int)PanelSources.INDEX_PANEL_CONTROL.LISTEDIT * (int)PanelSources.INDEX_PANEL.COUNT_INDEX_PANEL + (int)indxPanel;
                    setHandler_dgvConfigItemSelectionChanged (panelSrc.m_dictControl[(PanelSources.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem
                        , indxConfig
                        , indxPanel
                        );
                }
            }

            //Применение размещения элементов
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
