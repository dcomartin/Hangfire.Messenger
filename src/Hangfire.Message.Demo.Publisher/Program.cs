using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.Practices.Unity;
using StructureMap;

namespace Hangfire.Message.Demo.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterTypes(AllClasses.FromAssemblies(typeof(Ping).Assembly), WithMappings.FromAllInterfaces, GetName, GetLifetimeManager);
            container.RegisterInstance<SingleInstanceFactory>(t => container.Resolve(t));
            container.RegisterInstance<MultiInstanceFactory>(t => container.ResolveAll(t));
            
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
            GlobalConfiguration.Configuration.UseMemoryStorage();

            var mediator = GlobalConfiguration.Configuration.UseMessaging(container.Resolve<SingleInstanceFactory>(),
                container.Resolve<MultiInstanceFactory>());

            var ping = new Ping();
            mediator.Enqueue(ping);
            mediator.Enqueue(ping);
            mediator.Enqueue(ping);
            mediator.Enqueue(ping);
            mediator.Enqueue(ping);

            var hangfire = new BackgroundJobServer();
            Console.WriteLine($"Hangfire Server started on Thread {Thread.CurrentThread.ManagedThreadId}.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static bool IsNotificationHandler(Type type)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(INotificationHandler<>) || x.GetGenericTypeDefinition() == typeof(IAsyncNotificationHandler<>)));
        }

        static LifetimeManager GetLifetimeManager(Type type)
        {
            return IsNotificationHandler(type) ? new ContainerControlledLifetimeManager() : null;
        }

        static string GetName(Type type)
        {
            return IsNotificationHandler(type) ? string.Format("HandlerFor" + type.Name) : string.Empty;
        }
    }
}
