//using Application.Dtos.User;
//using Application.Dtos.UserDTO;
//using Application.Repository;
//using Domain.User;
//using MapsterMapper;
//using MediatR;
//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.Features.FeatureUser
//{
//    public class UpdateUser
//    {
//        public sealed record Command : IRequest<ApplicationUser>
//        {
//            public readonly string userId;
//            public readonly UpdateUserDTO Usertoupdate;

//            public Command(UpdateUserDTO usertoupdate, string id)
//            {
//                Usertoupdate = usertoupdate;
//                userId = id;
//            }
//        }

//        public sealed class Handler : IRequestHandler<Command, ApplicationUser>
//        {
//            private readonly IUserRepository _repository;
//            private readonly IMapper _mapper;
//            private readonly IGroupRepository _groupRepository;
//            private readonly UserManager<ApplicationUser> _userManager;
//            private readonly RoleManager<IdentityRole> _roleManager;

//            private static readonly List<string> Permissions = new()
//            {
//                "add_document", "view_document", "change_document", "delete_document",
//                "add_tag", "view_tag", "change_tag", "delete_tag",
//                "add_correspondent", "view_correspondent", "change_correspondent", "delete_correspondent",
//                "add_documenttype", "view_documenttype", "change_documenttype", "delete_documenttype",
//                "add_storagepath", "view_storagepath", "change_storagepath", "delete_storagepath",
//                "add_savedview", "view_savedview", "change_savedview", "delete_savedview",
//                "add_paperlesstask", "view_paperlesstask", "change_paperlesstask", "delete_paperlesstask",
//                "add_uisettings", "view_uisettings", "change_uisettings", "delete_uisettings",
//                "add_note", "view_note", "change_note", "delete_note",
//                "add_mailaccount", "view_mailaccount", "change_mailaccount", "delete_mailaccount",
//                "add_mailrule", "view_mailrule", "change_mailrule", "delete_mailrule",
//                "add_user", "view_user", "change_user", "delete_user",
//                "add_group", "view_group", "change_group", "delete_group",
//                "add_logentry", "view_logentry", "change_logentry", "delete_logentry",
//                "add_sharelink", "view_sharelink", "change_sharelink", "delete_sharelink",
//                "add_consumptiontemplate", "view_consumptiontemplate", "change_consumptiontemplate", "delete_consumptiontemplate",
//                "add_customfield", "view_customfield", "change_customfield", "delete_customfield"
//            };
//            public Handler(IUserRepository repository, IMapper mapper, IGroupRepository groupRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
//            {
//                _repository = repository;
//                _mapper = mapper;
//                _groupRepository = groupRepository;
//                _userManager = userManager;
//                _roleManager = roleManager;
//            }

//            public async Task<ApplicationUser> Handle(Command request, CancellationToken cancellationToken)
//            {
//                ApplicationUser theusertoupdate = _repository.FindByIdAsyncString(request.userId, cancellationToken).GetAwaiter().GetResult();

//                theusertoupdate.Superuser_status = request.Usertoupdate.is_superuser;
//                theusertoupdate.Permissions = request.Usertoupdate.user_permissions;
//                theusertoupdate.FirstName = request.Usertoupdate.firstName;
//                theusertoupdate.LastName = request.Usertoupdate.lastName;

//                // _mapper.Map(request.Usertoupdate, theusertoupdate);

//                // theusertoupdate.Groups.Clear();
//                if (request.Usertoupdate.Groups != null)
//                {
//                    foreach (var group in request.Usertoupdate.Groups)
//                    {
//                        Groups Foundgroup = _groupRepository.FindByIdAsync(group, cancellationToken).GetAwaiter().GetResult();
//                        if (theusertoupdate.Groups == null)
//                        {
//                            theusertoupdate.Groups = new List<UserGroups>();
//                        }

//                            UserGroups usergroups = new UserGroups
//                        {
//                            UserID = theusertoupdate.Id,
//                            User = theusertoupdate,
//                            GroupID = Foundgroup.Id,
//                            Group = Foundgroup
//                        };
//                        if (!theusertoupdate.Groups.Any(d => d.GroupID == usergroups.GroupID) )
//                        {

//                            theusertoupdate.Groups.Add(usergroups);
//                        }
//                    }
//                }

//                if (!theusertoupdate.Superuser_status)
//                {
//                    if (request.Usertoupdate.Groups != null)
//                    {
//                        foreach (var group in request.Usertoupdate.Groups)
//                        {
//                            Groups Foundgroup = _groupRepository.FindByIdAsync(group, cancellationToken).GetAwaiter().GetResult();


