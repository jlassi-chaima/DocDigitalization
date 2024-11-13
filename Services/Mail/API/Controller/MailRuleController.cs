using Application.Dtos.MailRule;
using Application.Features.FeaturesMailRule;
using Application.PaginationParams;
using Domain.MailRules;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controller
{
    public class MailRuleController : BaseApiController
    {
        [HttpPost("add_mailrule")]
        public async Task<IActionResult> AddMailRule([FromBody] MailRuleDto mailRuleDto)
        {
            try
            {

                var mailRule = await Mediator.Send(new AddMailRule.Command(mailRuleDto));

                return Ok(mailRule);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpGet("list_mailrule")]
        public async Task<IActionResult> ListMailruleAsync([FromQuery] MailRuleParameters mailruleparameters, [FromQuery] string? owner)
        {

            try
            {

                return Ok(await Mediator.Send(new ListMailRules.Query(mailruleparameters, owner)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpDelete("delete_mailrule/{id}")]
        public async Task<IActionResult> DeleteMailRuleAsync(Guid id)
        {
            try
            {

                await Mediator.Send(new DeleteMailRule.Command(id));

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpPut("update_mailrule/{id}")]
        public async Task<ActionResult<MailRule>> UpdateMailRuleAsync(MailRuleDto request, Guid id)
        {
            try
            {
                return await Mediator.Send(new UpdateMailRule.Command(request, id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
    }
}