namespace CommUnityApp.Models
{
    public class Community
    {
    }

    public class CommunityLoginResponse
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string Logo { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
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

}
