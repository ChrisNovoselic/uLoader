//using HClassLibrary;
using ASUTP;
using ASUTP.Helper;
using System;
using System.Net;
using uLoaderCommon;

namespace xmlServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ProgramBase.Start (Logging.LOG_MODE.FILE_EXE, false);

            HCmd_Arg cmdArg;

            TimeSpan tsIntervalSeries = TimeSpan.Zero;
            IPAddress ip = IPAddress.Loopback;

            UdpServer udpServer;

            try {
                cmdArg = new HCmd_Arg();

                if (!(cmdArg.Length > 2)) {
                    ip = (IPAddress)cmdArg.ElementAt(@"dest");

                    tsIntervalSeries = (TimeSpan)cmdArg.ElementAt(@"period");
                } else {
                    Console.WriteLine(string.Format(@"Ошибка при разборе аргументов командной строки - некорректное кол-во"));
                    Logging.Logg().Error(string.Format(@"Разбор аргументов командной строки - некорректное кол-во"), Logging.INDEX_MESSAGE.NOT_SET);

                    Console.WriteLine(string.Format(@"Допустимые аргументы командной строки:{0}
                        {1}")
                        , Environment.NewLine
                        , cmdArg.List
                    );
                }
            } catch (Exception e) {
                Console.WriteLine(string.Format(@"Исключение при разборе аргумента [{1}] командной строки: {0}", e.Message, args[0]));
                Logging.Logg().Exception(e, string.Format(@"Разбор аргумента [{0}] командной строки", args[0]), Logging.INDEX_MESSAGE.NOT_SET);
            }

            udpServer = new UdpServer(ip, tsIntervalSeries);
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
