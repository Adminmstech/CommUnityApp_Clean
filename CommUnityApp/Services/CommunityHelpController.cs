using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CommUnityApp.Services
{
        [ApiController]
        [Route("api/[controller]")]
        public class CommunityHelpController : ControllerBase
        {
            private readonly ICommunityHelpRepository _repository;

            public CommunityHelpController(
                ICommunityHelpRepository repository)
            {
                _repository = repository;
            }


            [HttpPost("CreateHelpRequest")]
            public async Task<IActionResult> CreateHelpRequest(
                [FromBody] CreateCommunityHelpRequestModel model)
            {
                if (model == null)
                    return BadRequest("Request data is required.");

                var result =
                    await _repository.CreateHelpRequest(model);

                if (result.ResultId == 0)
                    return BadRequest(result);

                return Ok(result);
            }


            [HttpGet("GetCommunityHelpRequests")]
            public async Task<IActionResult> GetCommunityHelpRequests(
                [FromQuery] long communityId,
                [FromQuery] Guid? userId = null,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] string? status = null)
            {
                if (communityId <= 0)
                    return BadRequest("CommunityId is required.");

                var result =
                    await _repository.GetCommunityHelpRequests(
                        communityId,
                        userId,
                        pageNumber,
                        pageSize,
                        search,
                        status);

                return Ok(new
                {
                    totalRecords = result.TotalRecords,
                    pageNumber,
                    pageSize,
                    data = result.Data
                });
            }


            [HttpGet("GetHelpRequestDetails")]
            public async Task<IActionResult> GetHelpRequestDetails(
                [FromQuery] long helpRequestId)
            {
                if (helpRequestId <= 0)
                    return BadRequest("HelpRequestId is required.");

                var result =
                    await _repository.GetHelpRequestDetails(
                        helpRequestId);

                if (result.HelpRequest == null)
                    return NotFound(new
                    {
                        resultId = 0,
                        resultMessage = "Help request not found."
                    });

                return Ok(result);
            }


            [HttpPost("OfferHelp")]
            public async Task<IActionResult> OfferHelp(
                [FromBody] CreateCommunityHelpOfferModel model)
            {
                if (model == null)
                    return BadRequest("Request data is required.");

                var result =
                    await _repository.CreateHelpOffer(model);

                if (result.ResultId == 0)
                    return BadRequest(result);

                return Ok(result);
            }


            [HttpGet("GetHelpOffers")]
            public async Task<IActionResult> GetHelpOffers(
                [FromQuery] long helpRequestId)
            {
                if (helpRequestId <= 0)
                    return BadRequest("HelpRequestId is required.");

                var result =
                    await _repository.GetHelpOffers(
                        helpRequestId);

                return Ok(new
                {
                    helpRequestId,
                    responses = result
                });
            }


            [HttpGet("GetMyHelpRequests")]
            public async Task<IActionResult> GetMyHelpRequests(
                [FromQuery] Guid userId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? status = null)
            {
                if (userId == Guid.Empty)
                    return BadRequest("UserId is required.");

                var result =
                    await _repository.GetMyHelpRequests(
                        userId,
                        pageNumber,
                        pageSize,
                        status);

                return Ok(new
                {
                    totalRecords = result.TotalRecords,
                    pageNumber,
                    pageSize,
                    data = result.Data
                });
            }


         
            [HttpGet("GetMyHelpOffers")]
            public async Task<IActionResult> GetMyHelpOffers(
                [FromQuery] Guid userId,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? status = null)
            {
                if (userId == Guid.Empty)
                    return BadRequest("UserId is required.");

                var result =
                    await _repository.GetMyHelpOffers(
                        userId,
                        pageNumber,
                        pageSize,
                        status);

                return Ok(new
                {
                    totalRecords = result.TotalRecords,
                    pageNumber,
                    pageSize,
                    data = result.Data
                });
            }


            [HttpPost("ConnectHelper")]
            public async Task<IActionResult> ConnectHelper(
                [FromBody] ConnectCommunityHelpModel model)
            {
                if (model == null)
                    return BadRequest("Request data is required.");

                var result =
                    await _repository.ConnectHelper(model);

                if (result.ResultId == 0)
                    return BadRequest(result);

                return Ok(result);
            }



            [HttpPost("CompleteHelp")]
            public async Task<IActionResult> CompleteHelp(
                [FromBody] CompleteCommunityHelpModel model)
            {
                if (model == null)
                    return BadRequest("Request data is required.");

                var result =
                    await _repository.CompleteHelp(model);

                if (result.ResultId == 0)
                    return BadRequest(result);

                return Ok(result);
            }

            [HttpPost("CloseHelpRequest")]
            public async Task<IActionResult> CloseHelpRequest(
                [FromBody] CloseCommunityHelpRequestModel model)
            {
                if (model == null)
                    return BadRequest("Request data is required.");

                var result =
                    await _repository.CloseHelpRequest(model);

                if (result.ResultId == 0)
                    return BadRequest(result);

                return Ok(result);
            }


            [HttpPost("CancelHelpRequest")]
            public async Task<IActionResult> CancelHelpRequest(
                [FromBody] CancelCommunityHelpRequestModel model)
            {
                if (model == null)
                    return BadRequest("Request data is required.");

                var result =
                    await _repository.CancelHelpRequest(model);

                if (result.ResultId == 0)
                    return BadRequest(result);

                return Ok(result);
            }
        }
    }
