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
        public PanelCommon(int iColumnCount = 16, int iRowCount = 16)
        {
            this.ColumnCount = iColumnCount; this.RowCount = iRowCount;

            InitializeComponent();
        }

        public PanelCommon(IContainer container, int iColumnCount = 16, int iRowCount = 16)
        {
            container.Add(this);

            this.ColumnCount = iColumnCount + 0; this.RowCount = iRowCount + 0;

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

            float val = (float)100 / this.ColumnCount;
            //Добавить стили "ширина" столлбцов
            for (int s = 0; s < this.ColumnCount - 0; s++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, val));
            //this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize, val));

            val = (float)100 / this.RowCount;
            //Добавить стили "высота" строк
            for (int s = 0; s < this.RowCount - 0; s++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, val));
            //this.RowStyles.Add(new RowStyle(SizeType.AutoSize, val));

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }

    public partial class DataGridViewListItem: DataGridView
    {
        TextBox m_linkTextBoxNewItem;
        Button m_linkBtnAdding;

        public DataGridViewListItem(TextBox tbxNewItem, Button btnAdd)
        {
            m_linkTextBoxNewItem = tbxNewItem; m_linkBtnAdding = btnAdd;

            InitializeComponent();
        }

        public DataGridViewListItem(IContainer container, TextBox tbxNewItem, Button btnAdd)
        {
            m_linkTextBoxNewItem = tbxNewItem; m_linkBtnAdding = btnAdd;

            container.Add(this);

            InitializeComponent();
        }

        private void PanelListEdit_NewItemTextChanged(object obj, EventArgs ev)
        {
            m_linkBtnAdding.Enabled = m_linkTextBoxNewItem.Text.Length > 0;
        }
    }

    partial class DataGridViewListItem
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

            this.ResumeLayout(false);
            this.PerformLayout();

            m_linkTextBoxNewItem.TextChanged += new EventHandler(PanelListEdit_NewItemTextChanged);
            m_linkBtnAdding.Enabled = false;
        }

        #endregion
    }
}
