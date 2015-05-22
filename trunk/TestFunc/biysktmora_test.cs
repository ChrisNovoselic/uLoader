using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using HClassLibrary;

using biysktmora;

namespace TestFunc
{
    class biysktmora_test
    {
        static Semaphore semaUserCancel;
        static int msecInreval = 1000;
        static Timer timer;
        static object lockTimer;

        static HBiyskTMOra data;

        public biysktmora_test ()
        {
            data = new HBiyskTMOra();
            data.Initialize (new ConnectionSettings (
                        @"OraSOTIASSO-ORD"
                        , @"10.220.2.5"
                        , 1521
                        , @"ORCL"
                        , @"arch_viewer"
                        , @"1"
                    ));
            data.Initialize(
                new object []
                {
                    new object [] { 20049, @"TAG_000049" }
                    , new object [] { 20051, @"TAG_000051" }
                    , new object [] { 20053, @"TAG_000053" }
                    , new object [] { 20056, @"TAG_000056" }
                    , new object [] { 20057, @"TAG_000057" }
                    , new object [] { 20061, @"TAG_000061" }
                    , new object [] { 20062, @"TAG_000062" }
                    , new object [] { 20063, @"TAG_000063" }
                    , new object [] { 20064, @"TAG_000064" }
                }
            );
            data.Start();
            data.Activate(true);

            semaUserCancel = new Semaphore(0, 1);
            lockTimer = new object();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            //Выполнить 1-ю итерацию немедленно (без повторения)
            timer = new Timer(timerCallback, null, 0, System.Threading.Timeout.Infinite);

            //Ожидать действия пользователя (Ctrl+C)
            semaUserCancel.WaitOne();

            data.Activate(false);
            data.Stop();
        }

        private static void timerCallback(object obj)
        {
            string msg = "\t\nИтерация... " + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff");
            Console.WriteLine(msg);
            Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);

            data.ChangeState();

            lock (lockTimer)
            {
                if (!(timer == null))
                    //Повторить операцию...
                    timer.Change(msecInreval, System.Threading.Timeout.Infinite);
                else
                    ;
            }
        }

        private static void Console_CancelKeyPress(object obj, ConsoleCancelEventArgs ev)
        {
            lock (lockTimer)
            {
                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                timer.Dispose();
                timer = null;
            }

            ev.Cancel = true;

            semaUserCancel.Release(1);
        }
    }
}
