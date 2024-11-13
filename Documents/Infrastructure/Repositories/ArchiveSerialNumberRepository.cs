using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.DocumentType;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using Core.Exceptions;
using DD.Core.Pagination;
using Domain.Documents;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Buffers;
using Serilog;
using System.Linq.Expressions;
using System.Threading;

namespace Infrastructure.Repositories
{
    
    public class ArchiveSerialNumberRepository : IArchiveSerialNumberRepository
    {
        private readonly DBContext _context;

        public ArchiveSerialNumberRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ArchiveSerialNumbers archiveNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.ArchiveSerialNumbers.Add(archiveNumber);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());
            }
        
        }

        public Task AddRangeAsync(IEnumerable<ArchiveSerialNumbers> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<ArchiveSerialNumbers, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(ArchiveSerialNumbers archiveNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                

                _context.ArchiveSerialNumbers.Remove(archiveNumber);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());
            }
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var archiveNumber = await _context.ArchiveSerialNumbers.FindAsync(id, cancellationToken);

                if (archiveNumber == null)
                {
                    throw new Exception($"Archive Serial Number with ID {id} not found.");
                }

                _context.ArchiveSerialNumbers.Remove(archiveNumber);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<ArchiveSerialNumbers> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public void DoWork()
        {
        }

        public Task<bool> ExistsAsync(Expression<Func<ArchiveSerialNumbers, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ArchiveSerialNumbers>> FindAsync(Expression<Func<ArchiveSerialNumbers, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ArchiveSerialNumbers?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ArchiveSerialNumbers.FirstOrDefaultAsync(a=>a.Id==id);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());
            }
        }
        public async Task<ArchiveSerialNumbers?> GetArchiveNumberByGroupIdAsync(string idOwner,Guid groupId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ArchiveSerialNumbers.FirstOrDefaultAsync(a => a.GroupId == groupId && a.Owner== idOwner);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());
            }
        }

        public Task<ArchiveSerialNumbers?> FindOneAsync(Expression<Func<ArchiveSerialNumbers, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ArchiveSerialNumbers>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(ArchiveSerialNumbers entity, CancellationToken cancellationToken = default)
        {
            
            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception($"An error occurred while updating the entity.Error message: {ex.Message}");
            }
        }
        public async Task<PagedList<ArchiveSerialNumberDto>> GetArchiveSerialNumberNameAsync<ArchiveSerialNumberDto>(ArchiveSerialNumberParameters archiveSerialNumberParameters, string owner,string?  nameIcontains)
        {
            try
            {
                List<Guid> guids = new List<Guid>();
                NewtonSoftService serializer = new NewtonSoftService();
                var query = _context.ArchiveSerialNumbers
              .Where(a => a.Owner == owner);

                if (!string.IsNullOrEmpty(nameIcontains))
                {
                    string[] parts = nameIcontains.Split('-');
                    string prefix = parts.Length > 0 ? parts[0] : string.Empty;
                    string groupName = parts.Length > 1 ? parts[1] : string.Empty;
                    string year = parts.Length > 2 ? parts[2] : string.Empty;

                    query = query.Where(a =>
                            (string.IsNullOrEmpty(prefix) || a.Prefix.ToLower().Contains(prefix)) &&
                            (string.IsNullOrEmpty(groupName) || a.GroupName.ToLower().Contains(groupName)) &&
                            (string.IsNullOrEmpty(year) || a.Year.Year.ToString().Contains(year))
                        );
                }
                //foreach (var archive in query)
                //{
                //    guids.Add(archive.Id);
                //}
                // Count the total number of archive
                int count = await query.CountAsync();
                // Get the paged archive
                var items = await query.Skip(archiveSerialNumberParameters.PageSize * (archiveSerialNumberParameters.Page - 1))
                                       .Take(archiveSerialNumberParameters.PageSize)
                                       .ToListAsync();
                // Serialize the paged archive to a JSON string
                string serializedItems = serializer.Serialize(items);
                // Deserialize the JSON string to a list of archive
                List<ArchiveSerialNumberDto> deserializedlist = serializer.Deserialize<List<ArchiveSerialNumberDto>>(serializedItems);
                return new PagedList<ArchiveSerialNumberDto>(deserializedlist, count, archiveSerialNumberParameters.Page, archiveSerialNumberParameters.PageSize, guids);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());

            }
        }

        public async Task<ArchiveSerialNumbers> GetArchiveNumberByGroupId(Guid groupId)
        {
            try
            {
                return await _context.ArchiveSerialNumbers.FirstOrDefaultAsync(a => a.GroupId == groupId);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message.ToString());
            }
        }

        //public async  Task<ArchiveSerialNumbers> GetArchiveNumberByGroupId(string idOwner, Guid groupId)
        //{
        //    try
        //    {
        //        return await _context.ArchiveSerialNumbers.FirstOrDefaultAsync(a => a.GroupId == groupId && a.Owner == idOwner);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message);
        //        throw new Exception(ex.Message.ToString());
        //    }
        //}
    }
}
