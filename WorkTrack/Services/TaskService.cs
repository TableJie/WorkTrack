using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;
using WorkTrack.Domain.Specifications;
using WorkTrack.Interfaces;
using WorkTrack.Services;

namespace WorkTrack.Services
{

    public class TaskService : ITaskService
    {
        private readonly IRepository<WorkTask> _taskRepository;
        private readonly ILogger _logger;

        // Task
        public TaskService(IRepository<WorkTask> taskRepository, ILogger logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
        }

        public async Task<List<WorkTask>> GetTasksAsync(DateTime date)
        {
            var spec = new DateRangeSpecification<WorkTask>(date, date);
            return (await _taskRepository.FindAsync(spec)).ToList();
        }


        public async Task<bool> SaveTaskAsync(WorkTask task)
        {
            if (task.Id == 0)
            {
                return await _taskRepository.AddAsync(task) > 0;
            }
            else
            {
                return await _taskRepository.UpdateAsync(task);
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            return await _taskRepository.DeleteAsync(taskId);
        }

        public async Task<List<DurationLevel>> GetDurationLevelsAsync()
        {
            var query = "SELECT * FROM DurationLevel";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return (await connection.QueryAsync<DurationLevel>(query)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "獲取持續時間級別時發生錯誤");
                return new List<DurationLevel>();
            }
        }


    }
}