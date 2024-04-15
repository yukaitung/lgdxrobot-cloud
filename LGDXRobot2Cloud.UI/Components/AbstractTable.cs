using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components
{
  public abstract class AbstractTable : ComponentBase
  {
    protected abstract Task HandlePageSizeChange(int number);
    protected abstract Task HandleSearch();
    protected abstract Task HandleClearSearch();
    protected abstract Task HandleItemSelect(int id);
    protected abstract Task HandlePageChange(int pageNum);
    public abstract Task Refresh(bool deleteOpt);
  }
}