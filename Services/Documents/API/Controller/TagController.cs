using Application.Dtos.Tag;
using Application.Features.FeaturesTag;
using Application.Parameters;
using Domain.DocumentManagement.tags;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class TagController: BaseApiController
    {
        [HttpGet("list_tags")]
        public async Task<IActionResult> ListTagsTagAsync([FromQuery] TagParameters tagparameters, [FromQuery] string? owner ,[FromQuery] string? name_icontains)
        {
            if (name_icontains == null)
            {
                var command = new ListTag.Query(owner,tagparameters);
                var commandResponse = await Mediator.Send(command);
                return Ok(commandResponse);
            }
            else
            {
                var command = new FilterbyNameTag.Query(name_icontains, tagparameters,owner);
                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);
            }

         
        }
        [HttpGet("list_tagsdropdown")]
        public async Task<IActionResult> ListTagDropDown([FromQuery] TagParameters tagparameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
        {
           
                var command = new ListTagDropDown.Query(owner, tagparameters);
                var commandResponse = await Mediator.Send(command);
                return Ok(commandResponse);
           
           

        }
        
        [HttpGet("get_tag/{id}")]
        public async Task<ActionResult<Tag>> GetTagAsync(Guid id)
        {
            var command = new DetailsTag.Query(id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
        //[HttpGet]
        //public async Task<IActionResult> ListTagsTagAsync([FromQuery] string? name_icontains)
        //{
        //    var command = new FilterbyNameTag.Query(name_icontains);
        //    var commandResponse = await Mediator.Send(command);

        //    return Ok(commandResponse);
        //}
        [HttpPost("post_tag")]
        public async Task<ActionResult<Tag>> AddTagAsync(TagDto request)
        {

            var command = new AddTag.Command(request);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
        [HttpDelete("delete_tag/{id}")]
        public async Task<IActionResult> DeleteTagAsync(Guid id)
        {
            var command = new DeleteTag.Command(id);
            await Mediator.Send(command);

            return NoContent();
        }

        [HttpPut("update_tag/{id}")]
        public async Task<ActionResult<Tag>> UpdateTagAsync(TagDto request, Guid id)
        {
            var command = new UpdateTag.Command(request, id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
    }
}
