using System;
using System.Collections.Generic;
using System.Linq;

using HClassLibrary; //HHandler

namespace uLoader
{
    class HHandlerQueue : HClassLibrary.HHandlerQueue
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
            , CLEARVALUES_DEST_GROUP_SIGNALS //Очистить значения для группы сигналов - инициатива пользователя
            , DATA_SRC_GROUP_SIGNALS //Данные группы сигналов (источник)
            , DATA_DEST_GROUP_SIGNALS //Данные группы сигналов (назначение)
            , SET_IDCUR_SOURCE_OF_GROUP //Установить идентификатор текущего источника
            , SET_TEXT_ADDING //Установить текст "дополнительных" параметров
            , SET_GROUP_SIGNALS_PARS //Установить параметры группы сигналов в группе источников при утрате фокуса ввода элементом управления (GroupBox) с их значениями
            , GET_GROUP_SIGNALS_DATETIME_PARS //Запросить параметры группы сигналов в группе источников при изменении типа параметров (CUR_DATETIME, COSTUMIZE)
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
        public HHandlerQueue()
            : base ()
        {
            string strNameFileINI = string.Empty;
            //Получить наименование файла из командной строки
            string []args = Environment.GetCommandLineArgs ();
            if (args.Length > 1)
            {
                //strNameFileINI = @"setup_biysktmora.ini";
                //strNameFileINI = @"setup_ktstusql.ini";
                strNameFileINI = args[1];
            }
            else
                //Наименование файла "по умолчанию"
                strNameFileINI = @"setup.ini";
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
        /// Обработчик события 'EvtDataAskedHostQueue' для установления взаимосвязи между "связанными" (по конф./файлу) по "цепочке" сигналов - групп сигналов - групп источников
        /// </summary>
        /// <param name="par">Параметры для установления соответствия</param>
        private void onEvtDataAskedHostQueue_GroupSourcesDest(object par)
        {
            EventArgsDataHost ev = par as EventArgsDataHost;
            object []pars = ev.par as object [];

            GroupSourcesDest grpSrcDest = pars[0] as GroupSourcesDest;

            int indxNeededGroupSources = (int)pars[1];

            foreach (GroupSources grpSrcSource in m_listGroupSources[(int)INDEX_SRC.SOURCE])
            {
                if (FormMain.FileINI.GetIDIndex(grpSrcSource.m_strID) == indxNeededGroupSources)
                {
                    if ((ID_DATA_ASKED_HOST)ev.id == ID_DATA_ASKED_HOST.START)
                        grpSrcSource.AddDelegatePlugInOnEvtDataAskedHost(FormMain.FileINI.GetIDIndex(grpSrcDest.m_strID), grpSrcDest.Clone_OnEvtDataAskedHost);
                    else
                        if ((ID_DATA_ASKED_HOST)ev.id == ID_DATA_ASKED_HOST.STOP)
                            grpSrcSource.RemoveDelegatePlugInOnEvtDataAskedHost(FormMain.FileINI.GetIDIndex(grpSrcDest.m_strID), grpSrcDest.Clone_OnEvtDataAskedHost);
                        else
                            ;

                    break;
                }
                else
                    ;
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
                if (indxSrc == INDEX_SRC.DEST)
                    (grpSrc as GroupSourcesDest).EvtDataAskedHostQueue += new DelegateObjectFunc(onEvtDataAskedHostQueue_GroupSourcesDest);
                else
                    ;

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
                case StatesMachine.CLEARVALUES_DEST_GROUP_SIGNALS:
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                case StatesMachine.SET_IDCUR_SOURCE_OF_GROUP:
                case StatesMachine.SET_TEXT_ADDING:
                case StatesMachine.SET_GROUP_SIGNALS_PARS:
                case StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
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
                case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                case StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
                    itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj });
                    break;
                case StatesMachine.SET_IDCUR_SOURCE_OF_GROUP:
                case StatesMachine.SET_TEXT_ADDING:
                case StatesMachine.SET_GROUP_SIGNALS_PARS:
                case StatesMachine.CLEARVALUES_DEST_GROUP_SIGNALS:
                    //Ответа не требуется
                    break;
                default:
                    break;
            }

            return iRes;
        }

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
                    case (int)StatesMachine.LIST_GROUP_SOURCES:
                        error = false;
                        outobj = new object [] {
                            m_fileINI.ListSrcGroupSources
                            , m_fileINI.ListDestGroupSources
                        };

                        iRes = 0;
                        break;
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
                    case StatesMachine.LIST_GROUP_SIGNALS:
                        error = false;
                        outobj = new object[] {
                            m_fileINI.ListSrcGroupSignals
                            , m_fileINI.ListDestGroupSignals
                        };

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
                    case StatesMachine.TIMER_WORK_UPDATE:
                        error = false;
                        outobj = m_fileINI.SecondWorkUpdate;

                        iRes = 0;
                        break;
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
                    case StatesMachine.DATA_SRC_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;

                        outobj = m_listGroupSources[(int)INDEX_SRC.SOURCE][(int)itemQueue.Pars[0]].GetDataToPanel(itemQueue.Pars[1] as string, out error);

                        iRes = 0;
                        break;
                    case StatesMachine.DATA_DEST_GROUP_SIGNALS:
                        error = false;
                        itemQueue = Peek;

                        if (! ((int)itemQueue.Pars[0] < 0))
                            outobj = m_listGroupSources[(int)INDEX_SRC.DEST][(int)itemQueue.Pars[0]].GetDataToPanel(itemQueue.Pars[1] as string, out error);
                        else
                            ;

                        iRes = 0;
                        break;
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
                    case StatesMachine.GET_GROUP_SIGNALS_DATETIME_PARS:
                        error = false;
                        itemQueue = Peek;

                        GROUP_SIGNALS_SRC_PARS grpSgnlsPars = m_listGroupSources[(int)INDEX_SRC.SOURCE][FormMain.FileINI.GetIDIndex((string)itemQueue.Pars[1])].GetGroupSignalsPars((string)itemQueue.Pars[2]);
                        outobj = grpSgnlsPars.m_arWorkIntervals[(int)itemQueue.Pars[3]];

                        iRes = 0;
                        break;
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
                    if (grpSrc.State == GroupSources.STATE.STARTED)
                        grpSrc.Stop ();
                    else
                        ;
            
            base.Stop();
        }
    }
}
