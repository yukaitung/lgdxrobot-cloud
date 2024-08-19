using LGDXRobot2Cloud.Data.Models.DTOs.Base;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class NodesCollectionCreateDto : NodesCollectionBaseDto
{
  public IEnumerable<NodesCollectionDetailCreateDto> Nodes { get; set; } = [];
}
