using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public  class Notification
    {
    }

    public class NotificationModel
    {
        public Guid ReceiverUserId { get; set; }
        public int MessageId { get; set; }
        public string Title { get; set; }
        public string MessageText { get; set; } 
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreatePostRequest
    {
       // public int CommunityId { get; set; }
        public Guid UserId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public List<string> Images { get; set; }
    }
    public class AddCommentModel
    {
        public int PostId { get; set; }
        public Guid UserId { get; set; }
        public string CommentText { get; set; }
    }
    public class AddReplyModel
    {
        public int PostId { get; set; }
        public Guid UserId { get; set; }
        public int ParentCommentId { get; set; }
        public string CommentText { get; set; }
    }
    public class LikePostModel
    {
        public int? PostId { get; set; }
        public Guid? UserId { get; set; }
    }
    public class PostResponse
    {
        public int PostId { get; set; }
        public int CommunityId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string ProfileImagePath { get; set; }
        public List<string> Images { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        public List<CommentDto> Comments { get; set; }
    }

    public class CommentDto
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class SubCategoryDto
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
    }
}
