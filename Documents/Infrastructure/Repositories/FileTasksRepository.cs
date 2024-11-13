using Application.Exceptions;
using Application.Respository;
using Domain.FileTasks;
using System.Linq.Expressions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Documents;
using System.Threading;

namespace Infrastructure.Repositories
{
    public class FileTasksRepository : IFileTasksRepository
    {
        private readonly DBContext _context;

        public FileTasksRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(FileTasks entity, CancellationToken cancellationToken = default)
        {
            try
            {
                 _context.Add(entity);
                 await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public Task AddRangeAsync(IEnumerable<FileTasks> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<FileTasks, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(FileTasks entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var group = await _context.FileTasks.Include(u => u.Task_document)
                                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                if (group == null)
                {
                    throw new FileTasksException($"FileTask with ID {id} not found.");
                }

                _context.FileTasks.Remove(group);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new FileTasksException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<FileTasks> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<FileTasks, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<FileTasks>> FindAsync(Expression<Func<FileTasks, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<FileTasks?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.FileTasks.Include(u => u.Task_document)
                                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new FileTasksException(ex.Message.ToString());
            }
        }

        public async Task<FileTasks> FindFileTaskByDocument(Document document)
        {
            try
            {
                return await _context.FileTasks
                                     .Include(ft => ft.Task_document) // Assuming Task_document is a navigation property
                                     .FirstOrDefaultAsync(ft => ft.Task_document.Id == document.Id);
            }
            catch (Exception ex)
            {
                throw new FileTasksException(ex.Message.ToString());
            }
        }

        public Task<FileTasks?> FindOneAsync(Expression<Func<FileTasks, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<FileTasks>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var filetasks = await _context.FileTasks
                .Include(u => u.Task_document)
                .ToListAsync(cancellationToken);

            return filetasks;
        }
        public Task UpdateAsync(FileTasks entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
