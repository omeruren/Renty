using Bogus;
using Renty.Server.Domain.Entities;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class LocationBuilder
{
    private static readonly Faker Faker = new();

    private Guid _id = Guid.NewGuid();
    private string _name = Faker.Address.City() + " Branch";
    private string _address = Faker.Address.StreetAddress();
    private string _city = Faker.Address.City();
    private bool _isActive = true;

    public LocationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public LocationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public LocationBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Location Build() => new()
    {
        Id = _id,
        Name = _name,
        Address = _address,
        City = _city,
        IsActive = _isActive
    };
}
