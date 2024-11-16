using Dapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;
using WorkTrack.Domain.Specifications;
using WorkTrack.Interfaces;

namespace WorkTrack.Services
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IDbFactory _dbFactory;
        private readonly ILogger _logger;

        public Repository(IDbFactory dbFactory, ILogger logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var query = $"SELECT * FROM {typeof(T).Name} WHERE Id = @Id";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.QuerySingleOrDefaultAsync<T>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get {Entity} by Id: {Id}", typeof(T).Name, id);
                return null;
            }
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var query = $"SELECT * FROM {typeof(T).Name}";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.QueryAsync<T>(query);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get all {Entity}", typeof(T).Name);
                return Enumerable.Empty<T>();
            }
        }
        public async Task<IEnumerable<T>> FindAsync(string query, object? parameters = null)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.QueryAsync<T>(query, parameters);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute query on {Entity}", typeof(T).Name);
                return Enumerable.Empty<T>();
            }
        }
        public async Task<int> AddAsync(T entity)
        {
            var query = $"INSERT INTO {typeof(T).Name} ({string.Join(", ", typeof(T).GetProperties().Select(p => p.Name))}) " +
                        $"VALUES ({string.Join(", ", typeof(T).GetProperties().Select(p => "@" + p.Name))});";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.ExecuteAsync(query, entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add {Entity}", typeof(T).Name);
                return 0;
            }
        }
        public async Task<bool> UpdateAsync(T entity)
        {
            var query = $"UPDATE {typeof(T).Name} SET " +
                        $"{string.Join(", ", typeof(T).GetProperties().Select(p => p.Name + " = @" + p.Name))} " +
                        $"WHERE Id = @Id;";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, entity);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update {Entity}", typeof(T).Name);
                return false;
            }
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var query = $"DELETE FROM {typeof(T).Name} WHERE Id = @Id";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete {Entity} by Id: {Id}", typeof(T).Name, id);
                return false;
            }
        }
        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            var query = $"SELECT COUNT(*) FROM {typeof(T).Name} {spec.ToSql()}";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.ExecuteScalarAsync<int>(query, spec.GetParameters());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to count {Entity}", typeof(T).Name);
                return 0;
            }
        }
        public async Task<bool> ExistsAsync(ISpecification<T> spec)
        {
            var query = $"SELECT EXISTS(SELECT 1 FROM {typeof(T).Name} {spec.ToSql()})";
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();
                return await connection.ExecuteScalarAsync<bool>(query, spec.GetParameters());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check existence of {Entity}", typeof(T).Name);
                return false;
            }
        }
    }

}
