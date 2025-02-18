using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public sealed class UserDetailViewModel : FormViewModel
{
  public Guid Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public string UserName { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public string Email { get; set; } = null!;

  public List<string> Roles { get; set; } = [];
}

public static class UserDetailViewModelExtensions
{
  public static void FromDto(this UserDetailViewModel userDetailViewModel, LgdxUserDto lgdxUserDto)
  {
    userDetailViewModel.Id = (Guid)lgdxUserDto.Id!;
    userDetailViewModel.Name = lgdxUserDto.Name!;
    userDetailViewModel.UserName = lgdxUserDto.UserName!;
    userDetailViewModel.Email = lgdxUserDto.Email!;
    userDetailViewModel.Roles = lgdxUserDto.Roles!;
  }

  public static LgdxUserUpdateDto ToUpdateDto(this UserDetailViewModel userDetailViewModel)
  {
    return new LgdxUserUpdateDto {
      Name = userDetailViewModel.Name,
      Email = userDetailViewModel.Email,
    };
  }
}