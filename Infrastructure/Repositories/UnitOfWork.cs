using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.BAL.Interfaces;
using IUnitOfWork = CommUnityApp.ApplicationCore.Interfaces.IUnitOfWork;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICommunityRepository Community { get; }
        public IEventRepository Events { get; }
        public IBrandGameRepository BrandGames { get; }
        public IBusinessRepository Business { get; }
        public IAuctionRepository Auction { get; }
        public IForgotPasswordRepository ForgotPassword { get; }
        public IUserRepository User { get; }

        public UnitOfWork(
            ICommunityRepository community,
            IEventRepository events,
            IBrandGameRepository brandGames,
            IBusinessRepository business,
            IAuctionRepository auction,
            IForgotPasswordRepository forgotPassword,
            IUserRepository user)
        {
            Community = community;
            Events = events;
            BrandGames = brandGames;
            Business = business;
            Auction = auction;
            ForgotPassword = forgotPassword;
            User = user;
        }
    }
}
