using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class CreateCommunityHelpRequestModel
    {
        public long CommunityId { get; set; }

        public Guid RequestedByUserId { get; set; }

        public string RequestTitle { get; set; } = string.Empty;

        public string RequestDescription { get; set; } = string.Empty;

        public string? Location { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsUrgent { get; set; }
    }

    public class CreateCommunityHelpOfferModel
    {
        public long HelpRequestId { get; set; }

        public Guid OfferedByUserId { get; set; }

        public string OfferMessage { get; set; } = string.Empty;
    }


    public class ConnectCommunityHelpModel
    {
        public long HelpRequestId { get; set; }

        public long HelpOfferId { get; set; }

        public Guid RequesterUserId { get; set; }
    }


    public class CompleteCommunityHelpModel
    {
        public long ConnectionId { get; set; }

        public Guid UserId { get; set; }
    }


    public class CloseCommunityHelpRequestModel
    {
        public long HelpRequestId { get; set; }

        public Guid UserId { get; set; }
    }


    public class CancelCommunityHelpRequestModel
    {
        public long HelpRequestId { get; set; }

        public Guid UserId { get; set; }
    }


    public class CommunityHelpResult
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; } = string.Empty;

        public long? HelpRequestId { get; set; }

        public long? HelpOfferId { get; set; }

        public long? ConnectionId { get; set; }

        public Guid? HelperUserId { get; set; }
    }

    public class CommunityHelpRequestResponse
    {
        public long HelpRequestId { get; set; }

        public long CommunityId { get; set; }

        public Guid RequestedByUserId { get; set; }

        public string? RequesterName { get; set; }

        public string? RequesterProfileImage { get; set; }

        public string RequestTitle { get; set; } = string.Empty;

        public string RequestDescription { get; set; } = string.Empty;

        public string? Location { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsUrgent { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public int ResponseCount { get; set; }
    }


    public class CommunityHelpOfferResponse
    {
        public long HelpOfferId { get; set; }

        public long HelpRequestId { get; set; }

        public Guid OfferedByUserId { get; set; }

        public string? HelperName { get; set; }

        public string? HelperProfileImage { get; set; }

        public string OfferMessage { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public long? ConnectionId { get; set; }

        public string? ConnectionStatus { get; set; }
    }


    public class CommunityHelpRequestDetailsResponse
    {
        public CommunityHelpRequestResponse? HelpRequest { get; set; }

        public List<CommunityHelpOfferResponse> Responses { get; set; }
            = new List<CommunityHelpOfferResponse>();
    }


    public class CommunityHelpMyRequestResponse
    {
        public long HelpRequestId { get; set; }

        public long CommunityId { get; set; }

        public Guid RequestedByUserId { get; set; }

        public string RequestTitle { get; set; } = string.Empty;

        public string RequestDescription { get; set; } = string.Empty;

        public string? Location { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsUrgent { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public int ResponseCount { get; set; }
    }


    public class CommunityHelpMyOfferResponse
    {
        public long HelpOfferId { get; set; }

        public long HelpRequestId { get; set; }

        public long CommunityId { get; set; }

        public Guid RequestedByUserId { get; set; }

        public string? RequesterName { get; set; }

        public string? RequesterProfileImage { get; set; }

        public string RequestTitle { get; set; } = string.Empty;

        public string RequestDescription { get; set; } = string.Empty;

        public string? Location { get; set; }

        public bool IsUrgent { get; set; }

        public string RequestStatus { get; set; } = string.Empty;

        public string OfferMessage { get; set; } = string.Empty;

        public string OfferStatus { get; set; } = string.Empty;

        public DateTime OfferCreatedDate { get; set; }

        public long? ConnectionId { get; set; }

        public string? ConnectionStatus { get; set; }

        public DateTime? ConnectedDate { get; set; }

        public DateTime? CompletedDate { get; set; }
    }

}
