using System.Globalization;

namespace Core.Localization.Extensions;

/// <summary>
/// Extension methods for CultureInfo
/// </summary>
public static class CultureInfoExtensions
{
    /// <summary>
    /// Gets the two-letter ISO language code
    /// </summary>
    public static string GetLanguageCode(this CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName;
    }

    /// <summary>
    /// Gets the full culture name (e.g., en-US)
    /// </summary>
    public static string GetFullName(this CultureInfo culture)
    {
        return culture.Name;
    }

    /// <summary>
    /// Checks if the culture is a neutral culture (e.g., en)
    /// </summary>
    public static bool IsNeutralCulture(this CultureInfo culture)
    {
        return culture.IsNeutralCulture;
    }

    /// <summary>
    /// Gets the parent culture or null if it's the invariant culture
    /// </summary>
    public static CultureInfo? GetParentCulture(this CultureInfo culture)
    {
        if (culture.Parent == CultureInfo.InvariantCulture)
        {
            return null;
        }
        return culture.Parent;
    }

    /// <summary>
    /// Gets all parent cultures up to the invariant culture
    /// </summary>
    public static IEnumerable<CultureInfo> GetParentCultures(this CultureInfo culture)
    {
        var current = culture.Parent;
        while (current != CultureInfo.InvariantCulture && !string.IsNullOrEmpty(current.Name))
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Checks if two cultures are the same or if one is a parent of the other
    /// </summary>
    public static bool IsRelatedTo(this CultureInfo culture, CultureInfo otherCulture)
    {
        if (culture.Equals(otherCulture))
        {
            return true;
        }

        var current = culture.Parent;
        while (current != CultureInfo.InvariantCulture && !string.IsNullOrEmpty(current.Name))
        {
            if (current.Equals(otherCulture))
            {
                return true;
            }
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Gets the display name in the culture's own language
    /// </summary>
    public static string GetNativeDisplayName(this CultureInfo culture)
    {
        return culture.NativeName;
    }

    /// <summary>
    /// Gets the display name in English
    /// </summary>
    public static string GetEnglishDisplayName(this CultureInfo culture)
    {
        return culture.EnglishName;
    }

    /// <summary>
    /// Checks if the culture is right-to-left
    /// </summary>
    public static bool IsRightToLeft(this CultureInfo culture)
    {
        return culture.TextInfo.IsRightToLeft;
    }
}
