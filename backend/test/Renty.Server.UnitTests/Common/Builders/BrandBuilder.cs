using Bogus;
using Renty.Server.Domain.Entities;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class BrandBuilder
{
    private static readonly Faker Faker = new();

    private Guid _id = Guid.NewGuid();
    private string _name = Faker.Company.CompanyName();

    public BrandBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public BrandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Brand Build() => new()
    {
        Id = _id,
        Name = _name
    };
}
