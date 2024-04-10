using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionUpdateDto
  {
    [Required]
    public required string Name { get; set; }
    public IEnumerable<NodesCollectionDetailUpdateDto> Nodes { get; set; } = new List<NodesCollectionDetailUpdateDto>();
  }
}