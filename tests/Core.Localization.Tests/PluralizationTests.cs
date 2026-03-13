using System.Globalization;
using Core.Localization.Abstractions;
using Core.Localization.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Core.Localization.Tests;

public class PluralizationTests
{
    private readonly FormatterService _formatter;

    public PluralizationTests()
    {
        _formatter = new FormatterService();
    }

    [Theory]
    [InlineData(1, "one", "ru-RU")] // 1 kitap
    [InlineData(2, "few", "ru-RU")] // 2 kitap
    [InlineData(5, "many", "ru-RU")] // 5 kitap
    [InlineData(11, "many", "ru-RU")] // 11 kitap
    [InlineData(21, "one", "ru-RU")] // 21 kitap
    public void GetPluralCategory_Russian_ShouldReturnCorrectCategory(double count, string expectedCategory, string cultureName)
    {
        // Arrange
        var culture = new CultureInfo(cultureName);
        var template = "{count, plural, one{one} few{few} many{many} other{other}}";

        // Act
        var result = _formatter.FormatPlural(template, count, culture: culture);

        // Assert
        result.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData(0, "zero", "ar-SA")]
    [InlineData(1, "one", "ar-SA")]
    [InlineData(2, "two", "ar-SA")]
    [InlineData(3, "few", "ar-SA")]
    [InlineData(11, "many", "ar-SA")]
    [InlineData(100, "other", "ar-SA")]
    public void GetPluralCategory_Arabic_ShouldReturnCorrectCategory(double count, string expectedCategory, string cultureName)
    {
        // Arrange
        var culture = new CultureInfo(cultureName);
        var template = "{count, plural, zero{zero} one{one} two{two} few{few} many{many} other{other}}";

        // Act
        var result = _formatter.FormatPlural(template, count, culture: culture);

        // Assert
        result.Should().Be(expectedCategory);
    }
}
