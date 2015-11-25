using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ForumModels
{
    public class UserList
    {
        List<User> _users;

        public DateTimeOffset StartOfLastWeek { get; }
        public DateTimeOffset StartOfThisWeek { get; }

        public int FirstStackOverflowPostForLastWeek { get; }
        public int FirstStackOverflowPostForThisWeek { get; }

        public UserList(string usersFile)
        {
            var nowLocal = DateTimeOffset.Now;
            var dayOfWeekLocal = (int)nowLocal.DayOfWeek;

            // +1 because we want to start the week on Monday and not Sunday (local time)
            var startOfThisWeekLocal = nowLocal.Date.AddDays(-dayOfWeekLocal + 1);

            StartOfThisWeek = startOfThisWeekLocal.ToUniversalTime();
            StartOfLastWeek = StartOfThisWeek.AddDays(-7);

            // We only count events attached to posts (questions or answers) that we created at most 3 days before
            // the week starts. The idea is to not keep rewarding users for old entries that are getting upticks
            FirstStackOverflowPostForThisWeek = QueryHelpers.GetIdOfFirstPostOfDay(StartOfThisWeek.AddDays(-3));
            FirstStackOverflowPostForLastWeek = QueryHelpers.GetIdOfFirstPostOfDay(StartOfLastWeek.AddDays(-3));

            string userJson = File.ReadAllText(usersFile);
            _users = JsonConvert.DeserializeObject<List<User>>(userJson);
        }

        public Task CalculateScores()
        {
            // Calculate the scores for all users in parallel
            return Task.WhenAll(_users.Select(user => user.CalculateScores(this)));
        }

        public void SaveDataFile(string dataFilePath)
        {
            File.WriteAllText(dataFilePath, JsonConvert.SerializeObject(_users, Formatting.Indented));
        }
    }
}
