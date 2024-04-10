using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Nodes
  {
    [Inject]
    public required INodeService NodeService { get; set; }

    public List<Node>? NodesList { get; set; } = default!;

    public PaginationMetadata? PaginationMetadata { get; set; }

    protected override async Task OnInitializedAsync()
    {
      var data = await NodeService.GetNodesAsync();
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }
  }
}