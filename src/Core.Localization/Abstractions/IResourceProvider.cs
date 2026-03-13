using System.Globalization;

namespace Core.Localization.Abstractions;

/// <summary>
/// Defines the contract for resource providers that supply localization data
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Gets a localized string value for the specified key and culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The target culture</param>
    /// <returns>Localized string value or null if not found</returns>
    string? GetString(string key, CultureInfo culture);

    /// <summary>
    /// Gets a localized object value for the specified key and culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The target culture</param>
    /// <returns>Localized object value or null if not found</returns>
    object? GetResource(string key, CultureInfo culture);

    /// <summary>
    /// Gets all available keys for a given culture
    /// </summary>
    /// <param name="culture">The target culture</param>
    /// <returns>Collection of available keys</returns>
    IEnumerable<string> GetAllKeys(CultureInfo culture);

    /// <summary>
    /// Checks if the provider has a specific key for a given culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The target culture</param>
    /// <returns>True if the key exists, false otherwise</returns>
    bool HasKey(string key, CultureInfo culture);

    /// <summary>
    /// Gets the provider's priority for conflict resolution
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets whether the provider supports dynamic reloading
    /// </summary>
    bool SupportsDynamicReload { get; }

    /// <summary>
    /// Reloads the resource provider's data
    /// </summary>
    Task ReloadAsync(CancellationToken cancellationToken = default);
}
