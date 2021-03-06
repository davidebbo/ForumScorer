﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ForumModels
{
    public class User
    {
        UserList _userList;

        public string Name { get; set; }

        public string MSDNName { get; set; }
        public string StackOverflowID { get; set; }

        public Scores Scores { get; } = new Scores();

        public async Task CalculateScores(UserList userList)
        {
            Console.WriteLine($"Processing {Name}");

            _userList = userList;

            try {
                await Task.WhenAll(CalculateMSDNScore(), CalculateStackOverflowScores());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error for user {Name}: " + e);
                throw;
            }
        }

        async Task CalculateMSDNScore()
        {
            if (String.IsNullOrWhiteSpace(MSDNName))
                return;

            string rssXml = await QueryHelpers.GetStringAsync($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={MSDNName}");
            var root = XElement.Parse(rssXml);
            CalculateMSDNScoresFromRss(root);
        }

        void CalculateMSDNScoresFromRss(XElement root)
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
                    newScore += 20;
                }
                else if (title.StartsWith("Contributed a helpful post"))
                {
                    newScore += 10;
                }
                else if (title.StartsWith("Replied to a forums thread"))
                {
                    newScore += 1;
                }

                if (dt > _userList.StartOfPeriod && dt < _userList.EndOfPeriod)
                {
                    Scores.MSDN += newScore;
                }
            }
        }

        async Task CalculateStackOverflowScores()
        {
            if (String.IsNullOrWhiteSpace(StackOverflowID))
                return;

            Scores.StackOverflow = await GetStackOverflowReputation(_userList.StartOfPeriod, _userList.EndOfPeriod, _userList.FirstStackOverflowPost);
        }

        async Task<int> GetStackOverflowReputation(DateTimeOffset start, DateTimeOffset end, int firstPostIdToConsider)
        {
            dynamic items = await QueryHelpers.Query(
                $"/2.2/users/{StackOverflowID}/reputation",
                $"fromdate={start.ToUnixTimeSeconds()}&todate={end.ToUnixTimeSeconds()}");

            int reputation = 0;
            foreach (var item in items)
            {
                // Ignore if the post that the event is attached to is too old
                if (item.post_id < firstPostIdToConsider)
                    continue;

                if (item.vote_type == "up_votes")
                {
                    if (item.post_type == "answer")
                    {
                        reputation += 10;
                    }
                    else
                    {
                        // No points for upvotes on questions
                    }
                }
                else if (item.vote_type == "accepts")
                {
                    reputation += 20;
                }
                else
                {
                    // We ignore bounties
                }
            }

            return reputation;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
