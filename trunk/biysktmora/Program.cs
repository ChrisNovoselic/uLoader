using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using HClassLibrary;

namespace biysktmora
{
    class Program
    {
        static Semaphore semaUserCancel;
        static int msecInreval = 16666;
        static Timer timer;
        static object lockTimer;

        static HBiyskTMOra data;

        static void Main(string[] args)
        {
            ProgramBase.Start(false);

            string msg = @"Старт приложения [" + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff" + @"]");
            Console.WriteLine(msg);

            data = new HBiyskTMOra ();
            data.Start ();
            data.Activate(true);

            semaUserCancel = new Semaphore (0, 1);
            lockTimer = new object ();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            //Выполнить 1-ю итерацию немедленно (без повторения)
            timer = new Timer (timerCallback, null, 0, System.Threading.Timeout.Infinite);

            //Ожидать действия пользователя (Ctrl+C)
            semaUserCancel.WaitOne ();

            data.Activate (false);
            data.Stop();

            ////Console.WriteLine("Time (msec): {0}", (DateTime.Now - dtStart).Milliseconds);
            //Console.WriteLine("Last changed at: {0}", data.results.Rows[0][@"DATETIME"]);
            //Console.WriteLine("Rows result count: {0}", data.results.Rows.Count);

            msg = @"Выход из приложения [" + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff" + @"]");
            Console.WriteLine(msg);

            ProgramBase.Exit ();
        }

        private static void timerCallback (object obj)
        {
            string msg = @"Итерация... " + DateTime.Now.ToString (@"dd.MM.yyyy HH:mm:ss.fff");
            Console.WriteLine (msg);
            Logging.Logg ().Action (msg, Logging.INDEX_MESSAGE.NOT_SET);

            data.ChangeState ();

            lock (lockTimer)
            {
                if (! (timer == null))
                    //Повторить операцию...
                    timer.Change(msecInreval, System.Threading.Timeout.Infinite);
                else
                    ;
            }
        }

        private static void Console_CancelKeyPress (object obj, ConsoleCancelEventArgs ev)
        {
            lock (lockTimer)
            {
                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                timer.Dispose ();
                timer = null;
            }

            ev.Cancel = true;

            semaUserCancel.Release (1);
        }
    }
}
