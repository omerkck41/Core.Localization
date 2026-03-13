using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from .resx files
/// </summary>
public class ResxResourceProvider : ResourceProviderBase
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<ResxResourceProvider> _logger;
    private readonly ConcurrentDictionary<string, ResourceManager> _resourceManagers;

    public ResxResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<ResxResourceProvider> logger,
        int priority = 100) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _resourceManagers = new ConcurrentDictionary<string, ResourceManager>();
        
        InitializeResourceManagers();
    }

    public override string? GetString(string key, CultureInfo culture)
    {
        var baseName = GetBaseName(key);
        var resourceKey = GetResourceKey(key);

        if (_resourceManagers.TryGetValue(baseName, out var resourceManager))
        {
            try
            {
                return resourceManager.GetString(resourceKey, culture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource {Key} for culture {Culture}", key, culture.Name);
            }
        }

        return null;
    }

    public override object? GetResource(string key, CultureInfo culture)
    {
        var baseName = GetBaseName(key);
        var resourceKey = GetResourceKey(key);

        if (_resourceManagers.TryGetValue(baseName, out var resourceManager))
        {
            try
            {
                return resourceManager.GetObject(resourceKey, culture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource object {Key} for culture {Culture}", key, culture.Name);
            }
        }

        return null;
    }

    public override IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        var keys = new List<string>();

        foreach (var kvp in _resourceManagers)
        {
            try
            {
                var resourceSet = kvp.Value.GetResourceSet(culture, true, false);
                if (resourceSet != null)
                {
                    foreach (System.Collections.DictionaryEntry entry in resourceSet)
                    {
                        var key = entry.Key.ToString();
                        if (key != null)
                        {
                            keys.Add($"{kvp.Key}.{key}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource keys for culture {Culture}", culture.Name);
            }
        }

        return keys;
    }

    private void InitializeResourceManagers()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var resourceNames = assembly.GetManifestResourceNames()
                    .Where(name => name.EndsWith(".resources"))
                    .Select(name => name.Replace(".resources", ""));

                foreach (var resourceName in resourceNames)
                {
                    var resourceManager = new ResourceManager(resourceName, assembly);
                    _resourceManagers.TryAdd(resourceName, resourceManager);
                    _logger.LogInformation("Loaded resource manager for {ResourceName}", resourceName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resources from assembly {Assembly}", assembly.FullName);
            }
        }
    }

    private string GetBaseName(string key)
    {
        var lastDotIndex = key.LastIndexOf('.');
        return lastDotIndex > 0 ? key.Substring(0, lastDotIndex) : "Resources";
    }

    private string GetResourceKey(string key)
    {
        var lastDotIndex = key.LastIndexOf('.');
        return lastDotIndex > 0 ? key.Substring(lastDotIndex + 1) : key;
    }
}
