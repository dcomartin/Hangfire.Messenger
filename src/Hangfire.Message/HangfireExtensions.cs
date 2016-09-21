using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire.Common;
using MediatR;
using Newtonsoft.Json;

namespace Hangfire.Message
{
    public static class HangfireExtensions
    {
        public static IMediator UseMessaging(this IGlobalConfiguration configuration, SingleInstanceFactory singleInstanceFactory, MultiInstanceFactory multiInstanceFactory)
        {
            JobHelper.SetSerializerSettings(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

            var mediator = new Mediator(singleInstanceFactory, multiInstanceFactory);

            var regex = new Regex("[^a-zA-Z0-9_]");

            var serverName =
                regex.Replace(
                    ("msg_" + Environment.MachineName + "_" + Process.GetCurrentProcess().Id).ToLowerInvariant(),
                    string.Empty);

            var options = new BackgroundJobServerOptions
            {
                ServerName = serverName,
                Queues = new string[] { "default", serverName },
                Activator = new MessageJobActivator(mediator)
            };

            new BackgroundJobServer(options);

            return mediator;
        }
    }
}
