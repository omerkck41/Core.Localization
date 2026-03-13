using System.Globalization;
using Core.Localization.Abstractions;

namespace Core.Localization.Providers;

/// <summary>
/// Base class for resource providers
/// </summary>
public abstract class ResourceProviderBase : IResourceProvider
{
    protected ResourceProviderBase(int priority = 100)
    {
        Priority = priority;
    }

    public virtual int Priority { get; }

    public virtual bool SupportsDynamicReload => false;

    public abstract string? GetString(string key, CultureInfo culture);

    public virtual object? GetResource(string key, CultureInfo culture)
    {
        return GetString(key, culture);
    }

    public abstract IEnumerable<string> GetAllKeys(CultureInfo culture);

    public virtual bool HasKey(string key, CultureInfo culture)
    {
        return GetString(key, culture) != null;
    }

    public virtual Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        if (!SupportsDynamicReload)
        {
            throw new NotSupportedException($"Provider {GetType().Name} does not support dynamic reloading.");
        }

        return Task.CompletedTask;
    }

    protected virtual CultureInfo? GetParentCulture(CultureInfo culture)
    {
        if (culture.Parent == CultureInfo.InvariantCulture)
        {
            return null;
        }

        return culture.Parent;
    }
}
