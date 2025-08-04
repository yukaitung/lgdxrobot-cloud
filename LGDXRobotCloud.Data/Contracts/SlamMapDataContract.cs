using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Contracts;

public record SlamMapData
{
  public float Resolution { get; set; }
  public uint Width { get; set; }
  public uint Height { get; set; }
  public RobotDof Origin { get; set; } = new();
  public List<short> Data { get; set; } = [];
}

public record SlamMapDataContract
{
  public Guid RobotId { get; set; }
  public int RealmId { get; set; }
  public RealtimeNavResult RealtimeNavResult { get; set; }
  public SlamMapData? MapData { get; set; }
}