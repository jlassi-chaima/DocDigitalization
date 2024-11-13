using Application.Dtos.ShareFolder;
using Application.Features.FeatureShareFolder;
using Application.PaginationParams;
using Domain.FileShare;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controller.ShareFolderController
{
    public class ShareFolderController : BaseApiController
    {
        [HttpPost("add_share_file")]
        public async Task<IActionResult> CreateShareFolder(ShareFolderDto shareFolderDto)
        {

            try
            {
                return Ok(await Mediator.Send(new AddShareFolder.Command(shareFolderDto)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        //public async Task<IActionResult> ListShareFolder()
        //{

        //        var command = new ListShareFolder.Query();
        //        var result = await Mediator.Send(command);
        //        return Ok(result);

        //}
        [HttpGet("show_all_share_files")]
        public async Task<IActionResult> ListShareFolder([FromQuery] ShareFolderParameters sharefolderparameters, [FromQuery] string? owner)
        {

            try
            {
                return Ok(await Mediator.Send(new ListShareFolder.Query(sharefolderparameters , owner)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpGet("get_specific_share_file/{id}")]
        public async Task<IActionResult> DetailsShareFolder(Guid id)
        {
            try
            {

                return Ok(await Mediator.Send(new DetailsShareFolder.Query(id)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpDelete("delete_share_file/{id}")]
        public async Task<IActionResult> DeleteShareFolder(Guid id)
        {
            try
            {

                await Mediator.Send(new DeleteShareFolder.Command(id));

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }

        }

        [HttpPut("update_share_file/{id}")]
        public async Task<ActionResult<ShareFolder>> UpdateShareFileAsync(ShareFolderDto request, Guid id)
        {
            try
            {

                return await Mediator.Send(new UpdateShareFile.Command(request, id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
    }
}
