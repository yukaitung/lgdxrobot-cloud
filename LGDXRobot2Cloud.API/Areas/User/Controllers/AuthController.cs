using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Areas.User.Controllers;

[ApiController]
[Area("User")]
[Route("[area]/[controller]")]
public class AuthController(UserManager<IdentityUser> userManager,
  SignInManager<IdentityUser> signInManager)
{
  private readonly UserManager<IdentityUser> _userManager = userManager;
  private readonly SignInManager<IdentityUser> _signInManager = signInManager;

  [HttpPost("login")]
  public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(LoginRequest login)
  {
    _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

    var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, false, true);
    if (!result.Succeeded)
    {
        return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
    }

    // The signInManager already produced the needed response in the form of a cookie or bearer token.
    return TypedResults.Empty;
  }
}