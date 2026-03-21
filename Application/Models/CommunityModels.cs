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

    public class CharityItem
    {
        public long CharityItemId { get; set; }
        public long CommunityId { get; set; }
        public Guid PostedByUserId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public Guid AssignedToUserId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
    public class AssignVolunteerRequest
    {
        public long CharityItemId { get; set; }
        public Guid AssignedToUserId { get; set; }
    }
    public class UpdateStatusRequest
    {
        public int CharityItemId { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } // Accepted / Delivered
    }
    public class AssignedVolunteerModel
    {
        public int CharityItemId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string DeliveryStatus { get; set; }

        public DateTime AssignedDate { get; set; }
    }

    public class CharityItemModel
    {
        public long CharityItemId { get; set; }
        public long CommunityId { get; set; }
        public Guid PostedByUserId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public string PostedMemberName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        public Guid AssignedToUserId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
