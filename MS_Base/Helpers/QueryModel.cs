using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MS_Base.Helpers
{
    public class QueryModel
    {
        public string filePath { get; set; }
        public string fileName { get; set; }
        public string queryContent { get; set; }
        public DateTime modifiedDate { get; set; }
    }
}
