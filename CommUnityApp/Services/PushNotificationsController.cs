using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushNotificationsController : ControllerBase
    {
        private readonly IPushNotificationsBal _pushNotificationsBal;

        public PushNotificationsController(IPushNotificationsBal pushNotificationsBal)
        {
            _pushNotificationsBal = pushNotificationsBal;
        }

        [HttpGet("Templates")]
        public IActionResult GetTemplates()
        {
            return Ok(_pushNotificationsBal.GetTemplates());
        }

        [HttpPost("Preview")]
        public IActionResult Preview([FromBody] PushNotificationPreviewRequest request)
        {
            var template = _pushNotificationsBal.GetTemplate(request.Trigger);
            var body = _pushNotificationsBal.RenderBody(
                request.Trigger,
                request.Data,
                request.BodyOverride);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                template.Trigger,
                template.DefaultRecipientScope,
                template.Title,
                Body = body
            });
        }

        [HttpPost("Trigger")]
        public async Task<IActionResult> Trigger([FromBody] PushNotificationTriggerRequest request)
        {
            if (request == null)
                return BadRequest(new { ResultId = 0, ResultMessage = "Request is required." });

            var result = await _pushNotificationsBal.TriggerAsync(request);

            if (result.ResultId == 0)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
