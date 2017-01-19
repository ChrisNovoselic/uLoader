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

            public int NUDPListener
            {
                get { return Int32.Parse(GetSecValueOfKey(@"Reader", @"NUDP")); }
            }

            public string XMLPackageVersion
            {
                get { return GetSecValueOfKey(@"Reader", @"XML_TEMPLATE_VERSION"); }
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
