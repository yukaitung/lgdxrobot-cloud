using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Helpers
{
  public class CustomFieldClassProvider : FieldCssClassProvider
  {
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
      var isValid = editContext.IsValid(fieldIdentifier);
      return isValid ? "" : "is-invalid";
    }
  }
}
