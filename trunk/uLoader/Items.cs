﻿using System;
using System.Collections.Generic;

using System.Reflection; //Assembly
using System.IO;
using System.Data;

using System.Threading;
using System.ComponentModel;

////using HClassLibrary;
using uLoaderCommon;
using ASUTP.Database;
using ASUTP.Helper;

namespace uLoader
{
    interface IItemSrc
    {
        void Initialize(string []values);
    }
    /// <summary>
    /// Базовый класс для группы элементов (источников данных, сигналов)
    /// </summary>
    public class ITEM_SRC
    {
        /// <summary>
        /// Строковый идентификатор группы элементов
        /// </summary>
        public string m_strID;
        /// <summary>
        /// Наименование группы элементов
        /// </summary>
        public string m_strShrName;
        /// <summary>
        /// Массив ключей для словаря со значениями параметров соединения (сигналов)
        /// </summary>
        public List<string> m_listSKeys;
        /// <summary>
        /// Массив ключей для словаря со значениями параметров групп сигналов
        /// </summary>
        public string[] m_GSgnlsKeys;
    }
    /// <summary>
    /// Параметры сигнала в группе сигналов (источник, назначение)
    /// </summary>            
    public class SIGNAL_SRC
    {
        //Ключами для словаря являются 'ITEM_SRC.m_SKeys'
        public string[] m_arSPars;

        public SIGNAL_SRC()
        {
            m_arSPars = new string[] {};
        }
    };
    /// <summary>
    /// Класс для параметров группы сигналов
    /// </summary>
    public class GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Строковый идентификатор группы сигналов
        /// </summary>
        public string m_strId;
        /// <summary>
        /// Наименование краткое группы сигналов
        /// </summary>
        public string m_strShrName;
        /// <summary>
        /// Признак возможности включения режима "выборочно"
        /// </summary>
        public bool m_bToolsEnabled;
        /// <summary>
        /// Признак автоматического запуска опроса
        /// </summary>
        public int m_iAutoStart;
        /// <summary>
        /// Массив параметров для всех (== COUNT_MODE_WORK) режимов работы
        /// </summary>
        public DATETIME_WORK[] m_arWorkIntervals;
        /// <summary>
        /// Наименование таблицы источника
        /// </summary>
        public string m_TableName;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_PARS ()
        {
            m_iAutoStart = -1;
            m_bToolsEnabled = false;
            m_arWorkIntervals = new DATETIME_WORK[(int)MODE_WORK.COUNT_MODE_WORK];
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE] = new DATETIME_WORK();

