using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChromaBroadcast;
using System.Net.Http;

namespace ChromaControlESP8266WorkerService
{
    public class TimerService : IHostedService, IAsyncDisposable
    {
        private readonly Task _completedTask = Task.CompletedTask;
        private readonly ILogger<TimerService> _logger;

        private Timer? _timer;

        static readonly Guid ChromaControlESP8266GUID = Guid.Parse("00000000-0000-0000-0000-000000000000");

        private static readonly HttpClient client = new HttpClient();
        private List<string> ipAddresses = new List<string>();

        public TimerService(ILogger<TimerService> logger)
        {
            _logger = logger;

            RzResult lResult = RzChromaBroadcastAPI.Init(ChromaControlESP8266GUID);

            if (lResult == RzResult.Success)
            {
                RzChromaBroadcastAPI.RegisterEventNotification(OnChromaBroadcastEvent);
            }
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(ChromaControlESP8266WorkerService)} is running.");
            //Change this to adding from file
            ipAddresses.Add("http://192.168.1.61:8080");

            try
            {
                TurnOnAllLEDs().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogInformation("Failed to Turn On LEDs");
            }

            return _completedTask;
        }

        protected RzResult OnChromaBroadcastEvent(RzChromaBroadcastType type, RzChromaBroadcastStatus? status, RzChromaBroadcastEffect? effect)
        {
            _logger.LogInformation("Message Received from Razer Chroma");
            if (type == RzChromaBroadcastType.BroadcastEffect)
            {
                _logger.LogInformation("Setting Razer Chroma Colours");
                var values = new Dictionary<string, string>
                {
                    { "r1", Convert.ToInt32(effect.Value.ChromaLink1.R).ToString() },
                    { "g1", Convert.ToInt32(effect.Value.ChromaLink1.G).ToString() },
                    { "b1", Convert.ToInt32(effect.Value.ChromaLink1.B).ToString() },

                    { "r2", Convert.ToInt32(effect.Value.ChromaLink2.R).ToString() },
                    { "g2", Convert.ToInt32(effect.Value.ChromaLink2.G).ToString() },
                    { "b2", Convert.ToInt32(effect.Value.ChromaLink2.B).ToString() },

                    { "r3", Convert.ToInt32(effect.Value.ChromaLink3.R).ToString() },
                    { "g3", Convert.ToInt32(effect.Value.ChromaLink3.G).ToString() },
                    { "b3", Convert.ToInt32(effect.Value.ChromaLink3.B).ToString() },

                    { "r4", Convert.ToInt32(effect.Value.ChromaLink4.R).ToString() },
                    { "g4", Convert.ToInt32(effect.Value.ChromaLink4.G).ToString() },
                    { "b4", Convert.ToInt32(effect.Value.ChromaLink4.B).ToString() },

                    { "r5", Convert.ToInt32(effect.Value.ChromaLink5.R).ToString() },
                    { "g5", Convert.ToInt32(effect.Value.ChromaLink5.G).ToString() },
                    { "b5", Convert.ToInt32(effect.Value.ChromaLink5.B).ToString() }
                };

                var content = new FormUrlEncodedContent(values);
                try
                {
                    PostToAll(content).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    _logger.LogInformation("Failed to set LEDs with Chroma");
                }

                _logger.LogInformation("Razer Chroma Colours Set!");
            }

            return RzResult.Success;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(ChromaControlESP8266WorkerService)} is stopping.");

            try
            {
                TurnOffAllLEDs().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogInformation("Failed to Turn On LEDs");
            }

            _timer?.Change(Timeout.Infinite, 0);

            return _completedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_timer is IAsyncDisposable timer)
            {
                await timer.DisposeAsync();
            }

            _timer = null;
        }

        private async Task PostToAll(FormUrlEncodedContent content)
        {
            foreach (string address in ipAddresses)
            {
                await client.PostAsync(address + "/LED", content); //TODO have to catch exceptions here
            }
        }

        private async Task TurnOffAllLEDs()
        {
            _logger.LogInformation("Shutting off LEDs");
            var values = new Dictionary<string, string>
            {
                { "r1", "0" },
                { "g1", "0" },
                { "b1", "0" },

                { "r2", "0" },
                { "g2", "0" },
                { "b2", "0" },

                { "r3", "0" },
                { "g3", "0" },
                { "b3", "0" },

                { "r4", "0" },
                { "g4", "0" },
                { "b4", "0" },

                { "r5", "0" },
                { "g5", "0" },
                { "b5", "0" }
            };

            var content = new FormUrlEncodedContent(values);
            await PostToAll(content);

            _logger.LogInformation("LEDs Shutoff");
        }

        private async Task TurnOnAllLEDs()
        {
            _logger.LogInformation("Turning on LEDs");
            var values = new Dictionary<string, string>
            {
                { "r1", "255" },
                { "g1", "255" },
                { "b1", "255" },

                { "r2", "255" },
                { "g2", "255" },
                { "b2", "255" },

                { "r3", "255" },
                { "g3", "255" },
                { "b3", "255" },

                { "r4", "255" },
                { "g4", "255" },
                { "b4", "255" },

                { "r5", "255" },
                { "g5", "255" },
                { "b5", "255" }
            };

            var content = new FormUrlEncodedContent(values);
            await PostToAll(content);

            _logger.LogInformation("LEDs Turned on");
        }
    }

}
