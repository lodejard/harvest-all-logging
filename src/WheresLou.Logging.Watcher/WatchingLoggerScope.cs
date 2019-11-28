using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace WheresLou.Logging.Watcher
{
    public class WatchingLoggerScope : IDisposable
    {
        private readonly WatchingLoggerProvider _provider;

        public WatchingLoggerScope(WatchingLoggerProvider provider, WatchingLoggerScope parent, ImmutableDictionary<string, object> properties, ImmutableList<string> levels)
        {
            _provider = provider;
            Parent = parent;
            Properties = properties;
            Levels = levels;
        }

        public WatchingLoggerScope Parent { get; }

        public ImmutableDictionary<string, object> Properties { get; }

        public ImmutableList<string> Levels { get; }

        public void Dispose()
        {
            _provider.EndScope(this);
        }

        internal static WatchingLoggerScope Create<TState>(WatchingLoggerProvider provider, WatchingLoggerScope parent, TState state)
        {
            var properties = parent?.Properties ?? ImmutableDictionary<string, object>.Empty;
            var levels = parent?.Levels ?? ImmutableList<string>.Empty;

            if (state is IReadOnlyList<KeyValuePair<string, object>> list)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    properties = properties.Add(list[index].Key, list[index].Value);
                }
            }
            levels = levels.Add(state.ToString());

            return new WatchingLoggerScope(provider, parent, properties, levels);
        }
    }
}
