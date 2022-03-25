using System.Net;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing;

public interface ILoadBalanceStrategy
{
    Target? GetTarget(EndPoint? sourceEndPoint);
}