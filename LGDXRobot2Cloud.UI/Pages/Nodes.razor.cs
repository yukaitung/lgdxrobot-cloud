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
    
    // Node Detail
    private int _nodeId { get; set; }

    // Table
    private int CurrentPage { get; set; } = 1;
    private int DataEntriesNumber { get; set; } = 10;
    private string DataSearch { get; set; } = string.Empty;
    private string LastDataSearch { get; set; } = string.Empty;

    // Table
    private async Task ChangePage(int pageNum)
    {
      if (pageNum == CurrentPage)
        return;
      CurrentPage = pageNum;
      if (pageNum > PaginationMetadata?.PageCount || pageNum < 1)
        return;
      var data = await NodeService.GetNodesAsync(DataSearch, pageNum, DataEntriesNumber);
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    private async Task ChangeShowingEntries(int number)
    {
      DataEntriesNumber = number;
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
      if (LastDataSearch == DataSearch)
        return;
      var data = await NodeService.GetNodesAsync(DataSearch, 1, DataEntriesNumber);
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
      LastDataSearch = DataSearch;
    }

    private async Task ClearSearch()
    {
      if (DataSearch == string.Empty)
        return;
      DataSearch = string.Empty;
      await SearchEntries();
      LastDataSearch = string.Empty;
    }

    private void EditEntries(MouseEventArgs e, int id)
    {
      _nodeId = id;
    }

    protected override async Task OnInitializedAsync()
    {
      var data = await NodeService.GetNodesAsync();
      NodesList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }
  }
}