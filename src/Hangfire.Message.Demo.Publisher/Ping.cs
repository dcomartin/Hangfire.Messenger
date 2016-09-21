using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Hangfire.Message.Demo.Publisher
{
    public class Ping : IAsyncRequest
    {
    }

    public class PingHandler : IAsyncRequestHandler<Ping, Unit>
    {
        public async Task<Unit> Handle(Ping message, IMediator mediator)
        {
            Console.WriteLine($"Ping on Thread #{Thread.CurrentThread.ManagedThreadId}");

            mediator.PublishEnqueue(new Pong());

            return Unit.Value;
        }
    }
}
