using System.Globalization;
using Core.Localization.Configuration;
using FluentAssertions;
using Xunit;

namespace Core.Localization.Tests;

public class LocalizationOptionsValidatorTests
{
    private readonly LocalizationOptionsValidator _validator;

    public LocalizationOptionsValidatorTests()
    {
        _validator = new LocalizationOptionsValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenOptionsAreCorrect()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            DefaultCulture = new CultureInfo("en-US"),
            FallbackCulture = new CultureInfo("en-US"),
            SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US") },
            ResourcePaths = new List<string> { "Resources" },
            CacheExpiration = TimeSpan.FromHours(1)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldBeInvalid_WhenDefaultCultureIsNull()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            DefaultCulture = null!,
            SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US") }
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LocalizationOptions.DefaultCulture));
    }

    [Fact]
    public void Validate_ShouldBeInvalid_WhenSupportedCulturesIsEmpty()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            DefaultCulture = new CultureInfo("en-US"),
            SupportedCultures = new List<CultureInfo>()
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LocalizationOptions.SupportedCultures));
    }
}
