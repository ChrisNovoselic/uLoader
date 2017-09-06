using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using HClassLibrary;

namespace uLoader
{

    public class GroupSourcesDest : GroupSources
    {
        /// <summary>
        /// Словарь с ключами - индексами групп источников
        ///  значениями - списками индексов групп сигналов
        ///  , подписчиками на таблицы результатов
        /// </summary>
        Dictionary<int, List<int>> m_dictLinkedIndexGroupSources;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="grpSrc">Объект группы источников из файла конфигурации</param>
        /// <param name="listGrpSgnls">Список объектов групп сигналов из файла конфигурации</param>
        public GroupSourcesDest(GROUP_SRC grpSrc, List<GROUP_SIGNALS_SRC> listGrpSgnls)
            : base(grpSrc, listGrpSgnls)
        {
            m_dictLinkedIndexGroupSources = new Dictionary<int, List<int>>();

            List<int> listNeededIndexGroupSources = GetListNeededIndexGroupSources();
            foreach (int indx in listNeededIndexGroupSources)
                m_dictLinkedIndexGroupSources.Add(indx, new List<int>());
        }
        /// <summary>
        /// Класс для описания группы сигналов назначения
        /// </summary>
        protected class GroupSignalsDest : GroupSignals
        {
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="grpSgnls">Объект группы сигналов из файла конфигурации</param>
            public GroupSignalsDest(GROUP_SIGNALS_SRC grpSgnls) : base(grpSgnls)
            {
            }
            ///// <summary>
            ///// Получить список индексов групп источников
            /////  , являющимися источниками значений для группы сигналов
            ///// </summary>
            ///// <returns></returns>
            //public List<int> GetListNeededIndexGroupSources()
            //{
            //    List<int> listRes = new List<int>();

            //    int indxGroupSrc = -1
            //        , iPosGroupSrc = m_listSKeys.IndexOf (@"ID_GROUP_SOURCES");
            //    //Поиск индексов групп источников
            //    foreach (SIGNAL_SRC sgnl in m_listSgnls)
            //    {
            //        //Получить индекс группы источников
            //        indxGroupSrc = FormMain.FileINI.GetIDIndex(sgnl.m_arSPars[iPosGroupSrc]);

            //        if (listRes.IndexOf (indxGroupSrc) < 0)
            //            listRes.Add(indxGroupSrc);
            //        else
            //            ;
            //    }

            //    return listRes;
            //}

