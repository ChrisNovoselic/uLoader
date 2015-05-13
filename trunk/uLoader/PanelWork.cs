using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using HClassLibrary;

namespace uLoader
{    
    public partial class PanelWork : PanelCommonDataHost
    {
        PanelLoader[] m_arLoader;

        public PanelWork() : base (1, 2)
        {
            InitializeComponent();
        }

        public PanelWork(IContainer container) : base (1, 2)
        {
            container.Add(this);

            InitializeComponent();
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
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCES:
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNALS:
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES:
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNALS:
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
                    throw new Exception(@"PanelWork::OnEvtDataRecievedHost () - IsHandleCreated==" + IsHandleCreated);
            else
                onEvtDataRecievedHost(obj);

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
                DataAskedHost(new object[] { new object [] { (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCES /*, без параметров*/ }
                                        , new object [] { (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNALS /*, без параметров*/ }
                                        , new object [] { (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES /*, без параметров*/ }
                                        , new object [] { (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNALS /*, без параметров*/ }
                                        });
            else
                ;

            return bRes;
        }

        /// <summary>
        /// Получить объект со списком групп (элементов групп)
        /// </summary>
        /// <param name="indxConfig">Индекс панели</param>
        /// <param name="indxPanel">Индекс типа объекта</param>
        /// <returns>Объект со списком групп</returns>
        private DataGridViewConfigItem getConfigItem(INDEX_SRC indxConfig/*, PanelLoader.INDEX_PANEL indxPanel*/)
        {
            PanelLoader panelLdr;
            int indxCtrl;

            panelLdr = this.Controls[(int)indxConfig] as PanelLoader;
            indxCtrl = (int)PanelLoader.INDEX_PANEL_CONTROL.LISTEDIT * (int)PanelLoader.INDEX_PANEL.COUNT_INDEX_PANEL + (int)indxPanel;
            return panelLdr.m_dictControl[(PanelLoader.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem;
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

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly ();
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

            initializeLayoutStyle ();

            Type typeLoader = typeof (PanelLoader);
            m_arLoader = new PanelLoader[(int)INDEX_SRC.COUNT_INDEX_SRC];
            for (int i = (int)INDEX_SRC.SOURCE; i < (int)INDEX_SRC.COUNT_INDEX_SRC; i++)
            {
                switch (i)
                {
                    case (int)INDEX_SRC.SOURCE:
                        typeLoader = typeof(PanelLoaderSource);
                        break;
                    case (int)INDEX_SRC.DEST:
                        typeLoader = typeof(PanelLoaderDest);
                        break;
                    default:
                        break;
                }
                m_arLoader[i] = Activator.CreateInstance(typeLoader) as PanelLoader;

                this.Controls.Add(m_arLoader[i], 0, i * this.RowCount / 2);
                this.SetColumnSpan(m_arLoader[i], this.ColumnCount); this.SetRowSpan(m_arLoader[i], this.RowCount / 2);
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
