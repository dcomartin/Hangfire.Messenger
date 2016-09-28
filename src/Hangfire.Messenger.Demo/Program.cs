using System;
using System.Threading;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;

namespace Hangfire.Messenger.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IAsyncRequestHandler<Ping, Unit>, PingHandler>();
            container.RegisterType<IAsyncNotificationHandler<Pong>, Pong1Handler>("HandlerForPong");
            container.RegisterType<IAsyncNotificationHandler<Pong>, Pong2Handler>("HandlerForPongPong");
            
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
            GlobalConfiguration.Configuration.UseSqlServerStorage("Server=(LocalDB)\\MSSQLLocalDB;Database=HangfireMessage;Trusted_Connection=True;");

            var mediator = GlobalConfiguration.Configuration
                .UseMessaging(type => container.Resolve(type), type => container.ResolveAll(type));

            Console.WriteLine($"Main Thread #{Thread.CurrentThread.ManagedThreadId}");

            mediator.Enqueue(new Ping("Background"));
            mediator.Send(new Ping("InProc"));

            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.WriteLine($"Hangfire Server started on Thread {Thread.CurrentThread.ManagedThreadId}.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}