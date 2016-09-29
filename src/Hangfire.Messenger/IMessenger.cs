using System.Threading.Tasks;

namespace Hangfire.Messenger
{
    /// <summary>
    /// Defines a messenger to encapsulate request/response and publishing interaction patterns
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Asynchronously send a request to a single handler 
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
        
        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task Publish(INotification notification);
        
        /// <summary>
        /// Send a notification and run each handler in a background process/thread.
        /// </summary>
        /// <param name="notification">Notification object</param>
        void PublishToBackground(INotification notification);

        /// <summary>
        /// Send a request to be executed by a handler in a background process/thread.
        /// </summary>
        /// <param name="request"></param>
        void Enqueue(IRequest request);
    }
}