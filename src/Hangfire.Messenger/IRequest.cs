namespace Hangfire.Messenger
{
    /// <summary>
    /// Marker interface to represent an asynchronous request with a void response
    /// </summary>
    public interface IRequest : IRequest<Unit> { }

    /// <summary>
    /// Marker interface to represent an asynchronous request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequest<out TResponse> { }
}