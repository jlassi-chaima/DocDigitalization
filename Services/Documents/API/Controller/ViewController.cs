using Application.Dtos.View;
using Application.Exceptions;
using Application.Features.FeaturesCustomField;
using Application.Features.FeaturesViews;
using Application.Parameters;
using Domain.DocumentManagement.Views;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TT.Internet.Framework.Infrastructure.Filters;

namespace API.Controller
{

    [ApiController]
        public class ViewController : BaseApiController
        {
            [HttpPost("post_view")]
            [ServiceFilter(typeof(ValidationFilter<ViewDto>))]
            public async Task<ActionResult<View>> AddArchive(ViewDto request)
            {
                try
                {
                    return await Mediator.Send(new AddView.Command(request));
                }
                catch (ViewException ex)
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
            [HttpGet("list_view")]
            public async Task<IActionResult> GetViews([FromQuery] ViewParameters viewParameters, [FromQuery] string? owner, [FromQuery] string? name_icontains)
            {
                try
                {

                      return Ok(await Mediator.Send(new ViewPagedList.Query(viewParameters, owner, name_icontains)));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return BadRequest("An error has occured, please try again later");

                }

            }
        }
}
