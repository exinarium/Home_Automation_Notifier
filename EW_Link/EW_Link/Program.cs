using EW_Link;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<DeviceHandler>();
    })
    .Build();

host.Run();
