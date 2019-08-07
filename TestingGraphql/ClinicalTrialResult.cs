using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingGraphql
{
    public class ClinicalTrialResult<T>
    {
        public List<T> Items { get; set; }

        public string EndCursor { get; set; }
        public bool HasNextPage { get; set; }

        public string StartCursor { get; set; }
        public bool HasPreviousPage { get; set; }

        public string TotalCount { get; set; }
    }
}
