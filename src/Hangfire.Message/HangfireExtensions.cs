using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            configuration.UseActivator(new MessageJobActivator(mediator));
            return mediator;
        }
    }
}
