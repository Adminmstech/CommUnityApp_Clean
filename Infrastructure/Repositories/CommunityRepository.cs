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
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                return await con.QueryFirstOrDefaultAsync<CharityItemModel>(
                    "sp_GetCharityItemDetails",
                    new { CharityItemId = charityItemId },
                    commandType: CommandType.StoredProcedure);
            }
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
        public async Task<(int CharityItemId, string ItemCode)> AddCharityItem(AddCharityItemModel model, string imagePath)
        {
           
                using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    var result = await con.QueryFirstAsync(
                        "SP_AddCharityItem",
                        new
                        {
                            model.CommunityId,
                            model.PostedByUserId,
                            model.ItemName,
                            model.ItemCategory,
                            model.Description,
                            model.Quantity,
                            ImagePath = imagePath
                        },
                        commandType: CommandType.StoredProcedure);

                    return ((int)result.CharityItemId, (string)result.ItemCode);
                }
            }

            public async Task UpdateCharityItemImage(int charityItemId, string imagePath)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.ExecuteAsync(
                    "UPDATE CharityItems SET ImagePath=@ImagePath WHERE CharityItemId=@CharityItemId",
                    new { CharityItemId = charityItemId, ImagePath = imagePath });
            }
        }

        public async Task<int> RequestCharityItem(RequestCharityItemModel model)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var id = await con.ExecuteScalarAsync<int>(
                    "sp_RequestCharityItem",
                    new
                    {
                        model.CharityItemId,
                        model.RequestedByUserId,
                        model.RequestedQuantity,
                        model.Description
                    },
                    commandType: CommandType.StoredProcedure);

                return id;
            }
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
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<CharityItemListModel>(
                    "sp_GetAllCharityItems",
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
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
    }
}
