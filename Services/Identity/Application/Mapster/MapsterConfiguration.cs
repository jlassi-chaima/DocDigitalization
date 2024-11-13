using Application.Dtos.UserDTO;
using Domain.Settings;
using Domain.User;
using Mapster;
using UserApp = Domain.User.ApplicationUser;
public static class MapsterConfiguration
{
    public static void Configure()
    {
      
        TypeAdapterConfig<UISettings, UserDto>.NewConfig()
            .Map(dest => dest.Id, src => Guid.Parse(src.User.Id))  
            .Map(dest => dest.UserName, src => src.User.UserName ?? string.Empty)
            .Map(dest => dest.Superuser_status, src => src.User.Superuser_status)
            .Map(dest => dest.FirstName, src => src.User.FirstName)
            .Map(dest => dest.LastName, src => src.User.LastName)

             .Map(dest => dest.Groups, src => src.User.Groups.Select(g => g.GroupID).ToList() ?? new List<Guid>())

            .Map(dest => dest.Permissions, src => src.User.Permissions ?? new List<string>());


    TypeAdapterConfig<UISettings, DarkModeSettingsDto>.NewConfig()
        .Map(dest => dest.Use_system, src => src.DarkMode_use_system)
        .Map(dest => dest.Enabled, src => src.DarkMode_enabled)
        .Map(dest => dest.Thumb_inverted, src => src.DarkMode_thumb_inverted);

        TypeAdapterConfig<UISettings, PermissionsDto>.NewConfig()
            .Map(dest => dest.Default_owner, src => Guid.Parse(src.User.Id)) 
            .Map(dest => dest.Default_view_users, src => src.Default_view_users.Select(Guid.Parse).ToList())
            .Map(dest => dest.Default_view_groups, src => src.Default_view_groups ?? new List<Guid>())
            .Map(dest => dest.Default_edit_users, src => src.Default_edit_users.Select(Guid.Parse).ToList()) 
            .Map(dest => dest.Default_edit_groups, src => src.Default_edit_groups ?? new List<Guid>());

        // UISettings -> SettingsDto
        TypeAdapterConfig<UISettings, SettingsDto>.NewConfig()
            .Map(dest => dest.Tour_complete, src => src.Tour_complete)
            .Map(dest => dest.DocumentListSize, src => src.DocumentListSize)
            .Map(dest => dest.Dark_mode, src => src.Adapt<DarkModeSettingsDto>()) 
            .Map(dest => dest.Notes_enabled, src => src.Notes_enabled)
            .Map(dest => dest.Saved_views, src => new { }) 
            .Map(dest => dest.Permissions, src => src.Adapt<PermissionsDto>()) 
            .Map(dest => dest.Language, src => src.Language ?? string.Empty);

        // UISettings -> ResultDto
        TypeAdapterConfig<UISettings, ResultDto>.NewConfig()
            .Map(dest => dest.User, src => src.Adapt<UserDto>()) 
            .Map(dest => dest.Settings, src => src.Adapt<SettingsDto>()) 
            .Map(dest => dest.Permissions, src => src.User.Permissions ?? new List<string>()); 

     
        TypeAdapterConfig<RegisterDto, UserApp>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Superuser_status, src => src.IsSuperUser)
            .Map(dest => dest.Active, src => src.IsActive)
            .Map(dest => dest.Groups, src => src.Groups ?? new List<Guid>()) 
            .Map(dest => dest.FirstName, src => src.FirstName ?? string.Empty)
            .Map(dest => dest.Permissions, src => src.Permissions ?? new List<string>())
            .Map(dest => dest.LastName, src => src.LastName ?? string.Empty);
        TypeAdapterConfig<ApplicationUser, UserList>.NewConfig()
        .Map(dest => dest.IsSuperUser, src => src.Superuser_status)
        .Map(dest => dest.PasswordHash, src => src.PasswordHash)


             .Map(dest => dest.Groups, src => src.Groups.Select(g => g.GroupID).ToList() ?? new List<Guid>())
       .Map(dest => dest.Permissions, src => src.Permissions.ToList());

    }

}