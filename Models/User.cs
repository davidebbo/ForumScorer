using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ForumModels
{
    public class User
    {
        public string Name { get; set; }

        public string MSDNName { get; set; }
        public string StackOverflowID { get; set; }

        public int WeekScore { get; set; }
        public int PreviousWeekScore { get; set; }

        public void CalculateScores()
        {
            GetMSDNScore();
            GetStackOverflowScore();
        }

        private void GetMSDNScore()
        {
            try
            {
                var root = XElement.Load($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={MSDNName}");
                GetScoreFromRss(root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                WeekScore = -1;
            }
        }

        private void GetScoreFromRss(XElement root)
        {
            XNamespace ns = "";

            var today = DateTime.UtcNow.Date;
            int dayOfWeek = (int)today.DayOfWeek;
            dayOfWeek = (dayOfWeek + 1) % 7;    // We wrap the week Friday night (utc)
            DateTime firstDayOfThisWeek = today.AddDays(-dayOfWeek);
            DateTime firstDayOfLastWeek = firstDayOfThisWeek.AddDays(-7);

            var titles = new HashSet<string>();

            foreach (var item in root.Descendants(ns + "item"))
            {
                string pubDate = item.Descendants(ns + "pubDate").FirstOrDefault()?.Value;
                string title = item.Descendants(ns + "title").FirstOrDefault()?.Value;

                // Don't process if title is identical
                if (titles.Contains(title)) continue;

                titles.Add(title);

                var dt = DateTime.Parse(pubDate);

                int newScore = 0;

                if (title.StartsWith("Quickly answered the question"))
                {
                    newScore += 20;
                }
                else if (title.StartsWith("Answered the question"))
                {
                    newScore += 15;
                }
                else if (title.StartsWith("Contributed a helpful post"))
                {
                    newScore += 5;
                }
                else if (title.StartsWith("Replied to a forums thread"))
                {
                    newScore += 1;
                }

                if (dt > firstDayOfThisWeek)
                {
                    WeekScore += newScore;
                }
                else if (dt > firstDayOfLastWeek)
                {
                    PreviousWeekScore += newScore;
                }
                else
                {
                    // Older than we process
                    break;
                }
            }
        }

        private void GetStackOverflowScore()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
