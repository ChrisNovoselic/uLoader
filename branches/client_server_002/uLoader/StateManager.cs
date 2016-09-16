using System;
using System.Collections.Generic;

using System.Reflection; //Assembly
using System.IO;
using System.Data;

using System.Threading;
using System.ComponentModel;

using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
    //public class StateManager : HClassLibrary.HHandlerQueue
    partial class HHandlerQueue
    {
#if _STATE_MANAGER
        /// <summary>
        /// Класс для идентификации объекта контроля
        /// </summary>
        public class ID : Object
        {
            public INDEX_SRC m_typeOwner;
            /// <summary>
            /// Идентификатор владельца контролируемого объекта
            /// </summary>
            public int m_idOwner;
            /// <summary>
            /// НЕпосредственный идентификатор объекта контроля
            /// </summary>
            public int m_idGroupSgnls;
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="ids">Массив объектов-идентификаторов для объекта контроля</param>
            public ID(object[] ids)
            {
                if (ids[0].GetType().IsEnum == true)
                    m_typeOwner = (INDEX_SRC)ids[0];
                else
                    throw new InvalidEnumArgumentException(@"HHandlerQueue.ID::ctor () - ...");
                m_idOwner = (int)ids[1];
                m_idGroupSgnls = (int)ids[2];
            }

            public override bool Equals(object obj)
            {
                return (this.m_typeOwner == (obj as ID).m_typeOwner)
                    && (this.m_idOwner == (obj as ID).m_idOwner)
                    && (this.m_idGroupSgnls.Equals((obj as ID).m_idGroupSgnls) == true);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        /// <summary>
        /// Перечисление - состояния объектов контроля
        /// </summary>
        private enum STATE { UNKNOWN, ADDED, REMOVED, CONTROLED, CRASH, COUNT }
        /// <summary>
        /// Класс для хранения информации о контролируемом объекте 
        /// </summary>
        private class OManagement
        {
            public ID m_id;
            /// <summary>
            /// Текущее состояние объекта контроля
            /// </summary>
            public STATE m_state;
            /// <summary>
            /// Предельный интервал отсутствия обновлений состояния
            /// </summary>
            public TimeSpan m_tsLimit;
            /// <summary>
            /// Крайнее время обновления
            /// </summary>
            public DateTime m_dtUpdate;
            /// <summary>
            /// Указать, что объект подготовлен к удалению
            /// </summary>
            public void SetRemoved() { m_state = STATE.REMOVED; }
            /// <summary>
            /// Указать, что объект контролируется
            /// </summary>
            public void SetControled() { m_state = STATE.CONTROLED; }
            /// <summary>
            /// Указать, что метка времени объекта не была обновлена своевременно
            /// </summary>
            public void SetCrashed() { m_state = STATE.CRASH; }
            /// <summary>
            /// Обновить метку времени крайнего обновления
            /// </summary>
            public void Update() { m_dtUpdate = DateTime.Now; }
        }
        /// <summary>
        /// Интервал в милисекундах для проверки меток времени обновления
        /// </summary>
        public static int MSEC_TIMERFUNC_UPDATE = 1006;
        /// <summary>
        /// Интервал времени, в течении которого состояние объекта считать актуальным
        /// </summary>
        public static int MSEC_CONFIRM_WAIT = 6666;
        ///// <summary>
        ///// Перечисление - состояния для организации контроля списка объектов
        ///// </summary>
        //private enum StatesMachine : int { Unknown = -1, Add, Remove, Confirm, Update, Control
        //    , Count }
        /// <summary>
        /// Таймер, обеспечивающий регулярный вызов целевой функции
        /// </summary>
        private System.Threading.Timer m_timerFunc;
        /// <summary>
        /// Кдассс для хранения характеристик контролируемого объекта
        /// </summary>
        private class ListOManagement : List <OManagement>
        {
            /// <summary>
            /// Добавть объект для контоля
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            /// <param name="tsLimit">Интервал в течении которого объект считать актульным без обновления его состояния</param>
            /// <returns>Индекс объекта в списке при добавлении</returns>
            public int AddItem(ID id, TimeSpan tsLimit)
            {
                int indx = indexOfId(id);
                //Console.WriteLine(@"HHandlerQueue.ListOManagement::AddItem (IdGrpSgnls=" + id.m_idGroupSgnls
                //        + @", IdOwner=" + id.m_idOwner + @") - ДОБАВЛЕН!");

                if (indx < 0)
                    this.Add(new OManagement() { m_id = id, m_state = STATE.ADDED, m_tsLimit = tsLimit, m_dtUpdate = DateTime.Now });
                else
                    // предупреждение - такой объект уже контролируется
                    Logging.Logg().Warning(@"HHandlerQueue.ListOManagement::AddItem (IdGrpSgnls=" + id.m_idGroupSgnls
                        + @", IdOwner=" + id.m_idOwner + @") - добавляемый объект уже контролируется ...", Logging.INDEX_MESSAGE.NOT_SET);

                return this.Count - 1;
            }
            /// <summary>
            /// Установить признак подготовки к удалению
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            public void RemoveItem (ID id)
            {
                int indx = indexOfId(id);
                //Console.WriteLine(@"HHandlerQueue.ListOManagement::RemoveItem (IdGrpSgnls=" + id.m_idGroupSgnls
                //        + @", IdOwner=" + id.m_idOwner + @") - удалЁн!");

                if (!(indx < 0))
                    this[indx].SetRemoved();
                else
                    Logging.Logg().Error(@"HHandlerQueue.ListOManagement::RemoveItem (IdGrpSgnls=" + id.m_idGroupSgnls
                        + @", IdOwner=" + id.m_idOwner + @") - объект для удаления не найден ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Подтвердить изменения состояния (добавление/удаление)
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            public void Confirm(ID id)
            {
                int indx = indexOfId(id);
                string msgErr = string.Empty;

                if (!(indx < 0))
                    if (this[indx].m_state == STATE.ADDED)
                        this[indx].SetControled();
                    else
                        if (this[indx].m_state == STATE.REMOVED)
                            this.RemoveAt(indx);
                        else
                            // ошибка - объект не может получить подтверждение
                            msgErr = @"объект [сост.=" + this[indx].m_state + @"] не может получить подтверждение";
                else
                    // ошибка - объект для подтверждения состояния не найден
                    msgErr = @"объект для подтверждения состояния не найден";

                if (msgErr.Equals (string.Empty) == false)
                    Logging.Logg().Error(@"HHandlerQueue.ListOManagement::Confirm (IdGrpSgnls=" + id.m_idGroupSgnls
                            + @", IdOwner=" + id.m_idOwner + @") - " + msgErr + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;
            }
            /// <summary>
            /// Обновить состояние контролируемого объекта
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            public void Update(ID id)
            {
                int indx = indexOfId(id);

                if (!(indx < 0))
                    if (this[indx].m_state == STATE.CONTROLED)
                        this[indx].Update();
                    else
                        ; // ошибка - объект не может быть обновлен
                else
                    ; // ошибка - объект для обновления состояния не найден
            }
            /// <summary>
            /// Найти индекс в списке по (сложному) идентификатору
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            /// <returns>Индекс в спсике</returns>
            private int indexOfId(ID id)
            {
                int iRes = -1
                    , i = 0;

                for (i = 0; i < this.Count; i++)
                    if (this[i].m_id.Equals(id) == true)
                        break;
                    else
                        ;

                if (i < this.Count)
                    iRes = i;
                else
                    ;

                return iRes;
            }
        }
        /// <summary>
        /// Список контролируемых объектов
        /// </summary>    
        ListOManagement m_listObjects;

        private class DictInfoCrashed : Dictionary<KeyValuePair <INDEX_SRC, int>, DictOManagementInfoCrashed>
        {
            public void AddItem(ID id)
            {
                KeyValuePair<INDEX_SRC, int> key = GetKey(id);

                if (ContainsKey(key) == true)
                    ; // группа источников уже добавлена
                else
                    Add(key, new DictOManagementInfoCrashed());
                // добавить группу сигналов
                this[key].AddItem(id.m_idGroupSgnls);
            }

            public void RemoveItem(ID id)
            {
                KeyValuePair<INDEX_SRC, int> key = GetKey(id);

                if (ContainsKey(key) == true)
                {
                    this[key].Remove(id.m_idGroupSgnls);

                    if (this[key].Count == 0)
                        Remove(key);
                    else
                        ; // не удалять остались контролируемые группы сигналов
                }
                else
                    ;
            }

            public void SetCrashed(INDEX_SRC indx, int idOwner)
            {
                KeyValuePair<INDEX_SRC, int> key = GetKey(indx, idOwner);

                if (ContainsKey(key) == true)
                    ; // группа источников уже добавлена
                else
                    Add(key, new DictOManagementInfoCrashed());
                // установить признак для немедленной выгрузки/загрузки библиотеки
                this[key].m_bCrashed = true;
            }

            public static KeyValuePair<INDEX_SRC, int> GetKey(ID id)
            {
                return GetKey(id.m_typeOwner, id.m_idOwner);
            }

            public static KeyValuePair<INDEX_SRC, int> GetKey(INDEX_SRC indx, int id)
            {
                return new KeyValuePair<INDEX_SRC, int>(indx, id);
            }
        }
        /// <summary>
        /// Словарь для хранения статистической информации
        /// </summary>
        private class DictOManagementInfoCrashed : Dictionary<int, long>
        {
            public bool IsCrashed { get { return CountCrashed > 1; } }

            public void AddItem(int id)
            {
                if (ContainsKey(id) == true)
                // группа сигналов уже добавлена
                    // обновить дату/время события
                    this[id] = _lIdCurrentTargetFunc;
                else
                    Add(id, _lIdCurrentTargetFunc);
            }

            public int CountCrashed {
                get {
                    int iRes = 0;

                    foreach (long id in Values)
                        if (id == _lIdCurrentTargetFunc)
                            iRes++;
                        else
                            ;

                    return iRes;
                }
            }

            public int GetIdCrashed()
            {
                int iRes = -1;

                foreach (KeyValuePair<int, long> pair in this)
                    ;

                return iRes;
            }
        }

        DictInfoCrashed m_dictInfoCrashed;
        /// <summary>
        /// Функция обратного вызова для таймера
        /// </summary>
        /// <param name="obj">Аргумент при вызове функции</param>
        private void timerFunc(object obj)
        {
            // добавить для обработки одно событие
            Push(null, new object[] { // несколько событий
                new object [] { //очередное событие
                    new object [] { StatesMachine.OMANAGEMENT_CONTROL, } // параметры события (с 0-ым индексом - идентификатор события)
                },
            });
        }
        /// <summary>
        /// Добавить новый объект для контроля
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        /// <param name="tsLimit">Интервал времени в течении которого состояние объекта остается актуальным без обновления</param>
        private void add(ID id, TimeSpan tsLimit)
        {
            lock (this)
            {
                m_listObjects.AddItem(id, tsLimit);
            }

            //Console.WriteLine(@"StateManager::add (id=" + id.m_idOwner + @", key=" + id.m_idGroupSgnls + @") - добавить объект; кол-во = " + m_listObjects.Count + @" ...");
            Logging.Logg().Debug(@"StateManager::add (id=" + id.m_idOwner + @", key=" + id.m_idGroupSgnls + @") - добавить объект; кол-во -> " + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Поставить в очередь событие для добавления нового объекта для контроля
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        /// <param name="tsLimit">Интервал времени в течении которого состояние объекта остается актуальным без обновления</param>
        protected void add(object [] pars, TimeSpan tsLimit)
        {
            Push (null, new object [] {
                new object [] {
                    new object [] { StatesMachine.OMANAGEMENT_ADD, new ID (pars), tsLimit }
                },
            });
        }
        /// <summary>
        /// Удалить объект из списка контролируемых объектов
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        private void remove(ID id)
        {
            lock (this)
            {
                m_listObjects.RemoveItem (id);
            }

            //Console.WriteLine(@"StateManager::remove (id=" + id.m_idOwner + @", key=" + id.m_idGroupSgnls + @") - удалить объект; кол-во = " + m_listObjects.Count + @" ...");
            Logging.Logg().Debug(@"StateManager::remove (id=" + id.m_idOwner + @", key=" + id.m_idGroupSgnls + @") - удалить объект; кол-во <- " + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Поставить в очередь событие для удаления объекта из списка контролируемых объектов
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        protected void remove(params object[] pars)
        {
            Push(null, new object[] {
                new object [] {
                    new object [] { StatesMachine.OMANAGEMENT_REMOVE, new ID (pars) }
                },
            });
        }
        /// <summary>
        /// Подтвердить изменение состояния контролируемого объекта
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        private void confirm(ID id)
        {
            lock (this)
            {
                m_listObjects.Confirm(id);
            }

            //Console.WriteLine(@"StateManager::confirm (id=" + id.m_idOwner + @", key=" + id.m_idGroupSgnls + @") - подтвердить состояние объекта ...");
            Logging.Logg().Debug(@"StateManager::confirm (id=" + id.m_idOwner + @", key=" + id.m_idGroupSgnls + @") - подтвердить состояние объекта; кол-во = " + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Поставить в очередь событие для подтверждения изменения состояния контролируемого объекта
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        protected void confirm(params object []pars)
        {
            Push(null, new object[] {
                new object [] {
                    new object [] { StatesMachine.OMANAGEMENT_CONFIRM, new ID (pars) }
                },
            });
        }
        /// <summary>
        /// Обновить состояние контролируемого объекта
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        private void update(ID id)
        {
            lock (this)
            {
                m_listObjects.Update(id);
            }
        }
        /// <summary>
        /// Обновить состояние контролируемого объекта
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        protected void update(params object[] pars)
        {
            Push(null, new object[] {
                new object [] {
                    new object [] { StatesMachine.OMANAGEMENT_UPDATE, new ID (pars) }
                },
            });
        }        

        private class EventCrashedArgs : EventArgs
        {
            public ID m_id;

            public STATE m_state;
        }

        private delegate void EventHandlerCrashed (EventCrashedArgs arg);

        private event /*EventHandlerCrashed*/ DelegateObjectFunc eventCrashed;
        /// <summary>
        /// Обработчик события - состояние группы сигналов не актуально
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        private void onEvtCrashed(/*HHandlerQueue.EventCrashedArgs*/ object obj)
        {
            HHandlerQueue.EventCrashedArgs ev = obj as HHandlerQueue.EventCrashedArgs;

            //for (INDEX_SRC type = INDEX_SRC.SOURCE; type < INDEX_SRC.COUNT_INDEX_SRC; type++)
            //    foreach (GroupSources grpSrc in m_listGroupSources[(int)type])
            //        if ((type == ev.m_id.m_typeOwner)
            //            && (FormMain.FileINI.GetIDIndex (grpSrc.m_strID) == ev.m_id.m_idOwner))
            //        {
                        switch (ev.m_state)
                        {
                            case STATE.CONTROLED:// группа сигналов в работе
                                m_dictInfoCrashed.AddItem(ev.m_id);                                
                                break;
                            case STATE.ADDED:
                            case STATE.REMOVED:
#if _SEPARATE_APPDOMAIN
                                // группа сигналов не получила подтверждения от библиотеки при изменении своего состояния
                                m_dictInfoCrashed.SetCrashed(ev.m_id.m_typeOwner, ev.m_id.m_idOwner);
#endif
                                break;
                            default:
                                break;
                        }

                    //    type = INDEX_SRC.COUNT_INDEX_SRC; // для прерывания внешнего цикла

                    //    break; // прервать внутренний цикл
                    //}
                    //else
                    //    ;
        }

        private void pushStateChangedGroupSignals(ID id)
        {
            object[] arToSend = null; // массив для аргументов состояния
            GroupSources grpSrc = null;

            grpSrc = m_listGroupSources[(int)id.m_typeOwner].Find(g => { return FormMain.FileINI.GetIDIndex (g.m_strID) == id.m_idOwner; });

            arToSend = new object[] {
                (int)StatesMachine.STATE_CHANGED_GROUP_SIGNALS
                , id.m_typeOwner
                , grpSrc.m_strID
                , grpSrc.GetIdGroupSignals (id.m_idGroupSgnls)
            };
            // поставить в очередь 2 состояния: последовательный останов/запуск группы сигналов
            Push(null, new object[] {
                new object[] {
                    arToSend // для 'STOP'
                    , arToSend // для 'START'
                    ,
                },
            });
        }
        /// <summary>
        /// Поставить в очередь 1(одно) состояние - полная выгрузка/загрузки библиотеки
        /// </summary>
        /// <param name="type">Тип группы источников</param>
        /// <param name="idOwner">Идентификатор (индекс) группы источников</param>
        private void pushCommandReloadGroupSources(INDEX_SRC type, int idOwner)
        {
            GroupSources grpSrc = null;

            grpSrc = m_listGroupSources[(int)type].Find(g => { return FormMain.FileINI.GetIDIndex(g.m_strID) == idOwner; });

            Push(null, new object[] {
                new object[] {
                    new object [] {
                        StatesMachine.COMMAND_RELAOD_GROUP_SOURCES
                        , type
                        , grpSrc.m_strID
                    }
                    ,
                },
            });
        }

        private static long _lIdCurrentTargetFunc;
        /// <summary>
        /// Целевая функция контроля
        /// </summary>
        private void targetFunc()
        {
            //Logging.Logg().Debug(@"StateManager::targetFunc () - итерация контроля; кол-во объектов=" + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

            DateTime now = DateTime.Now;
            int msecLimit = -1;

            _lIdCurrentTargetFunc = HMath.GetRandomNumber();

            foreach (OManagement o in m_listObjects)
            {
                msecLimit = 0; // признак необходимости проверки объекта в текущей итерации

                switch (o.m_state)
                {
                    case STATE.ADDED:
                    case STATE.REMOVED:
                        msecLimit = MSEC_CONFIRM_WAIT;
                        break;
                    case STATE.CONTROLED:
                        msecLimit = (int)o.m_tsLimit.TotalMilliseconds;
                        break;
                    default:
                        break;
                }

                if ((msecLimit > 0)
                    && (now - o.m_dtUpdate).TotalMilliseconds > msecLimit)
                {
                    //Пополнить словарь событиями-нарушениями
                    //new Thread (new ParameterizedThreadStart (onEvtCrashed)).Start(new EventCrashedArgs() { m_id = o.m_id, m_state = o.m_state });
                    eventCrashed(new EventCrashedArgs() { m_id = o.m_id, m_state = o.m_state });
                    //onEvtCrashed(new EventCrashedArgs() { m_id = o.m_id, m_state = o.m_state });
                    //Console.WriteLine(@"HHandlerQueue::targetFunc () - eventCrashed (id=" + o.m_id.m_idOwner
                    //    + @", key=" + o.m_id.m_idGroupSgnls
                    //    + @", state=" + o.m_state.ToString () + @") - ...");
                    // исключить из следующей проверки
                    o.SetCrashed();
                    //o.Update();
                }
                else
                    ;
            }

            foreach (KeyValuePair<KeyValuePair<INDEX_SRC, int>, DictOManagementInfoCrashed> pair in m_dictInfoCrashed)
                if (pair.Value.IsCrashed == true)
                    pushCommandReloadGroupSources(pair.Key.Key, pair.Key.Value);
                else
                    pushStateChangedGroupSignals (new ID (new object[] { pair.Key.Key, pair.Key.Value, pair.Value.GetIdCrashed () }));
        }
#endif

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);
#if _STATE_MANAGER
            int due = System.Threading.Timeout.Infinite
                , period = System.Threading.Timeout.Infinite;

            if (bRes == true)
                if (active == true)
                {
                    due = 0;
                    period = MSEC_TIMERFUNC_UPDATE;
                }
                else
                    if (active == false)
                        ; // оставить значения по умолчанию
                    else
                        ; // других состояний 'bool' не существует

            Logging.Logg().Debug(@"StateManager::Activate (active=" + active + @") - "
                + (due == System.Threading.Timeout.Infinite ? @"ДЕ" : string.Empty) + @"Активация объекта контроля ..."
                , Logging.INDEX_MESSAGE.NOT_SET);
            m_timerFunc.Change(due, period);
#endif
            return bRes;
        }        
    }
}