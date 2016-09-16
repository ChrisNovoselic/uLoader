using System;
using System.Collections.Generic;
using System.Linq;

using HClassLibrary; //HHandler
using uLoaderCommon;

namespace uLoader
{
    partial class HHandlerQueue : HClassLibrary.HHandlerQueue
    {
        /// <summary>
        /// Перечисление - возможные состояния для обработки
        /// </summary>
        public enum StatesMachine
        {
            UNKNOWN = -1
            , LIST_GROUP_SOURCES //Список групп источников (источник, назначение)
            , LIST_SRC_GROUP_SOURCE_ITEMS //Список источников в группе источников (источник)
            , LIST_SRC_GROUP_SOURCE_PARS //Список наименовний параметров соединения источников в группе источников (источник)
            , LIST_SRC_GROUP_SOURCE_PROP //Список параметров соединения источников в группе источников (источник)
            , LIST_GROUP_SIGNALS //Список групп сигналов (источник, назначение)
            , LIST_SRC_GROUP_SIGNAL_ITEMS //Список сигналов в группе сигналов (источник)
            , LIST_SRC_GROUP_SIGNAL_PARS //Список наименовний параметров сигналов в группе сигналов (источник)
            , LIST_SRC_GROUP_SIGNAL_PROP //Список параметров сигналов в группе сигналов (источник)
            , LIST_DEST_GROUP_SOURCE_ITEMS //Список источников в группе источников (назначение)
            , LIST_DEST_GROUP_SOURCE_PARS //Список наименований параметров соединения источников в группе источников (назначение)
            , LIST_DEST_GROUP_SOURCE_PROP //Список параметров соединения источников в группе источников (назначение)            
            , LIST_DEST_GROUP_SIGNAL_ITEMS //Список сигналов в группе сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_PARS //Список наименований параметров сигналов в группе сигналов (назначение)
            , LIST_DEST_GROUP_SIGNAL_PROP //Список параметров сигналов в группе сигналов (назначение)
            , OBJ_SRC_GROUP_SOURCES //Объект группы источников (источник)
            , OBJ_DEST_GROUP_SOURCES //Объект группы источников (назначение)
            , OBJ_SRC_GROUP_SIGNALS_PARS //Объект с параметрами группы сигналов (источник)
            , OBJ_SRC_GROUP_SIGNALS //Объект группы сигналов (источник)
            , OBJ_DEST_GROUP_SIGNALS_PARS //Объект с параметрами группы сигналов (назначение)
            , OBJ_DEST_GROUP_SIGNALS //Объект группы сигналов (назначение)
            , TIMER_WORK_UPDATE //Период обновления панели "Работа"
            , STATE_GROUP_SOURCES //Состояние группы источников (источник, назначение)
            , STATE_GROUP_SIGNALS //Состояние группы сигналов (источник, назначение)
            , STATE_CHANGED_GROUP_SOURCES //Изменение состояния группы источников (источник, назначение) - инициатива пользователя
            , STATE_CHANGED_GROUP_SIGNALS //Изменение состояния группы сигналов (источник, назначение) - инициатива пользователя
            , COMMAND_RELAOD_GROUP_SOURCES //Команда для выгрузки/загрузки библиотеки - инициатива пользователя
            , CLEARVALUES_DEST_GROUP_SIGNALS //Очистить значения для группы сигналов - инициатива пользователя
            , DATA_SRC_GROUP_SIGNALS //Данные группы сигналов (источник)
            , DATA_DEST_GROUP_SIGNALS //Данные группы сигналов (назначение)
            , SET_IDCUR_SOURCE_OF_GROUP //Установить идентификатор текущего источника
            , SET_TEXT_ADDING //Установить текст "дополнительных" параметров
            , SET_GROUP_SIGNALS_PARS //Установить параметры группы сигналов в группе источников при утрате фокуса ввода элементом управления (GroupBox) с их значениями
            , GET_GROUP_SIGNALS_DATETIME_PARS //Запросить параметры группы сигналов в группе источников при изменении типа параметров (CUR_DATETIME, COSTUMIZE)
            , GET_INTERACTION_PARAMETERS //Запросить параметры для вкладки "Взаимодействие"
            , INTERACTION_EVENT //Событие от вкладки "Взаимодействие"
            , FORMMAIN_COMMAND_TO_INTERACTION //Команда для вкладки "Взаимодействие"
#if _STATE_MANAGER
            , OMANAGEMENT_ADD
            , OMANAGEMENT_REMOVE
            , OMANAGEMENT_CONFIRM
            , OMANAGEMENT_UPDATE
            , OMANAGEMENT_CONTROL
#else
#endif
            ,
        }
        /// <summary>
        /// Объект для обработки файла конфигурации
        /// </summary>
        private FormMain.FileINI m_fileINI;
        /// <summary>
        /// Массив списков групп источников (по кол-ву панелей: источник, назначение)
        /// </summary>
        private List <GroupSources> [] m_listGroupSources;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public HHandlerQueue(string strNameFileINI)
            : base ()
        {
            //string strNameFileINI = string.Empty;
            ////Получить наименование файла из командной строки
            //string []args = Environment.GetCommandLineArgs ();
            //if (args.Length > 1)
            //{
            //    //strNameFileINI = @"setup_biysktmora.ini";
            //    //strNameFileINI = @"setup_ktstusql.ini";
            //    strNameFileINI = args[1];
            //}
            //else
            //    //Наименование файла "по умолчанию"
            //    strNameFileINI = @"setup.ini";
            if (strNameFileINI == string.Empty || strNameFileINI == null)
            {
                strNameFileINI = @"setup.ini";
            }
            else
                ;
#if _STATE_MANAGER
            m_timerFunc = new System.Threading.Timer(timerFunc);
            m_dictInfoCrashed = new DictInfoCrashed();
            m_listObjects = new ListOManagement();
            eventCrashed += new /*HHandlerQueue.EventHandlerCrashed*/ DelegateObjectFunc(onEvtCrashed);
#endif
            //Прочитать и "разобрать" файл конфигурации
            m_fileINI = new FormMain.FileINI(strNameFileINI);

            m_listGroupSources = new List<GroupSources> [(int)INDEX_SRC.COUNT_INDEX_SRC];
            //Заполнить данными (группами источников) списки элементов массива
            // панель - источник
            setListGroupSources(INDEX_SRC.SOURCE, m_fileINI.AllObjectsSrcGroupSources, m_fileINI.AllObjectsSrcGroupSignals);
            // панель - назначение
            setListGroupSources(INDEX_SRC.DEST, m_fileINI.AllObjectsDestGroupSources, m_fileINI.AllObjectsDestGroupSignals);
        }
        /// <summary>
        /// Обработчик события 'EvtDataAskedHostQueue'
        /// </summary>
        /// <param name="par">Параметры/аргументы события</param>
        private void onEvtDataAskedHostQueue_GroupSources(object par)
        {
            EventArgsDataHost ev = par as EventArgsDataHost;
            object []pars = ev.par as object [];

            // массив параметров
            // id_main - индекс типа группы источников (INDEX_SRC)
            // id_detail - индекс группы источников (идентификатор)
            //  'par' различается в ~ от типа объекта в 'pars[0]'
            // для 'GroupSourcesSrc, GroupSourcesDest' - длина=3 (набор исходный): [0] - индекс группы сигналов, [1] - команда, [2 | при необходимости] - 'ID_HEAD_ASKED_HOST'
            // для 'GroupSourcesDest' - длина=2 (набор изменен): [0] - объект 'GroupSourcesDest', [1] - команда
            GroupSourcesDest grpSrcDest = null; // только для 2-го набора
            int indx = -1; // для разных наборов - различное значение
            ID_HEAD_ASKED_HOST idHeadAskedHost = ID_HEAD_ASKED_HOST.UNKNOWN; // только для исходного набора (для [START | STOP] всегда = 'CONFIRM')
            ID_DATA_ASKED_HOST id_cmd = (ID_DATA_ASKED_HOST)pars[1];

            if (pars[0].GetType().IsPrimitive == true)
            {// действия по набору-1
                indx = (int)pars[0]; // индекс группы сигналов
                if (pars.Length == 2)
                // единственный параметр
                    switch (id_cmd)
                    {
                        //case ID_DATA_ASKED_HOST.START:
                        //    add(new object [] { ev.id_main, indx }, TimeSpan.FromMilliseconds (16667));
                        //    break;
                        case ID_DATA_ASKED_HOST.STOP:
#if _STATE_MANAGER
                            remove(ev.id_main, ev.id_detail, indx);
#endif
                            break;
                        case ID_DATA_ASKED_HOST.TABLE_RES:
#if _STATE_MANAGER
                            update(ev.id_main, ev.id_detail, indx);
#endif
                            break;
                        default:
                            break;
                    }
                else
                // 3 параметра
                    if (pars.Length == 3)
                    {
                        if (pars[2].GetType().IsEnum == true)
                        {//ID_DATA_ASKED_HOST.START, ID_DATA_ASKED_HOST.STOP; ID_HEAD_ASKED_HOST.CONFIRM
                            idHeadAskedHost = (ID_HEAD_ASKED_HOST)pars[2];

                            if (idHeadAskedHost == ID_HEAD_ASKED_HOST.CONFIRM)
#if _STATE_MANAGER
                                confirm(ev.id_main, ev.id_detail, indx)
#else
#endif
                                    ;
                            else
                            // ошибка - переменная имеет непредвиденное значение
                                throw new MissingMemberException(@"HHandleQueue::onEvtDataAskedHostQueue_GroupSources () - ...");
                        }
                        else
                        {//ID_DATA_ASKED_HOST.START
#if _STATE_MANAGER
                            // добавить группу сигналов в список контролируемых
                            add(new object[] { ev.id_main, ev.id_detail, indx }, TimeSpan.FromMilliseconds(((TimeSpan)pars[2]).TotalMilliseconds));
#else
#endif
                        }
                    }
                    else
                        ; // других вариантов по количеству параметров - нет
            }
            else
            {// действия по набору-2 (для установления взаимосвязи между "связанными" (по конф./файлу) по "цепочке" сигналов - групп сигналов - групп источников)
                if (pars[0] is GroupSourcesDest)
                {
                    grpSrcDest = pars[0] as GroupSourcesDest;
                    indx = ev.id_detail; // индекс группы источников

                    foreach (GroupSources grpSrcSource in m_listGroupSources[(int)INDEX_SRC.SOURCE])
                        if (FormMain.FileINI.GetIDIndex(grpSrcSource.m_strID) == indx) //indxNeededGroupSources
                        {
                            if (id_cmd == ID_DATA_ASKED_HOST.START)
                                grpSrcSource.AddDelegatePlugInOnEvtDataAskedHost(FormMain.FileINI.GetIDIndex(grpSrcDest.m_strID), grpSrcDest.Clone_OnEvtDataAskedHost);
                            else
                                if (id_cmd == ID_DATA_ASKED_HOST.STOP)
                                    grpSrcSource.RemoveDelegatePlugInOnEvtDataAskedHost(FormMain.FileINI.GetIDIndex(grpSrcDest.m_strID), grpSrcDest.Clone_OnEvtDataAskedHost);
                                else
                                    ;

                            break;
                        }
                        else
                            ;
                }
                else
                // ошибка - объект имеет неизвестный тип
                    throw new InvalidCastException(@"HHandleQueue::onEvtDataAskedHostQueue_GroupSources () - ...");
            }
        }

