using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HClassLibrary;

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
            , UDP_LISTENER_PACKAGE_RECIEVED //Получен очередной XML-пакет
            , XML_PACKAGE_VERSION //Версия(строка) шаблон XML-пакета
            , XML_PACKAGE_TEMPLATE //Шаблон XML-пакета
            , NUDP_LISTENER //Номер порта прослушивателя
            , LIST_DEST //Список источников данных (назначение - сохранение полученных значений)
        }
        private FormMain.FileINI m_fileINI;

        public HHandlerQueue()
        {
            m_fileINI = new FormMain.FileINI();
        }

        protected override int StateCheckResponse(int s, out bool error, out object outobj)
        {
            int iRes = -1;
            StatesMachine state = (StatesMachine)s;

            error = true;
            outobj = null;

            ItemQueue itemQueue = null;

            try
            {
                switch (state) {
                    case StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED: // получен очередной XML-пакет
                        iRes = 0;
                        error = false;

                        itemQueue = Peek;

                        //outobj = ???;
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

                        //outobj = ???
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
                case StatesMachine.UDP_LISTENER_PACKAGE_RECIEVED: // получен очередной XML-пакет
                    //Ответа не требуется/не требуют обработки результата
                    break;
                case StatesMachine.XML_PACKAGE_VERSION: // версия(строка) шаблон XML-пакета
                case StatesMachine.XML_PACKAGE_TEMPLATE: // шаблон XML-пакета
                case StatesMachine.NUDP_LISTENER: // номер порта прослушивателя
                case StatesMachine.LIST_DEST: // cписок источников данных (назначение - сохранение полученных значений)
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
