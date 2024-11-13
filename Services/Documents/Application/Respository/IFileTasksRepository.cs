using Core.Database;
using Domain.Documents;
using Domain.FileTasks;


namespace Application.Respository
{
    public interface IFileTasksRepository : IRepository<FileTasks, Guid>
    {
        Task<FileTasks> FindFileTaskByDocument(Document document);
    }
}
