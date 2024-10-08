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
using System.Text.Json;
using LGDXRobot2Cloud.Data.Entities;

namespace LGDXRobot2Cloud.API.Areas.Robot.Controllers;

[ApiController]
[Area("Robot")]
[Route("[area]")]

public class RobotController(
  IMapper mapper,
  IOnlineRobotsService OnlineRobotsService,
  IOptionsSnapshot<LgdxRobot2Configuration> options,
  IRobotRepository robotRepository,
  IRobotCertificateRepository robotCertificateRepository) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly IRobotRepository _robotRepository = robotRepository;
  private readonly IRobotCertificateRepository _robotCertificateRepository = robotCertificateRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = options.Value;

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
    var robotEntity = _mapper.Map<Entities.Robot>(robotDto.RobotInfo);
    robotEntity.Id = Guid.NewGuid();
    CertificateDetail certificates = _robotCertificateRepository.GenerateRobotCertificate(robotEntity.Id);
    robotEntity.Certificate = new RobotCertificate {
      Thumbprint = certificates.RobotCertificateThumbprint,
      NotBefore = certificates.RobotCertificateNotBefore,
      NotAfter = certificates.RobotCertificateNotAfter
    };
    var robotChassisInfoEntity = _mapper.Map<RobotChassisInfo>(robotDto.RobotChassisInfo);
    robotEntity.RobotChassisInfo = robotChassisInfoEntity;
    await _robotRepository.AddRobotAsync(robotEntity);
    await _robotRepository.SaveChangesAsync();
    var response = new RobotCertificateIssueDto
    {
      RobotId = robotEntity.Id,
      RobotName = robotEntity.Name,
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
