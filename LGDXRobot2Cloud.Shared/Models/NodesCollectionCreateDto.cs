using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionCreateDto : NodesCollectionBaseDto
  {
    public IEnumerable<NodesCollectionDetailCreateDto> Nodes { get; set; } = [];
  }
}