using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace uLoader
{
    public class PanelCommon : TableLayoutPanel
    {
        public PanelCommon(int iRowCount = 16, int iColumnCount = 16)
        {
            this.ColumnCount = iColumnCount; this.RowCount = iRowCount;

            InitializeComponent();
        }

        public PanelCommon(IContainer container, int iRowCount = 16, int iColumnCount = 16)
        {
            container.Add(this);

            this.ColumnCount = iColumnCount; this.RowCount = iRowCount;

            InitializeComponent();
        }

        private System.ComponentModel.IContainer components = null;

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
            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;            

            //Добавить стили "ширина" столлбцов
            for (int s = 0; s < this.ColumnCount; s++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)100 / this.ColumnCount));

            //Добавить стили "высота" строк
            for (int s = 0; s < this.RowCount; s++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)100 / this.RowCount));

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }

    public partial class PanelListEdit: PanelCommon
    {
        Label m_lblHeader;
        DataGridView m_dgvListItem;
        TextBox m_linkTextBoxNewItem;
        Button m_linkBtnAdding;

        public PanelListEdit(string name, TextBox tbxNewItem, Button btnAdd)
        {
            m_linkTextBoxNewItem = tbxNewItem; m_linkBtnAdding = btnAdd;

            InitializeComponent();

            m_lblHeader.Text = name;
        }

        public PanelListEdit(IContainer container, string name, TextBox tbxNewItem, Button btnAdd)
        {
            m_linkTextBoxNewItem = tbxNewItem; m_linkBtnAdding = btnAdd;

            container.Add(this);

            InitializeComponent();

            m_lblHeader.Text = name;
        }

        private void PanelListEdit_NewItemTextChanged(object obj, EventArgs ev)
        {
            m_linkBtnAdding.Enabled = m_linkTextBoxNewItem.Text.Length > 0;
        }
    }

    partial class PanelListEdit
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

            m_lblHeader = new Label ();
            m_dgvListItem = new DataGridView ();

            this.SuspendLayout();

            m_lblHeader.Dock = DockStyle.Fill;
            m_lblHeader.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.Controls.Add (m_lblHeader, 0, 0);
            this.SetColumnSpan (m_lblHeader, this.ColumnCount); this.SetRowSpan (m_lblHeader, 2);

            m_dgvListItem.Dock = DockStyle.Fill;
            this.Controls.Add (m_dgvListItem, 0, 2);
            this.SetColumnSpan (m_dgvListItem, this.ColumnCount); this.SetRowSpan (m_dgvListItem, 14);

            this.ResumeLayout(false);
            this.PerformLayout();

            m_linkTextBoxNewItem.TextChanged += new EventHandler(PanelListEdit_NewItemTextChanged);
            m_linkBtnAdding.Enabled = false;
        }

        #endregion
    }
}
