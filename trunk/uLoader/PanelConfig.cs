﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms; //Control
using System.Drawing; //Point

namespace uLoader
{
    public partial class PanelConfig : PanelCommon
    {
        //Индексы панелей
        enum INDEX_PANEL { GROUP_SOURCES, SOURCES_OF_GROUP, GROUP_SIGNALS, SIGNALS_OF_GROUP
            , COUNT_INDEX_PANEL
        };
        //Индексы групп элементов управления в панелях
        enum INDEX_PANEL_CONTROL { PANEL, TEXTBOX, BUTTON };
        //Индексы элементов управления
        enum INDEX_CONTROL {
            PANEL_LISTEDIT
            , TEXTBOX = PANEL_LISTEDIT + INDEX_PANEL.COUNT_INDEX_PANEL
            , BUTTON = TEXTBOX + INDEX_PANEL.COUNT_INDEX_PANEL
            , DGV_PARAMETER_SOURCES = BUTTON + INDEX_PANEL.COUNT_INDEX_PANEL
            , LABEL_DLLNAME_GROUPSOURCES, BUTTON_DLLNAME_GROUPSOURCES
            , RADIOBUTTON_SOURCE, RADIOBUTTON_DEST
            , BUTTON_UPDATE, BUTTON_SAVE
        };
        //Индексы строк параметров соединения
        enum INDEX_PARAMETER_SOURCE
        {
            ID, IP, PORT, DB_NAME, UID, PASSWORD
            , COUNT_INDEX_PARAMETER_SOURCE
        };
        ////Индексы...
        //enum INDEX_CONFIG
        //{
        //    SOURCE, DEST
        //    , COUNT_INDEX_CONFIG
        //};
        //Заголовки строк параметров сединения
        static string [] m_arHeaderRowParameterSources =
        {
            @"ID", @"IP", @"PORT", @"DB_NAME", @"UID", @"PASSWORD"
        };
        //Заголовки строк панелей
        static string[] m_arHeaderPanelListEdit =
        {
            @"Группы источников", @"Источники группы", @"Группы сигналов", @"Сигналы группы"
        };
        //Словарь элементов управления
        Dictionary<INDEX_CONTROL, Control> m_dictControl;
        
        public PanelConfig() : base (96, 90)
        {
            InitializeComponent();
        }

        public PanelConfig(IContainer container)
            : base(container, 96, 90)
        {
            container.Add(this);

            InitializeComponent();
        }
    }

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

