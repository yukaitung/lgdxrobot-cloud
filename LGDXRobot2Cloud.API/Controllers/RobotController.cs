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

    
    /*
    ** Robot
    */
    

    /*
    ** Nodes
    */
    

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