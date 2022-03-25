using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe;

public class HttpHealthProbe : IHealthProbe
{
    private readonly CuberOptions cuberOptions;
    private readonly HttpClient httpClient;

    public HttpHealthProbe(IOptions<CuberOptions> options)
    {
        this.cuberOptions = options.Value;
        TimeSpan timeout = TimeSpan.FromMilliseconds(this.cuberOptions.HealthProbe!.Timeout);
        this.httpClient = new HttpClient { Timeout = timeout };
    }

    public async Task<bool> IsReachableAsync(Target target, CancellationToken cancellationToken = default)
    {
        try
        {
            var uriBuilder = new UriBuilder(
                "http",
                target.Ip,
                this.cuberOptions.HealthProbe?.Port ?? target.Port,
                this.cuberOptions.HealthProbe?.Path
            );

            HttpResponseMessage response = await httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception exception) when (exception is ArgumentException or HttpRequestException)
        {
            return false;
        }
    }
}