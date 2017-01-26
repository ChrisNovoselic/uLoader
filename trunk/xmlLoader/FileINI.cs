using HClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

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

            private List<string> getSecValuesOfKey(string secName, string key)
            {
                return GetSecValueOfKey(secName, key).Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            public int NUDPListener
            {
                get { return Int32.Parse(GetSecValueOfKey(@"Reader", @"NUDP")); }
            }

            public string XMLPackageVersion
            {
                get { return GetSecValueOfKey(@"Reader", @"XML_TEMPLATE_VERSION"); }
            }

            public enum INDEX_CONNECTION_SETTING { ID, NAME_SHR, IP, NPORT, INSTANCE, DB_NAME, UID, PSWD }

            public List<ConnectionSettings> ListDest
            {
                get {
                    List <ConnectionSettings> listRes = new List<ConnectionSettings>();

                    //ConnectionSettings connSett;
                    int i = -1; // индекс источника данных
                    string secName = @"Writer";
                    List<string> keys
                        , values;

                    keys = getSecValuesOfKey(secName, @"S-PARS");

                    i = 0;
                    while (true) {
                        values = getSecValuesOfKey(secName, string.Format(@"S{0}", i));

                        if (!(values.Count < keys.Count))
                            listRes.Add(new ConnectionSettings(
                                Int32.Parse(values[keys.IndexOf(@"ID")])
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

                nameXMLTemplate = GetSecValueOfKey(@"Reader", @"XML_TEMPLATE_NAME");
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
