using System;
using System.Collections.Generic;
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
        private static readonly ITest s_Tests;
        private static bool s_TestingInProgress;

        static TestResults()
        {
            // Create the tests
            s_Tests = new QueryApiTests();
            Results = new List<JObject>();
        }

        /// <summary>
        /// Causes the tests to be run (will return immediately; tests will run in background)
        /// </summary>
        public static async void RunTests()
        {
            // Nothing to do if the tests are already running
            if (s_TestingInProgress)
                return;

            s_TestingInProgress = true;

            var result = await RunTestsInBackground();

            s_TestingInProgress = false;

            Results.Add(result);
        }

        /// <summary>
        /// Awaitable version of RunTests()
        /// </summary>
        private static async Task<JObject> RunTestsInBackground()
        {
            Trace.WriteLine("Starting tests...");

            // Wait for a short while to allow the rest of the service to come up
            await Task.Delay(TimeSpan.FromSeconds(5));

            var result = await s_Tests.Run();
            
            // Ensure that the test results end up in the log
            Trace.WriteLine("Test results: " + result);

            return result;
        }

        /// <summary>
        /// The current test results
        /// </summary>
        public static List<JObject> Results { get; private set; }
    }
}