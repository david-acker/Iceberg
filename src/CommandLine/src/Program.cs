using Iceberg.CommandLine.Logging;
using Iceberg.Export;
using Iceberg.Map.DependencyMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Iceberg.CommandLine;

public class Program
{
    public static async Task Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders()
                    .AddIcebergLogger();
            })
            .ConfigureServices(services =>
            {
                services.RegisterDependencyMapperServices();
                services.RegisterExportServices();
                services.RegisterCommandLineServices();
            })
            .Build();

        var sessionHandler = host.Services.GetService<IIcebergSessionHandler>()!;
       
        while (true)
        {
            Debug.Assert(sessionHandler != null, "Session handler was null.");

            var originalForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Iceberg > ");
            Console.ForegroundColor = originalForegroundColor;

            var commandLineInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(commandLineInput))
                continue;

            var separateCommands = commandLineInput.Split("&&");

            foreach (var command in separateCommands)
            {
                await sessionHandler.Handle(command);
            }
        }
    }
}