            /// <summary>
            /// Получить список индексов групп сигналов, использующихся в качестве источников
            /// </summary>
            /// <returns>Список индексов групп сигналов</returns>
            public List<int> GetListNeededIndexGroupSignals()
            {
                List<int> listRes = new List<int>();

                int iIdGrpSgnls = -1;

                if (!(m_listSgnls == null))
                    foreach (SIGNAL_SRC sgnl in m_listSgnls) {
                        iIdGrpSgnls = Convert.ToInt16(sgnl.m_arSPars[m_listSKeys.IndexOf(@"ID_SRC_SGNL")].Substring(1, 2)) - 1;
                        if (listRes.IndexOf(iIdGrpSgnls) < 0)
                            listRes.Add(iIdGrpSgnls);
                        else
                            ;
                    } else
                    ;

                return listRes;
            }
        }
        /// <summary>
        /// Создать группу сигналов
        /// </summary>
        /// <param name="grpSgnls">Объект группы сигналов из файла конфигурации</param>
        /// <returns>Объект группы сигналов</returns>
        protected override GroupSignals createGroupSignals(GROUP_SIGNALS_SRC grpSgnls)
        {
            return createGroupSignals(typeof(GroupSignalsDest), grpSgnls) as GroupSignals;
        }
        /// <summary>
        /// Получить список индексов групп сигналов
        /// </summary>
        /// <returns>Список индексов ожидаемых групп сигналов</returns>
        public List<int> GetListNeededIndexGroupSignals()
        {
            List<int> listRes = new List<int>()
                , listGrpSgnls = new List<int>();

            foreach (GroupSourcesDest.GroupSignalsDest grpSgnls in m_listGroupSignals) {
                ////Вариант №1
                //listRes.Union(grpSgnls.GetListNeededIndexGroupSignals());
                //Вариант №2
                listGrpSgnls = grpSgnls.GetListNeededIndexGroupSignals();
                foreach (int id in listGrpSgnls)
                    if (listRes.IndexOf(id) < 0)
                        listRes.Add(id);
                    else
                        ;
            }

            return listRes;
        }
        /// <summary>
        /// Получить список индексов групп источников
        ///  , являющимися источниками значений для указанной группы сигналов (назначения)
        /// </summary>
        /// <param name="id">Целочисленный идентификатор (индекс) группы сигналов</param>
        /// <returns>Список индексов групп источников</returns>
        public List<int> GetListNeededIndexGroupSources(int id)
        {
            List<int> listRes = new List<int>();
            GroupSourcesDest.GroupSignalsDest grpSgnls = getGroupSignals(id) as GroupSignalsDest;
            GROUP_SIGNALS_DEST_PARS grpSgnlsPars = getGroupSignalsPars(id) as GROUP_SIGNALS_DEST_PARS;

            listRes = new List<int>() { FormMain.FileINI.GetIDIndex(grpSgnlsPars.m_idGrpSrcs) };

            return listRes;
        }
        /// <summary>
        /// Получить список индексов групп источников
        ///  , являющимися источниками значений для группы источников (назначения)
        /// </summary>
        /// <returns>Список индексов групп источников</returns>
        public List<int> GetListNeededIndexGroupSources()
        {
            List<int> listRes = new List<int>()
                , listGrpSrcs = new List<int>();

            foreach (GroupSignalsDest grpSgnls in m_listGroupSignals) {
                ////Вариант №1
                //listRes.Union(grpSgnls.GetListNeededIndexGroupSignals());
                //Вариант №2
                listGrpSrcs = GetListNeededIndexGroupSources(FormMain.FileINI.GetIDIndex(grpSgnls.m_strID));
                foreach (int id in listGrpSrcs)
                    if (listRes.IndexOf(id) < 0)
                        listRes.Add(id);
                    else
                        ;
            }

            return listRes;
        }
        /// <summary>
        /// Возвратить список идентификаторов групп сигналов (в составе групп источников), являющихся получателями (подписчиками) информации
        ///  от группы сигналов в составе группы источников, указанной в аргументах
        /// </summary>
        /// <param name="idGrpSourceSrc">Идентификатор (индекс) группы источников</param>
        /// <param name="idGrpSgnls">Идентификатор группы сигналов</param>
        /// <returns>Список с идентификаторами групп сигналов</returns>
        public List<int> GetListLinkedIndexGroupSignals(int idGrpSourceSrc, int idGrpSgnls)
        {
            List<int> listRes = new List<int>()
                , listNeededIndexGroupSignals = null;

            if ((m_dictLinkedIndexGroupSources.ContainsKey(idGrpSourceSrc) == true)
                && (m_dictLinkedIndexGroupSources[idGrpSourceSrc].Contains(idGrpSgnls) == true))
                foreach (GroupSourcesDest.GroupSignalsDest grpSgnls in m_listGroupSignals) {
                    listNeededIndexGroupSignals = grpSgnls.GetListNeededIndexGroupSignals();

                    if (listNeededIndexGroupSignals.Contains(idGrpSgnls) == true)
                        listRes.Add(idGrpSgnls);
                    else
                        ;
                } else
                ; // ни одна из групп сигналов не подписана на целевую группу сигналов 'idGrpSgnls', указанной группы источников 'idGrpSgnls'

            return listRes;
        }
        /// <summary>
        /// Получает сообщения от библиотеки из "другого" (источника) объекта
        /// </summary>
        /// <param name="obj"></param>
        public void Clone_OnEvtDataAskedHost(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            int iIDGroupSignals = 0; //??? д.б. указана в "запросе"
            //pars[0] - идентификатор события
            //pars[1] - идентификатор группы сигналов
            //pars[2] - таблица с данными для "вставки"
            //??? pars[3] - object [] с доп./параметрами, для ретрансляции
            object[] pars = ev.par as object[];
            object[] parsToSend = null;

            //Logging.Logg().Debug(string.Format(@"GroupSourcesDest::Clone_OnEvtDataAskedHost (ev.par.Length={0}) - NAME={1}...", pars.Length, m_strShrName), Logging.INDEX_MESSAGE.NOT_SET);

            //pars[0] - идентификатор события
            switch ((ID_DATA_ASKED_HOST)pars[0]) {
                case ID_DATA_ASKED_HOST.INIT_SOURCE: //Получен запрос на парметры инициализации
                    break;
                case ID_DATA_ASKED_HOST.INIT_SIGNALS: //Получен запрос на обрабатываемую группу сигналов
                    break;
                case ID_DATA_ASKED_HOST.TABLE_RES:
                    parsToSend = new object[pars.Length - 1];
                    //Проверить таблицу со значениями от библиотеки на 'null'
                    if ((!(pars[2] == null))
                        && ((pars[2] as DataTable).Rows.Count > 0)) {
                        //Заполнить для передачи основные параметры - таблицу
                        parsToSend[1] = (pars[2] as DataTable).Copy();
                        //Проверить наличие дополнительных параметров
                        //??? 07.12.2015 лишнее 'Dest' не обрабатывает - см. TO_START
                        if ((parsToSend.Length > 2)
                            && (pars.Length > 3)
                            && (!(pars[3] == null))) {
                            parsToSend[2] = new object[(pars[3] as object[]).Length];
                            //Заполнить для передачи дополнительные параметры - массив объектов
                            ////Вариант №1
                            //for (int i = 0; i < (parsToSend[2] as object []).Length; i ++)
                            //    (parsToSend[2] as object [])[i] = (pars[3] as object[])[i];
                            //Вариант №2
                            (pars[3] as object[]).CopyTo(parsToSend[2] as object[], 0);
                        } else
                            ;
                        //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                        foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                            if (!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0)) {//Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]'
                                parsToSend[0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                                PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.TO_INSERT, parsToSend));

                                //Logging.Logg().Debug(@"GroupSources::Clone_OnEvtDataAskedHost () - NAME=" + m_strShrName + @", от [ID=" + (int)pars[1] + @"] для [ID=" + parsToSend[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
                            } else
                                ;
                    } else
                        ; // таблица со значениями от библиотеки = null
                    break;
                //case ID_DATA_ASKED_HOST.START:
                //    parsToSend = new object[(pars[3] as object[]).Length + 1]; // '+1' для идентификатора группы сигналов
                //    //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                //    foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                //        if (!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0))
                //        {//Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]'
                //            parsToSend [0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                //            parsToSend[1] = (MODE_WORK)(pars[3] as object[])[0]; //MODE_WORK
                //            parsToSend[2] = (int)(pars[3] as object[])[1]; //IdSourceConnSett
                //            parsToSend[3] = (int)(pars[3] as object[])[2]; //ID_TEC
                //            PerformDataAskedHostPlugIn(new EventArgsDataHost((int)ID_DATA_ASKED_HOST.TO_START, parsToSend));

