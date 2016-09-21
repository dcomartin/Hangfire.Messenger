using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace Hangfire.Message.Demo.Publisher
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard("/hangfire");
        }
    }
}
