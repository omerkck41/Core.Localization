using System.Globalization;
using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Core.Localization.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Core.Localization.Tests;

public class LocalizationServiceTests
{
    private readonly Mock<IResourceProvider> _mockProvider;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly LocalizationOptions _options;
    private readonly IMemoryCache _cache;

    public LocalizationServiceTests()
    {
        _mockProvider = new Mock<IResourceProvider>();
        _mockLogger = new Mock<ILogger<LocalizationService>>();
        _options = new LocalizationOptions
        {
            DefaultCulture = new CultureInfo("en-US"),
            FallbackCulture = new CultureInfo("en-US"),
            SupportedCultures = new List<CultureInfo> 
            { 
                new CultureInfo("en-US"), 
                new CultureInfo("tr-TR") 
            },
            EnableCaching = true,
            UseFallbackCulture = true
        };
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public void GetString_ShouldReturnLocalizedValue_WhenKeyExists()
    {
        // Arrange
        var key = "Greeting";
        var value = "Hello";
        var culture = new CultureInfo("en-US");
        
        _mockProvider.Setup(p => p.GetString(key, culture)).Returns(value);
        _mockProvider.Setup(p => p.Priority).Returns(1);

        var service = CreateService();

        // Act
        var result = service.GetString(key, culture);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void GetString_ShouldReturnFallbackValue_WhenKeyDoesNotExistInTargetCulture()
    {
        // Arrange
        var key = "Greeting";
        var fallbackValue = "Hello";
        var targetCulture = new CultureInfo("tr-TR");
        var fallbackCulture = new CultureInfo("en-US");
        
        _mockProvider.Setup(p => p.GetString(key, targetCulture)).Returns((string?)null);
        _mockProvider.Setup(p => p.GetString(key, fallbackCulture)).Returns(fallbackValue);
        _mockProvider.Setup(p => p.Priority).Returns(1);

        var service = CreateService();

        // Act
        var result = service.GetString(key, targetCulture);

        // Assert
        result.Should().Be(fallbackValue);
    }

    [Fact]
    public void GetString_ShouldReturnKey_WhenKeyDoesNotExistAnywhere()
    {
        // Arrange
        var key = "NonExistent";
        var culture = new CultureInfo("en-US");
        
        _mockProvider.Setup(p => p.GetString(It.IsAny<string>(), It.IsAny<CultureInfo>())).Returns((string?)null);
        _mockProvider.Setup(p => p.Priority).Returns(1);

        var service = CreateService();

        // Act
        var result = service.GetString(key, culture);

        // Assert
        result.Should().Be(key);
    }

    [Fact]
    public void GetString_ShouldUseCache_WhenEnabled()
    {
        // Arrange
        var key = "CachedKey";
        var value = "CachedValue";
        var culture = new CultureInfo("en-US");
        
        _mockProvider.Setup(p => p.GetString(key, culture)).Returns(value);
        _mockProvider.Setup(p => p.Priority).Returns(1);

        var service = CreateService();

        // Act
        var firstResult = service.GetString(key, culture);
        var secondResult = service.GetString(key, culture);

        // Assert
        firstResult.Should().Be(value);
        secondResult.Should().Be(value);
        _mockProvider.Verify(p => p.GetString(key, culture), Times.Once);
    }

    [Fact]
    public void GetString_ShouldReturnFormattedString_WithNamedArguments()
    {
        // Arrange
        var key = "WelcomeMessage";
        var template = "Hello {Name}, welcome to {Store}!";
        var culture = new CultureInfo("en-US");
        var args = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Store", "CoreShop" }
        };
        
        _mockProvider.Setup(p => p.GetString(key, culture)).Returns(template);
        _mockProvider.Setup(p => p.Priority).Returns(1);

        var service = CreateService();

        // Act
        var result = service.GetString(key, args, culture);

        // Assert
        result.Should().Be("Hello John, welcome to CoreShop!");
    }

    [Fact]
    public void GetPluralString_ShouldReturnCorrectForm_BasedOnCount()
    {
        // Arrange
        var key = "ItemCount";
        var template = "{count, plural, one{One item} other{# items}}";
        var culture = new CultureInfo("en-US");
        
        _mockProvider.Setup(p => p.GetString(key, culture)).Returns(template);
        _mockProvider.Setup(p => p.Priority).Returns(1);

        var service = CreateService();

        // Act
        var resultOne = service.GetPluralString(key, 1, culture: culture);
        var resultMany = service.GetPluralString(key, 5, culture: culture);

        // Assert
        resultOne.Should().Be("One item");
        resultMany.Should().Be("5 items");
    }

    private LocalizationService CreateService()
    {
        var optionsMock = new Mock<IOptions<LocalizationOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);

        return new LocalizationService(
            new[] { _mockProvider.Object },
            optionsMock.Object,
            _mockLogger.Object,
            new FormatterService(),
            _cache);
    }
}
