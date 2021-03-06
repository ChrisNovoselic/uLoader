﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

namespace xmlLoader
{
    class HHandlerQueue : HClassLibrary.HHandlerQueue
    {
        /// <summary>
        /// Перечисление - возможные состояния для обработки
        /// </summary>
        public enum StatesMachine
        {
            UNKNOWN = -1
            , UDP_CONNECTED_CHANGE // запрос на изменение состояния
            , WRITER_READY_CHANGE // запрос на изменение состояния
            , UDP_CONNECTED_CHANGED // событие - факт изменения состояния
            , UDP_LISTENER_PACKAGE_RECIEVED //Получен очередной XML-пакет
            , XML_PACKAGE_VERSION //Версия(строка) шаблон XML-пакета
            , XML_PACKAGE_TEMPLATE //Шаблон XML-пакета
            , NUDP_LISTENER //Номер порта прослушивателя
            , LIST_DEST //Список источников данных (назначение - сохранение полученных значений)
            , MESSAGE_TO_STATUSSTRIP
        }

        private FormMain.FileINI m_fileINI;

        public event DelegateObjectFunc EvtToFormMain;

        public HHandlerQueue(string strNameFileINI)
        {
            m_fileINI = new FormMain.FileINI(strNameFileINI);
        }

        public bool AutoStart { get { return bool.Parse(m_fileINI.GetSecValueOfKey(@"Reader", @"AUTO_START")); } }

        public string FormMainText { get { return m_fileINI.GetMainValueOfKey(@"Text"); } }
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
                    case StatesMachine.UDP_CONNECTED_CHANGE: // запрос-команда на изменение состояния (от формы)
                    case StatesMachine.WRITER_READY_CHANGE:
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = itemQueue.Pars[0];
                        break;
                    case StatesMachine.UDP_CONNECTED_CHANGED: // событие - факт изменения состояния (от объекта - прослушивателя UDP)
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        EvtToFormMain?.Invoke(new object[] { state, itemQueue.Pars[0] });
                        break;
                    case StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED: // получен очередной XML-пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        EvtToFormMain?.Invoke(new object[] { state, itemQueue.Pars[0], itemQueue.Pars[1] });

                        debugMsg = @"получен XML-пакет, добавлен в очередь для обработки";
                        Logging.Logg().Debug(MethodBase.GetCurrentMethod(), debugMsg, Logging.INDEX_MESSAGE.NOT_SET);
                        Debug.WriteLine(string.Format(@"{0}: {1}", DateTime.Now.ToString(), debugMsg));
                        break;
                    case StatesMachine.XML_PACKAGE_VERSION: // версия(строка) шаблон XML-пакета
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = m_fileINI.XMLPackageVersion;
                        break;
                    case StatesMachine.XML_PACKAGE_TEMPLATE: // шаблон XML-пакета
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = m_fileINI.GetXMLPackageTemplate((string)itemQueue.Pars[0]);
                        break;
                    case StatesMachine.NUDP_LISTENER: // номер порта прослушивателя
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        outobj = m_fileINI.NUDPListener;
                        break;
                    case StatesMachine.LIST_DEST: // cписок источников данных (назначение - сохранение полученных значений)
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        EvtToFormMain?.Invoke(new object[] { state, m_fileINI.ListDest });
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
                case StatesMachine.UDP_CONNECTED_CHANGE: // старт/стоп соединения по UDP
                case StatesMachine.WRITER_READY_CHANGE:
                case StatesMachine.UDP_CONNECTED_CHANGED: // факт старт/стоп соединения по UDP
                case StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED: // получен очередной XML-пакет
                case StatesMachine.XML_PACKAGE_VERSION: // версия(строка) шаблон XML-пакета
                case StatesMachine.XML_PACKAGE_TEMPLATE: // шаблон XML-пакета
                case StatesMachine.NUDP_LISTENER: // номер порта прослушивателя
                case StatesMachine.LIST_DEST: // cписок источников данных (назначение - сохранение полученных значений)
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
                case StatesMachine.UDP_CONNECTED_CHANGED:
                case StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED: // получен очередной XML-пакет
                case StatesMachine.LIST_DEST: // cписок источников данных (назначение - сохранение полученных значений)
                    //Ответа не требуется/не требуют обработки результата
                    break;
                case StatesMachine.UDP_CONNECTED_CHANGE:
                case StatesMachine.WRITER_READY_CHANGE:
                case StatesMachine.XML_PACKAGE_VERSION: // версия(строка) шаблон XML-пакета
                case StatesMachine.XML_PACKAGE_TEMPLATE: // шаблон XML-пакета
                case StatesMachine.NUDP_LISTENER: // номер порта прослушивателя                
                    if ((!(itemQueue == null))
                        && (!(itemQueue.m_dataHostRecieved == null)))
                        itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new object[] { state, obj });
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
