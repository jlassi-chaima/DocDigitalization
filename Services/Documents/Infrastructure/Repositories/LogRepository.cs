using Application.Respository;
using Domain.Logs;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;


namespace Infrastructure.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly DBContext _context;
        
        public LogRepository(DBContext context)
        {
            _context = context;
           
        }
        public Task AddAsync(Logs entity, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new Logs());
        }
    

        public Task AddRangeAsync(IEnumerable<Logs> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<Logs, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Logs entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

      

        public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRangeAsync(IReadOnlyList<Logs> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<Logs, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

      

        public Task<IReadOnlyList<Logs>> FindAsync(Expression<Func<Logs, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

       

        public Task<Logs?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Logs?> FindOneAsync(Expression<Func<Logs, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

       
        public async Task<IReadOnlyList<Logs>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var logs = await _context.Logs.ToListAsync(cancellationToken).ConfigureAwait(false);

            return logs;
        }

        public async Task<List<Logs>> getLogsBySource(LogName source)
        {
            var logs = await _context.Logs.Where(d => d.Source == source).ToListAsync().ConfigureAwait(false);
            return logs;
        }

      

        public Task UpdateAsync(Logs entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
