using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkTrack.Domain.Entities;

namespace WorkTrack.Domain.Specifications
{
    public class DateRangeSpecification<T> : BaseSpecification<T> where T : BaseEntity
    {
        public DateRangeSpecification(DateTime startDate, DateTime endDate)
        {
            _sql = "WHERE TaskDate BETWEEN @StartDate AND @EndDate AND DeleteFlag = 0";
            _parameters = new { StartDate = startDate.Date, EndDate = endDate.Date.AddDays(1).AddSeconds(-1) };
        }
    }
}
