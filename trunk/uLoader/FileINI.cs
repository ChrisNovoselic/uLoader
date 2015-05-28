using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
    public partial class FormMain
    {
        public class FileINI : HClassLibrary.FileINI
        {
            /// <summary>
            /// Перечисление типов групп для панели (группы источников, группы сигналов)
            /// </summary>
            private enum INDEX_TYPE_GROUP
            {
                SRC, SIGNAL
                    , COUNT_INDEX_TYPE_GROUP
            };            
            /// <summary>
            /// Индексы для ключей - источники
            /// </summary>
            enum INDEX_KEY_SRC { GROUP_SRC, SRC_OF_GROUP
                , COUNT_INDEX_KEY_SRC
            };
            /// <summary>
            /// Индексы для ключей - сигналы
            /// </summary>
            enum INDEX_KEY_SIGNAL { GROUP_SIGNALS, SIGNAL_OF_GROUP
                , COUNT_INDEX_KEY_SIGNAL
            };
            /// <summary>
            /// Значения из главной секции - составные части "смысловых" секций, ключи в "смысловых" секциях
            /// </summary>
            string[] SEC_SRC_TYPES
                , KEY_TREE_SRC
                , KEY_TREE_SGNLS;
            string KEY_PARS;
            /// <summary>
            /// Список групп источников
            /// </summary>
            SRC []m_arListGroupValues;
            /// <summary>
            /// Период времени (секунды) для обновления информации на панели "Работа"
            /// </summary>
            public int SecondWorkUpdate { get { return m_iSecPanelWorkUpdate; } }
            private int m_iSecPanelWorkUpdate;
            /// <summary>
            /// Конструктор - основной
            /// </summary>
            /// <param name="nameFile">Наименование файла конфигурации</param>
            public FileINI (string nameFile)
                : base(nameFile, true)
            {
                //Получить наименования частей секций
                SEC_SRC_TYPES = GetMainValueOfKey(@"SEC_SRC_TYPES").Split(s_chSecDelimeters[(int)INDEX_DELIMETER.VALUES]);
                KEY_TREE_SRC = GetMainValueOfKey(@"KEY_TREE_SRC").Split(s_chSecDelimeters[(int)INDEX_DELIMETER.VALUES]);
                KEY_TREE_SGNLS = GetMainValueOfKey(@"KEY_TREE_SGNLS").Split(s_chSecDelimeters[(int)INDEX_DELIMETER.VALUES]);
                //Получить ключ для чтения параметров в секции
                KEY_PARS = GetMainValueOfKey(@"KEY_PARS");
                //Получить период для обновления информации на панели "Работа"
                Logging.Logg().Debug (@"FileINI::ctor () - PANEL_WORK_UPDATE = " + GetMainValueOfKey(@"PANEL_WORK_UPDATE"), Logging.INDEX_MESSAGE.NOT_SET);
                m_iSecPanelWorkUpdate = Int32.Parse (GetMainValueOfKey(@"PANEL_WORK_UPDATE"));
                //if (Int32.TryParse (GetMainValueOfKey(@"PANEL_WORK_UPDATE"), out m_iSecPanelWorkUpdate) == false)
                //    throw new Exception(@"FileINI::FileINI () - Параметр PANEL_WORK_UPDATE не удалось инициализировать ...");
                //else ;

                //Создать все объекты, списки для значений из файла конфигурации
                m_arListGroupValues = new SRC [(int)INDEX_SRC.COUNT_INDEX_SRC];
                for (INDEX_SRC i = INDEX_SRC.SOURCE; i < INDEX_SRC.COUNT_INDEX_SRC; i++)
                    m_arListGroupValues[(int)i] = new SRC();
                foreach (SRC src in m_arListGroupValues)
                {
                    src.m_listGroupSrc = new List<GROUP_SRC>();
                    src.m_listGroupSgnlsSrc = new List<GROUP_SIGNALS_SRC>();
                }

                if (SEC_SRC_TYPES.Length == (int)INDEX_SRC.COUNT_INDEX_SRC)
                    for (INDEX_SRC i = INDEX_SRC.SOURCE; i < INDEX_SRC.COUNT_INDEX_SRC; i++)
                    {
                        //Получить наименование секции для группы источников (в ~ от 'i')
                        string sec = SEC_SRC_TYPES[(int)i];
                        //Получить словарь параметров для панели 'Источник'
                        Dictionary<string, string> dictSecValues = getSecValues(sec);

                        //Получить группы источников, сигналов (источник)
                        if (!(dictSecValues == null))
                        {
                            //Получить все группы источников
                            fillGroupValues(dictSecValues, sec, KEY_TREE_SRC[(int)INDEX_KEY_SRC.GROUP_SRC], i, typeof(GROUP_SRC));

                            //Получить все группы сигналов
                            fillGroupValues(dictSecValues, sec, KEY_TREE_SGNLS[(int)INDEX_KEY_SRC.GROUP_SRC], i, typeof(GROUP_SIGNALS_SRC));
                        }
                        else
                            ;
                    }
                else
                    ;
            }

            /// <summary>
            /// Заполнить объект группы значениями из секции файла конфигурации
            /// </summary>
            /// <param name="dictValues">Словарь значений с перечислением всех групп панели</param>
            /// <param name="secTarget">Наименование целевой секции панели (источник, назначение)</param>
            /// <param name="secPart">Часть наименования секции со значенями для группы</param>
            /// <param name="indxNewSrc">Индекс панели (источник, назначение)</param>
            /// <param name="typeNewGroup">Тип группы</param>
            /// <returns>Результат выполнения функции</returns>
            private int fillGroupValues(Dictionary<string, string> dictValues, string secTarget, string secPart, INDEX_SRC indxNewSrc, Type typeNewGroup)
            {
                int iRes = 0 //Результат
                    , i = -1; //Номер группы

                string key = string.Empty;

                i = 0;
                //Получить все группы источников
                while (true)
                {
                    //Получить часть наименования секции
                    key = secPart + i.ToString();

                    if (dictValues.ContainsKey(key) == true)
                    {
                        //Добавить группу...
                        addGroupValues(indxNewSrc, typeNewGroup, secTarget + s_chSecDelimeters[(int)INDEX_DELIMETER.SEC_PART_TARGET] + key);                        
                    }
                    else
                        break;
                    //Увеличить индекс группы
                    i ++;
                }

                return iRes;
            }

            private int parseWorkInterval(string val, ref DATETIME_WORK dtWorkRes)
            {
                int iRes = 0;

                dtWorkRes.m_dtStart = DateTime.Now;
                dtWorkRes.m_tsPeriod = TimeSpan.FromSeconds (60);
                dtWorkRes.m_iInterval = -1;

                return iRes;
            }

            /// <summary>
            /// Добавить группу и ее значения
            /// </summary>
            /// <param name="indxSrc">Индекс панели (источник, неазначение)</param>
            /// <param name="type">Тип группы</param>
            /// <param name="secGroup">Наименование для секции со значениями параметров группы</param>
            /// <returns>Результат выполнения</returns>
            private int addGroupValues (INDEX_SRC indxSrc, Type type, string secGroup)
            {
                int iRes = 0; //Результат выполнения
                //Индекс типа элемента группы (источник, сигнал)
                INDEX_TYPE_GROUP typeGroup;

                ////Вариант №1
                //switch (typeof(type))
                //{
                //    default:
                //        break;
                //}
                //Вариант №2
                ITEM_SRC itemSrc = null;
                if (type == typeof(GROUP_SRC))
                {//Источник
                    typeGroup = INDEX_TYPE_GROUP.SRC;
                }
                else
                    if (type == typeof(GROUP_SIGNALS_SRC))
                    {//Сигнал
                        typeGroup = INDEX_TYPE_GROUP.SIGNAL;
                    }
                    else
                        throw new Exception(@"FileINI::addGroupValues () - неизвестный тип группы: " + type.FullName);

                //Добавить элемент группы
                switch (typeGroup)
                {
                    case INDEX_TYPE_GROUP.SRC: //Добавить группу источников
                        itemSrc = new GROUP_SRC();
                        (itemSrc as GROUP_SRC).m_IDCurrentConnSett = GetSecValueOfKey(secGroup, @"SCUR");
                        (itemSrc as GROUP_SRC).m_strDLLName = GetSecValueOfKey(secGroup, @"DLL_NAME");
                        (itemSrc as GROUP_SRC).m_arIDGroupSignals = GetSecValueOfKey(secGroup, KEY_TREE_SGNLS[(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS]).Split(s_chSecDelimeters[(int)INDEX_DELIMETER.PAIR_VAL]);
                        m_arListGroupValues[(int)indxSrc].m_listGroupSrc.Add(itemSrc as GROUP_SRC);                        
                        break;
                    case INDEX_TYPE_GROUP.SIGNAL: //Добавить группу сигналов
                        itemSrc = new GROUP_SIGNALS_SRC();
                        (itemSrc as GROUP_SIGNALS_SRC).m_iAutoStart = bool.Parse(GetSecValueOfKey(secGroup, @"AUTO_START")) == true ? 1 : 0;
                        (itemSrc as GROUP_SIGNALS_SRC).m_mode = bool.Parse(GetSecValueOfKey(secGroup, @"CUR_INTERVAL_STATE")) == true ? MODE_WORK.CUR_INTERVAL : MODE_WORK.COSTUMIZE;
                        if (Int32.TryParse(GetSecValueOfKey(secGroup, @"CUR_INTERVAL_VALUE"), out (itemSrc as GROUP_SIGNALS_SRC).m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_iInterval) == false)
                            (itemSrc as GROUP_SIGNALS_SRC).m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_iInterval = -1;
                        else
                            ;
                        parseWorkInterval(GetSecValueOfKey(secGroup, @"COSTUMIZE_VALUE"), ref (itemSrc as GROUP_SIGNALS_SRC).m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE]);
                        m_arListGroupValues[(int)indxSrc].m_listGroupSgnlsSrc.Add(itemSrc as GROUP_SIGNALS_SRC);
                        break;
                    default:
                        break;
                }

                //Проверить наличие секции
                if ((isSec(secGroup) == true)
                    && (! (itemSrc == null)))
                {
                    //Присвоить наименование группы элементов (источников, сигналов)
                    itemSrc.m_strID = secGroup.Split (s_chSecDelimeters[(int)INDEX_DELIMETER.SEC_PART_TARGET])[1];
                    
                    int j = -1; //Индекс для ключа элемента группы (источник, сигнал) в секции
                    string key = string.Empty; //Ключ для элемента группы (источник, сигнал) в секции

                    //Получить словарь значений секции
                    Dictionary<string, string> dictSecValues = getSecValues(secGroup);
                    string[] values //ЗначениЕ для элемента группы
                        , vals; //ЗначениЕ для 1-го параметра элемента группы
                    //ЗначениЯ для элемента группы
                    // только для источника, т.к. для сигнала ... (см. 'SIGNAL_SRC')
                    Dictionary <string, string> dictItemValues;

                    //Проверить наличие значений в секции
                    if (!(dictSecValues == null))
                    {
                        //Получить наименовния параметров для элемента группы (источник, сигнал)
                        itemSrc.m_keys = GetSecValueOfKey(secGroup, KEY_PARS).Split (s_chSecDelimeters[(int)INDEX_DELIMETER.PAIR_VAL]);

                        foreach (string parName in itemSrc.m_keys)
                            if (parName.Equals (string.Empty) == true)
                                throw new Exception (@"FileINI::addGroupValues (" + indxSrc.ToString () + @", " + type.AssemblyQualifiedName + @", " + secGroup + @") - ...");
                            else
                                ;

                        j = 0; //1-ый индекс == 0
                        while (true)
                        {
                            //Сфрмировать ключ элемента группы в секции
                            switch (typeGroup)
                            {
                                case INDEX_TYPE_GROUP.SRC: //Источник
                                    key = KEY_TREE_SRC[(int)INDEX_KEY_SRC.SRC_OF_GROUP];
                                    break;
                                case INDEX_TYPE_GROUP.SIGNAL: //Сигнал
                                    key = KEY_TREE_SGNLS[(int)INDEX_KEY_SIGNAL.SIGNAL_OF_GROUP];
                                    break;
                                default:
                                    break;
                            }

                            //Добавить к ключу индекс
                            key += j.ToString();

                            //Проверить наличие ключа в сеекции
                            if (isSecKey(secGroup, key) == true)
                            {
                                //Получить значение по ключу для элмента группы (источник, сигнал)
                                values = GetSecValueOfKey(secGroup, key).Split(s_chSecDelimeters[(int)INDEX_DELIMETER.PAIR_VAL]);

                                if (values.Length == itemSrc.m_keys.Length)
                                    switch (typeGroup)
                                    {
                                        case INDEX_TYPE_GROUP.SRC: //Источник
                                            //Инициализация, если элемент группы 1-ый
                                            if ((itemSrc as GROUP_SRC).m_listConnSett == null)
                                                (itemSrc as GROUP_SRC).m_listConnSett = new List<ConnectionSettings>();
                                            else
                                                ;

                                            dictItemValues = new Dictionary<string, string>();
                                            foreach (string valPar in values)
                                            {
                                                vals = valPar.Split(s_chSecDelimeters[(int)INDEX_DELIMETER.VALUE]);
                                                if ((vals.Length == 2)
                                                    //&& (! (itemSrc.m_keys.IndexOf () < 0))
                                                    )
                                                    dictItemValues.Add(vals[0], vals[1]);
                                                else
                                                    throw new Exception(@"FileINI::ctor () - addGroupValues () ...");
                                            }

                                            (itemSrc as GROUP_SRC).m_listConnSett.Add(new ConnectionSettings(
                                                Int32.Parse(dictItemValues[@"ID"])
                                                , dictItemValues[@"NAME_SHR"]
                                                , dictItemValues[@"IP"]
                                                , Int32.Parse(dictItemValues[@"PORT"])
                                                , dictItemValues[@"DB_NAME"]
                                                , dictItemValues[@"UID"]
                                                , dictItemValues[@"PSWD*"]
                                                ));
                                            break;
                                        case INDEX_TYPE_GROUP.SIGNAL: //Сигнал
                                            //Инициализация, если элемент группы 1-ый
                                            if ((itemSrc as GROUP_SIGNALS_SRC).m_listSgnls == null)
                                                (itemSrc as GROUP_SIGNALS_SRC).m_listSgnls = new List<SIGNAL_SRC>();
                                            else
                                                ;

                                            (itemSrc as GROUP_SIGNALS_SRC).m_listSgnls.Add (new SIGNAL_SRC ());
                                            dictItemValues = (itemSrc as GROUP_SIGNALS_SRC).m_listSgnls[(itemSrc as GROUP_SIGNALS_SRC).m_listSgnls.Count - 1].m_dictPars;
                                            foreach (string valPar in values)
                                            {
                                                vals = valPar.Split(s_chSecDelimeters[(int)INDEX_DELIMETER.VALUE]);
                                                if ((vals.Length == 2)
                                                    //&& (! (itemSrc.m_keys.IndexOf () < 0))
                                                    )
                                                    dictItemValues.Add(vals[0], vals[1]);
                                                else
                                                    throw new Exception(@"FileINI::ctor () - addGroupValues () ...");
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                else
                                    //Значение по ключу не соответствует формату
                                    break;
                            }
                            else
                                //Ключ в секции не  найден
                                break;
                            //Увеличить индекс элемента (источник, сигнал)
                            j++;
                        }
                    }
                    else
                        //Секция есть, но в ней не определен ни один источник...
                        iRes = -1; //???
                }
                else
                    throw new Exception(@"FileINI::ctor () - addGroupValues () - группа объявлена, но значения не определены [" + secGroup + @"] ...");

                return iRes;
            }

            private string[] GetListGroupSources(INDEX_SRC indx)
            {
                string[] arStrRes = new string[] { };
                int i = -1;

                List<GROUP_SRC> listGroupSrc = m_arListGroupValues[(int)indx].m_listGroupSrc;
                arStrRes = new string[listGroupSrc.Count];

                i = 0;
                foreach (GROUP_SRC grpSrc in listGroupSrc)
                    arStrRes[i++] = grpSrc.m_strID;

                return arStrRes;
            }

            public GROUP_SRC[] AllObjectsSrcGroupSources { get { return m_arListGroupValues[(int)INDEX_SRC.SOURCE].m_listGroupSrc.ToArray(); } }
            public GROUP_SIGNALS_SRC[] AllObjectsSrcGroupSignals { get { return m_arListGroupValues[(int)INDEX_SRC.SOURCE].m_listGroupSgnlsSrc.ToArray(); } }

            public GROUP_SRC[] AllObjectsDestGroupSources { get { return m_arListGroupValues[(int)INDEX_SRC.DEST].m_listGroupSrc.ToArray(); } }
            public GROUP_SIGNALS_SRC[] AllObjectsDestGroupSignals { get { return m_arListGroupValues[(int)INDEX_SRC.DEST].m_listGroupSgnlsSrc.ToArray(); } }
            
            public string[] ListSrcGroupSources { get { return GetListGroupSources(INDEX_SRC.SOURCE); } }

            public string[] ListDestGroupSources { get { return GetListGroupSources(INDEX_SRC.DEST); } }

            private string[] GetListGroupSignals(INDEX_SRC indx)
            {
                string[] arStrRes = new string[] { };
                int i = -1;

                List<GROUP_SIGNALS_SRC> listGroupSrc = m_arListGroupValues[(int)indx].m_listGroupSgnlsSrc;
                arStrRes = new string[listGroupSrc.Count];

                i = 0;
                foreach (GROUP_SIGNALS_SRC grpSrc in listGroupSrc)
                    arStrRes[i++] = grpSrc.m_strID;

                return arStrRes;
            }

            public string[] ListSrcGroupSignals { get { return GetListGroupSignals(INDEX_SRC.SOURCE); } }

            public string[] ListDestGroupSignals { get { return GetListGroupSignals(INDEX_SRC.DEST); } }

            public string [] GetListItemsOfGroupSource (object []pars)
            {
                string[] arStrRes = new string[] { };
                int i = -1;
                ITEM_SRC itemSrc = getItemSrc(pars);

                arStrRes = new string[(itemSrc as GROUP_SRC).m_listConnSett.Count];

                i = 0;
                foreach (ConnectionSettings connSett in (itemSrc as GROUP_SRC).m_listConnSett)
                    arStrRes[i++] = connSett.name;

                return arStrRes;
            }

            public string [] GetListParsOfGroupSource (object []pars)
            {
                string[] arStrRes = new string[] { };
                int i = -1;
                ITEM_SRC itemSrc = getItemSrc(pars);

                arStrRes = new string[(itemSrc as ITEM_SRC).m_keys.Length];

                i = 0;
                foreach (string key in (itemSrc as ITEM_SRC).m_keys)
                    arStrRes[i++] = key;

                return arStrRes;
            }

            public string[] GetListItemPropOfGroupSource(object[] pars)
            {
                string[] arStrRes = new string[] { };
                int i = -1;
                //4-ый параметр не используется...
                ITEM_SRC itemSrc = getItemSrc(pars);

                arStrRes = new string[(itemSrc as GROUP_SRC).m_keys.Length];

                i = 0;
                ConnectionSettings connSett = (itemSrc as GROUP_SRC).m_listConnSett[(int)pars[3]];
                arStrRes[i++] = connSett.name;
                arStrRes[i++] = connSett.server;
                arStrRes[i++] = connSett.port.ToString ();
                arStrRes[i++] = connSett.dbName;
                arStrRes[i++] = connSett.userName;
                arStrRes[i++] = connSett.password;

                return arStrRes;
            }

            public string[] GetListItemsOfGroupSignal(object[] pars)
            {
                string[] arStrRes = new string[] { };
                int i = -1;
                ITEM_SRC itemSrc = getItemSrc (pars);                

                if (! ((itemSrc as GROUP_SIGNALS_SRC).m_listSgnls == null))
                {
                    arStrRes = new string[(itemSrc as GROUP_SIGNALS_SRC).m_listSgnls.Count];

                    i = 0;
                    foreach (SIGNAL_SRC sgnl in (itemSrc as GROUP_SIGNALS_SRC).m_listSgnls)
                        arStrRes[i++] = sgnl.m_dictPars[@"NAME_SHR"];
                }
                else
                    ;

                return arStrRes;
            }

            public string[] GetListParsOfGroupSignal(object[] pars)
            {
                string[] arStrRes = new string[] { };
                int i = -1;
                ITEM_SRC itemSrc = getItemSrc(pars);

                arStrRes = new string[(itemSrc as ITEM_SRC).m_keys.Length];

                i = 0;
                foreach (string key in (itemSrc as ITEM_SRC).m_keys)
                    arStrRes[i++] = key;

                return arStrRes;
            }

            public string[] GetListItemPropOfGroupSignal(object[] pars)
            {
                string[] arStrRes = new string[] { };
                int i = -1;
                ITEM_SRC itemSrc = getItemSrc(pars);

                arStrRes = new string[(itemSrc as GROUP_SIGNALS_SRC).m_keys.Length];

                i = 0;
                SIGNAL_SRC sgnl = (itemSrc as GROUP_SIGNALS_SRC).m_listSgnls[(int)pars[3]];
                foreach (string key in (itemSrc as GROUP_SIGNALS_SRC).m_keys)
                    arStrRes[i++] = sgnl.m_dictPars[key];

                return arStrRes;
            }

            public GROUP_SIGNALS_SRC GetObjectGroupSignals(object[] pars)
            {
                return getItemSrc(pars) as GROUP_SIGNALS_SRC;
            }

            private ITEM_SRC getItemSrc(object[] pars)
            {
                switch (pars.Length)
                {
                    case 2:
                        return getItemSrc((INDEX_SRC)pars[0], pars[1] as string);
                        //break;
                    case 3:
                    case 4: //'GetListItemPropOfGroupSource', 4-ый параметр не используется...
                        return getItemSrc((INDEX_SRC)pars[0], (int)pars[1], (int)pars[2]);
                        //break;
                    default:
                        throw new Exception(@"FileINI::getItemSrc () - неизвестное количество параметров ...");
                    //break;
                }
            }
            /// <summary>
            /// Получить "маску" строкового идетификатора
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static string GetIDMasked(string id)
            {
                string strRes = string.Empty;
                int lengthMaskId = id.Length;

                //Получить длину "маски" идентификатора
                while (char.IsNumber(id[lengthMaskId - 1]) == true)
                    lengthMaskId--;
                //Получить "маску" идентификатора
                strRes = id.Substring(0, lengthMaskId);

                return strRes;
            }
            /// <summary>
            /// Получить индекс строкового идентификатора
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static int GetIDIndex(string id)
            {
                int iRes = -1;
                int lengthMaskId = id.Length;

                //Получить длину "маски" идентификатора
                while (char.IsNumber(id[lengthMaskId - 1]) == true)
                    lengthMaskId--;
                //Получить индекс идентификатора
                iRes = Int32.Parse(id.Substring(lengthMaskId, id.Length - lengthMaskId));

                return iRes;
            }
            /// <summary>
            /// Получить объект 'ITEM_SRC' по индксу панели, типу группы, индексу текущей группы
            /// </summary>
            /// <param name="indxSrc">Индекс панели</param>
            /// <param name="groupType">Тип группы</param>
            /// <param name="indxSel">Индекс выбранной группы</param>
            /// <returns>Объект описания элемента группы</returns>
            private ITEM_SRC getItemSrc(INDEX_SRC indxSrc, int groupType, int indxSel)
            {
                ITEM_SRC itemSrcRes = null;

                SRC src = m_arListGroupValues[(int)indxSrc]; //источник/назначение
                //группы источников/сигналов
                switch (groupType)
                {
                    case 0: //GROUP_SOURCES
                        itemSrcRes = src.m_listGroupSrc[indxSel];
                        break;
                    case 2: //GROUP_SIGNALS
                        itemSrcRes = src.m_listGroupSgnlsSrc[indxSel];
                        break;
                    default:
                        break;
                }

                return itemSrcRes;
            }            
            /// <summary>
            /// Получить объект 'ITEM_SRC' по индксу панели, строковому идентификатору
            /// </summary>
            /// <param name="indxSrc">Индекс панели</param>
            /// <param name="id">Строковый идентификатор объекта</param>
            /// <returns>Объект описания элемента группы</returns>
            private ITEM_SRC getItemSrc(INDEX_SRC indxSrc, string id)
            {
                ITEM_SRC itemSrcRes = null; //Результат
                //Получить объект с 
                SRC src = m_arListGroupValues[(int)indxSrc]; //источник/назначение
                int groupType = -1;
                //Получить "маску" идентификатора
                string idType = GetIDMasked(id);
                //Определить тип группы по "маске" идентификатора
                //Сравнить с "маской" групп источников
                if (idType.Equals(KEY_TREE_SRC[(int)INDEX_KEY_SRC.GROUP_SRC]) == true)
                    groupType = 0;
                else
                    //Сравнить с "маской" групп сигналов
                    if (idType.Equals(KEY_TREE_SGNLS[(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS]) == true)
                        groupType = 2;
                    else
                        throw new Exception(@"FileINI::getItemSrc () - ...");

                //группы источников/сигналов
                switch (groupType)
                {
                    case 0: //GROUP_SOURCES
                        foreach (ITEM_SRC itemSrc in src.m_listGroupSrc)
                            if (itemSrc.m_strID.Equals (id) == true)
                            {//Получить результат
                                itemSrcRes = itemSrc;
                                //Прекратить выполнение цикла
                                break;
                            }
                            else
                                ;
                        break;
                    case 2: //GROUP_SIGNALS
                        foreach (ITEM_SRC itemSrc in src.m_listGroupSgnlsSrc)
                            if (itemSrc.m_strID.Equals (id) == true)
                            {//Получить результат
                                itemSrcRes = itemSrc;
                                //Прекратить выполнение цикла
                                break;
                            }
                            else
                                ;
                        break;
                    default:
                        break;
                }
                //Вернуть результат
                return itemSrcRes;
            }         
        }
    }
}
