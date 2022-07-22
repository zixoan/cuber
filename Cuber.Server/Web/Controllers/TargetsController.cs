using Microsoft.AspNetCore.Mvc;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;
using Zixoan.Cuber.Server.Web.Dtos;

namespace Zixoan.Cuber.Server.Web.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/targets")]
public class TargetsController : ControllerBase
{
    private readonly ITargetProvider targetProvider;

    public TargetsController(ITargetProvider targetProvider)
        => this.targetProvider = targetProvider;

    [HttpGet]
    public IList<Target> Get()
        => this.targetProvider.Targets;

    [HttpGet("{ip}")]
    public IEnumerable<Target> Get(string ip)
        => this.targetProvider.Targets.Where(target => target.Ip == ip);

    [HttpGet("{ip}/{port}")]
    public Target? Get(string ip, ushort port)
        => this.targetProvider.Targets.FirstOrDefault(target => target.Ip == ip && target.Port == port);

    [HttpPost]
    public IActionResult Post(TargetDto targetDto)
    {
        if (this.targetProvider.Targets.Any(target => target.Ip == targetDto.Ip && target.Port == targetDto.Port))
        {
            return this.BadRequest(new { Message = $"Target with IP {targetDto.Ip} and port {targetDto.Port} already exists" });
        }

        Target target = new Target(targetDto.Ip, targetDto.Port);
        this.targetProvider.Add(target);

        return this.Ok();
    }

    [HttpDelete("{ip}/{port}")]
    public IActionResult Delete(string ip, ushort port)
    {
        Target? target = this.targetProvider.Targets.FirstOrDefault(target => target.Ip == ip && target.Port == port);
        if (target == null)
        {
            return this.NotFound(new { Message = $"Target with IP {ip} and port {port} does not exist" });
        }

        this.targetProvider.Remove(this.targetProvider.Targets.IndexOf(target));

        return this.Ok();
    }
}