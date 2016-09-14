using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class AsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IAsyncRequest<TResult> message);
    }

    internal class AsyncRequestHandlerWrapper<TCommand, TResult> : AsyncRequestHandlerWrapper<TResult>
        where TCommand : IAsyncRequest<TResult>
    {
        private readonly IMediator _mediator;
        private readonly IAsyncRequestHandler<TCommand, TResult> _inner;

        public AsyncRequestHandlerWrapper(IMediator mediator, IAsyncRequestHandler<TCommand, TResult> inner)
        {
            _mediator = mediator;
            _inner = inner;
        }

        public override Task<TResult> Handle(IAsyncRequest<TResult> message)
        {
            return _inner.Handle((TCommand)message, _mediator);
        }
    }
}