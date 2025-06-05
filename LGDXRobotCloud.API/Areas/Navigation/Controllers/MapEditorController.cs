using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobotCloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificationScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class MapEditorController(
    IMapEditorService mapEditService
  ) : ControllerBase
{
  private readonly IMapEditorService _mapEditService = mapEditService ?? throw new ArgumentNullException(nameof(mapEditService));

  [HttpGet("{realmId}")]
  [ProducesResponseType(typeof(MapEditorDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<MapEditorDto>> GetMap(int realmId)
  {
    var mapEdit = await _mapEditService.GetMapAsync(realmId);
    return Ok(mapEdit.ToDto());
  }

  [HttpPost("{realmId}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateMap(int realmId, MapEditorUpdateDto mapEditUpdateDto)
  {
    if (!await _mapEditService.UpdateMapAsync(realmId, mapEditUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }
}