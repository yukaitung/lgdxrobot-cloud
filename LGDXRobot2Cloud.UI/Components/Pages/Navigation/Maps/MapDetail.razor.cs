using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Maps;
public sealed partial class MapDetail
{
  [Inject]
  public required IMapsService MapsService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private Map Map { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  private void LoadImage(InputFileChangeEventArgs e)
  {
    if (e.File != null)
    {
      Map.SelectedImage = e.File;
    }
  }

  public async Task HandleValidSubmit()
  {
    bool success;

    try
    {
      var file = Map.SelectedImage;
      if (file != null)
      {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        await using FileStream fs = new(path, FileMode.Create);
        await file.OpenReadStream(8_388_608).CopyToAsync(fs);
        fs.Close();
        var bytes = File.ReadAllBytes(path);
        Map.Image = Convert.ToBase64String(bytes);
        File.Delete(path);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }

    if (Id != null)
      // Update
      success = await MapsService.UpdateMapAsync((int)Id, Mapper.Map<MapUpdateDto>(Map));
    else
      // Create
      success = await MapsService.AddMapAsync(Mapper.Map<MapCreateDto>(Map));

    if (success)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Maps.Index);
    else 
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await MapsService.DeleteMapAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Maps.Index);
      else
        IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var map = await MapsService.GetMapAsync((int)_id);
        if (map != null) {
          Map = map;
          _editContext = new EditContext(Map);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Map = new Map();
        _editContext = new EditContext(Map);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}