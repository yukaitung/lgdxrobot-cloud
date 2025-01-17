namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RobotSystemInfoCreateBusinessModel
{  
  public required string Cpu { get; set; }

  public required bool IsLittleEndian { get; set; }

  public required string Motherboard { get; set; }

  public required string MotherboardSerialNumber { get; set; }

  public required int RamMiB { get; set; }

  public string? Gpu { get; set; }

  public required string Os { get; set; }

  public required bool Is32Bit { get; set; }

  public string? McuSerialNumber { get; set; }
}