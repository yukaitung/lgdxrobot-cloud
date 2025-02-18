using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LGDXRobotCloud.Data.Entities;

public class LgdxUser : IdentityUser
{
  [PersonalData]
  public string? Name { get; set; }

  [MaxLength(64)]
  public string? RefreshTokenHash { get; set; }
}