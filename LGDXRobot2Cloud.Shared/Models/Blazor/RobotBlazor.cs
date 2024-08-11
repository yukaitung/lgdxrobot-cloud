using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class RobotBlazor
  {
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? Namespace { get; set; }
    public int RobotStatusId { get; set; }
    public bool IsRealtimeExchange { get; set; }
    public bool IsProtectingHardwareSerialNumber { get; set; }
    public string CertificateThumbprint { get; set; } = null!;
    public string? CertificateThumbprintBackup { get; set; } = null!;
    public DateTime CertificateNotBefore { get; set; }
    public DateTime CertificateNotAfter { get; set; }
    public NodesCollectionBlazor? DefaultNodesCollection { get; set; }
    public RobotSystemInfoBlazor? RobotSystemInfo { get; set; }
    public RobotChassisInfoBlazor? RobotChassisInfo { get; set; }
    public ICollection<AutoTaskBlazor> AssignedTasks { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}