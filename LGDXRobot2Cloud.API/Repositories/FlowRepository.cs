using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IFlowRepository
  {
    Task<(IEnumerable<Flow>, PaginationMetadata)> GetFlowsAsync(string? name, int pageNumber, int pageSize);
    Task<Flow?> GetFlowAsync(int flowId);
    Task AddFlowAsync(Flow flow);
    void DeleteFlow(Flow flow);
    Task<bool> SaveChangesAsync();

    Task<Flow?> GetFlowProgressesAsync(int flowId);
  }

  public class FlowRepository(LgdxContext context) : IFlowRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<Flow>, PaginationMetadata)> GetFlowsAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Flows as IQueryable<Flow>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(f => f.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      // Does not include FlowDetails to reduce load
      var flows = await query.OrderBy(t => t.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (flows, paginationMetadata);
    }

    public async Task<Flow?> GetFlowAsync(int flowId)
    {
      return await _context.Flows.Where(f => f.Id == flowId)
        .Include(f => f.FlowDetails
          .OrderBy(fd => fd.Order))
        .ThenInclude(fd => fd.Progress)
        .Include(f => f.FlowDetails)
        .ThenInclude(fd => fd.ProceedCondition)
        .Include(f => f.FlowDetails)
        .ThenInclude(fd => fd.StartTrigger)
        .Include(f => f.FlowDetails)
        .ThenInclude(fd => fd.EndTrigger)
        .FirstOrDefaultAsync();
    }

    public async Task AddFlowAsync(Flow flow)
    {
      await _context.Flows.AddAsync(flow);
    }

    public void DeleteFlow(Flow flow)
    {
      _context.Flows.Remove(flow);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }

    public async Task<Flow?> GetFlowProgressesAsync(int flowId)
    {
      return await _context.Flows.Where(f => f.Id == flowId)
        .Include(f => f.FlowDetails
          .OrderBy(fd => fd.Order))
        .ThenInclude(fd => fd.Progress)
        .FirstOrDefaultAsync();
    }
  }
}