        struct Position
        {
            public Point loc; public Size sz;
            public Position(Point loc, Size sz) { this.loc = loc; this.sz = sz; }
        };

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            Control ctrl; //Для "безличного" обращения к элементу интерфейса
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
            //Создание объектов переключателей
            //Источник
            ctrl = new RadioButton();
            (ctrl as RadioButton).Text = @"Источник";
            (ctrl as RadioButton).Dock = DockStyle.Fill;
            (ctrl as RadioButton).Checked = true;
            m_dictControl.Add(INDEX_CONTROL.RADIOBUTTON_SOURCE, ctrl);
            //Назначение
            ctrl = new RadioButton();
            (ctrl as RadioButton).Text = @"Назначение";
            (ctrl as RadioButton).Dock = DockStyle.Fill;
            m_dictControl.Add(INDEX_CONTROL.RADIOBUTTON_DEST, ctrl);
            //Создание объектов кнопок
            //Загрузить
            ctrl = new Button();
            (ctrl as Button).Text = @"Загрузить";
            (ctrl as Button).Dock = DockStyle.Fill;
            m_dictControl.Add(INDEX_CONTROL.BUTTON_UPDATE, ctrl);
            //Сохранить
            ctrl = new Button();
            (ctrl as Button).Text = @"Сохранить";
            (ctrl as Button).Dock = DockStyle.Fill;
            m_dictControl.Add(INDEX_CONTROL.BUTTON_SAVE, ctrl);
            int i = -1
                , j = -1;
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
            i = (int)INDEX_PANEL_CONTROL.PANEL;
            for (j = 0; j < (int)INDEX_PANEL.COUNT_INDEX_PANEL; j++)
                m_dictControl.Add((INDEX_CONTROL)(i * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)
                    , new PanelListEdit(m_arHeaderPanelListEdit[j]
                    , m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + j)] as TextBox
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
            (ctrl as DataGridView).AllowUserToAddRows = false;
            (ctrl as DataGridView).AllowUserToDeleteRows = false;
            (ctrl as DataGridView).MultiSelect = false;
            (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            (ctrl as DataGridView).RowHeadersWidth = 108;
            foreach (string headerRow in m_arHeaderRowParameterSources)
            {
                (ctrl as DataGridView).Rows.Add(string.Empty);
                (ctrl as DataGridView).Rows[(ctrl as DataGridView).RowCount - 1].HeaderCell.Value = headerRow;
            }
            m_dictControl.Add(INDEX_CONTROL.DGV_PARAMETER_SOURCES, ctrl);

            //Размещение элементов
            this.SuspendLayout();

            //Переключатели
            //Источник
            ctrl = m_dictControl[INDEX_CONTROL.RADIOBUTTON_SOURCE];
            this.Controls.Add(ctrl, 0, 0);
            this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 4);
            //Назначение
            ctrl = m_dictControl[INDEX_CONTROL.RADIOBUTTON_DEST];
            this.Controls.Add(ctrl, 0, 4);
            this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 4);
            //Кнопки
            //Загрузить
            ctrl = m_dictControl[INDEX_CONTROL.BUTTON_UPDATE];
            this.Controls.Add(ctrl, 0, 8);
            this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 5);
            //Сохранить
            ctrl = m_dictControl[INDEX_CONTROL.BUTTON_SAVE];
            this.Controls.Add(ctrl, 0, 13);
            this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 5);
            //Группы источников
            //Панель
            ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.PANEL_LISTEDIT + (int)INDEX_PANEL.GROUP_SOURCES)];
            this.Controls.Add(ctrl, 18, 0);
            this.SetColumnSpan(ctrl, 36); this.SetRowSpan(ctrl, 15);
            //TextBox
            ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.TEXTBOX * (int)INDEX_PANEL.COUNT_INDEX_PANEL + (int)INDEX_PANEL.GROUP_SOURCES)];
            this.Controls.Add(ctrl, 18, 15);
            this.SetColumnSpan(ctrl, 30); this.SetRowSpan(ctrl, 5);
            //Кнопка
            ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_PANEL_CONTROL.BUTTON * (int)INDEX_PANEL.COUNT_INDEX_PANEL + (int)INDEX_PANEL.GROUP_SOURCES)];
            this.Controls.Add(ctrl, 48, 15);
            this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 5);
            //Label - наименование библиотеки
            ctrl = m_dictControl[INDEX_CONTROL.LABEL_DLLNAME_GROUPSOURCES];
            this.Controls.Add(ctrl, 18, 20);
            this.SetColumnSpan(ctrl, 30); this.SetRowSpan(ctrl, 5);
            //Кнопка - наименование библиотеки
            ctrl = m_dictControl[INDEX_CONTROL.BUTTON_DLLNAME_GROUPSOURCES];
            this.Controls.Add(ctrl, 48, 20);
            this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 5);
            //Источники группы
            ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.PANEL_LISTEDIT + (int)INDEX_PANEL.SOURCES_OF_GROUP)];
            this.Controls.Add(ctrl, 18, 25);
            this.SetColumnSpan(ctrl, 36); this.SetRowSpan(ctrl, 15);
            //Параметры соединения
            ctrl = m_dictControl[INDEX_CONTROL.DGV_PARAMETER_SOURCES];
            this.Controls.Add(ctrl, 18, 40);
            this.SetColumnSpan(ctrl, 36); this.SetRowSpan(ctrl, 15);
            //Группы сигналов
            ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.PANEL_LISTEDIT + (int)INDEX_PANEL.GROUP_SIGNALS)];
            this.Controls.Add(ctrl, 54, 0);
            this.SetColumnSpan(ctrl, 36); this.SetRowSpan(ctrl, 42);
            ////Сигналы группы
            //ctrl = m_dictControl[(INDEX_CONTROL)((int)INDEX_CONTROL.PANEL_LISTEDIT + (int)INDEX_PANEL.SIGNALS_OF_GROUP)];
            //this.Controls.Add(ctrl, 27, 18);
            //this.SetColumnSpan(ctrl, 18); this.SetRowSpan(ctrl, 30);

            //Применение размещения элементов
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
