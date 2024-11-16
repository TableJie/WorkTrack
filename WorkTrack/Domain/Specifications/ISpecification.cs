using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;

namespace WorkTrack.Domain.Specifications
{
    public interface ISpecification<T> where T : BaseEntity
    {
        string ToSql();
        object? GetParameters();
    }
}
