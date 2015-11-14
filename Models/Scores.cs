using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumModels
{
    public class Scores: IComparable<Scores>
    {
        public int MSDN { get; set; }
        public int StackOverflow { get; set; }
        public int Total { get { return MSDN + StackOverflow; } }

        public int CompareTo(Scores other)
        {
            return Total - other.Total;
        }
    }
}
