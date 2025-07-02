using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IRobotService
{
  Task<(IEnumerable<RobotListBusinessModel>, PaginationHelper)> GetRobotsAsync(int? realmId, string? name, int pageNumber, int pageSize);
  Task<RobotBusinessModel> GetRobotAsync(Guid robotId);
  Task<RobotCreateResponseBusinessModel> CreateRobotAsync(RobotCreateBusinessModel robotCreateBusinessModel);
  Task<bool> UpdateRobotAsync(Guid id, RobotUpdateBusinessModel robotUpdateDtoBusinessModel);
  Task<RobotChassisInfoBusinessModel?> GetRobotChassisInfoAsync(Guid robotId);
  Task<bool> UpdateRobotChassisInfoAsync(Guid id, RobotChassisInfoUpdateBusinessModel robotChassisInfoUpdateBusinessModel);
  Task<bool> TestDeleteRobotAsync(Guid id);
  Task<bool> DeleteRobotAsync(Guid id);

  Task<RobotSystemInfoBusinessModel?> GetRobotSystemInfoAsync(Guid robotId);
  Task<bool> CreateRobotSystemInfoAsync(Guid robotId, RobotSystemInfoCreateBusinessModel robotSystemInfoCreateBusinessModel);
  Task<bool> UpdateRobotSystemInfoAsync(Guid robotId, RobotSystemInfoUpdateBusinessModel robotSystemInfoUpdateBusinessModel);

  Task<IEnumerable<RobotSearchBusinessModel>> SearchRobotsAsync(int realmId, string? name, Guid? robotId);

  Task<int?> GetRobotRealmIdAsync(Guid robotId);
  Task<bool> GetRobotIsRealtimeExchange(Guid robotId);
}

