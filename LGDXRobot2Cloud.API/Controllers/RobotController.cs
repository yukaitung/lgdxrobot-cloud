using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Protos;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RobotController(IOnlineRobotsService OnlineRobotsService,
    INodeRepository nodeRepository,
    INodesCollectionRepository nodesCollectionRepository,
    IRobotRepository robotRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> options) : ControllerBase
  {
    private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService ?? throw new ArgumentNullException(nameof(OnlineRobotsService));
    private readonly INodeRepository _nodeRepository = nodeRepository ?? throw new ArgumentException(nameof(nodeRepository));
    private readonly INodesCollectionRepository _nodesCollectionRepository = nodesCollectionRepository ?? throw new ArgumentException(nameof(nodesCollectionRepository));
    private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = options.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly int maxPageSize = 100;

    public record RobotCertificates {
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
      var certificateNotAfter = DateTimeOffset.Now.AddDays(_lgdxRobot2Configuration.RobotCertificateValidDay);

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

    /*
    ** Robot
    */
    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<RobotListDto>>> GetRobots(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
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
      var robotEntity = _mapper.Map<Robot>(robotDto);
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

    /*
    ** Nodes
    */
    [HttpGet("nodes")]
    public async Task<ActionResult<IEnumerable<NodeDto>>> GetNodes(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (nodes, PaginationHelper) = await _nodeRepository.GetNodesAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
      return Ok(_mapper.Map<IEnumerable<NodeDto>>(nodes));
    }

    [HttpGet("nodes/{id}", Name = "GetNode")]
    public async Task<ActionResult<NodeDto>> GetNode(int id)
    {
      var node = await _nodeRepository.GetNodeAsync(id);
      if (node == null)
        return NotFound();
      return Ok(_mapper.Map<NodeDto>(node));
    }

    [HttpPost("nodes")]
    public async Task<ActionResult> CreateNode(NodeCreateDto nodeDto)
    {
      var nodeEntity = _mapper.Map<Node>(nodeDto);
      await _nodeRepository.AddNodeAsync(nodeEntity);
      await _nodeRepository.SaveChangesAsync();
      var returnNode = _mapper.Map<NodeDto>(nodeEntity);
      return CreatedAtAction(nameof(GetNode), new { id = returnNode.Id }, returnNode);
    }

    [HttpPut("nodes/{id}")]
    public async Task<ActionResult> UpdateNode(int id, NodeUpdateDto nodeDto)
    {
      var nodeEntity = await _nodeRepository.GetNodeAsync(id);
      if (nodeEntity == null)
        return NotFound();
      _mapper.Map(nodeDto, nodeEntity);
      nodeEntity.UpdatedAt = DateTime.UtcNow;
      await _nodeRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("nodes/{id}")]
    public async Task<ActionResult> DeleteNode(int id)
    {
      var node = await _nodeRepository.GetNodeAsync(id);
      if (node == null)
        return NotFound();
      _nodeRepository.DeleteNode(node);
      await _nodeRepository.SaveChangesAsync();
      return NoContent();
    }

    /*
    ** Nodes Collection
    */
    [HttpGet("collections")]
    public async Task<ActionResult<IEnumerable<NodesCollectionListDto>>> GetNodesCollections(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (nodesCollections, PaginationHelper) = await _nodesCollectionRepository.GetNodesCollectionsAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
      return Ok(_mapper.Map<IEnumerable<NodesCollectionListDto>>(nodesCollections));
    }

    [HttpGet("collections/{id}", Name = "GetNodesCollection")]
    public async Task<ActionResult<NodesCollectionDto>> GetNodesCollection(int id)
    {
      var nodesCollection = await _nodesCollectionRepository.GetNodesCollectionAsync(id);
      if (nodesCollection == null)
        return NotFound();
      return Ok(_mapper.Map<NodesCollectionDto>(nodesCollection));
    }

    [HttpPost("collections")]
    public async Task<ActionResult> CreateNodesCollection(NodesCollectionCreateDto nodesCollectionDto)
    {
      var nodesCollectionEntity = _mapper.Map<NodesCollection>(nodesCollectionDto);
      // Validate Nodes &, add to Entity
      HashSet<int> nodeIds = new HashSet<int>();
      foreach (var node in nodesCollectionDto.Nodes)
        nodeIds.Add(node.NodeId);
      var nodes = await _nodeRepository.GetNodesDictFromListAsync(nodeIds);
      foreach (var node in nodesCollectionEntity.Nodes)
      {
        if(nodes.ContainsKey(node.NodeId))
          node.Node = nodes[node.NodeId];
        else
          return BadRequest($"The Node ID {node.NodeId} is invalid.");
      }
      await _nodesCollectionRepository.AddNodesCollectionAsync(nodesCollectionEntity);
      await _nodesCollectionRepository.SaveChangesAsync();
      var returnNodesCollection = _mapper.Map<NodesCollectionDto>(nodesCollectionEntity);
      return CreatedAtAction(nameof(GetNodesCollection), new { id = returnNodesCollection.Id }, returnNodesCollection);
    }

    [HttpPut("collections/{id}")]
    public async Task<ActionResult> UpdateNodesCollection(int id, NodesCollectionUpdateDto nodesCollectionDto)
    {
      var nodesCollectionEntity = await _nodesCollectionRepository.GetNodesCollectionAsync(id);
      if (nodesCollectionEntity == null)
        return NotFound();
      // Ensure the detail does not include Detail ID from other Nodes Collection
      HashSet<int> detailIds = new HashSet<int>();
      foreach(var detail in nodesCollectionEntity.Nodes)
        detailIds.Add(detail.Id);
      foreach(var detailDto in nodesCollectionDto.Nodes)
      {
        if (detailDto.Id != null && !detailIds.Contains((int)detailDto.Id))
          return BadRequest($"The Nodes Collection Detail ID {(int)detailDto.Id} is belongs to other Nodes Collection.");
      }
      _mapper.Map(nodesCollectionDto, nodesCollectionEntity);
      // Validate Nodes &, add to Entity
      HashSet<int> nodeIds = new HashSet<int>();
      foreach (var node in nodesCollectionDto.Nodes)
        nodeIds.Add(node.NodeId);
      var nodes = await _nodeRepository.GetNodesDictFromListAsync(nodeIds);
      foreach (var node in nodesCollectionEntity.Nodes)
      {
        if(nodes.ContainsKey(node.NodeId))
          node.Node = nodes[node.NodeId];
        else
          return BadRequest($"The Node ID {node.NodeId} is invalid.");
      }
      
      nodesCollectionEntity.UpdatedAt = DateTime.UtcNow;
      await _nodesCollectionRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("collections/{id}")]
    public async Task<ActionResult> DeleteNodesCollection(int id)
    {
      var nodesCollection = await _nodesCollectionRepository.GetNodesCollectionAsync(id);
      if(nodesCollection == null)
        return NotFound();
      _nodesCollectionRepository.DeleteNodesCollection(nodesCollection);
      await _nodesCollectionRepository.SaveChangesAsync();
      return NoContent();
    }
  }
}