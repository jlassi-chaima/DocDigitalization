using Application.Dtos.DocumentType;
using Application.Features.FeaturesDocumentType;
using Application.Parameters;
using Domain.DocumentManagement.DocumentTypes;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;


namespace API.Controller
{
    [ApiController]
    public class DocumentTypeController : BaseApiController
    {
        [HttpPost("post_type")]
        public async Task<ActionResult<DocumentType>> AddTypeAsync(DocumentTypeDto request)
        {
            var command = new AddDocumentType.Command(request);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
        [HttpGet("get_type/{id}")]
        public async Task<ActionResult<DocumentType>> GetTypeAsync(Guid id)
        {
            var command = new DetailsDocumentType.Query(id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
       
        [HttpGet("list_types_dropdown")]
        public async Task<IActionResult> ListDocumentTypeDropDown([FromQuery] DocumentTypeParameters documenttypeparameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
        {
           
                var command = new ListDocumentTypeDropDown.Query(owner, documenttypeparameters);
                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);

        }
        [HttpGet("list_types")]
        public async Task<IActionResult> ListTypesAsync([FromQuery] DocumentTypeParameters documenttypeparameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
        {
            if(name_icontains == null)
            {
                var command = new ListDocumentType.Query(owner,documenttypeparameters);
                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);
            }
            else
            {
                var command = new FilterDocumentTypeByName.Query(name_icontains, documenttypeparameters, owner);
                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);
            }
           
        }
        [HttpDelete("delete_type/{id}")]
        public async Task<IActionResult> DeleteTypeAsync(Guid id)
        {
            var command = new DeleteDocumentType.Command(id);
            await Mediator.Send(command);

            return NoContent();
        }
        [HttpPut("update_type/{id}")]
        public async Task<ActionResult<DocumentType>> UpdateTypeAsync(DocumentTypeDto documenttype,Guid id)
        {
            var command = new UpdateDocumentType.Command(documenttype,id);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
    }
}
