using AutoMapper;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class RobotsController(
    IMapper mapper,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobot2Configuration> options,
    IRealmRepository realmRepository,
    IRobotCertificateRepository robotCertificateRepository,
    IRobotChassisInfoRepository robotChassisInfoRepository,
    IRobotRepository robotRepository
  ) : ControllerBase
{
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService ?? throw new ArgumentNullException(nameof(OnlineRobotsService));
  private readonly IRealmRepository _realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
  private readonly IRobotCertificateRepository _robotCertificateRepository = robotCertificateRepository ?? throw new ArgumentNullException(nameof(robotCertificateRepository));
  private readonly IRobotChassisInfoRepository _robotChassisInfoRepository = robotChassisInfoRepository ?? throw new ArgumentNullException(nameof(robotChassisInfoRepository));
  private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = options.Value ?? throw new ArgumentNullException(nameof(options));  

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

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<RobotListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RobotListDto>>> GetRobots(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (robots, PaginationHelper) = await _robotRepository.GetRobotsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<RobotListDto>>(robots));
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<RobotSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RobotSearchDto>>> SearchRobots(string name)
  {
    var waypoints = await _robotRepository.SearchRobotsAsync(name);
    return Ok(_mapper.Map<IEnumerable<RobotSearchDto>>(waypoints));
  }

  [HttpGet("{id}", Name = "GetRobot")]
  [ProducesResponseType(typeof(RobotDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RobotDto>> GetRobot(Guid id)
  {
    var robot = await _robotRepository.GetRobotAsync(id);
    if (robot == null)
    {
      return NotFound();
    }
    var robotsDto = _mapper.Map<RobotDto>(robot);
    return Ok(robotsDto);
  }

  [HttpPost(Name = "CreateRobot")]
  [ProducesResponseType(typeof(RobotCertificateIssueDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateRobot(RobotCreateDto robotCreateDto)
  {
    if (!await _realmRepository.IsRealmExistsAsync(robotCreateDto.RealmId))
    {
      ModelState.AddModelError(nameof(robotCreateDto.RealmId), "Realm does not exist.");
      return ValidationProblem();
    }
    var robotEntity = _mapper.Map<Robot>(robotCreateDto);
    robotEntity.Id = Guid.NewGuid();
    CertificateDetail certificates = _robotCertificateRepository.GenerateRobotCertificate(robotEntity.Id);
    robotEntity.RobotCertificate = new RobotCertificate {
      Thumbprint = certificates.RobotCertificateThumbprint,
      NotBefore = certificates.RobotCertificateNotBefore,
      NotAfter = certificates.RobotCertificateNotAfter
    };
    var robotChassisInfoEntity = _mapper.Map<RobotChassisInfo>(robotCreateDto.RobotChassisInfo);
    robotEntity.RobotChassisInfo = robotChassisInfoEntity;
    await _robotRepository.AddRobotAsync(robotEntity);
    await _robotRepository.SaveChangesAsync();
    var response = new RobotCertificateIssueDto
    {
      Robot = new RobotSearchDto {
        Id = robotEntity.Id,
        Name = robotEntity.Name
      },
      RootCertificate = certificates.RootCertificate,
      RobotCertificatePrivateKey = certificates.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = certificates.RobotCertificatePublicKey
    };
    return CreatedAtAction(nameof(CreateRobot), response);
  }

  [HttpPatch("{id}/EmergencyStop")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> SetSoftwareEmergencyStopAsync(Guid id, EnableDto data)
  {
    if (await _onlineRobotsService.SetSoftwareEmergencyStopAsync(id, data.Enable))
    {
      return NoContent();
    }
    ModelState.AddModelError(nameof(id), "Robot is offline or not found.");
    return ValidationProblem();
  }

  [HttpPatch("{id}/PauseTaskAssigement")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> SetPauseTaskAssigementAsync(Guid id, EnableDto data)
  {
    if (await _onlineRobotsService.SetPauseTaskAssigementAsync(id, data.Enable))
    {
      return NoContent();
    }
    ModelState.AddModelError(nameof(id), "Robot is offline or not found.");
    return ValidationProblem();
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRobot(Guid id, RobotUpdateDto robotUpdateDto)
  {
    if (!await _realmRepository.IsRealmExistsAsync(robotUpdateDto.RealmId))
    {
      ModelState.AddModelError(nameof(robotUpdateDto.RealmId), "Realm does not exist.");
      return ValidationProblem();
    }
    var robotEntity = await _robotRepository.GetRobotAsync(id);
    if (robotEntity == null)
      return NotFound();
    _mapper.Map(robotUpdateDto, robotEntity);
    await _robotRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpPut("{id}/Chassis")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRobotChassisInfo(Guid id, RobotChassisInfoUpdateDto robotChassisInfoUpdateDto)
  {
    var robotChassisInfoEntity = await _robotChassisInfoRepository.GetChassisInfoAsync(id);
    if (robotChassisInfoEntity == null)
      return NotFound();
    _mapper.Map(robotChassisInfoUpdateDto, robotChassisInfoEntity);
    await _robotChassisInfoRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
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
