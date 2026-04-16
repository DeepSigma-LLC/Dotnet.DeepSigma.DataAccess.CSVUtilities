using CsvHelper.Configuration;

namespace DeepSigma.DataAccess.CsvUtilities.Test.Fixtures;

/// <summary>Index-based mapping for headerless <see cref="CustomerCsvRow"/> files.</summary>
public sealed class CustomerNoHeaderMap : ClassMap<CustomerCsvRow>
{
    public CustomerNoHeaderMap()
    {
        Map(x => x.Id).Index(0);
        Map(x => x.FirstName).Index(1);
        Map(x => x.LastName).Index(2);
        Map(x => x.Email).Index(3);
        Map(x => x.Balance).Index(4);
        Map(x => x.BirthDate).Index(5).Optional();
    }
}
