using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace uLoader
{
    /// <summary>
    /// Перечисление для типов опроса
    /// </summary>
    public enum MODE_WORK { CUR_INTERVAL // по текущему интервалу
        , COSTUMIZE // выборочно (история)
        , COUNT_MODE_WORK
    }
    /// <summary>
    /// Базовый класс для группы элементов (источников данных, сигналов)
    /// </summary>
    public class ITEM_SRC
    {
        /// <summary>
        /// Наименование группы элементов
        /// </summary>
        public string m_strID;
        /// <summary>
        /// Массив ключей для словаря со значениями параметров
        /// </summary>
        public string[] m_keys;
    }
    /// <summary>
    /// Параметры сигнала в группе сигналов (источник, назначение)
    /// </summary>            
    public class SIGNAL_SRC
    {
        //Ключами для словаря являются 'ITEM_SRC.m_keys'
        public Dictionary<string, string> m_dictPars;

        public SIGNAL_SRC()
        {
            m_dictPars = new Dictionary<string, string>();
        }
    };
    /// <summary>
    /// Параметры группы сигналов (источник, назначение)
    /// </summary>
    public class GROUP_SIGNALS_SRC : ITEM_SRC
    {
        /// <summary>
        /// Список сигналов в группе
        /// </summary>
        public List<SIGNAL_SRC> m_listSgnls;
        /// <summary>
        /// Признак автоматического запуска опроса
        /// </summary>
        public int m_iAutoStart;
        /// <summary>
        /// Признак текущего режима работы
        /// </summary>
        public MODE_WORK m_mode;
        /// <summary>
        /// Массив параметров для всех (== COUNT_MODE_WORK) режимов работы
        /// </summary>
        public DATETIME_WORK[] m_arWorkIntervals;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC()
        {
            //Режим работы по умолчанию - текущий интервал
            m_mode = MODE_WORK.CUR_INTERVAL;
            m_arWorkIntervals = new DATETIME_WORK[(int)MODE_WORK.COUNT_MODE_WORK];
            for (int i = 0; i < (int)MODE_WORK.COUNT_MODE_WORK; i ++)
                m_arWorkIntervals[i] = new DATETIME_WORK();

            m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_dtEnd = DateTime.Now;
            //Выровнять по текущей минуте
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtEnd.AddMilliseconds (-1 * m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_dtEnd.Second * 1000 + m_arWorkIntervals [(int)MODE_WORK.CUR_INTERVAL].m_dtEnd.Millisecond);
        }
    };
    /// <summary>
    /// Параметры интервала опроса
    /// </summary>
    public class DATETIME_WORK
    {
        /// <summary>
        /// Длительность интервала (секунды)
        /// </summary>
        public int m_iInterval;
        /// <summary>
        /// Начало интервала
        /// </summary>
        public DateTime m_dtBegin;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        /// </summary>
        public DateTime m_dtEnd;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DATETIME_WORK ()
        {
            m_iInterval = -1;
            m_dtBegin = new DateTime ();
            m_dtEnd = new DateTime();
        }
        /// <summary>
        /// Установить значения для интервала
        /// </summary>
        /// <param name="iInterval">Длительность (секунды)</param>
        /// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        public int Set (int iInterval)
        {
            int iRes = 0;

            return iRes;
        }
        /// <summary>
        /// Установить значения для интервала
        /// </summary>
        /// <param name="iInterval">Длительность (секунды)</param>
        /// <param name="dtBegin">Начало интервала</param>
        /// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        public int Set(int iInterval, DateTime dtBegin)
        {
            int iRes = 0;

            return iRes;
        }
    }
    /// <summary>
    /// Параметры группы источников информации
    /// </summary>
    public class GROUP_SRC : ITEM_SRC
    {
        /// <summary>
        /// Список объектов с параметрами соединения
        /// </summary>
        public List<ConnectionSettings> m_listConnSett;
        /// <summary>
        /// Наименование библиотеки для работы с группами сигналов
        /// </summary>
        public string m_strDLLName;
        /// <summary>
        /// Массив строк с наименованями "присоединенных" к группе источников групп сигналов
        /// </summary>
        public string []m_arIDGroupSignals;        
        /// <summary>
        /// Конструктор - основной (без парпаметров)
        /// </summary>
        public GROUP_SRC ()
        {
            m_listConnSett = new List<ConnectionSettings> ();
            m_strDLLName = string.Empty;
            m_arIDGroupSignals = new string [] { };
        }
    };
    /// <summary>
    /// Параметры панели (источник, назначение)
    /// </summary>
    public class SRC
    {
        public List<GROUP_SRC> m_listGroupSrc;
        public List<GROUP_SIGNALS_SRC> m_listGroupSgnlsSrc;
    }
    /// <summary>
    /// Группа источников с "прикрепленными" группами сигналов
    /// </summary>
    public class GroupSources : GROUP_SRC
    {
        /// <summary>
        /// Группа сигналов (для получения данных)
        /// </summary>
        private class GroupSignals : GROUP_SIGNALS_SRC
        {
            public GroupSignals(GROUP_SIGNALS_SRC srcItem)
                : base()
            {
                this.m_arWorkIntervals = new DATETIME_WORK[srcItem.m_arWorkIntervals.Length];
                //??? Значения массивов независимы
                srcItem.m_arWorkIntervals.CopyTo(this.m_arWorkIntervals, 0);

                this.m_iAutoStart = srcItem.m_iAutoStart;

                this.m_keys = new string[srcItem.m_keys.Length];
                srcItem.m_keys.CopyTo(this.m_keys, 0);

                //??? Значения списка независимы
                this.m_listSgnls = srcItem.m_listSgnls;

                this.m_mode = srcItem.m_mode;

                this.m_strID = srcItem.m_strID;
            }
        }
        /// <summary>
        /// Вспомогательный домен приложения для загрузки/выгрузки библиотеки
        /// </summary>
        AppDomain m_appDomain;
        List <GroupSignals> m_listGroupSignals;

        public GroupSources (GROUP_SRC srcItem, List <GROUP_SIGNALS_SRC>listGroupSignals) : base ()
        {
            this.m_arIDGroupSignals = new string [srcItem.m_arIDGroupSignals.Length];
            srcItem.m_arIDGroupSignals.CopyTo(this.m_arIDGroupSignals, 0);

            m_listGroupSignals = new List<GroupSignals> ();
            foreach (GROUP_SIGNALS_SRC itemGroupSignals in listGroupSignals)
                m_listGroupSignals.Add(new GroupSignals(itemGroupSignals));

            this.m_keys = new string [srcItem.m_keys.Length];
            srcItem.m_keys.CopyTo(this.m_keys, 0);

            this.m_listConnSett = new List<ConnectionSettings> ();
            foreach (ConnectionSettings connSett in srcItem.m_listConnSett)
                //??? Значения списка независимы
                this.m_listConnSett.Add (connSett);

            this.m_strDLLName = srcItem.m_strDLLName;

            this.m_strID = srcItem.m_strID;
        } 
    }
}