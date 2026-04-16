using DeepSigma.DataAccess.CsvUtilities.Reading;
using DeepSigma.DataAccess.CsvUtilities.Test.Fixtures;
using System.Text;

namespace DeepSigma.DataAccess.CsvUtilities.Test.Reading;

public sealed class ReadFromStreamTests
{
    private static MemoryStream MakeCsvStream(string csv) =>
        new(Encoding.UTF8.GetBytes(csv));

    private const string ValidCsv =
        "customer_id,first_name,last_name,email,balance\n" +
        "1,Alice,Smith,alice@example.com,10.00\n" +
        "2,Bob,Jones,bob@example.com,20.00\n";

    [Fact]
    public void ReadFromStream_WithValidStream_ReturnsMappedRecords()
    {
        using var stream = MakeCsvStream(ValidCsv);
        var records = CsvReader.ReadFromStream<CustomerCsvRow>(stream);

        Assert.Equal(2, records.Count);
        Assert.Equal("Alice", records[0].FirstName);
    }

    [Fact]
    public void ReadFromStream_LeaveOpenFalse_DisposesStream()
    {
        var stream = MakeCsvStream(ValidCsv);
        CsvReader.ReadFromStream<CustomerCsvRow>(stream, leaveOpen: false);

        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    [Fact]
    public void ReadFromStream_LeaveOpenTrue_StreamRemainsOpen()
    {
        using var stream = MakeCsvStream(ValidCsv);
        CsvReader.ReadFromStream<CustomerCsvRow>(stream, leaveOpen: true);

        // Stream should still be readable (position will be at end but CanRead stays true).
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task ReadFromStreamAsync_WithValidStream_ReturnsMappedRecords()
    {
        using var stream = MakeCsvStream(ValidCsv);
        var records = await CsvReader.ReadFromStreamAsync<CustomerCsvRow>(
            stream, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, records.Count);
        Assert.Equal("Bob", records[1].FirstName);
    }

    [Fact]
    public async Task ReadFromStreamAsync_LeaveOpenTrue_StreamRemainsOpen()
    {
        var stream = MakeCsvStream(ValidCsv);
        await CsvReader.ReadFromStreamAsync<CustomerCsvRow>(
            stream, leaveOpen: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(stream.CanRead);
        await stream.DisposeAsync();
    }

    [Fact]
    public async Task ReadFromStreamAsync_CancellationToken_ThrowsWhenCancelled()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        using var stream = MakeCsvStream(ValidCsv);

        // Pre-cancelled token — intentionally not using TestContext token here.
#pragma warning disable xUnit1051
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CsvReader.ReadFromStreamAsync<CustomerCsvRow>(
                stream, cancellationToken: cts.Token));
#pragma warning restore xUnit1051
    }

    [Fact]
    public async Task StreamFromStream_YieldsAllRecords()
    {
        using var stream = MakeCsvStream(ValidCsv);
        var records = new List<CustomerCsvRow>();

        await foreach (var record in CsvReader.StreamFromStream<CustomerCsvRow>(
            stream, cancellationToken: TestContext.Current.CancellationToken))
        {
            records.Add(record);
        }

        Assert.Equal(2, records.Count);
    }

    [Fact]
    public void ReadFromStreamSafe_WithBadRow_CollectsError()
    {
        const string badCsv =
            "customer_id,first_name,last_name,email,balance\n" +
            "1,Alice,Smith,alice@example.com,1234.56\n" +
            "2,Bob,Jones,bob@example.com,OOPS\n";

        using var stream = MakeCsvStream(badCsv);
        var result = CsvReader.ReadFromStreamSafe<CustomerCsvRow>(stream);

        Assert.Single(result.Records);
        Assert.Single(result.Errors);
    }
}
