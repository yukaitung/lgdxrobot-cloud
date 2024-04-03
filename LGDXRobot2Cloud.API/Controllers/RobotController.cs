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
    private readonly IRobotRepository _robotRepository;
    private readonly IMapper _mapper;
    private readonly int maxPageSize = 100;

    public RobotController(INodeRepository nodeRepository,
      IRobotRepository robotRepository,
      IMapper mapper)
    {
      _nodeRepository = nodeRepository ?? throw new ArgumentException(nameof(nodeRepository));
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
  }
}