using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace WheresLou.Logging.Watcher
{
    public class WatchingLoggerProvider : ILoggerProvider
    {
        private readonly Dictionary<string, WatchingLogger> _loggers = new Dictionary<string, WatchingLogger>(StringComparer.Ordinal);
        private readonly object _loggersLock = new object();
        private readonly AsyncLocal<WatchingLoggerScope> _currentScope = new AsyncLocal<WatchingLoggerScope>();
        private readonly List<WatchingEntry> _entries = new List<WatchingEntry>();

        public WatchingLoggerScope CurrentScope => _currentScope.Value;

        public ILogger CreateLogger(string categoryName)
        {
            lock (_loggersLock)
            {
                if (!_loggers.TryGetValue(categoryName, out var logger))
                {
                    logger = new WatchingLogger(this, categoryName);
                    _loggers[categoryName] = logger;
                }
                return logger;
            }
        }

        internal IDisposable BeginScope<TState>(TState state)
        {
            var scope = WatchingLoggerScope.Create(this, _currentScope.Value, state);
            _currentScope.Value = scope;
            return scope;
        }

        internal void EndScope(WatchingLoggerScope scope)
        {
            // this is the correct condition
            if (_currentScope.Value == scope)
            {
                _currentScope.Value = scope.Parent;
                return;
            }

            // this is the incorrect condition - scopes ending out of order
            // scan to see if this scope is a parent of the current scope
            for (var scan = _currentScope.Value; scan != null; scan = scan.Parent)
            {
                if (scan == scope)
                {
                    // this is an early dispose. allow it to take effect.
                    _currentScope.Value = scope.Parent;
                    return;
                }
            }

        }

        public void Dispose()
        {
            var serializer = new SerializerBuilder()
                .DisableAliases()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            using(var writer = File.CreateText("log.yaml"))
            {
                serializer.Serialize(writer, _entries);
            }
        }

        public void Write(WatchingEntry entry)
        {
            _entries.Add(entry);
        }
    }
}
