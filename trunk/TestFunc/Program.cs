using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                //Вариант №1
                typeof(biysktmora_test)
                ;

            object objTest = Activator.CreateInstance(typeTest);

            msg = @"Выход из приложения [" + DateTime.Now.ToString(@"dd.MM.yyyy HH:mm:ss.fff" + @"]");
            Console.WriteLine(Environment.NewLine + msg);

            Console.Write("\t\nPress any key to exit program..."); Console.ReadKey (true);
            Console.WriteLine(Environment.NewLine);

            ProgramBase.Exit();
        }
    }
}
