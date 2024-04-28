namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class RobotSystemInfoBlazor
  {
    public int Id { get; set; }

    public string Cpu { get; set; } = null!;

    public bool IsLittleEndian { get; set; }

    public int RamMiB { get; set; }

    public string? Gpu { get; set; }

    public string Os { get; set; } = null!;

    public bool Is32Bit { get; set; }
  }
}