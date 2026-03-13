using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Core.Localization.Providers;
using Core.Localization.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Core.Localization.Extensions;

/// <summary>
/// Extension methods for registering localization services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core.Localization services to the service collection
    /// </summary>
    public static IServiceCollection AddCoreLocalization(
        this IServiceCollection services,
        Action<Core.Localization.Configuration.LocalizationOptions>? configureOptions = null)
    {
        // Register options with validation
        services.AddOptions<Core.Localization.Configuration.LocalizationOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("Localization").Bind(options);
                configureOptions?.Invoke(options);
            })
            .PostConfigure(options =>
            {
                var validator = new LocalizationOptionsValidator();
                var result = validator.Validate(options);
                if (!result.IsValid)
                {
                    throw new OptionsValidationException(
                        "Localization",
                        typeof(Core.Localization.Configuration.LocalizationOptions),
                        result.Errors.Select(e => e.ErrorMessage));
                }
            });

        // Register core services
        services.AddMemoryCache();
        services.TryAddSingleton<IFormatterService, FormatterService>();

        // Register default providers
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IResourceProvider, ResxResourceProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IResourceProvider, JsonResourceProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IResourceProvider, YamlResourceProvider>());

        // Register main service
        services.TryAddSingleton<ILocalizationService, LocalizationService>();

        // Register .NET Standard Localization services
        services.TryAddSingleton<IStringLocalizerFactory, CoreStringLocalizerFactory>();
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        services.TryAddTransient(typeof(IStringLocalizer), sp => 
            sp.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizationService)));

        return services;
    }

    /// <summary>
    /// Adds a custom resource provider
    /// </summary>
    public static IServiceCollection AddResourceProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IResourceProvider
    {
        services.AddSingleton<IResourceProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Replaces all providers with JSON resource provider
    /// </summary>
    public static IServiceCollection UseJsonResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, JsonResourceProvider>();
        return services;
    }

    /// <summary>
    /// Replaces all providers with YAML resource provider
    /// </summary>
    public static IServiceCollection UseYamlResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, YamlResourceProvider>();
        return services;
    }

    /// <summary>
    /// Replaces all providers with RESX resource provider
    /// </summary>
    public static IServiceCollection UseResxResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, ResxResourceProvider>();
        return services;
    }

    /// <summary>
    /// Adds in-memory resource provider for testing
    /// </summary>
    public static IServiceCollection AddInMemoryResourceProvider(
        this IServiceCollection services,
        IDictionary<string, IDictionary<string, string>> resources)
    {
        services.AddSingleton<IResourceProvider>(
            new InMemoryResourceProvider(resources));
        return services;
    }
}
