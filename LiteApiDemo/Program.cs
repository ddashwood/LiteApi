using LiteApi;

await Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLiteApi(80);
    })
    .Build()
    .RunAsync();
