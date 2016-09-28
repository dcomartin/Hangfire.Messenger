using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class Ping : IAsyncRequest
    {
        public string Message { get; }

        public Ping(string message)
        {
            Message = message;
        }
    }

    public class PingHandler : IAsyncRequestHandler<Ping, Unit>
    {
        public async Task<Unit> Handle(Ping message, IMessenger messenger)
        {
            Console.WriteLine($"Ping {message.Message} on Thread #{Thread.CurrentThread.ManagedThreadId}");

            messenger.PublishToBackground(new Pong("Background"));
            await messenger.Publish(new Pong("InProc"));
            return Unit.Value;
        }
    }
}
