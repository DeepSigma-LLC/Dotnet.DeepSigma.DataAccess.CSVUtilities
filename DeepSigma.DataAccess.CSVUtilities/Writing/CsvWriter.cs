using CsvHelper;
using CsvHelper.Configuration;
using DeepSigma.DataAccess.CsvUtilities.Configuration;
using System.Globalization;
using System.Text;

namespace DeepSigma.DataAccess.CsvUtilities.Writing;

/// <summary>
/// Static helper for writing CSV data to files, streams, strings, and <see cref="TextWriter"/> instances.
/// </summary>
/// <remarks>
/// All overloads accept an optional <see cref="CsvConfiguration"/>; when omitted the defaults from
/// <see cref="CsvDefaults.CreateDefault"/> are used.  Async overloads accept a
/// <see cref="CancellationToken"/>.  <see cref="AppendToFile{T}"/> omits the header row when the
/// target file already exists and is non-empty.
/// </remarks>
public static class CsvWriter
{
    // -------------------------------------------------------------------------
    // To string
    // -------------------------------------------------------------------------

    /// <summary>
    /// Serializes <paramref name="records"/> to a CSV string.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="records">The records to serialize.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A CSV-formatted string.</returns>
    public static string WriteToString<T>(
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        using var csv = CreateCsvWriter(writer, config, culture);
        csv.WriteRecords(records);
        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // To TextWriter
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes <paramref name="records"/> to an existing <see cref="TextWriter"/>.
    /// </summary>
    /// <remarks>The caller retains ownership of <paramref name="textWriter"/> and is responsible for disposal.</remarks>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="textWriter">The writer to output into.</param>
    /// <param name="records">The records to serialize.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    public static void WriteToWriter<T>(
        TextWriter textWriter,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var csv = CreateCsvWriter(textWriter, config, culture, leaveOpen: true);
        csv.WriteRecords(records);
    }

    // -------------------------------------------------------------------------
    // To stream
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes <paramref name="records"/> to a <see cref="Stream"/>.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="records">The records to serialize.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="leaveOpen">
    /// When <see langword="true"/> the <paramref name="stream"/> is left open after writing;
    /// when <see langword="false"/> (the default) it is disposed.
    /// </param>
    public static void WriteToStream<T>(
        Stream stream,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        bool leaveOpen = false)
    {
        using var writer = new StreamWriter(stream, leaveOpen: leaveOpen);
        using var csv = CreateCsvWriter(writer, config, culture);
        csv.WriteRecords(records);
    }

    /// <summary>
    /// Writes <paramref name="records"/> to a <see cref="Stream"/> asynchronously.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="records">The records to serialize.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="leaveOpen">
    /// When <see langword="true"/> the <paramref name="stream"/> is left open after writing.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static async Task WriteToStreamAsync<T>(
        Stream stream,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        bool leaveOpen = false,
        CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(stream, leaveOpen: leaveOpen);
        await using var csv = CreateCsvWriter(writer, config, culture);
        await csv.WriteRecordsAsync(records, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // To file
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes <paramref name="records"/> to a CSV file, creating or overwriting it.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="path">Absolute or relative path to the target file.</param>
    /// <param name="records">The records to serialize.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    public static void WriteToFile<T>(
        string path,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var writer = new StreamWriter(path, append: false);
        using var csv = CreateCsvWriter(writer, config, culture);
        csv.WriteRecords(records);
    }

    /// <summary>
    /// Writes <paramref name="records"/> to a CSV file asynchronously, creating or overwriting it.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="path">Absolute or relative path to the target file.</param>
    /// <param name="records">The records to serialize.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static async Task WriteToFileAsync<T>(
        string path,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(path, append: false);
        await using var csv = CreateCsvWriter(writer, config, culture);
        await csv.WriteRecordsAsync(records, cancellationToken);
    }

    /// <summary>
    /// Appends <paramref name="records"/> to an existing CSV file, or creates it if it does not exist.
    /// The header row is written only when the file is new or empty.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="path">Absolute or relative path to the target file.</param>
    /// <param name="records">The records to append.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    public static void AppendToFile<T>(
        string path,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        var fileHasContent = File.Exists(path) && new FileInfo(path).Length > 0;

        using var writer = new StreamWriter(path, append: true);
        using var csv = CreateCsvWriter(writer, config, culture);

        if (fileHasContent)
        {
            WriteRecordsWithoutHeader(csv, records);
        }
        else
        {
            csv.WriteRecords(records);
        }
    }

    /// <summary>
    /// Appends <paramref name="records"/> to an existing CSV file asynchronously, or creates it.
    /// The header row is written only when the file is new or empty.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="path">Absolute or relative path to the target file.</param>
    /// <param name="records">The records to append.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static async Task AppendToFileAsync<T>(
        string path,
        IEnumerable<T> records,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default)
    {
        var fileHasContent = File.Exists(path) && new FileInfo(path).Length > 0;

        await using var writer = new StreamWriter(path, append: true);
        await using var csv = CreateCsvWriter(writer, config, culture);

        if (fileHasContent)
        {
            await WriteRecordsWithoutHeaderAsync(csv, records, cancellationToken);
        }
        else
        {
            await csv.WriteRecordsAsync(records, cancellationToken);
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="CsvHelper.CsvWriter"/> over <paramref name="textWriter"/>, applying
    /// the supplied <paramref name="config"/> or falling back to <see cref="CsvDefaults.CreateDefault"/>.
    /// </summary>
    private static CsvHelper.CsvWriter CreateCsvWriter(
        TextWriter textWriter,
        CsvConfiguration? config,
        CultureInfo? culture,
        bool leaveOpen = false)
    {
        config ??= CsvDefaults.CreateDefault(culture: culture);
        return new CsvHelper.CsvWriter(textWriter, config, leaveOpen);
    }

    /// <summary>
    /// Writes <paramref name="records"/> one at a time, bypassing the header row that
    /// <see cref="CsvHelper.CsvWriter.WriteRecords{T}(IEnumerable{T})"/> would otherwise emit.
    /// </summary>
    private static void WriteRecordsWithoutHeader<T>(
        CsvHelper.CsvWriter csv,
        IEnumerable<T> records)
    {
        foreach (var record in records)
        {
            csv.WriteRecord(record);
            csv.NextRecord();
        }
    }

    /// <summary>
    /// Asynchronous counterpart to <see cref="WriteRecordsWithoutHeader{T}"/>.
    /// </summary>
    private static async Task WriteRecordsWithoutHeaderAsync<T>(
        CsvHelper.CsvWriter csv,
        IEnumerable<T> records,
        CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();
            csv.WriteRecord(record);
            await csv.NextRecordAsync();
        }
    }
}
