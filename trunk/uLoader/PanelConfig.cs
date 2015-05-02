using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace uLoader
{
    public partial class PanelConfig : PanelCommon
    {
        //Индексы элементов управления
        enum INDEX_CONTROL {
            PANEL_LISTEDIT_GROUP_SOURCES, PANEL_LISTEDIT_SOURCES_OF_GROUP
            , PANEL_LISTEDIT_GROUP_SIGNALS , PANEL_LISTEDIT_SIGNALS_OF_GROUP
            , DGV_PARAMETER_SOURCES
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
        //Словарь элементов управления
        Dictionary<INDEX_CONTROL, Control> m_dictControl;
        
        public PanelConfig() : base (16, 15)
        {
            InitializeComponent();
        }

        public PanelConfig(IContainer container)
            : base(container, 16, 15)
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

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            Control ctrl; //Для "безличного" обращения к элементу интерфейса

            m_dictControl = new Dictionary<INDEX_CONTROL, Control>();
            //Создание переключателей
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
            //Создание кнопок
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
            //Создание панелей
            m_dictControl.Add(INDEX_CONTROL.PANEL_LISTEDIT_GROUP_SOURCES, new PanelListEdit(@"Группы источников"));
            m_dictControl.Add(INDEX_CONTROL.PANEL_LISTEDIT_SOURCES_OF_GROUP, new PanelListEdit(@"Источники группы"));
            m_dictControl.Add(INDEX_CONTROL.PANEL_LISTEDIT_GROUP_SIGNALS, new PanelListEdit(@"Группы сигналов"));
            m_dictControl.Add(INDEX_CONTROL.PANEL_LISTEDIT_SIGNALS_OF_GROUP, new PanelListEdit(@"Сигналы группы"));
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

            this.SuspendLayout();

            //Переключатели
            //Источник
            ctrl = m_dictControl[INDEX_CONTROL.RADIOBUTTON_SOURCE];
            this.Controls.Add(ctrl, 0, 0);
            this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);
            //Назначение
            ctrl = m_dictControl[INDEX_CONTROL.RADIOBUTTON_DEST];
            this.Controls.Add(ctrl, 2, 0);
            this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);
            //Кнопки
            //Загрузить
            ctrl = m_dictControl[INDEX_CONTROL.BUTTON_UPDATE];
            this.Controls.Add(ctrl, 0, 1);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 1);
            //Сохранить
            ctrl = m_dictControl[INDEX_CONTROL.BUTTON_SAVE];
            this.Controls.Add(ctrl, 0, 2);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 1);
            //Группы источников
            ctrl = m_dictControl[INDEX_CONTROL.PANEL_LISTEDIT_GROUP_SOURCES];
            this.Controls.Add(ctrl, 0, 3);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 8);
            //Источники группы
            ctrl = m_dictControl[INDEX_CONTROL.PANEL_LISTEDIT_SOURCES_OF_GROUP];
            this.Controls.Add(ctrl, 5, 0);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 8);
            //Группы сигналов
            ctrl = m_dictControl[INDEX_CONTROL.PANEL_LISTEDIT_GROUP_SIGNALS];
            this.Controls.Add(ctrl, 10, 0);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 8);            
            //Параметры соединения
            ctrl = m_dictControl[INDEX_CONTROL.DGV_PARAMETER_SOURCES];
            this.Controls.Add(ctrl, 5, 8);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 8);
            //Сигналы группы
            ctrl = m_dictControl[INDEX_CONTROL.PANEL_LISTEDIT_SIGNALS_OF_GROUP];
            this.Controls.Add(ctrl, 10, 8);
            this.SetColumnSpan(ctrl, 5); this.SetRowSpan(ctrl, 8);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