        /// <summary>
        /// Заполнить список с информацией о группах источников
        /// </summary>
        /// <param name="indxSrc">Индекс панели</param>
        /// <param name="arGroupSources">Массив с информацией о группах с источниками</param>
        /// <param name="arGroupSignals">Массив с информацией о группах с сигналами</param>
        /// <returns>Признак выполнения функции</returns>
        private int setListGroupSources(INDEX_SRC indxSrc, GROUP_SRC[] arGroupSources, GROUP_SIGNALS_SRC[] arGroupSignals)
        {
            int iRes = 0;

            if (m_listGroupSources[(int)indxSrc] == null)
                m_listGroupSources[(int)indxSrc] = new List<GroupSources>();
            else
                ;

            Type typeObjGroupSources = typeof(GroupSources);
            if (indxSrc == INDEX_SRC.DEST)
                typeObjGroupSources = typeof (GroupSourcesDest);
            else
                ;

            List<GROUP_SIGNALS_SRC> listGroupSignals = new List<GROUP_SIGNALS_SRC>();
            GroupSources grpSrc;
            foreach (GROUP_SRC itemSrc in arGroupSources)
            {
                listGroupSignals.Clear();
                int cnt = (itemSrc as GROUP_SRC).m_listGroupSignalsPars.Count
                    , j = -1;
                for (j = 0; j < cnt; j ++)
                    foreach (GROUP_SIGNALS_SRC itemGrooupSignals in arGroupSignals)
                        if (itemGrooupSignals.m_strID.Equals((itemSrc as GROUP_SRC).m_listGroupSignalsPars[j].m_strId) == true)
                        {
                            listGroupSignals.Add(itemGrooupSignals);
                            break;
                        }
                        else
                            ;

                ////Вариант №1
                //grpSrc = new GroupSources(itemSrc, listGroupSignals);
                //Вариант №2
                grpSrc = Activator.CreateInstance(typeObjGroupSources, new object[] { itemSrc, listGroupSignals }) as GroupSources;
                //if (indxSrc == INDEX_SRC.DEST)
                    grpSrc.EvtDataAskedHostQueue += new DelegateObjectFunc(onEvtDataAskedHostQueue_GroupSources);
                //else ;

                m_listGroupSources[(int)indxSrc].Add(grpSrc);
                                
            }

            return iRes;
        }

