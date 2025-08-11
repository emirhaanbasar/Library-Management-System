using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Library.Web.Services
{
    public class MailService
    {
        private readonly IConfiguration _config;
        public MailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendMailAsync(string to, string subject, string body)
        {
            try
            {
                Console.WriteLine($"Mail gönderiliyor: {to}");
                Console.WriteLine($"SMTP Host: {_config["Smtp:Host"]}");
                Console.WriteLine($"SMTP Port: {_config["Smtp:Port"]}");
                Console.WriteLine($"SMTP User: {_config["Smtp:User"]}");
                
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["Smtp:From"]));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
                
                using var smtp = new SmtpClient();
                // SSL sertifika doğrulamasını devre dışı bırak
                smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                await smtp.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["Smtp:User"], _config["Smtp:Pass"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                
                Console.WriteLine($"Mail başarıyla gönderildi: {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail gönderimi hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 