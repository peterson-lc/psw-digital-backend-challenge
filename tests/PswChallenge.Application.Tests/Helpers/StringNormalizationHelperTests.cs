using FluentAssertions;
using PswChallenge.Application.Helpers;

namespace PswChallenge.Application.Tests.Helpers;

public class StringNormalizationHelperTests
{
    [Theory]
    [InlineData("Páscoa", "pascoa")]
    [InlineData("Confraternização", "confraternizacao")]
    [InlineData("Natal", "natal")]
    [InlineData("NATAL", "natal")]
    [InlineData("São João", "sao joao")]
    [InlineData("Tiradentes", "tiradentes")]
    [InlineData("Independência do Brasil", "independencia do brasil")]
    [InlineData("Proclamação da República", "proclamacao da republica")]
    [InlineData("Consciência Negra", "consciencia negra")]
    [InlineData("Aniversário da Cidade", "aniversario da cidade")]
    public void RemoveDiacritics_WithAccentedText_ReturnsNormalizedText(string input, string expected)
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("á", "a")]
    [InlineData("é", "e")]
    [InlineData("í", "i")]
    [InlineData("ó", "o")]
    [InlineData("ú", "u")]
    [InlineData("à", "a")]
    [InlineData("è", "e")]
    [InlineData("ì", "i")]
    [InlineData("ò", "o")]
    [InlineData("ù", "u")]
    [InlineData("â", "a")]
    [InlineData("ê", "e")]
    [InlineData("î", "i")]
    [InlineData("ô", "o")]
    [InlineData("û", "u")]
    [InlineData("ã", "a")]
    [InlineData("õ", "o")]
    [InlineData("ç", "c")]
    [InlineData("ñ", "n")]
    public void RemoveDiacritics_WithSingleAccentedCharacter_ReturnsBaseCharacter(string input, string expected)
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("ÁÉÍÓÚ", "aeiou")]
    [InlineData("ÀÈÌÒÙ", "aeiou")]
    [InlineData("ÂÊÎÔÛ", "aeiou")]
    [InlineData("ÃÕÑÇ", "aonc")]
    public void RemoveDiacritics_WithUppercaseAccents_ReturnsLowercaseWithoutAccents(string input, string expected)
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("   ", "   ")]
    public void RemoveDiacritics_WithEmptyOrWhitespace_ReturnsOriginal(string input, string expected)
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void RemoveDiacritics_WithNull_ReturnsNull()
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(null!);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("hello world", "hello world")]
    [InlineData("123456", "123456")]
    [InlineData("test@example.com", "test@example.com")]
    public void RemoveDiacritics_WithNoAccents_ReturnsLowercase(string input, string expected)
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Dia de São João", "dia de sao joao")]
    [InlineData("Véspera de Natal", "vespera de natal")]
    [InlineData("Paixão de Cristo", "paixao de cristo")]
    public void RemoveDiacritics_WithComplexBrazilianHolidayNames_ReturnsNormalized(string input, string expected)
    {
        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void RemoveDiacritics_WithMixedContent_PreservesSpacesAndPunctuation()
    {
        // Arrange
        var input = "São Paulo - Aniversário da Cidade!";
        var expected = "sao paulo - aniversario da cidade!";

        // Act
        var result = StringNormalizationHelper.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }
}

