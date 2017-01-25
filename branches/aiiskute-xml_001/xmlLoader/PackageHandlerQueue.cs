using System;
using HClassLibrary;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Linq;
using System.Reflection;

namespace xmlLoader
{
    public class PackageHandlerQueue : HClassLibrary.HHandlerQueue
    {
        private static TimeSpan TS_TIMER_TABLERES = TimeSpan.FromSeconds(20);

        private static int COUNT_VIEW_PACKAGE_ITEM = 6;

        private static TimeSpan TS_HISTORY_RUNTIME = TimeSpan.FromSeconds(60);
        private static TimeSpan TS_HISTORY_ALONG = TimeSpan.FromSeconds(0);

        /// <summary>
        /// Перечисление - возможные состояния для обработки
        /// </summary>
        public enum StatesMachine
        {
            UNKNOWN = -1
            , NEW = 100// получен новый пакет
            , LIST_PACKAGE // запрос для получения списка пакетов
            , PACKAGE_CONTENT // запрос для получения пакета
            , TIMER_TABLERES = 106 // событие устаревания XML-пакета ??? совпадает с 'WriteHandlerQueue.StatesMachine::STATISTIC'
            , STATISTIC
        }

        public struct STATISTIC
        {
            public enum INDEX_ITEM {
                DATETIME_PACKAGE_LAST_RECIEVED
                , LENGTH_PACKAGE_LAST_RECIEVED
                , COUNT_PACKAGE_RECIEVED
                , COUNT_PACKAGE_PARSED
            }

            public struct ITEM
            {
                public object value;

                public bool visibled;

                public string desc;
            }

            private ITEM[] _values;

            public STATISTIC(ITEM[]values)
            {
                _values = new ITEM[values.Length];

                values.CopyTo(_values, 0); 
            }

            public ITEM ElementAt(INDEX_ITEM indx)
            {
                return _values[(int)indx];
            }

            public void SetAt(INDEX_ITEM indx, object value)
            {
                _values[(int)indx].value = value;
            }

            public void Counter(INDEX_ITEM indx)
            {
                if (_values[(int)indx].value == null)
                    _values[(int)indx].value = 0;
                else
                    _values[(int)indx].value = (int)(_values[(int)indx].value) + 1;
            }

            public bool IsEmpty {
                get {
                    return (_values == null)
                        || ((!(_values == null)) && (_values.Length == 0));
                }
            }
        }

        public static STATISTIC s_Statistic = new STATISTIC (new STATISTIC.ITEM[] {
            new STATISTIC.ITEM { desc = @"Вр.кр.пакета", visibled = true, value = DateTime.MinValue } // DATETIME_PACKAGE_LAST_RECIEVED
            , new STATISTIC.ITEM { desc = @"Разм.кр.пакета", visibled = true, value = 0 } // LENGTH_PACKAGE_LAST_RECIEVED
            , new STATISTIC.ITEM { desc = @"Пак.получено", visibled = true, value = 0 } // COUNT_PACKAGE_RECIEVED
            , new STATISTIC.ITEM { desc = @"Пак.разобрано", visibled = true, value = 0 } // COUNT_PACKAGE_PARSED
        });

        //private struct RECORD
        //{
        //    public string m_NameGroup;

        //    public string m_NameParameter;

