using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Persistence;

namespace SA.Config.Tests.Integration.Fixtures;

public sealed class InMemoryDbFixture : IDisposable
{
    public ConfigDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ConfigDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new ConfigDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    public void Dispose() { }
}
