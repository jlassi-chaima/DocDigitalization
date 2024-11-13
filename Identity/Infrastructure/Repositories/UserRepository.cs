//using Application.Dtos.UserDTO;
//using Application.Exception;
//using Application.Parameters;
//using Application.Repository;
//using DD.Core.Pagination;
//using Domain.User;
//using Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using System.Linq.Expressions;
//using System.Runtime.InteropServices;
//using System.Threading;

//namespace Infrastructure.Repositories
//{
//    public class UserRepository : IUserRepository
//    {
//        private readonly DBContext _context;

//        public UserRepository(DBContext context)
//        {
//            _context = context;
//        }
//        public async Task AddAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                await _context.AddAsync(entity, cancellationToken);
//                if (entity.Groups != null)
//                {
//                    foreach (var group in entity.Groups)
//                    {
//                        UserGroups usergroups = new UserGroups
//                        {
//                            UserID = entity.Id,

//                            GroupID = group.GroupID,

//                        };
//                        // Check if the UserGroups entity already exists
//                        var existingUserGroup = await _context.UserGroups
//                                                              .AsNoTracking()
//                                                              .FirstOrDefaultAsync(ug => ug.UserID == usergroups.UserID && ug.GroupID == usergroups.GroupID, cancellationToken);

//                        if (existingUserGroup == null)
//                        {
//                            // Only add if it does not exist
//                            await _context.UserGroups.AddAsync(usergroups, cancellationToken);
//                        }

//                    }
//                }
//                _context.SaveChanges();

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message.ToString());
//            }
//        }

//        public Task AddRangeAsync(IEnumerable<ApplicationUser> entities, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task DeleteAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task DeleteAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }
//        public async Task<ApplicationUser?> FindByIdAsyncString(string id, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await _context.Users.FindAsync(id, cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                throw new UserException(ex.Message.ToString());
//            }
//        }
//        public async Task DeleteByIdAsyncString(string id, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                var user = await _context.Users.FindAsync(id, cancellationToken);

//                if (user == null)
//                {
//                    throw new UserException($"User with ID {id} not found.");
//                }
//                var userRoles = _context.UserRoles.Where(ur => ur.UserId == id);
//                _context.UserRoles.RemoveRange(userRoles);
//                _context.Users.Remove(user);

//                await _context.SaveChangesAsync(cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                throw new UserException(ex.Message.ToString());
//            }
//        }

//        public Task DeleteRangeAsync(IReadOnlyList<ApplicationUser> entities, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public void Dispose()
//        {
//            _context?.Dispose();
//        }

//        public async Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
//        {
//            return await _context.Users.AnyAsync(predicate, cancellationToken);
//        }

//        public async Task<bool> ExistsByIdAsync(string userId, CancellationToken cancellationToken = default)
//        {
//            return await ExistsAsync(user => user.Id == userId, cancellationToken);
//        }
//        public Task<IReadOnlyList<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ApplicationUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ApplicationUser?> FindOneAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }
//        public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }



//        public async Task UpdateAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                var existingEntity = await _context.Users
//                                        .Include(u => u.Groups) // Ensure Groups are included
//                                        .FirstOrDefaultAsync(u => u.Id == entity.Id);

//                existingEntity.UserName = existingEntity.Email;
//                existingEntity.NormalizedEmail = existingEntity.Email.ToUpper();
//                existingEntity.NormalizedUserName = existingEntity.Email.ToUpper();


//                if (existingEntity == null)
//                {
//                    throw new UserException($"User with ID {entity.Id} not found.");
//                }

//                await _context.SaveChangesAsync(cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                throw new UserException(ex.Message.ToString());
//            }
//        }

//        public Task<PagedList<ListUserDto>> GetPagedtagAsync<ListUserDto>(UserParameters userparameters, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<ApplicationUser?> FindUserByMailAsyncString(string mail, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await _context.Users.FirstOrDefaultAsync(u => u.Email.Contains(mail), cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                throw new UserException(ex.Message);
//            }
//        }

//        public async Task<string?> GetUserRoleByID(string id, CancellationToken cancellationToken = default)
//        {
//            var userRole = await _context.UserRoles
//                      .Where(d => d.UserId == id)
//                      .FirstOrDefaultAsync(cancellationToken);

//            if (userRole == null)
//            {
//                return null; // Or handle the case where the user role is not found
//            }

//            var roleName = await _context.Roles
//                                .Where(r => r.Id == userRole.RoleId)
//                                .Select(r => r.Name)
//                                .FirstOrDefaultAsync(cancellationToken);

//            return roleName;
//        }

//        public async Task<List<string>> GetListGroupsByID(string id, CancellationToken cancellationToken = default)
//        {

//            var user = await _context
//                .Users.Where(u => u.Id == id)
//                  .FirstOrDefaultAsync(cancellationToken);
//            if (user != null)
//            {
//                var Group = await _context.UserGroups.Where(d => d.UserID == user.Id).Select(d=>d.GroupID.ToString()).ToListAsync(cancellationToken);

//                return Group;
//            }
//            else
//            {
//                throw new UserException($"User with ID {id} not found.");
//            }

//        }

//        public async Task<List<UserList>> GetListUserDtos(CancellationToken cancellationToken = default)
//        {
//            var users = await _context.Users.Include(d => d.Groups).Select(u => new UserList
//            {
//                Id = u.Id,
//                UserName = u.UserName,
//                Email = u.Email,
//                is_superuser = u.Superuser_status,
//                Groups = u.Groups.Select(g => g.GroupID).ToList(),
//                user_permissions = u.Permissions.ToList(),
//                passwordHash = u.PasswordHash,
//                ConfirmPassword = null, // This would typically not be set here
//                FirstName = u.FirstName,
//                LastName = u.LastName,

