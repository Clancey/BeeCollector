using Microsoft.Extensions.Logging;

public class Logger : ILogger
{
	internal static ILogger<Program>? SharedLogger { get; set; }
	public IDisposable BeginScope<TState>(TState state) => SharedLogger!.BeginScope(state);

	public bool IsEnabled(LogLevel logLevel) => SharedLogger?.IsEnabled(logLevel) ?? false;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
	 SharedLogger?.Log(logLevel, eventId, state, exception, formatter);
}
