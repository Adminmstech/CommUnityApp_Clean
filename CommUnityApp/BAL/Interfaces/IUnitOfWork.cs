namespace CommUnityApp.BAL.Interfaces
{
    public interface IUnitOfWork
    {
        ICommunityRepository Community { get; }
    }
}
