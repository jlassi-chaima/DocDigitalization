using Application.Dtos.GroupDTO;
using Application.Features.FeatureGroup;
using Domain.User;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class GroupController : BaseApiController
    {
        [HttpPost("Add_group")]
        public async Task<ActionResult<Groups>> AddGroupAsync(GroupDto request)
        {
            //var command = new AddGroup.Command(request);
            //var commandResponse = await Mediator.Send(new AddGroup.Command(request));

            return await Mediator.Send(new AddGroup.Command(request));
        }

        [HttpDelete("delete_group/{id}")]
        public async Task<IActionResult> DeleteGroupAsync(Guid id)
        {
            var command = new DeleteGroup.Command(id);
            await Mediator.Send(command);

            return NoContent();
        }
        [HttpPut("update_group/{id}")]
        public async Task<ActionResult<Groups>> UpdateGroupAsync(GroupDto request, Guid id)
        {
            var command = new UpdateGroup.Command(request, id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
        [HttpGet("list_groups")]
        public async Task<IActionResult> ListGroups()
        {
            var command = new ListGroup.Query();
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }
    }
}
