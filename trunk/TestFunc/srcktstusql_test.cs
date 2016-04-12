using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using uLoaderCommon;

using SrcKTS;

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

            Data = new SrcKTS.SrcKTSTUsql();

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
                    new object [] { 10101, @"t окр.возд.", 12145, "true" }
                    , new object [] { 10102, @"t обск.воды", 12133, "true" }
                }
            );

            return iRes;
        }
    }
}
