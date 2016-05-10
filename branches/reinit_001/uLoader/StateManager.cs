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
    public class StateManager : HClassLibrary.HHandlerQueue
    {
        /// <summary>
        /// Класс для идентификации объекта контроля
        /// </summary>
        public class ID : Object
        {
            public int m_id;
        }
        /// <summary>
        /// Перечисление - состояния объектов контроля
        /// </summary>
        private enum STATE { UNKNOWN, ADDED, REMOVED, CONTROLED, CRASH, COUNT }
        /// <summary>
        /// Класс для хранения информации о контролируемом объекте 
        /// </summary>
        private struct OManagement
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
            public void SetControled() { m_state = STATE.REMOVED; }

            public void SetCrashed() { m_state = STATE.CRASH; }
            /// <summary>
            /// Обновить метку времени крайнего обновления
            /// </summary>
            public void Update()
            {
                m_dtUpdate = DateTime.Now;
            }
        }
        /// <summary>
        /// Интервал в милисекундах для проверки меток времени обновления
        /// </summary>
        public static int MSEC_TIMERFUNC_UPDATE = 1006;
        /// <summary>
        /// Интервал времени, в течении которого состояние объекта считать актуальным
        /// </summary>
        public static int MSEC_CONFIRM_WAIT = 6666;
        /// <summary>
        /// Перечисление - состояния для организации контроля списка объектов
        /// </summary>
        private enum StatesMachine : int { Unknown = -1, Add, Remove, Confirm, Update, Control
            , Count }
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

                if (indx < 0)
                    this.Add(new OManagement() { m_id = id, m_state = STATE.ADDED, m_tsLimit = tsLimit, m_dtUpdate = DateTime.Now });
                else
                    // ошибка - такой объект уже контролируется
                    ;

                return this.Count - 1;
            }
            /// <summary>
            /// Установить признак подготовки к удалению
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            public void RemoveItem (ID id)
            {
                this[indexOfId(id)].SetRemoved ();
            }
            /// <summary>
            /// Подтвердить изменения состояния (добавление/удаление)
            /// </summary>
            /// <param name="id">Сложный идентификатор объекта</param>
            public void Confirm(ID id)
            {
                int indx = indexOfId(id);

                if (!(indx < 0))
                    if (this[indx].m_state == STATE.ADDED)
                        this[indx].SetControled();
                    else
                        if (this[indx].m_state == STATE.REMOVED)
                            this.RemoveAt(indx);
                        else
                            ; // ошибка - объект не может получить подтверждение
                else
                    ; // ошибка - объект для подтверждения состояния не найден
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
                {
                    i++;

                    if (this[i].m_id == id)
                        break;
                    else
                        ;
                }

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
        /// Конструктор - основной (без параметров)
        /// </summary>
        public StateManager()
        {
            m_timerFunc = new Timer(timerFunc);
            m_listObjects = new ListOManagement();
        }
        /// <summary>
        /// Функция обратного вызова для таймера
        /// </summary>
        /// <param name="obj">Аргумент при вызове функции</param>
        private void timerFunc(object obj)
        {
            // добавить для обработки одно событие
            Push(null, new object[] { // несколько событий
                new object [] { //очередное событие
                    new object [] { StatesMachine.Control, } // параметры события (с 0-ым индексом - идентификатор события)
                },
            });
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

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
            m_timerFunc.Change (due, period);

            return bRes;
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
        }
        /// <summary>
        /// Добавить новый объект для контроля
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        /// <param name="tsLimit">Интервал времени в течении которого состояние объекта остается актуальным без обновления</param>
        public void Add(int id, TimeSpan tsLimit)
        {
            Push (null, new object [] {
                new object [] {
                    new object [] { StatesMachine.Add, id, tsLimit }
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
        }
        /// <summary>
        /// Удалить объект из списка контролируемых объектов
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        public void Remove(int id)
        {
            Push(null, new object[] {
                new object [] {
                    new object [] { StatesMachine.Remove, id }
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
        }
        /// <summary>
        /// Подтвердить изменение состояния контролируемого объекта
        /// </summary>
        /// <param name="id">Сложный идентификатор объекта</param>
        public void Confirm(int id)
        {
            Push(null, new object[] {
                new object [] {
                    new object [] { StatesMachine.Confirm, id }
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
        public void Update(int id)
        {
            Push(null, new object[] {
                new object [] {
                    new object [] { StatesMachine.Update, id }
                },
            });
        }
        /// <summary>
        /// Получить результат запроса
        /// </summary>
        /// <param name="state">Событие для которого требуется получить результат (идентификатор типа запроса)</param>
        /// <param name="error">Признак ошибки при получении результата</param>
        /// <param name="outobj">Объект - результат запроса</param>
        /// <returns>Признак выполнения функции</returns>
        protected override int StateCheckResponse(int state, out bool error, out object outobj)
        {
            int iRes = -1;

            error = true;
            outobj = null;

            ItemQueue itemQueue = null;

            switch ((StatesMachine)state)
            {
                case StatesMachine.Add:
                    iRes = 0;
                    error = false;

                    itemQueue = Peek;

                    add((ID)itemQueue.Pars[0], (TimeSpan)itemQueue.Pars[1]);
                    break;
                case StatesMachine.Remove:
                    remove((ID)itemQueue.Pars[0]);
                    break;
                case StatesMachine.Confirm:
                    confirm((ID)itemQueue.Pars[0]);
                    break;
                case StatesMachine.Update:
                    update((ID)itemQueue.Pars[0]);
                    break;
                case StatesMachine.Control:
                    iRes = 0;
                    error = false;

                    targetFunc();
                    break;
                default:
                    break;
            }

            return iRes;
        }
        /// <summary>
        /// Запросить данные
        /// </summary>
        /// <param name="state">Событие для которого запрашиваются данные</param>
        /// <returns>Признак выполнения функции</returns>
        protected override int StateRequest(int state)
        {
            int iRes = 0;
            
            switch ((StatesMachine)state)
            {
                case StatesMachine.Add:
                case StatesMachine.Remove:
                case StatesMachine.Update:
                case StatesMachine.Control:
                    //Не требуют запроса
                    break;
                default:
                    break;
            }

            return iRes;
        }
        /// <summary>
        /// Обработать результат запроса
        /// </summary>
        /// <param name="state">Событие для которого обрабатываются полученные данные</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;

            switch ((StatesMachine)state)
            {
                case StatesMachine.Add:
                case StatesMachine.Remove:
                case StatesMachine.Update:
                case StatesMachine.Control:
                    //Не требуют обработки результата
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        public class EventCrashedArgs : EventArgs
        {
            public ID m_id;
        }

        public delegate void EventHandlerCrashed (EventCrashedArgs arg);

        public event EventHandlerCrashed EventCrashed;
        /// <summary>
        /// Целевая функция контроля
        /// </summary>
        private void targetFunc()
        {
            Logging.Logg().Debug(@"StateManager::targetFunc () - итерация контроля; кол-во объектов=" + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

            DateTime now = DateTime.Now;
            int msecLimit = -1;

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
                    o.SetCrashed();

                    EventCrashed(new EventCrashedArgs() { m_id = o.m_id });
                }
                else
                    ;
            }
        }
    }
}