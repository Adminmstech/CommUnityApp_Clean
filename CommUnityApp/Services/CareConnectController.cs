using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{

    [ApiController]
    [Route("api/[controller]")]
    public class CareConnectController : ControllerBase
    {

        private readonly ICareConnectRepository _careConnectRepository;

        public CareConnectController(ICareConnectRepository careConnectRepository)
        {
            _careConnectRepository = careConnectRepository;
        }

        [HttpGet("GetCareConnectServices")]
        public async Task<IActionResult> GetServices()
        {
            var data = await _careConnectRepository.GetServices();

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }
        [HttpPost("RequestCareConnectSupporter")]
        public async Task<IActionResult> CreateRequest([FromBody] CareRequestModel model)
        {
            try
            {
                var requestId = await _careConnectRepository.CreateRequest(model);

                return Ok(new
                {
                    ResultId = requestId,
                    ResultMessage = "Request created successfully",
                    Status = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("GetCareConnectSupporters")]
        public async Task<IActionResult> GetSupporters(
            int serviceId,
            int communityId,
            decimal latitude,
            decimal longitude)
        {
            var data = await _careConnectRepository.GetSupporters(
                serviceId,
                communityId,
                latitude,
                longitude);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

       

        [HttpPost("sendCareConnnectMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageModel model)
        {
            try
            {
                await _careConnectRepository.SendMessage(model);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Message sent successfully",
                    Status = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("GetCareConnectChatMessages")]
        public async Task<IActionResult> GetMessages(long chatThreadId)
        {
            var data = await _careConnectRepository.GetMessages(chatThreadId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpPost("RespondCareConnectRequest")]
        public async Task<IActionResult> RespondRequest(
            [FromBody] RespondRequestModel model)
        {
            await _careConnectRepository.RespondRequest(model);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Response updated successfully",
                Status = true
            });
        }

        [HttpPost("FinalizeSupporter")]
        public async Task<IActionResult> FinalizeSupporter(
            [FromBody] FinalizeSupporterModel model)
        {
            await _careConnectRepository.FinalizeSupporter(model);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Supporter finalized successfully",
                Status = true
            });
        }

        [HttpPost("CreateCareConnectServiceRequest")]
        public async Task<IActionResult> CreateRequestWithSupporters(
    [FromBody] CreateRequestWithSupportersModel model)
        {
            try
            {
                var requestId = await _careConnectRepository.CreateRequestWithSupporters(model);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Request created successfully",
                    Status = true,
                    RequestId = requestId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("GetUserCareConnectMessages")]
        public async Task<IActionResult> GetUserMessages(Guid userId)
        {
            var data = await _careConnectRepository.GetUserMessages(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }
    }
}
    
