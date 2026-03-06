namespace CommUnityApp.ApplicationCore.Models
{
    public class CommunityLoginResponse
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string Logo { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ResultMessage { get; set; }
    }

    public class CommunityLoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class GroupDto
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class Community
    {
        public long CommunityId { get; set; }

        public string? CommunityName { get; set; }
        public string? Logo { get; set; }
        public string? Description { get; set; }

        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? OtherInfo { get; set; }

        public int? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

 

    public class DashboardResponse
    {
        public int ResultId { get; set; }
        public string? ResultMessage { get; set; }
        public DashboardData? Data { get; set; }
    }

    public class DashboardData
    {
        public List<TopEventDto>? Events { get; set; }
        public List<AuctionListModel>? Auctions { get; set; }
        public List<Community>? Communities { get; set; }
    }
}
