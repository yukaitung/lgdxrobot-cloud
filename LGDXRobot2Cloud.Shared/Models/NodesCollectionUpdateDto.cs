using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionUpdateDto : NodesCollectionBaseDto
  {
    public IEnumerable<NodesCollectionDetailUpdateDto> Nodes { get; set; } = [];
  }
}