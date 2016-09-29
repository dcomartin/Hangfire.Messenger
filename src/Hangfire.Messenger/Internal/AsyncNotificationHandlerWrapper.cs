using System;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Internal
{
    internal abstract class AsyncNotificationHandlerWrapper
    {
        public abstract Task Handle(INotification message);
        public abstract Type GetNotificationHandlerType();
    }

    internal class AsyncNotificationHandlerWrapper<TNotification> : AsyncNotificationHandlerWrapper
        where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> _inner;

        public AsyncNotificationHandlerWrapper(INotificationHandler<TNotification> inner)
        {
            _inner = inner;
        }

        public override Task Handle(INotification message)
        {
            return _inner.Handle((TNotification)message);
        }

        public override Type GetNotificationHandlerType()
        {
            return _inner.GetType();
        }
    }
}