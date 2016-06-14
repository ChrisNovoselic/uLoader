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
        /// <summary>
        /// Класс для идентификации объекта контроля
        /// </summary>
        public class ID : Object
        {
            public int m_idTypeRegistred;

            public int m_idGroupSgnls;

            public ID(object[] ids)
            {
                m_idTypeRegistred = (int)ids[0];
                m_idGroupSgnls = (int)ids[1];
            }

            public override bool Equals(object obj)
            {
                return (this.m_idTypeRegistred == (obj as ID).m_idTypeRegistred)
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
                int indx = indexOfId(id);

                if (!(indx < 0))
                    this[indx].SetRemoved();
                else
                    Logging.Logg().Error(@"HHandlerQueue.ListOManagement::RemoveItem (IdGrpSgnls=" + id.m_idGroupSgnls
                        + @", IdTypeRegistred=)" + id.m_idTypeRegistred + @" - объект для удаления не найден ...", Logging.INDEX_MESSAGE.NOT_SET);
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
            m_timerFunc.Change(due, period);

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

            //Console.WriteLine(@"StateManager::add (id=" + id.m_idTypeRegistred + @", key=" + id.m_idGroupSgnls + @") - добавить объект; кол-во = " + m_listObjects.Count + @" ...");
            Logging.Logg().Debug(@"StateManager::add (id=" + id.m_idTypeRegistred + @", key=" + id.m_idGroupSgnls + @") - добавить объект; кол-во -> " + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Добавить новый объект для контроля
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

            //Console.WriteLine(@"StateManager::remove (id=" + id.m_idTypeRegistred + @", key=" + id.m_idGroupSgnls + @") - удалить объект; кол-во = " + m_listObjects.Count + @" ...");
            Logging.Logg().Debug(@"StateManager::remove (id=" + id.m_idTypeRegistred + @", key=" + id.m_idGroupSgnls + @") - удалить объект; кол-во <- " + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Удалить объект из списка контролируемых объектов
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

            //Console.WriteLine(@"StateManager::confirm (id=" + id.m_idTypeRegistred + @", key=" + id.m_idGroupSgnls + @") - подтвердить состояние объекта ...");
            Logging.Logg().Debug(@"StateManager::confirm (id=" + id.m_idTypeRegistred + @", key=" + id.m_idGroupSgnls + @") - подтвердить состояние объекта; кол-во = " + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Подтвердить изменение состояния контролируемого объекта
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
        private void onCrashed(/*HHandlerQueue.EventCrashedArgs*/ object obj)
        {
            HHandlerQueue.EventCrashedArgs ev = obj as HHandlerQueue.EventCrashedArgs;
            
            object[] arToSend = null; // массив для аргументов состояния

            for (INDEX_SRC type = INDEX_SRC.SOURCE; type < INDEX_SRC.COUNT_INDEX_SRC; type++)
                foreach (GroupSources grpSrc in m_listGroupSources[(int)type])
                    if (grpSrc.m_iIdTypePlugInObjectLoaded == ev.m_id.m_idTypeRegistred)
                    {
                        switch (ev.m_state)
                        {
                            case STATE.CONTROLED:// группа сигналов в работе
                                arToSend = new object[] {
                                    (int)StatesMachine.STATE_CHANGED_GROUP_SIGNALS
                                    , type
                                    , grpSrc.m_strID
                                    , grpSrc.GetIdGroupSignals (ev.m_id.m_idGroupSgnls)
                                };
                                // поставить в очередь 2 состояния: последовательный останов/запуск группы сигналов
                                Push(null, new object[] {
                                    new object[] {
                                        arToSend // для 'STOP'
                                        , arToSend // для 'START'
                                        ,
                                    },
                                });
                                break;
                            case STATE.ADDED:
                            case STATE.REMOVED:
#if _SEPARATE_APPDOMAIN
                                // группа сигналов не получила подтверждения от библиотеки при изменении своего состояния
                                // поставить в очередь 1 состояние - полная выгрузка/загрузки библиотеки
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
#endif
                                break;
                            default:
                                break;
                        }

                        type = INDEX_SRC.COUNT_INDEX_SRC; // для прерывания внешнего цикла

                        break; // прервать внутренний цикл
                    }
                    else
                        ;
        }
        /// <summary>
        /// Целевая функция контроля
        /// </summary>
        private void targetFunc()
        {
            //Logging.Logg().Debug(@"StateManager::targetFunc () - итерация контроля; кол-во объектов=" + m_listObjects.Count + @" ...", Logging.INDEX_MESSAGE.NOT_SET);

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
                    //new Thread (new ParameterizedThreadStart (onCrashed)).Start(new EventCrashedArgs() { m_id = o.m_id, m_state = o.m_state });
                    onCrashed(new EventCrashedArgs() { m_id = o.m_id, m_state = o.m_state });
                    //Console.WriteLine(@"HHandlerQueue::targetFunc () - eventCrashed (id=" + o.m_id.m_idTypeRegistred
                    //    + @", key=" + o.m_id.m_idGroupSgnls
                    //    + @", state=" + o.m_state.ToString () + @") - ...");

                    o.SetCrashed();
                    //o.Update();
                }
                else
                    ;
            }
        }
    }
}