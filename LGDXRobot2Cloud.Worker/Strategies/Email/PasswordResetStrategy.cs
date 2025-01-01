using System.Text.Json;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Worker.Configurations;
using LGDXRobot2Cloud.Worker.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MimeKit;

namespace LGDXRobot2Cloud.Worker.Strategies.Email;

public class PasswordResetStrategy(
    EmailContract emailContract,
    EmailLinksConfiguration emailLinksConfiguration,
    HtmlRenderer htmlRenderer
  ) : IEmailStrategy
{
  private readonly EmailContract _emailContract = emailContract ?? throw new ArgumentNullException(nameof(emailContract));
  private readonly EmailLinksConfiguration _emailLinksConfiguration = emailLinksConfiguration ?? throw new ArgumentNullException(nameof(emailLinksConfiguration));
  private readonly HtmlRenderer _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));
  protected readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

  public async Task<IEnumerable<MimeMessage>> BuildEmailAsync()
  {
    var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
    {
      var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(emailContract.Metadata!, _jsonSerializerOptions);
      var dictionary = new Dictionary<string, object?>
      {
        { "Url", $"{_emailLinksConfiguration.AccessUrl}{_emailLinksConfiguration.PasswordResetPath}" }
      };
      foreach (var data in metadata!)
      {
        dictionary.Add(data.Key, data.Value);
      }
      var parameters = ParameterView.FromDictionary(dictionary);
      var output = await _htmlRenderer.RenderComponentAsync<PasswordReset>(parameters);
      return output.ToHtmlString();
    });

    List<MimeMessage> messages = [];
    foreach (var to in _emailContract.Recipients)
    {
      var message = new MimeMessage();
      message.To.Add(new MailboxAddress(to.Name, to.Email));
      message.Subject = "Reset Your Password";
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