﻿using System.IO.Compression;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using TheMinecraftAPI.ModLoaders.Clients;
using TheMinecraftAPI.Server.Data;
using TheMinecraftAPI.Vanilla;
using Timer = System.Timers.Timer;

namespace TheMinecraftAPI.Server;

internal static class Program
{
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Instance.Initialize(Files.ApplicationConfiguration);
        ConfigureLogging();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xmlFilePath))
                options.IncludeXmlComments(xmlFilePath);
            Console.WriteLine(xmlFilePath);
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "The Minecraft API",
                Version = "v1",
                Description = "The Minecraft API is a RESTful API for querying minecraft servers, various modding apis, etc. It is designed to be a simple and easy to use API for developers to use in their applications.",
                TermsOfService = new Uri("https://theminecraftapi.com/legal/tos"),
                License = new OpenApiLicense()
                {
                    Name = "All Rights Reserved",
                    Url = new Uri("https://raw.githubusercontent.com/Drew-Chase/TheMinecraftAPI/master/LICENSE")
                }
            });


#if RELEASE
            options.AddServer(new OpenApiServer()
            {
                Url = "https://api.theminecraftapi.com",
                Description = "The Minecraft API",
            });
#endif
        });
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
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "The Minecraft API";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "The Minecraft API");
            options.ConfigObject.AdditionalItems.Add("favicon", "https://theminecraftapi.com/assets/images/favicon.png");
        });

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
        timer.Elapsed += (_, _) => ArchiveLogs();


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
        ArchiveLogs();

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

    private static void ArchiveLogs()
    {
        string[] logs = Directory.GetFiles(Directories.Logs, "*.log");
        if (logs.Length == 0) return;
        using ZipArchive archive = ZipFile.Open(Path.Combine(Directories.Logs, $"logs-{DateTime.Now:MM-dd-yyyy HH-mm-ss.ffff}.zip"), ZipArchiveMode.Create);
        foreach (string log in logs)
        {
            archive.CreateEntryFromFile(log, Path.GetFileName(log));
            File.Delete(log);
        }
    }
}