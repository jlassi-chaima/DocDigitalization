using Core.Database;
using Domain.Logs;


namespace Application.Respository
{
    public interface ILogRepository :   IRepository<Logs, Guid>
    {
        Task<List<Logs>> getLogsBySource(LogName source);
    }
}
