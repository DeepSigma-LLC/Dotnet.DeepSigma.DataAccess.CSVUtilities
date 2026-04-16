using CsvHelper.Configuration;
using System.Globalization;

namespace DeepSigma.DataAccess.CSVUtilities.Configuration;

/// <summary>
/// Factory for creating pre-configured <see cref="CsvConfiguration"/> instances with sensible defaults.
/// </summary>
public static class CsvDefaults
{
    /// <summary>
    /// Creates a <see cref="CsvConfiguration"/> with forgiving header matching, empty-row skipping,
    /// and suppressed missing-field/bad-data exceptions.
    /// </summary>
    /// <param name="hasHeader">
    /// Whether the CSV has a header row. Defaults to <see langword="true"/>.
    /// </param>
    /// <param name="delimiter">
    /// The field delimiter. Defaults to <c>","</c>.
    /// </param>
    /// <param name="culture">
    /// The culture used for type conversion. Defaults to <see cref="CultureInfo.InvariantCulture"/>
    /// when <see langword="null"/>.
    /// </param>
    /// <returns>A new <see cref="CsvConfiguration"/> instance.</returns>
    public static CsvConfiguration CreateDefault(
        bool hasHeader = true,
        string delimiter = ",",
        CultureInfo? culture = null)
    {
        return new CsvConfiguration(culture ?? CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
            Delimiter = delimiter,

            // Trim whitespace and ignore casing/separators when matching headers to properties.
            PrepareHeaderForMatch = args =>
                args.Header?.Trim()
                    .Replace(" ", "")
                    .Replace("_", "")
                    .Replace("-", "")
                    .ToLowerInvariant() ?? "",

            // Skip rows where every field is empty or whitespace.
            ShouldSkipRecord = args =>
            {
                var record = args.Row.Parser.Record;
                return record is null || record.All(string.IsNullOrWhiteSpace);
            },

            // Suppress hard failures — callers that care can use ReadSafe / ReadFileSafe.
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null
        };
    }
}
