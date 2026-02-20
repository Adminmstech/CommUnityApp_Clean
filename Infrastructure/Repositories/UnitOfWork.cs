using CommUnityApp.ApplicationCore.Interfaces;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICommunityRepository Community { get; }
        public IEventRepository Events { get; }
        public IBrandGameRepository BrandGames { get; }
        public IBusinessRepository Business { get; }

        public UnitOfWork(
            ICommunityRepository community,
            IEventRepository events,
            IBrandGameRepository brandGames,
            IBusinessRepository business)
        {
            Community = community;
            Events = events;
            BrandGames = brandGames;
            Business = business;
        }
    }
}
