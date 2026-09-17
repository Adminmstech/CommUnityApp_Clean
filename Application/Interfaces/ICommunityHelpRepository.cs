using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace CommUnityApp.ApplicationCore.Interfaces
{
   
    
        public interface ICommunityHelpRepository
        {
            Task<CommunityHelpResult> CreateHelpRequest(CreateCommunityHelpRequestModel model);

            Task<(int TotalRecords, List<CommunityHelpRequestResponse> Data)>
            GetCommunityHelpRequests(long communityId,Guid? userId,int pageNumber,int pageSize,string? search,string? status);

            Task<CommunityHelpRequestDetailsResponse>GetHelpRequestDetails(long helpRequestId);

            Task<CommunityHelpResult> CreateHelpOffer(CreateCommunityHelpOfferModel model);

            Task<List<CommunityHelpOfferResponse>>GetHelpOffers(long helpRequestId);

            Task<(int TotalRecords, List<CommunityHelpMyRequestResponse> Data)>GetMyHelpRequests(Guid userId,int pageNumber,int pageSize,string? status);

            Task<(int TotalRecords, List<CommunityHelpMyOfferResponse> Data)>GetMyHelpOffers( Guid userId,int pageNumber,int pageSize,string? status);

            Task<CommunityHelpResult> ConnectHelper(ConnectCommunityHelpModel model);

            Task<CommunityHelpResult> CompleteHelp(CompleteCommunityHelpModel model);

            Task<CommunityHelpResult> CloseHelpRequest( CloseCommunityHelpRequestModel model);

            Task<CommunityHelpResult> CancelHelpRequest(CancelCommunityHelpRequestModel model);
        }
    
}
