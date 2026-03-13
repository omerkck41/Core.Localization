using System.Globalization;

namespace Core.Localization.Abstractions;

/// <summary>
/// Main interface for localization operations
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string value
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <returns>Localized string</returns>
    string GetString(string key, CultureInfo? culture = null);

    /// <summary>
    /// Gets a localized string with format arguments
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Gets a localized string with format arguments and specific culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">Target culture</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, CultureInfo culture, params object[] args);

    /// <summary>
    /// Gets a localized string with named arguments
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Dictionary of argument names and values</param>
    /// <param name="culture">Optional culture</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, IDictionary<string, object> args, CultureInfo? culture = null);

    /// <summary>
    /// Gets a localized string with named arguments from an object's properties
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Object with properties for formatting</param>
    /// <param name="culture">Optional culture</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, object args, CultureInfo? culture = null);

    /// <summary>
    /// Gets a localized string with pluralization
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="count">The count for pluralization</param>
    /// <param name="args">Optional additional arguments</param>
    /// <param name="culture">Optional culture</param>
    /// <returns>Formatted localized string</returns>
    string GetPluralString(string key, double count, IDictionary<string, object>? args = null, CultureInfo? culture = null);

    /// <summary>
    /// Tries to get a localized string
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="value">Output parameter for the localized string</param>
    /// <param name="culture">Optional culture</param>
    /// <returns>True if found, false otherwise</returns>
    bool TryGetString(string key, out string? value, CultureInfo? culture = null);

    /// <summary>
    /// Gets all localized strings for a specific key across cultures
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <returns>Dictionary of culture to localized string</returns>
    IDictionary<CultureInfo, string> GetAllStrings(string key);

    /// <summary>
    /// Gets all available keys in the current culture
    /// </summary>
    /// <returns>Collection of resource keys</returns>
    IEnumerable<string> GetAllKeys();

    /// <summary>
    /// Gets all available keys for a specific culture
    /// </summary>
    /// <param name="culture">Target culture</param>
    /// <returns>Collection of resource keys</returns>
    IEnumerable<string> GetAllKeys(CultureInfo culture);

    /// <summary>
    /// Gets all supported cultures
    /// </summary>
    /// <returns>Collection of supported cultures</returns>
    IEnumerable<CultureInfo> GetSupportedCultures();
}
