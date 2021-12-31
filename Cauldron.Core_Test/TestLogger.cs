using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    class TestLogger : ILogger
    {
        private readonly ITestOutputHelper output;

        public TestLogger()
        {
        }

        public TestLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.output?.WriteLine(formatter?.Invoke(state, exception) ?? "");
            return;
        }
    }
}
