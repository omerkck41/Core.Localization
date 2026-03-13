using System.Globalization;
using Core.Localization.Abstractions;
using Core.Localization.Services;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Core.Localization.Tests;

public class CoreStringLocalizerTests
{
    private readonly Mock<ILocalizationService> _mockLocalizationService;

    public CoreStringLocalizerTests()
    {
        _mockLocalizationService = new Mock<ILocalizationService>();
    }

    [Fact]
    public void Indexer_ShouldReturnLocalizedString_FromService()
    {
        // Arrange
        var key = "Welcome";
        var value = "Welcome Home";
        _mockLocalizationService.Setup(s => s.GetString(key, It.IsAny<CultureInfo>())).Returns(value);

        var localizer = new CoreStringLocalizer(_mockLocalizationService.Object);

        // Act
        var result = localizer[key];

        // Assert
        result.Value.Should().Be(value);
        result.Name.Should().Be(key);
        result.ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void Indexer_WithArgs_ShouldReturnFormattedString()
    {
        // Arrange
        var key = "HelloUser";
        var name = "John";
        var expected = "Hello John";
        
        _mockLocalizationService.Setup(s => s.GetString(key, It.IsAny<CultureInfo>(), It.IsAny<object[]>()))
            .Returns(expected);

        var localizer = new CoreStringLocalizer(_mockLocalizationService.Object);

        // Act
        var result = localizer[key, name];

        // Assert
        result.Value.Should().Be(expected);
        _mockLocalizationService.Verify(s => s.GetString(key, It.IsAny<CultureInfo>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void GetAllStrings_ShouldReturnAllResources()
    {
        // Arrange
        var keys = new List<string> { "Key1", "Key2" };
        _mockLocalizationService.Setup(s => s.GetAllKeys(It.IsAny<CultureInfo>())).Returns(keys);
        _mockLocalizationService.Setup(s => s.GetString("Key1", It.IsAny<CultureInfo>())).Returns("Value1");
        _mockLocalizationService.Setup(s => s.GetString("Key2", It.IsAny<CultureInfo>())).Returns("Value2");

        var localizer = new CoreStringLocalizer(_mockLocalizationService.Object);

        // Act
        var result = localizer.GetAllStrings(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Key1" && x.Value == "Value1");
        result.Should().Contain(x => x.Name == "Key2" && x.Value == "Value2");
    }
}
