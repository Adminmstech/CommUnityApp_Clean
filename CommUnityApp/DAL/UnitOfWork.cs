using CommUnityApp.BAL.Interfaces;

namespace CommUnityApp.DAL
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
