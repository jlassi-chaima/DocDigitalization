

using Application.Dtos.MailAccount;
using Application.Exceptions;
using Application.PaginationParams;
using Application.Repository;
using DD.Core.Pagination;
using Domain.MailAccounts;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Serilog;


namespace Infrastructure.Repositories
{
    public class MailAccountRepository : IMailAccountRepository
    {
        private readonly DBContext _context;

        public MailAccountRepository(DBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MailAccount entity, CancellationToken cancellationToken = default)
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
        }

        public Task AddRangeAsync(IEnumerable<MailAccount> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<MailAccount, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(MailAccount entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var mailaccount = await _context.MailAccounts.FindAsync(id, cancellationToken);

                if (mailaccount == null)
                {
                    throw new MailAccountException($"MailAccount with ID {id} not found."); // Or a custom exception type
                }

                _context.MailAccounts.Remove(mailaccount);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MailAccountException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<MailAccount> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<MailAccount, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<MailAccount>> FindAsync(Expression<Func<MailAccount, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<MailAccount?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.MailAccounts.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MailAccountException(ex.Message.ToString());
            }
        }

        public async Task<MailAccount?> FindOneAsync(Expression<Func<MailAccount, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.MailAccounts.SingleOrDefaultAsync(predicate, cancellationToken);
        }
        public async Task<MailAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await FindOneAsync(account => account.Username == username, cancellationToken);
        }

        public async Task<IReadOnlyList<MailAccount>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var MailAccounts = await _context.MailAccounts.ToListAsync(cancellationToken).ConfigureAwait(false);

                return MailAccounts;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new MailAccountException(ex.Message.ToString());
            }
        }

        public async Task<PagedList<MailAccountPagedList>> GetAllByPagedAsync(MailAccountParameters mailacountparameters, Guid groupId, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();

            var query = _context.MailAccounts.Where(m => m.GroupId == groupId).AsQueryable();

            foreach (var tag in query)
            {
                guids.Add(tag.Id);
            }
            int count = await query.CountAsync();

            var items = await query
                .Skip(mailacountparameters.PageSize * (mailacountparameters.Page - 1))
                .Take(mailacountparameters.PageSize)
                .ToListAsync();

            string serializedItems = serializer.Serialize(items);
            List<MailAccountPagedList> deserializedItems1 = serializer.Deserialize<List<MailAccountPagedList>>(serializedItems);
            return new PagedList<MailAccountPagedList>(deserializedItems1, count, mailacountparameters.Page, mailacountparameters.PageSize, guids);
        }

        public async Task UpdateAsync(MailAccount entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.MailAccounts.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    throw new MailAccountException($"MailAccount with ID {entity.Id} not found.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MailAccountException(ex.Message.ToString());
            }
        }


    }
}
