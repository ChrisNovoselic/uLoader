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
        private static TimeSpan TS_INTERVAL_OUTDATED = TimeSpan.FromSeconds(20);

        private static int COUNT_VIEW_PACKAGE_ITEM = 6;

        private static TimeSpan TS_HISTORY_RUNTIME = TimeSpan.FromSeconds(60);
        private static TimeSpan TS_HISTORY_ALONG = TimeSpan.FromSeconds(0);

        /// <summary>
        /// Перечисление - возможные состояния для обработки
        /// </summary>
        public enum StatesMachine
        {
            UNKNOWN = -1
            , NEW // получен новый пакет
            , LIST_PACKAGE // запрос для получения списка пакетов
            , PACKAGE_CONTENT // запрос для получения пакета
            , TIMER_OUTDATED // событие устаревания XML-пакета
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

        private struct RECORD
        {
            public string m_NameGroup;

            public string m_NameParameter;

            public float m_value;
        }

        public struct PACKAGE
        {
            public enum STATE : short { UNKNOWN = -1
                , NEW, PARSING, PARSED, ERROR
            };

            public DateTime m_dtRecieved;

            public DateTime m_dtSend;

            public STATE m_state;

            public XmlDocument m_xmlSource;

            public DataTable m_tableParameters;

            public DataTable m_tableValues;

            private List<object> _listXmlTree;

            private int parseNode(XmlNode node, List<object>listTree, int []indxRank)
            {
                int iRes = -1;

                int[] newIndxRank;
                float value = -1F;

                if (node.HasChildNodes == true)
                    if (node.ChildNodes.Count > 0)
                        for (int i = 0; i < node.ChildNodes.Count; i++) {
                            listTree.Add (new List<object>());

                            newIndxRank = new int[indxRank.Length + 1];
                            indxRank.CopyTo(newIndxRank, 0);
                            newIndxRank[indxRank.Length] = i;

                            parseNode(node.ChildNodes[i], listTree[i] as List<object>, newIndxRank);
                        }
                    else
                        return 1;
                else {
                    try {
                        if (!(node.Attributes == null)) {
                            if (string.IsNullOrEmpty(node.Attributes.GetNamedItem("value").Value) == false)
                                if (float.TryParse(node.Attributes.GetNamedItem("value").Value, out value) == false)
                                    value = -1F;
                                else
                                    ; //значние получено
                            else
                                value = -1F;

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

            public PACKAGE(DateTime dtScore, XmlDocument xmlSource)
            {
                m_dtRecieved = dtScore;
                m_dtSend = DateTime.MinValue;

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

                _listXmlTree = new List<object>();

                m_state = STATE.PARSING;

                for (int i = 0; i < m_xmlSource.ChildNodes.Count; i ++) {
                    if (m_xmlSource.ChildNodes[i].NodeType == XmlNodeType.Element) {
                        _listXmlTree.Add(new List<object>());

                        parseNode(m_xmlSource.ChildNodes[i], _listXmlTree[_listXmlTree.Count - 1] as List<object>, new int[] { 0, i });
                    } else
                        ;
                }

                m_state = STATE.PARSED; //ERROR
            }
        }

        private List<PACKAGE> _listPackage;

        private System.Threading.Timer m_timerOutdated;

        public event DelegateObjectFunc EvtToFormMain;

        public PackageHandlerQueue(short secIntervalOutdated = -1)
        {
            if (secIntervalOutdated > 0)
                TS_INTERVAL_OUTDATED = TimeSpan.FromSeconds(secIntervalOutdated);
            else
                ;

            _listPackage = new List<PACKAGE>();
            m_timerOutdated = new System.Threading.Timer(timerOutdated_onCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true) {
                m_timerOutdated.Change(active == true ? (int)TS_INTERVAL_OUTDATED.TotalMilliseconds : System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            } else
                ;

            return bRes;
        }

        private void timerOutdated_onCallback(object obj)
        {
            Push(null, new object[] {
                new object[] {
                    new object[] {
                        StatesMachine.TIMER_OUTDATED
                    }
                }
            });

            m_timerOutdated.Change((int)TS_INTERVAL_OUTDATED.TotalMilliseconds, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// Список паектов для отправки на главную форму для отображения
        /// </summary>
        private List<FormMain.VIEW_PACKAGE_ITEM> listViewPackageItem {
            get {
                List<FormMain.VIEW_PACKAGE_ITEM> listRes = new List<FormMain.VIEW_PACKAGE_ITEM>();

                (from package in _listPackage
                 orderby package.m_dtRecieved descending
                 select new FormMain.VIEW_PACKAGE_ITEM {
                    Values = new object[] {
                        package.m_tableValues.Rows.Count
                        , package.m_dtRecieved
                        , package.m_dtSend
                    }
                }).Take(COUNT_VIEW_PACKAGE_ITEM).ToList().ForEach(item => listRes.Add(item));

                return listRes;
            }
        }

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
                Logging.Logg().Exception(e, string.Format(@"Добавление пакета дата/время получения={} и статистики для него", dtPackage), Logging.INDEX_MESSAGE.NOT_SET);
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

                        var packages = from package in _listPackage where package.m_dtRecieved == (DateTime)itemQueue.Pars[0] select package;
                        if (packages.Count() == 1)
                            outobj = packages.ElementAt(0).m_xmlSource;
                        else
                            ; //??? - ошибка пакет не найден либо пакетов много
                        break;
                    case StatesMachine.STATISTIC: // статистика
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        //outobj = ??? объект статический
                        break;
                    case StatesMachine.TIMER_OUTDATED: // срок отправлять очередной пакет
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"HHandlerQueue::StateCheckResponse (state=" + state.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

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

            Logging.Logg().Error(@"HHandlerQueue::StateErrors () - не обработана ошибка [" + ((StatesMachine)state).ToString() + @", REQ=" + req + @", RES=" + res + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);

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
                    if ((!(itemQueue == null))
                        //&& (!(itemQueue.m_dataHostRecieved == null)) FormMain не реализует интерфейс 'IDataHost'
                        )
                        // вариант для объекта с интерфейсом 'IDataHost'
                        //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj })
                        EvtToFormMain(new object [] { state, obj })
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
            Logging.Logg().Warning(@"HHandlerQueue::StateWarnings () - не обработано предупреждение [" + ((StatesMachine)state).ToString() + @", REQ=" + req + @", RES=" + res + @"] ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
