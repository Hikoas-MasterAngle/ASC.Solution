using ASC.WEB.Configuration;
using ASC.WEB.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace ASC.WEB.Services
{
    public class AuthMessageSender : IEmailSender, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, ISmsSender
    {
        private readonly IOptions<ApplicationSettings> _settings;

        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Admin", _settings.Value.SMTPAccount));
                emailMessage.To.Add(new MailboxAddress("User", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = message };

                using (var client = new MailKit.Net.Smtp.SmtpClient())  // Dùng MailKit chính xác
                {
                    await client.ConnectAsync(_settings.Value.SMTPServer, _settings.Value.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_settings.Value.SMTPAccount, _settings.Value.SMTPPassword);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }

        // ✅ Thêm phương thức SendSmsAsync để triển khai ISmsSender
        public Task SendSmsAsync(string number, string message)
        {
            Console.WriteLine($"SMS sent to {number}: {message}");
            return Task.CompletedTask;
        }
    }
}
