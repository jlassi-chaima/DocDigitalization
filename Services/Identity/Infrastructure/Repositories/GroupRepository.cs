using Application.Repository;
using Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Application.Exceptions;


namespace Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DBContext _context;

        public GroupRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Groups entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.AddAsync(entity, cancellationToken);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                ///Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());
            }
        }

        public Task AddRangeAsync(IEnumerable<Groups> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<Groups, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Groups entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var group = await _context.Groups.Include(u => u.Users)
                                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                if (group == null)
                {
                    throw new GroupException($"Group with ID {id} not found."); // Or a custom exception type
                }

                _context.Groups.Remove(group);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<Groups> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<Groups, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Groups>> FindAsync(Expression<Func<Groups, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Groups?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Groups.Include(u => u.Users)
                                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new GroupException(ex.Message.ToString());
            }
        }

        public Task<Groups?> FindOneAsync(Expression<Func<Groups, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Groups>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var groups = await _context.Groups.ToListAsync(cancellationToken);

            return groups;
        }


        public async Task UpdateAsync(Groups entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.Groups.Include(u => u.Users) // Ensure Groups are included
                                        .FirstOrDefaultAsync(u => u.Id == entity.Id);

                if (existingEntity == null)
                {
                    throw new GroupException($"Group with ID {entity.Id} not found.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new GroupException(ex.Message.ToString());
            }
        }
    }
}
