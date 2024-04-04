using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class NodesCollectionCreateDto
  {
    [Required]
    public required string Name { get; set; }
    public IEnumerable<NodesCollectionDetailCreateDto> Nodes { get; set; } = new List<NodesCollectionDetailCreateDto>();
  }
}