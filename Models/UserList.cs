using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ForumModels
{
    public class UserList
    {
        List<User> _users;

        public UserList(string usersFile)
        {
            string userJson = File.ReadAllText(usersFile);
            _users = JsonConvert.DeserializeObject<List<User>>(userJson);
        }

        public void CalculateScores()
        {
            foreach (var user in _users)
            {
                Console.WriteLine($"Processing {user.Name}");
                try
                {
                    user.CalculateScores();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }
            }
        }

        public void SaveDataFile(string dataFilePath)
        {
            File.WriteAllText(dataFilePath, JsonConvert.SerializeObject(_users, Formatting.Indented));
        }
    }
}
