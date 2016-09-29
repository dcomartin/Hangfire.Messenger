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
            container.RegisterType<IRequestHandler<Ping, Unit>, PingHandler>();
            container.RegisterType<INotificationHandler<Pong>, Pong1Handler>("HandlerForPong");
            container.RegisterType<INotificationHandler<Pong>, Pong2Handler>("HandlerForPongPong");
            
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
            GlobalConfiguration.Configuration.UseSqlServerStorage("Server=(LocalDB)\\MSSQLLocalDB;Database=HangfireMessage;Trusted_Connection=True;");

            var messenger = GlobalConfiguration.Configuration
                .UseMessaging(type => container.Resolve(type), type => container.ResolveAll(type));

            Console.WriteLine($"Main Thread #{Thread.CurrentThread.ManagedThreadId}");

            messenger.Enqueue(new Ping("Background"));
            messenger.Send(new Ping("InProc"));

            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.WriteLine($"Hangfire Server started on Thread {Thread.CurrentThread.ManagedThreadId}.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}