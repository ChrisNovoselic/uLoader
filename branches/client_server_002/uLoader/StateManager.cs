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
                //if (ids[0].GetType().IsEnum == true)
                    m_typeOwner = (INDEX_SRC)ids[0];
                //else
                //    throw new InvalidEnumArgumentException(@"HHandlerQueue.ID::ctor () - ...");
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
        private enum STATE { UNKNOWN, ADDED, REMOVED, CONTROLED, CRASH, CCRASH
            , COUNT }
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

            public void SetCCrashed() { m_state = STATE.CCRASH; }
            /// <summary>
            /// Обновить метку времени крайнего обновления
            /// </summary>
            public void Update() { m_dtUpdate = DateTime.Now; }
        }
        /// <summary>
        /// Интервал в милисекундах для проверки меток времени обновления
        /// </summary>
        public static int MSEC_TIMERFUNC_UPDATE = 6006;
        /// <summary>
        /// Интервал времени, в течении которого состояние объекта считать актуальным
        /// </summary>
        public static int MSEC_CONFIRM_WAIT = 2 * MSEC_TIMERFUNC_UPDATE;
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
            public void MarkedItem(HHandlerQueue.EventCrashedArgs e) { MarkedItem(e.m_id, e.m_state); }

            public void MarkedItem(ID id, STATE state)
            {
                KeyValuePair<INDEX_SRC, int> key = GetKey(id);

                if (ContainsKey(key) == true)
                    ; // группа источников уже добавлена
                else
                    Add(key, new DictOManagementInfoCrashed());
                // добавить группу сигналов
                this[key].MarkedItem(id.m_idGroupSgnls, state);
            }

            //public void RemoveItem(ID id)
            //{
            //    KeyValuePair<INDEX_SRC, int> key = GetKey(id);

            //    if (ContainsKey(key) == true)
            //    {
            //        this[key].Remove(id.m_idGroupSgnls);

            //        if (this[key].Count == 0)
            //            Remove(key);
            //        else
            //            ; // не удалять остались контролируемые группы сигналов
            //    }
            //    else
            //        ;
            //}

            public static KeyValuePair<INDEX_SRC, int> GetKey(ID id)
            {
                return GetKey(id.m_typeOwner, id.m_idOwner);
            }

            public static KeyValuePair<INDEX_SRC, int> GetKey(INDEX_SRC indx, int id)
            {
                return new KeyValuePair<INDEX_SRC, int>(indx, id);
            }
        }

        private class InfoCrashed : Object
        {
            public long m_IdTargetFunc;

            public STATE m_state;

            public InfoCrashed()
            {
                m_IdTargetFunc = _listOTargetFunc[_listOTargetFunc.Count - 1].m_lId;
                m_state = STATE.UNKNOWN;
            }

            public InfoCrashed(STATE state) : this ()
            {
                m_state = state;
            }
        }

        private static int MAX_HISTORY_INFOCRASHED = 6
            , MAX_COUNT_CRASHED_TO_RELOAD_GROUPSOURCES = 1;

        private class ListInfoCrashed : List<InfoCrashed>
        {
            public void MarkedItem(STATE state)
            {
                Add(new InfoCrashed(state));

                while (Count > MAX_HISTORY_INFOCRASHED)
                    RemoveAt(0);
            }

            public bool IsCrashed {
                get {
                    bool bRes = false;

                    if (Count > 1)
                        //if ((pair.Value[cntIdCrushed - 2].m_IdTargetFunc == lPrevIdTargetFunc)
                        //    && (pair.Value[cntIdCrushed - 1].m_IdTargetFunc == lCurIdTargetFunc))
                        if ((this[Count - 1].m_IdTargetFunc == _listOTargetFunc[_listOTargetFunc.Count - 1].m_lId)
                            && (this[Count - 2].m_state == STATE.CRASH)
                            && (this[Count - 1].m_state == STATE.CRASH)) {
                                bRes = true;

                            //pair.Value.RemoveRange(pair.Value.Count - 2, 2);
                        } else
                            ;
                    else
                        ;

                    return bRes;
                }
            }
        }
        /// <summary>
        /// Словарь для хранения статистической информации
        /// </summary>
        private class DictOManagementInfoCrashed : Dictionary<int, ListInfoCrashed>
        {
            public bool IsAbortReload;

            public void MarkedItem(int id, STATE state)
            {
                if (ContainsKey(id) == true)
                // группа сигналов уже добавлена
                    ;
                else
                    Add(id, new ListInfoCrashed());

                if (state == STATE.CCRASH)
                    IsAbortReload = true;
                else
                    ;

                // обновить дату/время события
                this[id].MarkedItem(state);
            }

            //private int CountCurrentCrashed {
            //    get {
            //        int iRes = 0;

            //        long lIdCurrentTargetFunc = _listOTargetFunc[_listOTargetFunc.Count - 1].m_lId;

            //        foreach (ListInfoCrashed list in Values)
            //            if (!(list.Find(ic => { return ic.m_IdTargetFunc == lIdCurrentTargetFunc; }) == null))
            //                iRes++;
            //            else
            //                ;

            //        return iRes;
            //    }
            //}

            public List<int> GetListIDCurrentCrashed()
            {
                List<int> listRes = new List<int> ();

                //long lPrevIdTargetFunc = -1L
                //    , lCurIdTargetFunc = -1L;
                int cntIdTargetFunc = _listOTargetFunc.Count
                    , cntIdCrushed = -1;

                if (cntIdTargetFunc > 1) {
                    //lPrevIdTargetFunc = _listOTargetFunc[cntIdTargetFunc - 2].m_lId;
                    //lCurIdTargetFunc = _listOTargetFunc[cntIdTargetFunc - 1].m_lId;

                    foreach (KeyValuePair<int, ListInfoCrashed> pair in this) {
                        if (pair.Value.IsCrashed == true)                        
                            listRes.Add(pair.Key);
                        else
                            ;
                    }
                }
                else
                    ;

                return listRes;
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
        private void pushCommandReloadGroupSources(INDEX_SRC type, int idOwner, bool bAbort)
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

        private struct OTargetFunc {
            public long m_lId;

            public DateTime m_datetime;
        }

        //private static long _lIdCurrentTargetFunc;
        private static List <OTargetFunc> _listOTargetFunc;        
        /// <summary>
        /// Целевая функция контроля
        /// </summary>
        private void targetFunc()
        {
            //Logging.Logg().Debug(@"StateManager::targetFunc () - итерация контроля; кол-во объектов=" + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

            DateTime now = DateTime.Now;
            int msecLimit = -1;
            List<int> listIDCurrentCrashed = null;

            //_lIdCurrentTargetFunc = HMath.GetRandomNumber();
            _listOTargetFunc.Add(new OTargetFunc() { m_lId = HMath.GetRandomNumber(), m_datetime = DateTime.Now });

            while (_listOTargetFunc.Count > MAX_HISTORY_INFOCRASHED)
                _listOTargetFunc.RemoveAt(0);

            foreach (OManagement o in m_listObjects)
            {
                msecLimit = 0; // признак необходимости проверки объекта в текущей итерации

                switch (o.m_state)
                {
                    case STATE.CRASH:
                    //    msecLimit = -1;
                    //    break;
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
                    && (now - o.m_dtUpdate).TotalMilliseconds > msecLimit) {
                // 1-ое состояние сбоя
                    switch (o.m_state) {
                        case STATE.CONTROLED:
                            o.SetCrashed();
                            break;
                        case STATE.ADDED:
                        case STATE.REMOVED:
                            o.SetCCrashed();
                            break;
                        case STATE.CRASH:
                        default:
                            break;
                    }

                    m_dictInfoCrashed.MarkedItem(o.m_id, o.m_state);
                }
                else
                    if (msecLimit < 0)
                        // объект повторил состояние сбоя
                        ;
                    else
                        ;
            }

            foreach (KeyValuePair<KeyValuePair<INDEX_SRC, int>, DictOManagementInfoCrashed> pair in m_dictInfoCrashed)
#if _SEPARATE_APPDOMAIN
                if (pair.Value.IsAbortReload == true)
                {
                    pair.Value.IsAbortReload = false;
                    // выгрузка библиотеки БЕЗ корректного останова
                    pushCommandReloadGroupSources(pair.Key.Key, pair.Key.Value, true);
                    //??? самостоятельно выполнить ремове OManagement
                }
                else {
#endif
                    listIDCurrentCrashed = pair.Value.GetListIDCurrentCrashed();
                    if (listIDCurrentCrashed.Count > 0)
#if _SEPARATE_APPDOMAIN
                        if (listIDCurrentCrashed.Count > MAX_COUNT_CRASHED_TO_RELOAD_GROUPSOURCES)
                            // выгрузка библиотеки с корректным остановом
                            pushCommandReloadGroupSources(pair.Key.Key, pair.Key.Value, false);
                        else
#endif
                            foreach (int id in listIDCurrentCrashed)
                                pushStateChangedGroupSignals(new ID(new object[] { pair.Key.Key, pair.Key.Value, id }));
                    else
                        ;
                }
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