using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RealtimeFramework.Messaging.Tests
{
    [TestClass]
    public class RealtimeTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            System.Diagnostics.Trace.WriteLine("TestMethod1");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            for (int i = 0;i < 10;i++)
            {
                var client = new RealtimeTestClient();
                client.Connect();
                var fault = DateTime.UtcNow.AddMinutes(1);
                while (!client.client.IsConnected && DateTime.UtcNow < fault)
                {
                    await Task.Delay(100);
                }
                Assert.IsTrue(client.client.IsConnected);

                var counter = 0;

                client.client.Subscribe("Hello", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });
                await Task.Delay(200);

                for (int j = 0; j < 5; j++)
                {
                    client.client.Send("Hello", "Message" + j);
                    await Task.Delay(100);
                }
                await Task.Delay(100);

                Assert.IsTrue(counter == 5);

                client.Disconnect();
                System.Diagnostics.Trace.WriteLine("Time " + client.Watch.ElapsedMilliseconds);
                Assert.IsTrue(client.Watch.ElapsedMilliseconds < 5000);
            }
        }


        [TestMethod]
        public async Task TestMethod2()
        {
            System.Diagnostics.Trace.WriteLine("TestMethod1");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            for (int i = 0;i < 2;i++)
            {
                var client = new RealtimeTestClient();
                client.Connect();
                var fault = DateTime.UtcNow.AddMinutes(1);
                while (!client.client.IsConnected && DateTime.UtcNow < fault)
                {
                    await Task.Delay(100);
                }
                await Task.Delay(1000);
                Assert.IsTrue(client.client.IsConnected);
                Debug.WriteLine("IsConnected");

                var counter = 0;

                Debug.WriteLine("Hello");
                client.client.Subscribe("Hello", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });

                // Chain Subscribing causes the socket to close.
                // Adding a 1 second delay helps, but smells
                Debug.WriteLine("Hello2");
                client.client.Subscribe("Hello2", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });
                Debug.WriteLine("Hello3");
                client.client.Subscribe("Hello3", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });
                await Task.Delay(200);

                for (int j = 0;j < 5;j++)
                {
                    Debug.WriteLine("Send");
                    client.client.Send("Hello", "Message" + j);
                    await Task.Delay(100);
                }
                await Task.Delay(100);

                Assert.IsTrue(counter == 5);

                client.Disconnect();
                System.Diagnostics.Trace.WriteLine("Time " + client.Watch.ElapsedMilliseconds);
                Assert.IsTrue(client.Watch.ElapsedMilliseconds < 5000);
            }
        }
    }
}
