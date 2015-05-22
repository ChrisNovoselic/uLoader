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
        ///// <summary>
        ///// Массив текущих объектов "Группа источников" (источник, назначение)
        ///// </summary>
        //GroupSources[] m_arCurrentGroupSources;
        /// <summary>
        /// Таймер обновления состояния панели
        /// </summary>
        private System.Threading.Timer m_timerUpdate;

        private int m_iSecondUpdate;

        /// <summary>
        /// Конструктор - основной
        /// </summary>
        public PanelWork() : base (1, 2)
        {
            initialize ();
        }

        /// <summary>
        /// Конструктор - 
        /// </summary>
        /// <param name="container"></param>
        public PanelWork(IContainer container) : base (1, 2)
        {
            container.Add(this);

            initialize ();
        }

        private int initialize ()
        {
            int ires = 0;

            InitializeComponent ();

            //m_arCurrentGroupSources = new GroupSources[(int)INDEX_SRC.COUNT_INDEX_SRC];

            m_iSecondUpdate = -1;

            return ires;
        }

        /// <summary>
        /// Заполнить значениями объект со списком групп (элементов групп) (истоников, сигналов)
        /// </summary>
        /// <param name="indxWork">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillWorkItem(INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key, string[] rows)
        {
            DataGridView workItem = m_arLoader[(int)indxWork].GetWorkingItem(key) as DataGridView;
            if (!(rows == null))
                foreach (string row in rows)
                    workItem.Rows.Add(new object[] { row });
            else
                ;
        }
        /// <summary>
        /// Заполнить рабочий элемент - список источников 
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="grpSrc">Объект с данными для заполнения</param>
        private void fillWorkItem(INDEX_SRC indxWork, GROUP_SRC grpSrc)
        {
            m_arLoader[(int)indxWork].FillWorkItem(grpSrc);
        }
        /// <summary>
        /// Заполнить рабочий элемент - список групп 
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="grpSrc">Объект с данными для заполнения</param>
        private void fillWorkItem(INDEX_SRC indxWork, GROUP_SIGNALS_SRC grpSrc)
        {
            m_arLoader[(int)indxWork].FillWorkItem(grpSrc);
        }
        /// <summary>
        /// Активировать таймер
        /// </summary>
        /// <param name="active">ПРизнак активизации</param>
        /// <returns></returns>
        private int activeTimerUpdate (bool active)
        {
            int iRes = 0
                , msecDueTime = System.Threading.Timeout.Infinite;

            if (active == true)
                //Вызвать функцию обновления немедленно
                msecDueTime = 0;
            else
                ;
            //Изменить состояние таймера
            iRes = m_timerUpdate.Change(msecDueTime, System.Threading.Timeout.Infinite) == true ? 0 : -1;

            return iRes;
        }
        /// <summary>
        /// "Запустить" таймер
        /// </summary>
        private void startTimerUpdate ()
        {
            //Остановить, если уже выполняется
            stopTimerUpdate ();
            
            //Создать таймер запроса на обновление информации
            m_timerUpdate = new System.Threading.Timer (fTimerUpdate, null, 0, System.Threading.Timeout.Infinite); 
        }
        /// <summary>
        /// Остановить таймер
        /// </summary>        
        private void stopTimerUpdate ()
        {
            //Проверить объект таймера
            if (m_timerUpdate == null)
                return;
            else
                ;
            //Выполнить действия для останова таймера
            m_timerUpdate.Change (System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            m_timerUpdate.Dispose ();
            m_timerUpdate = null;
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
                case (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SOURCES: //Группа (объект) источников (источник)
                    //m_arCurrentSrcItems [(int)INDEX_SRC.SOURCE] = par as ITEM_SRC;
                    fillWorkItem(INDEX_SRC.SOURCE, par as GROUP_SRC);
                    break;
                case (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES: //Группы источников (назначение)
                    fillWorkItem(INDEX_SRC.DEST, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[]) as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SOURCES: //Группа (объект) источников (назначение)
                    //m_arCurrentSrcItems[(int)INDEX_SRC.DEST] = par as ITEM_SRC;
                    fillWorkItem(INDEX_SRC.DEST, par as GROUP_SRC);
                    break;
                case (int)HHandlerQueue.StatesMachine.TIMER_WORK_UPDATE:
                    m_iSecondUpdate = (int)par;
                    startTimerUpdate ();
                    break;
                case (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS: //Объект группы сигналов (источник)
                    fillWorkItem(INDEX_SRC.SOURCE, par as GROUP_SIGNALS_SRC);
                    break;
                case (int)HHandlerQueue.StatesMachine.STATE_SRC_GROUP_SOURCES: //Состояние группы источников  (источник)
                    break;
                case (int)HHandlerQueue.StatesMachine.STATE_DEST_GROUP_SOURCES: //Состояние группы источников  (назначение)
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
            //Проверить признак необходимости переноса действий в "свой" поток
            if (InvokeRequired == true)
                //Проверить наличие дескриптора
                if (IsHandleCreated == true)
                    //Перенести выполнение в "свой" поток
                    this.BeginInvoke(new DelegateObjectFunc(onEvtDataRecievedHost), obj);
                else
                    throw new Exception(@"PanelWork::OnEvtDataRecievedHost () - IsHandleCreated==" + IsHandleCreated);
            else
                //Выполнить в "вызывающем" потоке
                onEvtDataRecievedHost(obj);

            base.OnEvtDataRecievedHost(obj);
        }

        public override void Stop()
        {
            stopTimerUpdate ();

            base.Stop();
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
            {
                //Запросить данные
                DataAskedHost(new object[] { new object [] { (int)HHandlerQueue.StatesMachine.LIST_SRC_GROUP_SOURCES /*, без параметров*/ }
                                            , new object [] { (int)HHandlerQueue.StatesMachine.LIST_DEST_GROUP_SOURCES /*, без параметров*/ }
                                            , new object [] { (int)HHandlerQueue.StatesMachine.TIMER_WORK_UPDATE /*, без параметров*/ }
                                        });                
            }
            else                
                ;

            //Проверить необходимость активации/деактивации
            bool bActiveTimerUpdate = true;
            if (bRes == true)
                if (Actived == true)
                    bActiveTimerUpdate = ! IsFirstActivated;
                else
                    ;

            if (bActiveTimerUpdate == true)
                //Активировать/деактивировать таймер запроса на обновление информации
                activeTimerUpdate(Actived);
            else
                ;

            return bRes;
        }

        private void fTimerUpdate (object par)
        {
            m_timerUpdate.Change(1000 * m_iSecondUpdate, System.Threading.Timeout.Infinite);

            //Запросить данные
            DataAskedHost(new object[] {
                new object [] { (int)HHandlerQueue.StatesMachine.STATE_SRC_GROUP_SOURCES /*, без параметров*/ }
                , new object [] { (int)HHandlerQueue.StatesMachine.STATE_DEST_GROUP_SOURCES /*, без параметров*/ }
            });
        }

        ///// <summary>
        ///// Получить объект со списком групп (элементов групп)
        ///// </summary>
        ///// <param name="indxConfig">Индекс панели</param>
        ///// <param name="indxPanel">Индекс типа объекта</param>
        ///// <returns>Объект со списком групп</returns>
        //private Control getWorkingItem(INDEX_SRC indxSrc, PanelLoader.KEY_CONTROLS key)
        //{
        //    return m_arLoader [(int)indxSrc].GetWorkingItem (key);
        //}
        
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы источников (источник)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemSrcGroupSourcesSelectionChanged (object obj, EventArgs ev)
        {
            if (IsFirstActivated == false)
                m_arLoader [(int)INDEX_SRC.SOURCE].ClearValues();
            else
                ;

            int selIndex = (obj as DataGridView).SelectedRows [0].Index;
            DataAskedHost(new object[] { new object [] { (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SOURCES, selIndex } });
        }
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы сигналов (источник)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemSrcGroupSignalsSelectionChanged (object obj, EventArgs ev)
        {
            if (IsFirstActivated == false)
                m_arLoader [(int)INDEX_SRC.SOURCE].ClearValues (PanelLoader.KEY_CONTROLS.DGV_SIGNALS_OF_GROUP);
            else
                ;

            string idGroupSignals = (obj as DataGridView).SelectedRows[0].Cells[0].Value.ToString ().Trim ();
            DataAskedHost(new object[] { new object[] { (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS, INDEX_SRC.SOURCE, idGroupSignals } });
        }
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы источников (назначение)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemDestGroupSourcesSelectionChanged (object obj, EventArgs ev)
        {
            if (IsFirstActivated == false)
                m_arLoader[(int)INDEX_SRC.DEST].ClearValues ();
            else
                ;
            
            int selIndex = (obj as DataGridView).SelectedRows[0].Index;
            DataAskedHost(new object[] { new object[] { (int)HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SOURCES, selIndex } });
        }
        /// <summary>
        /// Обработка события "изменение выбора" для 'DataGridView' - группы сигналов (назначение)
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие ('DataGridView')</param>
        /// <param name="ev">Аргументы события</param>
        private void panelWork_dgvConfigItemDestGroupSignalsSelectionChanged (object obj, EventArgs ev)
        {
            if (IsFirstActivated == false)
                m_arLoader[(int)INDEX_SRC.DEST].ClearValues (PanelLoader.KEY_CONTROLS.DGV_SIGNALS_OF_GROUP);
            else
                ;
        }
        /// <summary>
        /// Обработчик события 'EvtDataAskedHost' от панелей (источник, назначение)
        /// </summary>
        /// <param name="obj">Параметр для передачи</param>
        private void OnEvtDataAskedPanelWork_PanelLoader (object obj)
        {
            //Ретрансляция для постановки в очередь
            //DataAskedHost (obj);
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

            stopTimerUpdate ();

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
                m_arLoader[i].EvtDataAskedHost += new DelegateObjectFunc(OnEvtDataAskedPanelWork_PanelLoader);

                this.Controls.Add(m_arLoader[i], 0, i * this.RowCount / 2);
                this.SetColumnSpan(m_arLoader[i], this.ColumnCount); this.SetRowSpan(m_arLoader[i], this.RowCount / 2);
            }

            this.ResumeLayout(false);
            this.PerformLayout();

            DataGridView dgv = m_arLoader[(int)INDEX_SRC.SOURCE].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemSrcGroupSourcesSelectionChanged);

            dgv = m_arLoader[(int)INDEX_SRC.SOURCE].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS) as DataGridView;
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemSrcGroupSignalsSelectionChanged);

            dgv = m_arLoader[(int)INDEX_SRC.DEST].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemDestGroupSourcesSelectionChanged);

            dgv = m_arLoader[(int)INDEX_SRC.DEST].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS) as DataGridView;
            dgv.SelectionChanged += new EventHandler(panelWork_dgvConfigItemDestGroupSignalsSelectionChanged);
        }

        #endregion
    }
}