        //    public float m_value;
        //}
        /// <summary>
        /// Структура для представления XML-пакета
        /// </summary>
        public class PACKAGE
        {
            /// <summary>
            /// Перечисление - возможные состояния XML-пакета
            /// </summary>
            public enum STATE : short { UNKNOWN = -1
                , NEW, PARSING, PARSED, SENDED, ERROR
            };
            /// <summary>
            /// Метка даты/времени получения
            /// </summary>
            public DateTime m_dtRecieved;
            /// <summary>
            /// Метка даты/времени отправления для обработки/записи
            /// </summary>
            public DateTime m_dtSended;
            /// <summary>
            /// Текущее состояние XML-пакета
            /// </summary>
            public STATE m_state;
            /// <summary>
            /// Содержание полученного XML-документа
            /// </summary>
            public XmlDocument m_xmlSource;
            /// <summary>
            /// Параметры после распознования XML-документа
            /// </summary>
            public DataTable m_tableParameters;
            /// <summary>
            /// Значения параметров после распознования XML-документа
            /// </summary>
            public DataTable m_tableValues;
            /// <summary>
            /// Список элементов XML-документа
            /// </summary>
            public FormMain.ListXmlTree m_listXmlTree;
            /// <summary>
            /// Распознать элемент XML-документа
            /// </summary>
            /// <param name="node">Элемент XML-документа</param>
            /// <param name="listXmlTree">Элемент списка</param>
            /// <returns>Результат обработки элемента XML-документа</returns>
            private int parseNode(XmlNode node, FormMain.ListXmlTree listXmlTree)
            {
                int iRes = -1;

                FormMain.ListXmlTree newItemXmlTree;
                float value = -1F;

                if (node.HasChildNodes == true) {
                    if (node.ChildNodes.Count > 0)
                        for (int i = 0; i < node.ChildNodes.Count; i++) {
                            if (!(node.ChildNodes[i].Attributes == null)) {
                                newItemXmlTree = new FormMain.ListXmlTree();
                                listXmlTree.Add(newItemXmlTree);

                                newItemXmlTree.Tag = node.ChildNodes[i].Name;

                                parseNode(node.ChildNodes[i], newItemXmlTree);
                            } else
                                //Logging.Logg().Warning(MethodBase.GetCurrentMethod()
                                //    , string.Format(@"XML-элемент Name={0} не имеет аттрибутов...", node.Name)
                                //    , Logging.INDEX_MESSAGE.NOT_SET)
                                    ;
                        }
                    else
                        return 1;
                } else {
                    try {
                        if (!(node.Attributes == null)) {
                            if (string.IsNullOrEmpty(node.Attributes.GetNamedItem("value").Value) == false)
                                if (float.TryParse(node.Attributes.GetNamedItem("value").Value, out value) == false)
                                    value = -1F;
                                else
                                    ; //значние получено
                            else
                                value = -1F;

                            listXmlTree.Attributes = new List<KeyValuePair<string, string>>();
                            if (!(node.Attributes.GetNamedItem("value") == null))
                                listXmlTree.Attributes.Add(new KeyValuePair<string, string>(node.Attributes.GetNamedItem("value").Name
                                    , node.Attributes.GetNamedItem("value").Value));
                            else
                                Logging.Logg().Warning(MethodBase.GetCurrentMethod()
                                    , string.Format(@"XML-элемент Name={0} не имеет аттрибута 'value'...", node.Name)
                                    , Logging.INDEX_MESSAGE.NOT_SET);

                            m_tableValues.Rows.Add(new object[] {
                                node.ParentNode.Name
                                , node.Name
                                , value
                            });

                            iRes = 0;
                        } else
                            ; //???
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, @"PackageHandlerQueue.PACKAGE::parseNode () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }

                return iRes;
            }
            /// <summary>
            /// Конструктор основной с параметрами
            /// </summary>
            /// <param name="dtRecieved">Мктка даты/времени получения</param>
            /// <param name="xmlSource">XML-документ</param>
            public PACKAGE(DateTime dtRecieved, XmlDocument xmlSource)
            {
                m_dtRecieved = dtRecieved;
                m_dtSended = DateTime.MinValue;

                m_state = STATE.NEW;

                m_xmlSource = UDPListener.CopyXmlDocument(xmlSource);

                m_tableParameters = new DataTable();
                m_tableParameters.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"GROUP", typeof(string))
                    , new DataColumn(@"PARAMETER", typeof(string))
                });
                m_tableValues = new DataTable();
                m_tableValues.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"GROUP", typeof(string))
                    , new DataColumn(@"PARAMETER", typeof(string))
                    , new DataColumn(@"VALUE", typeof(float))
                });

                m_listXmlTree = new FormMain.ListXmlTree();

                m_state = STATE.PARSING;

                for (int i = 0; i < m_xmlSource.ChildNodes.Count; i ++) {
                    if (m_xmlSource.ChildNodes[i].NodeType == XmlNodeType.Element) {
                        m_listXmlTree.Add(new FormMain.ListXmlTree());

                        m_listXmlTree.Tag = m_xmlSource.ChildNodes[i].Name;

                        parseNode(m_xmlSource.ChildNodes[i], m_listXmlTree[m_listXmlTree.Count - 1] as FormMain.ListXmlTree);
                    } else
                        ;
                }

                m_state = STATE.PARSED; //ERROR
            }
        }
        /// <summary>
        /// Список принятых XML-пакетов
        /// </summary>
        private List<PACKAGE> _listPackage;
        /// <summary>
        /// Таймер, определяющий срок отправления XML-пакета для обработки
        /// </summary>
        private System.Threading.Timer m_timerTableRes;
        /// <summary>
        /// Событие для отправки сообщения главной экранной форме
        ///  (в последующем для постановки(ретрансляции) в главную очередь обработки событий)
        /// </summary>
        public event DelegateObjectFunc EvtToFormMain;
        /// <summary>
        /// Конструктор основной (с парметрами)
        /// </summary>
        /// <param name="secIntervalOutdated">Интервал(сек) между отправлением XML-пакета для обработки</param>
        public PackageHandlerQueue(short secIntervalOutdated = -1)
        {
            if (secIntervalOutdated > 0)
                TS_TIMER_TABLERES = TimeSpan.FromSeconds(secIntervalOutdated);
            else
                ;

            _listPackage = new List<PACKAGE>();
            // создать объект таймера, не запускать
            m_timerTableRes = new System.Threading.Timer(timerTableRes_onCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// (Де)активировать таймер, определяющий срок отправления XML-пакета для обработки
        /// </summary>
        /// <param name="active">Признак (де)активации</param>
        /// <returns>Результат выполнения метода</returns>
        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true) {
                m_timerTableRes.Change(active == true ? (int)TS_TIMER_TABLERES.TotalMilliseconds : System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            } else
                ;

            return bRes;
        }
        /// <summary>
        /// Метод обратного вызова таймера, определяющего срок отправления XML-пакета для обработки
        /// </summary>
        /// <param name="obj">Аргумент при вызове метода</param>
        private void timerTableRes_onCallback(object obj)
        {
            Push(null, new object[] {
                new object[] {
                    new object[] {
                        StatesMachine.TIMER_TABLERES
                    }
                }
            });

            m_timerTableRes.Change((int)TS_TIMER_TABLERES.TotalMilliseconds, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// Список паектов для отправки на главную форму для отображения
        /// </summary>
        private List<FormMain.VIEW_ITEM> listViewPackageItem
        {
            get {
                List<FormMain.VIEW_ITEM> listRes = new List<FormMain.VIEW_ITEM>();

                (from package in _listPackage
                 orderby package.m_dtRecieved descending
                 select new FormMain.VIEW_ITEM {
                    Values = new object[] {
                        package.m_tableValues.Rows.Count
                        , package.m_dtRecieved
                        , package.m_dtSended
                    }
                }).Take(COUNT_VIEW_PACKAGE_ITEM).ToList().ForEach(item => listRes.Add(item));

                return listRes;
            }
        }
        /// <summary>
        /// Добавить XML-пакет в список
        /// </summary>
        /// <param name="dtPackage">Метка даты/времени получения XML-пакета</param>
        /// <param name="xmlDoc">XML-документ</param>
        private void addPackage(DateTime dtPackage, XmlDocument xmlDoc)
        {
            PACKAGE package;
            // определить лимит даты/времени хранения пакетов времени выполнения
            DateTime dtLimit = dtPackage - TS_HISTORY_RUNTIME;
            //список индексов элементов(пакетов) для удаления
            List<int> listIndxToRemove = new List<int>();
            for (int i = 0; i < _listPackage.Count; i++)
                if ((dtLimit - _listPackage[i].m_dtRecieved).TotalSeconds > 0)
                    listIndxToRemove.Add(i);
                else
                    ;
            // удалить пакеты дата/время получения которых больше, чем "лимит"
            listIndxToRemove.ForEach(indx => {
                Logging.Logg().Debug(MethodBase.GetCurrentMethod(), string.Format(@"удален пакет {0}", _listPackage[indx].m_dtRecieved), Logging.INDEX_MESSAGE.NOT_SET);

                _listPackage.RemoveAt(indx);
            });
            // добавить текущий пакет (даже, если он не удовлетворяет критерию "лимит")
            try {
                _listPackage.Add(package = new PACKAGE(dtPackage, xmlDoc));

                s_Statistic.SetAt(STATISTIC.INDEX_ITEM.DATETIME_PACKAGE_LAST_RECIEVED, package.m_dtRecieved);
                s_Statistic.SetAt(STATISTIC.INDEX_ITEM.LENGTH_PACKAGE_LAST_RECIEVED, package.m_tableValues.Rows.Count);
                s_Statistic.Counter(STATISTIC.INDEX_ITEM.COUNT_PACKAGE_RECIEVED);
                if (package.m_state == PACKAGE.STATE.PARSED)
                    s_Statistic.Counter(STATISTIC.INDEX_ITEM.COUNT_PACKAGE_PARSED);
                else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"Добавление пакета дата/время получения={0} и статистики для него", dtPackage), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// Подготовить объект для отправки адресату по его запросу
        /// </summary>
        /// <param name="s">Событие - идентификатор запрашиваемой информации/операции,действия</param>
        /// <param name="error">Признак выполнения операции/действия по запросу</param>
        /// <param name="outobj">Объект для отправления адресату как результат запроса</param>
        /// <returns>Признак выполнения метода (дополнительный)</returns>
        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;
            StatesMachine state = (StatesMachine)s;
            PACKAGE package;
            string debugMsg = string.Empty;

            error = true;
            outobj = null;

            ItemQueue itemQueue = null;

            try {
                switch (state) {
                    case StatesMachine.NEW: // новый пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        addPackage((DateTime)itemQueue.Pars[0], (XmlDocument)itemQueue.Pars[1]);
                        break;
                    case StatesMachine.LIST_PACKAGE: // список пакетов
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = listViewPackageItem;
                        break;
                    case StatesMachine.PACKAGE_CONTENT: // пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        var selectPackages = from p in _listPackage where p.m_dtRecieved == (DateTime)itemQueue.Pars[0] select p;
                        if (selectPackages.Count() == 1) {
                            package = selectPackages.ElementAt(0);

                            switch ((FormMain.INDEX_TABPAGE_VIEW_PACKAGE)itemQueue.Pars[1]) {
                                case FormMain.INDEX_TABPAGE_VIEW_PACKAGE.XML:
                                    outobj = package.m_xmlSource;
                                    break;
                                case FormMain.INDEX_TABPAGE_VIEW_PACKAGE.TREE:
                                    outobj = package.m_listXmlTree;
                                    break;
                                default: //??? - ошибка неизвестный тип вкладки просмотра XML-документа
                                    break;
                            }
                        } else
                            ; //??? - ошибка пакет не найден либо пакетов много
                        break;
                    case StatesMachine.STATISTIC: // статистика
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        //outobj = ??? объект статический
                        break;
                    case StatesMachine.TIMER_TABLERES: // срок отправлять очередной пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        var orderPckages = from p in _listPackage orderby p.m_dtRecieved descending select p;
                        if (orderPckages.Count() > 0) {
                            package = orderPckages.ElementAt(0);

                            package.m_dtSended = DateTime.UtcNow;

                            outobj = new object[] {
                                package.m_dtSended
                                , package.m_tableValues.Copy()
                                , package.m_tableParameters.Copy()
                            };
                        } else {
                            //??? - ошибка пакет не найден либо пакетов много
                        //    iRes = -1;
                        //    error = true;
                            outobj = false;
                        }
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PackageHandlerQueue::StateCheckResponse (state=" + state.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

                error = true;
                iRes = -1 * (int)state;
            }

            return iRes;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            switch ((StatesMachine)state) {
                default:
                    break;
            }

            Logging.Logg().Error(string.Format(@"PackageHandlerQueue::StateErrors () - не обработана ошибка [{0}, REQ={1}, RES={2}] ..."
                    , ((StatesMachine)state).ToString(), req, res)
                , Logging.INDEX_MESSAGE.NOT_SET);

            return HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch ((StatesMachine)state) {
                case StatesMachine.NEW: // 
                case StatesMachine.LIST_PACKAGE: //
                case StatesMachine.PACKAGE_CONTENT: //
                case StatesMachine.STATISTIC: //
                case StatesMachine.TIMER_TABLERES: //
                    // не требуют запроса
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            switch ((StatesMachine)state) {
                case StatesMachine.NEW: // 
                    //Ответа не требуется/не требуют обработки результата
                    break;
                case StatesMachine.LIST_PACKAGE: // 
                case StatesMachine.PACKAGE_CONTENT: // 
                case StatesMachine.STATISTIC: // статический объект
                case StatesMachine.TIMER_TABLERES:
                    if ((!(itemQueue == null))
                        //&& (!(itemQueue.m_dataHostRecieved == null)) FormMain не реализует интерфейс 'IDataHost'
                        )
                        // вариант для объекта с интерфейсом 'IDataHost'
                        //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj })
                        EvtToFormMain?.Invoke(new object [] { state, obj })
                        ;
                    else
                        ;
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(string.Format(@"PackageHandlerQueue::StateWarnings () - не обработано предупреждение [{0}, REQ={1}, RES={2}] ..."
                    , ((StatesMachine)state).ToString(), req, res)
                , Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
