using Owin;

namespace Hangfire.Messenger.Demo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard("/hangfire");
        }
    }
}
