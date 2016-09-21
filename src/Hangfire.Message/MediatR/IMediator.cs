using System.Threading;
using System.Threading.Tasks;
using Hangfire;

namespace MediatR
{
    /// <summary>
    /// Defines a mediator to encapsulate request/response and publishing interaction patterns
    /// </summary>
    public interface IMediator
    {
        void Enqueue(IAsyncRequest request);
        /// <summary>
        /// Asynchronously send a request to a single handler 
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request);

        /// <summary>
        /// Asynchronously send a cancellable request to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken);
        
        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(IAsyncNotification notification);

        void PublishEnqueue(IAsyncNotification notification);

        /// <summary>
        /// Asynchronously send a cancellable notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken);
    }
}