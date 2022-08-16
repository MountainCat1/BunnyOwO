using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Configuration;

/// <summary>
/// Configuration for connection to RabbitMQ server, to register use
/// <see cref="OptionsServiceCollectionExtensions.Configure{TOptions}(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{TOptions})"/>
/// </summary>
public class RabbitMQConfiguration
{
    [JsonPropertyName("userName")]
    public string UserName { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("virtualHost")]
    public string VirtualHost { get; set; }

    [JsonPropertyName("hostName")]
    public string HostName { get; set; }
    
    [JsonPropertyName("port")]
    public int Port { get; set; }
}