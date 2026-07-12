using Bogus;
using Renty.Server.Domain.Entities;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class UserBuilder
{
    private static readonly Faker Faker = new();

    private Guid _id = Guid.NewGuid();
    private string _email = Faker.Internet.Email();
    private string _firstName = Faker.Name.FirstName();
    private string _lastName = Faker.Name.LastName();
    private bool _isActive = true;

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public User Build() => new()
    {
        Id = _id,
        Email = _email,
        PasswordHash = "hashed-password",
        FirstName = _firstName,
        LastName = _lastName,
        IsActive = _isActive
    };
}
