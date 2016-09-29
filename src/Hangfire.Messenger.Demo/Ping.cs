using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class Ping : IRequest
    {
        public string Message { get; }

        public Ping(string message)
        {
            Message = message;
        }
    }

    public class PingHandler : RequestHandler<Ping>
    {
        protected override async Task HandleCore(Ping message, IMessenger messenger)
        {
            Console.WriteLine($"Ping {message.Message} on Thread #{Thread.CurrentThread.ManagedThreadId}");

            messenger.PublishToBackground(new Pong("Background"));
            await messenger.Publish(new Pong("InProc"));
        }
    }
}
