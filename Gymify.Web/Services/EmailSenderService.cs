using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace Gymify.Web.Services;

public class EmailSenderService : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSenderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            emailSettings["SenderName"],
            emailSettings["SenderEmail"]
        ));

        message.To.Add(new MailboxAddress("", email));

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(
                    emailSettings["MailServer"],
                    int.Parse(emailSettings["MailPort"]),
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    emailSettings["SenderEmail"],
                    emailSettings["Password"]
                );

                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EMAIL SENDING: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}