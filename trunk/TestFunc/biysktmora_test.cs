using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using HClassLibrary;
using uLoaderCommon;
using SrcBiyskTMora;

namespace TestFunc
{
    class biysktmora_test : TestFunc.Program.timer_test
    {
        public biysktmora_test()
            : base(@"БД RtSoft-Siberia", @"СОТИАССО для Бийской ТЭЦ")
        {            
        }

        protected override int initialize()
        {
            int iRes = 0;

            Data = new SrcBiyskTMora.SrcBiyskTMora();

            Data.Initialize(new object[] {
                                new ConnectionSettings (
                                    @"OraSOTIASSO-ORD"
                                    , @"10.220.2.5"
                                    , 1521
                                    , @"ORCL"
                                    , @"arch_viewer"
                                    , @"1")
                            }
                    );
            Data.Initialize(0
                , new object[]
                {
                    new object [] { 20049, @"Sgnl0", @"TAG_000049" }
                    , new object [] { 20051, @"Sgnl1", @"TAG_000051" }
                    , new object [] { 20053, @"Sgnl2", @"TAG_000053" }
                    , new object [] { 20056, @"Sgnl3", @"TAG_000056" }
                    , new object [] { 20057, @"Sgnl4", @"TAG_000057" }
                    , new object [] { 20061, @"Sgnl5", @"TAG_000061" }
                    , new object [] { 20062, @"Sgnl6", @"TAG_000062" }
                    , new object [] { 20063, @"Sgnl7", @"TAG_000063" }
                    , new object [] { 20064, @"Sgnl8", @"TAG_000064" }
                }
            );            

            return iRes;
        }
    }
}
