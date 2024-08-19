using LGDXRobot2Cloud.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Services;

public class SqlScriptRunner(LgdxContext context) : IHostedService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var path = Path.Combine(Directory.GetCurrentDirectory(), "SQL", "MySQL.sql");
    string sql = await File.ReadAllTextAsync(path, cancellationToken);
    sql = sql.Replace("delimiter //", string.Empty);
    sql = sql.Replace("delimiter ;", string.Empty);
    sql = sql.Replace("//", ";");
    await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    Environment.Exit(0);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}