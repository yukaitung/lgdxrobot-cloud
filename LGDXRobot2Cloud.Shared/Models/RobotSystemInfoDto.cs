namespace LGDXRobot2Cloud.Shared.Models
{
  public class RobotSystemInfoDto
  {
    public int Id { get; set; }

    public string Cpu { get; set; } = null!;

    public bool IsLittleEndian { get; set; }

    public string Motherboard { get; set; } = null!;

    public string MotherboardSerialNumber { get; set; } = null!;

    public int RamMiB { get; set; }

    public string? Gpu { get; set; }

    public string Os { get; set; } = null!;

    public bool Is32Bit { get; set; }
  }
}