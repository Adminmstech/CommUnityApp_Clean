using Azure;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConfiguration _configuration;

        public NotificationRepository(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        public async Task<List<NotificationModel>> GetNotifications(Guid userId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
           

            var data = await connection.QueryAsync<NotificationModel>(
                "sp_GetUserNotifications",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);

            return data.ToList();
        }


        public async Task<int> CreatePostAsync(CreatePostRequest request)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var parameters = new DynamicParameters();
                       // parameters.Add("@CommunityId", request.CommunityId);
                        parameters.Add("@UserId", request.UserId);
                        parameters.Add("@Category", request.Category);
                        parameters.Add("@Type", request.Type);
                        parameters.Add("@Priority", request.Priority);
                        parameters.Add("@Title", request.Title);
                        parameters.Add("@Description", request.Description);
                        parameters.Add("@Location", request.Location);

                        var postId = await connection.ExecuteScalarAsync<int>(
                            "sp_AddCommunityMessageBoardPost",
                            parameters,
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );


                        string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                        string folderPath = Path.Combine(rootPath, "Uploads", "Posts", postId.ToString());
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        if (request.Images != null && request.Images.Count > 0)
                        {
                            int count = 1;

                            foreach (var base64 in request.Images)
                            {
                                var base64Data = base64.Contains(",") ? base64.Split(',')[1] : base64;

                                byte[] imageBytes = Convert.FromBase64String(base64Data);

                                string fileName = $"img_{Guid.NewGuid()}.jpg";
                                string filePath = Path.Combine(folderPath, fileName);

                                await File.WriteAllBytesAsync(filePath, imageBytes);

                                string dbPath = $"/Uploads/Posts/{postId}/{fileName}";

                                await connection.ExecuteAsync(
                                    "dbo.InsertPostImages",
                                    new { PostId = postId, ImagePath = dbPath },
                                    transaction,
                                    commandType: CommandType.StoredProcedure
                                );

                                count++;
                            }
                        }

                        transaction.Commit();
                        return postId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task AddPostImage(int postId, string path)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            await connection.ExecuteAsync(
                "INSERT INTO CommunityPostImages(PostId, ImagePath) VALUES(@PostId,@Path)",
                new { PostId = postId, Path = path });
        }

        public async Task<List<dynamic>> GetFeed(long communityId, string category)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var data = await connection.QueryAsync(
                "sp_GetCommunityFeed",
                new { CommunityId = communityId, Category = category },
                commandType: CommandType.StoredProcedure);

            return data.ToList();
        }

        public async Task<BaseResponse> LikePost(LikePostModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_LikePost",
                new { model.PostId , model.UserId },
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<BaseResponse> AddComment(AddCommentModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_AddComment",
                new
                {
                    model.PostId,
                    model.UserId,
                    model.CommentText
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }
        public async Task AddReply(AddReplyModel model)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            await connection.ExecuteAsync(
                "sp_AddReply",
                new
                {
                    model.PostId,
                    model.UserId,
                    ParentCommentId = model.ParentCommentId,
                    CommentText = model.CommentText
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<PostResponse>> GetMessageBoardPosts()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var multi = await connection.QueryMultipleAsync(
                    "dbo.GetCommunityMessageBoardPostsWithDetails",
                    //new { CommunityId = communityId },
                    commandType: CommandType.StoredProcedure))
                {
                    var posts = (await multi.ReadAsync<PostResponse>()).ToList();
                    var images = (await multi.ReadAsync<dynamic>()).ToList();
                    var likes = (await multi.ReadAsync<dynamic>()).ToList();
                    var comments = (await multi.ReadAsync<dynamic>()).ToList();

                    foreach (var post in posts)
                    {
                        
                        post.Images = images
                            .Where(i => i.PostId == post.PostId)
                            .Select(i => (string)i.ImagePath)
                            .ToList();

                        post.LikesCount = likes
                            .Count(l => l.PostId == post.PostId);

                        var postComments = comments
                            .Where(c => c.PostId == post.PostId)
                            .ToList();

                        post.Comments = postComments.Select(c => new CommentDto
                        {
                            CommentId = c.CommentId,
                            CommentText = c.CommentText,
                            UserId = c.UserId.ToString()
                        }).ToList();

                        post.CommentsCount = post.Comments.Count;
                    }

                    return posts;
                }
            }
        }

        public async Task<List<CategoryDto>> GetCategories()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QueryAsync<CategoryDto>(
                    "dbo.SP_GetMessageBoardCategories",
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }
        public async Task<List<SubCategoryDto>> GetSubCategories(int categoryId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QueryAsync<SubCategoryDto>(
                    "dbo.SP_GetMessageBoardSubCategoriesById",
                    new { CategoryId = categoryId },
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }
    }
}
