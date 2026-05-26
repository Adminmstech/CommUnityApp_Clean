using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ICampaignRepository
    {
        Task<BaseResponse> SaveCampaign(Campaign entity);
        Task<List<Campaign>> GetCampaignList();
        Task<List<Campaign>> GetCampaignsByBusiness(int businessId);
    }
}
