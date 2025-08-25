using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobotCloud.UI.Helpers;

public class CustomFieldClassProvider : FieldCssClassProvider
{
  public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
  {
    var isValid = editContext.IsValid(fieldIdentifier);
    return isValid ? "" : "is-invalid";
  }
}

