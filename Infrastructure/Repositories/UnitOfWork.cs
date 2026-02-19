using CommUnityApp.ApplicationCore.Interfaces;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICommunityRepository Community { get; }
        public UnitOfWork(ICommunityRepository community)
        {
            Community = community;
        }
    }
}
