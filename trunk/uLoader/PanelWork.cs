using System;
using System.ComponentModel;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Data;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP;
using ASUTP.Core;
using ASUTP.PlugIn;

namespace uLoader
{    
    partial class PanelWork : HPanelCommonDataHost
    {
        /// <summary>
        /// Перечисление - возможные состояния рабочей панели
        ///  неизвестное, данные (не)загружаются
        /// </summary>
        public enum STATE {
            Unknown = -1
                , Paused, Started
            , Count
        }
        ///// <summary>
        ///// Перечисление - индексы ПРЕДподготавливаемых параметров
        ///// </summary>
        //private enum INDEX_PREPARE_PARS { OBJECT, KEY_OBJECT, KEY_EVENT, ID_OBJ_SEL, ID_OBJ_GROUP_SIGNALS_SEL, COUNT_INDEX_PREPARE_PARS }
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

        //private StateManager m_states;

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
        /// <summary>
        /// Операции по инициализации
        /// </summary>
        /// <returns>Результат выполнения инициализации</returns>
        private int initialize ()
        {
            int iRes = 0;

            InitializeComponent ();

            m_iSecondUpdate = -1;

            return iRes;
        }
        /// <summary>
        /// Заполнить значениями объект со списком групп (элементов групп) (истоников, сигналов)
        /// </summary>
        /// <param name="indxWork">Индекс панели конфигурации</param>
        /// <param name="indxPanel">Индекс группы элементов (элементов) на панели конфигурации</param>
        /// <param name="rows">Массив строк для заполнения</param>
        private void fillWorkItem(FormMain.INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key, string[,] rows)
        {
            m_arLoader[(int)indxWork].FillWorkItem(key, rows);
        }
        /// <summary>
        /// Заполнить рабочий элемент - список источников 
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="grpSrc">Объект с данными для заполнения</param>
        private void fillWorkItem(FormMain.INDEX_SRC indxWork, GROUP_SRC grpSrc)
        {
            m_arLoader[(int)indxWork].FillWorkItem(grpSrc);
        }
        /// <summary>
        /// Заполнить рабочий элемент - список групп 
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="grpSrc">Объект с данными для заполнения</param>
        private void fillWorkItem(FormMain.INDEX_SRC indxWork, GROUP_SIGNALS_SRC grpSrc)
        {
            m_arLoader[(int)indxWork].FillWorkItem(grpSrc);
        }
        /// <summary>
        /// Заполнить рабочий элемент - список групп 
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="grpSrc">Объект с данными для заполнения</param>
        private void fillWorkItem(FormMain.INDEX_SRC indxWork, GROUP_SIGNALS_PARS grpSrc)
        {
            m_arLoader[(int)indxWork].FillWorkItem(grpSrc);
        }
        /// <summary>
        /// "Включение"/"отключение" элементов интерфейса в зависимости от состояния
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="key">Ключ элемента интерфейса</param>
        /// <param name="states">Массив состояний объектов</param>
        private void enabledWorkItem(FormMain.INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key, GroupSources.STATE[] states)
        {
            m_arLoader[(int)indxWork].EnabledWorkItem(key, states);
        }
        /// <summary>
        /// "Включение"/"отключение" элементов интерфейса в зависимости от состояния (и-или параметров группы сигналов)
        /// </summary>
        /// <param name="indxWork">Индекс панели</param>
        /// <param name="key">Ключ элемента интерфейса</param>
        /// <param name="args">Массив аргументов для каждой из групп сигналов (STATE, bEnableTools)</param>
        private void enabledWorkItem(FormMain.INDEX_SRC indxWork, PanelLoader.KEY_CONTROLS key, object [] args)
        {
            m_arLoader[(int)indxWork].EnabledWorkItem(key, args);
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
            m_timerUpdate = new System.Threading.Timer(fTimerUpdate, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            //changeTimerUpdate (0);
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
        /// Мзменить состояние таймера обновления
        /// </summary>
        /// <param name="msec">Интервалл доо очередного запуска функции таймера</param>
        /// <returns></returns>
        private int changeTimerUpdate(int msec)
        {
            int iRes = -1;

            iRes = m_timerUpdate.Change(msec, System.Threading.Timeout.Infinite) == true ? 0 : -1;

            return iRes;
        }
        /// <summary>
        /// Обработчик события получения данных по запросу (выполняется в текущем потоке)
        /// </summary>
        /// <param name="obj">Результат, полученный по запросу (массив 'object')</param>
        protected override void onEvtDataRecievedHost(object obj)
        {
            //Обработанное состояние 
            HHandlerQueue.StatesMachine state = (HHandlerQueue.StatesMachine)Int32.Parse((obj as object[])[0].ToString());
            //Параметры (массив) в 1-ом элементе результата
            object par = (obj as object[])[1];

            try {
                switch (state) {
                    case HHandlerQueue.StatesMachine.LIST_GROUP_SOURCES: //Группы источников (источник)
                        fillWorkItem(FormMain.INDEX_SRC.SOURCE, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[])[(int)FormMain.INDEX_SRC.SOURCE] as string[,]);
                        fillWorkItem(FormMain.INDEX_SRC.DEST, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[])[(int)FormMain.INDEX_SRC.DEST] as string[,]);
                        break;
                    case HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SOURCES: //Группа (объект) источников (источник)
                        //m_arCurrentSrcItems [(int)FormMain.INDEX_SRC.SOURCE] = par as ITEM_SRC;
                        fillWorkItem(FormMain.INDEX_SRC.SOURCE, par as GROUP_SRC);
                        break;
                    case HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SOURCES: //Группа (объект) источников (назначение)
                        //m_arCurrentSrcItems[(int)FormMain.INDEX_SRC.DEST] = par as ITEM_SRC;
                        fillWorkItem(FormMain.INDEX_SRC.DEST, par as GROUP_SRC);
                        break;
                    case HHandlerQueue.StatesMachine.TIMER_WORK_UPDATE:
                        m_iSecondUpdate = (int)par;
                        startTimerUpdate ();
                        activeTimerUpdate (true);
                        break;
                    case HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS_PARS: //Объект с параметрами группы сигналов (источник)
                        //???
                        //fillWorkItem(FormMain.INDEX_SRC.SOURCE, par as GROUP_SIGNALS_SRC);
                        fillWorkItem(FormMain.INDEX_SRC.SOURCE, par as GROUP_SIGNALS_PARS);
                        break;
                    case HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS: //Объект группы сигналов (источник)
                        fillWorkItem(FormMain.INDEX_SRC.SOURCE, par as GROUP_SIGNALS_SRC);
                        break;
                    case HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SIGNALS_PARS: //Объект с параметрами группы сигналов (назначение)
                        //???
                        //fillWorkItem(FormMain.INDEX_SRC.DEST, par as GROUP_SIGNALS_SRC);
                        fillWorkItem(FormMain.INDEX_SRC.DEST, par as GROUP_SIGNALS_PARS);
                        break;
                    case HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SIGNALS: //Объект группы сигналов (назначение)
                        //???
                        fillWorkItem(FormMain.INDEX_SRC.DEST, par as GROUP_SIGNALS_SRC);
                        break;
                    case HHandlerQueue.StatesMachine.STATE_GROUP_SOURCES: //Состояние группы источников (источник, назначение)
                        for (FormMain.INDEX_SRC indxSrc = FormMain.INDEX_SRC.SOURCE; indxSrc < FormMain.INDEX_SRC.COUNT_INDEX_SRC; indxSrc++)
                            enabledWorkItem(indxSrc, PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES, (par as object[])[(int)indxSrc] as GroupSources.STATE[]);
                        break;
                    case HHandlerQueue.StatesMachine.STATE_GROUP_SIGNALS: //Состояние группы сигналов  (источник, назначение)
                        for (FormMain.INDEX_SRC indxSrc = FormMain.INDEX_SRC.SOURCE; indxSrc < FormMain.INDEX_SRC.COUNT_INDEX_SRC; indxSrc ++ )
                            enabledWorkItem(indxSrc, PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS, (par as object[])[(int)indxSrc] as object []);
                        break;
                    case HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SOURCES: //Состояние (изменено) группы источников (источник, назначение)
                        //Немедленно запросить состояния групп источников
                        changeTimerUpdate (0);
                        break;
                    case HHandlerQueue.StatesMachine.STATE_CHANGED_GROUP_SIGNALS: //Состояние (изменено) группы сигналов (источник, назначение)
                        //Немедленно запросить состояния групп сигналов
                        changeTimerUpdate(0);
                        break;
                    case HHandlerQueue.StatesMachine.COMMAND_RELAOD_GROUP_SOURCES: //Состояние (выгружена/загружена) группы источников (источник, назначение)
                        //Немедленно запросить состояния групп источников
                        changeTimerUpdate (0);
                        break;
                    case HHandlerQueue.StatesMachine.DATA_SRC_GROUP_SIGNALS:
                        if (par == null)
                            m_arLoader[(int)FormMain.INDEX_SRC.SOURCE].UpdateData ();
                        else
                            m_arLoader[(int)FormMain.INDEX_SRC.SOURCE].UpdateData(par as DataTable);
                        break;
                    case HHandlerQueue.StatesMachine.DATA_DEST_GROUP_SIGNALS:
                        if (par == null)
                            m_arLoader[(int)FormMain.INDEX_SRC.DEST].UpdateData ();
                        else
                            m_arLoader[(int)FormMain.INDEX_SRC.DEST].UpdateData(par as DataTable);
                        break;
                    //case HHandlerQueue.StatesMachine.SET_IDCUR_SOURCE_OF_GROUP:
                    //    break;
                    case HHandlerQueue.StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
                        if (!(par == null))
                            m_arLoader[(int)FormMain.INDEX_SRC.SOURCE].FillDatetimePars (par as DATETIME_WORK);
                        else
                            ;
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"PanelWork::onEvtDataRecievedHost (state={0})", state), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        public override void Stop()
        {
            //m_states.Activate(false); m_states.Stop();

            stopTimerUpdate ();

            foreach (PanelLoader pLoader in m_arLoader)
                pLoader.Stop ();

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
            bool bActiveTimerUpdate = false;
            if (bRes == true)
                //Вариантт №1 (при "автостарте" птоковой функции таймера)
                if (Actived == true)
                    bActiveTimerUpdate = !IsFirstActivated;
                else
                    ;
                ////Вариант №2
                //bActiveTimerUpdate = Actived;
            else
                ; //Рез-т выпорлнения базовой функции активации = 'нет изменений'

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
            string strSrcIDGrpSignals = string.Empty
                , strDestIDGrpSignals = string.Empty;
            List<object[]> listDataAskedHost = new List<object[]>();

            ctrl = m_arLoader[(int)FormMain.INDEX_SRC.SOURCE].GetWorkingItem (PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
            indxSrcSel = ctrl.SelectedRows.Count > 0 ? ctrl.SelectedRows[0].Index : -1;
            ctrl = m_arLoader[(int)FormMain.INDEX_SRC.DEST].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES) as DataGridView;
            indxDestSel = ctrl.SelectedRows.Count > 0 ? ctrl.SelectedRows[0].Index : -1;

            ctrl = m_arLoader[(int)FormMain.INDEX_SRC.SOURCE].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS) as DataGridView;
            strSrcIDGrpSignals = ctrl.SelectedRows.Count > 0 ? m_arLoader[(int)FormMain.INDEX_SRC.SOURCE].GetWorkingItemId(PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS) : string.Empty;
            ctrl = m_arLoader[(int)FormMain.INDEX_SRC.DEST].GetWorkingItem(PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS) as DataGridView;
            strDestIDGrpSignals = ctrl.SelectedRows.Count > 0 ? m_arLoader[(int)FormMain.INDEX_SRC.DEST].GetWorkingItemId (PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS) : string.Empty;

            //1-ое событие (состояние групп источников)
            listDataAskedHost.Add(new object[] { (int)HHandlerQueue.StatesMachine.STATE_GROUP_SOURCES /*, без параметров*/ });

            if ((!(indxSrcSel < 0))
                || (!(indxDestSel < 0)))
                //2-ое (состояние групп сигналов) д.б. выбраны группы источников - родительские для групп сигналов
                listDataAskedHost.Add(new object[] { (int)HHandlerQueue.StatesMachine.STATE_GROUP_SIGNALS, indxSrcSel, indxDestSel });
            else
                ;
            
            if ((!(indxSrcSel < 0))
                && (strSrcIDGrpSignals.Equals(string.Empty) == false))
                //3-ье (данные по группе сигналов)
                listDataAskedHost.Add(new object[] { (int)HHandlerQueue.StatesMachine.DATA_SRC_GROUP_SIGNALS, indxSrcSel, strSrcIDGrpSignals });
            else
                ;

            if ((!(indxDestSel < 0))
                && (strDestIDGrpSignals.Equals (string.Empty) == false))
                //4-ое (данные по группе сигналов)
                listDataAskedHost.Add(new object[] { (int)HHandlerQueue.StatesMachine.DATA_DEST_GROUP_SIGNALS, indxDestSel, strDestIDGrpSignals });
            else
                ;

            //Запросить данные
            DataAskedHost(listDataAskedHost.ToArray ());
        }

        /// <summary>
        /// Обработчик события 'EvtDataAskedHost' от панелей (источник, назначение)
        /// </summary>
        /// <param name="obj">Параметр для передачи-массив (0-панель, 1-индекс группы источников, 2-индекс группы сигналов)</param>
        private void onEvtDataAskedPanelWork_PanelLoader (object par)
        {
            object []pars = (par as EventArgsDataHost).par[0] as object [];
            //Массив параметров для передачи
            object[] arObjToDataHost = new object [] { };
            //Событие для постановки в очередь обработки событий
            HHandlerQueue.StatesMachine state = HHandlerQueue.StatesMachine.UNKNOWN;
            //Определить панель-инициатор сообщения
            FormMain.INDEX_SRC indxWork = (FormMain.INDEX_SRC)(pars[(int)PanelLoader.INDEX_PREPARE_PARS.OBJ] as PanelLoader).Index;

            switch ((KEY_EVENT)pars[(int)PanelLoader.INDEX_PREPARE_PARS.KEY_EVT])
            {
                case KEY_EVENT.SELECTION_CHANGED:
                    switch (indxWork)
                    {
                        case FormMain.INDEX_SRC.SOURCE:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.KEY_OBJ])
                            {
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    state = HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SOURCES;
                                    arObjToDataHost = new object[] { new object[] { (int)state, pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL] } };
                                    break;
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    //state = ;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS_PARS
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                        , new object[] {
                                            (int)HHandlerQueue.StatesMachine.OBJ_SRC_GROUP_SIGNALS
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                case PanelLoader.KEY_CONTROLS.CBX_SOURCE_OF_GROUP:
                                    state = HHandlerQueue.StatesMachine.SET_IDCUR_SOURCE_OF_GROUP;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                case PanelLoader.KEY_CONTROLS.TBX_GROUPSOURCES_ADDING:
                                //case PanelLoader.KEY_CONTROLS.TBX_GROUPSIGNALS_ADDING:
                                    state = HHandlerQueue.StatesMachine.SET_TEXT_ADDING;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                case PanelLoader.KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS:
                                    state = HHandlerQueue.StatesMachine.SET_GROUP_SIGNALS_PARS;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)(pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL])
                                            , (GROUP_SIGNALS_PARS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                case PanelLoader.KEY_CONTROLS.RBUTTON_CUR_DATETIME:
                                case PanelLoader.KEY_CONTROLS.RBUTTON_COSTUMIZE:
                                    state = HHandlerQueue.StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)(pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL])
                                            , (string)(pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA] as object [])[0] //Строковый идентификатор группы сигналов
                                            , (MODE_WORK)(pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA] as object [])[1] //MODE_WORK
                                        }
                                    };
                                    break;
                                default:
                                    break;
                            }
                            break;

                        #region INDEX_SRC.DEST
                        case FormMain.INDEX_SRC.DEST:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.KEY_OBJ])
                            {
                                #region DGV_GROUP_SOURCES
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES:
                                    state = HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SOURCES;
                                    arObjToDataHost = new object[] { new object[] { (int)state, pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL] } };
                                    break;
                                #endregion

                                #region DGV_GROUP_SIGNALS
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS:
                                    //state = ;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SIGNALS_PARS
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                        , new object[] {
                                            (int)HHandlerQueue.StatesMachine.OBJ_DEST_GROUP_SIGNALS
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                #endregion

                                #region CBX_SOURCE_OF_GROUP
                                case PanelLoader.KEY_CONTROLS.CBX_SOURCE_OF_GROUP:
                                    state = HHandlerQueue.StatesMachine.SET_IDCUR_SOURCE_OF_GROUP;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                #endregion

                                #region TBX_GROUPSOURCES_ADDING
                                case PanelLoader.KEY_CONTROLS.TBX_GROUPSOURCES_ADDING:
                                //case PanelLoader.KEY_CONTROLS.TBX_GROUPSIGNALS_ADDING:
                                    state = HHandlerQueue.StatesMachine.SET_TEXT_ADDING;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , (string)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                #endregion

                                #region GROUP_BOX_GROUP_SIGNALS
                                case PanelLoader.KEY_CONTROLS.GROUP_BOX_GROUP_SIGNALS:
                                    state = HHandlerQueue.StatesMachine.SET_GROUP_SIGNALS_PARS;
                                    arObjToDataHost = new object[] {
                                        new object[] {
                                            (int)state
                                            , indxWork
                                            , (string)(pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL])
                                            , (GROUP_SIGNALS_PARS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                        }
                                    };
                                    break;
                                #endregion

                                default:
                                    break;
                            }
                            break;
                        #endregion

                        default:
                            break;
                    }
                    break;
                case KEY_EVENT.CELL_CLICK:
                    switch (indxWork)
                    {
                        case FormMain.INDEX_SRC.SOURCE:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.KEY_OBJ])
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
                        case FormMain.INDEX_SRC.DEST:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.KEY_OBJ])
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
                            , pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                            , pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                        }
                    };
                    break;
                case KEY_EVENT.BTN_DLL_RELOAD:
                    switch (indxWork)
                    {
                        case FormMain.INDEX_SRC.SOURCE:
                        case FormMain.INDEX_SRC.DEST:
                            //state = ;
                            arObjToDataHost = new object[] {
                                new object []
                                {
                                    (int)HHandlerQueue.StatesMachine.COMMAND_RELAOD_GROUP_SOURCES
                                    , indxWork
                                    , pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                    //, pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
                                }
                            };
                            break;
                        default:
                            break;
                    }
                    break;
                case KEY_EVENT.BTN_CLEAR_CLICK:
                    switch (indxWork)
                    {
                        case FormMain.INDEX_SRC.SOURCE: //для SOURCE удаление НЕвозможно
                            ;
                            break;
                        case FormMain.INDEX_SRC.DEST:
                            switch ((PanelLoader.KEY_CONTROLS)pars[(int)PanelLoader.INDEX_PREPARE_PARS.KEY_OBJ])
                            {
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SOURCES: // для DEST группы источников удаление НЕвозможно
                                    ;
                                    break;
                                case PanelLoader.KEY_CONTROLS.DGV_GROUP_SIGNALS: // удаление возможно ТОЛЬКО для DEST группв сигналов
                                    state = HHandlerQueue.StatesMachine.CLEARVALUES_DEST_GROUP_SIGNALS;
                                    arObjToDataHost = new object[] { new object []
                                        {
                                            (int)state
                                            , indxWork
                                            , pars[(int)PanelLoader.INDEX_PREPARE_PARS.ID_OBJ_SEL]
                                            , pars[(int)PanelLoader.INDEX_PREPARE_PARS.DEPENDENCED_DATA]
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
                default:
                    break;
            }

            //Ретрансляция для постановки в очередь
            if (arObjToDataHost.Length > 0)
                DataAskedHost(arObjToDataHost);
            else
                ;
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
            m_arLoader = new PanelLoader[(int)FormMain.INDEX_SRC.COUNT_INDEX_SRC];
            for (int i = (int)FormMain.INDEX_SRC.SOURCE; i < (int)FormMain.INDEX_SRC.COUNT_INDEX_SRC; i++)
            {
                switch (i)
                {
                    case (int)FormMain.INDEX_SRC.SOURCE:
                        typeLoader = typeof(PanelLoaderSource);
                        break;
                    case (int)FormMain.INDEX_SRC.DEST:
                        typeLoader = typeof(PanelLoaderDest);
                        break;
                    default:
                        break;
                }
                m_arLoader[i] = Activator.CreateInstance(typeLoader) as PanelLoader;
                m_arLoader[i].EvtDataAskedHost += new DelegateObjectFunc(onEvtDataAskedPanelWork_PanelLoader);

                this.Controls.Add(m_arLoader[i], 0, i * this.RowCount / 2);
                this.SetColumnSpan(m_arLoader[i], this.ColumnCount); this.SetRowSpan(m_arLoader[i], this.RowCount / 2);
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
