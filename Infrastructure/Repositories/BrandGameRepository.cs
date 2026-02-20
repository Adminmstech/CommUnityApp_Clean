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
    public class BrandGameRepository : IBrandGameRepository
    {
        private readonly IConfiguration _configuration;

        public BrandGameRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<BaseResponse> AddUpdateBrandGameAsync(AddUpdateBrandGameRequest model, string brandGameImagePath, string unsuccessfulImagePath)
        {
            using var con = Connection;
            return await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_AddUpdateBrandGame",
                new
                {
                    model.BrandGameID,
                    BrandGame = model.BrandGameName,
                    model.BrandGameTitle,
                    BrandGameImage = brandGameImagePath,
                    model.UserGroupId,
                    BusinessId = model.BusinessId,
                    model.BrandGameDesc,
                    model.ConditionsApply,
                    model.GameClassificationID,
                    model.DateStart,
                    model.DateEnd,
                    model.PanelCount,
                    model.PanelOpeningLimit,
                    model.ChanceCount,
                    model.DestinationUrl,
                    model.Status,
                    model.PrimaryWinImageId,
                    model.SecondaryWinImageId,
                    model.ConsolationImageId,
                    model.ScratchCoverImageId,
                    model.QRImagePath,
                    model.LimitCount,
                    model.PrimaryOfferText,
                    model.OfferText,
                    model.PrimaryWinMessage,
                    model.SecondaryWinMessage,
                    model.ConsolationMessage,
                    model.PointsAwarded,
                    model.PermitNumber,
                    model.ClassNumber,
                    model.FormColor,
                    model.TextColor,
                    model.PromotionalCode,
                    model.PrimaryPrizeCount,
                    model.SecondaryPrizeCount,
                    model.ConsolationPrizeCount,
                    model.TotalEntries,
                    model.PrimaryPrizePromotionId,
                    model.SecondaryPrizePromotionId,
                    model.ConsolationPrizePromotionId,
                    model.IsPayment,
                    model.PaymentAmount,
                    UnSuccessfulImage = unsuccessfulImagePath,
                    model.ExpiryText,
                    model.OnceIn,
                    model.IsReleased,
                    model.ReferaFriend,
                    model.CurrentInterval,
                    model.IntervalId,
                    model.CustomTagIds,
                    model.GroupId,
                    model.QRlinkedId,
                    model.IsArchive
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<BrandGameDto> GetBrandGameByIdAsync(int brandGameId)
        {
            using var con = Connection;
            return await con.QueryFirstOrDefaultAsync<BrandGameDto>(
                "SELECT *, BrandGame AS BrandGameName FROM BrandGame WHERE BrandGameID = @BrandGameID",
                new { BrandGameID = brandGameId }
            );
        }

        public async Task<IEnumerable<BrandGameDto>> GetAllBrandGamesAsync()
        {
            using var con = Connection;
            return await con.QueryAsync<BrandGameDto>("SELECT *, BrandGame AS BrandGameName FROM BrandGame WHERE IsArchive = 0");
        }

        public async Task<IEnumerable<BrandGameDto>> GetBrandGamesByMerchantAsync(int merchantId)
        {
            using var con = Connection;
            return await con.QueryAsync<BrandGameDto>(
                "SELECT *, BrandGame AS BrandGameName FROM BrandGame WHERE BusinessId = @BusinessId AND IsArchive = 0",
                new { BusinessId = merchantId }
            );
        }

        public async Task<BaseResponse> DeleteBrandGameAsync(int brandGameId)
        {
            using var con = Connection;
            var result = await con.ExecuteAsync(
                "UPDATE BrandGame SET IsArchive = 1 WHERE BrandGameID = @BrandGameID",
                new { BrandGameID = brandGameId }
            );
            return new BaseResponse 
            { 
                ResultId = result > 0 ? 1 : 0, 
                ResultMessage = result > 0 ? "Brand game deleted successfully." : "Brand game not found." 
            };
        }
    }
}
