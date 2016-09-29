using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Hangfire.Common;
using Newtonsoft.Json;

namespace Hangfire.Messenger
{
    public static class HangfireExtensions
    {
        public static IMessenger UseMessaging(this IGlobalConfiguration configuration, Func<Type, object> singleInstanceFactory, Func<Type, IEnumerable<object>> multiInstanceFactory)
        {
            JobHelper.SetSerializerSettings(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

            var mediator = new Messenger(singleInstanceFactory, multiInstanceFactory);

            var regex = new Regex("[^a-zA-Z0-9_]");

            var serverName =
                regex.Replace(
                    ("msg_" + Environment.MachineName + "_" + Process.GetCurrentProcess().Id).ToLowerInvariant(),
                    string.Empty);

            var options = new BackgroundJobServerOptions
            {
                ServerName = serverName,
                Queues = new[] { "default", serverName },
                Activator = new MessageJobActivator(mediator)
            };

            new BackgroundJobServer(options);

            return mediator;
        }
    }
}
