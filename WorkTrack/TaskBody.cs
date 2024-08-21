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
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationLevelID { get; set; }
        public String DurationLevelName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int UnitID { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string ApplicationID { get; set; } = string.Empty;
        public bool DeleteFlag { get; set; }
        public DateTime TaskDate { get; set; }
    }

    public class Unit
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }

    public class DurationLevel
    {
    
        public int DurationLevelID { get; set; }
        public string DurationLevelName { get; set; } = string.Empty;

    }
}