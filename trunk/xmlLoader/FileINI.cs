using HClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using uLoaderCommon;

namespace xmlLoader
{
    public partial class FormMain
    {
        public class FileINI : HClassLibrary.FileINI
        {
            public FileINI(string strNameFileINI)
                : base(strNameFileINI.Equals(string.Empty) == true ? @"Config\setup.ini" : strNameFileINI
                      , true
                      , MODE_SECTION_APPLICATION.INSTANCE)
            {
                Logging.Logg().Debug(MethodBase.GetCurrentMethod(), @"успех", Logging.INDEX_MESSAGE.NOT_SET);
            }

            private List<string> getSecListValuesOfKey(string secName, string key)
            {
                return GetSecValueOfKey(secName, key).Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            private List<string> getSecListValuesOfKey(string secName, string key, Char[] separator)
            {
                return GetSecValueOfKey(secName, key).Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            private List<string> getSecListValuesOfKey(string secName, string key, string separator)
            {
                return GetSecValueOfKey(secName, key).Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            public ushort TimerUpdate { get { return (ushort)new HTimeSpan(GetMainValueOfKey(@"TIMER_UPDATE")).Value.TotalSeconds; } }

            public PackageHandlerQueue.OPTION OptionPackage
            {
                get {
                    Dictionary<string, string> dictOption = GetSecValuesOfKey(@"Reader", @"OPTION");

                    return new PackageHandlerQueue.OPTION() {
                        COUNT_VIEW_ITEM = Int32.Parse(dictOption[@"COUNT_VIEW_ITEM"])
                        , TS_TIMER_TABLERES = new HTimeSpan(dictOption[@"TIMER_TABLERES"]).Value
                        , TS_HISTORY_RUNTIME = new HTimeSpan(dictOption[@"HISTORY_RUNTIME"]).Value
                        , HISTORY_ISSUE = bool.Parse(dictOption[@"HISTORY_ISSUE"])
                        , TS_HISTORY_ALONG = new HTimeSpan(dictOption[@"HISTORY_ALONG"]).Value
                    };
                }
            }

            public WriterHandlerQueue.OPTION OptionDataSet
            {
                get {
                    Dictionary<string, string> dictOption = GetSecValuesOfKey(@"Writer", @"OPTION");

                    return new WriterHandlerQueue.OPTION() {
                        COUNT_VIEW_ITEM = Int32.Parse(dictOption[@"COUNT_VIEW_ITEM"])
                        , TS_HISTORY_RUNTIME = new HTimeSpan(dictOption[@"HISTORY_RUNTIME"]).Value
                    };
                }
            }

            public object [] UDPListener
            {
                get {
                    return new object[] { getSecListValuesOfKey(@"Reader", @"SERVER", @"::")[0]
                        , Int32.Parse(getSecListValuesOfKey(@"Reader", @"SERVER", @"::")[1])
                    };
                }
            }

            public object[] UDPDebug
            {
                get {
                    Dictionary<string, string> dictDebug = GetMainValuesOfKey(@"UDP_DEBUG");

                    return new object[] { dictDebug[@"TURN"]
                        , dictDebug[@"INTERVAL_SERIES"]
                    };
                }
            }

            private Dictionary <string, string> xmlTemplate
            {
                get {
                    return GetSecValuesOfKey(@"Reader", @"XML_TEMPLATE");
                }
            }

            public string XMLPackageVersion
            {
                get {
                    return xmlTemplate[@"VERSION"];
                }
            }

            public List<WriterHandlerQueue.ConnectionSettings> ListDest
            {
                get {
                    List <WriterHandlerQueue.ConnectionSettings> listRes = new List<WriterHandlerQueue.ConnectionSettings>();

                    //ConnectionSettings connSett;
                    int i = -1; // индекс источника данных
                    string secName = @"Writer";
                    List<string> keys
                        , values;

                    keys = getSecListValuesOfKey(secName, @"S-PARS");

                    i = 0;
                    while (true) {
                        values = getSecListValuesOfKey(secName, string.Format(@"S{0}", i));

                        //??? сверить ключи keys И dictValues.Keys

                        if (!(values.Count < keys.Count))
                            listRes.Add(new WriterHandlerQueue.ConnectionSettings(
                                bool.Parse(values[keys.IndexOf(@"AUTO_START")])
                                , Int32.Parse(values[keys.IndexOf(@"ID")])
                                , values[keys.IndexOf(@"NAME_SHR")]
                                , values[keys.IndexOf(@"IP")]
                                , string.Empty // Instanse
                                , Int32.Parse(values[keys.IndexOf(@"PORT")])
                                , values[keys.IndexOf(@"DB_NAME")]
                                , values[keys.IndexOf(@"UID")]
                                , values[keys.IndexOf(@"PSWD*")]
                            ));
                        else
                            break;

                        i++;
                    }

                    return listRes;
                }
            }

            public XmlDocument GetXMLPackageTemplate(string ver)
            {
                XmlDocument docRes = null;

                string nameXMLTemplate = string.Empty;

                nameXMLTemplate = xmlTemplate[@"NAME"];
                nameXMLTemplate = nameXMLTemplate.Replace(@"?VERSION?", ver);
                nameXMLTemplate = string.Format(@"{0}\{1}"
                    , Path.GetDirectoryName (m_NameFileINI)
                    , nameXMLTemplate);

                try {
                    if (File.Exists(nameXMLTemplate) == true) {
                        docRes = new XmlDocument();
                        docRes.Load(nameXMLTemplate);
                    } else
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"Загрузка шаблона XML-документа, полный_путь={0}", nameXMLTemplate), Logging.INDEX_MESSAGE.NOT_SET);
                }

                return docRes;
            }
        }
    }
}
