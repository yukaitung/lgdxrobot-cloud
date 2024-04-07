using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface ISystemComponentRepository
  {
    // Specific Functions
    Task<Dictionary<string, SystemComponent>> GetSystemComponentsDictAsync();
  }
}