using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Campaign
    {
        public int CampaignId { get; set; }

        public string CampaignName { get; set; }
        public string ShortNote { get; set; }
        public string Description { get; set; }
        public string CampaignImage { get; set; }

        public string PromotionLink { get; set; }
        public bool IsReferral { get; set; }

        public int BusinessId { get; set; }
        public string CampaignType { get; set; }

        public decimal? Budget { get; set; }
        public decimal? CostPerClick { get; set; }
        public decimal? CostPerImpression { get; set; }

        public string TargetLocation { get; set; }
        public int? TargetAgeMin { get; set; }
        public int? TargetAgeMax { get; set; }
        public string TargetGender { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Status { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
