using Application.Dtos.UserDTO;
using Application.Repository;
using Domain.Settings;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using UserApp = Domain.User.ApplicationUser;
namespace Application.Features.FeatureUser
{
    public class AddUISettings
    {
        public sealed record Command : IRequest<UISettings>
        {
            public readonly RegisterDto registerDto;
            public readonly UserApp userApp;

            public Command(RegisterDto userRegistration,UserApp user)
            {
                registerDto = userRegistration;
                userApp = user;
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

        public sealed class Handler : IRequestHandler<Command, UISettings>
        {

            private readonly IUISettingsRepository _uiSettingsRepository;

            public Handler(IUISettingsRepository uiSettingsRepository)
            {
               
                _uiSettingsRepository = uiSettingsRepository;
            }

            public async Task<UISettings> Handle(Command request, CancellationToken cancellationToken)
            {
                try { 
                            var userRegistration = request.registerDto;
                            var passwordHasher = new PasswordHasher<UserApp>();
                           



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

                            //UserApp user = await _userRepository.FindByIdAsyncString(userToAdd.Id, cancellationToken);
                            UISettingsToAdd.User = request.userApp;

                            await _uiSettingsRepository.AddAsync(UISettingsToAdd, cancellationToken);


                            return UISettingsToAdd;
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
