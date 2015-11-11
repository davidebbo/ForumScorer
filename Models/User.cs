using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ForumModels
{
    public class User
    {
        DateTimeOffset _firstDayOfLastWeek;
        DateTimeOffset _firstDayOfThisWeek;

        public string Name { get; set; }

        public string MSDNName { get; set; }
        public string StackOverflowID { get; set; }

        public int WeekScore { get; set; }
        public int PreviousWeekScore { get; set; }

        public void CalculateScores()
        {
            var today = DateTimeOffset.UtcNow.Date;
            int dayOfWeek = (int)today.DayOfWeek;
            dayOfWeek = (dayOfWeek + 1) % 7;    // We wrap the week Friday night (utc)
            _firstDayOfThisWeek = today.AddDays(-dayOfWeek);
            _firstDayOfLastWeek = _firstDayOfThisWeek.AddDays(-7);

            CalculateMSDNScore();
            //CalculateStackOverflowScores();
        }

        void CalculateMSDNScore()
        {
            if (String.IsNullOrWhiteSpace(MSDNName))
                return;

            try
            {
                var root = XElement.Load($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={MSDNName}");
                CalculateScoreFromRss(root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                WeekScore = -1;
            }
        }

        void CalculateScoreFromRss(XElement root)
        {
            XNamespace ns = "";

            var titles = new HashSet<string>();

            foreach (var item in root.Descendants(ns + "item"))
            {
                string pubDate = item.Descendants(ns + "pubDate").FirstOrDefault()?.Value;
                string title = item.Descendants(ns + "title").FirstOrDefault()?.Value;

                // Don't process if title is identical
                if (titles.Contains(title)) continue;

                titles.Add(title);

                var dt = DateTimeOffset.Parse(pubDate);

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

                if (dt > _firstDayOfThisWeek)
                {
                    WeekScore += newScore;
                }
                else if (dt > _firstDayOfLastWeek)
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

        void CalculateStackOverflowScores()
        {
            if (String.IsNullOrWhiteSpace(StackOverflowID))
                return;

            PreviousWeekScore += GetStackOverflowReputation(_firstDayOfLastWeek, _firstDayOfThisWeek);
            WeekScore += GetStackOverflowReputation(_firstDayOfThisWeek, DateTimeOffset.UtcNow);
        }

        int GetStackOverflowReputation(DateTimeOffset start, DateTimeOffset end)
        {
            string stackOverflowKey = ConfigurationManager.AppSettings["StackOverflowKey"];

            string queryUrl = String.Format(
                "http://api.stackexchange.com/2.2/users/{0}/reputation?fromdate={1}&todate={2}& key={3}&site=stackoverflow",
                StackOverflowID, start.ToUnixTimeSeconds(), end.ToUnixTimeSeconds(), stackOverflowKey);

            string json;
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                json = client.GetStringAsync(queryUrl).Result;
            }

            var items = JsonConvert.DeserializeObject<dynamic>(json).items;

            foreach (var item in items)
            {
                int qqq = item.reputation_change;
            }

            return 0;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
