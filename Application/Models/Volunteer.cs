using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    internal class Volunteer
    {
    }

    public class UpdateStatusModel
    {
        public int ApplicationId { get; set; }
        public string Status { get; set; } 
        public string Remarks { get; set; }
        public Guid UpdatedBy { get; set;}
    }

    public class VolunteerAssignedItemModel
    {
        public int RequestId { get; set; }
        public int CharityItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string ImagePath { get; set; }
        public int RequestedQuantity { get; set; }
        public string Status { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequestedUserName { get; set; }
        public string RequestedUserMobile { get; set; }
        public string DeliveryStatus { get; set; }
    }

    public class VolunteerStatusUpdateModel
    {
        public int RequestId { get; set; }
        public string Status { get; set; }
    }

    public class VolunteerLoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
