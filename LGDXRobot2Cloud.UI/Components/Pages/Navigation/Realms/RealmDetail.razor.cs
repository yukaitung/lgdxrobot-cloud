using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using LGDXRobot2Cloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Realms;
public sealed partial class RealmDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private int CurrentRealmId { get; set; } = 0;
  private RealmDetailViewModel RealmDetailViewModel { get; set; } = new();
  private DeleteEntryModalViewModel DeleteEntryModalViewModel { get; set; } = new();
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
        CachedRealmService.ClearCache((int)Id);
      }
      else
      {
        // Create
        await LgdxApiClient.Navigation.Realms.PostAsync(RealmDetailViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index);
    }
    catch (ApiException ex)
    {
      RealmDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTestDelete()
  {
    DeleteEntryModalViewModel.Errors = null;
    try
    {
      await LgdxApiClient.Navigation.Realms[(int)Id!].TestDelete.PostAsync();
      DeleteEntryModalViewModel.IsReady = true;
    }
    catch (ApiException ex)
    {
      DeleteEntryModalViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Navigation.Realms[(int)Id!].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index);
    }
    catch (ApiException ex)
    {
      RealmDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  protected override void OnInitialized()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    CurrentRealmId = settings.CurrentRealmId;
    base.OnInitializedAsync();
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