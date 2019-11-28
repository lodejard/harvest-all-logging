using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace WheresLou.Logging.Watcher
{
    public class WatchingLoggerFactory : ILoggerFactory
    {
        WatchingLoggerProvider _provider;

        public WatchingLoggerFactory()
        {
            _provider = new WatchingLoggerProvider();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _provider.CreateLogger(categoryName);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}
