using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Setting.Controllers;

[ApiController]
[Area("Setting")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class CertificatesController(
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
  IRobotRepository robotRepository,
  IRobotCertificateRepository robotCertificateRepository) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly IRobotCertificateRepository _robotCertificateRepository = robotCertificateRepository;
  private readonly IRobotRepository _robotRepository = robotRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<RobotCertificateListDto>>> GetCertificates(int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (certificates, PaginationHelper) = await _robotCertificateRepository.GetRobotCertificatesAsync(pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<RobotCertificateListDto>>(certificates));
  }

  [HttpGet("root")]
  public ActionResult<RootCertificateDto> GetRootCertificate()
  {
    var rootCertificate = _robotCertificateRepository.GetRootCertificate();
    return Ok(new RootCertificateDto { PublicKey = rootCertificate });
  }

  [HttpGet("{id}", Name = "GetCertificate")]
  public async Task<ActionResult<RobotCertificateDto>> GetCertificate(Guid id)
  {
    var certificate = await _robotCertificateRepository.GetRobotCertificateAsync(id);
    if (certificate == null)
      return NotFound();

    var robotEntity = await _robotRepository.GetRobotSimpleAsync(certificate.RobotId);
    var response = _mapper.Map<RobotCertificateDto>(certificate);
    response.RobotId = robotEntity?.Id;
    response.RobotName = robotEntity?.Name;
    return Ok(response);
  }

  [HttpPost("{id}/renew")]
  public async Task<ActionResult<RobotCertificateIssueDto>> RenewCertificate(Guid id, RobotRenewCertificateRenewDto dto)
  {
    var robotCertificateEntity = await _robotCertificateRepository.GetRobotCertificateAsync(id);
    if (robotCertificateEntity == null)
      return NotFound();

    CertificateDetail certificates = _robotCertificateRepository.GenerateRobotCertificate(robotCertificateEntity.RobotId);
    if (dto.RevokeOldCertificate)
      robotCertificateEntity.ThumbprintBackup = null;
    else
      robotCertificateEntity.ThumbprintBackup = robotCertificateEntity.Thumbprint;
    robotCertificateEntity.Thumbprint = certificates.RobotCertificateThumbprint;
    robotCertificateEntity.NotBefore = certificates.RobotCertificateNotBefore;
    robotCertificateEntity.NotAfter = certificates.RobotCertificateNotAfter;
    await _robotCertificateRepository.SaveChangesAsync();

    var robotEntity = await _robotRepository.GetRobotSimpleAsync(robotCertificateEntity.RobotId);

    return Ok(new RobotCertificateIssueDto {
      RobotId = robotEntity?.Id,
      RobotName = robotEntity?.Name,
      RootCertificate = certificates.RootCertificate,
      RobotCertificatePrivateKey = certificates.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = certificates.RobotCertificatePublicKey
    });
  }
}