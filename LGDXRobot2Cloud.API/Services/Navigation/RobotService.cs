using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.API.Services.Administration;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Administration;
using LGDXRobot2Cloud.Data.Models.Business.Automation;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Navigation;

public interface IRobotService
{
  Task<(IEnumerable<RobotListBusinessModel>, PaginationHelper)> GetRobotsAsync(int? realmId, string? name, int pageNumber, int pageSize);
  Task<RobotBusinessModel> GetRobotAsync(Guid robotId);
  Task<RobotCreateResponseBusinessModel> CreateRobotAsync(RobotCreateBusinessModel robotCreateBusinessModel);
  Task<bool> UpdateRobotAsync(Guid id, RobotUpdateBusinessModel robotUpdateDtoBusinessModel);
  Task<bool> UpdateRobotChassisInfoAsync(Guid id, RobotChassisInfoUpdateBusinessModel robotChassisInfoUpdateBusinessModel);
  Task<bool> DeleteRobotAsync(Guid id);

  Task<IEnumerable<RobotSearchBusinessModel>> SearchRobotsAsync(int realmId, string? name);
}

public class RobotService(
    IRobotCertificateService robotCertificateService,
    LgdxContext context
  ) : IRobotService
{
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
      query = query.Where(r => r.Name.Contains(name));
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
      .Include(r => r.Realm)
      .Include(r => r.RobotCertificate)
      .Include(r => r.RobotSystemInfo)
      .Include(r => r.RobotChassisInfo)
      .Include(r => r.AssignedTasks)
        .ThenInclude(t => t.Flow)
      .Include(r => r.AssignedTasks)
        .ThenInclude(t => t.CurrentProgress)
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
        AssignedTasks = r.AssignedTasks.Select(t => new AutoTaskListBusinessModel {
          Id = t.Id,
          Name = t.Name,
          Priority = t.Priority,
          FlowId = t.FlowId,
          FlowName = t.Flow.Name,
          RealmId = r.RealmId,
          RealmName = r.Realm.Name,
          AssignedRobotId = r.Id,
          AssignedRobotName = r.Name,
          CurrentProgressId = t.CurrentProgressId,
          CurrentProgressName = t.CurrentProgress.Name,
        }).ToList(),
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

    var id = Guid.NewGuid();
    var robotCertificate = _robotCertificateService.IssueRobotCertificate(id);
    var robot = new Robot {
      Id = id,
      Name = robotCreateBusinessModel.Name,
      RealmId = robotCreateBusinessModel.RealmId,
      IsRealtimeExchange = robotCreateBusinessModel.IsRealtimeExchange,
      IsProtectingHardwareSerialNumber = robotCreateBusinessModel.IsProtectingHardwareSerialNumber,
      RobotCertificate = new RobotCertificate {
        Thumbprint = robotCertificate.RobotCertificateThumbprint,
        NotBefore = robotCertificate.RobotCertificateNotBefore,
        NotAfter = robotCertificate.RobotCertificateNotAfter
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
      },
    };
    await _context.Robots.AddAsync(robot);
    await _context.SaveChangesAsync();

    return new RobotCreateResponseBusinessModel {
      RobotId = robot.Id,
      RobotName = robot.Name,
      RootCertificate = robotCertificate.RootCertificate,
      RobotCertificatePrivateKey = robotCertificate.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = robotCertificate.RobotCertificatePublicKey
    };
  }

  public async Task<bool> UpdateRobotAsync(Guid id, RobotUpdateBusinessModel robotUpdateDtoBusinessModel)
  {
    var realm = await _context.Realms.AsNoTracking().Where(r => r.Id == robotUpdateDtoBusinessModel.RealmId).AnyAsync();
    if (realm == false)
    {
      throw new LgdxValidation400Expection(nameof(robotUpdateDtoBusinessModel.RealmId), "Realm does not exist.");
    }

    return await _context.Robots
      .Where(r => r.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(r => r.Name, robotUpdateDtoBusinessModel.Name)
        .SetProperty(r => r.RealmId, robotUpdateDtoBusinessModel.RealmId)
        .SetProperty(r => r.IsRealtimeExchange, robotUpdateDtoBusinessModel.IsRealtimeExchange)
        .SetProperty(r => r.IsProtectingHardwareSerialNumber, robotUpdateDtoBusinessModel.IsProtectingHardwareSerialNumber)
      ) == 1;
  }

  public async Task<bool> UpdateRobotChassisInfoAsync(Guid id, RobotChassisInfoUpdateBusinessModel robotChassisInfoUpdateBusinessModel)
  {
    return await _context.RobotChassisInfos
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
  }

  public async Task<bool> DeleteRobotAsync(Guid id)
  {
    return await _context.Robots.Where(r => r.Id == id)
      .ExecuteDeleteAsync() >= 1;
  }

  public async Task<IEnumerable<RobotSearchBusinessModel>> SearchRobotsAsync(int realmId, string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return await _context.Robots.AsNoTracking()
        .Where(w => w.RealmId == realmId)
        .Take(10)
        .Select(m => new RobotSearchBusinessModel {
          Id = m.Id,
          Name = m.Name,
        })
        .ToListAsync();
    }
    else
    {
      return await _context.Robots.AsNoTracking()
        .Where(w => w.Name.Contains(name))
        .Where(w => w.RealmId == realmId)
        .Take(10)
        .Select(m => new RobotSearchBusinessModel {
          Id = m.Id,
          Name = m.Name,
        })
        .ToListAsync();
    }
  }
}