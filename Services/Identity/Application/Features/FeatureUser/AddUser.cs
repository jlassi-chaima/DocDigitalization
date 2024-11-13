using Application.Dtos.User;
using Application.Dtos.UserDTO;
using Application.Helper;
using Application.Repository;
using DD.Core.Constants;
using Domain.Ports;
using Domain.Settings;
using Domain.User;
using FluentValidation;
using MailKit.Net.Smtp;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;

namespace Application.Features.FeatureUser
{
    public class AddUser
    {
        public sealed record Command : IRequest<ResultDto>
        {
            public readonly RegisterDto registerDto;

            public Command(RegisterDto userRegistration)
            {
                registerDto = userRegistration;
            }
        }

        public sealed class AddValidator : AbstractValidator<Command>
        {
            public AddValidator(IUserRepository _repository)
            {
                RuleFor(p => p.registerDto.Email).NotEmpty();
                //RuleFor(p => p.registerDto.passwordHash).NotEmpty();
                RuleFor(mr => mr.registerDto.Email).NotEmpty();
                    //.MustAsync(async (email, ct) => !await _repository.ExistsAsync(mr => mr.Email == email, ct))
                    //.WithMessage("Email must be unique.");
                //RuleFor(mr => mr.registerDto.passwordHash)
                   //.NotEmpty()
                   //.MinimumLength(6)
                   //.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                   //.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                   //.Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                   //.Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

            }
        }

        public sealed class Handler : IRequestHandler<Command, ResultDto>
        {
            private readonly IUserRepository _userRepository;
            private readonly IUISettingsRepository _uiSettingsRepository;
            private readonly IGroupRepository _groupRepository;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ISendEmailPort _sendEmailPort;
            //private readonly RoleManager<IdentityRole> _roleManager;



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
                "add_customfield", "view_customfield", "change_customfield", "delete_customfield"
            };
          

            public Handler(IUserRepository userRepository,
                IUISettingsRepository uiSettingsRepository,
                IGroupRepository groupRepository,
                UserManager<ApplicationUser> userManager, 
                RoleManager<IdentityRole> roleManager,
                ISendEmailPort sendEmailPort)
            {
                _userRepository = userRepository;
                _uiSettingsRepository = uiSettingsRepository;
                _groupRepository = groupRepository;
                _userManager = userManager;
                _sendEmailPort = sendEmailPort;
               //_roleManager = roleManager;
            }

            public async Task<ResultDto> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    if (await _userManager.Users.AnyAsync(x => x.UserName == request.registerDto.UserName || x.Email == request.registerDto.Email))
                     {
                        var userExist = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.registerDto.Email);

                        if (Guid.TryParse(userExist.Id, out Guid
                            Id))
                        {
                           
                            UISettings uiSettings = await _uiSettingsRepository.FindByIdAsync(Id);

                            if (uiSettings == null)
                            {
                                throw new System.Exception("Settings are null");
                            }
                            var result= uiSettings.Adapt<ResultDto>();
                            result.User.Roles= await _userManager.GetRolesAsync(userExist);
                            return result;
                        }
                    }
                    var userRegistration = request.registerDto;

                    var userToAdd = CreateUser(userRegistration);
                    if(request.registerDto.Groups != null)
                    {
                       await AddGroupsAndPermissionsAsync(userToAdd, userRegistration.Groups, cancellationToken);
                    }


                    var role = DetermineUserRole(userToAdd);

                    await SetPasswordAndCreateUserAsync(userToAdd, userRegistration?.PasswordHash, role);

                    //string body = ConfigureEmail.getBody(userRegistration?.PasswordHash);
                    //string subject = "Easydoc Account Creation";
                    //await _sendEmailPort.SendEmail(userRegistration.Email, userRegistration.FirstName, subject, body);

                    //await _userRepository.AddAsync(userToAdd, cancellationToken);

                    var uiSettingsAdded =await AddUserSettingsAsync(userToAdd, cancellationToken);

