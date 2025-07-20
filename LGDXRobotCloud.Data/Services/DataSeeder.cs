using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Services;

public record WaypointData(string Name, double X, double Y, double Rotation);

public class DataSeeder(LgdxContext context)
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  private readonly List<WaypointData> waypoints = [
    new WaypointData("Waypoint 1", 1.5, 0.5, 0),
    new WaypointData("Waypoint 2", 1.5, 2.5, 0),
    new WaypointData("Waypoint 3", 1.5, 4.5, 0),
    new WaypointData("Waypoint 4", 1.5, 6.5, 0),
    new WaypointData("Waypoint 5", 1.5, 8.5, 0),
    new WaypointData("Waypoint 6", 6.5, 8.5, 3.14159265),
    new WaypointData("Waypoint 7", 6.5, 6.5, 3.14159265),
    new WaypointData("Waypoint 8", 6.5, 4.5, 3.14159265),
    new WaypointData("Waypoint 9", 6.5, 2.5, 3.14159265),
    new WaypointData("Waypoint 10",6.5, 0.5, 0),
  ];

  public async Task Seed()
  {
    // Realm
    var realm = await _context.Realms.FirstOrDefaultAsync();
    realm!.Resolution = 0.05;
    realm.OriginX = -0.951;
    realm.OriginY = -1.11;

    // Waypoint
    foreach (var waypoint in waypoints)
    {
      var wp = new Waypoint
      {
        Name = waypoint.Name,
        X = waypoint.X,
        Y = waypoint.Y,
        Rotation = waypoint.Rotation,
        RealmId = realm.Id
      };
      await _context.Waypoints.AddAsync(wp);
    }

    // Flow
    var flow = new Flow
    {
      Name = "Flow 1",
      FlowDetails = [new FlowDetail
      {
        ProgressId = (int)ProgressState.Waiting,
      }]
    };
    await _context.Flows.AddAsync(flow);

    // Robots
    var robots = new List<Robot>
    {
      new() {
        Id = Guid.Parse("0196dae7-7f07-7b08-94ba-713ba80d6497"),
        Name = "Robot1",
        RealmId = realm.Id,
        IsProtectingHardwareSerialNumber = false,
        IsRealtimeExchange = false,
      },
      new (){
        Id = Guid.Parse("0196e06c-6afb-7130-8e1d-1cfbe3272142"),
        Name = "Robot2",
        RealmId = realm.Id,
        IsProtectingHardwareSerialNumber = false,
        IsRealtimeExchange = false,
      }
    };

    var robotChassisInfo = new List<RobotChassisInfo> 
    {
      new (){
        RobotTypeId = (int)LgdxRobotType.LGDXRobot2Classic,
        BatteryCount = 2,
        BatteryMaxVoltage = 12.0,
        BatteryMinVoltage = 10.0,
        ChassisLengthX = 1.0,
        ChassisLengthY = 1.0,
        ChassisWheelCount = 4,
        ChassisWheelRadius = 0.1,
      },
      new (){
        RobotTypeId = (int)LgdxRobotType.LGDXRobot2Classic,
        BatteryCount = 2,
        BatteryMaxVoltage = 12.0,
        BatteryMinVoltage = 10.0,
        ChassisLengthX = 1.0,
        ChassisLengthY = 1.0,
        ChassisWheelCount = 4,
        ChassisWheelRadius = 0.1,
      }
    };
    robots[0].RobotChassisInfo = robotChassisInfo[0];
    robots[1].RobotChassisInfo = robotChassisInfo[1];

    var robotCertificate = new List<RobotCertificate> {
      new() {
        Thumbprint = "4B323EAA8D853F00F03768E5F5208DE397EA6775",
        ThumbprintBackup = string.Empty,
        NotBefore = DateTime.UtcNow,
        NotAfter = DateTime.UtcNow.AddYears(100),
      },
      new() {
        Thumbprint = "4F750BA308C8C220436C147E4D0D08B90A352893",
        ThumbprintBackup = string.Empty,
        NotBefore = DateTime.UtcNow,
        NotAfter = DateTime.UtcNow.AddYears(100),
      }
    };
    robots[0].RobotCertificate = robotCertificate[0];
    robots[1].RobotCertificate = robotCertificate[1];

    await _context.Robots.AddRangeAsync(robots);

    await _context.SaveChangesAsync();
  }
}