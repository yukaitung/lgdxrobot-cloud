using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class NodesCollectionBaseDto
  {
    [Required]
    public string Name { get; set; } = null!;
  }
}