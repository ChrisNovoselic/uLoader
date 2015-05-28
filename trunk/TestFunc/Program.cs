using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

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

            DataTable dt1 = new DataTable()
                , dt2 = new DataTable();

            dt1.Columns.Add (@"ID", typeof(int));
            dt1.Columns.Add(@"DATETIME", typeof(DateTime));
            dt1.Columns.Add(@"VALUE", typeof(decimal));

            dt2 = dt1.Clone();

            dt1.Rows.Add(new object[] { 1, DateTime.Now, 12.3 });
            dt1.Rows.Add(new object[] { 2, DateTime.Now, 23.4 });
            dt1.Rows.Add(new object[] { 1, DateTime.Now, 34.5 });

            dt2.Rows.Add(new object[] { 2, DateTime.Now, 12.4 });
            dt2.Rows.Add(new object[] { 1, DateTime.Now, 23.5 });
            dt2.Rows.Add(new object[] { 2, DateTime.Now, 34.6 });

            dt1.Merge(dt2, true);

            dt1 = dt1.DefaultView.ToTable(true);

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