                    return uiSettingsAdded;
            }
                catch (Exception ex)
                {

                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
    }

            private  ApplicationUser CreateUser(RegisterDto userRegistration)
            {
                return new ApplicationUser
                {
                    Id = userRegistration.Id ?? Guid.NewGuid().ToString(),
                    Email = userRegistration.Email,
                    Superuser_status = userRegistration.IsSuperUser,
                    Active = userRegistration.IsActive,
                    Permissions = userRegistration.IsSuperUser ? Permissions : userRegistration.Permissions,
                    FirstName = userRegistration.FirstName,
                    LastName = userRegistration.LastName,
                    UserName = userRegistration.Email,
                    NormalizedUserName = userRegistration.Email.ToUpper(),
                    NormalizedEmail = userRegistration.Email.ToUpper(),
                   
                };

            }

            private async Task AddGroupsAndPermissionsAsync(ApplicationUser userToAdd, ICollection<Guid>? groups, CancellationToken cancellationToken)
            {
                try
                {

                   foreach (var groupId in groups)
                {
                    var getGroup = await _groupRepository.FindByIdAsync(groupId, cancellationToken);
                    if (userToAdd.Groups == null)
                    {
                        userToAdd.Groups = new List<UserGroups>();
                    }
                    UserGroups usergroups = new UserGroups
                    {
                        UserID = userToAdd.Id,
                        User = userToAdd,
                        GroupID = groupId,
                        Group = getGroup
                    };
                    userToAdd.Groups.Add(usergroups);
                    if (userToAdd.Permissions != null)
                    {
                        if (getGroup?.Permissions != null)
                        {
                            foreach (var permission in getGroup.Permissions.Where(permission => !userToAdd.Permissions.Contains(permission)))
                            {
                                userToAdd.Permissions.Add(permission);
                            }
                        }
                    }
                    else
                    {
                        if (getGroup?.Permissions != null)
                        {
                            foreach (var permission in getGroup.Permissions)

                                userToAdd.Permissions.Add(permission);
                        }

                    }
                        
                    }
                }
                 catch(Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
                    
                    
                
            }

            private string DetermineUserRole(ApplicationUser userToAdd)
            {
                // Determine if the user should be an Admin
                bool isAdmin = userToAdd.Superuser_status;
                if(isAdmin) userToAdd.Permissions = Permissions;
                return isAdmin ? "Admin" : "DocumentManager";
            }

            private async Task SetPasswordAndCreateUserAsync(ApplicationUser userToAdd, string password, string role)
            {
                try
                {

                    var passwordHasher = new PasswordHasher<ApplicationUser>();
                    userToAdd.PasswordHash = passwordHasher.HashPassword(userToAdd, password != null ? password : PasswordGenerator.GenerateSecurePassword());
                    // userToAdd.PasswordHash = passwordHasher.HashPassword(userToAdd, password);
                    //if (role == Roles.DocumentManagerRole)
                    //{
                        //userToAdd.Permissions = Permissions;
                    //}

                    var result = await _userManager.CreateAsync(userToAdd);
                    if (!result.Succeeded) throw new System.Exception(result.Errors.ToString());
                    
                   
                    await _userManager.AddToRoleAsync(userToAdd, role);

                }
                catch (Exception ex)
                {

                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
    }

           
            private async Task<ResultDto> AddUserSettingsAsync(ApplicationUser userToAdd, CancellationToken cancellationToken)
            {
                try
                {
                    UISettings UISettingsToAdd = new UISettings
                    {
                        Tour_complete = false,
                        DocumentListSize = 25,
                        DarkMode_use_system = false,
                        DarkMode_enabled = false,
                        DarkMode_thumb_inverted = false,
                        Notes_enabled = false,
                        Language = "English",
                        Default_view_users = [],
                        Default_view_groups = [],
                        Default_edit_users = [],
                        Default_edit_groups = [],
                        Settings = "",

                    };
                    //var user = await _userRepository.FindByIdAsyncString(userToAdd.Id, cancellationToken);
                    //if(user == null)
                    //{
                    //    throw new System.Exception("User is  null");
                    //}
                   UISettingsToAdd.User = userToAdd;
                    UISettingsToAdd.UserId = userToAdd.Id;
                    await _uiSettingsRepository.AddAsync(UISettingsToAdd, cancellationToken);
                    if (Guid.TryParse(userToAdd.Id, out Guid userId))
                    {
                        UISettings uiSettings = await _uiSettingsRepository.FindByIdAsync(userId);
                        // UserApp user = await _userRepository.FindByIdAsyncString(request.Id.ToString());

                        if (uiSettings == null)
                        {
                            throw new System.Exception("Settings is  null");
                        }
                        var result = uiSettings.Adapt<ResultDto>();
                        result.User.Roles = await _userManager.GetRolesAsync(userToAdd);
                        return result;
                    }

                    throw new System.Exception("An error occurred. Please try again later.");

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
               

            }
            
        }
    }
}
