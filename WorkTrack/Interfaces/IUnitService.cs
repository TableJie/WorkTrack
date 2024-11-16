using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTrack;

namespace WorkTrack.Interfaces
{
    public interface IUnitService
    {
        Task<List<Unit>> GetUnitNamesAsync();
        Task<bool> SaveUnitAsync(Unit unit);
        Task<bool> DeleteUnitAsync(int unitId);
    }
}
