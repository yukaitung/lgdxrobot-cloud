using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public class ProgressDetailViewModel : FormViewModel
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  public bool System { get; set; }

  public bool Reserved { get; set; }
}

public static class ProgressDetailViewModelExtensions
{
  public static void FromDto(this ProgressDetailViewModel progressDetailViewModel, ProgressDto progressDto)
  {
    progressDetailViewModel.Id = (int)progressDto.Id!;
    progressDetailViewModel.Name = progressDto.Name!;
    progressDetailViewModel.System = (bool)progressDto.System!;
    progressDetailViewModel.Reserved = (bool)progressDto.Reserved!;
  }

  public static ProgressUpdateDto ToUpdateDto(this ProgressDetailViewModel progressDetailViewModel)
  {
    return new ProgressUpdateDto {
      Name = progressDetailViewModel.Name,
    };
  }

  public static ProgressCreateDto ToCreateDto(this ProgressDetailViewModel progressDetailViewModel)
  {
    return new ProgressCreateDto {
      Name = progressDetailViewModel.Name,
    };
  }
}