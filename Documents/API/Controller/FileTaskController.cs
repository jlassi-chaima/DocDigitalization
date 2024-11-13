using Application.Features.FeaturesCustomField;
using Application.Features.FeaturesFileTasks;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class FileTaskController : BaseApiController
    {
        [HttpGet("list_file_tasks")]
        public async Task<IActionResult> ListFileTaskAsync()
        {
            var command = new ListFileTasks.Query();
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }

        //[HttpDelete("delete_file_task/{id}")]
        //public async Task<IActionResult> DeleteFileTaskAsync(Guid id, Guid doc_correspondent, string doc_owner)
        //{
        //    var command = new DeleFileTasks.Command(id, doc_owner, doc_correspondent);
        //    await Mediator.Send(command);

        //    return NoContent();
        //}
    }
}
