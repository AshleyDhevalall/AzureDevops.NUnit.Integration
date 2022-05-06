using AzureDevops.NUnit.Integration;
using NUnit.Framework;

namespace SampleTestProject
{
    [Property("suiteid", "1")]
    [Property("projectid", "SampleProject")]
    public class Tests : TestBase
    {
        [Test]
        public void RandomTest()
        {
            string message = "Hello World";
            Assert.AreEqual("Hello World", message);
        }
    }
}