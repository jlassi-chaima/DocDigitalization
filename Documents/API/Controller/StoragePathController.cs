using Application.Dtos.StoragePath;
using Application.Features.FeaturesStoragePath;
using Application.Parameters;
using Domain.DocumentManagement.StoragePath;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class StoragePathController : BaseApiController
    {
        //[Authorize]
        [HttpPost("add_storage_path")]
        public async Task<ActionResult<StoragePath>> AddStoragePathAsync(UpdateStoragePathDto request)
        {
            var command = new AddStoragePath.Command(request);
            var commandResponse = await Mediator.Send(command);
            return Ok(commandResponse);
        }

        [HttpDelete("delete_storage_path/{id}")]
        public async Task<IActionResult> DeleteTagAsync(Guid id)
        {
            var command = new DeleteStoragePath.Command(id);
            await Mediator.Send(command);

            return NoContent();
        }
        
       
        [HttpGet("list_storage_paths_dropdown")]
        public async Task<ActionResult<List<StoragePath>>> ListStoragePathDropDown([FromQuery] StoragePathParameters storagepathparameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
        {
            
                var command = new ListStoragePathDropDown.Query(owner, storagepathparameters);
                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);
            

        }
        [HttpGet("list_storage_paths")]
        public async Task<ActionResult<List<StoragePathDto>>> ListTagsTagAsync([FromQuery] StoragePathParameters storagepathparameters, [FromQuery] string? owner,[FromQuery] string? name_icontains)
        {
            if(name_icontains== null)
            {
                var command = new ListStoragePath.Query(owner,storagepathparameters);
                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);
            }
            else
            {
                var command = new FilterStoragePathByName.Query(name_icontains,storagepathparameters, owner);
                var commandResponse = await Mediator.Send(command);
                return Ok(commandResponse);
            }
          
        }
        [HttpGet("get_storage_path_by_id/{id}")]
        public async Task<ActionResult<StoragePath>> GetTagAsync(Guid id)
        {
            var command = new DetailsStoragePath.Query(id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }

        [HttpPut("update_storage_path/{id}")]
        public async Task<ActionResult<StoragePath>> UpdateTagAsync(UpdateStoragePathDto request, Guid id)
        {
            
            var command = new UpdateStoragePath.Command(request, id);
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }
    }
}
