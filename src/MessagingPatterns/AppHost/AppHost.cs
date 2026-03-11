var builder = DistributedApplication.CreateBuilder(args);

var natsTransport = builder.AddNats("nats")
    .WithJetStream()
    .WithLifetime(ContainerLifetime.Persistent);

var pg = builder.AddPostgres("pg")
    .WithLifetime(ContainerLifetime.Persistent);

var dbOne = pg.AddDatabase("db-one");
var dbTwo = pg.AddDatabase("db-two");

var apiOne = builder.AddProject<Projects.ApiOne>("one")
    .WithReference(dbOne)
    .WithReference(natsTransport)
    .WaitFor(natsTransport)
    .WaitFor(dbOne);

builder.AddProject<Projects.ApiTwo>("two")
    .WithReference(dbTwo)
    .WithReference(natsTransport)
    .WaitFor(natsTransport)
    .WaitFor(apiOne) // apiOne creates the "PEOPLE" stream
    .WaitFor(dbTwo);

builder.AddProject<Projects.PlainListener>("listener")
    .WithReplicas(2)
    .WithReference(natsTransport)
    .WaitFor(natsTransport)
    .WaitFor(apiOne); // apiOne creates the "PEOPLE" stream

builder.AddProject<Projects.LateComer>("late-comer")
    .WithExplicitStart()
    .WithReference(natsTransport)
    .WaitFor(natsTransport)
    .WaitFor(apiOne); // apiOne creates the "PEOPLE" stream

builder.Build().Run();