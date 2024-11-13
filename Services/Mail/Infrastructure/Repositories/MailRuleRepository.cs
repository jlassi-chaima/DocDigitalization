using Application.Dtos.MailRule;
using Application.Exceptions;
using Application.PaginationParams;
using Application.Repository;
using DD.Core.Pagination;
using Domain.MailRules;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Infrastructure.Repositories
{
    public class MailRuleRepository : IMailRuleRepository
    {
        private readonly DBContext _context;

        public MailRuleRepository(DBContext context)
        {
            _context = context;
        }
        public Task AddAsync(MailRule entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.AddAsync(entity, cancellationToken);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return Task.FromResult(new MailRule());
        }

        public Task AddRangeAsync(IEnumerable<MailRule> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<MailRule, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(MailRule entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var mailrules = await _context.MailRules.FindAsync(id, cancellationToken);

                if (mailrules == null)
                {
                    throw new MailRuleException($"MailRule with ID {id} not found."); // Or a custom exception type
                }

                _context.MailRules.Remove(mailrules);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MailRuleException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<MailRule> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<MailRule, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Check if any MailRule satisfies the predicate
            return await _context.MailRules.AnyAsync(predicate, cancellationToken);
        }

        public Task<IReadOnlyList<MailRule>> FindAsync(Expression<Func<MailRule, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<MailRule?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.MailRules
                                                .Include(d => d.Account)
                                                .FirstOrDefaultAsync(mr => mr.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MailRuleException(ex.Message.ToString());
            }
        }

        public Task<MailRule?> FindOneAsync(Expression<Func<MailRule, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<MailRule>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var MailRules = await _context.MailRules.ToListAsync(cancellationToken).ConfigureAwait(false);

            return MailRules;
        }
        public async Task<List<MailRule>> GetAllByOrderAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var mailRules = await _context.MailRules.Include(d => d.Account)
                    .OrderBy(rule => (int)rule.Order)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                // Use mailRules as needed...

                return mailRules;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving mail rules: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
        }

        public async Task<PagedList<MailRulePagedList>> GetAllByPagedAsync(MailRuleParameters mailruleparameters, Guid groupId, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();

            var query = _context.MailRules.Include(d => d.Account).Where(d => d.Account.GroupId == groupId).AsQueryable();
            foreach (var tag in query)
            {
                guids.Add(tag.Id);
            }
            int count = await query.CountAsync();

            var items = await query
                .Skip(mailruleparameters.PageSize * (mailruleparameters.Page - 1))
                .Take(mailruleparameters.PageSize)
                .ToListAsync();

            string serializedItems = serializer.Serialize(items);
            List<MailRulePagedList> deserializedItems1 = serializer.Deserialize<List<MailRulePagedList>>(serializedItems);
            return new PagedList<MailRulePagedList>(deserializedItems1, count, mailruleparameters.Page, mailruleparameters.PageSize, guids);

        }

        public async Task UpdateAsync(MailRule entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.MailRules.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    throw new MailRuleException($"MailAccount with ID {entity.Id} not found.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MailRuleException(ex.Message.ToString());
            }
        }


    }
}