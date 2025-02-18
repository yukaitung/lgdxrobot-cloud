using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class RobotCertificatesController(
    IRobotCertificateService robotCertificateService,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration
  ) : ControllerBase
{
  private readonly IRobotCertificateService _robotCertificateService = robotCertificateService ?? throw new ArgumentNullException(nameof(robotCertificateService));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<RobotCertificateListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RobotCertificateListDto>>> GetCertificates(int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (certificates, PaginationHelper) = await _robotCertificateService.GetRobotCertificatesAsync(pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(certificates.ToDto());
  }

  [HttpGet("Root")]
  [ProducesResponseType(typeof(RootCertificateDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public ActionResult<RootCertificateDto> GetRootCertificate()
  {
    var rootCertificate = _robotCertificateService.GetRootCertificate();
    if (rootCertificate == null)
      return NotFound();
    return Ok(rootCertificate.ToDto());
  }

  [HttpGet("{id}", Name = "GetCertificate")]
  [ProducesResponseType(typeof(RobotCertificateDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RobotCertificateDto>> GetCertificate(Guid id)
  {
    var certificate = await _robotCertificateService.GetRobotCertificateAsync(id);
    return Ok(certificate.ToDto());
  }

  [HttpPost("{id}/Renew")]
  [ProducesResponseType(typeof(RobotCertificateIssueDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RobotCertificateIssueDto>> RenewCertificate(Guid id, RobotCertificateRenewRequestDto robotCertificateRenewRequestDto)
  {
    var robotCertificate = await _robotCertificateService.RenewRobotCertificateAsync(new() {
      CertificateId = id,
      RevokeOldCertificate = robotCertificateRenewRequestDto.RevokeOldCertificate
    });
    return Ok(robotCertificate.ToDto());
  }
}