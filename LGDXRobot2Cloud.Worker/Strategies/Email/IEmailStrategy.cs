using MimeKit;

namespace LGDXRobot2Cloud.Worker.Strategies.Email;

public interface IEmailStrategy
{
  Task<IEnumerable<MimeMessage>> BuildEmailAsync();
}