using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<NotificationModel>> GetNotifications(Guid userId);
        Task<int> CreatePostAsync(CreatePostRequest request);
        Task AddPostImage(int postId, string path);
        Task<List<dynamic>> GetFeed(long communityId, string category);

        Task<BaseResponse> LikePost(LikePostModel model);
        Task<BaseResponse> AddComment(AddCommentModel model);
        Task AddReply(AddReplyModel model);

        Task<List<PostResponse>> GetMessageBoardPosts();
        Task<List<CategoryDto>> GetCategories();
        Task<List<SubCategoryDto>> GetSubCategories(int categoryId);

    }
}
