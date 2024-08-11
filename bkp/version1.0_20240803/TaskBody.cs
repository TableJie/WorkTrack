using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTrack
{
    public class TaskBody
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public string UnitID { get; set; }
        public string ApplicationID { get; set; }
        public string DurationLevel { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime TaskDate { get; set; }
    }
}