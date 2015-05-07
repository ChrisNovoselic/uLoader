using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace uLoader
{    
    public partial class PanelWork : PanelCommon
    {
        enum INDEX_LOADER { SOURCE, DEST, COUNT_INDEX_LOADER };
        PanelLoader[] m_arLoader;

        public PanelWork()
        {
            InitializeComponent();
        }

        public PanelWork(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }

    partial class PanelWork
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

            this.SuspendLayout();

            Type typeLoader;
            m_arLoader = new PanelLoader[(int)INDEX_LOADER.COUNT_INDEX_LOADER];
            for (int i = (int)INDEX_LOADER.SOURCE; i < (int)INDEX_LOADER.COUNT_INDEX_LOADER; i++)
            {
                switch (i)
                {
                    case (int)INDEX_LOADER.SOURCE:
                        m_arLoader[i] = new PanelLoaderSource ();
                        break;
                    case (int)INDEX_LOADER.DEST:
                        m_arLoader[i] = new PanelLoaderDest();
                        break;
                    default:
                        break;
                }                
                this.Controls.Add(m_arLoader[i], 0, i * this.RowCount / 2);
                this.SetColumnSpan(m_arLoader[i], this.ColumnCount); this.SetRowSpan(m_arLoader[i], this.RowCount / 2);
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
