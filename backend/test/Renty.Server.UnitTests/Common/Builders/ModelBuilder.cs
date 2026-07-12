using Bogus;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class ModelBuilder
{
    private static readonly Faker Faker = new();

    private Guid _id = Guid.NewGuid();
    private string _name = Faker.Commerce.ProductName();
    private Guid _brandId = Guid.NewGuid();
    private VehicleCategory _category = VehicleCategory.Sedan;
    private int _seatCount = 5;
    private TransmissionType _transmissionType = TransmissionType.Automatic;
    private FuelType _fuelType = FuelType.Gasoline;

    public ModelBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ModelBuilder WithBrandId(Guid brandId)
    {
        _brandId = brandId;
        return this;
    }

    public ModelBuilder WithCategory(VehicleCategory category)
    {
        _category = category;
        return this;
    }

    public Model Build() => new()
    {
        Id = _id,
        Name = _name,
        Category = _category,
        SeatCount = _seatCount,
        TransmissionType = _transmissionType,
        FuelType = _fuelType,
        BrandId = _brandId
    };
}
