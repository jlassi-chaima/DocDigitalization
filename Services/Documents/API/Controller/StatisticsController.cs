using Application.Statistics.FeaturesStatistics;
using Application.Statistics.Model;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controller
{
    [ApiController]
    public class statisticsController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<Statistics>> DetailsStatisticsAsync()
        {
            try { 

                 return await Mediator.Send(new StatisticsDetails.Query());
             }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
    }
}
    }
}
