using System;
using System.Collections.Generic;
using System.ServiceProcess;
using ChromaBroadcast;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ChromaControlESP8266
{
    public partial class ChromaControlESP8266Service : ServiceBase
    {
        private EventLog eventLog1;

        private static readonly HttpClient client = new HttpClient();
        private List<string> ipAddresses = new List<string>();

        static readonly Guid ChromaBroadcastSampleApp = Guid.Parse("00000000-0000-0000-0000-000000000000");

        public ChromaControlESP8266Service()
        {
            InitializeComponent();

            eventLog1 = new EventLog();

            if (!EventLog.SourceExists("ChromaControlESP8266"))
            {
                EventLog.CreateEventSource(
                    "ChromaControlESP8266", "ChromaControlESP8266Logs");
            }
            eventLog1.Source = "ChromaControlESP8266";
            eventLog1.Log = "ChromaControlESP8266Logs";


            RzResult lResult = RzChromaBroadcastAPI.Init(ChromaBroadcastSampleApp);

            if (lResult == RzResult.Success)
            {
                RzChromaBroadcastAPI.RegisterEventNotification(OnChromaBroadcastEvent);
            }
        }

        protected RzResult OnChromaBroadcastEvent(RzChromaBroadcastType type, RzChromaBroadcastStatus? status, RzChromaBroadcastEffect? effect)
        {
            eventLog1.WriteEntry("Message Received from Razer Chroma");
            if (type == RzChromaBroadcastType.BroadcastEffect)
            {
                eventLog1.WriteEntry("Setting Razer Chroma Colours");
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
                PostToAll(content);

                eventLog1.WriteEntry("Razer Chroma Colours Set!");
            }

            return RzResult.Success;
        }

        private async Task PostToAll(FormUrlEncodedContent content)
        {
            foreach(string address in ipAddresses)
            {
                await client.PostAsync(address + "/LED", content);
            }
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");

            string fileIPAddrresses = File.ReadAllText(Application.StartupPath + "\\ipaddresses.txt");
            string[] splitIPs = fileIPAddrresses.Split(',');

            foreach(string ip in splitIPs)
            {
                ipAddresses.Add(ip);
                eventLog1.WriteEntry("Added IP address: " + ip);
            }

            //ipAddresses.Add("http://192.168.1.61:8080"); //TODO Make this not hard coded

            TurnOnAllLEDs();
        }

        protected override void OnStop()
        {
            this.RequestAdditionalTime(1000);

            eventLog1.WriteEntry("In OnStop");
            RzChromaBroadcastAPI.UnRegisterEventNotification();
            RzChromaBroadcastAPI.UnInit();

            try
            {
                shutoffLEDs().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Failed to shutoff LEDs");
            }
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            this.RequestAdditionalTime(1000);

            eventLog1.WriteEntry("In OnShutdown");
            RzChromaBroadcastAPI.UnRegisterEventNotification();
            RzChromaBroadcastAPI.UnInit();

            try
            {
                shutoffLEDs().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Failed to shutoff LEDs");
            }

        }

        private async Task shutoffLEDs()
        {
            eventLog1.WriteEntry("Shutting off LEDs");
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

            eventLog1.WriteEntry("LEDs Shutoff");
        }

        private async Task TurnOnAllLEDs()
        {
            eventLog1.WriteEntry("Turning on LEDs");
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

            eventLog1.WriteEntry("LEDs Turned on");
        }
    }
}
