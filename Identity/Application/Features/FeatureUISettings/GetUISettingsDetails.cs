using Application.Repository;
using Domain.User;
using MediatR;
using Newtonsoft.Json;
using Domain.Settings;

namespace Application.Features.FeatureUISettings
{
    public class GetUISettingsDetails
    {
        public sealed record Query : IRequest<string>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, string>
        {
            private readonly IUISettingsRepository _repository;
            private readonly IUserRepository _userRepository;

            public Handler(IUISettingsRepository repository, IUserRepository userRepository)
            {
                _repository = repository;
                _userRepository = userRepository;
            }

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                UISettings uiSettings = await _repository.FindByIdAsync(request.Id);
                ApplicationUser user = await _userRepository.FindByIdAsyncString(request.Id.ToString());


                var result = new
                {
                    user = new
                    {
                        id = user.Id, // Assuming you have UserId in UISettings
                        username = user.UserName, // Assuming you have Username in UISettings
                        is_superuser = user.Superuser_status, // Assuming you have IsSuperuser in UISettings
                        groups = user.Groups // Assuming you have Groups in UISettings
                    },
                    settings = new
                    {
                        tour_complete = uiSettings.Tour_complete,
                        documentListSize = uiSettings.DocumentListSize,
                        dark_mode = new
                        {
                            use_system = uiSettings.DarkMode_use_system,
                            enabled = uiSettings.DarkMode_enabled,
                            thumb_inverted = uiSettings.DarkMode_thumb_inverted
                        },
                        notes_enabled = uiSettings.Notes_enabled,
                        saved_views = new { },
                        permissions = new
                        {
                            default_owner = user.Id,
                            default_view_users = uiSettings.Default_view_users,
                            default_view_groups = uiSettings.Default_view_groups,
                            default_edit_users = uiSettings.Default_edit_users,
                            default_edit_groups = uiSettings.Default_edit_groups
                        },
                        language = uiSettings.Language
                    },
                    permissions= user.Permissions.ToList(),
                };



                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
        }

    }
}
