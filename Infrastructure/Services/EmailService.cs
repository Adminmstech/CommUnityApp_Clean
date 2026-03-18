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

        public async Task SendPasswordResetEmailAsync(string toEmail, string otp)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return;

            string subject = "CommUnityApp - Password Reset OTP";
            string body = $@"
<html>
<body>
    <p>Hello,</p>
    <p>Your OTP for password reset is:</p>
    <h2 style='color:#2563eb'>{otp}</h2>
    <p>This OTP is valid for 5 minutes.</p>
    <p>If you did not request this, please ignore this email.</p>
    <br/>
    <p>Regards,<br/>
    CommUnityApp Team</p>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
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


        public async Task SendBusinessUserCredentialsEmailAsync(string toEmail, string password)
        {
            var smtpSection = _configuration.GetSection("SmtpSettings");

            using (var client = new SmtpClient())
            {
                client.Host = smtpSection["Host"];
                client.Port = int.Parse(smtpSection["Port"]);
                client.EnableSsl = bool.Parse(smtpSection["EnableSsl"]);
                client.Credentials = new NetworkCredential(
                    smtpSection["UserName"],
                    smtpSection["Password"]
                );

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSection["UserName"]),
                    Subject = "Your Business Account Credentials",
                    Body = $"Your login password is: {password}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }


        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_configuration["Email:SmtpHost"])
            {
                Port = int.Parse(_configuration["Email:Port"]),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }


        public async Task<bool> SendBulkEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtp = _configuration.GetSection("SMTP");

                var host = smtp["Host"];
                var port = smtp["Port"];
                var username = smtp["Username"];
                var password = smtp["Password"];
                var from = smtp["FromEmail"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
                    throw new Exception("SMTP configuration missing");

                using var smtpClient = new SmtpClient(host)
                {
                    Port = int.Parse(port),
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(from, "CommUnityApp"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
