using Application.Dtos.StoragePath;
using Application.Dtos.Templates;
using Application.Features.FeaturesStoragePath;
using Application.Features.FeaturesTemplates;
using Application.Parameters;
using DD.Core.Pagination;
using Domain.DocumentManagement.StoragePath;
using Domain.Templates;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class TemplateController : BaseApiController
    {
        [HttpDelete("delete_template/{id}")]
        public async Task<IActionResult> DeleteTemplateAsync(Guid id)
        {
            var command = new DeleteTemplate.Command(id);
            await Mediator.Send(command);

            return NoContent();
        }


        [HttpGet("list_templates")]
        public async Task<PagedList<PagedTemplate>> ListTemplateAsync([FromQuery] TemplateParametres templateparameters, [FromQuery] string owner)
        {
            Console.WriteLine(owner);
            var command = new ListTemplate.Query(templateparameters,owner);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }

        [HttpPost("add_template")]
        public async Task<ActionResult<Template>> AddTemplateAsync(TemplateDto request)
        {
            var command = new AddTemplate.Command(request);
            var commandResponse = await Mediator.Send(command);
            return Ok(commandResponse);
        }
        [HttpPut("update_template/{id}")]
        public async Task<ActionResult<Template>> UpdateTemplateAsync(TemplateDto request, Guid id)
        {
            var command = new UpdateTemplate.Command(request, id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
    }
}
