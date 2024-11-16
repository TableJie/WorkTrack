using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;

namespace WorkTrack.Domain.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
    {
        protected string _sql = string.Empty;
        protected object? _parameters;

        public virtual string ToSql() => _sql;
        public virtual object? GetParameters() => _parameters;
    }
}
