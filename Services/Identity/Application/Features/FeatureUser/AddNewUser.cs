using Application.Dtos.UserDTO;
using Application.Helper;
using Application.Repository;
using DD.Core.Constants;
using Domain.Settings;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserApp = Domain.User.ApplicationUser;
namespace Application.Features.FeatureUser
{
    public class AddNewUser
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
            public AddValidator()
            {
                RuleFor(p => p.registerDto.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Invalid Email");
                RuleFor(p => p.registerDto.UserName).NotEmpty().WithMessage("Username is required").EmailAddress().WithMessage("Invalid Username");
                // RuleFor(p => p.registerDto.passwordHash).NotEmpty();

            }
        }

        public sealed class Handler : IRequestHandler<Command, ResultDto>
        {

            private readonly IUISettingsRepository _uiSettingsRepository;
            public readonly UserManager<UserApp> _userManager;
            private readonly IPasswordHasher<UserApp> _passwordHasher;
   
            public Handler(IUISettingsRepository uiSettingsRepository, UserManager<UserApp> userManager,
            IPasswordHasher<UserApp> passwordHasher
           )
            {

                _uiSettingsRepository = uiSettingsRepository;
                _userManager = userManager;
                _passwordHasher = passwordHasher;
             
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
                           return uiSettings.Adapt<ResultDto>();
                        }
                    }
                    List<string> permissions = [
                                "add_document",
                                "view_document",
                                "change_document",
                                "delete_document",
                                "add_tag",
                                "view_tag",
                                "change_tag",
                                "delete_tag",
                                "add_correspondent",
                                "view_correspondent",
                                "change_correspondent",
                                "delete_correspondent",
                                "add_documenttype",
                                "view_documenttype",
                                "change_documenttype",
                                "delete_documenttype",
                                "add_storagepath",
                                "view_storagepath",
                                "change_storagepath",
                                "delete_storagepath",
                                "add_savedview",
                                "view_savedview",
                                "change_savedview",
                                "delete_savedview",
                                "add_paperlesstask",
                                "view_paperlesstask",
                                "change_paperlesstask",
                                "delete_paperlesstask",
                                "add_uisettings",
                                "view_uisettings",
                                "change_uisettings",
                                "delete_uisettings",
                                "add_note",
                                "view_note",
                                "change_note",
                                "delete_note",
                                "add_mailaccount",
                                "view_mailaccount",
                                "change_mailaccount",
                                "delete_mailaccount",
                                "add_mailrule",
                                "view_mailrule",
                                "change_mailrule",
                                "delete_mailrule",
                                "add_user",
                                "view_user",
                                "change_user",
                                "delete_user",
                                "add_group",
                                "view_group",
                                "change_group",
                                "delete_group",
                                "add_logentry",
                                "view_logentry",
                                "change_logentry",
                                "delete_logentry",
                                "add_sharelink",
                                "view_sharelink",
                                "change_sharelink",
                                "delete_sharelink",
                                "add_consumptiontemplate",
                                "view_consumptiontemplate",
                                "change_consumptiontemplate",
                                "delete_consumptiontemplate",
                                "add_customfield",
                                "view_customfield",
                                "change_customfield",
                                "delete_customfield"
                                 ];
                    var user = new UserApp
                    {
                        Id = request.registerDto.Id ?? Guid.NewGuid().ToString(),
                        FirstName = request.registerDto.FirstName,
                        LastName = request.registerDto.LastName,
                        UserName = request.registerDto.UserName,
                        Email = request.registerDto.Email,
                        Superuser_status = request.registerDto.IsSuperUser,
                      //  Groups = request.registerDto.Groups,
                        Active = request.registerDto.IsActive,
                        Permissions = request.registerDto.IsSuperUser ? permissions : request.registerDto.Permissions                                                                
                    };



                    user.PasswordHash = _passwordHasher.HashPassword(user, request.registerDto.PasswordHash !=null? request.registerDto.PasswordHash: PasswordGenerator.GenerateSecurePassword());


                    // Create user
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded) throw new System.Exception(result.Errors.ToString());

                    if (request.registerDto.IsSuperUser ||
                       (request.registerDto.Permissions != null && permissions.All(p => request.registerDto.Permissions.Contains(p))))
                    {
                        await _userManager.AddToRoleAsync(user, Roles.AdminRole);
                    }
                    await _userManager.AddToRoleAsync(user, Roles.DocumentManagerRole);


                    //add user settings 

                    UISettings UISettingsToAdd = new UISettings
                    {
                        Tour_complete = false,
                        DocumentListSize = 25,
                        DarkMode_use_system = false,
                        DarkMode_enabled = false,
                        DarkMode_thumb_inverted = false,
                        Notes_enabled = false,
                        Language = "fr-fr",
                        Default_view_users = [],
                        Default_view_groups = [],
                        Default_edit_users = [],
                        Default_edit_groups = [],
                    };

                    UISettingsToAdd.User = user;

                    await _uiSettingsRepository.AddAsync(UISettingsToAdd, cancellationToken);
                    if (Guid.TryParse(user.Id, out Guid userId))
                    {
                        UISettings uiSettings = await _uiSettingsRepository.FindByIdAsync(userId);
                        // UserApp user = await _userRepository.FindByIdAsyncString(request.Id.ToString());

                        if (uiSettings == null)
                        {
                            throw new System.Exception("Settings is  null");
                        }
                        return uiSettings.Adapt<ResultDto>();
                    }

                    throw new System.Exception("An error occurred. Please try again later.");
                }
                catch (System.Exception ex)
                {

                    Log.Error(ex.Message);
                    throw new System.Exception(ex.Message);
                }
            }
        }
    }
}
