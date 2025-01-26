using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Realms;
public sealed partial class RealmDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private RealmDetailViewModel RealmDetailViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  private void LoadImage(InputFileChangeEventArgs e)
  {
    if (e.File != null)
    {
      RealmDetailViewModel.SelectedImage = e.File;
    }
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      var file = RealmDetailViewModel.SelectedImage;
      if (file != null)
      {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        await using FileStream fs = new(path, FileMode.Create);
        await file.OpenReadStream(8_388_608).CopyToAsync(fs);
        fs.Close();
        var bytes = File.ReadAllBytes(path);
        RealmDetailViewModel.Image = Convert.ToBase64String(bytes);
        File.Delete(path);
      }
    }
    catch (Exception ex)
    {
      RealmDetailViewModel.Errors = [];
      RealmDetailViewModel.Errors.Add(nameof(RealmDetailViewModel.SelectedImage), ex.Message);
    }

    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Navigation.Realms[(int)Id].PutAsync(RealmDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Navigation.Realms.PostAsync(RealmDetailViewModel.ToCreateDto());
      }
    }
    catch (ApiException ex)
    {
      RealmDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index);
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Navigation.Realms[(int)Id!].DeleteAsync();
    }
    catch (ApiException ex)
    {
      RealmDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id) && _id != null)
    {
      var response = await LgdxApiClient.Navigation.Realms[(int)_id].GetAsync();
      RealmDetailViewModel.FromDto(response!);
      _editContext = new EditContext(RealmDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    else
    {
      _editContext = new EditContext(RealmDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}