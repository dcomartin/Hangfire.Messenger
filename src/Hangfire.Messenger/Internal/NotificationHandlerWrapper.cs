using System;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Internal
{
    internal abstract class NotificationHandlerWrapper
    {
        public abstract Task Handle(INotification message);
        public abstract Type GetNotificationHandlerType();
    }

    internal class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> _inner;
        private readonly IMessenger _messenger;

        public NotificationHandlerWrapper(IMessenger messenger, INotificationHandler<TNotification> inner)
        {
            _inner = inner;
            _messenger = messenger;
        }

        public override Task Handle(INotification message)
        {
            return _inner.Handle((TNotification)message, _messenger);
        }

        public override Type GetNotificationHandlerType()
        {
            return _inner.GetType();
        }
    }
}