using Microsoft.Extensions.Logging;

namespace EPR.Payment.Facade.UnitTests
{
    public class TestLogger<T> : ILogger<T>
    {
        private readonly List<string> _logs = new List<string>();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            // Implementing the BeginScope method to return a valid IDisposable instance
            return NullScope.Instance;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            // You can customize the behavior for BeginScope here if needed
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            string message = formatter(state, exception);
            _logs.Add(message);
            Console.WriteLine(message);
        }

        public IEnumerable<string> Logs => _logs;

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}