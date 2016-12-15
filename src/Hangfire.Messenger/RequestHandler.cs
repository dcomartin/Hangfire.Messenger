using System.Threading.Tasks;

namespace Hangfire.Messenger
{
    /// <summary>
    /// Helper class for asynchronous requests that return a void response
    /// </summary>
    /// <typeparam name="TMessage">The type of void request being handled</typeparam>
    public abstract class RequestHandler<TMessage> : IRequestHandler<TMessage, NoResult>
        where TMessage : IRequest
    {
        public async Task<NoResult> Handle(TMessage message, IMessenger messenger)
        {
            await HandleCore(message, messenger).ConfigureAwait(false);
            return NoResult.Value;
        }

        /// <summary>
        /// Handles a void request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="messenger"></param>
        /// <returns>A task representing the void response from the request</returns>
        protected abstract Task HandleCore(TMessage message, IMessenger messenger);
    }
}
