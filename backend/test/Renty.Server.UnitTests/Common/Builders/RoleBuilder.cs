using Renty.Server.Domain.Entities;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class RoleBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Customer";

    public RoleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RoleBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Role Build() => new()
    {
        Id = _id,
        Name = _name
    };
}
