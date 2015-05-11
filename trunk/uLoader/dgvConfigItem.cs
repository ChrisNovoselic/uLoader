using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Runtime.Remoting.Messaging; //AsyncResult...

using HClassLibrary; //DelegateObjectFunc...

namespace uLoader
{
    public class PanelCommon : TableLayoutPanel, IDataHost
    {
        public event DelegateObjectFunc EvtDataAskedHost;

        /// <summary>
        /// Отправить запрос главной форме
        /// </summary>
        /// <param name="idOwner">Идентификатор панели, отправляющей запрос</param>
        /// <param name="par"></param>
        public void DataAskedHost (object par)
        {
            EvtDataAskedHost.BeginInvoke(new EventArgsDataHost(-1, new object[] { par }), new AsyncCallback(this.dataRecievedHost), new Random());
        }

        /// <summary>
        /// Обработчик события ответа от главной формы
        /// </summary>
        /// <param name="obj">объект класса 'EventArgsDataHost' с идентификатором/данными из главной формы</param>
        public virtual void OnEvtDataRecievedHost(object res)
        {            
        }

        private void dataRecievedHost (object res)
        {
            if ((res as AsyncResult).EndInvokeCalled == false)
                ; //((DelegateObjectFunc)((AsyncResult)res).AsyncDelegate).EndInvoke(res as AsyncResult);
            else
                ;
        }

        public PanelCommon(int iColumnCount = 16, int iRowCount = 16)
        {
            this.ColumnCount = iColumnCount; this.RowCount = iRowCount;

            InitializeComponent();

            iActive = -1;
        }

        public PanelCommon(IContainer container, int iColumnCount = 16, int iRowCount = 16)
        {
            container.Add(this);

            this.ColumnCount = iColumnCount + 0; this.RowCount = iRowCount + 0;

            InitializeComponent();

            iActive = -1;
        }

        //-1 - исходное, 0 - старт, 1 - активная
        private int iActive;

        public bool Actived { get { return (iActive > 0) && (iActive % 2 == 1); } }

        public bool IsFirstActivated { get { return iActive == 1; } }

        public virtual bool Activate(bool active)
        {
            bool bRes = false;

            if (!(Actived == active))
            {
                this.iActive++;
                //Только при 1-ой активации
                if (this.iActive == 0)
                    this.iActive++;
                else
                    ;

                bRes = true;
            }
            else
                ;

            return bRes;
        }

        #region Обязательный код для корректного освобождения памяти
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

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

        private void PanelListEdit_NewItemTextChanged(object obj, EventArgs ev)
        {
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
            this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //this.Columns[0].Frozen = true;
            this.AllowUserToResizeColumns = false;
            this.AllowUserToResizeRows = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.MultiSelect = false;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //this.RowHeadersWidth = 76;
            this.ColumnHeadersVisible = false; this.RowHeadersVisible = false;

            this.ResumeLayout(false);
            this.PerformLayout();

            m_linkTextBoxNewItem.TextChanged += new EventHandler(PanelListEdit_NewItemTextChanged);
            m_linkBtnAdding.Enabled = false;
        }

        #endregion
    }
}
