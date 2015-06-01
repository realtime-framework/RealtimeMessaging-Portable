using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RealtimeFramework.Messaging.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            System.Diagnostics.Trace.WriteLine("TestMethod1");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(UnitTestClient.AppKey));

            for (int i = 0;i < 10;i++)
            {
                var client = new UnitTestClient();
                client.Connect();
                var fault = DateTime.UtcNow.AddMinutes(1);
                while (!client.client.IsConnected && DateTime.UtcNow < fault)
                {
                    await Task.Delay(100);
                }
                client.Disconnect();
                System.Diagnostics.Trace.WriteLine("Time " + client.Watch.ElapsedMilliseconds);
                Assert.IsTrue(client.Watch.ElapsedMilliseconds < 5000);
            }
        }
    }
}
