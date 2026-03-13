using Core.Localization.Abstractions;
using Microsoft.Extensions.Localization;

namespace Core.Localization.Services;

/// <summary>
/// Factory for creating CoreStringLocalizer instances
/// </summary>
public class CoreStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly ILocalizationService _localizationService;

    public CoreStringLocalizerFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return new CoreStringLocalizer(_localizationService);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new CoreStringLocalizer(_localizationService);
    }
}
