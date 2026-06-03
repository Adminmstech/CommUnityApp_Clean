using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class CareConnect
    {
    }
    public class CareRequestModel
    {
        public Guid UserId { get; set; }
        public int ServiceId { get; set; }
        public string Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; }
        public bool IsCustomLocation { get; set; }
    }

    public class SendMessageModel
    {
        public long ChatThreadId { get; set; }
        public Guid SenderId { get; set; }
        public string Message { get; set; }
    }

    public class SelectSupporterModel
    {
        public long RequestId { get; set; }
        public Guid UserId { get; set; }
        public Guid SupporterId { get; set; }
        public string ServiceName { get; set; }
    }
    public class ConnectSupporterModel
    {
        public Guid UserId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public Guid SupporterId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; }
    }


    public class RespondRequestModel
    {
        public long RequestId { get; set; }
        public Guid SupporterId { get; set; }
        public string Status { get; set; } 
    }

    public class FinalizeSupporterModel
    {
        public long RequestId { get; set; }
        public Guid SupporterId { get; set; }
    }

    public class CreateRequestWithSupportersModel
    {
        public Guid UserId { get; set; }

        public int ServiceId { get; set; }

        public string Description { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string Address { get; set; }

        public List<Guid> SupporterIds { get; set; }
    }

    public class AddSupporterServiceModel
    {
        public Guid UserId { get; set; }

        public string ServiceIds { get; set; }
    }

    public class SendCareMessageModel
    {
        public long ChatThreadId { get; set; }

        public Guid SenderId { get; set; }

        public string MessageText { get; set; }

        public string MessageType { get; set; } = "Text";
    }

    public class VolunteerRequestModel
    {
        public Guid UserId { get; set; }
    }

    public class CareConnectDashboardResponse
    {
        public int TotalRequests { get; set; }
        public int TotalUsers { get; set; }
        public List<CareConnectRequestItem> Requests { get; set; }
    }

    public class CareConnectRequestItem
    {
        public int RequestId { get; set; }
        public string UserName { get; set; }
        public string Mobile { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}


