using System.Globalization;

namespace Core.Localization.Providers;

/// <summary>
/// In-memory resource provider for testing and simple scenarios
/// </summary>
public class InMemoryResourceProvider : ResourceProviderBase
{
    private readonly IDictionary<string, IDictionary<string, string>> _resources;

    public InMemoryResourceProvider(
        IDictionary<string, IDictionary<string, string>> resources,
        int priority = 300) : base(priority)
    {
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
    }

    public override string? GetString(string key, CultureInfo culture)
    {
        var cultureName = culture.Name;
        
        if (_resources.TryGetValue(cultureName, out var cultureResources) &&
            cultureResources.TryGetValue(key, out var value))
        {
            return value;
        }

        // Try neutral culture
        var neutralCulture = culture.Parent;
        if (neutralCulture != CultureInfo.InvariantCulture && 
            _resources.TryGetValue(neutralCulture.Name, out cultureResources) &&
            cultureResources.TryGetValue(key, out value))
        {
            return value;
        }

        // Try invariant culture
        if (_resources.TryGetValue("", out cultureResources) &&
            cultureResources.TryGetValue(key, out value))
        {
            return value;
        }

        return null;
    }

    public override IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        var cultureName = culture.Name;
        
        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            return cultureResources.Keys;
        }

        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Adds or updates a resource for a specific culture
    /// </summary>
    public void AddOrUpdateResource(string cultureName, string key, string value)
    {
        if (!_resources.TryGetValue(cultureName, out var cultureResources))
        {
            cultureResources = new Dictionary<string, string>();
            _resources[cultureName] = cultureResources;
        }

        cultureResources[key] = value;
    }

    /// <summary>
    /// Removes a resource for a specific culture
    /// </summary>
    public bool RemoveResource(string cultureName, string key)
    {
        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            return cultureResources.Remove(key);
        }

        return false;
    }

    /// <summary>
    /// Clears all resources
    /// </summary>
    public void Clear()
    {
        _resources.Clear();
    }
}
