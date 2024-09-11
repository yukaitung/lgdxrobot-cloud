using AutoMapper;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Robot.Controllers;

[ApiController]
[Area("Robot")]
[Route("[area]/[controller]")]

public class CollectionsController(
  IMapper mapper,
  INodeRepository nodeRepository,
  INodesCollectionRepository nodesCollectionRepository,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly INodeRepository _nodeRepository = nodeRepository;
  private readonly INodesCollectionRepository _nodesCollectionRepository = nodesCollectionRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<NodesCollectionListDto>>> GetNodesCollections(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (nodesCollections, PaginationHelper) = await _nodesCollectionRepository.GetNodesCollectionsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<NodesCollectionListDto>>(nodesCollections));
  }

  [HttpGet("{id}", Name = "GetNodesCollection")]
  public async Task<ActionResult<NodesCollectionDto>> GetNodesCollection(int id)
  {
    var nodesCollection = await _nodesCollectionRepository.GetNodesCollectionAsync(id);
    if (nodesCollection == null)
      return NotFound();
    return Ok(_mapper.Map<NodesCollectionDto>(nodesCollection));
  }

  private async Task<(NodesCollection?, string)> ValidateNodesCollection(NodesCollectionUpdateDto nodesCollectionDto)
  {
    var nodesCollectionEntity = _mapper.Map<NodesCollection>(nodesCollectionDto);
    // Validate Nodes 
    HashSet<int> nodeIds = [];
    foreach (var node in nodesCollectionDto.Nodes)
      nodeIds.Add(node.NodeId);
    var nodes = await _nodeRepository.GetNodesDictFromListAsync(nodeIds);
    foreach (var nodeId in nodesCollectionEntity.Nodes.Select(n => n.NodeId))
    {
      if (!nodes.ContainsKey(nodeId))
        return (null, $"The Node ID {nodeId} is invalid.");
    }
    return (nodesCollectionEntity, string.Empty);
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateNodesCollection(NodesCollectionCreateDto nodesCollectionDto)
  {
    var (nodesCollectionEntity, error) = await ValidateNodesCollection(_mapper.Map<NodesCollectionUpdateDto>(nodesCollectionDto));
    if (nodesCollectionEntity == null)
      return BadRequest(error);
    await _nodesCollectionRepository.AddNodesCollectionAsync(nodesCollectionEntity);
    await _nodesCollectionRepository.SaveChangesAsync();
    var returnNodesCollection = _mapper.Map<NodesCollectionDto>(nodesCollectionEntity);
    return CreatedAtAction(nameof(GetNodesCollection), new { id = returnNodesCollection.Id }, returnNodesCollection);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateNodesCollection(int id, NodesCollectionUpdateDto nodesCollectionDto)
  {
    var nodesCollectionEntity = await _nodesCollectionRepository.GetNodesCollectionAsync(id);
    if (nodesCollectionEntity == null)
      return NotFound();
    // Ensure the detail does not include Detail ID from other Nodes Collection
    HashSet<int> detailIds = [];
    foreach(var detail in nodesCollectionEntity.Nodes)
      detailIds.Add(detail.Id);
    foreach(var detailId in nodesCollectionDto.Nodes.Select(n => n.Id))
    {
      if (detailId != null && !detailIds.Contains((int)detailId))
        return BadRequest($"The Nodes Collection Detail ID {(int)detailId} is belongs to other Nodes Collection.");
    }
    var (newNodesCollectionEntity, error) = await ValidateNodesCollection(nodesCollectionDto);
    if (newNodesCollectionEntity == null)
      return BadRequest(error);
    _mapper.Map(nodesCollectionDto, nodesCollectionEntity);
    nodesCollectionEntity.UpdatedAt = DateTime.UtcNow;
    await _nodesCollectionRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
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