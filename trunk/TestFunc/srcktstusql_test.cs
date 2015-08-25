using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

using SrcKTSTUsql;

namespace TestFunc
{
    class srcktstusql_test : TestFunc.Program.timer_test
    {
        protected override object Data
        {
            get
            {
                return base.Data as SrcKTSTUsql.SrcKTSTUsql;
            }

            set
            {
                base.Data = value;
            }
        }

        public srcktstusql_test() : base ()
        {
            Data = new SrcKTSTUsql.SrcKTSTUsql();
            Data.Initialize (new object [] {
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
                , new object []
                {
                    new object [] { 20049, @"TAG_000049" }
                    , new object [] { 20051, @"TAG_000051" }
                    , new object [] { 20053, @"TAG_000053" }
                    , new object [] { 20056, @"TAG_000056" }
                    , new object [] { 20057, @"TAG_000057" }
                    , new object [] { 20061, @"TAG_000061" }
                    , new object [] { 20062, @"TAG_000062" }
                    , new object [] { 20063, @"TAG_000063" }
                    , new object [] { 20064, @"TAG_000064" }
                }
            );
        }
    }
}
