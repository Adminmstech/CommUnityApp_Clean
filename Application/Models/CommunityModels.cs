using Microsoft.AspNetCore.Http;

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
        public string ItemCategory { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public Guid AssignedToUserId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string AssignedDateString {
            get
            {
                return AssignedDate.HasValue
                    ? AssignedDate.Value.ToString("dd MMM yyyy")
                    : "";
            }
        }
        public DateTime? CreatedDate { get; set; }

    }
    public class AssignVolunteerRequest
    {
        public long CharityItemId { get; set; }
        public Guid AssignedToUserId { get; set; }
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
            public Rewards Rewards { get; set; }
            public List<TopEventDto>? Events { get; set; }
            public List<AuctionListModel>? Auctions { get; set; }
            public List<Community>? Communities { get; set; }
            public List<BusinessDetailsDto> Businesses { get; set; }
        }
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

    public class CharityRequestModel
    {
        public int RequestId { get; set; }
        public int CharityItemId { get; set; }
        public string ItemCode { get; set; }

        public string ItemName { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public string RequestedUserName { get; set; }
        public string Mobile { get; set; }
        public int RequestedQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string VolunteerName { get; set; }
        public string DeliveryStatus { get; set; }
    }

    public class AddCharityItemModel
    {
        public long CommunityId { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string Description { get; set; } 
        public int Quantity { get; set; }
        public Guid PostedByUserId { get; set; }
        public string FileName { get; set; }
        public string ImagePath { get; set; }
    }
    public class RequestCharityItemModel
    {
        public int CharityItemId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public int RequestedQuantity { get; set; }
        public string Description { get; set; }  // NEW
    }

    public class RequestedUserModel
    {
        public int RequestId { get; set; }
        public int CharityItemId { get; set; }
        public int RequestedQuantity { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid UserId { get; set; }
        public string RequestedUserName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

   

    public class AssignVolunteerModel
    {
        public int RequestId { get; set; }
        public Guid VolunteerId { get; set; }
    }

    public class CharityItemListModel
    {
        public int CharityItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedDateString
        {
            get
            {
                return CreatedDate.HasValue
                    ? CreatedDate.Value.ToString("dd MMM yyyy")
                    : "";
            }
        }
        public string DeliveryStatus { get; set; }
    }
    public class MyRequestedItemsModel
    {
        public int RequestId { get; set; }
        public int CharityItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string ItemDescription { get; set; }
        public string ImagePath { get; set; }
        public int RequestedQuantity { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedDateString
        {
            get
            {
                return CreatedDate.HasValue
                    ? CreatedDate.Value.ToString("dd MMM yyyy")
                    : "";
            }
        }
        public DateTime? AssignedDate { get; set; }
        public string AssignedDateString
        {
            get
            {
                return AssignedDate.HasValue
                    ? AssignedDate.Value.ToString("dd MMM yyyy")
                    : "";
            }
        }
        public string DeliveryStatus { get; set; }
        public string VolunteerName { get; set; }
        public string VolunteerMobile { get; set; }
    }

    public class ItemCategoryModel
    {
        public int CategoryId { get; set; }
        public string ItemCategory{ get; set; }
    }
}
