using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Core.Localization.Abstractions;

namespace Core.Localization.Services;

/// <summary>
/// Implementation of formatting services for various data types
/// </summary>
public class FormatterService : IFormatterService
{
    private static readonly Regex NamedParamRegex = new Regex(@"\{(?<name>[^}]+)\}", RegexOptions.Compiled);

    public string Format(string template, IDictionary<string, object> args, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(template) || args == null) return template;
        
        culture ??= CultureInfo.CurrentCulture;

        return NamedParamRegex.Replace(template, match =>
        {
            var key = match.Groups["name"].Value;
            if (args.TryGetValue(key, out var value))
            {
                if (value is IFormattable formattable)
                {
                    return formattable.ToString(null, culture);
                }
                return value?.ToString() ?? string.Empty;
            }
            return match.Value;
        });
    }

    public string Format(string template, object args, CultureInfo? culture = null)
    {
        if (template == null || args == null) return template ?? string.Empty;

        var dict = args.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p.GetValue(args) ?? string.Empty);

        return Format(template, dict, culture);
    }

    private static readonly Regex PluralRegex = new Regex(@"\{(?<key>[^,]+),\s*plural,\s*(?<rules>(?:[^{}]*\{[^{}]*\})*)\s*\}", RegexOptions.Compiled);
    private static readonly Regex RuleRegex = new Regex(@"(?<category>\w+)\s*\{(?<text>[^{}]*)\}", RegexOptions.Compiled);

    public string FormatPlural(string template, double count, IDictionary<string, object>? args = null, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(template)) return template;
        
        culture ??= CultureInfo.CurrentCulture;
        args ??= new Dictionary<string, object>();
        args["count"] = count;

        return PluralRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value.Trim();
            if (key != "count" && !args.ContainsKey(key)) return match.Value;

            var val = key == "count" ? count : Convert.ToDouble(args[key]);
            var rulesText = match.Groups["rules"].Value;
            var rules = RuleRegex.Matches(rulesText)
                .ToDictionary(m => m.Groups["category"].Value, m => m.Groups["text"].Value);

            var category = GetPluralCategory(val, culture);
            
            if (!rules.TryGetValue(category, out var result))
            {
                if (!rules.TryGetValue("other", out result))
                {
                    return match.Value;
                }
            }

            result = result.Replace("#", val.ToString(culture));
            return Format(result, args, culture);
        });
    }

    private string GetPluralCategory(double count, CultureInfo culture)
    {
        var language = culture.TwoLetterISOLanguageName;

        switch (language)
        {
            case "tr":
                return count == 1 ? "one" : "other"; // Basic TR rule
            case "en":
                return count == 1 ? "one" : "other"; // Basic EN rule
            default:
                return count == 1 ? "one" : "other";
        }
    }

    public string FormatDate(DateTime date, string? format = null, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        return date.ToString(format ?? culture.DateTimeFormat.ShortDatePattern, culture);
    }

    public string FormatNumber(decimal number, string? format = null, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        return number.ToString(format ?? "N2", culture);
    }

    public string FormatCurrency(decimal amount, string? currencyCode = null, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        
        if (currencyCode != null)
        {
            // Create a custom NumberFormatInfo with the specified currency
            var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberFormat.CurrencySymbol = currencyCode;
            return amount.ToString("C", numberFormat);
        }
        
        return amount.ToString("C", culture);
    }

    public string FormatPercentage(decimal value, int decimals = 2, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var format = $"P{decimals}";
        return value.ToString(format, culture);
    }

    public DateTime? ParseDate(string dateString, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        
        if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        // Try parsing with various standard formats
        string[] formats = { 
            culture.DateTimeFormat.ShortDatePattern,
            culture.DateTimeFormat.LongDatePattern,
            culture.DateTimeFormat.ShortTimePattern,
            culture.DateTimeFormat.LongTimePattern,
            culture.DateTimeFormat.FullDateTimePattern,
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ"
        };

        if (DateTime.TryParseExact(dateString, formats, culture, DateTimeStyles.None, out result))
        {
            return result;
        }

        return null;
    }

    public decimal? ParseNumber(string numberString, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        
        if (decimal.TryParse(numberString, NumberStyles.Number, culture, out var result))
        {
            return result;
        }

        return null;
    }

    public decimal? ParseCurrency(string currencyString, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        
        // Remove any currency symbols and spaces
        var cleanedString = currencyString.Trim();
        foreach (var symbol in new[] { culture.NumberFormat.CurrencySymbol, "$", "€", "£", "¥", "₺" })
        {
            cleanedString = cleanedString.Replace(symbol, "");
        }
        
        cleanedString = cleanedString.Trim();
        
        if (decimal.TryParse(cleanedString, NumberStyles.Currency, culture, out var result))
        {
            return result;
        }

        return null;
    }
}
