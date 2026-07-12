using Bogus;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class CarBuilder
{
    private static readonly Faker Faker = new();

    private Guid _id = Guid.NewGuid();
    private string _licensePlate = Faker.Vehicle.Vin()[..8];
    private int _year = 2023;
    private string _color = Faker.Commerce.Color();
    private int _mileage = 1000;
    private decimal _dailyPrice = 750m;
    private CarStatus _status = CarStatus.Available;
    private Guid _brandId = Guid.NewGuid();
    private Guid _modelId = Guid.NewGuid();
    private Guid _locationId = Guid.NewGuid();
    private Brand? _brand;
    private Model? _model;

    public CarBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CarBuilder WithLicensePlate(string licensePlate)
    {
        _licensePlate = licensePlate;
        return this;
    }

    public CarBuilder WithStatus(CarStatus status)
    {
        _status = status;
        return this;
    }

    public CarBuilder WithDailyPrice(decimal dailyPrice)
    {
        _dailyPrice = dailyPrice;
        return this;
    }

    public CarBuilder WithBrandId(Guid brandId)
    {
        _brandId = brandId;
        return this;
    }

    public CarBuilder WithModelId(Guid modelId)
    {
        _modelId = modelId;
        return this;
    }

    public CarBuilder WithLocationId(Guid locationId)
    {
        _locationId = locationId;
        return this;
    }

    public CarBuilder WithBrand(Brand brand)
    {
        _brand = brand;
        _brandId = brand.Id;
        return this;
    }

    public CarBuilder WithModel(Model model)
    {
        _model = model;
        _modelId = model.Id;
        return this;
    }

    public Car Build() => new()
    {
        Id = _id,
        LicensePlate = _licensePlate,
        Year = _year,
        Color = _color,
        Mileage = _mileage,
        DailyPrice = _dailyPrice,
        Status = _status,
        BrandId = _brandId,
        ModelId = _modelId,
        LocationId = _locationId,
        Brand = _brand!,
        Model = _model!
    };
}
