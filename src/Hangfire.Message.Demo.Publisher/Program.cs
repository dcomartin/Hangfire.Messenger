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
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using StructureMap;

namespace Hangfire.Message.Demo.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IAsyncRequestHandler<Ping, Unit>, PingHandler>();
            container.RegisterType<IAsyncNotificationHandler<Pong>, PongHandler>("HandlerForPong");
            container.RegisterType<IAsyncNotificationHandler<Pong>, PongPongHandler>("HandlerForPongPong");

            container.RegisterInstance<SingleInstanceFactory>(t => container.Resolve(t));
            container.RegisterInstance<MultiInstanceFactory>(t => container.ResolveAll(t));
            
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
            GlobalConfiguration.Configuration.UseSqlServerStorage("Server=(LocalDB)\\MSSQLLocalDB;Database=HangfireMessage;Trusted_Connection=True;");

            var mediator = GlobalConfiguration.Configuration.UseMessaging(container.Resolve<SingleInstanceFactory>(),
                container.Resolve<MultiInstanceFactory>());
            
            var ping = new Ping();
            mediator.Enqueue(ping);

            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.WriteLine($"Hangfire Server started on Thread {Thread.CurrentThread.ManagedThreadId}.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}