        public void AutoStart ()
        {
            for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc ++)
                foreach (GroupSources grpSources in m_listGroupSources[(int)indxSrc])
                    grpSources.AutoStart ();
                    //grpSources.StateChange();
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;
            //ItemQueue itemQueue;

            switch ((StatesMachine)state)
            {
                case StatesMachine.LIST_GROUP_SOURCES:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_GROUP_SIGNALS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                case StatesMachine.OBJ_SRC_GROUP_SOURCES:
                case StatesMachine.OBJ_DEST_GROUP_SOURCES:
                case StatesMachine.TIMER_WORK_UPDATE:
                case StatesMachine.OBJ_SRC_GROUP_SIGNALS_PARS:
                case StatesMachine.OBJ_SRC_GROUP_SIGNALS:
                case StatesMachine.OBJ_DEST_GROUP_SIGNALS_PARS:
                case StatesMachine.OBJ_DEST_GROUP_SIGNALS:
                case StatesMachine.STATE_GROUP_SOURCES:
                case StatesMachine.STATE_GROUP_SIGNALS:
                case StatesMachine.STATE_CHANGED_GROUP_SOURCES:
                case StatesMachine.STATE_CHANGED_GROUP_SIGNALS:
                case StatesMachine.COMMAND_RELAOD_GROUP_SOURCES:
                case StatesMachine.CLEARVALUES_DEST_GROUP_SIGNALS:
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                case StatesMachine.SET_IDCUR_SOURCE_OF_GROUP:
                case StatesMachine.SET_TEXT_ADDING:
                case StatesMachine.SET_GROUP_SIGNALS_PARS:
                case StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
                case StatesMachine.GET_INTERACTION_PARAMETERS:
                case StatesMachine.INTERACTION_EVENT:
                    // группа событий диагностики/контроля
#if _STATE_MANAGER
                case StatesMachine.OMANAGEMENT_ADD:
                case StatesMachine.OMANAGEMENT_REMOVE:
                case StatesMachine.OMANAGEMENT_UPDATE:
                case StatesMachine.OMANAGEMENT_CONFIRM:
                case StatesMachine.OMANAGEMENT_CONTROL:
#endif
                    //Не требуют запроса
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            switch ((StatesMachine)state)
            {
                case StatesMachine.LIST_GROUP_SOURCES:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_GROUP_SIGNALS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                case StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                case StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                case StatesMachine.OBJ_SRC_GROUP_SOURCES:                
                case StatesMachine.OBJ_DEST_GROUP_SOURCES:
                case StatesMachine.TIMER_WORK_UPDATE:
                case StatesMachine.OBJ_SRC_GROUP_SIGNALS_PARS:
                case StatesMachine.OBJ_SRC_GROUP_SIGNALS:
                case StatesMachine.OBJ_DEST_GROUP_SIGNALS_PARS:
                case StatesMachine.OBJ_DEST_GROUP_SIGNALS:
                case StatesMachine.STATE_GROUP_SOURCES:
                case StatesMachine.STATE_GROUP_SIGNALS:
                case StatesMachine.STATE_CHANGED_GROUP_SOURCES:
                case StatesMachine.STATE_CHANGED_GROUP_SIGNALS:                
                case StatesMachine.COMMAND_RELAOD_GROUP_SOURCES:
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                case StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
                case StatesMachine.GET_INTERACTION_PARAMETERS:
                    if ((!(itemQueue == null))
                        && (!(itemQueue.m_dataHostRecieved == null)))
                        itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj });
                    else
                        ;
                    break;
                // группа событий инициированных пользователем (элементы управления на панели)
                case StatesMachine.SET_IDCUR_SOURCE_OF_GROUP:
                case StatesMachine.SET_TEXT_ADDING:
                case StatesMachine.SET_GROUP_SIGNALS_PARS:
                case StatesMachine.CLEARVALUES_DEST_GROUP_SIGNALS:
                case StatesMachine.INTERACTION_EVENT:
                // группа событий диагностики/контроля
#if _STATE_MANAGER
                case StatesMachine.OMANAGEMENT_ADD:
                case StatesMachine.OMANAGEMENT_REMOVE:
                case StatesMachine.OMANAGEMENT_CONFIRM:
                case StatesMachine.OMANAGEMENT_UPDATE:
                case StatesMachine.OMANAGEMENT_CONTROL:
#endif
                    //Ответа не требуется/не требуют обработки результата
                    break;
                default:
                    break;
            }

