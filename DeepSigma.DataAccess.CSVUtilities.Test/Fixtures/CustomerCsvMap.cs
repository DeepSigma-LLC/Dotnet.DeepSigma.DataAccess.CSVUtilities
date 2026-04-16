using CsvHelper.Configuration;

namespace DeepSigma.DataAccess.CsvUtilities.Test.Fixtures;

/// <summary>Explicit header-name mapping for <see cref="CustomerCsvRow"/>.</summary>
public sealed class CustomerCsvMap : ClassMap<CustomerCsvRow>
{
    public CustomerCsvMap()
    {
        Map(x => x.Id).Name("customer_id", "id");
        Map(x => x.FirstName).Name("first_name", "firstname", "first");
        Map(x => x.LastName).Name("last_name", "lastname", "last");
        Map(x => x.Email).Name("email_address", "email");
        Map(x => x.Balance).Name("account_balance", "balance");
        Map(x => x.BirthDate).Name("birth_date", "dob").Optional();
    }
}
