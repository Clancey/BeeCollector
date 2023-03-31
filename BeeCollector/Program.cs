using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.Internal;

using var channel = new InMemoryChannel();

async Task Run()
{
	Console.WriteLine("Hello, World!");

	var server = await Server.Run_Minimal_Server();
	server.InterceptingPublishAsync += args =>
	{
		var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
		var topic = args.ApplicationMessage.Topic;
		Database.Shared.ProcessMessage(topic, payload);
		return CompletedTask.Instance;
	};

	Console.WriteLine("Press Enter to exit.");
	Console.ReadLine();
	await server.StopAsync();
}


// The following code is required to ensure that all telemetry is sent to the back end.

try
{
	IServiceCollection services = new ServiceCollection();
	services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = channel);
	services.AddLogging(builder =>
	{
		// Only Application Insights is registered as a logger provider
		builder.AddApplicationInsights(
			configureTelemetryConfiguration: (config) => config.ConnectionString = "InstrumentationKey=bdfcdb3b-6983-4634-ba84-3b4cf8af51a2;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/;LiveEndpoint=https://westus2.livediagnostics.monitor.azure.com/",
			configureApplicationInsightsLoggerOptions: (options) => { }
		);
	});

	IServiceProvider serviceProvider = services.BuildServiceProvider();
	ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();
	Logger.SharedLogger = logger;
	logger.LogInformation("Logger is working...");

	await Run();
}
finally
{
	// Explicitly call Flush() followed by Delay, as required in console apps.
	// This ensures that even if the application terminates, telemetry is sent to the back end.
	channel.Flush();

	await Task.Delay(TimeSpan.FromMilliseconds(1000));
}
