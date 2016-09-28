using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class Pong : IAsyncNotification
    {
    }

    public class Pong1Handler : IAsyncNotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong2 on Thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }

    public class Pong2Handler : IAsyncNotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong2 on Thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
