using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Automation;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Automation;

public interface IProgressService
{
  Task<(IEnumerable<ProgressBusinessModel>, PaginationHelper)> GetProgressesAsync(string? name, int pageNumber, int pageSize, bool system = false);
  Task<ProgressBusinessModel> GetProgressAsync(int progressId);
  Task<ProgressBusinessModel> CreateProgressAsync(ProgressCreateBusinessModel progressCreateBusinessModel);
  Task<bool> UpdateProgressAsync(int progressId, ProgressUpdateBusinessModel progressUpdateBusinessModel);
  Task<bool> TestDeleteProgressAsync(int progressId);
  Task<bool> DeleteProgressAsync(int progressId);
  
  Task<IEnumerable<ProgressSearchBusinessModel>> SearchProgressesAsync(string? name, bool reserved = false);
}

public class ProgressService(LgdxContext context) : IProgressService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<ProgressBusinessModel>, PaginationHelper)> GetProgressesAsync(string? name, int pageNumber, int pageSize, bool system)
  {
    var query = _context.Progresses as IQueryable<Progress>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(t => t.Name.Contains(name));
    }
    query = query.Where(t => t.System == system);
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var progresses = await query.AsNoTracking()
      .OrderBy(p => p.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(p => new ProgressBusinessModel{
        Id = p.Id,
        Name = p.Name,
        System = p.System,
        Reserved = p.Reserved,
      })
      .ToListAsync();
    return (progresses, PaginationHelper);
  }

  public async Task<ProgressBusinessModel> GetProgressAsync(int progressId)
  {
    return await _context.Progresses.AsNoTracking()
      .Where(p => p.Id == progressId)
      .Select(p => new ProgressBusinessModel {
        Id = p.Id,
        Name = p.Name,
        System = p.System,
        Reserved = p.Reserved,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  public async Task<ProgressBusinessModel> CreateProgressAsync(ProgressCreateBusinessModel progressCreateBusinessModel)
  {
    var progress = new Progress {
      Name = progressCreateBusinessModel.Name,
    };
    await _context.Progresses.AddAsync(progress);
    await _context.SaveChangesAsync();
    return new ProgressBusinessModel {
      Id = progress.Id,
      Name = progress.Name,
      System = progress.System,
      Reserved = progress.Reserved,
    };
  }

  public async Task<bool> UpdateProgressAsync(int progressId, ProgressUpdateBusinessModel progressUpdateBusinessModel)
  {
    var progress = await _context.Progresses.Where(p => p.Id == progressId)
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    if (progress.System)
    {
      throw new LgdxValidation400Expection(nameof(progressId), $"Cannot update system progress.");
    }
    progress.Name = progressUpdateBusinessModel.Name;
    return await _context.SaveChangesAsync() >= 1;
  }

  public async Task<bool> TestDeleteProgressAsync(int progressId)
  {
    var progress = await _context.Progresses.AsNoTracking()
      .Where(p => p.Id == progressId)
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    if (progress.System)
    {
      throw new LgdxValidation400Expection(nameof(progressId), $"Cannot delete system progress.");
    }

    var depeendencies = await _context.FlowDetails.Where(t => t.ProgressId == progressId).CountAsync();
    if (depeendencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(progressId), $"This progress has been used by {depeendencies} details in flows.");
    }
    // Don't check AutoTasks because it dependes on the Flow/FlowDetails

    return true;
  }


  public async Task<bool> DeleteProgressAsync(int progressId)
  {
    var progress = await _context.Progresses.AsNoTracking()
      .Where(p => p.Id == progressId)
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    if (progress.System)
    {
      throw new LgdxValidation400Expection(nameof(progressId), $"Cannot delete system progress.");
    }

    return await _context.Progresses.Where(p => p.Id == progressId)
      .ExecuteDeleteAsync() >= 1;
  }

  public async Task<IEnumerable<ProgressSearchBusinessModel>> SearchProgressesAsync(string? name, bool reserved)
  {
    var n = name ?? string.Empty;
    return await _context.Progresses.AsNoTracking()
      .Where(t => t.Name.ToLower().Contains(n.ToLower()))
      .Where(t => t.Reserved == reserved)
      .Take(10)
      .Select(t => new ProgressSearchBusinessModel {
        Id = t.Id,
        Name = t.Name,
      })
      .ToListAsync();
  }
}