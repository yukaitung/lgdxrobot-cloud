using System.Text.Json;
using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LGDXRobotCloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificationScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]

public sealed class ActivityLogController(
    IActivityLogService activityLogService,
    IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration
  ) : ControllerBase
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value ?? throw new ArgumentNullException(nameof(lgdxRobotCloudConfiguration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<ActivityLogListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ActivityLogListDto>>> GetActivityLogs(string? entityName, string? entityId, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
    var (activityLogs, PaginationHelper) = await _activityLogService.GetActivityLogsAsync(entityName, entityId, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(activityLogs.ToDto());
  }

  [HttpGet("{id}", Name = "GetActivityLog")]
  [ProducesResponseType(typeof(ActivityLogDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ActivityLogDto>> GetActivityLog(int id)
  {
    var activityLog = await _activityLogService.GetActivityLogAsync(id);
    return Ok(activityLog.ToDto());
  }
}