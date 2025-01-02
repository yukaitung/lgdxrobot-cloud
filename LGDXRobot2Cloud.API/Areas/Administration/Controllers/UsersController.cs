using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services.Common;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Data.Models.Emails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class UsersController(
    IEmailService emailService,
    ILgdxUsersRepository lgdxUsersRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    SignInManager<LgdxUser> signInManager,
    UserManager<LgdxUser> userManager
  ) : ControllerBase
{
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly ILgdxUsersRepository _lgdxUsersRepository = lgdxUsersRepository ?? throw new ArgumentNullException(nameof(lgdxUsersRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2Configuration));
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2SecretConfiguration));
  private readonly SignInManager<LgdxUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<LgdxUserListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<LgdxUserListDto>>> GetUsers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (users, PaginationHelper) = await _lgdxUsersRepository.GetUsersAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<LgdxUserListDto>>(users));
  }

  [HttpGet("{id}", Name = "GetUser")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LgdxUserDto>> GetUser(Guid id)
  {
    var user = await _userManager.FindByIdAsync(id.ToString());
    if (user == null)
    {
      return NotFound();
    }
    var returnDto = _mapper.Map<LgdxUserDto>(user);
    returnDto.Roles = await _userManager.GetRolesAsync(user);
    return Ok(returnDto);
  }

  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateUser(LgdxUserCreateAdminDto lgdxUserCreateAdminDto)
  {
    var user = new LgdxUser
    {
      Email = lgdxUserCreateAdminDto.Email,
      EmailConfirmed = true,
      LockoutEnabled = true,
      Name = lgdxUserCreateAdminDto.Name,
      NormalizedEmail = lgdxUserCreateAdminDto.Email.ToUpper(),
      NormalizedUserName = lgdxUserCreateAdminDto.UserName.ToUpper(),
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = lgdxUserCreateAdminDto.UserName
    };
    if (lgdxUserCreateAdminDto.Password != null)
    {
      var password = new PasswordHasher<LgdxUser>();
      var hashed = password.HashPassword(user, lgdxUserCreateAdminDto.Password);
      user.PasswordHash = hashed;
    }
    var result = await _userManager.CreateAsync(user);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    var userEntity = await _userManager.FindByNameAsync(lgdxUserCreateAdminDto.UserName);
    var roleToAdd = lgdxUserCreateAdminDto.Roles;
    result = await _userManager.AddToRolesAsync(userEntity!, roleToAdd);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }

    userEntity = await _userManager.FindByNameAsync(lgdxUserCreateAdminDto.UserName);
    // Send Email
    if (string.IsNullOrWhiteSpace(lgdxUserCreateAdminDto.Password))
    {
      // No password is specified
      var token = await _userManager.GeneratePasswordResetTokenAsync(userEntity!);
      await _emailService.SendWellcomePasswordSetEmailAsync(
        lgdxUserCreateAdminDto.Email, 
        lgdxUserCreateAdminDto.Name, 
        lgdxUserCreateAdminDto.UserName,
        token
      );
    }
    else
    {
      // Password is specified
      await _emailService.SendWelcomeEmailAsync(
        lgdxUserCreateAdminDto.Email, 
        lgdxUserCreateAdminDto.Name, 
        lgdxUserCreateAdminDto.UserName
      );
    }
    var lgdxUserDto = _mapper.Map<LgdxUserDto>(userEntity);
    return CreatedAtAction(nameof(GetUser), new { id = lgdxUserDto.Id }, lgdxUserDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateUser(Guid id, LgdxUserUpdateAdminDto lgdxUserUpdateAdminDto)
  {
    var userEntity = await _userManager.FindByIdAsync(id.ToString());
    if (userEntity == null)
    {
      return NotFound();
    }
    _mapper.Map(lgdxUserUpdateAdminDto, userEntity);
    var result = await _userManager.UpdateAsync(userEntity);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    var rolesEntity = await _userManager.GetRolesAsync(userEntity);
    var roleToAdd = lgdxUserUpdateAdminDto.Roles.Except(rolesEntity);
    result = await _userManager.AddToRolesAsync(userEntity, roleToAdd);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    var roleToRemove = rolesEntity.Except(lgdxUserUpdateAdminDto.Roles);
    result = await _userManager.RemoveFromRolesAsync(userEntity, roleToRemove);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    return NoContent();
  }

  [HttpPatch("{id}/Unlock")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UnlockUser(Guid id)
  {
    var user = await _userManager.FindByIdAsync(id.ToString());
    if (user == null)
    {
      return NotFound();
    }
    user.AccessFailedCount = 0;
    user.LockoutEnd = null;
    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteUser(Guid id)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await _userManager.FindByIdAsync(id.ToString());
    if (user == null)
    {
      return NotFound();
    }
    if (user.Id == userId)
    {
      ModelState.AddModelError(nameof(id), "Cannot delete yourself.");
      return ValidationProblem();
    }
    var result = await _userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    return NoContent();
  }
}