using LGDXRobot2Cloud.Data.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;


namespace LGDXRobot2Cloud.API.Areas.Setting.Controllers;

[ApiController]
[Area("Setting")]
[Route("[area]/[controller]")]
public class TestController(IBus bus) : ControllerBase
{
  private readonly IBus _bus = bus;

  [HttpGet("test")]
  public async Task<ActionResult<string>> Test()
  {
    await _bus.Publish(new BusTest{
      Value = "Test12345"
    });
    return Ok("Test");
  }
}