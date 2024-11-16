using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WorkTrack.Interfaces;
using WorkTrack.Services;

namespace WorkTrack.Services
{

    public class OverTime : IOverTime
    {
        private readonly ILogger _logger;
        private readonly IDbFactory _dbFactory;

        // OverTime
        public async Task<OverTime?> GetOverTimeAsync(DateTime date)
        {
            var query = "SELECT * FROM OverTime WHERE TaskDate = @TaskDate";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.QuerySingleOrDefaultAsync<OverTime>(query, new { TaskDate = date });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch overtime for {TaskDate}", date);
                return null;
            }
        }
        public async Task<bool> AddOverTimeAsync(OverTime overTime)
        {
            var query = "INSERT INTO OverTime (TaskDate, Hours) VALUES (@TaskDate, @Hours)";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, overTime);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add overtime for {TaskDate}", overTime.TaskDate);
                return false;
            }
        }
        public async Task<bool> EditOverTimeAsync(OverTime overTime)
        {
            var query = "UPDATE OverTime SET Hours = @Hours WHERE TaskDate = @TaskDate";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, overTime);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to edit overtime for {TaskDate}", overTime.TaskDate);
                return false;
            }
        }
        public async Task<bool> DeleteOverTimeAsync(DateTime date)
        {
            var query = "DELETE FROM OverTime WHERE TaskDate = @TaskDate";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, new { TaskDate = date });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete overtime for {TaskDate}", date);
                return false;
            }
        }

    }
}