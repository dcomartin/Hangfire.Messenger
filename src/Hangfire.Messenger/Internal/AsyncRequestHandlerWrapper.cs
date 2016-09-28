using System.Threading.Tasks;

namespace Hangfire.Messenger.Internal
{
    internal abstract class AsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IAsyncRequest<TResult> message);
    }

    internal class AsyncRequestHandlerWrapper<TCommand, TResult> : AsyncRequestHandlerWrapper<TResult>
        where TCommand : IAsyncRequest<TResult>
    {
        private readonly IMessenger _messenger;
        private readonly IAsyncRequestHandler<TCommand, TResult> _inner;

        public AsyncRequestHandlerWrapper(IMessenger messenger, IAsyncRequestHandler<TCommand, TResult> inner)
        {
            _messenger = messenger;
            _inner = inner;
        }

        public override Task<TResult> Handle(IAsyncRequest<TResult> message)
        {
            return _inner.Handle((TCommand)message, _messenger);
        }
    }
}