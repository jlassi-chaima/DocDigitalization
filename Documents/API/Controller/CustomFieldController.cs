using Application.Dtos.CustomField;
using Application.Features.FeaturesCustomField;
using Application.Parameters;
using Domain.DocumentManagement.CustomFields;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controller
{
    [ApiController]
    public class CustomFieldController : BaseApiController
    {
        [HttpGet("list_customfield")]
        public async Task<IActionResult> ListCustomFieldAsync()
        {

            try
            {


                return Ok(await Mediator.Send(new ListCustomFields.Query()));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet]
        public async Task<IActionResult> PagedCustomFieldAsync([FromQuery] CustomFieldParameters customfiledparameters)
        {

            try
            {


                return Ok(await Mediator.Send(new CustomFildPagedList.Query(customfiledparameters)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("get_customfield/{id}")]
        public async Task<ActionResult<CustomField>> GetCustomFieldAsync(Guid id)
        {
            try
            {

                return await Mediator.Send(new DetailsCustomField.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpPost("post_customfield")]
        public async Task<ActionResult<CustomField>> AddCustomFieldAsync(CustomFieldDto request)
        {
            try
            {

                return await Mediator.Send(new AddCustomField.Command(request)); ;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpPut("update_customfield/{id}")]
        public async Task<ActionResult<CustomField>> UpdateCustomFieldAsync(CustomFieldDto request, Guid id)
        {
            try
            {

                return await Mediator.Send(new UpdateCustomField.Command(request, id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpDelete("delete_customfield/{id}")]
        public async Task<IActionResult> DeleteCustomFieldAsync(Guid id)
        {
            try
            {

                var command = new DeleteCustomField.Command(id);
                await Mediator.Send(command);

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }


    }
}
