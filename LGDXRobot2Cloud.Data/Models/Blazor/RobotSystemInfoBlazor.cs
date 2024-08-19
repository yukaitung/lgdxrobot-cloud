namespace LGDXRobot2Cloud.Data.Models.Blazor;

public class RobotSystemInfoBlazor
{
  public int Id { get; set; }

  public string Motherboard { get; set; } = null!;

  public string MotherboardSerialNumber { get; set; } = null!;

  public string Cpu { get; set; } = null!;

  public bool IsLittleEndian { get; set; }

  public int RamMiB { get; set; }

  public string? Gpu { get; set; }

  public string Os { get; set; } = null!;

  public bool Is32Bit { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
