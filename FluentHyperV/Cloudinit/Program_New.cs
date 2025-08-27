//using HyperVProvisioning.Core;
//using HyperVProvisioning.Models;
//using HyperVProvisioning.Services;
//using HyperVProvisioning.Utils;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Serilog;
//using System.CommandLine;
//using System.Text.RegularExpressions;

//namespace HyperVProvisioning;

//class Program
//{
//    static async Task<int> Main(string[] args)
//    {
//        // Configure Serilog
//        Log.Logger = new LoggerConfiguration()
//            .WriteTo.Console()
//            .WriteTo.File("hyperv-provisioning.log")
//            .CreateLogger();

//        try
//        {
//            var host = CreateHostBuilder(args).Build();

//            var rootCommand = new RootCommand("Hyper-V Cloud Image VM Provisioning Tool");

//            // Add main options
//            var vmNameOption = new Option<string>("--vm-name", () => "CloudVm", "Virtual machine name");
//            var vmGenerationOption = new Option<int>("--vm-generation", () => 2, "VM generation (1 or 2)");
//            var vmProcessorCountOption = new Option<int>("--vm-processor-count", () => 2, "Number of virtual processors");
//            var vmMemoryOption = new Option<string>("--vm-memory", () => "1GB", "VM memory (e.g., 1GB, 2GB)");
//            var vhdSizeOption = new Option<string>("--vhd-size", () => "40GB", "VHD size (e.g., 40GB, 60GB)");
//            var imageVersionOption = new Option<string>("--image-version", () => "22.04", "Ubuntu image version");
//            var forceOption = new Option<bool>("--force", () => false, "Force overwrite existing VM");
//            var verboseOption = new Option<bool>("--verbose", () => false, "Enable verbose logging");

//            rootCommand.AddOption(vmNameOption);
//            rootCommand.AddOption(vmGenerationOption);
//            rootCommand.AddOption(vmProcessorCountOption);
//            rootCommand.AddOption(vmMemoryOption);
//            rootCommand.AddOption(vhdSizeOption);
//            rootCommand.AddOption(imageVersionOption);
//            rootCommand.AddOption(forceOption);
//            rootCommand.AddOption(verboseOption);

//            rootCommand.SetHandler(async (context) =>
//            {
//                var options = new CommandLineOptions
//                {
//                    VMName = context.ParseResult.GetValueForOption(vmNameOption)!,
//                    VMGeneration = context.ParseResult.GetValueForOption(vmGenerationOption),
//                    VMProcessorCount = context.ParseResult.GetValueForOption(vmProcessorCountOption),
//                    VMMemory = context.ParseResult.GetValueForOption(vmMemoryOption)!,
//                    VHDSize = context.ParseResult.GetValueForOption(vhdSizeOption)!,
//                    ImageVersion = context.ParseResult.GetValueForOption(imageVersionOption)!,
//                    Force = context.ParseResult.GetValueForOption(forceOption),
//                    Verbose = context.ParseResult.GetValueForOption(verboseOption)
//                };

//                // Configure logging level
//                if (options.Verbose)
//                {
//                    Log.Logger = new LoggerConfiguration()
//                        .MinimumLevel.Debug()
//                        .WriteTo.Console()
//                        .WriteTo.File("hyperv-provisioning.log")
//                        .CreateLogger();
//                }

//                var provisioningService = host.Services.GetRequiredService<VirtualMachineProvisioningService>();
//                var success = await provisioningService.ProvisionVirtualMachineAsync(options);

//                context.ExitCode = success ? 0 : 1;
//            });

//            return await rootCommand.InvokeAsync(args);
//        }
//        catch (Exception ex)
//        {
//            Log.Fatal(ex, "Application terminated unexpectedly");
//            return 1;
//        }
//        finally
//        {
//            Log.CloseAndFlush();
//        }
//    }

//    static IHostBuilder CreateHostBuilder(string[] args) =>
//        Host.CreateDefaultBuilder(args)
//            .UseSerilog()
//            .ConfigureServices((hostContext, services) =>
//            {
//                // Register services
//                services.AddHttpClient();
//                services.AddScoped<PowerShellExecutor>();
//                services.AddScoped<ProcessExecutor>();
//                services.AddScoped<IHyperVService, HyperVService>();
//                services.AddScoped<ICloudImageService, CloudImageService>();
//                services.AddScoped<ICloudInitService, CloudInitService>();
//                services.AddScoped<VirtualMachineProvisioningService>();
//            });

//    private static ulong ParseSize(string sizeString)
//    {
//        var match = Regex.Match(sizeString.ToUpperInvariant(), @"^(\d+(?:\.\d+)?)\s*(B|KB|MB|GB|TB)?$");
//        if (!match.Success)
//        {
//            throw new ArgumentException($"Invalid size format: {sizeString}");
//        }

//        var value = double.Parse(match.Groups[1].Value);
//        var unit = match.Groups[2].Value;

//        var multiplier = unit switch
//        {
//            "B" or "" => 1UL,
//            "KB" => 1024UL,
//            "MB" => 1024UL * 1024UL,
//            "GB" => 1024UL * 1024UL * 1024UL,
//            "TB" => 1024UL * 1024UL * 1024UL * 1024UL,
//            _ => throw new ArgumentException($"Unknown size unit: {unit}")
//        };

//        return (ulong)(value * multiplier);
//    }
//}
