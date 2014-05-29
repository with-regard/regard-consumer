using System.Threading;
using System.Web.Http;
using Owin;

namespace Regard.Query.Internal.WebAPI
{
    /// <summary>
    /// Startup class for the internal Web API (used to drive the query library from internal services).
    /// </summary>
    /// <remarks>
    /// Note that this version has no authentication so care should be taken not to expose the interface externally.
    /// </remarks>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // We need some completion threads for finishing data writes
            ThreadPool.SetMaxThreads(40, 100);

            // Configure for attribute routes
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.MapHttpAttributeRoutes();

            app.UseWebApi(httpConfiguration);

            app.Run(async context =>
            {
                // Default behaviour is is to just 404
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("");
            });
        }
    }
}