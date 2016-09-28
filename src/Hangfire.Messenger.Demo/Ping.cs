using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class Ping : IAsyncRequest
    {
    }

    public class PingHandler : IAsyncRequestHandler<Ping, Unit>
    {
        public async Task<Unit> Handle(Ping message, IMessenger messenger)
        {
            Console.WriteLine($"Ping on Thread #{Thread.CurrentThread.ManagedThreadId}");

            messenger.PublishToBackground(new Pong());

            return Unit.Value;
        }
    }
}
