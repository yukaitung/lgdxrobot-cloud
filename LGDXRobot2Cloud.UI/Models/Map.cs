using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Models;

public class Map
{
  public int Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Description { get; set; }

  public string Image { get; set; } = null!;

  public IBrowserFile SelectedImage { get; set; } = null!;

  [Required]
  public double Resolution { get; set; }

  [Required]
  public double OriginX { get; set; }

  [Required]
  public double OriginY { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}