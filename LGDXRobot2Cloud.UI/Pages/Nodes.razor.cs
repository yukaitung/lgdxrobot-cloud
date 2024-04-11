using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Nodes
  {
    [Inject]
    public required INodeService NodeService { get; set; }
    private List<Node>? NodesList { get; set; } = default!;
    private PaginationMetadata? PaginationMetadata { get; set; } = default!;
    private int DataEntriesNumber { get; set; } = 10;
    private string DataSearch { get; set; } = string.Empty;

    private async Task ChangePage(int pageNum)
    {
      if (pageNum > PaginationMetadata?.PageCount || pageNum < 1)
        return;
      var data = await NodeService.GetNodesAsync(DataSearch, pageNum, DataEntriesNumber);
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    private async Task ChangeShowingEntries()
    {
      if (DataEntriesNumber > 100)
        DataEntriesNumber = 100;
      else if (DataEntriesNumber < 1)
        DataEntriesNumber = 1;
      var data = await NodeService.GetNodesAsync(DataSearch, 1, DataEntriesNumber);
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    private async Task SearchEntries()
    {
      var data = await NodeService.GetNodesAsync(DataSearch, 1, DataEntriesNumber);
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    protected override async Task OnInitializedAsync()
    {
      var data = await NodeService.GetNodesAsync();
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }
  }
}