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
public class MapEditController(
    IMapEditService mapEditService
  ) : ControllerBase
{
  private readonly IMapEditService _mapEditService = mapEditService ?? throw new ArgumentNullException(nameof(mapEditService));

  [HttpGet("{realmId}")]
  [ProducesResponseType(typeof(MapEditDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<MapEditDto>> GetMapEdit(int realmId)
  {
    var mapEdit = await _mapEditService.GetMapEditAsync(realmId);
    return Ok(mapEdit.ToDto());
  }

  [HttpPut("{realmId}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateMapEdit(int realmId, MapEditUpdateDto mapEditUpdateDto)
  {
    if (!await _mapEditService.UpdateMapEditlAsync(realmId, mapEditUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }
}