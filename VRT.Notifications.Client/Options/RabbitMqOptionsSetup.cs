using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace VRT.Notifications.Client.Options;
internal class RabbitMqOptionsSetup : IConfigureOptions<RabbitMqOptions>
{
    private const string ConfigurationSectionName = "RabbitMq";
    private readonly IConfiguration _configuration;

    public RabbitMqOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void Configure(RabbitMqOptions options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
