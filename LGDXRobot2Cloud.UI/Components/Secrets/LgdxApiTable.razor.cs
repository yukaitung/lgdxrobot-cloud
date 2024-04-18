using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Secrets
{
  public partial class LgdxApiTable : AbstractTable
  {
    [Inject]
    public required IApiKeyService ApiKeyService { get; set; }

    [Parameter]
    public EventCallback<int> OnIdSelected { get; set; }

    private List<ApiKey>? _apiKeyList { get; set; } = default!;
    private PaginationMetadata? _paginationMetadata { get; set; } = default!;
    private int _currentPage { get; set; } = 1;
    private int _pageSize { get; set; } = 10;
    private string _dataSearch { get; set; } = string.Empty;
    private string _lastDataSearch { get; set; } = string.Empty;
    
    protected override async Task HandlePageSizeChange(int number)
    {
      _pageSize = number;
      if (_pageSize > 100)
        _pageSize = 100;
      else if (_pageSize < 1)
        _pageSize = 1;
      var data = await ApiKeyService.GetApiKeysAsync(false, _dataSearch, 1, _pageSize);
      _apiKeyList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
    }

    protected override async Task HandleSearch()
    {
      if (_lastDataSearch == _dataSearch)
        return;
      var data = await ApiKeyService.GetApiKeysAsync(false, _dataSearch, 1, _pageSize);
      _apiKeyList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
      _lastDataSearch = _dataSearch;
    }

    protected override async Task HandleClearSearch()
    {
      if (_dataSearch == string.Empty && _lastDataSearch == string.Empty)
        return;
      _dataSearch = string.Empty;
      await HandleSearch();
    }

    protected override async Task HandleItemSelect(int id)
    {
      await OnIdSelected.InvokeAsync(id);
    }

    protected override async Task HandlePageChange(int pageNum)
    {
      if (pageNum == _currentPage)
        return;
      _currentPage = pageNum;
      if (pageNum > _paginationMetadata?.PageCount || pageNum < 1)
        return;
      var data = await ApiKeyService.GetApiKeysAsync(false, _dataSearch, pageNum, _pageSize);
      _apiKeyList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
    }

    public override async Task Refresh(bool deleteOpt = false)
    {
      if (deleteOpt && _currentPage > 1 && _apiKeyList?.Count == 1)
        _currentPage--;
      var data = await ApiKeyService.GetApiKeysAsync(false, _dataSearch, _currentPage, _pageSize);
      _apiKeyList = data.Item1?.ToList();
      _paginationMetadata = data.Item2;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        await Refresh();
        StateHasChanged();
      }
    }
  }
}