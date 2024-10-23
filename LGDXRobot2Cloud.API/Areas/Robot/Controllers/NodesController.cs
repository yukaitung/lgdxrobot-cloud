using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Robot.Controllers;

[ApiController]
[Area("Robot")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class NodesController(
  IMapper mapper,
  INodeRepository nodeRepository,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly INodeRepository _nodeRepository = nodeRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<NodeDto>>> GetNodes(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (nodes, PaginationHelper) = await _nodeRepository.GetNodesAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<NodeDto>>(nodes));
  }

  [HttpGet("{id}", Name = "GetNode")]
  public async Task<ActionResult<NodeDto>> GetNode(int id)
  {
    var node = await _nodeRepository.GetNodeAsync(id);
    if (node == null)
      return NotFound();
    return Ok(_mapper.Map<NodeDto>(node));
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateNode(NodeCreateDto nodeDto)
  {
    var nodeEntity = _mapper.Map<Node>(nodeDto);
    await _nodeRepository.AddNodeAsync(nodeEntity);
    await _nodeRepository.SaveChangesAsync();
    var returnNode = _mapper.Map<NodeDto>(nodeEntity);
    return CreatedAtAction(nameof(GetNode), new { id = returnNode.Id }, returnNode);
  }

  [HttpPut("{id}")]
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

  [HttpDelete("{id}")]
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