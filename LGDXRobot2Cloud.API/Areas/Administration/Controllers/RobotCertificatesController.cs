using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class RobotCertificatesController(
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IRobotCertificateRepository robotCertificateRepository,
    IRobotRepository robotRepository
  ) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly IRobotCertificateRepository _robotCertificateRepository = robotCertificateRepository;
  private readonly IRobotRepository _robotRepository = robotRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<RobotCertificateListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RobotCertificateListDto>>> GetCertificates(int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (certificates, PaginationHelper) = await _robotCertificateRepository.GetRobotCertificatesAsync(pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<RobotCertificateListDto>>(certificates));
  }

  [HttpGet("Root")]
  [ProducesResponseType(typeof(IEnumerable<RootCertificateDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public ActionResult<RootCertificateDto> GetRootCertificate()
  {
    var rootCertificate = _robotCertificateRepository.GetRootCertificate();
    if (rootCertificate == null)
      return NotFound();
    return Ok(new RootCertificateDto { PublicKey = rootCertificate });
  }

  [HttpGet("{id}", Name = "GetCertificate")]
  [ProducesResponseType(typeof(RobotCertificateDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RobotCertificateDto>> GetCertificate(Guid id)
  {
    var certificate = await _robotCertificateRepository.GetRobotCertificateAsync(id);
    if (certificate == null)
      return NotFound();
    var robotEntity = await _robotRepository.GetRobotSimpleAsync(certificate.RobotId);
    if (robotEntity == null)
      return NotFound();
    var response = _mapper.Map<RobotCertificateDto>(certificate);
    response.Robot = new RobotSearchDto {
      Id = robotEntity.Id,
      Name = robotEntity.Name
    };
    return Ok(response);
  }

  [HttpPost("{id}/Renew")]
  [ProducesResponseType(typeof(RobotCertificateIssueDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RobotCertificateIssueDto>> RenewCertificate(Guid id, RobotCertificateRenewRequestDto robotCertificateRenewRequestDto)
  {
    var robotCertificateEntity = await _robotCertificateRepository.GetRobotCertificateAsync(id);
    if (robotCertificateEntity == null)
      return NotFound();

    CertificateDetail certificates = _robotCertificateRepository.GenerateRobotCertificate(robotCertificateEntity.RobotId);
    if (robotCertificateRenewRequestDto.RevokeOldCertificate)
      robotCertificateEntity.ThumbprintBackup = null;
    else
      robotCertificateEntity.ThumbprintBackup = robotCertificateEntity.Thumbprint;

    robotCertificateEntity.Thumbprint = certificates.RobotCertificateThumbprint;
    robotCertificateEntity.NotBefore = certificates.RobotCertificateNotBefore;
    robotCertificateEntity.NotAfter = certificates.RobotCertificateNotAfter;
    await _robotCertificateRepository.SaveChangesAsync();

    var robotEntity = await _robotRepository.GetRobotSimpleAsync(robotCertificateEntity.RobotId);
    if (robotEntity == null)
      return NotFound();

    return Ok(new RobotCertificateIssueDto {
      Robot = new RobotSearchDto {
        Id = robotEntity.Id,
        Name = robotEntity.Name
      },
      RootCertificate = certificates.RootCertificate,
      RobotCertificatePrivateKey = certificates.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = certificates.RobotCertificatePublicKey
    });
  }
}