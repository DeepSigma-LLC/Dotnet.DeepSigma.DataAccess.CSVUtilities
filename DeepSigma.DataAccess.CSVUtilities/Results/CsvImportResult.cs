namespace DeepSigma.DataAccess.CsvUtilities.Results;

/// <summary>
/// The result of a safe CSV import that collects both successfully parsed records
/// and any per-row errors, rather than throwing on the first failure.
/// </summary>
/// <typeparam name="T">The type each CSV row is mapped to.</typeparam>
public sealed class CsvImportResult<T>
{
    /// <summary>
    /// Successfully parsed records.
    /// </summary>
    public List<T> Records { get; } = [];

    /// <summary>
    /// Per-row errors encountered during parsing.
    /// </summary>
    public List<CsvImportError> Errors { get; } = [];

    /// <summary>
    /// <see langword="true"/> when <see cref="Errors"/> contains no entries.
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;
}
