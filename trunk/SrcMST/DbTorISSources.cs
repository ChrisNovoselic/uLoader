using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using HClassLibrary;

namespace SrcMSTKKSNAMEtoris
{
    /// <summary>
    /// Класс для описания объекта управления установленными соединенями
    /// </summary>
    public class DbTorISSources : DbSources
    {
        /// <summary>
        /// Уникальный идентификатор Модес-Центра
        /// </summary>
        private const int TORIS_ID = -40815;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        protected DbTorISSources()
            : base()
        {
        }
        /// <summary>
        /// Функция доступа к объекту управления установленными соединенями
        /// </summary>
        /// <returns>Объект управления установленными соединенями</returns>
        public static new DbTorISSources Sources()
        {
            if (m_this == null)
                m_this = new DbTorISSources();
            else
                ;

            return (DbTorISSources)m_this;
        }
        /// <summary>
        /// Регистриует клиента соединения, активным или нет, при необходимости принудительно отдельный экземпляр
        /// </summary>
        /// <param name="connSett">параметры соединения</param>
        /// <param name="active">признак активности</param>
        /// <param name="bReq">признак принудительного создания отдельного экземпляра</param>
        /// <returns></returns>
        public override int Register(object connSett, bool active, string desc, bool bReq = false)
        {
            int id = -1,
                err = -1;

            if (connSett is ConnectionSettings == true)
                return base.Register (connSett, active, desc, bReq);
            else
                if (connSett is string == true) {
                    if (m_dictDbInterfaces.ContainsKey (TORIS_ID) == true) id = m_dictDbInterfaces[TORIS_ID].ListenerRegister(); else ;
                }
                else
                    ;

            if (id < 0)
            {
                string dbNameType = string.Empty;
                DbTSQLInterface.DB_TSQL_INTERFACE_TYPE dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.ModesCentre;
                dbNameType = dbType.ToString();

                m_dictDbInterfaces.Add(TORIS_ID, new DbTorISSources((string)connSett));
                
                if (active == true) m_dictDbInterfaces[TORIS_ID].Start(); else ;
                m_dictDbInterfaces[TORIS_ID].SetConnectionSettings(connSett, active);

                id = m_dictDbInterfaces[TORIS_ID].ListenerRegister();
            }
            else
                ;

            return registerListener(TORIS_ID, id, active, out err);
        }
    }
}