using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uLoaderCommon
{
    public enum DATETIME
    {
        SEC_SPANPERIOD_DEFAULT = 60
        , MSEC_INTERVAL_DEFAULT = 6666
        , MSEC_INTERVAL_TIMER_ACTIVATE = 66
    }
    /// <summary>
    /// Перечисление для типов опроса
    /// </summary>
    public enum MODE_WORK
    {
        UNKNOWN = -1
        , CUR_INTERVAL // по текущему интервалу
        , COSTUMIZE // выборочно (история)
            , COUNT_MODE_WORK
    }
}
