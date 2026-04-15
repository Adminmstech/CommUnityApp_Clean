using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {

        private readonly INotificationRepository _notificationRepository;
        private readonly IWebHostEnvironment _env;
        public NotificationController(IWebHostEnvironment env, INotificationRepository notificationRepository)
        {
            _env = env;
            _notificationRepository = notificationRepository;
        }

        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("UserId required");

            var data = await _notificationRepository.GetNotifications(userId);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (item.ImagePath != null)
                    item.ImagePath = baseUrl + item.ImagePath;
            }

            return Ok(data);
        }
        [HttpPost("AddPost")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                var postId = await _notificationRepository.CreatePostAsync(request);

                return Ok(new
                {
                    Status = true,
                    Message = "Post created successfully",
                    PostId = postId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetFeed")]
        public async Task<IActionResult> GetFeed(long communityId, string category = null)
        {
            var data = await _notificationRepository.GetFeed(communityId, category);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (item.Images != null)
                {
                    var imgs = item.Images.Split(',');
                    //item.Images = imgs.Select(x => baseUrl + x).ToArray();
                }
            }

            return Ok(data);
        }

        [HttpPost("LikePost")]
        public async Task<IActionResult> LikePost([FromBody] LikePostModel model)
        {
            try
            {
                var result = await _notificationRepository.LikePost(model);

                return Ok(result); 
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CommentText))
                {
                    return BadRequest(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "Comment is required"
                    });
                }

                await _notificationRepository.AddComment(model);

                return Ok(new BaseResponse
                {
                    ResultId = 1,
                    ResultMessage = "Comment added successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpPost("AddReply")]
        public async Task<IActionResult> AddReply([FromBody] AddReplyModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CommentText))
                {
                    return BadRequest(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "Reply is required"
                    });
                }

                await _notificationRepository.AddReply(model);

                return Ok(new BaseResponse
                {
                    ResultId = 1,
                    ResultMessage = "Reply added successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }
        [HttpGet("GetMessageBoardPosts")]
        public async Task<IActionResult> GetPosts()
        {
            var result = await _notificationRepository.GetMessageBoardPosts();

            return Ok(new
            {
                Status = true,
                Data = result
            });
        }
        [HttpGet("GetMessageBoardCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _notificationRepository.GetCategories();

            return Ok(new
            {
                Status = true,
                Data = categories
            });
        }


        [HttpGet("GetMessageBoardSubCategoryType")]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var result = await _notificationRepository.GetSubCategories(categoryId);

            return Ok(result);
        }
    }
}
