using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Stripe;
using System.Security.Claims;

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
        private Guid GetLoggedInUserId()
        {
            string userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("UserId");

            return Guid.TryParse(userId, out Guid id) ? id : Guid.Empty;
        }
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendCommunityHelpMessageModel model)
        {
            if (model == null || model.ConnectionId <= 0)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid ConnectionId is required."
                });
            }

            if (model.SenderUserId == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid SenderUserId is required."
                });
            }

            if (string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Message is required."
                });
            }

            var result = await _repository.SendMessage(
                model.ConnectionId,
                model.SenderUserId,
                model.Message.Trim());

            if (result == null || (int)result.ResultId == 0)
            {
                return BadRequest(new
                {
                    status = false,
                    message = result?.ResultMessage
                });
            }

            return Ok(new
            {
                status = true,
                message = result.ResultMessage,
                messageId = result.MessageId
            });
        }

        // 2. GET CONVERSATION MESSAGES
        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessages([FromQuery] long connectionId,[FromQuery] Guid userId)
        {
            if (connectionId <= 0)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid ConnectionId is required."
                });
            }

            if (userId == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid UserId is required."
                });
            }

            var messages = await _repository.GetMessages(
                connectionId,
                userId);

            return Ok(new
            {
                status = true,
                message = "Messages retrieved successfully.",
                data = messages
            });
        }

        // 3. MARK MESSAGES AS READ
        [HttpGet("MarkMessagesRead")]
        public async Task<IActionResult> MarkMessagesRead([FromQuery] long connectionId,[FromQuery] Guid userId)
        {
            if (connectionId <= 0)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid ConnectionId is required."
                });
            }

            if (userId == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid UserId is required."
                });
            }

            var result = await _repository.MarkMessagesRead(
                connectionId,
                userId);

            return Ok(new
            {
                status = true,
                message = result?.ResultMessage
                    ?? "Messages marked as read."
            });
        }

        [HttpGet("GetMyHelpConnections")]
        public async Task<IActionResult> GetMyHelpConnections([FromQuery] Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid UserId is required."
                });
            }

            var connections = await _repository.GetMyHelpConnections(userId);

            return Ok(new
            {
                status = true,
                message = "Help conversations retrieved successfully.",
                data = connections
            }); 
        }

        [HttpPost("UpdateHelpRequest")]
        public async Task<IActionResult> UpdateHelpRequest([FromBody] UpdateCommunityHelpRequestModel model)
        {
            if (model == null || model.HelpRequestId <= 0)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid HelpRequestId is required."
                });
            }

            if (model.RequestedByUserId == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Valid RequestedByUserId is required."
                });
            }

            if (string.IsNullOrWhiteSpace(model.RequestTitle))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Request title is required."
                });
            }

            if (string.IsNullOrWhiteSpace(model.RequestDescription))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Request description is required."
                });
            }

            var result = await _repository.UpdateHelpRequest(model);

            if (result == null || result.ResultId == 0)
            {
                return BadRequest(new
                {
                    status = false,
                    message = result?.ResultMessage ?? "Unable to update help request."
                });
            }

            return Ok(new
            {
                status = true,
                message = result.ResultMessage,
                helpRequestId = result.HelpRequestId
            });
        }
    }
    }