//                            //when we add a user to a group, he should take all the permissions of the group he's in
//                            //check if the group permissions exist in the user permissions, else add them 
//                            if (theusertoupdate.Permissions != null)
//                            {
//                                bool hasMissingPermissions = Foundgroup.Permissions.Any(permission => !theusertoupdate.Permissions.Contains(permission));
//                                if (hasMissingPermissions)
//                                {
//                                    foreach (var permission in Foundgroup.Permissions)
//                                    {
//                                        if (!theusertoupdate.Permissions.Contains(permission))
//                                        {
//                                            theusertoupdate.Permissions.Add(permission);
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }


//                // Determine the role based on permissions
//                string role = DetermineUserRole(theusertoupdate);

//                // Add or update user role if necessary
//                await UpdateUserRoleAsync(theusertoupdate, role);


//                await _repository.UpdateAsync(theusertoupdate);
//                return theusertoupdate;
//            }
//            private string DetermineUserRole(ApplicationUser user)
//            {
//                // Set permissions based on SuperuserStatus
//                if (user.Superuser_status)
//                {
//                    user.Permissions = Permissions;
//                }


//                // Determine if the user should be an Admin
//                bool isAdmin = user.Superuser_status;

//                return isAdmin ? "Admin" : "DocumentManager";
//            }

//            private async Task UpdateUserRoleAsync(ApplicationUser user, string role)
//            {
//                try
//                {
//                    IList<string> existingRoles = await _userManager.GetRolesAsync(user);
//                    if (!existingRoles.Contains(role))
//                    {
//                        await _userManager.RemoveFromRolesAsync(user, existingRoles);
//                        await _userManager.AddToRoleAsync(user, role);
//                    }

//                }
//                catch (System.Exception e)
//                {
//                    throw new System.Exception(e.Message);


//                }
//            }
//        }

