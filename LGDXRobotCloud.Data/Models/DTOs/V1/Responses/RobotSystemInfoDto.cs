namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record RobotSystemInfoDto
{
  public required int Id { get; set; }

  public required string Cpu { get; set; } = null!;

  public required bool IsLittleEndian { get; set; }

  public required string Motherboard { get; set; } = null!;

  public required string MotherboardSerialNumber { get; set; } = null!;

  public required int RamMiB { get; set; }

  public string? Gpu { get; set; }

  public required string Os { get; set; } = null!;

  public required bool Is32Bit { get; set; }

  public string? McuSerialNumber { get; set; }
}
