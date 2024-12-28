using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Realms;
public sealed partial class RealmDetail : ComponentBase
{
  [Inject]
  public required IRealmService RealmService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private RealmDetailViewModel RealmDetailViewModel { get; set; } = null!;
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
      throw new Exception("Error on uploading image.", ex);
    }

    ApiResponse<bool> response;
    if (Id != null)
      // Update
      response = await RealmService.UpdateRealmAsync((int)Id, Mapper.Map<RealmUpdateDto>(RealmDetailViewModel));
    else
      // Create
      response = await RealmService.AddRealmAsync(Mapper.Map<RealmCreateDto>(RealmDetailViewModel));

    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Maps.Index);
    else 
      RealmDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await RealmService.DeleteRealmAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Maps.Index);
      else
        RealmDetailViewModel.Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await RealmService.GetRealmAsync((int)_id);
        var realm = response.Data;
        if (realm != null) {
          RealmDetailViewModel = Mapper.Map<RealmDetailViewModel>(realm);
          _editContext = new EditContext(RealmDetailViewModel);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        RealmDetailViewModel = new RealmDetailViewModel();
        _editContext = new EditContext(RealmDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}