using Alba;
using Alba.Security;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Software.Api.Clients;
using SoftwareShared.Notifications;
using Testcontainers.PostgreSql;

namespace Software.Tests.Fixtures;

public class SoftwareSystemTestFixture : IAsyncLifetime
{
    public IAlbaHost Host { get; set; } = null!;
    private PostgreSqlContainer _pgContainer = null!;
    public IDoNotifications NotificationMock { get; set; } = null!;
    public DateTimeOffset TestClock = new DateTimeOffset(1969, 04, 20, 23, 59, 59, TimeSpan.FromHours(-4));
    public async ValueTask InitializeAsync()
    {
        var fakeTime = new FakeTimeProvider(TestClock);
        NotificationMock = Substitute.For<IDoNotifications>();
        _pgContainer = new PostgreSqlBuilder("postgres:17.6")
            
            .Build(); 
       
        await _pgContainer.StartAsync();
        Host = await AlbaHost.For<Program>(config =>
        {
            config.UseSetting("ConnectionStrings:software-db", _pgContainer.GetConnectionString());
            // config.ConfigureServices = add a service that doesn't exist yet.
            // config.ConfigureTestServices = replace one that is there with something else for this test.
            config.ConfigureTestServices(sp =>
            {
                sp.AddSingleton(NotificationMock);
                sp.AddSingleton<TimeProvider>(_ => fakeTime);
            });
        }, new AuthenticationStub().WithName("test-user") );
       
    }
    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
        await _pgContainer.DisposeAsync();
    }
}

[CollectionDefinition("SoftwareSystemTestCollection")]
public class SystemTestCollection : ICollectionFixture<SoftwareSystemTestFixture>
{
    
}

public class DummyNotifier : IDoNotifications
{
    public Task SendNotification(NotificationRequest request)
    {
        // cool cool. Whatevs, dude.
        return Task.CompletedTask;
    }
}