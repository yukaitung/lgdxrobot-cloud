using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LGDXRobot2Cloud.UI.Components.Nodes
{
  public partial class NodeTable
  {
    [Inject]
    public required INodeService NodeService { get; set; }

    [Parameter]
    public EventCallback<int> OnIdSelected { get; set; }

    private List<Node>? _nodesList { get; set; } = default!;
    private PaginationMetadata? _paginationMetadata { get; set; } = default!;

    private int _currentPage { get; set; } = 1;
    private int _pageSize { get; set; } = 10;
    private string _dataSearch { get; set; } = string.Empty;
    private string _lastDataSearch { get; set; } = string.Empty;

    private async Task HandlePageSizeChange(int number)
    {
      _pageSize = number;
      if (_pageSize > 100)
        _pageSize = 100;
      else if (_pageSize < 1)
        _pageSize = 1;
      var data = await NodeService.GetNodesAsync(_dataSearch, 1, _pageSize);
      _nodesList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
    }

    private async Task HandleSearch()
    {
      if (_lastDataSearch == _dataSearch)
        return;
      var data = await NodeService.GetNodesAsync(_dataSearch, 1, _pageSize);
      _nodesList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
      _lastDataSearch = _dataSearch;
    }

    private async Task HandleClearSearch()
    {
      if (_dataSearch == string.Empty)
        return;
      _dataSearch = string.Empty;
      await HandleSearch();
    }

    private async Task HandleItemSelect(int id)
    {
      await OnIdSelected.InvokeAsync(id);
    }

    private async Task HandlePageChange(int pageNum)
    {
      if (pageNum == _currentPage)
        return;
      _currentPage = pageNum;
      if (pageNum > _paginationMetadata?.PageCount || pageNum < 1)
        return;
      var data = await NodeService.GetNodesAsync(_dataSearch, pageNum, _pageSize);
      _nodesList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
    }

    protected override async Task OnInitializedAsync()
    {
      var data = await NodeService.GetNodesAsync();
      _nodesList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
    }
  }
}