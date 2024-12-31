using LGDXRobot2Cloud.Data.Contracts;
using MimeKit;

namespace LGDXRobot2Cloud.Worker.Strategies.Email;

public class WelcomeStrategy(EmailContract emailContract) : IEmailStrategy
{
  private readonly EmailContract _emailContract = emailContract;

  public Task<MimeMessage> BuildEmailAsync()
  {
    var message = new MimeMessage();
    message.To.Add(new MailboxAddress(_emailContract.ReceipentName, _emailContract.ReceipentAddress));
    message.Subject = "Welcome to LGDXRobot2";
    message.Body = new TextPart("plain")
    {
      Text = "Welcome to LGDXRobot2!"
    };
    return Task.FromResult(message);
  }
}