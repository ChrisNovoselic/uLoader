using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using HClassLibrary; //DelegateObjectFunc...

namespace uLoader
{
    public class PanelCommonULoader : HPanelCommon
    {
        public PanelCommonULoader (int cols, int rows) : base (cols, rows)
        {
            initializeLayoutStyle ();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly ();
        }
    }

    public partial class DataGridViewConfigItem: DataGridView
    {
        TextBox m_linkTextBoxNewItem;
        Button m_linkBtnAdding;

        public DataGridViewConfigItem(TextBox tbxNewItem, Button btnAdd)
        {
            m_linkTextBoxNewItem = tbxNewItem; m_linkBtnAdding = btnAdd;

            InitializeComponent();
        }

        public DataGridViewConfigItem(IContainer container, TextBox tbxNewItem, Button btnAdd)
        {
            m_linkTextBoxNewItem = tbxNewItem; m_linkBtnAdding = btnAdd;

            container.Add(this);

            InitializeComponent();
        }
        /// <summary>
        /// обработчик события - изменение текста нового элемента
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void PanelListEdit_NewItemTextChanged(object obj, EventArgs ev)
        {
            //Изменить доступность кнопки добавить
            m_linkBtnAdding.Enabled = m_linkTextBoxNewItem.Text.Length > 0;
        }
    }

    partial class DataGridViewConfigItem
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

            this.Dock = DockStyle.Fill;

            this.SuspendLayout();

            this.Columns.AddRange(
                new DataGridViewColumn [] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewButtonColumn ()
                }
            );            
            this.AllowUserToResizeColumns = false;
            this.AllowUserToResizeRows = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.MultiSelect = false;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //this.RowHeadersWidth = 76;
            this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.Columns[1].Width = 34;
            //this.Columns[0].Frozen = true;
            this.ColumnHeadersVisible = false; this.RowHeadersVisible = false;

            this.ResumeLayout(false);
            this.PerformLayout();

            m_linkTextBoxNewItem.TextChanged += new EventHandler(PanelListEdit_NewItemTextChanged);
            m_linkBtnAdding.Enabled = false;
        }

        #endregion
    }

    public class DataGridViewConfigItemProp: DataGridView
    {
        public DataGridViewConfigItemProp()
        {
            InitializeComponent();
        }

        public DataGridViewConfigItemProp(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
        
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

            this.Dock = DockStyle.Fill;

            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            this.Columns.Add(@"Value", @"Значение");
            this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //this.Columns[0].Frozen = true;
            this.AllowUserToResizeColumns = false;
            this.AllowUserToResizeRows = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.MultiSelect = false;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.RowHeadersWidth = 76;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
