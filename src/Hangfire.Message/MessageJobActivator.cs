using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Hangfire.Message
{
    public class MessageJobActivator : JobActivator
    {
        private readonly IMediator _mediator;

        public MessageJobActivator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override object ActivateJob(Type type)
        {
            return _mediator;
        }
    }
}
