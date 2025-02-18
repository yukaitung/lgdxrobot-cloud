using System.Text.Json;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Worker.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MimeKit;

namespace LGDXRobotCloud.Worker.Strategies.Email;

public class RobotStuckStrategy(
    EmailContract emailContract,
    HtmlRenderer htmlRenderer
  ) : IEmailStrategy
{
  private readonly EmailContract _emailContract = emailContract ?? throw new ArgumentNullException(nameof(emailContract));
  private readonly HtmlRenderer _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));
  protected readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

  public async Task<IEnumerable<MimeMessage>> BuildEmailAsync()
  {
    var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
    {
      var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(emailContract.Metadata!, _jsonSerializerOptions);
      var dictionary = new Dictionary<string, object?>();
      foreach (var data in metadata!)
      {
        dictionary.Add(data.Key, data.Value);
      }
      var parameters = ParameterView.FromDictionary(dictionary);
      var output = await _htmlRenderer.RenderComponentAsync<RobotStuck>(parameters);
      return output.ToHtmlString();
    });

    List<MimeMessage> messages = [];
    foreach (var to in _emailContract.Recipients)
    {
      var message = new MimeMessage();
      message.To.Add(new MailboxAddress(to.Name, to.Email));
      message.Subject = "Action Needed: Your Robot Encountered an Issue";
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