            //Дата/время начала опроса (режим: выборочно)
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart = DateTime.Now;
            // округлить по прошедшему часу
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.AddHours(-1);
            // округлить по 0-ой минуте
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.AddMinutes(-1 * m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.Minute);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.AddMilliseconds(-1 * m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.Second * 1000 + m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart.Millisecond);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain = HTimeSpan.FromHours(1);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal = HTimeSpan.FromMinutes(1);
            m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsIntervalLocal = HTimeSpan.FromMilliseconds((int)DATETIME.MSEC_INTERVAL_DEFAULT);
        }
    }
    /// <summary>
    /// Класс для параметров группы сигналов (источник)
    /// </summary>
    public class GROUP_SIGNALS_SRC_PARS : GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Признак текущего режима работы
        /// </summary>
        public MODE_WORK m_mode;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC_PARS ()
        {
            //Режим работы по умолчанию - текущий интервал
            m_mode = MODE_WORK.CUR_INTERVAL;

            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL] = new DATETIME_WORK();
            //Дата/время начала опроса (режим: тек./дата/время)
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart = DateTime.Now;
            // округлить по текущей минуте
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.AddMilliseconds(-1 * m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Second * 1000 + m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_dtStart.Millisecond);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain =
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodLocal =
                HTimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsIntervalLocal = HTimeSpan.FromMilliseconds((int)DATETIME.MSEC_INTERVAL_DEFAULT);
        }
    }
    /// <summary>
    /// Класс для параметров группы сигналов (назначение)
    /// </summary>
    public class GROUP_SIGNALS_DEST_PARS : GROUP_SIGNALS_PARS
    {
        /// <summary>
        /// Строковый идентификатор группы источников
        ///  к которой прикреплена группа сигналов-источник
        ///  для текущей группы сигналов
        /// </summary>
        public string m_idGrpSrcs;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_DEST_PARS () : base ()
        {
        }
    }
    /// <summary>
    /// Параметры группы сигналов (источник, назначение)
    /// </summary>
    public class GROUP_SIGNALS_SRC : ITEM_SRC
    {
        /// <summary>
        /// Список сигналов в группе
        /// </summary>
        public List<SIGNAL_SRC> m_listSgnls;

        public Dictionary<string, string> m_dictFormula;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public GROUP_SIGNALS_SRC()
        {
        }
        /// <summary>
        /// Добавить сигнал в группу
        /// </summary>
        /// <param name="values">Значения параметров сигнала</param>
        public void Add(string []values)
        {
            //Инициализация, если элемент группы 1-ый
            if (m_listSgnls == null)
                m_listSgnls = new List<SIGNAL_SRC>();
            else
                ;
            // выделить память для сигнала
            m_listSgnls.Add(new SIGNAL_SRC());
            // выделить память для параметров сигнала
            m_listSgnls[m_listSgnls.Count - 1].m_arSPars = new string[values.Length];
            // ??? уточнить не является ли один из параметров формула
            values.CopyTo(m_listSgnls[m_listSgnls.Count - 1].m_arSPars, 0);
        }
        /// <summary>
        /// Добавить описание формулы, используемой в группе
        /// </summary>
        /// <param name="fKey">Ключ формулы</param>
        /// <param name="formula">Содержание формулы</param>
        public void AddFormula(string fKey, string formula)
        {
            if (m_dictFormula == null)
                m_dictFormula = new Dictionary<string, string>();
            else
                ;
            // проверить не была ли формула уже добавлена
            if (m_dictFormula.ContainsKey(fKey) == false)
                m_dictFormula.Add(fKey, formula);
            else
                ;
        }

        public int ReinitArgs(int iSgnl, int iPar)
        {
            int iRes = 0; // признак ошибки (0 - нет ошибок)
            
            // найти в параметре 'item.m_listSgnls[i].m_arSPars[j]' скобки ('(', ')')
            int iStartArgs = m_listSgnls[iSgnl].m_arSPars[iPar].IndexOf('(') + 1;
            if (iStartArgs > 0)
                ;
            else
                ;

            return iRes;
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
        public HTimeSpan m_tsIntervalLocal;
        /// <summary>
        /// Начало интервала
        /// </summary>
        public DateTime m_dtStart;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        /// </summary>
        public HTimeSpan m_tsPeriodMain;
        /// <summary>
        /// Окончание интервала (зависит от начала и длительности)
        ///  для "текущ./интервала" равен 'm_tsPeriodMain'
        /// </summary>
        public HTimeSpan m_tsPeriodLocal;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DATETIME_WORK ()
        {
            m_dtStart = new DateTime ();
            m_tsPeriodMain =
            m_tsPeriodLocal =
                HTimeSpan.FromSeconds((int)DATETIME.SEC_SPANPERIOD_DEFAULT);
            m_tsIntervalLocal = HTimeSpan.NotValue;
        }
        ///// <summary>
        ///// Установить значения для интервала
        ///// </summary>
        ///// <param name="iInterval">Длительность (секунды)</param>
        ///// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        //public int Set (int iInterval)
        //{
        //    int iRes = 0;

        //    return iRes;
        //}
        ///// <summary>
        ///// Установить значения для интервала
        ///// </summary>
        ///// <param name="iInterval">Длительность (секунды)</param>
        ///// <param name="dtBegin">Начало интервала</param>
        ///// <returns>Признак успешного выполнения функции (0 - успех, иначе - ошибка)</returns>
        //public int Set(int iInterval, DateTime dtBegin)
        //{
        //    int iRes = 0;

        //    return iRes;
        //}
    }
    /// <summary>
    /// Параметры группы источников информации
    /// </summary>
    public class GROUP_SRC : ITEM_SRC
    {
        /// <summary>
        /// Индекс текущего источника данных
        ///  цифровое обозначение (окончание) строкового идентифиатора
        /// </summary>
        public string m_IDCurrentConnSett;
        /// <summary>
        /// Список объектов с параметрами соединения
        /// </summary>
        public Dictionary<string, ConnectionSettings> m_dictConnSett;
        /// <summary>
        /// Наименование библиотеки для работы с группами сигналов
        /// </summary>
        public string m_strDLLName;

        protected int _iIdTypePlugInObjectLoaded;
        /// <summary>
        /// Идентификатор типа объекта плюгИна, используемого для обращения к данным
        /// </summary>
        public int m_iIdTypePlugInObjectLoaded { get { return _iIdTypePlugInObjectLoaded; } } // _plugIn.KeySingleton
        /// <summary>
        /// Список с параметрами "присоединенных" к группе источников групп сигналов
        /// </summary>
        public List <GROUP_SIGNALS_PARS> m_listGroupSignalsPars;
        /// <summary>
        /// Словарь для "дополнительных" параметров
        /// </summary>
        public Dictionary<string, string> m_dictAdding;
        /// <summary>
        /// Установить значения дополнительных параметров для группы источников
        /// </summary>
        /// <param name="vals">Дополнительные параметры</param>
        public void SetAdding(string []vals)
        {
            if (m_dictAdding == null)
                m_dictAdding = new Dictionary<string, string>();
            else
                m_dictAdding.Clear();

            if ((vals.Length > 0)
                && (vals[0].Equals(string.Empty) == false))
                foreach (string pair in vals)
                    m_dictAdding.Add(pair.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[0], pair.Split(FileINI.s_chSecDelimeters[(int)FileINI.INDEX_DELIMETER.VALUE])[1]);
            else
                //throw new Exception (@"FileINI::addGroupValues () - ADDING - некорректные разделители...")
                ;
        }

        private static Type getTypeGroupSignals(List<string> pars)
        {
            Type typeRes = Type.Missing as Type;
            if ((!(pars.IndexOf(@"CURINTERVAL_PERIODMAIN") < 0))
                && (!(pars.IndexOf(@"CURINTERVAL_PERIODLOCAL") < 0)))
                typeRes = typeof(GROUP_SIGNALS_SRC_PARS);
            else
                if (!(pars.IndexOf(@"ID_GS") < 0))
                    typeRes = typeof(GROUP_SIGNALS_DEST_PARS);
                else
                    ;

            return typeRes;
        }

        public int SetGroupSignalsPars (List <string> pars, string []vals)
        {
            int iRes = -1;
            //Объект с параметрами группы сигналов
            GROUP_SIGNALS_PARS item;
            //Строковый идентификатор группы сигналов
            string strId = vals[pars.IndexOf(@"ID")];
            //Тип группы сигналов (для источника, для назаначения)
            Type typeGrpSgnls = Type.Missing as Type;

            iRes = getIndexGroupSignalsPars (strId);

            if (iRes < 0)
            {
                typeGrpSgnls = getTypeGroupSignals(pars);
                item = Activator.CreateInstance(typeGrpSgnls) as GROUP_SIGNALS_PARS; //new GROUP_SIGNALS_PARS ();
                m_listGroupSignalsPars.Add(item);
                iRes = m_listGroupSignalsPars.Count - 1;
            }
            else
                item = m_listGroupSignalsPars[iRes];

            item.m_strId = strId; //ID

            item.m_iAutoStart = Int32.Parse(vals[pars.IndexOf(@"AUTO_START")]); //AUTO_START
            item.m_bToolsEnabled = bool.Parse(vals[pars.IndexOf(@"TOOLS_ENABLED")]); //TOOLS_ENABLED
            if(pars.IndexOf(@"TABLE")>0)
                item.m_TableName = vals[pars.IndexOf(@"TABLE")]; //AUTO_START

            if (item is GROUP_SIGNALS_SRC_PARS)
            {
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodMain =
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsPeriodLocal = new HTimeSpan(vals[pars.IndexOf(@"CURINTERVAL_PERIODMAIN")]); //CURINTERVAL_PERIODMAIN
                item.m_arWorkIntervals[(int)MODE_WORK.CUR_INTERVAL].m_tsIntervalLocal = new HTimeSpan(vals[pars.IndexOf(@"CURINTERVAL_PERIODLOCAL")]); //CURINTERVAL_PERIODLOCAL
            }
            else
                if (item is GROUP_SIGNALS_DEST_PARS)
                    (item as GROUP_SIGNALS_DEST_PARS).m_idGrpSrcs = vals[pars.IndexOf(@"ID_GS")];
                else
                    ;

            return iRes;
        }
        /// <summary>
        /// Установить новые значения параметров для СУЩЕСТВУЮЩей группы сигналов
        ///  в контексте списка объектов выполнения, возвращает (внутренний) индекс группы сигналов в списке
        /// </summary>
        /// <param name="idGroupSgnls">Идентификатор (строковый) группы сигналов</param>
        /// <param name="pars">Параметры группы сигналов для установки</param>
        /// <returns>Индекс (внутренний) группы сигналов в списке</returns>
        public int SetGroupSignalsPars (/*string idGroupSgnls,*/ GROUP_SIGNALS_PARS pars)
        {
            int iRes = getIndexGroupSignalsPars (pars.m_strId/*idGroupSgnls*/);

            if (pars is GROUP_SIGNALS_SRC_PARS)
            {
                MODE_WORK mode =
                (m_listGroupSignalsPars[iRes] as GROUP_SIGNALS_SRC_PARS).m_mode =
                    (pars as GROUP_SIGNALS_SRC_PARS).m_mode;

                m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_dtStart =
                    pars.m_arWorkIntervals[(int)mode].m_dtStart;
                m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_tsPeriodMain =
                    pars.m_arWorkIntervals[(int)mode].m_tsPeriodMain;
                m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_tsPeriodLocal =
                    pars.m_arWorkIntervals[(int)mode].m_tsPeriodLocal;

                //if (mode == MODE_WORK.CUR_INTERVAL)
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)mode].m_tsIntervalLocal =
                        pars.m_arWorkIntervals[(int)mode].m_tsIntervalLocal;
                //else ;
            }
            else
                if (pars is GROUP_SIGNALS_DEST_PARS)
                {
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart =
                        pars.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_dtStart;
                    m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain =                    
                        pars.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodMain;
                    //m_listGroupSignalsPars[iRes].m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal =
                    //    pars.m_arWorkIntervals[(int)MODE_WORK.COSTUMIZE].m_tsPeriodLocal;
                }
                else
                    ;

            return iRes;
        }

        public GROUP_SIGNALS_SRC_PARS GetGroupSignalsPars (string id)
        {
            return m_listGroupSignalsPars[getIndexGroupSignalsPars(id)] as GROUP_SIGNALS_SRC_PARS;
        }
        /// <summary>
        /// Возвращает (внутренний) индекс группы сигналов в списке
        /// </summary>
        /// <param name="idGroupSgnls">Идентификатор (строковый) группы сигналов</param>
        /// <returns>Индекс (внутренний) группы сигналов в списке</returns>
        private int getIndexGroupSignalsPars (string idGroupSgnls)
        {
            int iRes = -1;

            foreach (GROUP_SIGNALS_PARS par in m_listGroupSignalsPars)
                if (par.m_strId.Equals(idGroupSgnls) == true)
                {
                    iRes = m_listGroupSignalsPars.IndexOf (par);

                    break;
                }
                else
                    ;

            return iRes;
        }
                
        /// <summary>
        /// Конструктор - основной (без парпаметров)
        /// </summary>
        public GROUP_SRC ()
        {
            m_IDCurrentConnSett = string.Empty;
            m_dictConnSett = new Dictionary<string, ConnectionSettings>();
            m_strDLLName = string.Empty;
            m_listGroupSignalsPars = new List <GROUP_SIGNALS_PARS> ();
        }
        /// <summary>
        /// Добавить в группу источник
        /// </summary>
        /// <param name="key">Базовая строка для формирования ключа добавляемого источника</param>
        /// <param name="values">Значения параметров источника (параметры соединения с БД)</param>
        public void Add(string key, string [] values)
        {
            //Инициализация, если элемент группы 1-ый
            if (m_dictConnSett == null)
                m_dictConnSett = new Dictionary<string, ConnectionSettings>();
            else
                ;

            m_dictConnSett.Add(key + m_dictConnSett.Count
                , new ConnectionSettings(
                    Int32.Parse(values[m_listSKeys.IndexOf(@"ID")])
                    , values[m_listSKeys.IndexOf(@"NAME_SHR")]
                    , values[m_listSKeys.IndexOf(@"IP")]
                    , string.Empty // Instanse
                    , Int32.Parse(values[m_listSKeys.IndexOf(@"PORT")])
                    , values[m_listSKeys.IndexOf(@"DB_NAME")]
                    , values[m_listSKeys.IndexOf(@"UID")]
                    , values[m_listSKeys.IndexOf(@"PSWD*")]
                ));
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
    
    //public class GroupSourcesSrc : GroupSources
    //{
    //    /// <summary>
    //    /// Передать в очередь обработки событий сообщение о необходимости установления/разрыва связи между группами источников
    //    /// </summary>
    //    /// <param name="ev">Аргумент при передаче сообщения</param>
    //    public override void PerformDataAskedHostQueue(EventArgsDataHost ev)
    //    {
    //        //id_main - идентификатор типа объекта в загруженной в ОЗУ библиотеки
    //        //id_detail - команда на изменение состояния группы сигналов
    //        //В 0-ом параметре передан индекс (???идентификатор) группы сигналов
    //        int indxGrpSgnls = (int)(ev.par as object[])[0];
    //        //В 1-ом параметре передан признак инициирования/подтверждения изменения состояния группы сигналов
    //        ID_HEAD_ASKED_HOST idHeadAskedHost = (ID_HEAD_ASKED_HOST)(ev.par as object[])[1];

    //        base.PerformDataAskedHostQueue(new EventArgsDataHost(ev.id_main, ev.id_detail, new object[] { this, indxGrpSgnls, idHeadAskedHost }));
    //    }
    //}
}