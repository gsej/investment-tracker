using Microsoft.Extensions.Configuration;

namespace Common;

public static class ConfigurationExtensions
{
    public static string GetString(this IConfiguration configuration, string keyName)
    {
        var configurationValue = configuration[keyName];

        if (String.IsNullOrWhiteSpace(configurationValue))
        {
            throw new InvalidOperationException($"configuration value {keyName} is missing");
        }

        return configurationValue;
    }
}