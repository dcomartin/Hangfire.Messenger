using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Hangfire.Message.Demo.Publisher
{
    public class Pong : IAsyncNotification
    {
    }

    public class PongHandler : IAsyncNotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong on {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
