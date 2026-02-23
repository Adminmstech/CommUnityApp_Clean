using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IAuctionRepository
    {
        Task<BaseResponse> SaveAuction(Auction entity);
        Task<BaseResponse> SaveAuctionItemImage(AuctionItemImage entity);
        Task<List<ItemType>> GetItemType();
        Task<List<AuctionListModel>> GetAuctions();
        Task<List<AuctionItemImage>> GetAuctionImages(int auctionId);
        Task<List<GetAuctionImagedModel>> GetAuctionImagesByIds(List<int> auctionIds);
        Task<List<AuctionListModel>> GetAuctionAuctionId(int auctionId);
        Task<List<AuctionListModel>> GetAuctionByItemTypeId(int ItemTypeId);
        Task<List<AuctionListModel>> GetTop5Auctions();
        Task<PlaceBidResponse> PlaceBid(PlaceBidRequest request);
        Task<List<BidDto>> GetRecentBids(int auctionId);

        Task<BaseResponse> SaveBidRegistration(BidRegistration entity);

    }
}