public class RobotService(
    IActivityLogService activityLogService,
    IMemoryCache memoryCache,
    IRobotCertificateService robotCertificateService,
    LgdxContext context
  ) : IRobotService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRobotCertificateService _robotCertificateService = robotCertificateService ?? throw new ArgumentNullException(nameof(robotCertificateService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<RobotListBusinessModel>, PaginationHelper)> GetRobotsAsync(int? realmId, string? name, int pageNumber, int pageSize)
  {
    var query = _context.Robots as IQueryable<Robot>;
    if (realmId != null)
    {
      query = query.Where(r => r.RealmId == realmId);
    }
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(r => r.Name.ToLower().Contains(name.ToLower()));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var robots = await query.AsNoTracking()
      .OrderBy(r => r.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(r => new RobotListBusinessModel {
        Id = r.Id,
        Name = r.Name,
        RealmId = r.RealmId,
        RealmName = r.Realm.Name,
      })
      .AsSplitQuery()
      .ToListAsync();
    return (robots, PaginationHelper);
  }

  public async Task<RobotBusinessModel> GetRobotAsync(Guid robotId)
  {
    return await _context.Robots.AsNoTracking()
      .Where(r => r.Id == robotId)
      .Select(r => new RobotBusinessModel {
        Id = r.Id,
        Name = r.Name,
        RealmId = r.RealmId,
        RealmName = r.Realm.Name,
        IsRealtimeExchange = r.IsRealtimeExchange,
        IsProtectingHardwareSerialNumber = r.IsProtectingHardwareSerialNumber,
        RobotCertificate = new RobotCertificateBusinessModel {
          Id = r.RobotCertificate.Id,
          RobotId = r.Id,
          RobotName = r.Name,
          Thumbprint = r.RobotCertificate.Thumbprint,
          ThumbprintBackup = r.RobotCertificate.ThumbprintBackup,
          NotBefore = r.RobotCertificate.NotBefore,
          NotAfter = r.RobotCertificate.NotAfter,
        },
        RobotSystemInfo = r.RobotSystemInfo == null ? null : new RobotSystemInfoBusinessModel {
          Id = r.RobotSystemInfo.Id,
          Cpu = r.RobotSystemInfo.Cpu,
          IsLittleEndian = r.RobotSystemInfo.IsLittleEndian,
          Motherboard = r.RobotSystemInfo.Motherboard,
          MotherboardSerialNumber = r.RobotSystemInfo.MotherboardSerialNumber,
          RamMiB = r.RobotSystemInfo.RamMiB,
          Gpu = r.RobotSystemInfo.Gpu,
          Os = r.RobotSystemInfo.Os,
          Is32Bit = r.RobotSystemInfo.Is32Bit,
          McuSerialNumber = r.RobotSystemInfo.McuSerialNumber,
        },
        RobotChassisInfo = r.RobotChassisInfo == null ? null : new RobotChassisInfoBusinessModel {
          Id = r.RobotChassisInfo.Id,
          RobotTypeId = r.RobotChassisInfo.RobotTypeId,
          ChassisLengthX = r.RobotChassisInfo.ChassisLengthX,
          ChassisLengthY = r.RobotChassisInfo.ChassisLengthY,
          ChassisWheelCount = r.RobotChassisInfo.ChassisWheelCount,
          ChassisWheelRadius = r.RobotChassisInfo.ChassisWheelRadius,
          BatteryCount = r.RobotChassisInfo.BatteryCount,
          BatteryMaxVoltage = r.RobotChassisInfo.BatteryMaxVoltage,
          BatteryMinVoltage = r.RobotChassisInfo.BatteryMinVoltage,
        },
        AssignedTasks = r.AssignedTasks
          .Where(t =>  t.CurrentProgressId != (int)ProgressState.Aborted 
                    && t.CurrentProgressId != (int)ProgressState.Completed 
                    && t.CurrentProgressId != (int)ProgressState.Template)
          .OrderByDescending(t => t.CurrentProgressId)
          .ThenBy(t => t.Id)
          .Select(t => new AutoTaskListBusinessModel {
          Id = t.Id,
          Name = t.Name,
          Priority = t.Priority,
          FlowId = t.FlowId,
          FlowName = t.Flow!.Name,
          RealmId = r.RealmId,
          RealmName = r.Realm.Name,
          AssignedRobotId = r.Id,
          AssignedRobotName = r.Name,
          CurrentProgressId = t.CurrentProgressId,
          CurrentProgressName = t.CurrentProgress.Name,
        })
        .ToList(),
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  public async Task<RobotCreateResponseBusinessModel> CreateRobotAsync(RobotCreateBusinessModel robotCreateBusinessModel)
  {
    var realm = await _context.Realms.Where(r => r.Id == robotCreateBusinessModel.RealmId).AnyAsync();
    if (realm == false)
    {
      throw new LgdxValidation400Expection(nameof(robotCreateBusinessModel.RealmId), "Realm does not exist.");
    }

    var id = Guid.CreateVersion7();
    var robotCertificate = await _robotCertificateService.IssueRobotCertificateAsync(id);
    var robot = new Robot {
      Id = id,
      Name = robotCreateBusinessModel.Name,
      RealmId = robotCreateBusinessModel.RealmId,
      IsRealtimeExchange = robotCreateBusinessModel.IsRealtimeExchange,
      IsProtectingHardwareSerialNumber = robotCreateBusinessModel.IsProtectingHardwareSerialNumber,
      RobotCertificate = new RobotCertificate {
        Thumbprint = robotCertificate.RobotCertificateThumbprint,
        NotBefore = DateTime.SpecifyKind(robotCertificate.RobotCertificateNotBefore, DateTimeKind.Utc),
        NotAfter = DateTime.SpecifyKind(robotCertificate.RobotCertificateNotAfter, DateTimeKind.Utc)
      },
      RobotChassisInfo = new RobotChassisInfo {
        RobotTypeId = robotCreateBusinessModel.RobotChassisInfo.RobotTypeId,
        ChassisLengthX = robotCreateBusinessModel.RobotChassisInfo.ChassisLengthX,
        ChassisLengthY = robotCreateBusinessModel.RobotChassisInfo.ChassisLengthY,
        ChassisWheelCount = robotCreateBusinessModel.RobotChassisInfo.ChassisWheelCount,
        ChassisWheelRadius = robotCreateBusinessModel.RobotChassisInfo.ChassisWheelRadius,
        BatteryCount = robotCreateBusinessModel.RobotChassisInfo.BatteryCount,
        BatteryMaxVoltage = robotCreateBusinessModel.RobotChassisInfo.BatteryMaxVoltage,
        BatteryMinVoltage = robotCreateBusinessModel.RobotChassisInfo.BatteryMinVoltage,
      }
    };
    await _context.Robots.AddAsync(robot);
    await _context.SaveChangesAsync();
    
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Robot),
      EntityId = robot.Id.ToString(),
      Action = ActivityAction.Create,
    });

    return new RobotCreateResponseBusinessModel
    {
      RobotId = robot.Id,
      RobotName = robot.Name,
      RootCertificate = robotCertificate.RootCertificate,
      RobotCertificatePrivateKey = robotCertificate.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = robotCertificate.RobotCertificatePublicKey
    };
  }

  public async Task<bool> UpdateRobotAsync(Guid id, RobotUpdateBusinessModel robotUpdateDtoBusinessModel)
  {
    bool result = await _context.Robots
      .Where(r => r.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(r => r.Name, robotUpdateDtoBusinessModel.Name)
        .SetProperty(r => r.IsRealtimeExchange, robotUpdateDtoBusinessModel.IsRealtimeExchange)
        .SetProperty(r => r.IsProtectingHardwareSerialNumber, robotUpdateDtoBusinessModel.IsProtectingHardwareSerialNumber)
      ) == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = id.ToString(),
        Action = ActivityAction.Update,
      });
    }
    return result;
  }

  public async Task<bool> UpdateRobotChassisInfoAsync(Guid id, RobotChassisInfoUpdateBusinessModel robotChassisInfoUpdateBusinessModel)
  {
    bool result = await _context.RobotChassisInfos
      .Where(r => r.RobotId == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(r => r.RobotTypeId, robotChassisInfoUpdateBusinessModel.RobotTypeId)
        .SetProperty(r => r.ChassisLengthX, robotChassisInfoUpdateBusinessModel.ChassisLengthX)
        .SetProperty(r => r.ChassisLengthY, robotChassisInfoUpdateBusinessModel.ChassisLengthY)
        .SetProperty(r => r.ChassisWheelCount, robotChassisInfoUpdateBusinessModel.ChassisWheelCount)
        .SetProperty(r => r.ChassisWheelRadius, robotChassisInfoUpdateBusinessModel.ChassisWheelRadius)
        .SetProperty(r => r.BatteryCount, robotChassisInfoUpdateBusinessModel.BatteryCount)
        .SetProperty(r => r.BatteryMaxVoltage, robotChassisInfoUpdateBusinessModel.BatteryMaxVoltage)
        .SetProperty(r => r.BatteryMinVoltage, robotChassisInfoUpdateBusinessModel.BatteryMinVoltage)
      ) == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = id.ToString(),
        Action = ActivityAction.Update,
      });
    }
    return result;
  }

  public async Task<RobotChassisInfoBusinessModel?> GetRobotChassisInfoAsync(Guid robotId)
  {
    return await _context.RobotChassisInfos.AsNoTracking()
      .Where(r => r.RobotId == robotId)
      .Select(r => new RobotChassisInfoBusinessModel {
        Id = r.Id,
        RobotTypeId = r.RobotTypeId,
        ChassisLengthX = r.ChassisLengthX,
        ChassisLengthY = r.ChassisLengthY,
        ChassisWheelCount = r.ChassisWheelCount,
        ChassisWheelRadius = r.ChassisWheelRadius,
        BatteryCount = r.BatteryCount,
        BatteryMaxVoltage = r.BatteryMaxVoltage,
        BatteryMinVoltage = r.BatteryMinVoltage,
      }).FirstOrDefaultAsync();
  }

  public async Task<bool> TestDeleteRobotAsync(Guid id)
  {
    var dependencies = await _context.AutoTasks
      .Where(t => t.AssignedRobotId == id)
      .Where(t => t.CurrentProgressId != (int)ProgressState.Completed && t.CurrentProgressId != (int)ProgressState.Aborted)
      .CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(id), $"This robot has been used by {dependencies} running/waiting/template tasks.");
    }
    return true;
  }

  public async Task<bool> DeleteRobotAsync(Guid id)
  {
    bool result = await _context.Robots.Where(r => r.Id == id)
      .ExecuteDeleteAsync() == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = id.ToString(),
        Action = ActivityAction.Delete,
      });
    }
    return result;
  }

  public async Task<RobotSystemInfoBusinessModel?> GetRobotSystemInfoAsync(Guid robotId)
  {
    return await _context.RobotSystemInfos.AsNoTracking()
      .Where(r => r.RobotId == robotId)
      .Select(r => new RobotSystemInfoBusinessModel {
        Id = r.Id,
        Cpu = r.Cpu,
        IsLittleEndian = r.IsLittleEndian,
        Motherboard = r.Motherboard,
        MotherboardSerialNumber = r.MotherboardSerialNumber,
        RamMiB = r.RamMiB,
        Gpu = r.Gpu,
        Os = r.Os,
        Is32Bit = r.Is32Bit,
        McuSerialNumber = r.McuSerialNumber,
      })
      .FirstOrDefaultAsync();
  }

  public async Task<bool> CreateRobotSystemInfoAsync(Guid robotId, RobotSystemInfoCreateBusinessModel robotSystemInfoCreateBusinessModel)
  {
    var robotSystemInfo = new RobotSystemInfo {
      Cpu = robotSystemInfoCreateBusinessModel.Cpu,
      IsLittleEndian = robotSystemInfoCreateBusinessModel.IsLittleEndian,
      Motherboard = robotSystemInfoCreateBusinessModel.Motherboard,
      MotherboardSerialNumber = robotSystemInfoCreateBusinessModel.MotherboardSerialNumber,
      RamMiB = robotSystemInfoCreateBusinessModel.RamMiB,
      Gpu = robotSystemInfoCreateBusinessModel.Gpu,
      Os = robotSystemInfoCreateBusinessModel.Os,
      Is32Bit = robotSystemInfoCreateBusinessModel.Is32Bit,
      McuSerialNumber = robotSystemInfoCreateBusinessModel.McuSerialNumber,
      RobotId = robotId
    };
    _context.RobotSystemInfos.Add(robotSystemInfo);
    return await _context.SaveChangesAsync() >= 1;
  }

  public async Task<bool> UpdateRobotSystemInfoAsync(Guid robotId, RobotSystemInfoUpdateBusinessModel robotSystemInfoUpdateBusinessModel)
  {
    return await _context.RobotSystemInfos
      .Where(r => r.RobotId == robotId)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(r => r.Cpu, robotSystemInfoUpdateBusinessModel.Cpu)
        .SetProperty(r => r.IsLittleEndian, robotSystemInfoUpdateBusinessModel.IsLittleEndian)
        .SetProperty(r => r.Motherboard, robotSystemInfoUpdateBusinessModel.Motherboard)
        .SetProperty(r => r.MotherboardSerialNumber, robotSystemInfoUpdateBusinessModel.MotherboardSerialNumber)
        .SetProperty(r => r.RamMiB, robotSystemInfoUpdateBusinessModel.RamMiB)
        .SetProperty(r => r.Gpu, robotSystemInfoUpdateBusinessModel.Gpu)
        .SetProperty(r => r.Os, robotSystemInfoUpdateBusinessModel.Os)
        .SetProperty(r => r.Is32Bit, robotSystemInfoUpdateBusinessModel.Is32Bit)
        .SetProperty(r => r.McuSerialNumber, robotSystemInfoUpdateBusinessModel.McuSerialNumber)
      ) == 1;
  }

  public async Task UpsertRobotChassisInfoAsync(Guid robotId, RobotChassisInfoBusinessModel robotChassisInfo)
  {
    var robotChassisInfoEntity = await _context.RobotChassisInfos.Where(r => r.RobotId == robotId).FirstOrDefaultAsync();
    if (robotChassisInfoEntity == null)
    {
      _context.RobotChassisInfos.Add(new RobotChassisInfo {
        RobotId = robotId,
        RobotTypeId = robotChassisInfo.RobotTypeId,
        ChassisLengthX = robotChassisInfo.ChassisLengthX,
        ChassisLengthY = robotChassisInfo.ChassisLengthY,
        ChassisWheelCount = robotChassisInfo.ChassisWheelCount,
        ChassisWheelRadius = robotChassisInfo.ChassisWheelRadius,
        BatteryCount = robotChassisInfo.BatteryCount,
        BatteryMaxVoltage = robotChassisInfo.BatteryMaxVoltage,
        BatteryMinVoltage = robotChassisInfo.BatteryMinVoltage,
      });
    }
    else
    {
      robotChassisInfoEntity.RobotTypeId = robotChassisInfo.RobotTypeId;
      robotChassisInfoEntity.ChassisLengthX = robotChassisInfo.ChassisLengthX;      
      robotChassisInfoEntity.ChassisLengthY = robotChassisInfo.ChassisLengthY;
      robotChassisInfoEntity.ChassisWheelCount = robotChassisInfo.ChassisWheelCount;
      robotChassisInfoEntity.ChassisWheelRadius = robotChassisInfo.ChassisWheelRadius;
      robotChassisInfoEntity.BatteryCount = robotChassisInfo.BatteryCount;
      robotChassisInfoEntity.BatteryMaxVoltage = robotChassisInfo.BatteryMaxVoltage;
      robotChassisInfoEntity.BatteryMinVoltage = robotChassisInfo.BatteryMinVoltage;
    }
    await _context.SaveChangesAsync();
  }

  public async Task<IEnumerable<RobotSearchBusinessModel>> SearchRobotsAsync(int realmId, string? name, Guid? robotId)
  {
    var n = name ?? string.Empty;
    return await _context.Robots.AsNoTracking()
      .Where(r => r.RealmId == realmId)
      .Where(t => t.Name.ToLower().Contains(n.ToLower()))
      .Where(r => robotId == null || r.Id == robotId)
      .Take(10)
      .Select(m => new RobotSearchBusinessModel {
        Id = m.Id,
        Name = m.Name,
      })
      .ToListAsync();
  }

  public async Task<int?> GetRobotRealmIdAsync(Guid robotId)
  {
    if (_memoryCache.TryGetValue<int>($"RobotService_GetRobotRealmIdAsync_{robotId}", out var RealmId))
    {
      return RealmId;
    }

    var robot = await _context.Robots.AsNoTracking()
      .Where(m => m.Id == robotId)
      .Select(m => new { m.Id, m.RealmId })
      .FirstOrDefaultAsync();
    if (robot == null)
      return null;

    _memoryCache.Set($"RobotService_GetRobotRealmIdAsync_{robotId}", robot!.RealmId, TimeSpan.FromDays(1));
    return robot.RealmId;
  }

  public async Task<bool> GetRobotIsRealtimeExchange(Guid robotId)
  {
    return await _context.Robots.AsNoTracking()
      .Where(r => r.Id == robotId)
      .Select(r => r.IsRealtimeExchange)
      .FirstOrDefaultAsync();
  }
}