//            }).ToListAsync(cancellationToken);
//            return users;
//        }
//    }
//}
using Application.Dtos.UserDTO;
using Application.Exceptions;
using Application.Parameters;
using Application.Repository;
using DD.Core.Pagination;
using Domain.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DBContext _context;

        public UserRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
        {
            try
            {


                 _context.Add(entity);
                 await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                if (entity.Groups != null)
                {
                    foreach (var group in entity.Groups)
                    {
                        UserGroups usergroups = new UserGroups
                        {
                            UserID = entity.Id,
                            GroupID = group.GroupID,
                        };
                         _context.UserGroups.Add(usergroups);

                    }
                }
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public Task AddRangeAsync(IEnumerable<ApplicationUser> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<ApplicationUser?> FindByIdAsyncString(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message.ToString());
            }

        }
        public async Task DeleteByIdAsyncString(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _context.Users.FindAsync(id, cancellationToken);

                if (user == null)
                {
                    throw new UserException($"User with ID {id} not found.");
                }
                var userRoles = _context.UserRoles.Where(ur => ur.UserId == id);
                _context.UserRoles.RemoveRange(userRoles);
                _context.Users.Remove(user);

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<ApplicationUser> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await ExistsAsync(user => user.Id == userId, cancellationToken);
        }
        public Task<IReadOnlyList<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser?> FindOneAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }



        //public async Task UpdateAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var existingEntity = await _context.Users
        //                                .Include(u => u.Groups) // Ensure Groups are included
        //                                .FirstOrDefaultAsync(u => u.Id == entity.Id);

        //        existingEntity.UserName = existingEntity.Email;
        //        existingEntity.NormalizedEmail = existingEntity.Email.ToUpper();
        //        existingEntity.NormalizedUserName = existingEntity.Email.ToUpper();


        //        if (existingEntity == null)
        //        {
        //            throw new UserException($"User with ID {entity.Id} not found.");
        //        }


        //        await _context.SaveChangesAsync(cancellationToken);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new UserException(ex.Message.ToString());
        //    }
        //}

        public async Task UpdateAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create a temporary list to hold the new groups
                var newGroups = entity.Groups?.ToList();

                var existingEntity = await _context.Users
                                        .Include(u => u.Groups) // Ensure Groups are included
                                        .FirstOrDefaultAsync(u => u.Id == entity.Id);

                if (entity == null)
                {
                    throw new UserException($"User with ID {entity.Id} not found.");
                }

                // Update user properties
                entity.UserName = entity.Email;
                entity.NormalizedEmail = entity.Email.ToUpper();
                entity.NormalizedUserName = entity.Email.ToUpper();



                //// Clear existing groups
                //entity.Groups.Clear();
                existingEntity.Groups.Clear();

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);

                // Add new groups
                if (newGroups != null)
                {
                    foreach (var group in newGroups)
                    {
                        UserGroups userGroup = new UserGroups
                        {
                            UserID = entity.Id,
                            GroupID = group.GroupID,
                        };
                        await _context.UserGroups.AddAsync(userGroup);
                    }
                }
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message.ToString());
            }
        }


        public Task<PagedList<ListUserDto>> GetPagedtagAsync<ListUserDto>(UserParameters userparameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser?> FindUserByMailAsyncString(string mail, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email.Contains(mail), cancellationToken);
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        public async Task<string?> GetUserRoleByID(string id, CancellationToken cancellationToken = default)
        {
            var userRole = await _context.UserRoles
                      .Where(d => d.UserId == id)
                      .FirstOrDefaultAsync(cancellationToken);

            if (userRole == null)
            {
                return null; // Or handle the case where the user role is not found
            }

            var roleName = await _context.Roles
                                .Where(r => r.Id == userRole.RoleId)
                                .Select(r => r.Name)
                                .FirstOrDefaultAsync(cancellationToken);

            return roleName;
        }

        public async Task<List<string>> GetListGroupsByID(string id, CancellationToken cancellationToken = default)
        {

            var user = await _context
                .Users.Where(u => u.Id == id)
                  .FirstOrDefaultAsync(cancellationToken);
            if (user != null)
            {
                var Group = await _context.UserGroups.Where(d => d.UserID == user.Id).Select(d => d.GroupID.ToString()).ToListAsync(cancellationToken);

                return Group;
            }
            else
            {
                throw new UserException($"User with ID {id} not found.");
            }

        }
        public async Task<UserGroups> GetFirstGroupForUser(string userId, CancellationToken cancellationToken = default)
        {
            var group = await _context.UserGroups
                .Where(ug => ug.UserID == userId)
                .OrderBy(ug => ug.Group.Id) 
                .FirstOrDefaultAsync(cancellationToken);

            return group;
        }

        public async Task<List<ApplicationUser>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _context.Users.Include(d => d.Groups).ThenInclude(g=>g.Group).ToListAsync(cancellationToken);
                //    .Select(u => new UserList
                //{
                //    Id = u.Id,
                //    UserName = u.UserName,
                //    Email = u.Email,
                //    is_superuser = u.Superuser_status,
                //    //Groups = u.Groups.Select(g => g.GroupID).ToList(),
                //    Groups = u.Groups.Select(g => g.Group).ToList(), 
                //    user_permissions = u.Permissions.ToList(),
                //    passwordHash = u.PasswordHash,
                //    ConfirmPassword = null, // This would typically not be set here
                //    FirstName = u.FirstName,
                //    LastName = u.LastName,

                //})
                return users;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }

        public Task FindByIdAsyncd(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
