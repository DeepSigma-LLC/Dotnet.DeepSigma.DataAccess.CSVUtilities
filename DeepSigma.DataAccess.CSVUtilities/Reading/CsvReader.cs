using CsvHelper;
using CsvHelper.Configuration;
using DeepSigma.DataAccess.CSVUtilities.Configuration;
using DeepSigma.DataAccess.CSVUtilities.Results;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DeepSigma.DataAccess.CSVUtilities.Reading;

/// <summary>
/// Static helper for reading CSV data from files, streams, strings, and <see cref="TextReader"/> instances.
/// </summary>
/// <remarks>
/// All overloads accept an optional <see cref="CsvConfiguration"/>; when omitted the defaults from
/// <see cref="CsvDefaults.CreateDefault"/> are used.  Async overloads accept a
/// <see cref="CancellationToken"/> and streaming overloads return <see cref="IAsyncEnumerable{T}"/>
/// so callers can process large files row-by-row without buffering everything into memory.
/// </remarks>
public static class CsvReader
{
    // -------------------------------------------------------------------------
    // From string
    // -------------------------------------------------------------------------

    /// <summary>
    /// Parses a CSV string and returns all records as a list.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="csvText">The raw CSV content.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static List<T> ReadFromString<T>(
        string csvText,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var reader = new StringReader(csvText);
        return ReadFromReader<T>(reader, config, culture);
    }

    /// <summary>
    /// Parses a CSV string using a custom <see cref="ClassMap{T}"/> and returns all records as a list.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <typeparam name="TMap">The <see cref="ClassMap{T}"/> to register.</typeparam>
    /// <param name="csvText">The raw CSV content.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static List<T> ReadFromString<T, TMap>(
        string csvText,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
        where TMap : ClassMap
    {
        using var reader = new StringReader(csvText);
        using var csv = CreateCsvReader(reader, config, culture);
        csv.Context.RegisterClassMap<TMap>();
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Parses a CSV string, collecting successfully mapped records and per-row errors
    /// instead of throwing on the first failure.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="csvText">The raw CSV content.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A <see cref="CsvImportResult{T}"/> containing records and any errors.</returns>
    public static CsvImportResult<T> ReadFromStringSafe<T>(
        string csvText,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var reader = new StringReader(csvText);
        return ReadFromReaderSafe<T>(reader, config, culture);
    }

    /// <summary>
    /// Parses a CSV string using a custom <see cref="ClassMap{T}"/>, collecting records and per-row errors.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <typeparam name="TMap">The <see cref="ClassMap{T}"/> to register.</typeparam>
    /// <param name="csvText">The raw CSV content.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A <see cref="CsvImportResult{T}"/> containing records and any errors.</returns>
    public static CsvImportResult<T> ReadFromStringSafe<T, TMap>(
        string csvText,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
        where TMap : ClassMap
    {
        using var reader = new StringReader(csvText);
        using var csv = CreateCsvReader(reader, config, culture);
        csv.Context.RegisterClassMap<TMap>();
        return ReadSafe<T>(csv);
    }

    // -------------------------------------------------------------------------
    // From TextReader
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads all records from an existing <see cref="TextReader"/>.
    /// </summary>
    /// <remarks>The caller retains ownership of <paramref name="textReader"/> and is responsible for disposal.</remarks>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="textReader">The reader to consume.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static List<T> ReadFromReader<T>(
        TextReader textReader,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var csv = CreateCsvReader(textReader, config, culture);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Reads all records from an existing <see cref="TextReader"/>, capturing per-row errors.
    /// </summary>
    /// <remarks>The caller retains ownership of <paramref name="textReader"/> and is responsible for disposal.</remarks>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="textReader">The reader to consume.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A <see cref="CsvImportResult{T}"/> containing records and any errors.</returns>
    public static CsvImportResult<T> ReadFromReaderSafe<T>(
        TextReader textReader,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var csv = CreateCsvReader(textReader, config, culture);
        return ReadSafe<T>(csv);
    }

    // -------------------------------------------------------------------------
    // From stream
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads all records from a <see cref="Stream"/>.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="leaveOpen">
    /// When <see langword="true"/> the underlying <paramref name="stream"/> is left open after reading;
    /// when <see langword="false"/> (the default) it is disposed.
    /// </param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static List<T> ReadFromStream<T>(
        Stream stream,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        bool leaveOpen = false)
    {
        using var reader = new StreamReader(stream, leaveOpen: leaveOpen);
        return ReadFromReader<T>(reader, config, culture);
    }

    /// <summary>
    /// Reads all records from a <see cref="Stream"/>, capturing per-row errors.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="leaveOpen">
    /// When <see langword="true"/> the underlying <paramref name="stream"/> is left open after reading.
    /// </param>
    /// <returns>A <see cref="CsvImportResult{T}"/> containing records and any errors.</returns>
    public static CsvImportResult<T> ReadFromStreamSafe<T>(
        Stream stream,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        bool leaveOpen = false)
    {
        using var reader = new StreamReader(stream, leaveOpen: leaveOpen);
        return ReadFromReaderSafe<T>(reader, config, culture);
    }

    /// <summary>
    /// Reads all records from a <see cref="Stream"/> asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="leaveOpen">
    /// When <see langword="true"/> the underlying <paramref name="stream"/> is left open after reading.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static async Task<List<T>> ReadFromStreamAsync<T>(
        Stream stream,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        bool leaveOpen = false,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, leaveOpen: leaveOpen);
        using var csv = CreateCsvReader(reader, config, culture);
        return await CollectAsync<T>(csv, cancellationToken);
    }

    /// <summary>
    /// Streams records from a <see cref="Stream"/> one at a time via <see cref="IAsyncEnumerable{T}"/>,
    /// suitable for large files where buffering all rows is undesirable.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="leaveOpen">
    /// When <see langword="true"/> the underlying <paramref name="stream"/> is left open after reading.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async sequence of <typeparamref name="T"/> instances.</returns>
    public static async IAsyncEnumerable<T> StreamFromStream<T>(
        Stream stream,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        bool leaveOpen = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, leaveOpen: leaveOpen);
        using var csv = CreateCsvReader(reader, config, culture);

        await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
        {
            yield return record;
        }
    }

    // -------------------------------------------------------------------------
    // From file
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads all records from a CSV file at <paramref name="path"/>.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static List<T> ReadFromFile<T>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var reader = new StreamReader(path);
        return ReadFromReader<T>(reader, config, culture);
    }

    /// <summary>
    /// Reads all records from a CSV file, applying a custom <see cref="ClassMap{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <typeparam name="TMap">The <see cref="ClassMap{T}"/> to register.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static List<T> ReadFromFile<T, TMap>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
        where TMap : ClassMap
    {
        using var reader = new StreamReader(path);
        using var csv = CreateCsvReader(reader, config, culture);
        csv.Context.RegisterClassMap<TMap>();
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Reads all records from a CSV file, capturing per-row errors instead of throwing.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A <see cref="CsvImportResult{T}"/> containing records and any errors.</returns>
    public static CsvImportResult<T> ReadFromFileSafe<T>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
    {
        using var reader = new StreamReader(path);
        return ReadFromReaderSafe<T>(reader, config, culture);
    }

    /// <summary>
    /// Reads all records from a CSV file using a custom <see cref="ClassMap{T}"/>,
    /// capturing per-row errors instead of throwing.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <typeparam name="TMap">The <see cref="ClassMap{T}"/> to register.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <returns>A <see cref="CsvImportResult{T}"/> containing records and any errors.</returns>
    public static CsvImportResult<T> ReadFromFileSafe<T, TMap>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null)
        where TMap : ClassMap
    {
        using var reader = new StreamReader(path);
        using var csv = CreateCsvReader(reader, config, culture);
        csv.Context.RegisterClassMap<TMap>();
        return ReadSafe<T>(csv);
    }

    /// <summary>
    /// Reads all records from a CSV file asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static async Task<List<T>> ReadFromFileAsync<T>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        using var csv = CreateCsvReader(reader, config, culture);
        return await CollectAsync<T>(csv, cancellationToken);
    }

    /// <summary>
    /// Reads all records from a CSV file asynchronously using a custom <see cref="ClassMap{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <typeparam name="TMap">The <see cref="ClassMap{T}"/> to register.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <typeparamref name="T"/> instances.</returns>
    public static async Task<List<T>> ReadFromFileAsync<T, TMap>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default)
        where TMap : ClassMap
    {
        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        using var csv = CreateCsvReader(reader, config, culture);
        csv.Context.RegisterClassMap<TMap>();
        return await CollectAsync<T>(csv, cancellationToken);
    }

    /// <summary>
    /// Streams records from a file one at a time via <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async sequence of <typeparamref name="T"/> instances.</returns>
    public static async IAsyncEnumerable<T> StreamFromFile<T>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        using var csv = CreateCsvReader(reader, config, culture);

        await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
        {
            yield return record;
        }
    }

    /// <summary>
    /// Streams records from a file one at a time via <see cref="IAsyncEnumerable{T}"/>,
    /// applying a custom <see cref="ClassMap{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    /// <typeparam name="TMap">The <see cref="ClassMap{T}"/> to register.</typeparam>
    /// <param name="path">Absolute or relative path to the CSV file.</param>
    /// <param name="config">Optional configuration; defaults to <see cref="CsvDefaults.CreateDefault"/>.</param>
    /// <param name="culture">Optional culture override for type conversion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async sequence of <typeparamref name="T"/> instances.</returns>
    public static async IAsyncEnumerable<T> StreamFromFile<T, TMap>(
        string path,
        CsvConfiguration? config = null,
        CultureInfo? culture = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TMap : ClassMap
    {
        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        using var csv = CreateCsvReader(reader, config, culture);
        csv.Context.RegisterClassMap<TMap>();

        await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
        {
            yield return record;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="CsvHelper.CsvReader"/> over <paramref name="textReader"/>, applying
    /// the supplied <paramref name="config"/> or falling back to <see cref="CsvDefaults.CreateDefault"/>.
    /// </summary>
    private static CsvHelper.CsvReader CreateCsvReader(
        TextReader textReader,
        CsvConfiguration? config,
        CultureInfo? culture)
    {
        config ??= CsvDefaults.CreateDefault(culture: culture);
        return new CsvHelper.CsvReader(textReader, config);
    }

    /// <summary>
    /// Asynchronously consumes <paramref name="csv"/> into a list of <typeparamref name="T"/>.
    /// </summary>
    private static async Task<List<T>> CollectAsync<T>(
        CsvHelper.CsvReader csv,
        CancellationToken cancellationToken)
    {
        var records = new List<T>();
        await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
        {
            records.Add(record);
        }
        return records;
    }

    /// <summary>
    /// Iterates an already-configured <see cref="CsvHelper.CsvReader"/>, collecting records and
    /// capturing per-row errors into a <see cref="CsvImportResult{T}"/>.
    /// </summary>
    private static CsvImportResult<T> ReadSafe<T>(CsvHelper.CsvReader csv)
    {
        var result = new CsvImportResult<T>();

        // Read and skip the header row when present.
        if (csv.Configuration.HasHeaderRecord)
        {
            csv.Read();
            csv.ReadHeader();
        }

        while (csv.Read())
        {
            try
            {
                var record = csv.GetRecord<T>();
                if (record is not null)
                    result.Records.Add(record);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new CsvImportError(
                    Row: csv.Context.Parser?.Row,
                    Message: ex.Message,
                    RawRecord: csv.Context.Parser?.RawRecord));
            }
        }

        return result;
    }
}
