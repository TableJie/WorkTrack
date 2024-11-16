using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;

namespace WorkTrack.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<WorkTask> WorkTasks { get; }
        IRepository<OverTime> OverTimes { get; }
        IRepository<Unit> Units { get; }
        IRepository<DurationLevel> DurationLevels { get; }

        Task<int> SaveChangesAsync();
    }
}
