using Application.Dtos.User;
using Application.Dtos.UserDTO;
using Application.Features.FeatureUser;
using Core.Domain.AllEntity;
using Core.Exceptions;
using DD.Core.Domain.AllEntity;
using Domain.User;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controller
{
    [ApiController]
    public class UserController : BaseApiController
    {
        [HttpPost("Registration")]
        public async Task<ActionResult<ResultDto>> AddUserAsync(RegisterDto request)
        {
            try
            {
                return await Mediator.Send(new AddUser.Command(request));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("list_user")]
        public async Task<IActionResult> ListUser()
        {
            try
            {

                return Ok(await Mediator.Send(new ListUser.Query()));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpPut("update_user/{id}")]
        public async Task<ActionResult<ApplicationUser>> UpdateUserAsync(UpdateUserDto request, string id)
        {
            try
            {

                return await Mediator.Send(new UpdateUser.Command(request, id));
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                throw new CustomException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }



        }
        [HttpDelete("delete_user/{id}")]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            try
            {
                await Mediator.Send(new DeleteUser.Command(id));

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("get_user_permissions")]
        public async Task<ActionResult<List<string>>> GetUserAsync([FromQuery] string id)
        {
            try
            {
                return await Mediator.Send(new GetUser.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("get_user_name")]
        public async Task<ActionResult<string>> GetNameUser([FromQuery] string id)
        {
            try
            {

                return await Mediator.Send(new GetNameUser.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("getuserbymail")]
        public async Task<ActionResult<string>> GetMailUser([FromQuery] string mail)
        {
            try
            {

                return await Mediator.Send(new FindUserByMail.Query(mail));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("verify_user_exist")]
        public async Task<ActionResult<bool>> GetUserInfosAsync([FromQuery] string id)
        {
            try
            {

                return await Mediator.Send(new VerifyUserExist.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("getuserrole")]
        public async Task<ActionResult<string>> GetUserRoleByID([FromQuery] string id)
        {
            try
            {
                return await Mediator.Send(new GetUserRoleByID.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("getlistGroups")]
        public async Task<ActionResult<List<string>>> GetListGroupsByID([FromQuery] string id)
        {
            try
            {

                return await Mediator.Send(new GetListGroupsByID.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("firstGroupForUser")]
        public async Task<ActionResult<string>> GetFirstGroupForUser([FromQuery] string id)
        {
            try
            {
                return await Mediator.Send(new GetFirstGroupForUser.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
    }
}
