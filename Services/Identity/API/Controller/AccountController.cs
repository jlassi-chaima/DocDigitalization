using Application.Dtos.UserDTO;
using Application.Features.FeatureUser;
using Application.Helper;
using Domain.User;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UserApp = Domain.User.ApplicationUser;
namespace API.Controller
{
    public class AccountController : BaseApiController
    {
        
        public readonly UserManager<UserApp> _userManager;
        private readonly IPasswordHasher<UserApp> _passwordHasher;
        private readonly RoleManager<AppRole> _roleManager;
        public AccountController(UserManager<UserApp> userManager,
            IPasswordHasher<UserApp> passwordHasher,
            RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _roleManager = roleManager;
        }
    //    [AllowAnonymous]
    //    [HttpPost("register")]
    //    public async Task<ActionResult<string>> Register(RegisterDto registerDto)
    //    {
    //        try
    //        {

    //            return Ok(await Mediator.Send(new AddNewUser.Command(registerDto)));
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Error(ex, ex.Message);
    //            return BadRequest(ex.Message);
    //        }
    //    }
    //    [HttpPost("create-first-admin")]
    //    public async Task<IActionResult> CreateFirstAdmin([FromBody] RegisterDto registerDto)
    //    {
          
    //        if (await _userManager.FindByNameAsync(registerDto.Email) == null)
    //        {
    //            List<string> permissions = [
    //                            "add_document",
    //                            "view_document",
    //                            "change_document",
    //                            "delete_document",
    //                            "add_tag",
    //                            "view_tag",
    //                            "change_tag",
    //                            "delete_tag",
    //                            "add_correspondent",
    //                            "view_correspondent",
    //                            "change_correspondent",
    //                            "delete_correspondent",
    //                            "add_documenttype",
    //                            "view_documenttype",
    //                            "change_documenttype",
    //                            "delete_documenttype",
    //                            "add_storagepath",
    //                            "view_storagepath",
    //                            "change_storagepath",
    //                            "delete_storagepath",
    //                            "add_savedview",
    //                            "view_savedview",
    //                            "change_savedview",
    //                            "delete_savedview",
    //                            "add_paperlesstask",
    //                            "view_paperlesstask",
    //                            "change_paperlesstask",
    //                            "delete_paperlesstask",
    //                            "add_uisettings",
    //                            "view_uisettings",
    //                            "change_uisettings",
    //                            "delete_uisettings",
    //                            "add_note",
    //                            "view_note",
    //                            "change_note",
    //                            "delete_note",
    //                            "add_mailaccount",
    //                            "view_mailaccount",
    //                            "change_mailaccount",
    //                            "delete_mailaccount",
    //                            "add_mailrule",
    //                            "view_mailrule",
    //                            "change_mailrule",
    //                            "delete_mailrule",
    //                            "add_user",
    //                            "view_user",
    //                            "change_user",
    //                            "delete_user",
    //                            "add_group",
    //                            "view_group",
    //                            "change_group",
    //                            "delete_group",
    //                            "add_logentry",
    //                            "view_logentry",
    //                            "change_logentry",
    //                            "delete_logentry",
    //                            "add_sharelink",
    //                            "view_sharelink",
    //                            "change_sharelink",
    //                            "delete_sharelink",
    //                            "add_consumptiontemplate",
    //                            "view_consumptiontemplate",
    //                            "change_consumptiontemplate",
    //                            "delete_consumptiontemplate",
    //                            "add_customfield",
    //                            "view_customfield",
    //                            "change_customfield",
    //                            "delete_customfield"
    //                          ];
    //            var user = new UserApp
    //            {
    //                UserName = registerDto.Email,
    //                Email = registerDto.Email,
    //                Superuser_status = registerDto.Is_superuser,
    //                Active = registerDto.Is_active,
    //                FirstName = registerDto.First_name,
    //                LastName = registerDto.Last_name,
    //                Groups = registerDto.Groups,
    //                Permissions = permissions 
    //            };
    
    //            var result = await _userManager.CreateAsync(user, PasswordGenerator.GenerateSecurePassword());

