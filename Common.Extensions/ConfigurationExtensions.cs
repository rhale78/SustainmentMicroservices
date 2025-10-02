using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static string GetValue(this IConfiguration configuration, string key)
    {
        return configuration[key];
    }
    public static string GetValue(this IConfiguration configuration, string key, string defaultValue)
    {
        string value = configuration[key];
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }
    public static bool GetBoolValueWithDefault(this IConfiguration configuration, string key, bool defaultValue)
    {
        string value = GetValue(configuration, key);
        return !bool.TryParse(value, out bool result) ? defaultValue : result;
    }
    public static int GetIntValueWithDefault(this IConfiguration configuration, string key, int defaultValue)
    {
        string value = GetValue(configuration, key);
        return !int.TryParse(value, out int result) ? defaultValue : result;
    }
    public static TimeSpan GetTimespanValue(this IConfiguration configuration, string key, TimeSpan defaultValue)
    {
        string value = GetValue(configuration, key);
        return !TimeSpan.TryParse(value, out TimeSpan result) ? defaultValue : result;
    }
}