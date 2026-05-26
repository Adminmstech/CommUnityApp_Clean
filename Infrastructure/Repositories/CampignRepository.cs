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
    public class CampignRepository: ICampaignRepository
    {
        private readonly IConfiguration _configuration;

        public CampignRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<BaseResponse> SaveCampaign(Campaign entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@CampaignId", entity.CampaignId);
            parameters.Add("@CampaignName", entity.CampaignName);
            parameters.Add("@ShortNote", entity.ShortNote);
            parameters.Add("@Description", entity.Description);
            parameters.Add("@CampaignImage", entity.CampaignImage);
            parameters.Add("@PromotionLink", entity.PromotionLink);
            parameters.Add("@IsReferral", entity.IsReferral);
            parameters.Add("@BusinessId", entity.BusinessId);
            parameters.Add("@CampaignType", entity.CampaignType);
            parameters.Add("@Budget", entity.Budget);
            parameters.Add("@CostPerClick", entity.CostPerClick);
            parameters.Add("@CostPerImpression", entity.CostPerImpression);
            parameters.Add("@TargetLocation", entity.TargetLocation);
            parameters.Add("@TargetAgeMin", entity.TargetAgeMin);
            parameters.Add("@TargetAgeMax", entity.TargetAgeMax);
            parameters.Add("@TargetGender", entity.TargetGender);
            parameters.Add("@StartDate", entity.StartDate);
            parameters.Add("@EndDate", entity.EndDate);
            parameters.Add("@Status", entity.Status);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_Campaign",   
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }


        public async Task<List<Campaign>> GetCampaignList()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Campaign>(
                "Get_CampaignList",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<Campaign>> GetCampaignsByBusiness(int businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@BusinessId", businessId);

            var result = await connection.QueryAsync<Campaign>(
                "Get_Campaigns_ByBusiness",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
}
