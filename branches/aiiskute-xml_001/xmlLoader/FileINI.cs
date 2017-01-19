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
            public FileINI()
                : base(@"Config\setup.ini", true, MODE_SECTION_APPLICATION.INSTANCE)
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

                if (File.Exists(nameXMLTemplate) == true)
                    ;
                else
                    ;

                return docRes;
            }
        }
    }
}