                //            //Logging.Logg().Debug(@"GroupSources::Clone_OnEvtDataAskedHost () - NAME=" + m_strShrName + @", от [ID=" + (int)pars[1] + @"] для [ID=" + parsToSend[0] + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
                //        }
                //        else
                //            ;
                //    break;
                case ID_DATA_ASKED_HOST.STOP:
                    parsToSend = new object[1];
                    //Установить взаимосвязь между полученными значениями группы сигналов и группой сигналов назначения
                    foreach (GroupSignalsDest grpSgnls in m_listGroupSignals)
                        if ((!(grpSgnls.GetListNeededIndexGroupSignals().IndexOf((int)pars[1]) < 0))
                            && (grpSgnls.State == STATE.STARTED)) {
                            parsToSend[0] = FormMain.FileINI.GetIDIndex(grpSgnls.m_strID);
                            //Да, группа сигналов 'grpSgnls' ожидает значения от группы сигналов '(int)pars[1]';
                            PerformDataAskedHostPlugIn(new EventArgsDataHost(m_iIdTypePlugInObjectLoaded, (int)ID_DATA_ASKED_HOST.TO_STOP, parsToSend));
                        } else
                            ;
                    break;
                case ID_DATA_ASKED_HOST.ERROR:
                    iIDGroupSignals = (int)pars[1];
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Передать в очередь обработки событий сообщение о необходимости установления/разрыва связи между группами источников
        /// </summary>
        /// <param name="ev">Аргумент при передаче сообщения</param>
        public override void PerformDataAskedHostQueue(EventArgsDataHost ev)
        {
            //id_main - идентификатор типа объекта в загруженной в ОЗУ библиотеки
            //id_detail - команда на изменение состояния группы сигналов
            //В 0-ом параметре передан индекс (???идентификатор) группы сигналов
            int indxGrpSgnls = (int)(ev.par as object[])[0];
            //Во 2-ом параметре передан признак инициирования/подтверждения изменения состояния группы сигналов
            ID_DATA_ASKED_HOST idDataAskedHost = (ID_DATA_ASKED_HOST)(ev.par as object[])[1];
            ////Во 2-ом параметре передан признак инициирования/подтверждения изменения состояния группы сигналов
            //ID_HEAD_ASKED_HOST idHeadAskedHost = (ID_HEAD_ASKED_HOST)(ev.par as object[])[2];

            base.PerformDataAskedHostQueue(ev);

            List<int> listNeededIndexGroupSources = GetListNeededIndexGroupSources(indxGrpSgnls);
            bool bEvtDataAskedHostQueue = false;

            lock (this) {
                foreach (int indx in listNeededIndexGroupSources) {
                    bEvtDataAskedHostQueue = false;

                    if (m_dictLinkedIndexGroupSources.ContainsKey(indx) == true)
                        if (idDataAskedHost == ID_DATA_ASKED_HOST.START) {
                            m_dictLinkedIndexGroupSources[indx].Add(indxGrpSgnls);

                            if (m_dictLinkedIndexGroupSources[indx].Count == 1)
                                bEvtDataAskedHostQueue = true;
                            else
                                ;
                        } else
                            if (idDataAskedHost == ID_DATA_ASKED_HOST.STOP) {
                            m_dictLinkedIndexGroupSources[indx].Remove(indxGrpSgnls);

                            if (m_dictLinkedIndexGroupSources[indx].Count == 0)
                                // удаление выполнено корректно
                                bEvtDataAskedHostQueue = true;
                            else
                                ;
                        } else
                            ;
                    else
                        ;

                    if (bEvtDataAskedHostQueue == true)
                        base.PerformDataAskedHostQueue(new EventArgsDataHost(
                            (int)INDEX_SRC.SOURCE //!!! подмена - передать "чужой" тип группы источников (не используется в обработчике)
                            , indx //!!! подмена - передать индекс "чужой" группы источников, но связанной с текущей
                            , new object[] { this //!!! подмена - вместо идентификатора группы сигналов: сам текущий объект группы источников
                                , ev.par[1] } // стандартно - команда, копия из полученного объекта - аргумента события
                            ));
                    else
                        ;
                }
            }
        }
    }
}
