using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Tests.Integration;

public static class TestLogger
{
    public static ILogger<T> Create<T>(LogLevel minLevel = LogLevel.Trace)
    {
        var logger = new NUnitLogger<T>(minLevel);
        return logger;
    }

    private class NUnitLogger<T> : ILogger<T>, IDisposable
    {
        private readonly LogLevel _minLevel;
        private readonly Action<string> output = Console.WriteLine;

        public NUnitLogger(LogLevel minLevel = LogLevel.Trace)
        {
            _minLevel = minLevel;
        }

        public void Dispose()
        {
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (logLevel >= _minLevel)
            {
                output($"{DateTime.UtcNow:s} | {logLevel} | {formatter(state, exception)}");
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => this;
    }
}