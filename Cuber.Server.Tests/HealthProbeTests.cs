using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Options;

using Xunit;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;

namespace Zixoan.Cuber.Server.Tests;

public class HealthProbeTests
{
    private readonly IOptions<CuberOptions> cuberOptions = Options.Create(new CuberOptions
    {
        HealthProbe = new HealthProbe
        {
            Timeout = 2000
        }
    });

    [Fact]
    public async Task TcpHealthProbeSuccessful()
    {
        IHealthProbe tcpProbe = new TcpHealthProbe(this.cuberOptions);

        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();

        var target = new Target(IPAddress.Loopback.ToString(), (ushort)((IPEndPoint)tcpListener.LocalEndpoint).Port);

        Task<bool> reachableTask = tcpProbe.IsReachableAsync(target);

        Socket client = await tcpListener.AcceptSocketAsync();
        client.Close();

        Assert.True(await reachableTask);
    }

    [Fact]
    public async Task TcpHealthProbeUnsuccessful()
    {
        IHealthProbe tcpProbe = new TcpHealthProbe(this.cuberOptions);

        var target = new Target(IPAddress.Loopback.ToString(), 0);

        Assert.False(await tcpProbe.IsReachableAsync(target));
    }

    [Fact]
    public async Task HttpHealthProbeSuccessful()
    {
        IHealthProbe httpProbe = new HttpHealthProbe(this.cuberOptions);

        ushort port = PortHelper.GetFreePort();

        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
        httpListener.Start();

        var target = new Target(IPAddress.Loopback.ToString(), port);

        Task<bool> reachableTask = httpProbe.IsReachableAsync(target);

        HttpListenerContext client = await httpListener.GetContextAsync();
        client.Response.StatusCode = (int)HttpStatusCode.OK;
        client.Response.Close();

        Assert.True(await reachableTask);
    }

    [Fact]
    public async Task HttpHealthProbeUnsuccessful()
    {
        IHealthProbe httpProbe = new HttpHealthProbe(this.cuberOptions);

        var target = new Target(IPAddress.Loopback.ToString(), 0);

        Assert.False(await httpProbe.IsReachableAsync(target));
    }
}