            return iRes;
        }

        public event DelegateObjectFunc EventInteraction;

        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;
            StatesMachine state = (StatesMachine)s;

            error = true;
            outobj = null;

            ItemQueue itemQueue = null;

            try
            {
                switch (state)
                {
                    #region LIST_GROUP_SOURCES
                    case (int)StatesMachine.LIST_GROUP_SOURCES:
                        error = false;
                        outobj = new object [] {
                            m_fileINI.ListSrcGroupSources
                            , m_fileINI.ListDestGroupSources
                        };

                        iRes = 0;
                        break;
                    #endregion

                    #region LIST_GROUP_SIGNALS
                    case StatesMachine.LIST_GROUP_SIGNALS:
                        error = false;
                        outobj = new object[] {
                            m_fileINI.ListSrcGroupSignals
                            , m_fileINI.ListDestGroupSignals
                        };

                        iRes = 0;
                        break;
                    #endregion

                    #region LIST_SRC_GROUP_SOURCE_ITEMS, LIST_SRC_GROUP_SOURCE_PARS, LIST_SRC_GROUP_SOURCE_PROP, LIST_SRC_GROUP_SIGNAL_ITEMS, LIST_SRC_GROUP_SIGNAL_PARS, LIST_SRC_GROUP_SIGNAL_PROP
                    case StatesMachine.LIST_SRC_GROUP_SOURCE_ITEMS:
                        error = false;
                        itemQueue = Peek;
                        outobj =
                            //m_fileINI.GetListItemsOfGroupSource(itemQueue.Pars.ToArray())
                            m_fileINI.GetListItemsOfGroupSource(itemQueue.Pars.ToArray())
                            ;

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_SRC_GROUP_SOURCE_PARS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListParsOfGroupSource(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_SRC_GROUP_SOURCE_PROP:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemPropOfGroupSource(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;                    
                    case StatesMachine.LIST_SRC_GROUP_SIGNAL_ITEMS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemsOfGroupSignal(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_SRC_GROUP_SIGNAL_PARS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListParsOfGroupSignal(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_SRC_GROUP_SIGNAL_PROP:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemPropOfGroupSignal(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    #endregion

                    #region LIST_DEST_GROUP_SOURCE_ITEMS, LIST_DEST_GROUP_SOURCE_PARS, LIST_DEST_GROUP_SOURCE_PROP, LIST_DEST_GROUP_SIGNAL_ITEMS, LIST_DEST_GROUP_SIGNAL_PARS, LIST_DEST_GROUP_SIGNAL_PROP                        
                    case StatesMachine.LIST_DEST_GROUP_SOURCE_ITEMS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemsOfGroupSource(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_DEST_GROUP_SOURCE_PARS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListParsOfGroupSource(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_DEST_GROUP_SOURCE_PROP:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemPropOfGroupSource(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_DEST_GROUP_SIGNAL_ITEMS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemsOfGroupSignal(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_DEST_GROUP_SIGNAL_PARS:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListParsOfGroupSignal(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.LIST_DEST_GROUP_SIGNAL_PROP:
                        error = false;
                        itemQueue = Peek;
                        outobj = m_fileINI.GetListItemPropOfGroupSignal(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    #endregion

                    #region OBJ_SRC_GROUP_SOURCES, OBJ_DEST_GROUP_SOURCES
                    case StatesMachine.OBJ_SRC_GROUP_SOURCES:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр строковый идентификатор "выбранноой" группы источников
                        outobj = m_fileINI.GetObjectSrcGroupSources((string)itemQueue.Pars[0]);

                        iRes = 0;
                        break;
                    case StatesMachine.OBJ_DEST_GROUP_SOURCES:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр индекс "выбранноой" группы источников
                        outobj = m_fileINI.GetObjectDestGroupSources((string)itemQueue.Pars[0]);

                        iRes = 0;
                        break;
                    #endregion

                    #region TIMER_WORK_UPDATE
                    case StatesMachine.TIMER_WORK_UPDATE:
                        error = false;
                        outobj = m_fileINI.SecondWorkUpdate;

                        iRes = 0;
                        break;
                    #endregion

                    #region OBJ_SRC_GROUP_SIGNALS_PARS, OBJ_SRC_GROUP_SIGNALS, OBJ_DEST_GROUP_SIGNALS_PARS, OBJ_DEST_GROUP_SIGNALS
                    case StatesMachine.OBJ_SRC_GROUP_SIGNALS_PARS:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр идентификатор "выбранноой" группы сигналов
                        outobj = m_fileINI.GetObjectGroupSignalsPars(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.OBJ_SRC_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр идентификатор "выбранноой" группы сигналов
                        outobj = m_fileINI.GetObjectGroupSignals(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.OBJ_DEST_GROUP_SIGNALS_PARS:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр идентификатор "выбранноой" группы сигналов
                        outobj = m_fileINI.GetObjectGroupSignalsPars(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    case StatesMachine.OBJ_DEST_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр идентификатор "выбранноой" группы сигналов
                        outobj = m_fileINI.GetObjectGroupSignals(itemQueue.Pars.ToArray());

                        iRes = 0;
                        break;
                    #endregion

                    #region STATE_GROUP_SOURCES, STATE_GROUP_SIGNALS
                    case StatesMachine.STATE_GROUP_SOURCES:
                        error = false;
                        outobj = new object[(int)INDEX_SRC.COUNT_INDEX_SRC];
                        for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc++)
                        {
                            (outobj as object[])[(int)indxSrc] = new GroupSources.STATE[m_listGroupSources[(int)indxSrc].Count];

                            foreach (GroupSources grpSrc in m_listGroupSources[(int)indxSrc])
                                ((outobj as object[])[(int)indxSrc] as GroupSources.STATE[])[m_listGroupSources[(int)indxSrc].IndexOf(grpSrc)] = grpSrc.State;
                        }

                        iRes = 0;
                        break;
                    case StatesMachine.STATE_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;
                        //??? 0-й параметр индекс "выбранноой" группы сигналов
                        outobj = new object[(int)INDEX_SRC.COUNT_INDEX_SRC];
                        for (INDEX_SRC indxSrc = INDEX_SRC.SOURCE; indxSrc < INDEX_SRC.COUNT_INDEX_SRC; indxSrc++)
                            if (!((int)itemQueue.Pars[(int)indxSrc] < 0))
                                (outobj as object[])[(int)indxSrc] = m_listGroupSources[(int)indxSrc][(int)itemQueue.Pars[(int)indxSrc]].GetArgGroupSignals ();
                            else
                                (outobj as object[])[(int)indxSrc] = new object [] { };

                        iRes = 0;
                        break;
                    #endregion

                    #region STATE_CHANGED_GROUP_SOURCES, STATE_CHANGED_GROUP_SIGNALS
                    case StatesMachine.STATE_CHANGED_GROUP_SOURCES:
                        error = false;
                        itemQueue = Peek;

                        iRes = m_listGroupSources[(int)((INDEX_SRC)itemQueue.Pars[0])][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])].StateChange();
                        break;
                    case StatesMachine.STATE_CHANGED_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;

                        iRes = m_listGroupSources[(int)((INDEX_SRC)itemQueue.Pars[0])][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])].StateChange((string)itemQueue.Pars[2]);
                        break;
                    #endregion

                    #region COMMAND_RELAOD_GROUP_SOURCES
                    case StatesMachine.COMMAND_RELAOD_GROUP_SOURCES:
                        error = false;
                        itemQueue = Peek;

                        iRes = m_listGroupSources[(int)((INDEX_SRC)itemQueue.Pars[0])][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])].Reload();
                        break;
                    #endregion

                    #region CLEARVALUES_DEST_GROUP_SIGNALS
                    case StatesMachine.CLEARVALUES_DEST_GROUP_SIGNALS:
                        //[1] - идентификаторы
                        //[2] = дата/время / продолжительность
                        error = false;
                        itemQueue = Peek;

                        int idGrpSrc = FormMain.FileINI.GetIDIndex((string)(itemQueue.Pars[1] as object [])[0])
                         , idGrpSgnls = FormMain.FileINI.GetIDIndex((string)(itemQueue.Pars[1] as object[])[1]);
                        DateTime dtStartDate = (DateTime)(itemQueue.Pars[2] as object[])[0];
                        TimeSpan tsStartTime = (TimeSpan)(itemQueue.Pars[2] as object[])[1]
                            , tsPeriodMain = (TimeSpan)(itemQueue.Pars[2] as object[])[2];

                        iRes = 0;
                        break;
                    #endregion

                    #region DATA_SRC_GROUP_SIGNALS, DATA_DEST_GROUP_SIGNALS
                    case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                    case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;

                        INDEX_SRC indxGroupSrc = state == StatesMachine.DATA_SRC_GROUP_SIGNALS ? INDEX_SRC.SOURCE :
                            state == StatesMachine.DATA_DEST_GROUP_SIGNALS ? INDEX_SRC.DEST :
                                INDEX_SRC.COUNT_INDEX_SRC;
                        //??? зачем проверка индекса группы источников, как это значение м.б. отрицательным (в элементе управления не выделена ни одна строка!!!)
                        // см. 'PanelWork::fTimerUpdate ()' - из-за того, что при старте /minimize элемент управления не отображается и в нем не назначается выделенная строка
                        if (!((int)itemQueue.Pars[0] < 0))
                            outobj = m_listGroupSources[(int)indxGroupSrc][(int)itemQueue.Pars[0]].GetDataToPanel(itemQueue.Pars[1] as string, out error);
                        else
                            ;

                        iRes = 0;
                        break;                    
                    #endregion

                    #region SET_IDCUR_SOURCE_OF_GROUP, SET_TEXT_ADDING, SET_GROUP_SIGNALS_PARS
                    case StatesMachine.SET_IDCUR_SOURCE_OF_GROUP:
                        error = false;
                        itemQueue = Peek;

                        m_listGroupSources[(int)itemQueue.Pars[0]][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])].m_IDCurrentConnSett = (string)itemQueue.Pars[2];
                        m_fileINI.UpdateParameter((int)itemQueue.Pars[0], (string)itemQueue.Pars[1], @"SCUR", (string)itemQueue.Pars[2]);

                        iRes = 0;
                        break;
                    case StatesMachine.SET_TEXT_ADDING:
                        error = false;
                        itemQueue = Peek;

                        (m_listGroupSources[(int)itemQueue.Pars[0]][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])] as GroupSources).SetAdding(((string)itemQueue.Pars[2]).Split(new char[] { FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL] }));
                        m_fileINI.UpdateParameter((int)itemQueue.Pars[0], (string)itemQueue.Pars[1], @"ADDING", (string)itemQueue.Pars[2]);

                        iRes = 0;
                        break;
                    case StatesMachine.SET_GROUP_SIGNALS_PARS:
                        error = false;
                        itemQueue = Peek;

                        int indxGroupSgnls = -1;
                        GroupSources grpSrcs = (m_listGroupSources[(int)itemQueue.Pars[0]][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])] as GroupSources);
                        indxGroupSgnls = grpSrcs.SetGroupSignalsPars(/*(string)itemQueue.Pars[2],*/ itemQueue.Pars[2] as GROUP_SIGNALS_PARS);
                        //indxGroupSgnls = grpSrcs.getIndexGroupSignalsPars((string)itemQueue.Pars[2]);
                        m_fileINI.UpdateParameter((int)itemQueue.Pars[0], (string)itemQueue.Pars[1], indxGroupSgnls, itemQueue.Pars[2] as GROUP_SIGNALS_PARS);

                        iRes = 0;
                        break;
                    #endregion

                    #region GET_GROUP_SIGNALS_DATETIME_PARS
                    case StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
                        error = false;
                        itemQueue = Peek;

                        GROUP_SIGNALS_SRC_PARS grpSgnlsPars = m_listGroupSources[(int)INDEX_SRC.SOURCE][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])].GetGroupSignalsPars((string)itemQueue.Pars[2]);
                        outobj = grpSgnlsPars.m_arWorkIntervals[(int)itemQueue.Pars[3]];

                        iRes = 0;
                        break;
                    #endregion

                    #region GET_INTERACTION_PARAMETERS
                    case StatesMachine.GET_INTERACTION_PARAMETERS:
                        error = false;
                        itemQueue = Peek;

                        outobj = m_fileINI.m_InteractionPars;

                        iRes = 0;
                        break;
                    #endregion

                    #region INTERACTION_EVENT
                    case StatesMachine.INTERACTION_EVENT:
                        error = false;
                        itemQueue = Peek;

                        if (itemQueue.Pars.Length > 1)
                            EventInteraction(new object [] { itemQueue.Pars[0], itemQueue.Pars[1] });
                        else
                            EventInteraction(new object[] { itemQueue.Pars[0]});

                        iRes = 0;
                        break;
                    #endregion

