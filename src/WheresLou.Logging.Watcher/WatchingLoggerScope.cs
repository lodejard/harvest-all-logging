using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace WheresLou.Logging.Watcher
{
    public class WatchingLoggerScope : IDisposable
    {
        private readonly WatchingLoggerProvider _provider;

        public WatchingLoggerScope(WatchingLoggerProvider provider, WatchingLoggerScope parent, ImmutableDictionary<string, object> properties, ImmutableList<string> levels, string originalFormat)
        {
            _provider = provider;
            Parent = parent;
            Properties = properties;
            Levels = levels;
            OriginalFormat = originalFormat;
        }

        public WatchingLoggerScope Parent { get; }

        public ImmutableDictionary<string, object> Properties { get; }

        public ImmutableList<string> Levels { get; }

        public string OriginalFormat { get; }

        public void Dispose()
        {
            _provider.EndScope(this);
        }

        internal static WatchingLoggerScope Create<TState>(WatchingLoggerProvider provider, WatchingLoggerScope parent, TState state)
        {
            var properties = parent?.Properties ?? ImmutableDictionary<string, object>.Empty;
            var levels = parent?.Levels ?? ImmutableList<string>.Empty;
            string originalFormat = default;

            if (state is IReadOnlyList<KeyValuePair<string, object>> list)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var property = list[index];
                    if (string.Equals(property.Key, "{OriginalFormat}", StringComparison.Ordinal))
                    {
                        originalFormat = property.Value?.ToString();
                    }
                    else
                    {
                        properties = properties.Add(list[index].Key, list[index].Value);
                    }
                }
            }
            levels = levels.Add(state.ToString());

            return new WatchingLoggerScope(provider, parent, properties, levels, originalFormat);
        }
    }
}
