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

            // Adjust it so the week starts Monday instead of Sunday
            dayOfWeekLocal = (dayOfWeekLocal + 6) % 7;

            // Go back to the start of the current week
            return dt.Date.AddDays(-dayOfWeekLocal);
        }

        public static string GetDataFileNameFromDate(DateTimeOffset startOfPeriod)
        {
            return startOfPeriod.ToString("yyyy-MM-dd") + ".json";
        }
    }
}
