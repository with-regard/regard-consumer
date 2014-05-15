using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Regard.Consumer.SelfTest.QueryAPI;

namespace Regard.Consumer.SelfTest
{
    /// <summary>
    /// Static class that can be used to retrieve/update the test results
    /// </summary>
    public static class TestResults
    {
        private static Task s_RunningTests;
        private static ITest s_Tests;

        static TestResults()
        {
            // Create the tests
            s_Tests = new QueryApiTests();
            Results = JObject.FromObject(new
            {
                Status = "Tests not yet completed"
            });
        }

        /// <summary>
        /// Causes the tests to be run (will return immediately; tests will run in background)
        /// </summary>
        public static void RunTests()
        {
            // Nothing to do if the tests are already running
            if (s_RunningTests != null) return;

            // Begin the tests
            s_RunningTests = RunTestsInBackground();
        }

        /// <summary>
        /// Awaitable version of RunTests()
        /// </summary>
        private static async Task RunTestsInBackground()
        {
            Trace.WriteLine("Starting tests...");

            // Wait for a short while to allow the rest of the service to come up
            await Task.Delay(TimeSpan.FromSeconds(5));

            var result = await s_Tests.Run();
            Results = result;

            // Ensure that the test results end up in the log
            Trace.WriteLine("Test results: " + result);
        }

        /// <summary>
        /// The current test results
        /// </summary>
        public static JObject Results { get; private set; }
    }
}