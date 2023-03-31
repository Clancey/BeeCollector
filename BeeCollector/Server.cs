using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Server;

public class Server
{
	public static async Task<MqttServer> Run_Minimal_Server()
	{
		/*
         * This sample starts a simple MQTT server which will accept any TCP connection.
         */

		var mqttFactory = new MqttFactory(new ConsoleLogger());

		// The port for the default endpoint is 1883.
		// The default endpoint is NOT encrypted!
		// Use the builder classes where possible.
		var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

		// The port can be changed using the following API (not used in this example).
		// new MqttServerOptionsBuilder()
		//     .WithDefaultEndpoint()
		//     .WithDefaultEndpointPort(1234)
		//     .Build();

		var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
		await mqttServer.StartAsync();
		return mqttServer;

	}
	class ConsoleLogger : IMqttNetLogger
	{
		readonly object _consoleSyncRoot = new();

		public bool IsEnabled => true;

		public void Publish(MqttNetLogLevel logLevel, string source, string message, object[]? parameters, Exception? exception)
		{
			if (parameters?.Length > 0)
			{
				message = string.Format(message, parameters);
			}
			var foregroundColor = ConsoleColor.White;
			switch (logLevel)
			{
				case MqttNetLogLevel.Verbose:
					foregroundColor = ConsoleColor.White;
					Logger.SharedLogger.LogTrace(message);
					break;

				case MqttNetLogLevel.Info:
					foregroundColor = ConsoleColor.Green;
					Logger.SharedLogger.LogInformation(message);
					break;

				case MqttNetLogLevel.Warning:
					foregroundColor = ConsoleColor.DarkYellow;
					Logger.SharedLogger.LogWarning(message);
					break;

				case MqttNetLogLevel.Error:
					foregroundColor = ConsoleColor.Red;
					Logger.SharedLogger.LogError(message);
					break;
			}


			lock (_consoleSyncRoot)
			{
				Console.ForegroundColor = foregroundColor;
				Console.WriteLine(message);

				if (exception != null)
				{
					Console.WriteLine(exception);
				}
			}
		}
	}
}