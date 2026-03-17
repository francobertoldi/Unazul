using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Persistence;

namespace SA.Identity.Tests.Integration.Fixtures;

public sealed class InMemoryDbFixture : IDisposable
{
    public IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new IdentityDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    public void Dispose() { }
}
