using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.ComponentModel;

using HClassLibrary;

namespace TestProject_001
{
    [TestClass]
    public class UnitTest_DBSources
    {
        [TestMethod]
        public void TestMethodRegister()
        {
            BackgroundWorker [] arThreadDbConn;
            
            List <int> listListenerId = new List<int> ();
            ConnectionSettings [] arConnSett = new ConnectionSettings []
            {
                new ConnectionSettings (671, @"Statistic-CentreV", "10.100.104.18", 1433, @"techsite-2.X.X", @"client", @"client")
                , new ConnectionSettings (63, @"Oracle", "10.220.2.5", 1521, @"ORCL", @"arch_viewer", @"1")
                , new ConnectionSettings (675, @"Statistic-CentreS", "10.100.204.63", 1433, @"techsite-2.X.X", @"client", @"client")
            };

            arThreadDbConn = new BackgroundWorker [arConnSett.Length];
            int i = -1;
            for (i = 0; i < arThreadDbConn.Length; i ++)
            {
                arThreadDbConn[i] = new BackgroundWorker ();
                arThreadDbConn[i].DoWork += fThreadProc;
                arThreadDbConn[i].RunWorkerAsync(arConnSett[i]);
            }

            DbSources.Sources().UnRegister();
        }

        private void fThreadProc (object obj, DoWorkEventArgs ev)
        {
            int iListenerId = -1;
            for (int j = 0; j < 16; j++)
            {
                iListenerId = DbSources.Sources().Register(ev.Argument as ConnectionSettings, true, (ev.Argument as ConnectionSettings).name + @"-DbInterface");
                //listListenerId.Add(iListenerId);
                //Console.WriteLine(@"ConnectionSettings.ID=" + (ev.Argument as ConnectionSettings).id + @", подписка.ID=" + iListenerId);
            }
        }
    }
}
