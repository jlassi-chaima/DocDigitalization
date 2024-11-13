using Application.Dtos.MailAccount;
using Application.PaginationParams;
using Core.Database;
using DD.Core.Pagination;
using Domain.MailAccounts;

namespace Application.Repository
{
    public interface IMailAccountRepository : IRepository<MailAccount, Guid>
    {
        Task <PagedList<MailAccountPagedList>> GetAllByPagedAsync(MailAccountParameters mailacountparameters, Guid groupId, CancellationToken cancellationToken = default);
        Task<MailAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
