using System.Data;
using System.Threading.Tasks;

namespace WorkTrack.Interfaces
{
    public interface IDbFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }

    public interface IInitializer
    {
        void Initialize();
    }
}