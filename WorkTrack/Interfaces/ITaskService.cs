using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTrack;

namespace WorkTrack.Interfaces
{
    public interface ITaskService
    {
        Task<List<WorkTask>> GetTasksByDateAsync(DateTime taskDate);
        Task<List<DurationLevel>> GetDurationLevelsAsync();
        Task<bool> SaveTaskAsync(WorkTask newTask); // 基於業務邏輯的保存方法（新增或更新）
    }

}
