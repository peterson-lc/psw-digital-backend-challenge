using System.Globalization;
using System.Text;

namespace PswChallenge.Application.Helpers;

/// <summary>
/// Helper class for normalizing strings by removing diacritical marks (accents) and converting to lowercase.
/// </summary>
public static class StringNormalizationHelper
{
    /// <summary>
    /// Normalizes a string by removing diacritical marks (accents) and converting to lowercase.
    /// This is useful for accent-insensitive and case-insensitive string comparisons.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text without accents and in lowercase.</returns>
    /// <example>
    /// "Confraternização" -> "confraternizacao"
    /// "Páscoa" -> "pascoa"
    /// "NATAL" -> "natal"
    /// </example>
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        // Normalize the string to FormD (canonical decomposition)
        // This separates characters from their diacritical marks
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        // Iterate through each character
        foreach (var c in normalizedString)
        {
            // Get the Unicode category of the character
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

            // Skip non-spacing marks (diacritical marks)
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Normalize back to FormC (canonical composition) and convert to lowercase
        return stringBuilder.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }
}

