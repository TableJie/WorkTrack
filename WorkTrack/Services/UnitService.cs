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

    public class UnitService : IUnitService
    {
        private readonly ILogger _logger;
        private readonly IDbFactory _dbFactory;

        // Unit
        public async Task<List<Unit>> GetUnitNamesAsync()
        {
            var query = "SELECT * FROM Unit WHERE DeleteFlag = 0";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return (await connection.QueryAsync<Unit>(query)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "獲取單位名稱時發生錯誤");
                return new List<Unit>();
            }
        }
        public async Task<bool> SaveUnitAsync(Unit unit)
        {
            var query = "INSERT INTO Unit (UnitName) VALUES (@UnitName)";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, unit);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存單位時發生錯誤");
                return false;
            }
        }
        public async Task<bool> DeleteUnitAsync(int unitId)
        {
            var query = "UPDATE Unit SET DeleteFlag = 1 WHERE UnitID = @UnitID";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, new { UnitID = unitId });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "刪除單位時發生錯誤");
                return false;
            }
        }

    }
}