namespace HydroLink.Services;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpClient = new SmtpClient(_config["Smtp:Host"])
        {
            Port = int.Parse(_config["Smtp:Port"]),
            Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
