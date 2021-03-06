﻿using System;
using System.Threading;
using System.Web.Http;
using Owin;

namespace Regard.Consumer.SelfTest
{
    /// <summary>
    /// Public self-test API for the regard consumer process
    /// </summary>
    /// <remarks>
    /// This is mainly to allow us to verify that the consumer and query engine is up and processing commands. The results are public: anybody can see them, so the tests are
    /// fairly limited: basically, make sure the public functionality is working.
    /// </remarks>
    public class Startup
    {
        private Timer m_Timer;

        public void Configuration(IAppBuilder app)
        {
            // Set up a small thread pool
            ThreadPool.SetMaxThreads(40, 100);
            ThreadPool.SetMinThreads(1, 5);

            // Configure for attribute routes
            var httpConfiguration = new HttpConfiguration();

            // DANGER WILL ROBINSON
            //
            // This performs magical reflection voodoo to find anything with an HttpRoute attribute. If you reference the Query WebAPI library from this project then
            // you'll make the internal query API public! This won't be a bad problem unless you also add the database connection strings to the configuration for this
            // role.
            //
            // It's quite likely we will eventually want this to be able to talk to the query database directly.
            httpConfiguration.MapHttpAttributeRoutes();

            app.UseWebApi(httpConfiguration);

            // Start the tests running in the background
            m_Timer = new Timer(x => TestResults.RunTests(), null, 0, TimeSpan.FromMinutes(30).Milliseconds);

            app.Run(async context =>
            {
                // Default response is pretty boring
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            });
        }
    }
}