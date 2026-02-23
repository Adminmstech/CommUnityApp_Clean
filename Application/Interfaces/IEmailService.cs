namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IEmailService
    {
        Task SendRegistrationEmailAsync(string toEmail, string name, string eventName, string registrationUrl, string qrImagePath);
        Task SendWelcomeEmailAsync(string toEmail, string fullName, string password);
        Task SendPasswordResetEmailAsync(string toEmail, string otp);
        Task SendPasswordResetSuccessEmailAsync(string toEmail);

    }
}
