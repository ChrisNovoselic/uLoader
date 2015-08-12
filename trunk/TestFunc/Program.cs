using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using HClassLibrary;

namespace TestFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            ProgramBase.Start(false);

            string msg = @"Старт приложения [" + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff" + @"]");
            Console.WriteLine(msg);            

            Type typeTest =
                ////Вариант №1
                //typeof(biysktmora_test)
                //Вариант №2
                typeof(ClassDataTableDistinct)
                ;

            object objTest = Activator.CreateInstance(typeTest);

            msg = @"Выход из приложения [" + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff" + @"]");
            Console.WriteLine(Environment.NewLine + msg);

            Console.Write("\t\nPress any key to exit program..."); Console.ReadKey (true);
            Console.WriteLine(Environment.NewLine);

            ProgramBase.Exit();
        }

        public /*abstract */class timer_test
        {
            private object _data;
            protected virtual object Data { get { return _data; } set { _data = value; } }

            static Semaphore semaUserCancel;
            static int s_msecInterval = 1666;
            static Timer timer
                , timerKeyPress;
            static object lockTimer;

            public timer_test()
            {
                string msgWelcome = "\t\nКонсольное приложение тестирования объекта получения значений БД RtSoft-Siberia"
                + "\t\nСОТИАССО для Бийской ТЭЦ, ОАО СибЭКо (@ЗАО ИТС)"
                + "\t\n использование: при запуске /t=MSEC (MSEC - интервал обновления"
                + "\t\n в процессе выполнения 'Space' - пауза, 'Ctrl+C' - выход из программы"
                ;
                Console.WriteLine(msgWelcome);

                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    if ((args[1].Equals(@"/?") == true)
                        || (args[1].Equals(@"?") == true))
                        return;
                    else
                        ;

                    if ((args[1].Split('=').Length == 2)
                        && (args[1].Split('=')[0].Equals(@"/t") == true))
                        s_msecInterval = Int32.Parse(args[1].Split('=')[1]);
                    else
                        ;
                }
                else
                    ;

                EvtKeyPress += new DelegateObjectFunc(timer_test_EvtKeyPress);

                data.Start();
                data.Activate(true);

                semaUserCancel = new Semaphore(0, 1);
                lockTimer = new object();
                Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
                //Выполнить 1-ю итерацию немедленно (без повторения)
                timer = new Timer(timerCallback, null, 0, System.Threading.Timeout.Infinite);

                //Ожидать действия пользователя (другие)
                timerKeyPress = new Timer(timerKeyPressCallback, null, 0, 66);

                //Ожидать действия пользователя (Ctrl+C)
                semaUserCancel.WaitOne();

                data.Activate(false);
                data.Stop();
            }

            private event DelegateObjectFunc EvtKeyPress;
            private bool _keyPress;
            private bool KeyPress
            {
                get { return _keyPress; }

                set
                {
                    if (!(_keyPress == value))
                    {
                        _keyPress = value;

                        if (_keyPress == true)
                        {
                            ConsoleKeyInfo cki;
                            while (!((cki = Console.ReadKey(true)) == null))
                                EvtKeyPress(cki);

                            _keyPress = false;
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
            }

            private void timer_test_EvtKeyPress(object obj)
            {
                if (((ConsoleKeyInfo)obj).Key == ConsoleKey.Spacebar)
                {
                    if (Data.Actived == true)
                    {
                        lock (lockTimer)
                        {
                            if (!(timer == null))
                                //Деактивация...
                                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                            else
                                ;
                        }
                        //Деактивация...
                        Data.Activate(false);

                        Console.WriteLine();
                        Console.Write(@"Пауза (для продолжения нажмите 'пробел')...");
                    }
                    else
                    {
                        //Активация...
                        Data.Activate(true);

                        lock (lockTimer)
                        {
                            if (!(timer == null))
                                //Активация...
                                timer.Change(0, System.Threading.Timeout.Infinite);
                            else
                                ;
                        }

                        Console.WriteLine();
                    }
                }
                else
                    ;
            }

            private void timerKeyPressCallback(object obj)
            {
                KeyPress = Console.KeyAvailable;
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
                        timer.Change(s_msecInterval, System.Threading.Timeout.Infinite);
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

                timerKeyPress.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                timerKeyPress.Dispose();
                timerKeyPress = null;

                ev.Cancel = true;

                semaUserCancel.Release(1);
            }
        }
    }
}
