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
        public string TaskName { get; set; } = string.Empty; // 設置初始值
        public string Description { get; set; } = string.Empty; // 設置初始值
        public long Duration { get; set; }
        public string UnitID { get; set; } = string.Empty; // 設置初始值
        public string ApplicationID { get; set; } = string.Empty; // 設置初始值
        public string DurationLevel { get; set; } = string.Empty; // 設置初始值
        public int Points { get; set; } = 0;
        public bool DeleteFlag { get; set; }
        public DateTime TaskDate { get; set; }
    }
}