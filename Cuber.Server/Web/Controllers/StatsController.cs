using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using Zixoan.Cuber.Server.Stats;

namespace Zixoan.Cuber.Server.Web.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/stats")]
public class StatsController
{
    private readonly IStatsService statsService;

    public StatsController(IStatsService statsService)
        => this.statsService = statsService;

    [HttpGet]
    public IReadOnlyDictionary<string, ProxyStats> Get()
        => this.statsService.Get();

    [HttpGet("{type}")]
    public ProxyStats? Get(string type)
        => this.statsService.Get(type);
}