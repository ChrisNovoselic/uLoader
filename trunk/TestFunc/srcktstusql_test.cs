using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using uLoaderCommon;

using SrcKTSTUsql;

namespace TestFunc
{
    class srcktstusql_test : TestFunc.Program.timer_test
    {
        public srcktstusql_test()
            : base(@"БД КТС Энергия+", @"Вх.данные для расчета ТЭП")
        {
        }

        protected override int initialize()
        {
            int iRes = 0;

            Data = new SrcKTSTUsql.SrcKTSTUsql();

            Data.Initialize(new object[] {
                                new ConnectionSettings (
                                    @"KTS-Teplo-1"
                                    , @"10.105.2.157"
                                    , 1433
                                    , @"e6work"
                                    , @"eng6"
                                    , @"eng6")
                            }
                    );
            Data.Initialize(0
                , new object[]
                {
                    new object [] { 12077, @"t окр.возд." }
                    , new object [] { 12609, @"t обск.воды" }
                }
            );

            return iRes;
        }
    }
}
