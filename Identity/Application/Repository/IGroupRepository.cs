using Core.Database;
using Domain.User;


namespace Application.Repository
{
    public interface IGroupRepository : IRepository<Groups, Guid>
    {
    }
}