#if _STATE_MANAGER
                    #region OMANAGEMENT_ADD, OMANAGEMENT_REMOVE, OMANAGEMENT_CONFIRM, OMANAGEMENT_UPDATE, OMANAGEMENT_CONTROL
                    case StatesMachine.OMANAGEMENT_ADD:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        add((ID)itemQueue.Pars[0], (TimeSpan)itemQueue.Pars[1]);
                        break;
                    case StatesMachine.OMANAGEMENT_REMOVE:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        remove((ID)itemQueue.Pars[0]);
                        break;
                    case StatesMachine.OMANAGEMENT_CONFIRM:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        confirm((ID)itemQueue.Pars[0]);
                        break;
                    case StatesMachine.OMANAGEMENT_UPDATE:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        update((ID)itemQueue.Pars[0]);
                        break;
                    case StatesMachine.OMANAGEMENT_CONTROL:
                        iRes = 0;
                        error = false;

                        targetFunc();
                        break;
                    #endregion
#endif
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"HHandlerQueue::StateCheckResponse (state=" + state.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

                error = true;
                iRes = -1 * (int)state;
            }

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            Logging.Logg().Error(@"HHandlerQueue::StateErrors () - не обработана ошибка [" + ((StatesMachine)state).ToString () + @", REQ=" + req + @", RES=" + res + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);

            return HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(@"HHandlerQueue::StateWarnings () - не обработано предупреждение [" + ((StatesMachine)state).ToString() + @", REQ=" + req + @", RES=" + res + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        public override void Stop()
        {
            for (int i = 0; i < m_listGroupSources.Length; i ++)
                foreach (GroupSources grpSrc in m_listGroupSources[i])
                {
                    if (grpSrc.State == GroupSources.STATE.STARTED)
                        grpSrc.Stop();
                    else
                        ;
                    //??? следует ожидать остановки (подтверждения) всех групп сигналов
                    //grpSrc.Unload();
                }
            
            base.Stop();
        }
    }
}
