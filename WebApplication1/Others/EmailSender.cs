using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApplication1.Others;

public class EmailSender: IEmailSender
{
    private readonly EmailConfiguration _emailConfiguration;

    public EmailSender(EmailConfiguration emailConfiguration)
    {
        _emailConfiguration = emailConfiguration ?? throw new InvalidOperationException("EmailSender Class was not implemented");
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {

        var mailMessage = new MailMessage();
        mailMessage.Subject = subject;
        mailMessage.To.Add(email);
        mailMessage.From = new MailAddress(_emailConfiguration.From);
        mailMessage.IsBodyHtml = true;
        mailMessage.Body = htmlMessage;

        using (var client = new SmtpClient(_emailConfiguration.SmtpServer))
        {
            client.Port = _emailConfiguration.Port;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_emailConfiguration.Username, _emailConfiguration.Password);
            client.UseDefaultCredentials = false;// make false cause default true

            try
            {
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Succesfully created and sent {email}");
            }
            catch (SmtpException se)
            {
                Console.WriteLine($"Cannot create SMTP Client {se}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Generic Exception: {e}");
                throw;
            }
        }


    }
}