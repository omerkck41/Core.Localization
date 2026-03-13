using System.Globalization;
using System.Text.Json;
using Core.Localization.Configuration;
using Core.Localization.Providers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Core.Localization.Tests;

public class JsonResourceProviderTests : IDisposable
{
    private readonly string _tempPath;
    private readonly Mock<ILogger<JsonResourceProvider>> _mockLogger;
    private readonly LocalizationOptions _options;

    public JsonResourceProviderTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
        
        _mockLogger = new Mock<ILogger<JsonResourceProvider>>();
        _options = new LocalizationOptions
        {
            DefaultCulture = new CultureInfo("en-US"),
            SupportedCultures = new List<CultureInfo> 
            { 
                new CultureInfo("en-US"), 
                new CultureInfo("tr-TR") 
            },
            ResourcePaths = new List<string> { _tempPath },
            EnableResourceFileWatching = false
        };
    }

    [Fact]
    public void GetString_ShouldReturnLocalizedValue_FromFile()
    {
        // Arrange
        var culture = new CultureInfo("en-US");
        var resources = new Dictionary<string, string> { { "Welcome", "Welcome to our store" } };
        var json = JsonSerializer.Serialize(resources);
        File.WriteAllText(Path.Combine(_tempPath, "resources.en-US.json"), json);

        var provider = CreateProvider();

        // Act
        var result = provider.GetString("Welcome", culture);

        // Assert
        result.Should().Be("Welcome to our store");
    }

    [Fact]
    public void GetAllKeys_ShouldReturnAllKeys_FromFile()
    {
        // Arrange
        var culture = new CultureInfo("en-US");
        var resources = new Dictionary<string, string> 
        { 
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        };
        var json = JsonSerializer.Serialize(resources);
        File.WriteAllText(Path.Combine(_tempPath, "resources.en-US.json"), json);

        var provider = CreateProvider();

        // Act
        var result = provider.GetAllKeys(culture);

        // Assert
        result.Should().HaveCount(2).And.Contain(new[] { "Key1", "Key2" });
    }

    private JsonResourceProvider CreateProvider()
    {
        var optionsMock = new Mock<IOptions<LocalizationOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);
        
        return new JsonResourceProvider(optionsMock.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }
}
