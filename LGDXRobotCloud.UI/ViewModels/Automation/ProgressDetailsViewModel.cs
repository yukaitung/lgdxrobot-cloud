using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Automation;

public class ProgressDetailsViewModel : FormViewModel
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  public bool System { get; set; }

  public bool Reserved { get; set; }
}

public static class ProgressDetailsViewModelExtensions
{
  public static void FromDto(this ProgressDetailsViewModel ProgressDetailsViewModel, ProgressDto progressDto)
  {
    ProgressDetailsViewModel.Id = (int)progressDto.Id!;
    ProgressDetailsViewModel.Name = progressDto.Name!;
    ProgressDetailsViewModel.System = (bool)progressDto.System!;
    ProgressDetailsViewModel.Reserved = (bool)progressDto.Reserved!;
  }

  public static ProgressUpdateDto ToUpdateDto(this ProgressDetailsViewModel ProgressDetailsViewModel)
  {
    return new ProgressUpdateDto {
      Name = ProgressDetailsViewModel.Name,
    };
  }

  public static ProgressCreateDto ToCreateDto(this ProgressDetailsViewModel ProgressDetailsViewModel)
  {
    return new ProgressCreateDto {
      Name = ProgressDetailsViewModel.Name,
    };
  }
}