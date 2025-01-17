using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RobotSystemInfoBusinessModel
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

public static class RobotSystemInfoBusinessModelExtensions
{
  public static RobotSystemInfoDto ToDto(this RobotSystemInfoBusinessModel robotSystemInfo)
  {
    return new RobotSystemInfoDto {
      Id = robotSystemInfo.Id,
      Cpu = robotSystemInfo.Cpu,
      IsLittleEndian = robotSystemInfo.IsLittleEndian,
      Motherboard = robotSystemInfo.Motherboard,
      MotherboardSerialNumber = robotSystemInfo.MotherboardSerialNumber,
      RamMiB = robotSystemInfo.RamMiB,
      Gpu = robotSystemInfo.Gpu,
      Os = robotSystemInfo.Os,
      Is32Bit = robotSystemInfo.Is32Bit,
      McuSerialNumber = robotSystemInfo.McuSerialNumber,
    };
  }

  public static RobotSystemInfoCreateBusinessModel ToCreateBusinessModel(this RobotSystemInfoBusinessModel model)
  {
    return new RobotSystemInfoCreateBusinessModel {
      Cpu = model.Cpu,
      IsLittleEndian = model.IsLittleEndian,
      Motherboard = model.Motherboard,
      MotherboardSerialNumber = model.MotherboardSerialNumber,
      RamMiB = model.RamMiB,
      Gpu = model.Gpu,
      Os = model.Os,
      Is32Bit = model.Is32Bit,
      McuSerialNumber = model.McuSerialNumber,
    };
  }

  public static RobotSystemInfoUpdateBusinessModel ToUpdateBusinessModel(this RobotSystemInfoBusinessModel model)
  {
    return new RobotSystemInfoUpdateBusinessModel {
      Cpu = model.Cpu,
      IsLittleEndian = model.IsLittleEndian,
      Motherboard = model.Motherboard,
      MotherboardSerialNumber = model.MotherboardSerialNumber,
      RamMiB = model.RamMiB,
      Gpu = model.Gpu,
      Os = model.Os,
      Is32Bit = model.Is32Bit,
      McuSerialNumber = model.McuSerialNumber,
    };
  }
}