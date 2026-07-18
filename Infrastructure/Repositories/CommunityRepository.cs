using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static CommUnityApp.ApplicationCore.Models.AssignVolunteerRequest;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly IConfiguration _configuration;

        public CommunityRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<CommunityLoginResponse> LoginAsync(CommunityLoginRequest request)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<CommunityLoginResponse>(
                "sp_CommunityLogin",
                new
                {
                    UserName = request.UserName,
                    Password = request.Password
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<GroupDto>> GetGroupsByCommunityAsync(long communityId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return (await con.QueryAsync<GroupDto>(
                "sp_GetGroupsByCommunity",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            )).ToList();
        }

        public async Task<List<CharityItem>> GetCharityItemsByCommunityId(long communityId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return (await con.QueryAsync<CharityItem>(
                "SP_GetCharityItemsByCommunityId",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            )).ToList();
        }
        public async Task<IEnumerable<dynamic>> GetVolunteersList(long? communityId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "SP_GetVolunteersList",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> AssignVolunteer(long charityItemId, Guid assignedToUserId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result = await con.ExecuteAsync(
                "SP_AssignVolunteerToCharity",
                new
                {
                    CharityItemId = charityItemId,
                    AssignedToUserId = assignedToUserId
                },
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }
        public async Task<AssignedVolunteerModel> GetAssignedVolunteer(int charityItemId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                return con.QueryFirstOrDefault<AssignedVolunteerModel>(
                    "sp_GetAssignedVolunteerByCharityItem",
                    new { CharityItemId = charityItemId },
                    commandType: CommandType.StoredProcedure);
            }
        }
        public async Task<CharityItemModel> GetCharityItemDetails(int charityItemId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(
                "sp_GetCharityItemDetails",
                new
                {
                    CharityItemId = charityItemId
                },
                commandType: CommandType.StoredProcedure);

            var charityItem = await multi.ReadFirstOrDefaultAsync<CharityItemModel>();

            if (charityItem == null)
                return null;

            charityItem.ImagePaths =
                (await multi.ReadAsync<string>()).ToList();

            return charityItem;
        }

        public async Task<bool> UpdateVolunteerStatusAsync(UpdateStatusRequest request)
        {
            try
            {
                using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@CharityItemId", request.CharityItemId);
                    parameters.Add("@UserId", request.UserId);
                    parameters.Add("@Status", request.Status);

                    var result = await con.ExecuteAsync(
                        "SP_UpdateVolunteerStatus",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CharityRequestModel>> GetCharityItemRequestsList(long communityId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<CharityRequestModel>(
                    "sp_GetCharityItemRequestsList",
                    new { CommunityId = communityId },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }
        public async Task<(int CharityItemId, string ItemCode)> AddCharityItem(
     AddCharityItemModel model)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstAsync(
                "SP_AddCharityItem",
                new
                {
                    model.CommunityId,
                    model.PostedByUserId,
                    model.ItemName,
                    model.ItemCategory,
                    model.Description,
                    model.Quantity
                },
                commandType: CommandType.StoredProcedure);

            return (
                (int)result.CharityItemId,
                (string)result.ItemCode
            );
        }

        public async Task UpdateCharityItemImage(
    long charityItemId,
    string imagePath)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.ExecuteAsync(
                "SP_UpdateCharityItemImage",
                new
                {
                    CharityItemId = charityItemId,
                    ImagePath = imagePath
                },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<int> AddCharityItemImage(
    int charityItemId,
    string imagePath)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await con.OpenAsync();

            var result = await con.QueryFirstAsync(
                "dbo.SP_AddCharityItemImage",
                new
                {
                    CharityItemId = charityItemId,
                    ImagePath = imagePath
                },
                commandType: CommandType.StoredProcedure
            );

            int status = Convert.ToInt32(result.Status);

            if (status != 1)
            {
                throw new Exception(
                    $"SP_AddCharityItemImage failed for CharityItemId {charityItemId}."
                );
            }

            return Convert.ToInt32(
                result.CharityItemImageId
            );
        }
        public async Task<RequestCharityItemResponseModel> RequestCharityItem(RequestCharityItemModel model)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<RequestCharityItemResponseModel>(
                "sp_RequestCharityItem",
                new
                {
                    model.CharityItemId,
                    model.RequestedByUserId,
                    model.RequestedQuantity,
                    model.Description
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<List<RequestedUserModel>> GetRequestedUsersByItemId(int charityItemId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<RequestedUserModel>(
                    "sp_GetRequestedUsersByItemId",
                    new { CharityItemId = charityItemId },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }



        public async Task AssignVolunteerToRequest(AssignVolunteerModel model)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.ExecuteAsync(
                    "sp_AssignVolunteerToRequest",
                    new
                    {
                        model.RequestId,
                        model.VolunteerId
                    },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<List<CharityItemListModel>> GetAllCharityItems()
        {
            using (var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                using var multi = await con.QueryMultipleAsync(
                    "Get_AllCharityItems",
                    commandType: CommandType.StoredProcedure);

                var items = (await multi.ReadAsync<CharityItemListModel>())
                    .ToList();

                var images = (await multi.ReadAsync<CharityItemImageModel>())
                    .ToList();

                foreach (var item in items)
                {
                    item.ImagePaths = images
                        .Where(x => x.CharityItemId == item.CharityItemId)
                        .Select(x => x.ImagePath)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();
                }

                return items;
            }
        }
        public async Task<List<MyRequestedItemsModel>> GetMyRequestedItems(Guid userId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<MyRequestedItemsModel>(
                    "sp_GetMyRequestedItems",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }
        public async Task<List<ItemCategoryModel>> GetItemCategories()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QueryAsync<ItemCategoryModel>(
                    "SP_GetItemCategories",
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
        }
        public async Task<List<Community>> GetCommunities()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Community>("Get_AllCommunities", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<MemberModel>> GetMembersByCommunity(int communityId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QueryAsync<MemberModel>(
                    "sp_GetMembersByCommunity",
                    new { CommunityId = communityId },
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }



        //public async Task<List<MemberModel>> GetMembersByCommunity(int communityId)
        //    {
        //        using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        //        {
        //            var result = await connection.QueryAsync<MemberModel>(
        //                "sp_GetMembersByCommunity",
        //                new { CommunityId = communityId },
        //                commandType: CommandType.StoredProcedure
        //            );

        //            return result.ToList();
        //        }
        //    }
        //public async Task<List<Community>> GetCommunities()
        //{
        //    using var connection = new SqlConnection(
        //        _configuration.GetConnectionString("DefaultConnection")
        //    );

        //    await connection.OpenAsync();

        //    var result = await connection.QueryAsync<Community>("Get_AllCommunities", commandType: CommandType.StoredProcedure);

        //    return result.ToList();
        //}
        public async Task<List<dynamic>> GetCommunityUsers(long communityId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var users = await connection.QueryAsync(
                    "sp_GetCommunityUsers",
                    new { CommunityId = communityId },
                    commandType: CommandType.StoredProcedure);

                return users.ToList();
            }
        }

        // Send message
        public async Task<int> SendMessage(long communityId, Guid receiverUserId, string message, string imagePath)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var id = await connection.ExecuteScalarAsync<int>(
                    "sp_SendCommunityMessage",
                    new
                    {
                        CommunityId = communityId,
                        ReceiverUserId = receiverUserId,
                        MessageText = message,
                        ImagePath = imagePath
                    },
                    commandType: CommandType.StoredProcedure);

                return id;
            }
        }

        public async Task<List<dynamic>> GetMessages(long communityId, Guid receiverUserId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var data = await connection.QueryAsync(
                    "sp_GetPrivateMessages",
                    new
                    {
                        CommunityId = communityId,
                        ReceiverUserId = receiverUserId
                    },
                    commandType: CommandType.StoredProcedure);

                return data.ToList();
            }
        }

        public async Task<List<CommunityCategoryDto>> GetCommunityCategoriesAsync()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QueryAsync<CommunityCategoryDto>("Get_CommunityCategories", commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        public async Task<BaseResponse> AddCommunityAsync(AddCommunityRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@CommunityId", entity.CommunityId);
            parameters.Add("@CommunityCategoryId", entity.CommunityCategoryId);
            parameters.Add("@CommunityName", entity.CommunityName);
            parameters.Add("@Logo", entity.Logo);
            parameters.Add("@Description", entity.Description);
            parameters.Add("@ContactName", entity.ContactName);
            parameters.Add("@ContactEmail", entity.ContactEmail);
            parameters.Add("@ContactPhone", entity.ContactPhone);
            parameters.Add("@Website", entity.Website);
            parameters.Add("@Address", entity.Address);
            parameters.Add("@OtherInfo", entity.OtherInfo);
            parameters.Add("@UserName", entity.UserName);
            parameters.Add("@Password", entity.Password);
            parameters.Add("@IsActive", entity.IsActive);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_Community",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<CommunityDto>> GetCommunitiesAsync()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<CommunityDto>(
                "Get_Communities",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<CommunityDto> GetCommunityDetailsAsync(int communityId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@CommunityId", communityId);

            var result = await connection.QueryAsync<CommunityDto>(
                "Get_CommunityDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<CommunityDto>> GetCommunitiesByCategoryAsync(int communityCategoryId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@CommunityCategoryId", communityCategoryId);

            var result = await connection.QueryAsync<CommunityDto>(
                "Get_CommunitiesByCategory",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        public async Task<BaseResponse> UpdateUserCommunityAsync(UpdateUserCommunityRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@CommunityId", entity.CommunityId);

            var result = await connection.QueryAsync<BaseResponse>(
                "Update_UserCommunityMembership",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<dynamic> AddCommunityPost(
CommunityPostModel model)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))

            {
                var result =
                    await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_AddCommunityPost",
                        new
                        {
                            CommunityId = model.CommunityId,
                            Title = model.Title,
                            Message = model.Message,
                            ImagePath = model.ImagePath,
                            CreatedBy = model.CreatedBy
                        },
                        commandType:
                        CommandType.StoredProcedure);

                return result;
            }
        }


        public async Task<List<CommunityPostModel>> GetCommunityPostsByUser(Guid userId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))

            {
                var result =
                    await connection.QueryAsync<CommunityPostModel>(
                        "sp_GetCommunityPostsByUser",
                        new
                        {
                            UserId = userId
                        },
                        commandType:
                        CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task<List<CommunityPostModel>>
    GetCommunityPosts(int communityId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))

            {
                var result =
                    await connection.QueryAsync<CommunityPostModel>(
                        "sp_GetCommunityPosts",
                        new
                        {
                            CommunityId = communityId
                        },
                        commandType:
                        CommandType.StoredProcedure);

                return result.ToList();
            }
        }
        public async Task<dynamic> DeleteCommunityPost(
    int postId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))

            {
                return await connection
                    .QueryFirstOrDefaultAsync<dynamic>(
                        "sp_DeleteCommunityPost",
                        new
                        {
                            PostId = postId
                        },
                        commandType:
                        CommandType.StoredProcedure);
            }

        }

        public async Task<List<UserCommunityResponse>> GetUserCommunitiesAsync(Guid userId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);

            var result = await con.QueryAsync<UserCommunityResponse>(
                "SP_GetUserCommunities",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<UpdateCharityItemResult> UpdateCharityItem(
    UpdateCharityItemModel model)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<UpdateCharityItemResult>(
                "SP_UpdateCharityItem",
                new
                {
                    CharityItemId = model.CharityItemId,
                    CommunityId = model.CommunityId,
                    ItemName = model.ItemName,
                    ItemCategory = model.ItemCategory,
                    Description = model.Description,
                    Quantity = model.Quantity
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }
        public Task<IEnumerable<dynamic>> GetCharityItemsByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }

        

       

        public async Task<List<CommunityPostModel>> GetTopFiveCommunityPostsByUser(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result =
                await connection.QueryAsync<CommunityPostModel>(
                    "sp_GetTopFiveCommunityPostsByUser",
                    new
                    {
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public Task UpdateCharityItemImage(int charityItemId, string imagePath)
        {
            throw new NotImplementedException();
        }

        public async Task<DeleteCharityItemResult> DeleteCharityItem(long charityItemId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<DeleteCharityItemResult>(
                "SP_DeleteCharityItem",
                new
                {
                    CharityItemId = charityItemId
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<List<UserCommunityCharityItemModel>> GetCharityItemsByUserCommunities(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetCharityItemsByUserCommunities",
                new
                {
                    UserId = userId
                },
                commandType: CommandType.StoredProcedure);

            var charityItems = (await multi.ReadAsync<UserCommunityCharityItemModel>()).ToList();

            var images = (await multi.ReadAsync<CharityItemImageModel>()).ToList();

            foreach (var item in charityItems)
            {
                item.Images = images
                    .Where(x => x.CharityItemId == item.CharityItemId)
                    .Select(x => x.ImagePath)
                    .ToList();
            }

            return charityItems;
        }
    }
}

