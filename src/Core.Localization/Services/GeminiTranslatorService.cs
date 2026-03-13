using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Localization.Abstractions;
using Google.GenAI;
using Microsoft.Extensions.Logging;

namespace Core.Localization.Services;

/// <summary>
/// Implementation of IAiTranslatorService using Google Gemini AI
/// </summary>
public class GeminiTranslatorService : IAiTranslatorService
{
    private readonly string _apiKey;
    private readonly ILogger<GeminiTranslatorService> _logger;
    private const string ModelName = "gemini-1.5-flash";

    public GeminiTranslatorService(string apiKey, ILogger<GeminiTranslatorService> logger)
    {
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<IDictionary<string, string>> TranslateAsync(
        IDictionary<string, string> resources, 
        CultureInfo sourceCulture, 
        CultureInfo targetCulture, 
        CancellationToken cancellationToken = default)
    {
        if (resources == null || !resources.Any()) return new Dictionary<string, string>();

        try
        {
            var client = new Google.GenAI.Client(apiKey: _apiKey);
            
            var sourceJson = JsonSerializer.Serialize(resources, new JsonSerializerOptions { WriteIndented = true });
            
            var prompt = $@"
                You are a professional software translator. 
                Translate the following JSON localization resources from {sourceCulture.NativeName} ({sourceCulture.Name}) 
                to {targetCulture.NativeName} ({targetCulture.Name}).
                
                Rules:
                1. Keep the JSON keys exactly as they are.
                2. Translate only the values.
                3. Preserve all placeholders like {{count}}, {{Name}}, {{0}}, etc.
                4. Maintain pluralization formats if present (e.g., ICU format).
                5. Return ONLY the translated JSON object.
                
                Source JSON:
                {sourceJson}";

            var response = await client.Models.GenerateContentAsync(ModelName, prompt);
            
            // In Official SDK 1.0.0, text is nested in Candidates[0].Content.Parts[0].Text
            var resultText = response.Candidates?[0]?.Content?.Parts?[0]?.Text;

            if (string.IsNullOrEmpty(resultText))
            {
                _logger.LogWarning("Gemini returned an empty response.");
                return new Dictionary<string, string>();
            }

            // Extract JSON if AI wrapped it in markdown code blocks
            if (resultText.Contains("```json"))
            {
                resultText = resultText.Split("```json")[1].Split("```")[0].Trim();
            }
            else if (resultText.Contains("```"))
            {
                resultText = resultText.Split("```")[1].Split("```")[0].Trim();
            }

            var translatedResources = JsonSerializer.Deserialize<Dictionary<string, string>>(resultText);
            return translatedResources ?? new Dictionary<string, string>();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Gemini translation failed from {Source} to {Target}", sourceCulture.Name, targetCulture.Name);
            throw;
        }
    }
}
