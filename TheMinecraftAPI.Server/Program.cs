using System.IO.Compression;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using TheMinecraftAPI.ModLoaders.Clients;
using TheMinecraftAPI.Server.Data;
using TheMinecraftAPI.Vanilla;
using Timer = System.Timers.Timer;

namespace TheMinecraftAPI.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        ApplicationConfiguration.Instance.Initialize(Files.ApplicationConfiguration);
        ConfigureLogging();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSerilog();
        builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

#if RELEASE
        builder.Services.Configure<ForwardedHeadersOptions>(options => { options.KnownProxies.Add(IPAddress.Parse("10.0.0.100")); });
#endif

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseStatusCodePagesWithRedirects("/error/{0}?url={1}");
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseHttpsRedirection();
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        app.MapControllers();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            ApplicationConfiguration.Instance.Save();

            Log.Debug("Application exiting after {TIME}.", ApplicationData.UpTime);
            Log.CloseAndFlush();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "Unhandled exception: {REPORT}", CrashHandler.Report(exception));
            }
        };

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        UpdateCache();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        Timer timer = new(TimeSpan.FromHours(24));
        timer.Elapsed += async (_, _) => await UpdateCache();


        app.Run($"http://localhost:{ApplicationConfiguration.Instance.Port}");
    }

    private static async Task UpdateCache()
    {
        long startTime = DateTime.Now.Ticks;
        Log.Debug("Updating cache.");
        var versionHistory = await MinecraftResources.GetVersions();
        await ForgeClient.UpdateCacheFromWeb(versionHistory.Releases.Select(i => i.Id).ToArray());
        Log.Debug("Cache took {TIME} to update", TimeSpan.FromTicks(DateTime.Now.Ticks - startTime));
    }

    private static void ConfigureLogging()
    {
        // Initialize Logging
        string[] logs = Directory.GetFiles(Directories.Logs, "*.log");
        if (logs.Length != 0)
        {
            using ZipArchive archive = ZipFile.Open(Path.Combine(Directories.Logs, $"logs-{DateTime.Now:MM-dd-yyyy HH-mm-ss.ffff}.zip"), ZipArchiveMode.Create);
            foreach (string log in logs)
            {
                archive.CreateEntryFromFile(log, Path.GetFileName(log));
                File.Delete(log);
            }
        }

        TimeSpan flushTime = TimeSpan.FromSeconds(30);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(

#if DEBUG
                LogEventLevel.Verbose,
#else
                ApplicationConfiguration.Instance.LogLevel,
#endif
                outputTemplate: $"[{ApplicationData.ApplicationName}] [{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}")
            .WriteTo.File(Files.DebugLog, LogEventLevel.Verbose, buffered: true, flushToDiskInterval: flushTime)
            .WriteTo.File(Files.LatestLog, LogEventLevel.Information, buffered: true, flushToDiskInterval: flushTime)
            .WriteTo.File(Files.ErrorLog, LogEventLevel.Error, buffered: false)
            .CreateLogger();
    }
}