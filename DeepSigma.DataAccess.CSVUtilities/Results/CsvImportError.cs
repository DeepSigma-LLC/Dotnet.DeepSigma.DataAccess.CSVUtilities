namespace DeepSigma.DataAccess.CSVUtilities.Results;

/// <summary>
/// Represents a single row-level parse failure captured during a safe import operation.
/// </summary>
/// <param name="Row">The 1-based row number where the error occurred, if available.</param>
/// <param name="Message">The exception message describing the failure.</param>
/// <param name="RawRecord">The raw CSV text of the failing row, if available.</param>
public sealed record CsvImportError(int? Row, string Message, string? RawRecord);
