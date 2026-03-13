using System.Globalization;
using Core.Localization.Abstractions;

namespace Core.Localization.Extensions;

/// <summary>
/// Extension methods for string localization
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets a localized string using the specified key
    /// </summary>
    public static string Localize(this string key, ILocalizationService localizationService)
    {
        return localizationService.GetString(key);
    }

    /// <summary>
    /// Gets a localized string using the specified key and culture
    /// </summary>
    public static string Localize(this string key, ILocalizationService localizationService, CultureInfo culture)
    {
        return localizationService.GetString(key, culture);
    }

    /// <summary>
    /// Gets a localized string with format arguments
    /// </summary>
    public static string Localize(this string key, ILocalizationService localizationService, params object[] args)
    {
        return localizationService.GetString(key, args);
    }

    /// <summary>
    /// Gets a localized string with format arguments and specific culture
    /// </summary>
    public static string Localize(this string key, ILocalizationService localizationService, CultureInfo culture, params object[] args)
    {
        return localizationService.GetString(key, culture, args);
    }

    /// <summary>
    /// Tries to get a localized string
    /// </summary>
    public static bool TryLocalize(this string key, ILocalizationService localizationService, out string? value)
    {
        return localizationService.TryGetString(key, out value);
    }

    /// <summary>
    /// Tries to get a localized string with specific culture
    /// </summary>
    public static bool TryLocalize(this string key, ILocalizationService localizationService, CultureInfo culture, out string? value)
    {
        return localizationService.TryGetString(key, out value, culture);
    }
}
