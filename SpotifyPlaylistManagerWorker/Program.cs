using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SpotifyPlaylistManagerWorker.Repositories;
using SpotifyPlaylistManagerWorker.Sources;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace SpotifyPlaylistManagerWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("SpotifyPlaylistManagerWorker.Program Starting Up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "SpotifyPlaylistManagerWorker.Program failed to start correctly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    var spotifyConfiguration = hostContext.Configuration.GetSection("Spotify");
                    var sourcesConfiguration = hostContext.Configuration.GetSection("Source");

                    services.AddHttpClient("SpotifyApi", x =>
                    {
                        x.BaseAddress = new Uri(spotifyConfiguration["ApiServer"]);
                        x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    });

                    var key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{spotifyConfiguration["ClientId"]}:{spotifyConfiguration["ClientSecret"]}"));
                    services.AddHttpClient("TokenApi", x =>
                    {
                        x.BaseAddress = new Uri(spotifyConfiguration["TokenServer"]);
                        x.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {key}");
                    });

                    services.AddHttpClient("EksenServer", x =>
                    {
                        x.BaseAddress = new Uri(sourcesConfiguration["Eksen:BaseAddress"]);
                    });

                    services.AddHttpClient("JoyFmServer", x =>
                    {
                        x.BaseAddress = new Uri(sourcesConfiguration["JoyFm:BaseAddress"]);
                    });

                    services.AddHttpClient("JoyTurkRockServer", x =>
                    {
                        x.BaseAddress = new Uri(sourcesConfiguration["JoyTurkRock:BaseAddress"]);
                    });

                    services.AddHttpClient("RedFmServer", x =>
                    {
                        x.BaseAddress = new Uri(sourcesConfiguration["RedFm:BaseAddress"]);
                    });

                    services.AddHttpClient("VeronicaServer", x =>
                    {
                        x.BaseAddress = new Uri(sourcesConfiguration["Veronica:BaseAddress"]);
                    });

                    services.AddHttpClient("VeronicaRockServer", x =>
                    {
                        x.BaseAddress = new Uri(sourcesConfiguration["VeronicaRock:BaseAddress"]);
                    });

                    services.AddSingleton<CredentialsRepository>();
                    services.AddSingleton<PlaylistRepository>();
                    services.AddSingleton<TrackRepository>();
                    services.AddSingleton<Spotify>();
                    services.AddSingleton<Eksen>();
                    services.AddSingleton<JoyFm>();
                    services.AddSingleton<JoyTurkRock>();
                    services.AddSingleton<RedFm>();
                    services.AddSingleton<Veronica>();
                    services.AddSingleton<VeronicaRock>();
                });
    }
}
