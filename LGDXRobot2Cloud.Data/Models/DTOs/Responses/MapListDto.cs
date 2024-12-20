using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public record MapListDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public double Resolution { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}