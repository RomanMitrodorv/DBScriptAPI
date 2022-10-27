using DBScriptDeployment;
using Microsoft.AspNetCore;
using Serilog;

var configuration = GetConfiguration();

Log.Logger = GetLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", Program.Namespace);
    var host = CreateHostBuilder(configuration, args);

    Log.Information("Starting web host ({ApplicationContext})...", Program.Namespace);
    host.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.Namespace);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}


IWebHost CreateHostBuilder(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
        .CaptureStartupErrors(false)
        .ConfigureKestrel((a, b) => { })
        .UseStartup<Startup>()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseSerilog()
        .Build();



IConfiguration GetConfiguration()
{
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
        .AddJsonFile("appsettings.Development.json", false, true)
#endif
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .Build();
}

Serilog.ILogger GetLogger(IConfiguration configuration)
{
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithEnvironmentName()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationContext", Namespace)
        .WriteTo.Console()

        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}




public partial class Program
{

    public static string Namespace = typeof(Startup).Namespace;
}
