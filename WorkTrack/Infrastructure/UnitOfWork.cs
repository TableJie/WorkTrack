using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;
using WorkTrack.Interfaces;
using WorkTrack.Services;

namespace WorkTrack.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbFactory _dbFactory;
        private readonly ILogger _logger;

        public IRepository<WorkTask> WorkTasks { get; }
        public IRepository<OverTime> OverTimes { get; }
        public IRepository<Unit> Units { get; }
        public IRepository<DurationLevel> DurationLevels { get; }

        public UnitOfWork(IDbFactory dbFactory, ILogger logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;

            WorkTasks = new Repository<WorkTask>(_dbFactory, _logger);
            OverTimes = new Repository<OverTime>(_dbFactory, _logger);
            Units = new Repository<Unit>(_dbFactory, _logger);
            DurationLevels = new Repository<DurationLevel>(_dbFactory, _logger);
        }

        public async Task<int> SaveChangesAsync()
        {
            // 實現保存更改的邏輯
            return 0;
        }

        public void Dispose()
        {
            // 實現釋放資源的邏輯
        }
    }
}
