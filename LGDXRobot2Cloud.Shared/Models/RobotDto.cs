namespace LGDXRobot2Cloud.Shared.Models
{
  public class RobotDto
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int RobotStatusId { get; set; }
    public bool IsRealtimeExchange { get; set; }
    public bool IsProtectingHardwareSerialNumber { get; set; }
    public string CertificateThumbprint { get; set; } = null!;
    public string? CertificateThumbprintBackup { get; set; } = null!;
    public DateTime CertificateNotBefore { get; set; }
    public DateTime CertificateNotAfter { get; set; }
    public NodesCollectionListDto? DefaultNodesCollection { get; set; }
    public RobotSystemInfoDto? RobotSystemInfo { get; set; }
    public RobotChassisInfoDto? RobotChassisInfoDto { get; set; }
    public ICollection<AutoTaskListDto> AssignedTasks { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}