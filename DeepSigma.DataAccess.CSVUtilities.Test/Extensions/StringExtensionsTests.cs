using DeepSigma.DataAccess.CsvUtilities.Extensions;
using DeepSigma.DataAccess.CsvUtilities.Test.Fixtures;

namespace DeepSigma.DataAccess.CsvUtilities.Test.Extensions;

public sealed class StringExtensionsTests
{
    private static List<CustomerCsvRow> SampleRecords() =>
    [
        new() { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", Balance = 10.00m },
        new() { Id = 2, FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", Balance = 20.00m }
    ];

    [Fact]
    public void FromCsv_ParsesValidCsvString()
    {
        const string csv =
            "Id,FirstName,LastName,Email,Balance\n" +
            "1,Alice,Smith,alice@example.com,10.00\n";

        var records = csv.FromCsv<CustomerCsvRow>();

        Assert.Single(records);
        Assert.Equal("Alice", records[0].FirstName);
    }

    [Fact]
    public void ToCsv_SerializesRecordsToString()
    {
        var csv = SampleRecords().ToCsv();

        Assert.False(string.IsNullOrWhiteSpace(csv));
        Assert.Contains("Alice", csv);
        Assert.Contains("Bob", csv);
    }

    [Fact]
    public void ToCsv_ThenFromCsv_RoundTripsData()
    {
        var original = SampleRecords();
        var csv = original.ToCsv();
        var result = csv.FromCsv<CustomerCsvRow>();

        Assert.Equal(original.Count, result.Count);
        Assert.Equal(original[0].FirstName, result[0].FirstName);
        Assert.Equal(original[0].Balance, result[0].Balance);
        Assert.Equal(original[1].Email, result[1].Email);
    }

    [Fact]
    public void FromCsv_EmptyString_ReturnsEmptyList()
    {
        var records = string.Empty.FromCsv<CustomerCsvRow>();

        Assert.Empty(records);
    }

    [Fact]
    public void ToCsv_EmptyList_ReturnsHeaderOnlyString()
    {
        var csv = new List<CustomerCsvRow>().ToCsv();

        // CsvHelper always writes the header even for empty input.
        Assert.False(string.IsNullOrWhiteSpace(csv));
    }
}
