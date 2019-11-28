using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace WheresLou.Logging.Watcher
{
    public class WatchingLogger : ILogger
    {
        private readonly WatchingLoggerProvider _provider;
        private readonly string _categoryName;

        public WatchingLogger(WatchingLoggerProvider provider, string categoryName)
        {
            _provider = provider;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _provider.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var stateProperties = state as IReadOnlyList<KeyValuePair<string, object>>;
            var logProperties = ImmutableDictionary<string, object>.Empty;
            string originalFormat = default;

            for (var index = 0; index < stateProperties?.Count; index++)
            {
                var property = stateProperties[index];
                if (string.Equals(property.Key, "{OriginalFormat}", StringComparison.Ordinal))
                {
                    originalFormat = property.Value?.ToString();
                }
                else
                {
                    if (IsAllowedType(property.Value))
                    {
                        logProperties = logProperties.Add(property.Key, property.Value);
                    }
                    else
                    {
                        logProperties = logProperties.Add(property.Key, Convert.ToString(property.Value));
                    }
                }
            }

            var entry = new WatchingEntry
            {
                LogType = new WatchingEntry.EntryLogType
                {
                    CategoryName = _categoryName,
                    LogLevel = logLevel,
                    EventId = eventId,
                    OriginalFormat = originalFormat,
                },
                Log = new WatchingEntry.EntryLog
                {
                    Message = formatter(state, exception),
                    Properties = logProperties,
                    Exception = WatchingEntry.EntryException.From(exception),
                },
                Scope = WatchingEntry.EntryScope.From(_provider.CurrentScope)
            };

            _provider.Write(entry);
        }

        private bool IsAllowedType(object value)
        {
            var type = value?.GetType();
            if (type == null ||
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(Guid) ||
                type == typeof(DateTimeOffset))
            {
                return true;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var itemType = i.GenericTypeArguments[0];
                        return IsAllowedType(itemType);
                    }
                }
                return false;
            }

            if (typeof(Type).IsAssignableFrom(type) ||
                typeof(MethodInfo).IsAssignableFrom(type))
            {
                return false;
            }

            if (string.Equals(type.FullName, "Microsoft.AspNetCore.Routing.RouteValuesAddress", StringComparison.Ordinal))
            {
                return false;
            }

            return false;
        }
    }
}
