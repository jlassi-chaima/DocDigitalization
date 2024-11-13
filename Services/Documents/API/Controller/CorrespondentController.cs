using Application.Dtos.Correspondent;
using Application.Features.FeaturesCorrespondent;
using Application.Parameters;
using Domain.DocumentManagement.Correspondent;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controller
{
    [ApiController]
    public class CorrespondentsController : BaseApiController
    {
        [HttpPost("post_correspondent")]
        public async Task<ActionResult<Correspondent>> AddCorrespondentAsync(CorrespondentDto request)
        {
            try
            {
                await Mediator.Send(new AddCorrespondent.Command(request));
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }

        }
        [HttpGet("list_correspondent")]
        public async Task<IActionResult> ListCorrespondentAsync([FromQuery] CorrespondentParameters correspondentparameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
        {
            try
            {
                return Ok(await Mediator.Send(new ListCorrespondent.Query(owner, correspondentparameters, name_icontains)));


            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }

        }
        [HttpDelete("delete_correspondent/{id}")]
        public async Task<IActionResult> DeleteCorrespondentAsync(Guid id)
        {
            try
            {
                await Mediator.Send(new DeleteCorrespondent.Command(id));

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }

        }
        [HttpPut("update_correspondent/{id}")]
        public async Task<ActionResult<Correspondent>> UpdateCorrespondentAsync(CorrespondentDto request, Guid id)
        {
            try
            {
                
                return await Mediator.Send(new UpdateCorrespondent.Command(request, id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }

        }

    }
}
