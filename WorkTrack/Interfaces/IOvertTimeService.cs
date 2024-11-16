using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTrack;

namespace WorkTrack.Interfaces
{
    public interface IOverTimeService
    {
        Task<OverTime?> GetOverTimeAsync(DateTime date);
        Task<bool> AddOverTimeAsync(OverTime overTime);
        Task<bool> EditOverTimeAsync(OverTime overTime);
        Task<bool> DeleteOverTimeAsync(DateTime date);

    }
}
