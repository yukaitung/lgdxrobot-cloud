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
        ProgressId = (int)ProgressState.Moving,
        AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      }]
    };
    await _context.Flows.AddAsync(flow);

    // Robots
    var robots = new List<Robot>
    {
      new() {
        Id = Guid.Parse("0198276e-97f7-7dd1-982e-56ea02c6d921"),
        Name = "Robot1",
        RealmId = realm.Id,
        IsProtectingHardwareSerialNumber = false,
      },
      new (){
        Id = Guid.Parse("0198276e-e868-733b-b9e7-d67c0ab84afd"),
        Name = "Robot2",
        RealmId = realm.Id,
        IsProtectingHardwareSerialNumber = false,
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
        Thumbprint = "DD53D5856E815B10B9B04B0D85B3BA5A622C184A",
        ThumbprintBackup = string.Empty,
        NotBefore = DateTime.UtcNow,
        NotAfter = DateTime.UtcNow.AddYears(100),
      },
      new() {
        Thumbprint = "08970F71CED907B1064FA3AD43F82992900FA905",
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