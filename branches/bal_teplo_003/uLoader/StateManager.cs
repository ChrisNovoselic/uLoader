using System;
using System.Collections.Generic;

using System.Reflection; //Assembly
using System.IO;
using System.Data;

using System.Threading;
using System.ComponentModel;

using System.Diagnostics; //Debug

using HClassLibrary;
using uLoaderCommon;

namespace uLoader
{
    //public class StateManager : HClassLibrary.HHandlerQueue
    partial class HHandlerQueue
    {
        public class StateManager
        {
            private static int MIN_MSEC_TIMERFUNC_UPDATE = 6006;
            /// <summary>
            /// Интервал в милисекундах для проверки меток времени обновления
            /// </summary>
            public static int MSEC_TIMERFUNC_UPDATE = -1;

            private static int MIN_MSEC_WAIT_CONFIRMED = 46006;
            /// <summary>
            /// Интервал времени, в течении которого состояние объекта считать актуальным
            /// </summary>
            public static int MSEC_WAIT_CONFIRMED = -1;

            public static int MAX_HISTORY_INFOCRASHED = 6
                , MAX_COUNT_CRASHED_TO_RELOAD_GROUPSOURCES = 1;
            /// <summary>
            /// Перечисление - параметры менеджера управления состояниями
            /// </summary>
            private enum INDEX_PARAMETER { UNKNOWN = -1
                //, DEBUG
                , TIMER_UPDATE, WAIT_CONFIRMED
                , SHEDULE_TIMESTART, SHEDULE_TIMESPAN
            , COUNT }

            //public bool m_bDebug;
            /// <summary>
            /// Признак необходимости включения в работу менеджера состояний групп сигналов
            /// </summary>
            public bool m_bTurn;
            /// <summary>
            /// Признак выполнения операции выгрузки/загрузки по расписанию
            ///  должен быть указан интервал, при этом интервал д.б. == или более 1 ч
            /// </summary>
            private bool sheduleTurn { get { return m_tsShedule == TimeSpan.Zero ? false : (!(m_tsShedule.TotalHours < 1)) ? true : false; } }
            ///// <summary>
            ///// Дата/время активации объекта (для определения первого выполнения выгрузки/загрузки !!!по расписанию)
            ///// </summary>
            //private DateTime _dtStart;

            int _cntShedule;
            /// <summary>
            /// Метка даты/времени крайнего выполнения выгрузки/загрузки
            /// </summary>
            private DateTime m_dtReload;
            /// <summary>
            /// Смещение от начала часа выполнения операции выгрузки/загрузки
            /// </summary>
            private DateTime m_dtShedule;
            /// <summary>
            /// Интервал выполнения операции выгрузки/загрузки
            /// </summary>
            private TimeSpan m_tsShedule;

            private StateManager()
            {
                //_dtStart = DateTime.Now;
                _cntShedule = -1;

                m_dtReload =
                    //DateTime.MinValue
                    DateTime.Now
                    ;

                m_bTurn = false;
                m_dtShedule = DateTime.MinValue;
                m_tsShedule = TimeSpan.Zero;
            }

            public StateManager(string ini) : this ()
            {
                string[] arPars = null
                    , values = null;

                try {
                    if (ini.Equals(string.Empty) == false) {
                        arPars = ini.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.PAIR_VAL]);

                        foreach (string key_val in arPars)
                            for (INDEX_PARAMETER par = (INDEX_PARAMETER.UNKNOWN + 1); par < INDEX_PARAMETER.COUNT; par++)                            
                                if (key_val.IndexOf(par.ToString()) == 0) {
                                    values = key_val.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE]);

