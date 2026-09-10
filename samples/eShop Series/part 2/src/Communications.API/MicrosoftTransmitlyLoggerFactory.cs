using System.Collections;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using TransmitlyLogEntry = Transmitly.Logging.LogEntry;
using TransmitlyLogLevel = Transmitly.Logging.LogLevel;

namespace eShop.Communications.API;

/// <summary>
/// Routes Transmitly's structured log entries through the host logging pipeline.
/// The eShop service defaults attach OpenTelemetry to that pipeline.
/// </summary>
internal sealed class MicrosoftTransmitlyLoggerFactory : Transmitly.Logging.ILoggerFactory
{
    private Microsoft.Extensions.Logging.ILoggerFactory? _loggerFactory;

    public void UseLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        if (Interlocked.CompareExchange(ref _loggerFactory, loggerFactory, null) is not null)
        {
            throw new InvalidOperationException("The host logger factory has already been attached.");
        }
    }

    public Transmitly.Logging.ILogger CreateLogger(string categoryName) =>
        new MicrosoftTransmitlyLogger(this, categoryName);

    private Microsoft.Extensions.Logging.ILogger? CreateMicrosoftLogger(string categoryName) =>
        Volatile.Read(ref _loggerFactory)?.CreateLogger(categoryName);

    private sealed class MicrosoftTransmitlyLogger(
        MicrosoftTransmitlyLoggerFactory loggerFactory,
        string categoryName) : Transmitly.Logging.ILogger
    {
        private Microsoft.Extensions.Logging.ILogger? _logger;

        public string CategoryName { get; } = categoryName;

        public bool IsEnabled(TransmitlyLogLevel level) =>
            GetLogger()?.IsEnabled(ToMicrosoftLogLevel(level)) == true;

        public void Log(TransmitlyLogEntry entry)
        {
            var logger = GetLogger();
            var level = ToMicrosoftLogLevel(entry.Level);
            if (logger is null || !logger.IsEnabled(level))
            {
                return;
            }

            logger.Log(
                level,
                new Microsoft.Extensions.Logging.EventId(0, entry.EventName),
                new TransmitlyLogState(entry),
                entry.Exception,
                static (state, _) => state.Message);
        }

        private Microsoft.Extensions.Logging.ILogger? GetLogger()
        {
            var logger = Volatile.Read(ref _logger);
            if (logger is not null)
            {
                return logger;
            }

            logger = loggerFactory.CreateMicrosoftLogger(CategoryName);
            if (logger is null)
            {
                return null;
            }

            Interlocked.CompareExchange(ref _logger, logger, null);
            return _logger;
        }
    }

    private sealed class TransmitlyLogState : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly KeyValuePair<string, object?>[] _properties;

        public TransmitlyLogState(TransmitlyLogEntry entry)
        {
            Message = entry.Message;
            _properties = new KeyValuePair<string, object?>[entry.Properties.Count + 1];

            var index = 0;
            foreach (var property in entry.Properties)
            {
                _properties[index++] = property;
            }

            // Logging providers use this well-known property for the message template.
            _properties[index] = new("{OriginalFormat}", entry.Message);
        }

        public string Message { get; }

        public int Count => _properties.Length;

        public KeyValuePair<string, object?> this[int index] => _properties[index];

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
            ((IEnumerable<KeyValuePair<string, object?>>)_properties).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _properties.GetEnumerator();
    }

    private static MicrosoftLogLevel ToMicrosoftLogLevel(TransmitlyLogLevel level) => level switch
    {
        TransmitlyLogLevel.Trace => MicrosoftLogLevel.Trace,
        TransmitlyLogLevel.Debug => MicrosoftLogLevel.Debug,
        TransmitlyLogLevel.Information => MicrosoftLogLevel.Information,
        TransmitlyLogLevel.Warning => MicrosoftLogLevel.Warning,
        TransmitlyLogLevel.Error => MicrosoftLogLevel.Error,
        TransmitlyLogLevel.Critical => MicrosoftLogLevel.Critical,
        _ => MicrosoftLogLevel.None
    };
}
