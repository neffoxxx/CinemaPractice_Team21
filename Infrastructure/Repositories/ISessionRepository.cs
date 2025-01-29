using Infrastructure.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        new Task<Session> GetByIdAsync(int id);
        Task<Session> GetByIdWithDetailsAsync(int id);
    }
} 