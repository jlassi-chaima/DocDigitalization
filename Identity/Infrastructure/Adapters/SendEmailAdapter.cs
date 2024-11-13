

using Application.Helper;
using Domain.Ports;
using Infrastructure.Adapters.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Serilog;

namespace Infrastructure.Adapters
{
    public class SendEmailAdapter : ISendEmailPort
    {
        private readonly IConfiguration _config;
        private readonly SmtpConfiguration _smtpConfiguration;
        public SendEmailAdapter(IConfiguration config)
        {
            _config = config;
            _smtpConfiguration = _config.GetSection("smtpConfiguration").Get<SmtpConfiguration>()!;



        }
        //"Easydoc Account Creation"
        public  async Task SendEmail(string email, string firstName, string subject, string body)
        {
            try
            {


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Easydoc", _smtpConfiguration.Email));
                message.To.Add(new MailboxAddress(firstName, email));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = body

                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("imap.gmail.com", 587, false);
                    await client.AuthenticateAsync(_smtpConfiguration.Email, _smtpConfiguration.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send email.Error Message: {ex.Message}");
                throw new Exception(ex.Message); 
            }
        }

       
    }
}
