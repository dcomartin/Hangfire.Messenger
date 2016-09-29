using System;

namespace Hangfire.Messenger
{
    public class MessageJobActivator : JobActivator
    {
        private readonly IMessenger _messenger;

        public MessageJobActivator(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public override object ActivateJob(Type type)
        {
            return _messenger;
        }
    }
}
