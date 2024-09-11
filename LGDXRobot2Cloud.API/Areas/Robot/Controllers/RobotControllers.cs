using AutoMapper;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using Entities = LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Robot.Controllers;

[ApiController]
[Area("Robot")]
[Route("[area]")]

public class RobotController(
  IMapper mapper,
  IOnlineRobotsService OnlineRobotsService,
  IOptionsSnapshot<LgdxRobot2Configuration> options,
  IRobotRepository robotRepository) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly IRobotRepository _robotRepository = robotRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = options.Value;

  public record RobotCertificates 
  {
    required public string RootCertificate { get; set; }
    required public string RobotCertificatePrivateKey { get; set; }
    required public string RobotCertificatePublicKey { get; set; }
    required public string RobotCertificateThumbprint { get; set; }
    required public DateTime RobotCertificateNotBefore { get; set; }
    required public DateTime RobotCertificateNotAfter { get; set; }
  }

  private static RobotStatus ConvertRobotStatus(RobotClientsRobotStatus robotStatus)
  {
    return robotStatus switch
    {
      RobotClientsRobotStatus.Idle => RobotStatus.Idle,
      RobotClientsRobotStatus.Running => RobotStatus.Running,
      RobotClientsRobotStatus.Stuck => RobotStatus.Stuck,
      RobotClientsRobotStatus.Aborting => RobotStatus.Aborting,
      RobotClientsRobotStatus.Paused => RobotStatus.Paused,
      RobotClientsRobotStatus.Critical => RobotStatus.Critical,
      RobotClientsRobotStatus.Charging => RobotStatus.Charging,
      RobotClientsRobotStatus.Offline => RobotStatus.Offline,
      _ => RobotStatus.Offline,
    };
  }

  private RobotCertificates GenerateRobotCertificate(Guid robotId)
  {
    X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.OpenExistingOnly);
    X509Certificate2 rootCertificate = store.Certificates.First(c => c.SerialNumber == _lgdxRobot2Configuration.RootCertificateSN);

    var certificateNotBefore = DateTime.UtcNow;
    var certificateNotAfter = DateTimeOffset.UtcNow.AddDays(_lgdxRobot2Configuration.RobotCertificateValidDay);

    var rsa = RSA.Create();
    var certificateRequest = new CertificateRequest("CN=LGDXRobot2 Robot Certificate for " + robotId.ToString() + ",OID.0.9.2342.19200300.100.1.1=" + robotId.ToString(), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var certificate = certificateRequest.Create(rootCertificate, certificateNotBefore, certificateNotAfter, RandomNumberGenerator.GetBytes(20));

    return new RobotCertificates {
      RootCertificate = rootCertificate.ExportCertificatePem(),
      RobotCertificatePrivateKey = new string(PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey())),
      RobotCertificatePublicKey = certificate.ExportCertificatePem(),
      RobotCertificateThumbprint = certificate.Thumbprint,
      RobotCertificateNotBefore = certificateNotBefore,
      RobotCertificateNotAfter = certificateNotAfter.DateTime
    };
  }

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<RobotListDto>>> GetRobots(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (robots, PaginationHelper) = await _robotRepository.GetRobotsAsync(name, pageNumber, pageSize);
    var OnlineRobotssData = _onlineRobotsService.GetRobotsData(robots.Select(r => r.Id).ToList());
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));

    var robotsDto = _mapper.Map<IEnumerable<RobotListDto>>(robots);
    if (OnlineRobotssData != null)
    {
      for (int i = 0; i < robotsDto.Count(); i++)
      {
        if (OnlineRobotssData.TryGetValue(robotsDto.ElementAt(i).Id, out var data))
        {
          robotsDto.ElementAt(i).RobotStatus = ConvertRobotStatus(data.Data.RobotStatus);
          robotsDto.ElementAt(i).Batteries = data.Data.Batteries;
          robotsDto.ElementAt(i).IsSoftwareEmergencyStop = data.Commands.SoftwareEmergencyStop;
          robotsDto.ElementAt(i).IsPauseTaskAssigement = data.Commands.PauseTaskAssigement;
        }
      }
    }
    
    return Ok(robotsDto);
  }

  [HttpGet("{id}", Name = "GetRobot")]
  public async Task<ActionResult<RobotDto>> GetRobot(Guid id)
  {
    var robot = await _robotRepository.GetRobotAsync(id);
    if (robot == null)
      return NotFound();

    var robotsDto = _mapper.Map<RobotDto>(robot);
    var OnlineRobotssData = _onlineRobotsService.GetRobotData(robot.Id);
    if (OnlineRobotssData != null && OnlineRobotssData.TryGetValue(robot.Id, out var data))
    {
      robotsDto.RobotStatus = ConvertRobotStatus(data.Data.RobotStatus);
      robotsDto.Batteries = data.Data.Batteries;
      robotsDto.IsSoftwareEmergencyStop = data.Commands.SoftwareEmergencyStop;
      robotsDto.IsPauseTaskAssigement = data.Commands.PauseTaskAssigement;
    }

    return Ok(robotsDto);
  }

  [HttpPost(Name = "CreateRobot")]
  public async Task<ActionResult> CreateRobot(RobotCreateDto robotDto)
  {
    var robotEntity = _mapper.Map<Entities.Robot>(robotDto);
    robotEntity.Id = Guid.NewGuid();
    RobotCertificates certificates = GenerateRobotCertificate(robotEntity.Id);
    robotEntity.CertificateThumbprint = certificates.RobotCertificateThumbprint;
    robotEntity.CertificateNotBefore = certificates.RobotCertificateNotBefore;
    robotEntity.CertificateNotAfter = certificates.RobotCertificateNotAfter;
    await _robotRepository.AddRobotAsync(robotEntity);
    await _robotRepository.SaveChangesAsync();
    var response = new RobotCreateResponseDto
    {
      Id = robotEntity.Id,
      Name = robotEntity.Name,
      RootCertificate = certificates.RootCertificate,
      RobotCertificatePrivateKey = certificates.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = certificates.RobotCertificatePublicKey
    };
    return CreatedAtAction(nameof(CreateRobot), response);
  }

  [HttpPost("{id}/emergencystop")]
  public ActionResult UpdateSoftwareEmergencyStop(Guid id, EnableDto data)
  {
    if (_onlineRobotsService.UpdateSoftwareEmergencyStop(id, data.Enable))
    {
      return NoContent();
    }
    return BadRequest("Robot is offline or not found.");
  }

  [HttpPost("{id}/pausetaskassigement")]
  public ActionResult UpdatePauseTaskAssigement(Guid id, EnableDto data)
  {
    if (_onlineRobotsService.UpdatePauseTaskAssigement(id, data.Enable))
    {
      return NoContent();
    }
    return BadRequest("Robot is offline or not found.");
  }

  [HttpPost("{id}/information")]
  public async Task<ActionResult> UpdateRobot(Guid id, RobotUpdateDto robotDto)
  {
    var robotEntity = await _robotRepository.GetRobotAsync(id);
    if (robotEntity == null)
      return NotFound();
    _mapper.Map(robotDto, robotEntity);
    robotEntity.UpdatedAt = DateTime.UtcNow;
    await _robotRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpPost("{id}/certificate")]
  public async Task<ActionResult> RenewCertificate(Guid id, RobotRenewCertificateRenewDto dto)
  {
    var robotEntity = await _robotRepository.GetRobotAsync(id);
    if (robotEntity == null)
      return NotFound();
    RobotCertificates certificates = GenerateRobotCertificate(robotEntity.Id);
    if (dto.RevokeOldCertificate)
      robotEntity.CertificateThumbprintBackup = null;
    else
      robotEntity.CertificateThumbprintBackup = robotEntity.CertificateThumbprint;
    robotEntity.CertificateThumbprint = certificates.RobotCertificateThumbprint;
    robotEntity.CertificateNotBefore = certificates.RobotCertificateNotBefore;
    robotEntity.CertificateNotAfter = certificates.RobotCertificateNotAfter;
    robotEntity.UpdatedAt = DateTime.UtcNow;
    await _robotRepository.SaveChangesAsync();
    var response = new RobotCreateResponseDto
    {
      Id = robotEntity.Id,
      Name = robotEntity.Name,
      RootCertificate = certificates.RootCertificate,
      RobotCertificatePrivateKey = certificates.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = certificates.RobotCertificatePublicKey
    };
    return CreatedAtAction(nameof(CreateRobot), response);
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteRobot(Guid id)
  {
    var robot = await _robotRepository.GetRobotAsync(id);
    if (robot == null)
      return NotFound();
    _robotRepository.DeleteRobot(robot);
    await _robotRepository.SaveChangesAsync();
    return NoContent();
  }
}
