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
        /// Перечисление - индексы ПРЕДподготавливаемых параметров
        /// </summary>
        private enum INDEX_PREPARE_PARS { OBJECT, KEY_OBJECT, KEY_EVENT, INDEX_OBJ_GROUP_SOURCES_SEL, INDEX_OBJ_GROUP_SIGNALS_SEL, COUNT_INDEX_PREPARE_PARS }
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
        /// "Включение"/"отключение" элементов интерфейса в зависимости от состояния
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="key">Ключ элемента интерфейса</param>
        /// <param name="states">Массив состояний объектов</param>
        private void enabledWorkItem(INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key, GroupSources.STATE[] states)
        {
            m_arLoader[(int)indxWork].EnabledWorkItem(key, states);
        }
        private int changeTimerUpdate (int msec)
        {
            int iRes = -1;

            iRes = m_timerUpdate.Change (msec, System.Threading.Timeout.Infinite) == true ? 0 : -1;

            return iRes;
        }
        /// <summary>
        /// Активировать таймер
        /// </summary>
        /// <param name="active">ПРизнак активизации</param>
        /// <returns></returns>
        private int activeTimerUpdate (bool active)
        {
            int iRes = 0
                //Отложить выызов функции обновлени на неопределенное время
                , msecDueTime = System.Threading.Timeout.Infinite;

            if (active == true)
                //Вызвать функцию обновления немедленно
                msecDueTime = 0;
            else
                ;
            //Изменить состояние таймера
            iRes = changeTimerUpdate(msecDueTime);

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
            changeTimerUpdate (System.Threading.Timeout.Infinite);
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
                case (int)HHandlerQueue.StatesMachine.LIST_GROUP_SOURCES: //Группы источников (источник)
                    fillWorkItem(INDEX_SRC.SOURCE, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[])[(int)INDEX_SRC.SOURCE] as string[]);
                    fillWorkItem(INDEX_SRC.DEST, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[])[(int)INDEX_SRC.DEST] as string[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SOURCES: //Группа (объект) источников (источник)
                    //m_arCurrentSrcItems [(int)INDEX_SRC.SOURCE] = par as ITEM_SRC;
                    fillWorkItem(INDEX_SRC.SOURCE, par as GROUP_SRC);
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
                case (int)HHandlerQueue.StatesMachine.STATE_GROUP_SOURCES: //Состояние группы источников (источник, назначение)
                    for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc++)
                        enabledWorkItem(indxSrc, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[])[(int)indxSrc] as GroupSources.STATE[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.STATE_GROUP_SIGNALS: //Состояние группы сигналов  (источник, назначение)
                    for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc ++ )
                        enabledWorkItem(indxSrc, PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS, (par as object[])[(int)indxSrc] as GroupSources.STATE[]);
                    break;
                case (int)HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SOURCES: //Состояние (изменено) группы источников (источник, назначение)
                    changeTimerUpdate (0);
                    break;
                case (int)HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SIGNALS: //Состояние (изменено) группы сигналов (источник, назначение)
                    changeTimerUpdate(0);
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
                DataAskedHost(new object[] { new object [] { (int)HHandlerQueue.StatesMachine.LIST_GROUP_SOURCES /*, без параметров*/ }
                                            //??? временно отключить для отладки...
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
            changeTimerUpdate (1000 * m_iSecondUpdate);

            DataGridView ctrl = null;
            int indxSrcSel = -1
                , indxDestSel = -1;

            ctrl = m_arLoader[(int)INDEX_SRC.SOURCE].GetWorkingItem (PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
            indxSrcSel = ctrl.SelectedRows.Count > 0 ? ctrl.SelectedRows[0].Index : -1;
            ctrl = m_arLoader[(int)INDEX_SRC.DEST].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
            indxDestSel = ctrl.SelectedRows.Count > 0 ? ctrl.SelectedRows[0].Index : -1;

            //Запросить данные
            DataAskedHost(new object[] {
                new object [] { (int)HHandlerQueue.StatesMachine.STATE_GROUP_SOURCES /*, без параметров*/ }
                , new object [] { (int)HHandlerQueue.StatesMachine.STATE_GROUP_SIGNALS, indxSrcSel, indxDestSel }
            });
        }

        /// <summary>
        /// Обработчик события 'EvtDataAskedHost' от панелей (источник, назначение)
        /// </summary>
        /// <param name="obj">Параметр для передачи-массив (0-панель, 1-индекс группы источников, 2-индекс группы сигналов)</param>
        private void OnEvtDataAskedPanelWork_PanelLoader (object par)
        {
            object []pars = (par as EventArgsDataHost).par[0] as object [];
            //Массив параметров для передачи
            object[] arObjToDataHost = new object [] { };
            //Событие для постановки в очередь обработки событий
            HHandlerQueue.StatesMachine state = HHandlerQueue.StatesMachine.UNKNOWN;
            //Определить панель-инициатор сообщения
            INDEX_SRC indxWork = (INDEX_SRC)this.Controls.GetChildIndex(pars[(int)INDEX_PREPARE_PARS.OBJECT] as PanelLoader);

            switch ((PanelLoader.KEY_EVENT)pars[(int)INDEX_PREPARE_PARS.KEY_EVENT])
            {
                case PanelLoader.KEY_EVENT.SELECTION_CHANGED:
                    switch (indxWork)
                    {
                        case INDEX_SRC.SOURCE:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)INDEX_PREPARE_PARS.KEY_OBJECT])
                            {
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    state = HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SOURCES;
                                    arObjToDataHost = new object[] { new object[] { (int)state, pars[(int)INDEX_PREPARE_PARS.INDEX_OBJ_GROUP_SOURCES_SEL] } };
                                    break;
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    state = HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (pars[(int)INDEX_PREPARE_PARS.OBJECT] as PanelLoader).GetWorkingItemValue (
                                                (PanelLoader.KEY_CONTROLS)pars[(int)INDEX_PREPARE_PARS.KEY_OBJECT]
                                                , (int)pars[(int)INDEX_PREPARE_PARS.INDEX_OBJ_GROUP_SIGNALS_SEL]
                                            )
                                        }
                                    };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case INDEX_SRC.DEST:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)INDEX_PREPARE_PARS.KEY_OBJECT])
                            {
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    state = HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SOURCES;
                                    arObjToDataHost = new object[] { new object[] { (int)state, pars[(int)INDEX_PREPARE_PARS.INDEX_OBJ_GROUP_SOURCES_SEL] } };
                                    break;
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    state = HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SIGNALS;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (pars[(int)INDEX_PREPARE_PARS.OBJECT] as DataGridView).SelectedRows[0].Cells[0].Value.ToString ().Trim ()
                                        }
                                    };
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case PanelLoader.KEY_EVENT.CELL_CLICK:
                    switch (indxWork)
                    {
                        case INDEX_SRC.SOURCE:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)INDEX_PREPARE_PARS.KEY_OBJECT])
                            {
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    state = HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SOURCES;
                                    break;
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    state = HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SIGNALS;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case INDEX_SRC.DEST:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)INDEX_PREPARE_PARS.KEY_OBJECT])
                            {
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    state = HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SOURCES;
                                    break;
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    state = HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SIGNALS;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                    arObjToDataHost = new object[] { new object []
                                                        {
                                                            (int)state
                                                            , indxWork
                                                            , pars[(int)INDEX_PREPARE_PARS.INDEX_OBJ_GROUP_SOURCES_SEL]
                                                            , pars[(int)INDEX_PREPARE_PARS.INDEX_OBJ_GROUP_SIGNALS_SEL]
                                                        }
                    };
                    break;
                default:
                    break;
            }

            //Ретрансляция для постановки в очередь
            DataAskedHost(arObjToDataHost);
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
        }

        #endregion
    }
}
