using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var scalar = builder.AddScalarApiReference(); // running a container and configuring it.
var pgServer = builder.AddPostgres("pg-server") // so is this!
    .WithLifetime(ContainerLifetime.Persistent);

var softwareDb = pgServer.AddDatabase("software-db");

// Above this line is the "infra" stuff - or just development tools
// All the stuff that will need to be in the environment where this is running.
// After this line is the stuff I'm actually responsible for shipping.
var notificationApi = builder.AddProject<Projects.Notification_Api>("notification-api");

var softwareApi = builder.AddProject<Projects.Software_Api>("software-api")
    .WithReference(notificationApi)
    .WithReference(softwareDb)
    .WaitFor(softwareDb);


scalar.WithApiReference(softwareApi);
scalar.WithApiReference(notificationApi);


builder.Build().Run();
