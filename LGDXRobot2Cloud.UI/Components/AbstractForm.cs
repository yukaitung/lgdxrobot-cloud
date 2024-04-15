using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components
{
  public abstract class AbstractForm : ComponentBase
  {
    protected abstract Task HandleValidSubmit();
    protected abstract void HandleInvalidSubmit();
    protected abstract void HandleDelete();
  }
}