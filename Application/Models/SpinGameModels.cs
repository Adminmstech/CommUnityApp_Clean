using Microsoft.AspNetCore.Http;
using System;

namespace CommUnityApp.ApplicationCore.Models
{
    public class SpinGameDto
    {
        public int GameId { get; set; }
        public int BusinessId { get; set; }
        public string? BusinessLocation { get; set; }

        public string GameName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BusinessLocation { get; set; }
        public string? GameImage { get; set; }
        public int ConfigId { get; set; }
        public bool IsActive { get; set; }

        public int RewardCoins { get; set; }
        // Add other props as needed
    }

    public class SpinGameConfigRequest
    {
        public int ConfigId { get; set; } = 0;
        public int MaxSpinsPerDay { get; set; }
        public int NumberOfSections { get; set; }
        public DateTime GameStartDate { get; set; } = DateTime.Now;
        public DateTime GameEndDate { get; set; } = DateTime.Now.AddDays(30);
        public bool IsActive { get; set; } = true;
    }

    public class SpinSectionRequest
    {
        public int SectionId { get; set; } = 0;
        public int GameId { get; set; }
        public int SectionNumber { get; set; }
        public int? Points { get; set; }
        public int? PromotionId { get; set; }
        public string? Color { get; set; } 
        public string? PrizeText { get; set; } 
        public string? SectionImage { get; set; }       // saved relative path, e.g. "/images/spingames/sections/xxx.jpg"
        public IFormFile? SectionImageFile { get; set; } // the uploaded file, not persisted directly 
    }

    public class AddUpdateSpinGameRequest
    {
        public int GameId { get; set; } = 0;
        public int BusinessId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? GameImage { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? GameImageFile { get; set; }
        public int ConfigId { get; set; }
        public int CreatedByAdminId { get; set; }
        public bool IsActive { get; set; } = true;

        public List<SpinGameConfigRequest> Configs { get; set; } = new();
        public List<SpinSectionRequest> Sections { get; set; } = new();
        public string? BusinessLocation { get; set; }
    }

    public class PlaySpinRequest
    {
        public int GameId { get; set; }
        public Guid UserId { get; set; }
        public int SectionId { get; set; }
    }

    public class PlaySpinResponse
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public SpinSectionRequest? SelectedSection { get; set; }
        public int CoinsEarned { get; set; }
        public int GameResultId { get; set; }
        public int GameId { get; set; }
        public int SectionId { get; set; }
        public string? RewardValue { get; set; }
        public string? RedeemCode { get; set; }
        public string? QRCodePath { get; set; }
        public string? BusinessLocation { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PlayedAt { get; set; }
    }
    public class GameSpinResultDto
    {
        public int SpinId { get; set; }
        public Guid UserId { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public DateTime SpinDate { get; set; }
        public int SelectedSectionId { get; set; }
        public string PrizeText { get; set; } = string.Empty;
        public int? PointsAwarded { get; set; }
        public int? PromotionId { get; set; }
        public string? RedeemCode { get; set; }

        public string? QRCodePath { get; set; }

        public string? BusinessLocation { get; set; }
    }

    // Add more DTOs for configs, sections, spins as needed
}

