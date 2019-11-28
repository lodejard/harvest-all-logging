using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;

namespace WheresLou.Logging.Watcher
{
    public class WatchingEntry
    {
        public EntryLogType LogType { get; set; }
        public EntryLog Log { get; set; }
        public EntryScope Scope { get; set; }

        public class EntryLogType
        {
            public string CategoryName { get; set; }
            public LogLevel LogLevel { get; set; }
            public EventId EventId { get; set; }
            public string OriginalFormat { get; set; }
        }

        public class EntryLog
        {
            public string Message { get; set; }

            public ImmutableDictionary<string, object> Properties { get; set; }

            public ImmutableList<EntryException> Exception { get; set; }
        }

        public class EntryException
        {
            public string Type { get; set; }
            public string Message { get; set; }
            public string Stack { get; set; }

            public static ImmutableList<EntryException> From(Exception exception)
            {
                if (exception == null)
                {
                    return null;
                }

                var list = ImmutableList<EntryException>.Empty;
                for (var scan = exception; scan != null; scan = scan.InnerException)
                {
                    list = list.Add(new EntryException
                    {
                        Type = scan.GetType().FullName,
                        Message = scan.Message,
                        Stack = scan.StackTrace?.Replace("\r\n", "\n"),
                    });
                }
                return list;
            }
        }

        public class EntryScope
        {
            public ImmutableDictionary<string, object> Properties { get; set; }
            public IImmutableList<string> Levels { get; set; }

            public static EntryScope From(WatchingLoggerScope scope)
            {
                return scope == null ? null : new EntryScope
                {
                    Properties = scope.Properties,
                    Levels = scope.Levels
                };
            }

        }
    }
}
