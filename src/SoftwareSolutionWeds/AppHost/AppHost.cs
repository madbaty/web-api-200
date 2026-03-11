using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var natsTransport = builder.AddNats("nats")
    .WithJetStream()
    .WithLifetime(ContainerLifetime.Persistent);

var scalar = builder.AddScalarApiReference(); // running a container and configuring it.
var pgServer = builder.AddPostgres("pg-server") // so is this!
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb();

var softwareDb = pgServer.AddDatabase("software-db");
var vendorsDb = pgServer.AddDatabase("vendors-db");

var notificationApi = builder.AddProject<Projects.Notification_Api>("notification-api");

var softwareApi = builder.AddProject<Projects.Software_Api>("software-api")
    .WithReference(notificationApi)
    .WithReference(softwareDb)
    .WaitFor(softwareDb);


scalar.WithApiReference(softwareApi);
scalar.WithApiReference(notificationApi);


builder.AddProject<Projects.Gateway>("gateway")
    .WithReference(softwareApi);


builder.AddProject<Projects.Vendors_Api>("vendors-api")
    .WaitFor(vendorsDb)
    .WithReference(vendorsDb)
    .WithReference(natsTransport);

builder.Build().Run();
