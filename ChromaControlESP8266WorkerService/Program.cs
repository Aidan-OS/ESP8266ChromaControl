using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System;

namespace ChromaControlESP8266WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ESP8266ChromaControl\\logs");

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
            
            return Host.CreateDefaultBuilder(args)
                        .ConfigureLogging(loggerFactory => loggerFactory.AddFile(Path.Combine(logFolder, "ESP8266ChromaControl.log")))
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddHostedService<TimerService>();
                         });
        }
    }
}