    //            if (result.Succeeded)
    //            {
    //                await _userManager.AddToRoleAsync(user, "Admin");
    //                await _userManager.AddToRoleAsync(user, "DocumentManager");
    //                 await Mediator.Send(new AddUISettings.Command(registerDto, user));
    //                return Ok("Admin user created successfully.");
    //            }

    //            return BadRequest(result.Errors);
    //        }

    //        return BadRequest("Admin user already exists.");
    //    }
    }
}
//if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username || x.Email == registerDto.Email))
//{
//    var userExist = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == registerDto.Email);

//    if (Guid.TryParse(userExist.Id, out Guid
//        Id))
//    {
//        return Ok(await Mediator.Send(new GetUISettingsDetails.Query(Id)));
//    }
//}
//List<string> permissions = [
//               "add_document",
//                "view_document",
//                "change_document",
//                "delete_document",
//                "add_tag",
//                "view_tag",
//                "change_tag",
//                "delete_tag",
//                "add_correspondent",
//                "view_correspondent",
//                "change_correspondent",
//                "delete_correspondent",
//                "add_documenttype",
//                "view_documenttype",
//                "change_documenttype",
//                "delete_documenttype",
//                "add_storagepath",
//                "view_storagepath",
//                "change_storagepath",
//                "delete_storagepath",
//                "add_savedview",
//                "view_savedview",
//                "change_savedview",
//                "delete_savedview",
//                "add_paperlesstask",
//                "view_paperlesstask",
//                "change_paperlesstask",
//                "delete_paperlesstask",
//                "add_uisettings",
//                "view_uisettings",
//                "change_uisettings",
//                "delete_uisettings",
//                "add_note",
//                "view_note",
//                "change_note",
//                "delete_note",
//                "add_mailaccount",
//                "view_mailaccount",
//                "change_mailaccount",
//                "delete_mailaccount",
//                "add_mailrule",
//                "view_mailrule",
//                "change_mailrule",
//                "delete_mailrule",
//                "add_user",
//                "view_user",
//                "change_user",
//                "delete_user",
//                "add_group",
//                "view_group",
//                "change_group",
//                "delete_group",
//                "add_logentry",
//                "view_logentry",
//                "change_logentry",
//                "delete_logentry",
//                "add_sharelink",
//                "view_sharelink",
//                "change_sharelink",
//                "delete_sharelink",
//                "add_consumptiontemplate",
//                "view_consumptiontemplate",
//                "change_consumptiontemplate",
//                "delete_consumptiontemplate",
//                "add_customfield",
//                "view_customfield",
//                "change_customfield",
//                "delete_customfield"
//             ];
//var user = new UserApp
//{

//    FirstName = registerDto.FirstName,
//    LastName = registerDto.LastName,
//    UserName = registerDto.Username,
//    Email = registerDto.Email,
//    Superuser_status = registerDto.Superuser_status,
//    Groups = registerDto.Groups,
//    Active = registerDto.Active,
//    Permissions = (registerDto.Superuser_status && registerDto.Permissions == null)
//                                               ?
//                                               permissions : registerDto.Permissions,

//};



//user.PasswordHash = _passwordHasher.HashPassword(user, PasswordGenerator.GenerateSecurePassword());

//// Create user
//var result = await _userManager.CreateAsync(user);
//if (!result.Succeeded) return BadRequest(result.Errors);

//if (!string.IsNullOrEmpty(registerDto.Role) && registerDto.Role == Roles.AdminRole) { 
//    await _userManager.AddToRoleAsync(user, Roles.AdminRole); 
//}
//await _userManager.AddToRoleAsync(user, Roles.DocumentManagerRole);


//UISettings uiSettings = await Mediator.Send(new AddUISettings.Command(registerDto, user));   

//if (Guid.TryParse(user.Id, out Guid userId))
//{
//    return Ok(await Mediator.Send(new GetUISettingsDetails.Query(userId)));
//}
//throw new Exception("An error occurred. Please try again later.");