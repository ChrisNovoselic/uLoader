using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace uLoader
{
    public partial class FormMain
    {
        private class FileINI : HClassLibrary.FileINI
        {            
            private static char s_chDelimeter = '-';
            /// <summary>
            /// Параметры сигнала в группе сигналов (источник)
            /// </summary>
            private class SIGNAL_SOURCE
            {
                public int id;
                public string id_name;

                public SIGNAL_SOURCE ()
                {
                    id = 0;
                    id_name = string.Empty;
                }
            };
            /// <summary>
            /// Параметры сигнала в группе сигналов (назначение)
            /// </summary>
            private class SIGNAL_DEST : SIGNAL_SOURCE
            {
                public int id_src;
            };
            /// <summary>
            /// Параметры группы сигналов (источник)
            /// </summary>
            private class GROUP_SIGNALS_SOURCE
            {
                public List<SIGNAL_SOURCE> listSgnls;
            };
            /// <summary>
            /// Параметры группы сигналов (назначение)
            /// </summary>
            private class GROUP_SIGNALS_DEST
            {
                public List<SIGNAL_DEST> listSgnls;
            };
            /// <summary>
            /// Параметры группы источников информации
            /// </summary>
            private class GROUP_SRC
            {
                public string Name;
                public List<ConnectionSettings> m_listConnSett;
            };
            /// <summary>
            /// Параметры группы источников информации (источник)
            /// </summary>
            private class GROUP_SRC_SOURCE : GROUP_SRC
            {
                public List <GROUP_SIGNALS_SOURCE> m_listGroupSgnls;
            };
            /// <summary>
            /// Параметры группы источников информации (назначение)
            /// </summary>
            private class GROUP_SRC_DEST : GROUP_SRC
            {
                public List<GROUP_SIGNALS_DEST> m_listGroupSgnls;
            };
            
            enum INDEX_KEY_SRC { GROUP_SRC, SRC_OF_GROUP, SRC
                , COUNT_INDEX_KEY_SRC
            };
            enum INDEX_KEY_SIGNAL { GROUP_SRC, GROUP_SIGNALS, SIGNAL_OF_GROUP
                , COUNT_INDEX_KEY_SIGNAL
            };

            private static string [] SEC_KEY_SOURCE = {@"GSrc", @"GS", @"S"}
                , SEC_KEY_DEST = {@"GDst", @"GD", @"D"}
                , SEC_KEY_SOURCE_SIGNALS = { @"GSrc", @"GSgnls", @"Sgnl" }
                , SEC_KEY_DEST_SIGNALS = { @"GDst", @"GSgnls", @"Sgnl" };

            private List<GROUP_SRC_SOURCE> m_listGroupSrc;
            private List<GROUP_SRC_DEST> m_listGroupDest;

            public FileINI ()
                : base (@"setup.ini", true)
            {
                m_listGroupSrc = new List<GROUP_SRC_SOURCE> ();
                m_listGroupDest = new List<GROUP_SRC_DEST> ();

                int i =-1;
                //Получить наименование секции для группы источников (источник)
                string sec = getSrcSec(INDEX_KEY_SRC.GROUP_SRC)
                    , key = string.Empty;
                //Получить словарь параметров для группы источников (источник)
                Dictionary<string, string> dictValues = getSecValues(sec);

                //Заполнить группу источников (источник)
                if (! (dictValues == null))
                {
                    i = 0;
                    //Получить все источники данных
                    while (true)
                    {
                        //Получить ключ из пары (ключ-значение)
                        key = SEC_KEY_SOURCE[(int)INDEX_KEY_SRC.SRC_OF_GROUP] + i.ToString();

                        if (dictValues.ContainsKey(key) == true)
                        {
                            //Добавить группу источников
                            m_listGroupSrc.Add (new GROUP_SRC_SOURCE ());
                            m_listGroupSrc[i].Name = dictValues[key];

                            //Получить наименование для секции с источниками из группы источников
                            sec = getSrcSec(INDEX_KEY_SRC.SRC_OF_GROUP) + i.ToString ();

                            if (isSec (sec) == true)
                            {
                                int j = -1;
                                //Получить словарь параметров соединения
                                Dictionary<string, string> dictSrcValues = getSecValues(sec);

                                if (! (dictValues == null))
                                {
                                    j = 0;
                                    while (true)
                                    {
                                        break;
                                    }
                                }
                                else
                                    //Секция есть, но в ней не определен ни один источник...
                                    ; //???
                            }
                            else
                                throw new Exception (@"FileINI::ctor () - источники объявлены, но не определены [" + sec + @"] ...");
                        }
                        else
                            break;

                        i ++;
                    }

                    i = 0;
                    //Получить все группы сигналов
                    while (true)
                    {
                        //Получить ключ из пары (ключ-значение)
                        key = SEC_KEY_SOURCE_SIGNALS[(int)INDEX_KEY_SIGNAL.GROUP_SIGNALS] + i.ToString();

                        break;
                    }
                }
                else
                    ;

                //Получить наименование секции для группы источников (назначение)
                sec = getDestSec(INDEX_KEY_SRC.GROUP_SRC);
                //Получить словарь параметров для группы источников (назначение)
                dictValues = getSecValues(sec);
                //Заполнить группу источников (назначение)
                if (!(dictValues == null))
                {
                    //Получить все источники данных

                    //Получить все группы сигналов
                }
                else
                    ;
            }

            private string getSrcSec (INDEX_KEY_SRC key)
            {
                string strRes = string.Empty;

                for (INDEX_KEY_SRC i = INDEX_KEY_SRC.GROUP_SRC; i < INDEX_KEY_SRC.COUNT_INDEX_KEY_SRC; i ++)
                {
                    if (!(i > key))
                    {
                        if (strRes.Equals(string.Empty) == false)
                            strRes += s_chDelimeter;
                        else
                            ;

                        strRes += SEC_KEY_SOURCE [(int)i];
                    }
                    else
                        break;
                }

                return strRes;
            }

            private string getSrcSec(INDEX_KEY_SIGNAL key)
            {
                string strRes = string.Empty;

                for (INDEX_KEY_SIGNAL i = INDEX_KEY_SIGNAL.GROUP_SRC; i < INDEX_KEY_SIGNAL.COUNT_INDEX_KEY_SIGNAL; i++)
                {
                    if (! (i > key))
                    {
                        if (strRes.Equals(string.Empty) == false)
                            strRes += s_chDelimeter;
                        else
                            ;

                        strRes += SEC_KEY_SOURCE_SIGNALS[(int)i];
                    }
                    else
                        break;
                }

                return strRes;
            }

            private string getDestSec(INDEX_KEY_SRC key)
            {
                string strRes = string.Empty;

                for (INDEX_KEY_SRC i = INDEX_KEY_SRC.GROUP_SRC; i < INDEX_KEY_SRC.COUNT_INDEX_KEY_SRC; i ++)
                {
                    if (!(i > key))
                    {
                        if (strRes.Equals(string.Empty) == false)
                            strRes += s_chDelimeter;
                        else
                            ;

                        strRes += SEC_KEY_DEST [(int)i];
                    }
                    else
                        break;
                }

                return strRes;
            }

            private string getDestSec(INDEX_KEY_SIGNAL key)
            {
                string strRes = string.Empty;

                for (INDEX_KEY_SIGNAL i = INDEX_KEY_SIGNAL.GROUP_SRC; i < INDEX_KEY_SIGNAL.COUNT_INDEX_KEY_SIGNAL; i++)
                {
                    if (! (i > key))
                    {
                        if (strRes.Equals(string.Empty) == false)
                            strRes += s_chDelimeter;
                        else
                            ;

                        strRes += SEC_KEY_DEST_SIGNALS [(int)i];
                    }
                    else
                        break;
                }

                return strRes;
            }

            private int load ()
            {
                int iRes = 0;

                return iRes;
            }

            private int save()
            {
                int iRes = 0;

                return iRes;
            }
        }
    }
}
