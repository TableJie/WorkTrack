using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;
using WorkTrack.Domain.Specifications;

namespace WorkTrack.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(ISpecification<T> spec);
        Task<int> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<bool> ExistsAsync(ISpecification<T> spec);
    }
}
