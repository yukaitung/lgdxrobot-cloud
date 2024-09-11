using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface INodesCollectionRepository
  {
    Task<(IEnumerable<NodesCollection>, PaginationHelper)> GetNodesCollectionsAsync(string? name, int pageNumber, int pageSize);
    Task<NodesCollection?> GetNodesCollectionAsync(int nodesCollectionId);
    Task AddNodesCollectionAsync(NodesCollection nodesCollection);
    void DeleteNodesCollection(NodesCollection nodesCollection);
    Task<bool> SaveChangesAsync();
  }

  public class NodesCollectionRepository(LgdxContext context) : INodesCollectionRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<NodesCollection>, PaginationHelper)> GetNodesCollectionsAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.NodesCollections as IQueryable<NodesCollection>;
      if (!string.IsNullOrEmpty(name))
      {
        name = name.Trim();
        query = query.Where(n => n.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
      var nodesCollections = await query.AsNoTracking()
        .OrderBy(n => n.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (nodesCollections, PaginationHelper);
    }
    
    public async Task<NodesCollection?> GetNodesCollectionAsync(int nodesCollectionId)
    {
      return await _context.NodesCollections.Where(n => n.Id == nodesCollectionId)
        .Include(n => n.Nodes)
        .ThenInclude(n => n.Node)
        .FirstOrDefaultAsync();
    }

    public async Task AddNodesCollectionAsync(NodesCollection nodesCollection)
    {
      await _context.NodesCollections.AddAsync(nodesCollection);
    }

    public void DeleteNodesCollection(NodesCollection nodesCollection)
    {
      _context.NodesCollections.Remove(nodesCollection);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}