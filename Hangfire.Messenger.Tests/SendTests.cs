using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Shouldly;
using Xunit;

namespace Hangfire.Messenger.Tests
{
    public class SendTests
    {
        public class Response
        {
            public Response(string message)
            {
                Message = message;
            }
            public string Message { get; }
        }

        public class Request : IRequest<Response> { }

        public class Handler : IRequestHandler<Request, Response>
        {
            public Task<Response> Handle(Request message, IMessenger messenger)
            {
                return Task.FromResult(new Response("Hello World"));
            }
        }
        [Fact]
        public async Task Should_return_response()
        {
            var container = new UnityContainer();
            container.RegisterType<IRequestHandler<Request, Response>, Handler>();

            var messenger = new Messenger(type => container.Resolve(type), type => container.ResolveAll(type));
            var response = await messenger.Send(new Request());

            response.Message.ShouldBe("Hello World");
        }
    }
}
