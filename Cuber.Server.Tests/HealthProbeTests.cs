﻿using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Xunit;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;

namespace Zixoan.Cuber.Server.Tests
{
    public class HealthProbeTests
    {
        private readonly IOptions<CuberOptions> CuberOptions = Options.Create(new CuberOptions { HealthProbe = new HealthProbe() });

        [Fact]
        public async Task TcpHealthProbeSuccessful()
        {
            IHealthProbe tcpProbe = new TcpHealthProbe(CuberOptions);

            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();

            Target target = new Target { Ip = IPAddress.Loopback.ToString(), Port = ((IPEndPoint)tcpListener.LocalEndpoint).Port };

            Task<bool> reachableTask = tcpProbe.IsReachable(target);

            Socket client = await tcpListener.AcceptSocketAsync();
            client.Close();

            Assert.True(await reachableTask);
        }

        [Fact]
        public async Task TcpHealthProbeUnsuccessful()
        {
            IHealthProbe tcpProbe = new TcpHealthProbe(CuberOptions);

            Target target = new Target { Ip = IPAddress.Loopback.ToString(), Port = 0 };

            Assert.False(await tcpProbe.IsReachable(target));
        }

        [Fact]
        public async Task HttpHealthProbeSuccessful()
        {
            IHealthProbe httpProbe = new HttpHealthProbe(CuberOptions);

            int port = PortHelper.GetFreePort();

            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
            httpListener.Start();

            Target target = new Target { Ip = IPAddress.Loopback.ToString(), Port = port };

            Task<bool> reachableTask = httpProbe.IsReachable(target);

            HttpListenerContext client = await httpListener.GetContextAsync();
            client.Response.StatusCode = (int)HttpStatusCode.OK;
            client.Response.Close();

            Assert.True(await reachableTask);
        }

        [Fact]
        public async Task HttpHealthProbeUnsuccessful()
        {
            IHealthProbe httpProbe = new HttpHealthProbe(CuberOptions);

            Target target = new Target { Ip = IPAddress.Loopback.ToString(), Port = 0 };

            Assert.False(await httpProbe.IsReachable(target));
        }
    }
}
