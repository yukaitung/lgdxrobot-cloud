using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.Redis;

public record MapData
{
  public float Resolution { get; set; }
  public uint Width { get; set; }
  public uint Height { get; set; }
  public RobotDof Origin { get; set; } = new();
  public List<short> Data { get; set; } = [];
}

public record SlamData
{
  public Guid RobotId { get; set; }
  public int RealmId { get; set; }
  public SlamStatus SlamStatus { get; set; }
  public MapData? MapData { get; set; }
}