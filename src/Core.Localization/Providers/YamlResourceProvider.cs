using System.Collections.Concurrent;
using System.Globalization;
using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from YAML files
/// </summary>
public class YamlResourceProvider : ResourceProviderBase
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<YamlResourceProvider> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _resourceCache;
    private FileSystemWatcher? _fileWatcher;
    private readonly IDeserializer _yamlDeserializer;

    public YamlResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<YamlResourceProvider> logger,
        int priority = 150) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _resourceCache = new ConcurrentDictionary<string, Dictionary<string, object>>();
        
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        if (_options.EnableResourceFileWatching)
        {
            InitializeFileWatcher();
        }

        LoadResources();
    }

    public override bool SupportsDynamicReload => true;

    public override string? GetString(string key, CultureInfo culture)
    {
        var cultureKey = GetResourceFileName(culture);
        
        if (_resourceCache.TryGetValue(cultureKey, out var resources))
        {
            if (TryGetNestedValue(resources, key, out var value) && value != null)
            {
                return value.ToString();
            }
        }

        // Try parent culture
        var parentCulture = GetParentCulture(culture);
        if (parentCulture != null)
        {
            return GetString(key, parentCulture);
        }

        return null;
    }

    public override object? GetResource(string key, CultureInfo culture)
    {
        var cultureKey = GetResourceFileName(culture);
        
        if (_resourceCache.TryGetValue(cultureKey, out var resources))
        {
            if (TryGetNestedValue(resources, key, out var value))
            {
                return value;
            }
        }

        // Try parent culture
        var parentCulture = GetParentCulture(culture);
        if (parentCulture != null)
        {
            return GetResource(key, parentCulture);
        }

        return null;
    }

    public override IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        var cultureKey = GetResourceFileName(culture);
        
        if (_resourceCache.TryGetValue(cultureKey, out var resources))
        {
            return GetAllKeysFromDictionary(resources);
        }

        return Enumerable.Empty<string>();
    }

    public override async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => LoadResources(), cancellationToken);
    }

    private void LoadResources()
    {
        foreach (var culture in _options.SupportedCultures)
        {
            foreach (var resourcePath in _options.ResourcePaths)
            {
                var fileName = GetResourceFileName(culture);
                var filePath = Path.Combine(resourcePath, $"{fileName}.yaml");
                var altFilePath = Path.Combine(resourcePath, $"{fileName}.yml");

                var actualPath = File.Exists(filePath) ? filePath 
                    : File.Exists(altFilePath) ? altFilePath 
                    : null;

                if (actualPath != null)
                {
                    try
                    {
                        var yamlContent = File.ReadAllText(actualPath);
                        var resources = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
                        
                        if (resources != null)
                        {
                            _resourceCache[fileName] = resources;
                            _logger.LogInformation("Loaded YAML resources from {FilePath}", actualPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load YAML resources from {FilePath}", actualPath);
                    }
                }
            }
        }
    }

    private bool TryGetNestedValue(Dictionary<string, object> resources, string key, out object? value)
    {
        value = null;
        var keys = key.Split('.');
        object? current = resources;

        foreach (var k in keys)
        {
            if (current is Dictionary<string, object> dict && dict.TryGetValue(k, out var nextValue))
            {
                current = nextValue;
            }
            else
            {
                return false;
            }
        }

        value = current;
        return true;
    }

    private IEnumerable<string> GetAllKeysFromDictionary(Dictionary<string, object> resources, string prefix = "")
    {
        var keys = new List<string>();

        foreach (var kvp in resources)
        {
            var fullKey = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                keys.AddRange(GetAllKeysFromDictionary(nestedDict, fullKey));
            }
            else
            {
                keys.Add(fullKey);
            }
        }

        return keys;
    }

    private void InitializeFileWatcher()
    {
        foreach (var resourcePath in _options.ResourcePaths)
        {
            if (Directory.Exists(resourcePath))
            {
                _fileWatcher = new FileSystemWatcher(resourcePath)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                    Filter = "*.yaml"
                };

                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Created += OnFileChanged;
                _fileWatcher.Deleted += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;

                // Also watch for .yml files
                var ymlWatcher = new FileSystemWatcher(resourcePath, "*.yml")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
                };

                ymlWatcher.Changed += OnFileChanged;
                ymlWatcher.Created += OnFileChanged;
                ymlWatcher.Deleted += OnFileChanged;
                ymlWatcher.EnableRaisingEvents = true;
            }
        }
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Resource file changed: {FilePath}", e.FullPath);
        await ReloadAsync();
    }

    private string GetResourceFileName(CultureInfo culture)
    {
        return _options.ResourceNameGenerator?.Invoke("resources", culture) 
               ?? $"resources.{culture.Name}";
    }

    public void Dispose()
    {
        _fileWatcher?.Dispose();
    }
}
