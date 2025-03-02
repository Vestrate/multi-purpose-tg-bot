using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WordCounterBot.APIL.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EnsurePfxCertExists();
            CreateHostBuilder(args).Build().Run();
        }

        private static void EnsurePfxCertExists()
        {
            var certbotEnabled =
                Environment.GetEnvironmentVariable("CertbotEnabled") == "true";

            var pfxFilePath = 
                Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
            var pemCertPath = Environment.GetEnvironmentVariable("SSLCertPath");
            var pemKeyPath = Environment.GetEnvironmentVariable("CertKeyPath");
            var certPassword =
                Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant();
            
            if (string.IsNullOrEmpty(pfxFilePath) 
                || string.IsNullOrEmpty(pemCertPath) 
                || string.IsNullOrEmpty(pemKeyPath) 
                || string.IsNullOrEmpty(certPassword)
                || (!string.IsNullOrEmpty(environment) && environment == "development")
                || !certbotEnabled)
            {
                Console.WriteLine("Certbot is disabled");
                return;
            }

            var certificateExists = false;

            while (!certificateExists)
            {
                if (File.Exists(pfxFilePath))
                {
                    certificateExists = true;

                    Console.WriteLine("Pfx file has been created");
                    continue;
                }

                if (!File.Exists(pemCertPath) || !File.Exists(pemKeyPath))
                {
                    Thread.Sleep(5000);
                    continue;
                }

                var args = $@"pkcs12 -export -out {pfxFilePath} -inkey {pemKeyPath} -passin pass:{certPassword} -passout pass:{certPassword} -in {pemCertPath}";
                
                var startInfo = new ProcessStartInfo()
                    { FileName = "openssl", Arguments = args, };
                var process = new Process() { StartInfo = startInfo, };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception("openssl failed");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseStartup<Startup>();
                });
    }
}
