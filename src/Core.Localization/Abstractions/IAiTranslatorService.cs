using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Localization.Abstractions;

/// <summary>
/// Service for translating resources using AI
/// </summary>
public interface IAiTranslatorService
{
    /// <summary>
    /// Translates a dictionary of resources from source culture to target culture
    /// </summary>
    /// <param name="resources">The resources to translate (key-value pairs)</param>
    /// <param name="sourceCulture">Source culture</param>
    /// <param name="targetCulture">Target culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Translated resources</returns>
    Task<IDictionary<string, string>> TranslateAsync(
        IDictionary<string, string> resources, 
        CultureInfo sourceCulture, 
        CultureInfo targetCulture, 
        CancellationToken cancellationToken = default);
}
