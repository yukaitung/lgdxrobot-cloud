using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Models;
using LGDXRobot2Cloud.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RobotController : ControllerBase
  {
    private readonly INodeRepository _nodeRepository;
    private readonly INodesCollectionRepository _nodesCollectionRepository;
    private readonly IRobotRepository _robotRepository;
    private readonly IMapper _mapper;
    private readonly int maxPageSize = 100;

    public RobotController(INodeRepository nodeRepository,
      INodesCollectionRepository nodesCollectionRepository,
      IRobotRepository robotRepository,
      IMapper mapper)
    {
      _nodeRepository = nodeRepository ?? throw new ArgumentException(nameof(nodeRepository));
      _nodesCollectionRepository = nodesCollectionRepository ?? throw new ArgumentException(nameof(nodesCollectionRepository));
      _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /*
    ** Robot
    */
    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<RobotListDto>>> GetRobots(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (robots, paginationMetadata) = await _robotRepository.GetRobotsAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<RobotListDto>>(robots));
    }

    [HttpGet("{id}", Name = "GetRobot")]
    public async Task<ActionResult<RobotListDto>> GetRobot(int id)
    {
      var robot = await _robotRepository.GetRobotAsync(id);
      if (robot == null)
        return NotFound();
      return Ok(_mapper.Map<RobotListDto>(robot));
    }

    [HttpPost("")]
    public async Task<ActionResult> CreateRobot(RobotCreateDto robotDto)
    {
      var robotEntity = _mapper.Map<Robot>(robotDto);
      await _robotRepository.AddRobotAsync(robotEntity);
      await _robotRepository.SaveChangesAsync();
      var returnRobot = _mapper.Map<RobotListDto>(robotEntity);
      return CreatedAtAction(nameof(GetRobot), new { id = returnRobot.Id }, returnRobot);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRobot(int id)
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
      var (nodes, paginationMetadata) = await _nodeRepository.GetNodesAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
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
    public async Task<ActionResult> UpdateNode(int id, NodeCreateDto nodeDto)
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
      var (nodesCollections, paginationMetadata) = await _nodesCollectionRepository.GetNodesCollectionsAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
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