using FluentValidation;

namespace Core.Localization.Configuration;

/// <summary>
/// Validator for LocalizationOptions
/// </summary>
public class LocalizationOptionsValidator : AbstractValidator<LocalizationOptions>
{
    public LocalizationOptionsValidator()
    {
        RuleFor(x => x.DefaultCulture)
            .NotNull()
            .WithMessage("Default culture must be specified.");

        RuleFor(x => x.FallbackCulture)
            .NotNull()
            .WithMessage("Fallback culture must be specified.");

        RuleFor(x => x.SupportedCultures)
            .NotEmpty()
            .WithMessage("At least one supported culture must be provided.");

        RuleForEach(x => x.ResourcePaths)
            .NotEmpty()
            .WithMessage("Resource path cannot be empty.");

        RuleFor(x => x.CacheExpiration)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("Cache expiration must be greater than zero.");
    }
}
