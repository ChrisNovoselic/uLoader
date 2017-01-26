using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace xmlLoader
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int iRes = 0;
            FormMain formMain = null;

            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            try { ProgramBase.Start(); }
            catch (Exception e) {
                MessageBox.Show(null, e.Message + "\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                iRes = -1;
            }

            if (iRes == 0) {
                string strHeader = string.Empty;
                try {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    formMain = new FormMain();
                } catch (Exception e) {
                    strHeader = "Ошибка запуска приложения";
                    MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeader);
                    Logging.Logg().Exception(e, strHeader, Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(formMain == null))
                    try {                        
                        Application.Run(formMain);
                    } catch (Exception e) {
                        strHeader = "Ошибка выполнения приложения";
                        MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeader);
                        Logging.Logg().Exception(e, strHeader, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                else
                    ;

                ProgramBase.Exit();
            }
            else
                ;
        }
    }
}
