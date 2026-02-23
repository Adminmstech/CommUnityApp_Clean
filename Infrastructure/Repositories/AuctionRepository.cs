using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class AuctionRepository:IAuctionRepository
    {
        private readonly IConfiguration _configuration;

        public AuctionRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<int> AddAsync(Auction entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Auction>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Auction> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> SaveAuction(Auction entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@AuctionId", entity.AuctionId);
            parameters.Add("@BusinessId", entity.BusinessId);
            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@ItemTypeId", entity.ItemTypeId);
            parameters.Add("@ItemTitle", entity.ItemTitle);
            parameters.Add("@ItemDescription", entity.ItemDescription);
            parameters.Add("@ItemCondition", entity.ItemCondition);
            parameters.Add("@PriceIncrement", entity.PriceIncrement);
            parameters.Add("@ReservePrice", entity.ReservePrice);
            parameters.Add("@MinDeposite", entity.MinDeposite);
            parameters.Add("@StartTime", entity.StartTime);
            parameters.Add("@EndTime", entity.EndTime);
            parameters.Add("@ItemLocation", entity.ItemLocation);
            parameters.Add("@DeleveryMethodId", entity.DeleveryMethodId);
            parameters.Add("@RegistrationStartDate", entity.RegistrationStartDate);
            parameters.Add("@RegistrationEndDate", entity.RegistrationEndDate);
            parameters.Add("@AuctionStatus", entity.AuctionStatus);
            parameters.Add("@CreatedBy", entity.CreatedBy);
            var result = await connection.QueryAsync<BaseResponse>( "Add_Auction", parameters, commandType: CommandType.StoredProcedure);

            return result.FirstOrDefault();
        }
        public async Task<List<BidDto>> GetRecentBids(int auctionId)
        {
            
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@AuctionId", auctionId);

            var result = await connection.QueryAsync<BidDto>("[Get_CurrentHighestBid]", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public Task<int> UpdateAsync(Auction entity)
        {
            throw new NotImplementedException();
        }


        public async Task<BaseResponse> SaveAuctionItemImage(AuctionItemImage entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@AuctionId", entity.AuctionId);
            parameters.Add("@ImageUrl", entity.ImageUrl);
            parameters.Add("@ImageName", entity.ImageName);
            parameters.Add("@IsPrimary", entity.IsPrimary);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_InsertAuctionItemImage",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }


        public async Task<List<ItemType>> GetItemType()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<ItemType>(
                "Get_ItemType",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<AuctionListModel>> GetAuctions()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<AuctionListModel>( "Get_All_Auctions",commandType: CommandType.StoredProcedure );

            return result.ToList();
        }

        public async Task<List<AuctionListModel>> GetTop5Auctions()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<AuctionListModel>("Get_Top5LiveAuctions", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }


        public async Task<List<AuctionItemImage>> GetAuctionImages(int auctionId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@AuctionId", auctionId);

            var result = await connection.QueryAsync<AuctionItemImage>("Get_AuctionImagesByAuctionId",parameters,commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<GetAuctionImagedModel>> GetAuctionImagesByIds(List<int> auctionIds)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@AuctionIds", string.Join(",", auctionIds));

            var result = await connection.QueryAsync<GetAuctionImagedModel>(
                "Get_AuctionImagesByMultipleIds",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
        public async Task<List<AuctionListModel>> GetAuctionAuctionId(int auctionId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@AuctionId", auctionId);

            var result = await connection.QueryAsync<AuctionListModel>("Get_AuctionById", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }


        public async Task<List<AuctionListModel>> GetAuctionByItemTypeId(int ItemTypeId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ItemTypeId", ItemTypeId);

            var result = await connection.QueryAsync<AuctionListModel>("Get_AuctionByItemTypeId", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<PlaceBidResponse> PlaceBid(PlaceBidRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@AuctionId", request.AuctionId);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@BidAmount", request.BidAmount);

            var result = await connection.QueryFirstOrDefaultAsync<PlaceBidResponse>(
                "dbo.Place_Bid",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }


        public async Task<BaseResponse> SaveBidRegistration(BidRegistration entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BidRegistrationId", entity.BidRegistrationId == Guid.Empty
                ? (Guid?)null
                : entity.BidRegistrationId);

            parameters.Add("@AuctionId", entity.AuctionId);
            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@PaymentId", entity.PaymentId);
            parameters.Add("@PaymentStatusId", entity.PaymentStatusId);

            var result = await connection.QueryAsync<BaseResponse>( "Add_BidRegistration",parameters,commandType: CommandType.StoredProcedure );

            return result.FirstOrDefault();
        }

    }
}
