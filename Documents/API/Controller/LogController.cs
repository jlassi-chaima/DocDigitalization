using Application.Features.FeaturesLogs;
using Domain.Logs;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    public class LogController : BaseApiController
    {
        [HttpGet("work")]
        public async Task<IActionResult> ListLogsAsync()
        {
            var command = new ListLogs.Query();
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogsByLogName(string id)
        {
            var command = new GetLogsByLogName.Query(id);
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }
        [HttpGet("add")]
        public async Task<IActionResult> AddLog(string id)
        {
            var command = new GetLogsByLogName.Query(id);
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }
    }
}
