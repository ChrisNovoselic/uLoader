using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TestFunc
{
    class ClassDataTableDistinct
    {
        public ClassDataTableDistinct()
        {
            DataTable dt1 = new DataTable()
                , dt2 = new DataTable();

            dt1.Columns.Add(@"ID", typeof(int));
            dt1.Columns.Add(@"DATETIME", typeof(DateTime));
            dt1.Columns.Add(@"VALUE", typeof(decimal));

            dt2 = dt1.Clone();

            dt1.Rows.Add(new object[] { 1, DateTime.Now.AddHours(4), 12.3 });
            dt1.Rows.Add(new object[] { 2, DateTime.Now.AddHours(0), 23.4 });
            dt1.Rows.Add(new object[] { 1, DateTime.Now.AddHours(6), 34.5 });

            dt2.Rows.Add(new object[] { 2, DateTime.Now.AddHours(0), 12.4 });
            dt2.Rows.Add(new object[] { 1, DateTime.Now.AddHours(1), 23.5 });
            dt2.Rows.Add(new object[] { 2, DateTime.Now.AddHours(0), 34.6 });

            dt1.Merge(dt2, true);

            ////Вариант №1
            ////ТаблицаБезОдинаковыхСтрок = YourTable.DefaultView.ToTable(true,ColumnList);
            //dt1 = dt1.DefaultView.ToTable(true, @"ID", @"DATETIME");

            //IEnumerable<DataRow> distinctRows = dt1.AsEnumerable().Distinct(c >= (DataRow)c[@"DATETIME"]);
            //var vRes = dt1.AsEnumerable().Select(c => (DataRow)c["ID"]).Distinct().ToList();
            //// получение самой "свежей" записи для каждого сигнала
            //var vRes = dt1.AsEnumerable().GroupBy(g => g[@"ID"]).Select (s => s.First()).ToList();
            // получение уникальных записей по нескольким полям
            var vRes = dt1.AsEnumerable().GroupBy(g => new { ID = g.Field<int>(@"ID"), DATETIME = g.Field<DateTime>(@"DATETIME")}).Select(s => s.First()).ToList();

            vRes.ForEach (v => Console.WriteLine(@"ID=" + v[@"ID"] + @", DATETIME=" + v[@"DATETIME"].ToString ()));
        }
    }
}
