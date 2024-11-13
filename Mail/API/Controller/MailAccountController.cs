using Application.Dtos.MailAccount;
using Application.Features.FeaturesMailAccount;
using Application.PaginationParams;
using Domain.MailAccounts;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;


namespace API.Controller
{
    public class MailAccountController : BaseApiController
    {
        [HttpPost("add_MailAccount")]
        public async Task<IActionResult> AddMailAccount([FromBody] MailAccountDto mailAccountDto)
        {
            try
            {
                return Ok(await Mediator.Send(new AddMailAccount.Command(mailAccountDto)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");
            }

        }
        [HttpPost("test")]
        public async Task<IActionResult> TestMaiAccount([FromBody] MailAccountDto mailAccountDto)
        {

            try
            {


                return Ok(await Mediator.Send(new TestMailAccount.Command(mailAccountDto)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }

        }
        [HttpGet("list_MailAccount")]
        public async Task<IActionResult> ListMaiAccountAsync([FromQuery] MailAccountParameters mailaccountparameters, [FromQuery] string? owner)
        {
            try
            {

                return Ok(await Mediator.Send(new ListMailAccount.Query(mailaccountparameters, owner)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpPut("update_MailAccount/{id}")]
        public async Task<ActionResult<MailAccount>> UpdateMailAccountAsync(MailAccountDto request, Guid id)
        {
            try
            {


                return await Mediator.Send(new UpdateMailAccount.Command(request, id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }

        [HttpDelete("delete_MailAccount/{id}")]
        public async Task<IActionResult> DeleteMailAccountAsync(Guid id)
        {
            try
            {

                await Mediator.Send(new DeleteMailAccount.Command(id));

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }



    }
}
