namespace DeepSigma.DataAccess.CSVUtilities.Test.Fixtures;

/// <summary>Test model representing a customer record.</summary>
public sealed class CustomerCsvRow
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateOnly? BirthDate { get; set; }
}
