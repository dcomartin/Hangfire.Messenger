using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Messenger.Demo
{
    public class RequestWithResponse : IRequest<string>
    {
        public string YourName { get; }

        public RequestWithResponse(string yourName)
        {
            YourName = yourName;
        }
    }

    public class RequestWithResponseHandler : IRequestHandler<RequestWithResponse, string>
    {
        public Task<string> Handle(RequestWithResponse message, IMessenger messenger)
        {
            return Task.FromResult($"Your Name: {message.YourName}");
        }
    }
}
