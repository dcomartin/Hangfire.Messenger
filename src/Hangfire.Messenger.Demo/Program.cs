using System;
using System.Threading;
using System.Threading.Tasks;
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
            container.RegisterType<IRequestHandler<RequestWithResponse, string>, RequestWithResponseHandler>();
            container.RegisterType<INotificationHandler<Pong>, Pong1Handler>("HandlerForPong");
            container.RegisterType<INotificationHandler<Pong>, Pong2Handler>("HandlerForPongPong");
            
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
            GlobalConfiguration.Configuration.UseSqlServerStorage("Server=(LocalDB)\\MSSQLLocalDB;Database=HangfireMessage;Trusted_Connection=True;");

            Task.Run(async () =>
            {
                var messenger = GlobalConfiguration.Configuration
                    .UseMessaging(type => container.Resolve(type), type => container.ResolveAll(type));

                Console.WriteLine($"Main Thread #{Thread.CurrentThread.ManagedThreadId}");

                messenger.Enqueue(new Ping("Background"));
                await messenger.Send(new Ping("InProc"));

                var response = await messenger.Send(new RequestWithResponse("Michael Bolton"));
                Console.WriteLine($"Response: {response}");
            });


            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.WriteLine($"Hangfire Server started on Thread {Thread.CurrentThread.ManagedThreadId}.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}