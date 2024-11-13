using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.CustomField;
using Application.Exceptions;
using Application.Features.ArchiveSerialNumbersFeature;
using Application.Features.FeaturesCustomField;
using Application.Parameters;
using Core.Exceptions;
using Domain.Documents;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TT.Internet.Framework.Infrastructure.Filters;

namespace API.Controller
{
    [ApiController]
    public class ArchiveSerialNumberController : BaseApiController
    {
        [HttpPost("post_archive")]
        [ServiceFilter(typeof(ValidationFilter<ArchiveSerialNumberDto>))]
        public async Task<ActionResult<ArchiveSerialNumbers>> AddArchive(ArchiveSerialNumberDto request)
        {
            try {
                return await Mediator.Send(new AddArchiveSerialNumber.Command(request));
            }
            catch (ArchiveException ex)
            {
                Log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");


            }
        }
        [HttpGet("list_archive")]
        public async Task<IActionResult> ListArchiveDropDown([FromQuery] ArchiveSerialNumberParameters archiveSerialNumberParameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
        {
           try{

             return Ok(await Mediator.Send(new ArchiveSerialNumberPagedList.Query(archiveSerialNumberParameters, owner, name_icontains)));
            }
           catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }

        }
        [HttpGet("get_archive/{id}")]
        public async Task<ActionResult<ArchiveSerialNumbers>> GetArchive(Guid id)
        {
            try
            {

                return await Mediator.Send(new DetailsArchiveSerialNumber.Query(id));
            }
            catch (NotFoundException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpDelete("delete_archive/{id}")]
        public async Task<IActionResult> DeleteArchive(Guid id)
        {
            try
            {
                await Mediator.Send(new DeleteArchiveSerialNumber.Command(id));

                return NoContent();
            }
            catch (NotFoundException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpPut("update_archive/{id}")]
        public async Task<ActionResult<ArchiveSerialNumbers>> UpdateCustomFieldAsync(ArchiveSerialNumberDto request, Guid id)
        {
            try
            {

                return await Mediator.Send(new UpdateArchiveSerialNumber.Command(request, id));
            }
            catch (NotFoundException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
    }
}
