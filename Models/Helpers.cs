using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumModels
{
    public static class Helpers
    {
        public static DateTimeOffset GetStartOfWeek(string date)
        {
            var dt = DateTimeOffset.Now;
            if (!String.IsNullOrEmpty(date))
            {
                dt = DateTimeOffset.Parse(date);
            }
            dt = dt.ToLocalTime();

            var dayOfWeekLocal = (int)dt.DayOfWeek;

            // +1 because we want to start the week on Monday and not Sunday (local time)
            return dt.Date.AddDays(-dayOfWeekLocal + 1);
        }

        public static string GetDataFileNameFromDate(DateTimeOffset startOfPeriod)
        {
            return startOfPeriod.ToString("yyyy-MM-dd") + ".json";
        }
    }
}
