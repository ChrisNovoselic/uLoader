using HClassLibrary;
using System;
using uLoaderCommon;

namespace xmlServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logging.SetMode(@"log=", Logging.LOG_MODE.FILE_EXE);

            TimeSpan tsIntervalSeries = TimeSpan.Zero;

            UdpServer udpServer;

            try {
                if (args.Length == 1)
                    if (args[0].Length > 3)
                        tsIntervalSeries = new HTimeSpan(args[0].Substring(1, args[0].Length - 1)).Value;
                    else {
                        Console.WriteLine(string.Format(@"Ошибка при разборе аргумента [{0}] командной строки", args[0]));
                        Logging.Logg().Error(string.Format(@"Разбор аргумента [{0}] командной строки", args[0]), Logging.INDEX_MESSAGE.NOT_SET);
                    }
                else {
                    Console.WriteLine(string.Format(@"Ошибка при разборе аргументов командной строки - некорректное кол-во"));
                    Logging.Logg().Error(string.Format(@"Разбор аргументов командной строки - некорректное кол-во"), Logging.INDEX_MESSAGE.NOT_SET);
                }
            } catch (Exception e) {
                Console.WriteLine(string.Format(@"Исключение при разборе аргумента [{1}] командной строки: {0}", e.Message, args[0]));
                Logging.Logg().Exception(e, string.Format(@"Разбор аргумента [{0}] командной строки", args[0]), Logging.INDEX_MESSAGE.NOT_SET);
            }

            udpServer = new UdpServer(tsIntervalSeries);
            udpServer.Start();

            Console.WriteLine(string.Format(@"Для продолжения нажмите любую..."));
            //Console.CancelKeyPress += onCancelKeyPress;
            Console.ReadKey(true);

            try {
                udpServer.Stop();
            } catch (Exception e) {
                Console.WriteLine(string.Format(@"Исключение: {0}", e.Message));
            }

            GC.Collect();
            }

        private static void onCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
