using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchForumData
{
    class Program
    {
        static int Main(string[] args)
        {
            string usersFile = Path.GetFullPath(@"..\..\..\ForumScorer\App_Data\users.txt");
            if (!File.Exists(usersFile))
            {
                Console.WriteLine($"Can't find users file '{usersFile}'.");
                return 1;
            }

            return 0;
        }
    }
}
