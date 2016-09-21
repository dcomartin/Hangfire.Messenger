using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Hangfire;
using Hangfire.Message;
using MediatR.Internal;

namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
    /// </summary>
    public class Mediator : IMediator, IDisposable
    {
        private readonly SingleInstanceFactory _singleInstanceFactory;
        private readonly MultiInstanceFactory _multiInstanceFactory;
        private readonly ConcurrentDictionary<Type, Type> _genericHandlerCache;
        private readonly ConcurrentDictionary<Type, Type> _wrapperHandlerCache;
        private BackgroundJobServer _backgroundJobServer;

        public Mediator(SingleInstanceFactory singleInstanceFactory, MultiInstanceFactory multiInstanceFactory)
        {
            _singleInstanceFactory = singleInstanceFactory;
            _multiInstanceFactory = multiInstanceFactory;
            _genericHandlerCache = new ConcurrentDictionary<Type, Type>();
            _wrapperHandlerCache = new ConcurrentDictionary<Type, Type>();
        }

        public void Enqueue(IAsyncRequest request)
        {
            BackgroundJob.Enqueue<Mediator>(m => m.ProcessRequestInBackground(request.GetType().FullName, "default", request));
        }

        public void PublishEnqueue(IAsyncNotification notification)
        {
            var regex = new Regex("[^a-zA-Z0-9_]");
            
            var servers = Hangfire.JobStorage.Current.GetMonitoringApi().Servers();
            foreach (var server in servers)
            {
                var nameParts = server.Name.Split(':');
                var serverName = nameParts[0];

                var queueName = regex.Replace(serverName.ToLowerInvariant(), string.Empty);
                var notificationHandlers = GetNotificationHandlers(notification).ToArray();
                foreach (var handler in notificationHandlers)
                {
                    BackgroundJob.Enqueue<Mediator>(
                        m =>
                            m.ProcessNotificationInBackground(handler.GetNotificationHandlerType().FullName, queueName,
                                handler.GetNotificationHandlerType(), notification));

                }
            }
        }

        [UseQueueFromParameter(1)]
        [DisplayName("{0}")]
        public void ProcessRequestInBackground(string jobName, string queueName, IAsyncRequest request)
        {
            SendAsync(request).Wait();
        }

        [DisplayName("{0}")]
        [UseQueueFromParameter(1)]
        public void ProcessNotificationInBackground(string jobName, string queueName, Type handler, IAsyncNotification notification)
        {
            PublishAsync(notification, handler).Wait();
        }
        
        public Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            var defaultHandler = GetHandler(request);

            var result = defaultHandler.Handle(request);

            return result;
        }

        public Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var defaultHandler = GetHandler(request);

            var result = defaultHandler.Handle(request, cancellationToken);

            return result;
        }

        public Task PublishAsync(IAsyncNotification notification)
        {
            var notificationHandlers = GetNotificationHandlers(notification)
                .Select(handler => handler.Handle(notification))
                .ToArray();

            return Task.WhenAll(notificationHandlers);
        }

        public async Task PublishAsync(IAsyncNotification notification, Type notificationHandlerType)
        {
            var notificationHandlers =
                GetNotificationHandlers(notification)
                    .Where(x => x.GetNotificationHandlerType().FullName == notificationHandlerType.FullName)
                    .Select(handler => handler.Handle(notification))
                    .ToArray();

            await Task.WhenAll(notificationHandlers);
        }

        public Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken)
        {
            var notificationHandlers = GetNotificationHandlers(notification)
                .Select(handler => handler.Handle(notification, cancellationToken))
                .ToArray();

            return Task.WhenAll(notificationHandlers);
        }

        private AsyncRequestHandlerWrapper<TResponse> GetHandler<TResponse>(IAsyncRequest<TResponse> request)
        {
            return GetHandler<AsyncRequestHandlerWrapper<TResponse>, TResponse>(request,
                typeof(IAsyncRequestHandler<,>),
                typeof(AsyncRequestHandlerWrapper<,>));
        }

        private CancellableAsyncRequestHandlerWrapper<TResponse> GetHandler<TResponse>(ICancellableAsyncRequest<TResponse> request)
        {
            return GetHandler<CancellableAsyncRequestHandlerWrapper<TResponse>, TResponse>(request,
                typeof(ICancellableAsyncRequestHandler<,>),
                typeof(CancellableAsyncRequestHandlerWrapper<,>));
        }

        private TWrapper GetHandler<TWrapper, TResponse>(object request, Type handlerType, Type wrapperType)
        {
            var requestType = request.GetType();

            var genericHandlerType = _genericHandlerCache.GetOrAdd(requestType, handlerType, (type, root) => root.MakeGenericType(type, typeof(TResponse)));
            var genericWrapperType = _wrapperHandlerCache.GetOrAdd(requestType, wrapperType, (type, root) => root.MakeGenericType(type, typeof(TResponse)));

            var handler = GetHandler(request, genericHandlerType);

            return (TWrapper) Activator.CreateInstance(genericWrapperType, this, handler);
        }

        private object GetHandler(object request, Type handlerType)
        {
            try
            {
                return _singleInstanceFactory(handlerType);
            }
            catch (Exception e)
            {
                throw BuildException(request, e);
            }
        }

        private IEnumerable<AsyncNotificationHandlerWrapper> GetNotificationHandlers(IAsyncNotification notification)
        {
            return GetNotificationHandlers<AsyncNotificationHandlerWrapper>(notification,
                typeof(IAsyncNotificationHandler<>),
                typeof(AsyncNotificationHandlerWrapper<>));
        }

        private IEnumerable<CancellableAsyncNotificationHandlerWrapper> GetNotificationHandlers(ICancellableAsyncNotification notification)
        {
            return GetNotificationHandlers<CancellableAsyncNotificationHandlerWrapper>(notification,
                typeof (ICancellableAsyncNotificationHandler<>),
                typeof(CancellableAsyncNotificationHandlerWrapper<>));
        }

        private IEnumerable<TWrapper> GetNotificationHandlers<TWrapper>(object notification, Type handlerType, Type wrapperType)
        {
            var notificationType = notification.GetType();

            var genericHandlerType = _genericHandlerCache.GetOrAdd(notificationType, handlerType, (type, root) => root.MakeGenericType(type));
            var genericWrapperType = _wrapperHandlerCache.GetOrAdd(notificationType, wrapperType, (type, root) => root.MakeGenericType(type));

            return GetNotificationHandlers(notification, genericHandlerType)
                .Select(handler => Activator.CreateInstance(genericWrapperType, handler))
                .Cast<TWrapper>()
                .ToList();
        }

        private IEnumerable<object> GetNotificationHandlers(object notification, Type handlerType)
        {
            try
            {
                return _multiInstanceFactory(handlerType);
            }
            catch (Exception e)
            {
                throw BuildException(notification, e);
            }
        }

        private static InvalidOperationException BuildException(object message, Exception inner)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.", inner);
        }

        public void Dispose()
        {
            _backgroundJobServer?.Dispose();
        }
    }
}
