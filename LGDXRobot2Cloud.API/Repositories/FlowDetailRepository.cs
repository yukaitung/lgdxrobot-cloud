using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface IFlowDetailRepository
{
  Task<FlowDetail?> GetFlowDetailAsync(int flowId, int order, bool startTrigger);
}

public class FlowDetailRepository(LgdxContext context) : IFlowDetailRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<FlowDetail?> GetFlowDetailAsync(int flowId, int order, bool startTrigger = true)
  {
    if (startTrigger)
      return await _context.FlowDetails.Where(fd => fd.FlowId == flowId && fd.Order == order)
        .Include(f => f.StartTrigger)
        .ThenInclude(st => st!.ApiKey)
        .FirstOrDefaultAsync();
    else
      return await _context.FlowDetails.Where(fd => fd.FlowId == flowId && fd.Order == order)
        .Include(f => f.EndTrigger)
        .ThenInclude(et => et!.ApiKey)
        .FirstOrDefaultAsync();
  }
}