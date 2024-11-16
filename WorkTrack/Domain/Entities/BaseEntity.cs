using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkTrack.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class WorkTask : BaseEntity
    {
        [ObservableProperty]
        private int _taskID;

        [ObservableProperty]
        private string _taskName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _durationLevelID;

        [ObservableProperty]
        private string _durationLevelName = string.Empty;

        [ObservableProperty]
        private double _duration;

        [ObservableProperty]
        private int _unitID;

        [ObservableProperty]
        private string _unitName = string.Empty;

        [ObservableProperty]
        private string _applicationID = string.Empty;

        [ObservableProperty]
        private bool _deleteFlag;

        [ObservableProperty]
        private DateTime taskDate;
    }

    public class OverTime : BaseEntity
    {
        [ObservableProperty]
        private DateTime _taskDate;

        [ObservableProperty]
        private double _overHours;

        [ObservableProperty]
        private string _taskPlan1 = string.Empty;

        [ObservableProperty]
        private string _taskPlan2 = string.Empty;

        [ObservableProperty]
        private string _taskPlan3 = string.Empty;

        [ObservableProperty]
        private string _taskPlan4 = string.Empty;

        [ObservableProperty]
        private string _taskPlan5 = string.Empty;

        [ObservableProperty]
        private string _taskPlan6 = string.Empty;

        [ObservableProperty]
        private string _taskPlan7 = string.Empty;

        [ObservableProperty]
        private string _taskPlan8 = string.Empty;
    }

    public class Unit : BaseEntity
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }

    public class DurationLevel : BaseEntity
    {
    
        public int DurationLevelID { get; set; }
        public string DurationLevelName { get; set; } = string.Empty;

    }
}