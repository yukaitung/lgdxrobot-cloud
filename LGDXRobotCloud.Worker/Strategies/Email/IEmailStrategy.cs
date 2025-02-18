using MimeKit;

namespace LGDXRobotCloud.Worker.Strategies.Email;

public interface IEmailStrategy
{
  Task<IEnumerable<MimeMessage>> BuildEmailAsync();
}