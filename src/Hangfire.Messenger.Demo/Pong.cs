using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class Pong : INotification
    {
        public string Message { get; }

        public Pong(string message)
        {
            Message = message;
        }
    }

    public class Pong1Handler : INotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong1 {notification.Message} on Thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }

    public class Pong2Handler : INotificationHandler<Pong>
    {
        public async Task Handle(Pong notification)
        {
            Console.WriteLine($"Pong2 {notification.Message} on Thread #{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
