using Application.Dtos.GroupDTO;
using Application.Dtos.UISettingsDTO;

using Application.Features.FeatureUISettings;
using Application.Features.FeatureUser;
using Domain.Settings;
using Domain.User;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class UISettingsController : BaseApiController
    {
        

    //    [HttpGet("get_ui_settings_details")]
    //    public async Task<ActionResult<string>> GetUISettingsAsync([FromQuery] Guid id)
    //    {
    //        var command = new GetUISettingsDetails.Query(id);
    //        var commandResponse = await Mediator.Send(command);

    //        return commandResponse;
    //    }
    
    //[HttpPut("store_settings")]
    //public async Task<ActionResult<UISettings>> UpdateUISettingsAsync(StoreSettingsDTO request, [FromQuery] Guid id)
    //{
    //    var command = new StoreSettings.Command(request, id);
    //    var commandResponse = await Mediator.Send(command);

    //    return commandResponse;
    //}
}
}
