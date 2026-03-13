using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from JSON files
/// </summary>
public class JsonResourceProvider : ResourceProviderBase
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<JsonResourceProvider> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resourceCache;
    private FileSystemWatcher? _fileWatcher;

    public JsonResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<JsonResourceProvider> logger,
        int priority = 200) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _resourceCache = new ConcurrentDictionary<string, Dictionary<string, string>>();

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
        
        if (_resourceCache.TryGetValue(cultureKey, out var resources) && 
            resources.TryGetValue(key, out var value))
        {
            return value;
        }

        // Try parent culture
        var parentCulture = GetParentCulture(culture);
        if (parentCulture != null)
        {
            return GetString(key, parentCulture);
        }

        return null;
    }

    public override IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        var cultureKey = GetResourceFileName(culture);
        
        if (_resourceCache.TryGetValue(cultureKey, out var resources))
        {
            return resources.Keys;
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
                var filePath = Path.Combine(resourcePath, $"{fileName}.json");

                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(filePath);
                        var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        
                        if (resources != null)
                        {
                            _resourceCache[fileName] = resources;
                            _logger.LogInformation("Loaded JSON resources from {FilePath}", filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load JSON resources from {FilePath}", filePath);
                    }
                }
            }
        }
    }

    private void InitializeFileWatcher()
    {
        foreach (var resourcePath in _options.ResourcePaths)
        {
            if (Directory.Exists(resourcePath))
            {
                _fileWatcher = new FileSystemWatcher(resourcePath, "*.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
                };

                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Created += OnFileChanged;
                _fileWatcher.Deleted += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;
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
