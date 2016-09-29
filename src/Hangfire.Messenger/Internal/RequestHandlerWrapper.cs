using System.Threading.Tasks;

namespace Hangfire.Messenger.Internal
{
    internal abstract class RequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IRequest<TResult> message);
    }

    internal class RequestHandlerWrapper<TCommand, TResult> : RequestHandlerWrapper<TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IMessenger _messenger;
        private readonly IRequestHandler<TCommand, TResult> _inner;

        public RequestHandlerWrapper(IMessenger messenger, IRequestHandler<TCommand, TResult> inner)
        {
            _messenger = messenger;
            _inner = inner;
        }

        public override Task<TResult> Handle(IRequest<TResult> message)
        {
            return _inner.Handle((TCommand)message, _messenger);
        }
    }
}