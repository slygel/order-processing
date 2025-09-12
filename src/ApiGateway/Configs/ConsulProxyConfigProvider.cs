using Consul;
using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Configs;

public class ConsulProxyConfigProvider : IProxyConfigProvider
{
    private readonly IConsulClient _consul;
    private ConsulProxyConfig _config;

    public ConsulProxyConfigProvider(IConsulClient consul)
    {
        _consul = consul;
        _config = BuildConfig();
    }

    public IProxyConfig GetConfig() => _config;

    private ConsulProxyConfig BuildConfig()
    {
        var services = _consul.Agent.Services().Result.Response;

        var clusters = services.Values
            .GroupBy(s => s.Service)
            .Select(g => new ClusterConfig
            {
                ClusterId = g.Key,
                Destinations = g.ToDictionary(
                    s => s.ID,
                    s => new Yarp.ReverseProxy.Configuration.DestinationConfig
                    {
                        Address = $"http://{s.Address}:{s.Port}"
                    })
            }).ToList();

        var routes = clusters.Select(c => new Yarp.ReverseProxy.Configuration.RouteConfig
        {
            RouteId = c.ClusterId + "-route",
            ClusterId = c.ClusterId,
            Match = new RouteMatch
            {
                Path = $"/{c.ClusterId}/{{**catch-all}}"
            },
            Transforms = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "PathRemovePrefix", $"/{c.ClusterId}" }
                    }
                }
        }).ToList();

        return new ConsulProxyConfig(routes, clusters);
    }
}
