using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Quartz;

namespace VRT.Backups.Infrastructure.Options;
internal sealed class QuartzOptionsSetup : IConfigureOptions<QuartzOptions>
{
    private const string ConfigurationSectionName = "Quartz";
    private readonly IConfiguration _configuration;

    public QuartzOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void Configure(QuartzOptions options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