                                    if (values.Length == 2)
                                        if (values[1].Equals(string.Empty) == false)
                                            switch (par) {
                                                case INDEX_PARAMETER.WAIT_CONFIRMED:
                                                    MSEC_WAIT_CONFIRMED = (int)new HTimeSpan(values[1]).Value.TotalSeconds * 1000; ;
                                                    break;
                                                case INDEX_PARAMETER.TIMER_UPDATE:
                                                    MSEC_TIMERFUNC_UPDATE = (int)new HTimeSpan(values[1]).Value.TotalSeconds * 1000;                                                    
                                                    break;
                                                //case INDEX_PARAMETER.SHEDULE_TURN:
                                                //    break;
                                                case INDEX_PARAMETER.SHEDULE_TIMESTART:
                                                    break;
                                                case INDEX_PARAMETER.SHEDULE_TIMESPAN:
                                                    m_tsShedule = new HTimeSpan(values[1]).Value;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        else
                                            Logging.Logg().Warning(string.Format(@"HHandlerQueue.StateManager::ctor (ключ={0}) - значение=не_установлено", par.ToString())
                                                , Logging.INDEX_MESSAGE.NOT_SET);
                                    else
                                        throw new Exception(string.Format(@"HHandlerQueue.StateManager::ctor (ключ={0}) - пара ключ:значение не распознана", par.ToString()));

                                    break;
                                } else
                                    ;

                        // таймер включается только если интервал обновления > 6 сек И время ожидания подтверждения состояния > 46 сек
                        // т.к. иначе проверка работоспособности заблокирует выполнение основного потока
                        m_bTurn = (!(MSEC_TIMERFUNC_UPDATE < MIN_MSEC_TIMERFUNC_UPDATE)) && (!(MSEC_WAIT_CONFIRMED < MIN_MSEC_WAIT_CONFIRMED));
                    } else
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"HHandlerQueue.StateManager::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public bool IsReload {
                get {
                    bool bRes = false;

                    bRes = (sheduleTurn == true)
                        && (((DateTime.Now -
                            //(m_dtReload == DateTime.MinValue ? _dtStart : m_dtReload))
                            m_dtReload)
                            - m_tsShedule).TotalMinutes > 0);

                    if (bRes == true) {
                        m_dtReload = DateTime.Now;
                        _cntShedule++;
                    } else
                        ;

                    return bRes;
            } }
        }
        public HHandlerQueue.StateManager m_stateManager;
