using LGDXRobotCloud.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.DbContexts;

public class LgdxLogsContext(DbContextOptions<LgdxLogsContext> options) : DbContext(options)
{
  public DbSet<ActivityLog> ActivityLogs { get; set; }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    configurationBuilder
      .Properties<DateTime>()
      .HaveConversion<UtcValueConverter>();
  }
}