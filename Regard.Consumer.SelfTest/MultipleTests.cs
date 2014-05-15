using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest
{
    /// <summary>
    /// A test that runs multiple tests
    /// </summary>
    class MultipleTests : ITest
    {
        /// <summary>
        /// A list of tests and names
        /// </summary>
        private readonly List<ITest> m_Tests;

        public MultipleTests(IEnumerable<ITest> tests, string name)
        {
            if (tests == null) throw new ArgumentNullException("tests");
            m_Tests = tests.ToList();
            Name = name;
        }

        public string Name { get; set; }

        public async Task<JObject> Run()
        {
            JObject result = new JObject();

            foreach (var test in m_Tests)
            {
                try
                {
                    Trace.WriteLine("Test: " + test.Name);
                    var testResult = await test.Run();

                    result[test.Name] = testResult;
                }
                catch (Exception e)
                {
                    Trace.TraceError("Test {0} Exception: {1}", test.Name, e);

                    // Report exceptions if any occur
                    result[test.Name] = JObject.FromObject(new
                    {
                        Exception = e.GetType().Name
                    });
                }
            }

            return result;
        }
    }
}