#if _STATE_MANAGER
        /// <summary>
        /// Класс для идентификации объекта контроля
        /// </summary>
        public struct ID //: Object
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
                return (this.m_typeOwner == ((ID)obj).m_typeOwner)
                    && (this.m_idOwner == ((ID)obj).m_idOwner)
                    && (this.m_idGroupSgnls.Equals(((ID)obj).m_idGroupSgnls) == true);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public string ToPrint()
            {
                string strRes = string.Empty;

                strRes = string.Format(@"TypeOwner={0}, IdOwner={1}, Id={2}", m_typeOwner, m_idOwner, m_idGroupSgnls);

                return strRes;
            }
        }
        /// <summary>
        /// Перечисление - состояния объектов контроля
        /// </summary>
        private enum STATE { UNKNOWN, ADDED, REMOVED, CONTROLED, CRASH, FCRASH
            , COUNT }
        /// <summary>
        /// Класс для хранения информации о контролируемом объекте 
        /// </summary>
        private class OManagement
        {
            public ID m_id;

            private STATE _state;
            /// <summary>
            /// Текущее состояние объекта контроля
            /// </summary>
            public STATE m_state { get { return _state; } set { if (!(_state == value)) _dtUpdateState = DateTime.Now; else; _state = value; } }
            /// <summary>
            /// Предельный интервал отсутствия обновлений состояния
            /// </summary>
            public TimeSpan m_tsLimit;
            /// <summary>
            /// Крайнее время обновления
            /// </summary>
            private DateTime _dtUpdateState;
            /// <summary>
            /// Крайнее время обновления
            /// </summary>
            private DateTime _dtUpdateData;

            public DateTime m_dtUpdate { get { return (_dtUpdateState - _dtUpdateData).TotalSeconds > 0 ? _dtUpdateState : _dtUpdateData; } }
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

            public void SetFCrashed() { m_state = STATE.FCRASH; }
            /// <summary>
            /// Обновить метку времени крайнего обновления
            /// </summary>
            public void Update() { _dtUpdateData = DateTime.Now; }

            private OManagement()
            {
                _state = STATE.ADDED;

                _dtUpdateState =
                _dtUpdateData =
                    DateTime.MinValue;
            }

            public OManagement(ID id, TimeSpan tsLimit) : this()
            {
                m_id = id;

                m_tsLimit = tsLimit;
            }
        }
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
                    this.Add(new OManagement(id, tsLimit));
                else
                    // предупреждение - такой объект уже контролируется
                    Logging.Logg().Warning(@"HHandlerQueue.ListOManagement::AddItem (IdGrpSgnls=" + id.m_idGroupSgnls
                        + @", IdOwner=" + id.m_idOwner + @") - добавляемый объект уже контролируется ...", Logging.INDEX_MESSAGE.NOT_SET);

                return this.Count - 1;
            }
            /// <summary>
            /// Удалить (непосредственно) элементы - объекты контроля
            ///  в случае аварийной выгрузки/загрузки библиотеки, установить признаки подготовки к удалению не представляется возможным
            ///  команда 'Stop' не выполняется
            /// </summary>
            /// <param name="id">Фильтр (сложный идентификатор) для удаления</param>
            /// <returns>Количество удалкенных элементов</returns>
            public int DeleteItems(ID id)
            {
                int iRes = -1; // ошибка при удалении

                List<OManagement> listToDelete =
                    FindAll(item => (item.m_id.m_typeOwner == id.m_typeOwner)
                        && (item.m_id.m_idOwner == id.m_idOwner));

                while (listToDelete.Count > 0) {
                    Remove(listToDelete[0]);

                    listToDelete.RemoveAt(0);
                }

                return iRes;
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
                            msgErr = string.Format(@"объект [сост.={0}] не может получить подтверждение", this[indx].m_state);
                else
                    // ошибка - объект для подтверждения состояния не найден
                    msgErr = @"объект для подтверждения состояния не найден";

                if (msgErr.Equals (string.Empty) == false)
                    Logging.Logg().Error(string.Format(@"HHandlerQueue.ListOManagement::Confirm (IdGrpSgnls={0}, IdOwner={1}) - {2} ...", id.m_idGroupSgnls, id.m_idOwner, msgErr), Logging.INDEX_MESSAGE.NOT_SET);
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
        /// <summary>
        /// Класс для хранения статистической информации об объектах контроля
        /// </summary>
        private class DictInfoCrashed : Dictionary<KeyValuePair <INDEX_SRC, int>, DictOManagementInfoCrashed>
        {
            /// <summary>
            /// Сохранить состояние объекта контроля
            /// </summary>
            /// <param name="e">Аргумент события - аврийное состояние объекта контроля</param>
            public void MarkedItem(HHandlerQueue.EventCrashedArgs e, STATE state) { MarkedItem(e.m_id, state); }
            /// <summary>
            /// Сохранить состояние объекта контроля
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта контроля</param>
            /// <param name="state">Состояние объекта контроля</param>
            public void MarkedItem(ID id, STATE state)
            {
                KeyValuePair<INDEX_SRC, int> key = GetKey(id);

                if (ContainsKey(key) == true)
                    ; // группа источников уже добавлена
                else
                    Add(key, new DictOManagementInfoCrashed());
                // добавить группу сигналов
                this[key].MarkedItem(id.m_idGroupSgnls, state);

                //Debug.WriteLine
                Logging.Logg().Debug
                (
                    string.Format(@"MARKED- {0}- {1} [STATE={2}]"
                    , DateTime.Now.ToString()
                    , id.ToPrint(), state)
                        , Logging.INDEX_MESSAGE.NOT_SET
                );
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

        private class ListInfoCrashed : List<InfoCrashed>
        {
            public void MarkedItem(STATE state)
            {
                Add(new InfoCrashed(state));

                while (Count > StateManager.MAX_HISTORY_INFOCRASHED)
                    RemoveAt(0);
            }

            public bool IsCrashed {
                get {
                    bool bRes = false;

                    STATE prevState = STATE.UNKNOWN
                        , curState = STATE.UNKNOWN;

                    // произведено как минимум две проверки
                    if (Count > 1) {
                        prevState = this[Count - 2].m_state;
                        curState = this[Count - 1].m_state;

                        if ((this[Count - 1].m_IdTargetFunc == _listOTargetFunc[_listOTargetFunc.Count - 1].m_lId)
                                && (prevState == STATE.CRASH)
                                && (curState == STATE.CRASH)) {
                            bRes = true;

                            //pair.Value.RemoveRange(pair.Value.Count - 2, 2);
                        } else
                            ;
                    } else
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
            /// <summary>
            /// Признак аварийной выгрузки/загрузки библиотеки
            /// </summary>
            public bool IsForceReload;
            /// <summary>
            /// Сохранить состояние объекта контроля
            /// </summary>
            /// <param name="id">Идентификатор объекта контроля</param>
            /// <param name="state">Состояние объекта контроля при событии</param>
            public void MarkedItem(int id, STATE state)
            {
                if (ContainsKey(id) == true)
                // группа сигналов уже добавлена
                    ;
                else
                    Add(id, new ListInfoCrashed());

                if (state == STATE.FCRASH)
                    IsForceReload = true;
                else
                    ;

                // обновить дату/время события
                this[id].MarkedItem(state);
            }
            /// <summary>
            /// Возвратить список идентификаторов объектов контроля, требующих стоп/старт
            /// </summary>
            /// <returns>Список идентификаторов</returns>
            public List<int> GetListIDCrashed()
            {
                List<int> listRes = new List<int> ();

                foreach (KeyValuePair<int, ListInfoCrashed> pair in this)
                    if (pair.Value.IsCrashed == true)                        
                        listRes.Add(pair.Key);
                    else
                        ;

                return listRes;
            }
        }
        /// <summary>
        /// Словарь статистической информации об объектах контроля
        /// </summary>
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
        /// <summary>
        /// Класс для храннеия информации о событии - аварийное состояние объекта контроля
        /// </summary>
        private class EventCrashedArgs : EventArgs
        {
            /// <summary>
            /// Конструктор - дополнительный (без параметров)
            ///  чтобы нельзя было создать пустое событие
            /// </summary>
            private EventCrashedArgs() { }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="id">Сложный идентификатор</param>
            /// <param name="bForceReload">Признак аврийной выгрузки/загрузки библиотеки</param>
            public EventCrashedArgs(ID id, bool bForceReload)
            {
                m_id = id;
                m_bForceReload = bForceReload;
            }
            /// <summary>
            /// Сложный идентификатор объекта контроля
            /// </summary>
            public ID m_id;
            /// <summary>
            /// Признак аврийной выгрузки/загрузки библиотеки
            ///  действителен только для групп источников (ID::m_idGroupSgnls == -1)
            /// </summary>
            public bool m_bForceReload;
        }

        private delegate void EventHandlerCrashed (EventCrashedArgs arg);

        private event EventHandlerCrashed eventCrashed;
        /// <summary>
        /// Поставить в очередь 2 события изменения состояния группы сигналов
        /// </summary>
        /// <param name="id">Сложный идентификатор группы сигналов (тип, источник, группа)</param>
        private void pushStateChangedGroupSignals(ID id)
        {
            object[] arToSend = null; // массив для аргументов состояния
            GroupSources grpSrc =
                m_listGroupSources[(int)id.m_typeOwner].Find(g => { return FormMain.FileINI.GetIDIndex (g.m_strID) == id.m_idOwner; });
            string msgDebug = string.Empty;

            //if (m_stateManager.m_bDebug == false) {
            // поставить в очередь 2 состояния: последовательный останов/запуск группы сигналов
                arToSend = new object[] {
                    (int)StatesMachine.STATE_CHANGED_GROUP_SIGNALS
                    , id.m_typeOwner
                    , grpSrc.m_strID
                    , grpSrc.GetIdGroupSignals (id.m_idGroupSgnls)
                };

                Push(null, new object[] {
                    new object[] {
                        arToSend // для 'STOP'
                        , arToSend // для 'START'
                        ,
                    },
                });
            //} else
            //    ;

            msgDebug = string.Format(@"HHandlerQueue::pushStateChangedGroupSignals ({0}) - ...", id.ToPrint());
            Logging.Logg().Debug(msgDebug, Logging.INDEX_MESSAGE.NOT_SET);
            //Debug.WriteLine(DateTime.Now.ToString() + @"- " + msgDebug);
        }
#if _SEPARATE_APPDOMAIN
        /// <summary>
        /// Поставить в очередь 1(одно) состояние - полная выгрузка/загрузки библиотеки
        /// </summary>
        /// <param name="type">Тип группы источников</param>
        /// <param name="idOwner">Идентификатор (индекс) группы источников</param>
        private void pushCommandReloadGroupSources(INDEX_SRC type, int idOwner, bool bAbort)
        {
            GroupSources grpSrc = null;
            ID id;
            string msgDebug = string.Empty;

            grpSrc = m_listGroupSources[(int)type].Find(g => { return FormMain.FileINI.GetIDIndex(g.m_strID) == idOwner; });

            //if (m_stateManager.m_bDebug == false)
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
            //else
            //    ;

            id = new ID(new object[] { type, idOwner, -1 });
            msgDebug = string.Format(@"HHandlerQueue::pushCommandReloadGroupSources ({0}) - ...", id.ToPrint());
            Logging.Logg().Debug(msgDebug, Logging.INDEX_MESSAGE.NOT_SET);
            //Debug.WriteLine(DateTime.Now.ToString() + @"- " + msgDebug);
        }
#endif

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
            double msecLimit = -1F
                , msecCurrent = -1F;
            List<int> listIdCrashed = null;
            List<ID?> listIDToReload = null;
            ID id;

            //_lIdCurrentTargetFunc = HMath.GetRandomNumber();
            _listOTargetFunc.Add(new OTargetFunc() { m_lId = HMath.GetRandomNumber(), m_datetime = DateTime.Now });

            while (_listOTargetFunc.Count > StateManager.MAX_HISTORY_INFOCRASHED)
                _listOTargetFunc.RemoveAt(0);

#if _SEPARATE_APPDOMAIN
            if (m_stateManager.IsReload == true) {
            // выгрузка/загрузка по расписанию
                listIDToReload = new List<ID?>();

                foreach (OManagement o in m_listObjects) {
                    if (listIDToReload.Find(item => {
                            return ((item.GetValueOrDefault().m_typeOwner == o.m_id.m_typeOwner)
                                && (item.GetValueOrDefault().m_idOwner == o.m_id.m_idOwner));
                        }) == null)
                        listIDToReload.Add(new ID(new object[] { o.m_id.m_typeOwner, o.m_id.m_idOwner, -1 }));
                    else
                        ;
                }

                foreach (ID item in listIDToReload)
                // выгрузка библиотеки БЕЗ корректного останова
                    eventCrashed(new EventCrashedArgs(item, true));
            } else {
#endif
                foreach (OManagement o in m_listObjects) {
                    msecLimit = 0; // признак необходимости проверки объекта в текущей итерации                    

                    switch (o.m_state) {
                        case STATE.CONTROLED:
                            msecLimit = (int)o.m_tsLimit.TotalMilliseconds;
                            break;
                        case STATE.ADDED:
                        case STATE.REMOVED:
                        case STATE.CRASH:
                            msecLimit = StateManager.MSEC_WAIT_CONFIRMED;
                            break;
                        default:
                            break;
                    }

                    msecCurrent = (now - o.m_dtUpdate).TotalMilliseconds;

                    //Debug.WriteLine(string.Format(@"{0}- {1}, STATE={2} [CURRENT={3}, LIMIT={4}]"
                    //    , DateTime.Now.ToString()
                    //    , o.m_id.ToPrint(), o.m_state
                    //    , msecCurrent, msecLimit));

                    if ((msecLimit > 0)
                        && (msecCurrent > msecLimit)) {
                        switch (o.m_state) {
                            case STATE.CONTROLED:
                                o.SetCrashed();
                                break;
                            case STATE.ADDED:
                            case STATE.REMOVED:
                                o.SetFCrashed();
                                break;
                            case STATE.CRASH:
                            //??? почему не ForceCrashed. Т.к.  'eventCrashed' принимается на основе 2-х подряд STATE::CRASH
                            default:
                                break;
                        }

                        m_dictInfoCrashed.MarkedItem(o.m_id, o.m_state);
                    }
                    else
                        ;
                }

                foreach (KeyValuePair<KeyValuePair<INDEX_SRC, int>, DictOManagementInfoCrashed> pair in m_dictInfoCrashed) {
#if _SEPARATE_APPDOMAIN
                    if (pair.Value.IsForceReload == true) {
                        id = new ID(new object[] { pair.Key.Key, pair.Key.Value, -1 });
                        // восстановить признак
                        pair.Value.IsForceReload = false;
                        //??? самостоятельно выполнить ремове OManagement
                        m_listObjects.DeleteItems(id);
                        // выгрузка библиотеки АВАРИЙНО (БЕЗ корректного останова)
                        eventCrashed(new EventCrashedArgs(id, true));
                    } else {
#endif
                        listIdCrashed = pair.Value.GetListIDCrashed();
                        if (listIdCrashed.Count > 0)
#if _SEPARATE_APPDOMAIN
                            if (listIdCrashed.Count > StateManager.MAX_COUNT_CRASHED_TO_RELOAD_GROUPSOURCES)
                                // выгрузка библиотеки ШТАТНО (с корректным остановом)
                                eventCrashed(new EventCrashedArgs(new ID(new object[] { pair.Key.Key, pair.Key.Value, -1 }), false));
                            else
#endif
                                foreach (int item in listIdCrashed) {
                                    id = new ID(new object[] { pair.Key.Key, pair.Key.Value, item });
                                    // стоп/старт объекта контроля
                                    eventCrashed(new EventCrashedArgs(id, false));
                                }
                        else
                            ;
#if _SEPARATE_APPDOMAIN
                    }
#endif
                }
            }
        }
#endif

#if _STATE_MANAGER
        private void onEventCrashed(EventCrashedArgs ev)
        {
            if (ev.m_id.m_idGroupSgnls < 0) {
#if _SEPARATE_APPDOMAIN
                pushCommandReloadGroupSources(ev.m_id.m_typeOwner, ev.m_id.m_idOwner, ev.m_bForceReload);
#endif
            } else {
                pushStateChangedGroupSignals(ev.m_id);
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
                    if (m_stateManager.m_bTurn == true) {
                        due = 0;
                        period = StateManager.MSEC_TIMERFUNC_UPDATE;
                    } else
                        if (active == false)
                            ; // оставить значения по умолчанию
                        else
                            ; // других состояний 'bool' не существует
                else
                    ; // отмена активации
            else
                ; // состояние не изменилось

            Logging.Logg().Debug(string.Format(@"StateManager::Activate (active={0}, TURN={1}) - "
                + (due == System.Threading.Timeout.Infinite ? @"ДЕ" : string.Empty) + @"Активация объекта контроля ...", active, m_stateManager.m_bTurn)
                , Logging.INDEX_MESSAGE.NOT_SET);

            m_timerFunc.Change(due, period);
#endif
            return bRes;
        }        
    }
}