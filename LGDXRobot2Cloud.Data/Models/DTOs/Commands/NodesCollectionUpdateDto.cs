using LGDXRobot2Cloud.Data.Models.DTOs.Base;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class NodesCollectionUpdateDto : NodesCollectionBaseDto
{
  public IEnumerable<NodesCollectionDetailUpdateDto> Nodes { get; set; } = [];
}
