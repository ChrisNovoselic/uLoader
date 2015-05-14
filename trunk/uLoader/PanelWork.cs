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
        /// <summary>
        /// Массив панелей (источник, назначение)
        /// </summary>
        PanelLoader[] m_arLoader;

        /// <summary>
        /// Конструктор - основной
        /// </summary>
        public PanelWork() : base (1, 2)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Конструктор - 
        /// </summary>
        /// <param name="container"></param>
        public PanelWork(IContainer container) : base (1, 2)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// Заполнить значениями объект со списком групп (элементов групп) (истоников, сигналов)
        /// </summary>
        /// <param name="indxWork">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillWorkItem(INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key, string[] rows)
        {
            DataGridView workItem = getWorkingItem(indxWork, key);
            if (!(rows == null))
                foreach (string row in rows)
                    workItem.Rows.Add(new object[] { row, @"->" });
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
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCES: //Группы источников (источник)
                    fillWorkItem(INDEX_SRC.SOURCE, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SIGNALS: //Группы сигналов (источник)
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES: //Группы источников (назначение)
                    fillWorkItem(INDEX_SRC.DEST, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SIGNALS: //Группы сигналов (назначение)
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
            //Проверить признак 1-го запуска            
            if (IsFirstActivated == true)
                //Запросить данные
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
        private DataGridView getWorkingItem(INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key)
        {
            DataGridView dgvRes = null;
            PanelLoader panelLdr;

            panelLdr = this.Controls[(int)indxWork] as PanelLoader;
            //Вариант №1
            //int indxCtrl = (int)PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES;
            //dgvRes = panelLdr.m_dictControl[(PanelLoader.INDEX_CONTROL)indxCtrl] as DataGridViewConfigItem;
            //Вариант №2
            Control []arCtrls = panelLdr.Controls.Find(key.ToString(), true);
            if (arCtrls.Length == 1)
                dgvRes = arCtrls[0] as DataGridView;
            else
                throw new Exception(@"PanelWork::getWorkingItem (" + indxWork.ToString() + @", " + key.ToString () + @") - не найден элемент управления...");

            return dgvRes;
        }

        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы источников (источник)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemSrcGroupSourcesSelectionChanged (object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы сигналов (источник)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemSrcGroupSignalsSelectionChanged (object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы источников (назначение)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemDestGroupSourcesSelectionChanged (object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы сигналов (назначение)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemDestGroupSignalsSelectionChanged (object obj, EventArgs ev)
        {
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
            //"Сетка" для позиционирования элементов управления
            initializeLayoutStyle ();
            //Создать панели (тсточник, назначение)
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

            DataGridView dgv = getWorkingItem(INDEX_SRC.SOURCE, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES);
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemSrcGroupSourcesSelectionChanged);

            dgv = getWorkingItem(INDEX_SRC.SOURCE, PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS);
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemSrcGroupSignalsSelectionChanged);

            dgv = getWorkingItem(INDEX_SRC.DEST, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES);
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemDestGroupSourcesSelectionChanged);
            
            dgv = getWorkingItem(INDEX_SRC.DEST, PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS);
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemDestGroupSignalsSelectionChanged);
        }

        #endregion
    }
}
