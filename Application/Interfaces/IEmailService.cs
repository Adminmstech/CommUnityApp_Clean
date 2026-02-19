namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IEmailService
    {
        Task SendRegistrationEmailAsync(string toEmail, string name, string eventName, string registrationUrl, string qrImagePath);
    }
}
