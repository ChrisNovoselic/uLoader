using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace uLoader
{
    public partial class PanelLoader : PanelCommon
    {
        //Ключи элементов управления
        protected enum KEY_CONTROLS { };
        
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

    partial class PanelLoader
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

            PanelCommon panelSources = new PanelCommon (5, 8)
                , panelQueries = new PanelCommon(5, 8)
                , panelRes = new PanelCommon(5, 8)
                , panelCommon = null;

            this.SuspendLayout ();

            //Панель источников данных
            this.Controls.Add (panelSources, 0, 0);
            this.SetColumnSpan(panelSources, 1); this.SetRowSpan(panelSources, 1);
            //Группы источников
            ctrl = new DataGridView ();
            ctrl.Dock = DockStyle.Fill;
            panelSources.Controls.Add (ctrl, 0, 0);
            panelSources.SetColumnSpan(ctrl, 5); panelSources.SetRowSpan(ctrl, 6);
            //Библиотека для загрузки
            ctrl = new Label();
            (ctrl as Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            (ctrl as Label).FlatStyle = FlatStyle.Standard;
            ctrl.Dock = DockStyle.Fill;
            panelSources.Controls.Add(ctrl, 0, 6);
            panelSources.SetColumnSpan(ctrl, 4); panelSources.SetRowSpan(ctrl, 1);
            //Кнопка для выгрузки/загрузки библиотеки
            ctrl = new Button();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as Button).Text = @"<->";
            panelSources.Controls.Add(ctrl, 4, 6);
            panelSources.SetColumnSpan(ctrl, 1); panelSources.SetRowSpan(ctrl, 1);
            //Выбор текущего источника
            ctrl = new ComboBox();
            ctrl.Dock = DockStyle.Bottom;
            panelSources.Controls.Add(ctrl, 0, 7);
            panelSources.SetColumnSpan(ctrl, 5); panelSources.SetRowSpan(ctrl, 1);

            //Панель опроса
            this.Controls.Add(panelQueries, 1, 0);
            this.SetColumnSpan(panelQueries, 1); this.SetRowSpan(panelQueries, 1);
            //Группы сигналов
            ctrl = new DataGridView();
            ctrl.Dock = DockStyle.Fill;
            panelQueries.Controls.Add(ctrl, 0, 0);
            panelQueries.SetColumnSpan(ctrl, 5); panelQueries.SetRowSpan(ctrl, 3);
            //ГроупБокс режима опроса
            ctrl = new GroupBox();
            (ctrl as GroupBox).Text = @"Режим опроса";
            ctrl.Dock = DockStyle.Fill;
            panelQueries.Controls.Add(ctrl, 0, 3);
            panelQueries.SetColumnSpan(ctrl, 5); panelQueries.SetRowSpan(ctrl, 5);
            //Панель для ГроупБокса
            panelCommon = new PanelCommon (8, 5);
            panelCommon.Dock = DockStyle.Fill;
            ctrl.Controls.Add (panelCommon);
            //Текущая дата/время
            //РадиоБуттон (тек. дата/время)
            ctrl = new RadioButton ();
            (ctrl as RadioButton).Text = @"Текущая дата/время";
            ctrl.Dock = DockStyle.Fill;
            panelCommon.Controls.Add (ctrl, 0, 0);
            panelCommon.SetColumnSpan(ctrl, 8); panelCommon.SetRowSpan(ctrl, 1);
            //Описание для интервала
            //Выборочно
            //РадиоБуттон (выборочно)
            ctrl = new RadioButton();
            (ctrl as RadioButton).Text = @"Выборочно";
            ctrl.Dock = DockStyle.Fill;
            panelCommon.Controls.Add(ctrl, 0, 2);
            panelCommon.SetColumnSpan(ctrl, 8); panelCommon.SetRowSpan(ctrl, 1);
            //Календарь
            ctrl = new DateTimePicker ();
            ctrl.Dock = DockStyle.Fill;
            panelCommon.Controls.Add(ctrl, 0, 3);
            panelCommon.SetColumnSpan(ctrl, 8); panelCommon.SetRowSpan(ctrl, 1);

            //Панель рез-ов опроса
            this.Controls.Add(panelRes, 2, 0);
            this.SetColumnSpan(panelRes, 2); this.SetRowSpan(panelRes, 1);

            this.ResumeLayout (false);
            this.PerformLayout ();
        }

        #endregion
    }

    public class PanelLoaderSource : PanelLoader
    {
        public PanelLoaderSource ()
        {
            InitializeComponent ();
        }

        private void InitializeComponent ()
        {
        }
    }

    public class PanelLoaderDest : PanelLoader
    {
        public PanelLoaderDest()
        {
            InitializeComponent ();
        }

        private void InitializeComponent ()
        {
        }
    }
}
