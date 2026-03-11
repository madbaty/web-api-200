using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// params are things that are going to be in your environment.
// connection strings are handled , services are handled

//var p1 = builder.AddParameter("secret-word", true);

var scalar = builder.AddScalarApiReference(); // running a container and configuring it.
var pgServer = builder.AddPostgres("pg-server") // so is this!
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb();

var softwareDb = pgServer.AddDatabase("software-db");
    // this will be more complicated, probably. You'll need a database with a schema, maybe loaded with some "seed" data, etc.
    // Scripts can be run, or other container images can be used.

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
