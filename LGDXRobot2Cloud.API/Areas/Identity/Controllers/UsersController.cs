using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Data.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class UsersController(
    ILgdxUsersRepository lgdxUsersRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    SignInManager<LgdxUser> signInManager,
    UserManager<LgdxUser> userManager
  ) : ControllerBase
{
  private readonly ILgdxUsersRepository _lgdxUsersRepository = lgdxUsersRepository ?? throw new ArgumentNullException(nameof(lgdxUsersRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2Configuration));
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2SecretConfiguration));
  private readonly SignInManager<LgdxUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<LgdxUserListDto>>> GetUsers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (users, PaginationHelper) = await _lgdxUsersRepository.GetUsersAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<LgdxUserListDto>>(users));
  }

  [HttpGet("{id}", Name = "GetUser")]
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
  public async Task<ActionResult> CreateUser(LgdxUserCreateDto lgdxUserCreateDto)
  {
    var user = new LgdxUser
    {
      Email = lgdxUserCreateDto.Email,
      EmailConfirmed = true,
      LockoutEnabled = true,
      Name = lgdxUserCreateDto.Name,
      NormalizedEmail = lgdxUserCreateDto.Email.ToUpper(),
      NormalizedUserName = lgdxUserCreateDto.UserName.ToUpper(),
      SecurityStamp = Guid.NewGuid().ToString("D"),
      UserName = lgdxUserCreateDto.UserName
    };
    if (lgdxUserCreateDto.Password != null)
    {
      var password = new PasswordHasher<LgdxUser>();
      var hashed = password.HashPassword(user, lgdxUserCreateDto.Password);
      user.PasswordHash = hashed;
    }
    var result = await _userManager.CreateAsync(user);
    if (!result.Succeeded)
    {
      return BadRequest();
    }
    var userEntity = await _userManager.FindByNameAsync(lgdxUserCreateDto.UserName);
    var roleToAdd = lgdxUserCreateDto.Roles;
    result = await _userManager.AddToRolesAsync(userEntity!, roleToAdd);
    if (!result.Succeeded)
    {
      return BadRequest();
    }
    userEntity = await _userManager.FindByNameAsync(lgdxUserCreateDto.UserName);
    var returnUser = _mapper.Map<LgdxUserDto>(userEntity);
    return CreatedAtAction(nameof(GetUser), new { id = returnUser.Id }, returnUser);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateUser(Guid id, LgdxUserUpdateAdminDto lgdxUserUpdateDto)
  {
    var userEntity = await _userManager.FindByIdAsync(id.ToString());
    if (userEntity == null)
    {
      return NotFound();
    }
    _mapper.Map(lgdxUserUpdateDto, userEntity);
    var result = await _userManager.UpdateAsync(userEntity);
    if (!result.Succeeded)
    {
      return BadRequest();
    }
    var rolesEntity = await _userManager.GetRolesAsync(userEntity);
    var roleToAdd = lgdxUserUpdateDto.Roles.Except(rolesEntity);
    result = await _userManager.AddToRolesAsync(userEntity, roleToAdd);
    if (!result.Succeeded)
    {
      return BadRequest();
    }
    var roleToRemove = rolesEntity.Except(lgdxUserUpdateDto.Roles);
    result = await _userManager.RemoveFromRolesAsync(userEntity, roleToRemove);
    if (!result.Succeeded)
    {
      return BadRequest();
    }
    return NoContent();
  }

  [HttpPatch("{id}/unlock")]
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
      return BadRequest();
    }
    return NoContent();
  }

  [HttpDelete("{id}")]
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
      return BadRequest("Cannot delete yourself.");
    }
    var result = await _userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
      return BadRequest();
    }
    return NoContent();
  }
}