using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public class LoginViewModel
{
  [Required (ErrorMessage = "Please enter a username.")]
  public string Username { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a password.")]
  public string Password { get; set; } = null!;

  public IDictionary<string,string[]>? Errors { get; set; }
}