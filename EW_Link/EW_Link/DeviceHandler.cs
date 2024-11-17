using EwelinkNet.Classes;
using EwelinkNet;
using Microsoft.Extensions.Configuration;
using EwelinkNet.Classes.Events;

namespace EW_Link
{
    public class DeviceHandler : BackgroundService
    {
        private readonly ILogger<DeviceHandler> _logger;
        private static string? _signalDevice;
        private static string?[] _turnOnDevices;
        private static string?[] _turnOffDevices;
        private Ewelink? _ewelink;

        public DeviceHandler(ILogger<DeviceHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RunProcess();

            Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_ewelink?.Devices != null)
                    {
                        _ewelink.OpenWebSocket();
                    }
                    Task.Delay(1000);
                }
            });
        }

        private async void HandleEvent(object sender, EvendDeviceUpdate e)
        {
            if (e.Device.name == _signalDevice)
            {
                await _ewelink.GetDevices();
                var statusDevice = _ewelink.Devices.Where(x => x._id == e.Device._id).FirstOrDefault();

                if (statusDevice.online)
                {
                    Console.WriteLine($"{statusDevice.name} offline");

                    foreach (var device in _turnOffDevices)
                    {
                        var turnOff = _ewelink.Devices.Where(x => x.name == device).FirstOrDefault() as SwitchDevice;
                        if (turnOff != null)
                        {
                            Console.WriteLine($"Turning off {device}...");
                            turnOff.TurnOff();
                        }
                    }

                    foreach (var device in _turnOnDevices)
                    {
                        var turnOn = _ewelink.Devices.Where(x => x.name == device).FirstOrDefault() as SwitchDevice;
                        if (turnOn != null)
                        {
                            Console.WriteLine($"Turning on {device}...");
                            turnOn.TurnOn();
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{statusDevice.name} online");

                    foreach (var device in _turnOffDevices)
                    {
                        var turnOff = _ewelink.Devices.Where(x => x.name == device).FirstOrDefault() as SwitchDevice;
                        if (turnOff != null)
                        {
                            Console.WriteLine($"Turning on {device}...");
                            turnOff.TurnOn();
                        }
                    }

                    foreach (var device in _turnOnDevices)
                    {
                        var turnOn = _ewelink.Devices.Where(x => x.name == device).FirstOrDefault() as SwitchDevice;
                        if (turnOn != null)
                        {
                            Console.WriteLine($"Turning off {device}...");
                            turnOn.TurnOff();
                        }
                    }
                }
            }
        }

        public async Task RunProcess()
        {
            try
            {
                Console.WriteLine("Starting home automation...\n");

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json");

                var config = configuration.Build();
                _signalDevice = config.GetSection("signalDevice")?.Get<string>();
                _turnOnDevices = config.GetSection("turnOnDevices")?.Get<string[]>();
                _turnOffDevices = config.GetSection("turnOffDevices")?.Get<string[]>();
                var credentials = config.GetSection("credentials").Get<EW_Link.Config.Credentials>();

                Console.WriteLine("Logging in");
                if (credentials != null)
                {
                    _ewelink = new Ewelink(credentials.Username, credentials.Password);
                    var region = await _ewelink.GetRegion();
                    await _ewelink.GetCredentials();
                    await _ewelink.GetDevices();
                    _ewelink.OpenWebSocket();
                    _ewelink.OnDeviceChanged += HandleEvent;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await RunProcess();
            }

        }
    }
}