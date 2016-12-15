using System.ComponentModel;
using System.Text.RegularExpressions;
using Hangfire.Messenger.Internal;
using Hangfire.Server;

namespace Hangfire.Messenger
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    public class Messenger : IMessenger, IDisposable
    {
        private readonly Func<Type, object> _commandHandlerFactory;
        private readonly Func<Type, IEnumerable<object>> _notificationHandlerFactory;
        private readonly ConcurrentDictionary<Type, Type> _genericHandlerCache;
        private readonly ConcurrentDictionary<Type, Type> _wrapperHandlerCache;
        public PerformContext PerformContext { get; set; }

        public Messenger(Func<Type, object> commandHandlerFactory, Func<Type, IEnumerable<object>> notificationHandlerFactory)
        {
            _commandHandlerFactory = commandHandlerFactory;
            _notificationHandlerFactory = notificationHandlerFactory;
            _genericHandlerCache = new ConcurrentDictionary<Type, Type>();
            _wrapperHandlerCache = new ConcurrentDictionary<Type, Type>();
        }

        public void Enqueue(IRequest request)
        {
            BackgroundJob.Enqueue<Messenger>(m => m.DequeueRequest(null, request.GetType().FullName, "default", request));
        }

        public void PublishToBackground(INotification notification)
        {
            var regex = new Regex("[^a-zA-Z0-9_]");
            
            var servers = JobStorage.Current.GetMonitoringApi().Servers();
            foreach (var server in servers)
            {
                var nameParts = server.Name.Split(':');
                var serverName = nameParts[0];

                var queueName = regex.Replace(serverName.ToLowerInvariant(), string.Empty);
                var notificationHandlers = GetNotificationHandlers(notification).ToArray();
                foreach (var handler in notificationHandlers)
                {
                    BackgroundJob.Enqueue<Messenger>(
                        m => m.DequeueNotification(null, handler.GetNotificationHandlerType().FullName, queueName,
                                handler.GetNotificationHandlerType(), notification));

                }
            }
        }

        [UseQueueFromParameter(2)]
        [DisplayName("{1}")]
        public void DequeueRequest(PerformContext context, string jobName, string queueName, IRequest request)
        {
            PerformContext = context;
            Send(request).Wait();
        }

        [DisplayName("{1}")]
        [UseQueueFromParameter(2)]
        public void DequeueNotification(PerformContext context, string jobName, string queueName, Type handler, INotification notification)
        {
            PerformContext = context;
            PublishAsync(notification, handler).Wait();
        }
        
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var defaultHandler = GetRequestHandler(request);

            var result = defaultHandler.Handle(request);

            return result;
        }

        public Task Publish(INotification notification)
        {
            var notificationHandlers = GetNotificationHandlers(notification)
                .Select(handler => handler.Handle(notification))
                .ToArray();

            return Task.WhenAll(notificationHandlers);
        }

        public async Task PublishAsync(INotification notification, Type notificationHandlerType)
        {
            var notificationHandlers =
                GetNotificationHandlers(notification)
                    .Where(x => x.GetNotificationHandlerType().FullName == notificationHandlerType.FullName)
                    .Select(handler => handler.Handle(notification))
                    .ToArray();

            await Task.WhenAll(notificationHandlers);
        }
        
        private RequestHandlerWrapper<TResponse> GetRequestHandler<TResponse>(IRequest<TResponse> request)
        {
            var requestType = request.GetType();

            var genericHandlerType = _genericHandlerCache.GetOrAdd(requestType, typeof(IRequestHandler<,>),
                (type, root) => root.MakeGenericType(type, typeof(TResponse)));

            var genericWrapperType = _wrapperHandlerCache.GetOrAdd(requestType, typeof(RequestHandlerWrapper<,>),
                (type, root) => root.MakeGenericType(type, typeof(TResponse)));

            var handler = GetRequestHandler(request, genericHandlerType);

            return (RequestHandlerWrapper<TResponse>)Activator.CreateInstance(genericWrapperType, this, handler);
        }
        
        private object GetRequestHandler(object request, Type handlerType)
        {
            try
            {
                return _commandHandlerFactory(handlerType);
            }
            catch (Exception e)
            {
                throw BuildException(request, e);
            }
        }

        private IEnumerable<NotificationHandlerWrapper> GetNotificationHandlers(INotification notification)
        {
            var notificationType = notification.GetType();

            var genericHandlerType = _genericHandlerCache.GetOrAdd(notificationType, typeof(INotificationHandler<>),
                (type, root) => root.MakeGenericType(type));

            var genericWrapperType = _wrapperHandlerCache.GetOrAdd(notificationType,
                typeof(NotificationHandlerWrapper<>), (type, root) => root.MakeGenericType(type));

            return GetNotificationHandlers(notification, genericHandlerType)
                .Select(handler => Activator.CreateInstance(genericWrapperType, this, handler))
                .Cast<NotificationHandlerWrapper>()
                .ToList();
        }

        private IEnumerable<object> GetNotificationHandlers(object notification, Type handlerType)
        {
            try
            {
                return _notificationHandlerFactory(handlerType);
            }
            catch (Exception e)
            {
                throw BuildException(notification, e);
            }
        }

        private static InvalidOperationException BuildException(object message, Exception inner)
        {
            return
                new InvalidOperationException(
                    $"Handler was not found for request of type {message.GetType()}.  Container or service locator not configured properly or handlers not registered with your container.",
                    inner);
        }

        public void Dispose()
        {
            
        }
    }
}
