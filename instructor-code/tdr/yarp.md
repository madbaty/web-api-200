# Yarp - Reverse Proxy



## Config and Schema

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/yarp/refs/heads/main/src/ReverseProxy/ConfigurationSchema.json",
  "ReverseProxy": {
    "Routes": {
     
     
    },
    "Clusters": {
      
    }
  }
}
```

## Providing

```csharp
builder.Configuration.AddJsonFile("yarp-config.json", optional: false, reloadOnChange: true).Build();


builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();
```