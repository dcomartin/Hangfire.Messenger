using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Messenger;

namespace Hangfire.Message
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
