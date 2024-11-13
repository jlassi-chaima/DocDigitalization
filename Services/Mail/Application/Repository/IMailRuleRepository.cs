using Application.Dtos.MailRule;
using Application.PaginationParams;
using Core.Database;
using DD.Core.Pagination;
using Domain.MailRules;


namespace Application.Repository
{
    public interface IMailRuleRepository : IRepository<MailRule, Guid>
    {
        Task<List<MailRule>> GetAllByOrderAsync(CancellationToken cancellationToken = default);
        Task<PagedList<MailRulePagedList>> GetAllByPagedAsync(MailRuleParameters mailacountparameters, Guid groupId, CancellationToken cancellationToken = default);
    }
}