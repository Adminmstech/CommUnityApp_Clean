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
        public IProductRepository Product { get; }
        public IRewardsRepository Rewards { get; }
        public IOrderRepository Order { get; }
        public IServiceRepository Service { get; }
        public ICampaignRepository Campaign { get; }
        public ISpinGameRepository Spingamemodels { get; }
        public IQuizGameRepository QuizGames { get; }
        public INotificationRepository Notification { get; }
        public IGameResultsRepository GameResults { get; }
        public ICareConnectRepository CareConnect { get; }
       

        public UnitOfWork(
            ICommunityRepository community,
            IEventRepository events,
            IBrandGameRepository brandGames,
            IBusinessRepository business,
            IAuctionRepository auction,
            IForgotPasswordRepository forgotPassword,
            IUserRepository user,
            IProductRepository product,
            IRewardsRepository rewards,
            IOrderRepository order,
            IServiceRepository service,
            ICampaignRepository campaign,
            ISpinGameRepository spingamemodels,
            INotificationRepository notification,
            IGameResultsRepository gameResults,
            ICareConnectRepository careConnect)
            IQuizGameRepository quizGames
                    )
        {
            Community = community;
            Events = events;
            BrandGames = brandGames;
            Business = business;
            Auction = auction;
            ForgotPassword = forgotPassword;
            User = user;
            Product = product;
            Rewards = rewards;
            Order = order;
            Service = service;
            Campaign = campaign;
            Spingamemodels = spingamemodels;
            QuizGames = quizGames;
            Notification = notification;
            GameResults = gameResults;
            CareConnect = careConnect;
        }
    }
}
