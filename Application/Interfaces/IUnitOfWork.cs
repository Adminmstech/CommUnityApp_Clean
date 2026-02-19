namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IUnitOfWork
    {
        ICommunityRepository Community { get; }
    }
}