//    }
//}
using Application.Dtos.User;
using Application.Dtos.UserDTO;
using Application.Helper;
using Application.Repository;
using Core.Exceptions;
using Domain.Ports;
using Domain.User;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureUser
{
    public class UpdateUser
    {
        public sealed record Command : IRequest<ApplicationUser>
        {
            public readonly string userId;
            public readonly UpdateUserDto UserToUpdate;

            public Command(UpdateUserDto usertoupdate, string id)
            {
                UserToUpdate = usertoupdate;
                userId = id;
            }
        }

        public sealed class Handler : IRequestHandler<Command, ApplicationUser>
        {
            private readonly IUserRepository _repository;
            private readonly IMapper _mapper;
            private readonly IGroupRepository _groupRepository;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ISendEmailPort _sendEmailPort;

            private static readonly List<string> Permissions = new()
            {
                "add_document", "view_document", "change_document", "delete_document",
                "add_tag", "view_tag", "change_tag", "delete_tag",
                "add_correspondent", "view_correspondent", "change_correspondent", "delete_correspondent",
                "add_documenttype", "view_documenttype", "change_documenttype", "delete_documenttype",
                "add_storagepath", "view_storagepath", "change_storagepath", "delete_storagepath",
                "add_savedview", "view_savedview", "change_savedview", "delete_savedview",
                "add_task", "view_task", "change_task", "delete_task",
                "add_uisettings", "view_uisettings", "change_uisettings", "delete_uisettings",
                "add_note", "view_note", "change_note", "delete_note",
                "add_mailaccount", "view_mailaccount", "change_mailaccount", "delete_mailaccount",
                "add_mailrule", "view_mailrule", "change_mailrule", "delete_mailrule",
                "add_user", "view_user", "change_user", "delete_user",
                "add_group", "view_group", "change_group", "delete_group",
                "add_logentry", "view_logentry", "change_logentry", "delete_logentry",
                "add_sharelink", "view_sharelink", "change_sharelink", "delete_sharelink",
                "add_consumptiontemplate", "view_consumptiontemplate", "change_consumptiontemplate", "delete_consumptiontemplate",
                "add_customfield", "view_customfield", "change_customfield", "delete_customfield",
                 "add_fileshare", "view_fileshare", "change_fileshare", "delete_fileshare"
            };
            public Handler(IUserRepository repository, 
                           IMapper mapper,
                           IGroupRepository groupRepository, 
                           UserManager<ApplicationUser> userManager, 
                           RoleManager<IdentityRole> roleManager,
                           ISendEmailPort sendEmailPort)
            {
                _repository = repository;
                _mapper = mapper;
                _groupRepository = groupRepository;
                _userManager = userManager;
                _roleManager = roleManager;
                _sendEmailPort = sendEmailPort;
            }

            public async Task<ApplicationUser> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    ApplicationUser userToUpdate = await _repository.FindByIdAsyncString(request.userId, cancellationToken);
                    if (userToUpdate == null)
                    {
                        throw new CustomException("User to udate nit Found");
                    }
                    UpdateUserProperties(userToUpdate, request.UserToUpdate);
                    await HandlePasswordUpdate(userToUpdate, request.UserToUpdate.PasswordHash, request.UserToUpdate.Email, request.UserToUpdate.FirstName);
                    List<Guid> groupsIds = request?.UserToUpdate?.Groups?.ToList() ?? new List<Guid>() ;
                    await UpdateUserGroups(userToUpdate, groupsIds, cancellationToken);
                    await UpdateUserPermissions(userToUpdate, groupsIds, cancellationToken);

                    await _repository.UpdateAsync(userToUpdate);
                    return userToUpdate;
                }
                catch (CustomException ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
            }
            private void UpdateUserProperties(ApplicationUser userToUpdate, UpdateUserDto updateDto)
            {
                userToUpdate.Superuser_status = updateDto.IsSuperUser;
                userToUpdate.Permissions = updateDto.Permissions;
                userToUpdate.FirstName = updateDto.FirstName;
                userToUpdate.LastName = updateDto.LastName;
                userToUpdate.Email = updateDto.Email;
                userToUpdate.Active = updateDto.IsActive;
                DetermineUserRole(userToUpdate);
            }

            private async Task HandlePasswordUpdate(ApplicationUser userToUpdate, string newPasswordHash, string email, string firstName)
            {
                if (userToUpdate.PasswordHash != newPasswordHash)
                {
                    string body = ConfigureEmail.getBody(newPasswordHash);
                    string subject = "Easydoc Account Updating";
                    await _sendEmailPort.SendEmail(email, firstName, subject, body);

                    var passwordHasher = new PasswordHasher<ApplicationUser>();
                    userToUpdate.PasswordHash = passwordHasher.HashPassword(userToUpdate, newPasswordHash);
                }
            }

            private async Task UpdateUserGroups(ApplicationUser userToUpdate, List<Guid> groupIds, CancellationToken cancellationToken)
            {
                var userGroups = new List<UserGroups>();

                if (groupIds != null)
                {
                    foreach (var groupId in groupIds)
                    {
                        var group = await _groupRepository.FindByIdAsync(groupId, cancellationToken);
                        if (group != null)
                        {
                            var userGroup = new UserGroups
                            {
                                UserID = userToUpdate.Id,
                                User = userToUpdate,
                                GroupID = group.Id,
                                Group = group
                            };
                            userGroups.Add(userGroup);
                        }
                    }
                }

                userToUpdate.Groups = userGroups;
            }

            private async Task UpdateUserPermissions(ApplicationUser userToUpdate, List<Guid> groupIds, CancellationToken cancellationToken)
            {
                if (!userToUpdate.Superuser_status)
                {
                    if (groupIds != null)
                    {
                        foreach (var groupId in groupIds)
                        {
                            var group = await _groupRepository.FindByIdAsync(groupId, cancellationToken);
                            if (group?.Permissions != null)
                            {
                                if (userToUpdate.Permissions == null)
                                {
                                    userToUpdate.Permissions = new List<string>();
                                }

                                foreach (var permission in group.Permissions.Where(permission => !userToUpdate.Permissions.Contains(permission)))
                                {
                                    userToUpdate.Permissions.Add(permission);
                                }
                            }
                        }
                    }
                }
            }
            private string DetermineUserRole(ApplicationUser user)
            {
                // Set permissions based on SuperuserStatus
                if (user.Superuser_status)
                {
                    user.Permissions = Permissions;
                }


                // Determine if the user should be an Admin
                bool isAdmin = user.Superuser_status;

                return isAdmin ? "Admin" : "DocumentManager";
            }

            private async Task UpdateUserRoleAsync(ApplicationUser user, string role)
            {
                try
                {
                    IList<string> existingRoles = await _userManager.GetRolesAsync(user);
                    if (!existingRoles.Contains(role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, existingRoles);
                        await _userManager.AddToRoleAsync(user, role);
                    }

                }
                catch (System.Exception e)
                {
                    throw new System.Exception(e.Message);


                }
            }
        }

    }
}
