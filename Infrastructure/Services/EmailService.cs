using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace CommUnityApp.InfrastructureLayer.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string otp)
        {
            throw new NotImplementedException();
        }

        public Task SendPasswordResetSuccessEmailAsync(string toEmail)
        {
            throw new NotImplementedException();
        }

        public async Task SendRegistrationEmailAsync(
            string toEmail,
            string name,
            string eventName,
            string registrationUrl,
            string qrImagePath)
        {
            var smtpSection = _configuration.GetSection("SMTP");

            string host = smtpSection["Host"];
            int port = int.Parse(smtpSection["Port"]);
            string username = smtpSection["Username"];
            string password = smtpSection["Password"];
            string fromEmail = smtpSection["FromEmail"];

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, "Event Team"),
                Subject = $"🎉 Registration Confirmed - {eventName}",
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            var qrAttachment = new Attachment(qrImagePath);
            qrAttachment.ContentId = "qrCodeImage";
            qrAttachment.ContentDisposition.Inline = true;
            qrAttachment.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
            mail.Attachments.Add(qrAttachment);

            mail.Body = $@"
        <div style='font-family: Arial; background:#f4f6f9; padding:20px;'>
            <div style='max-width:600px; margin:auto; background:#ffffff;
                        border-radius:10px; padding:30px; box-shadow:0 5px 15px rgba(0,0,0,0.1);'>
                
                <h2 style='color:#2c3e50;'>Hello {name},</h2>

                <p style='font-size:16px;'>
                    Your registration for <strong>{eventName}</strong> has been successfully confirmed.
                </p>

                <div style='text-align:center; margin:30px 0;'>
                    <img src='cid:qrCodeImage' width='200'/>
                </div>

                <p style='font-size:15px;'>
                    Please show this QR code at the event entry.
                </p>

                <p style='margin-top:20px;'>
                    Or click below to view your booking details:
                </p>

                <p>
                    <a href='{registrationUrl}' 
                       style='background:#007bff; color:#ffffff; 
                               padding:10px 20px; text-decoration:none; 
                               border-radius:5px;'>
                        View Registration Details
                    </a>
                </p>

                <hr/>

                <p style='font-size:12px; color:#888;'>
                    This is an automated email. Please do not reply.
                </p>

            </div>
        </div>";

            await client.SendMailAsync(mail);
        }

        public Task SendWelcomeEmailAsync(string toEmail, string fullName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
