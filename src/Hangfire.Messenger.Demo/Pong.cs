using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class Pong : IAsyncNotification
    {
        public string Message { get; }

        public Pong(string message)
        {
            Message = message;
        }
    }

    public class Pong1Handler : IAsyncNotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong1 {notification.Message} on Thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }

    public class Pong2Handler : IAsyncNotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong2 {notification.Message} on Thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
