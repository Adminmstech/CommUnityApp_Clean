namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IUnitOfWork
    {
        ICommunityRepository Community { get; }
        IEventRepository Events { get; }
        IBrandGameRepository BrandGames { get; }
        IBusinessRepository Business { get; }
        IAuctionRepository Auction { get; }
        IForgotPasswordRepository ForgotPassword { get; }
        IUserRepository User { get; }
        IProductRepository Product { get; }
        IRewardsRepository Rewards { get; }
        IOrderRepository Order { get; }
        IServiceRepository Service { get; }
        ICampaignRepository Campaign { get; }
        ISpinGameRepository Spingamemodels { get;}
    }
}
