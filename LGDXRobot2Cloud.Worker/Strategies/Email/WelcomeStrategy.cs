using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Worker.Components;
using Microsoft.AspNetCore.Components.Web;
using MimeKit;

namespace LGDXRobot2Cloud.Worker.Strategies.Email;

public class WelcomeStrategy(
    EmailContract emailContract,
    HtmlRenderer htmlRenderer
  ) : IEmailStrategy
{
  private readonly EmailContract _emailContract = emailContract ?? throw new ArgumentNullException(nameof(emailContract));
  private readonly HtmlRenderer _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));

  public async Task<IEnumerable<MimeMessage>> BuildEmailAsync()
  {
    var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
    {
      /*
      var dictionary = new Dictionary<string, object?>
      {
        { "Message", "Hello from the Render Message component!" }
      };
      var parameters = ParameterView.FromDictionary(dictionary);*/
      var output = await _htmlRenderer.RenderComponentAsync<Welcome>();
      return output.ToHtmlString();
    });

    List<MimeMessage> messages = [];
    foreach (var to in _emailContract.Recipients)
    {
      var message = new MimeMessage();
      message.To.Add(new MailboxAddress(to.Name, to.Email));
      message.Subject = "Welcome To LGDXRobot2";
      var emailBodyBuilder = new BodyBuilder
      {
        TextBody = html,
        HtmlBody = html
      };
      message.Body = emailBodyBuilder.ToMessageBody();
      messages.Add(message);
    }
    return messages;
  }
}