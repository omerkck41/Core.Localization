using System.Globalization;
using Microsoft.Extensions.Localization;
using Core.Localization.Abstractions;

namespace Core.Localization.Services;

/// <summary>
/// Implementation of IStringLocalizer that uses ILocalizationService
/// </summary>
public class CoreStringLocalizer : IStringLocalizer
{
    private readonly ILocalizationService _localizationService;
    private readonly CultureInfo? _culture;

    public CoreStringLocalizer(ILocalizationService localizationService, CultureInfo? culture = null)
    {
        _localizationService = localizationService;
        _culture = culture;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = _localizationService.GetString(name, _culture);
            return new LocalizedString(name, value, value == name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = _localizationService.GetString(name, _culture ?? CultureInfo.CurrentCulture, arguments);
            return new LocalizedString(name, value, value == name);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = _culture ?? CultureInfo.CurrentCulture;
        return _localizationService.GetAllKeys(culture)
            .Select(key => {
                var value = _localizationService.GetString(key, culture);
                return new LocalizedString(key, value, false);
            });
    }
}
