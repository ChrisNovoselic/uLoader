using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Threading;

using HClassLibrary;
using uLoaderCommon;

namespace biysktmora
{    
    public class HBiyskTMOra : HHandlerDbULoader
    {
        enum StatesMachine
        {
            CurrentTime
            , Values
        }

        public HBiyskTMOra()
            : base()
        {
        }
        
        public HBiyskTMOra(IPlugIn iPlugIn)
            : base(iPlugIn)
        {
        }

        private class GroupSignalsBiyskTMOra : GroupSignals
        {
            public class SIGNALBiyskTMOra : SIGNAL
            {
                public string m_NameTable;

                public SIGNALBiyskTMOra(int idMain, string nameTable) : base (idMain)
                {
                    this.m_NameTable = nameTable;
                }
            }

            private DataTable m_tableResults;
            public override DataTable TableResults { get { return m_tableResults; } set { m_tableResults = value; } }

            public GroupSignalsBiyskTMOra(object[] pars) : base (pars)
            {
            }

            public override GroupSignals.SIGNAL createSignal(object[] objs)
            {
                return new SIGNALBiyskTMOra((int)objs[0], objs[2] as string);
            }
        }

        protected override int addAllStates()
        {
            int iRes = 0;

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            return iRes;
        }

        private void setQuery(DateTime dtStart, int secInterval = -1)
        {
            m_strQuery = string.Empty;

            if (secInterval < 0)
            {
                secInterval =
                    //SEC_INTERVAL_DEFAULT
                    (int)TimeSpanPeriod.TotalSeconds
                    ;
            }
            else
                ;

            string strUnion = @" UNION "
                //Строки для условия "по дате/времени"
                , strStart = dtStart.ToString(@"yyyyMMdd HHmmss")
                , strEnd = dtStart.AddSeconds(secInterval).ToString(@"yyyyMMdd HHmmss");
            //Формировать зпрос
            foreach (GroupSignalsBiyskTMOra.SIGNALBiyskTMOra s in Signals)
            {
                m_strQuery += @"SELECT " + s.m_idMain + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.m_NameTable + @" WHERE DATETIME BETWEEN"
                    + @" to_timestamp('" + strStart + @"', 'yyyymmdd hh24miss')" + @" AND"
                    + @" to_timestamp('" + strEnd + @"', 'yyyymmdd hh24miss')"
                    + strUnion
                ;
            }

            //Удалить "лишний" UNION
            m_strQuery = m_strQuery.Substring(0, m_strQuery.Length - strUnion.Length);
            ////Установить сортировку
            //m_strQuery += @" ORDER BY DATETIME DESC";
        }

        protected override HHandlerDbULoader.GroupSignals createGroupSignals(object[] objs)
        {
            return new GroupSignalsBiyskTMOra(objs);
        }

        public void ChangeState()
        {
            ClearStates();

            AddState((int)StatesMachine.CurrentTime);
            AddState((int)StatesMachine.Values);

            Run(@"HHandlerDbULoader::ChangeState ()");
        }

        public override void ClearValues()
        {
            int iPrev = 0, iDel = 0, iCur = 0;
            if (!(TableResults == null))
            {
                iPrev = TableResults.Rows.Count;
                string strSel =
                    @"DATETIME<'" + DateTimeStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' OR DATETIME>='" + DateTimeStart.AddSeconds(TimeSpanPeriod.TotalSeconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    //@"DATETIME BETWEEN '" + m_dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"' AND '" + m_dtStart.AddSeconds(m_tmSpanPeriod.Seconds).ToString(@"yyyy/MM/dd HH:mm:ss") + @"'"
                    ;

                DataRow[] rowsDel = null;
                try { rowsDel = TableResults.Select(strSel); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ClearValues () - ...");
                }

                if (!(rowsDel == null))
                {
                    iDel = rowsDel.Length;
                    if (rowsDel.Length > 0)
                    {
                        foreach (DataRow r in rowsDel)
                            TableResults.Rows.Remove(r);
                        //??? Обязательно ли...
                        TableResults.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;

                iCur = TableResults.Rows.Count;

                Console.WriteLine(@"Обновление рез-та [ID=" + m_IdGroupSignalsCurrent + @"]: " + @"(было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @")");
            }
            else
                ;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (! (m_IdGroupSignalsCurrent < 0))
                        GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.Oracle, m_dictIdListeners[m_IdGroupSignalsCurrent][0]);
                    else
                        throw new Exception(@"HBiyskTMOra::StateRequest () - state=" + state.ToString () + @"...");
                    break;
                case (int)StatesMachine.Values:                    
                    try
                    {
                        actualizeDatetimeStart ();
                        ClearValues();
                        setQuery(DateTimeStart);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateRequest () - ::Values - ...");
                    }
                    Request (m_dictIdListeners[m_IdGroupSignalsCurrent][0], m_strQuery);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            DataTable table = obj as DataTable;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    try
                    {
                        m_dtServer = (DateTime)(table as DataTable).Rows[0][0];
                        Console.WriteLine(m_dtServer.ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse () - ::CurrentTime - ...");
                    }
                    break;
                case (int)StatesMachine.Values:
                    try
                    {
                        Console.WriteLine(@"Получено строк [ID=" + m_IdGroupSignalsCurrent + @"]: " + (table as DataTable).Rows.Count);
                        if (TableResults == null)
                        {
                            TableResults = new DataTable();
                        }
                        else
                            ;

                        //int iPrev = -1, iDupl = -1, iAdd = -1, iCur = -1;
                        //iPrev = 0; iDupl = 0; iAdd = 0; iCur = 0;
                        //iPrev = TableResults.Rows.Count;

                        //if (results.Rows.Count == 0)
                        //{
                        //    results = table.Copy ();
                        //}
                        //else
                        //    ;

                        //!!! Перенесена в библ. для "вставки"
                        ////Удалить из таблицы записи, метки времени в которых, совпадают с метками времени в таблице-рез-те предыдущего опроса
                        //iDupl = clearDupValues (ref table);

                        ////!!! Перенесена в библ. для "вставки"
                        ////Сформировать таблицу с "новыми" данными
                        //DataTable tableIns = getTableIns(ref table);
                        //tableIns.Columns.Add(@"tmdelta", typeof(int));

                        //iAdd = table.Rows.Count;
                        //TableResults.Merge(table);
                        //iCur = TableResults.Rows.Count;
                        //Console.WriteLine(@"Объединение таблицы-рез-та: [было=" + iPrev + @", дублирущих= " + iDupl + @", добавлено=" + iAdd + @", стало=" + iCur + @"]");

                        TableResults = table.Copy ();
                    }
                    catch (Exception e)
                    {
                        Logging.Logg ().Exception (e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::StateResponse () - ::Values - ...");
                    }
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, int request, int result)
        {
            string unknownErr = @"Неизвестная ошибка"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получения даты/времени сервера-источника";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"получения значений источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals (unknownErr) == false)
                msgErr = @"Ошибка " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);

            (_iPlugin as PlugInBase).DataAskedHost(new object[] { (int)ID_DATA_ASKED_HOST.ERROR, 0, state, msgErr });
        }

        protected override void StateWarnings(int state, int request, int result)
        {
            string unknownErr = @"Неизвестное предупреждение"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получении даты/времени сервера-источника";
                    break;
                case (int)StatesMachine.Values: //Ошибка получения значений источника
                    msgErr = @"получении значений источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals(unknownErr) == false)
                msgErr = @"Предупреждение при " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);
        }
    }

    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1001;

            createObject(typeof(HBiyskTMOra));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
