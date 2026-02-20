namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IUnitOfWork
    {
        ICommunityRepository Community { get; }
        IEventRepository Events { get; }
        IBrandGameRepository BrandGames { get; }
        IBusinessRepository Business { get; }
    }
}
