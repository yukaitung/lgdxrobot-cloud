using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface INodeRepository
  {
    Task<(IEnumerable<Node>, PaginationMetadata)> GetNodesAsync(string? name, int pageNumber, int pageSize);
    Task<Node?> GetNodeAsync(int nodeId);
    Task AddNodeAsync(Node node);
    void DeleteNode(Node node);
    Task<bool> SaveChangesAsync();

    // Specific Functions
    Task<Dictionary<int, Node>> GetNodesDictFromListAsync(IEnumerable<int> nodeIds);
  }

  public class NodeRepository(LgdxContext context) : INodeRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<Node>, PaginationMetadata)> GetNodesAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Nodes as IQueryable<Node>;
      if (!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(n => n.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      var nodes = await query.OrderBy(n => n.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (nodes, paginationMetadata);
    }
    
    public async Task<Node?> GetNodeAsync(int nodeId)
    {
      return await _context.Nodes.Where(n => n.Id == nodeId).FirstOrDefaultAsync();
    }

    public async Task AddNodeAsync(Node node)
    {
      await _context.Nodes.AddAsync(node);
    }

    public void DeleteNode(Node node)
    {
      _context.Nodes.Remove(node);
    }
    
    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }

    public async Task<Dictionary<int, Node>> GetNodesDictFromListAsync(IEnumerable<int> nodeIds)
    {
      return await _context.Nodes.Where(n => nodeIds.Contains(n.Id))
        .ToDictionaryAsync(n => n.Id, n => n);
    }
  }
}