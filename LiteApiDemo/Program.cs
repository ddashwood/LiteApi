using LiteApi;

await Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddHostedService<LiteApiServer>();
    })
    .Build()
    .RunAsync();
