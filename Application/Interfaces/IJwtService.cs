namespace CommUnityApp.BAL.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, string role);
    }

}
