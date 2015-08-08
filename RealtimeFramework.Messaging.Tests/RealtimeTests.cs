using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RealtimeFramework.Messaging.Tests
{
    [TestClass]
    public class RealtimeTests
    {

        [TestMethod]
        public async Task TestChat()
        {
            Trace.WriteLine("Test10Clients");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            var master = new RealtimeTestClient();
            var slave = new RealtimeTestClient();

            //connect
            master.Connect();
            var fault = DateTime.UtcNow.AddMinutes(1);
            while (!master.client.IsConnected && DateTime.UtcNow < fault)
            {
                await Task.Delay(100);
            }
            Assert.IsTrue(master.client.IsConnected);

            slave.Connect();
            fault = DateTime.UtcNow.AddMinutes(1);
            while (!slave.client.IsConnected && DateTime.UtcNow < fault)
            {
                await Task.Delay(100);
            }
            Assert.IsTrue(slave.client.IsConnected);


            var scounter = 0;
            var mcounter = 0;
            var sStop = false;
            var mStop = false;

            master.client.Subscribe("Master", true, (sender, channel, message) =>
            {
                Trace.WriteLine(message);

                mcounter++;
                if (mcounter < 20)
                {
                    master.client.Send("Slave", "Hello " + mcounter);
                }
                else
                {
                    mStop = true;
                }


            });
            await Task.Delay(200);


            slave.client.Subscribe("Slave", true, (sender, channel, message) =>
            {
                Trace.WriteLine(message);
                scounter++;
                if (scounter < 20)
                {
                    slave.client.Send("Master", "Hello " + scounter);
                }
                else
                {
                    sStop = true;
                }

            });
            await Task.Delay(200);

            master.client.Send("Slave", "Start");

            fault = DateTime.UtcNow.AddMinutes(3);
            while (
                master.client.IsConnected
                && slave.client.IsConnected
                && !sStop
                && !mStop
                && DateTime.UtcNow < fault)
            {
                await Task.Delay(100);
            }

            Assert.IsTrue(slave.client.IsConnected);
            Assert.IsTrue(master.client.IsConnected);
            Assert.IsTrue(scounter == mcounter + 1);
            master.Disconnect();
            slave.Disconnect();
        }

        [TestMethod]
        public async Task Test10Clients()
        {
            Trace.WriteLine("Test10Clients");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            var ten = 10;
            for (int i = 0;i < ten;i++)
            {
                var counter = 0;
                var client = new RealtimeTestClient();
                client.Connect();
                var fault = DateTime.UtcNow.AddMinutes(1);
                while (!client.client.IsConnected && DateTime.UtcNow < fault)
                {
                    await Task.Delay(100);
                }
                Assert.IsTrue(client.client.IsConnected);


                client.client.Subscribe("Hello", true, (sender, channel, message) =>
                {
                    Trace.WriteLine(message);
                    counter++;

                });
                await Task.Delay(200);

                for (int j = 0;j < ten;j++)
                {
                    client.client.Send("Hello", "Message" + j);
                    await Task.Delay(100);
                }
                await Task.Delay(100);

                Assert.IsTrue(counter == ten);
                Trace.WriteLine("Time " + client.Watch.ElapsedMilliseconds);
                client.Disconnect();
            }
        }


        [TestMethod]
        public async Task Test3Clients()
        {
            Trace.WriteLine("Test3Clients");

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
                    Trace.WriteLine(message);
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
                Trace.WriteLine("Time " + client.Watch.ElapsedMilliseconds);
                Assert.IsTrue(client.Watch.ElapsedMilliseconds < 5000);
            }
        }


        // Failing

        //[TestMethod]
        public async Task TestResubscribeBug()
        {
            Trace.WriteLine("TestResubscribeBug");

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

                var counter = 0;

                client.client.Subscribe("Hello", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });

                // Chain Subscribing causes the socket to close.
                // Adding a 1 second delay helps, but smells
                client.client.Subscribe("Hello2", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });
                client.client.Subscribe("Hello3", true, (sender, channel, message) =>
                {
                    System.Diagnostics.Trace.WriteLine(message);
                    counter++;

                });
                await Task.Delay(200);

                for (int j = 0;j < 5;j++)
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


        // Debug mode below

        /// <summary>
        /// NOTE USE DEBUG
        /// </summary>
        //[TestMethod]
        public async Task TestReconnectBug()
        {
            //Bug causes reconnecting/reconnected loop
            //This test method proves it was fixed

            var client = new RealtimeTestClient();
            client.Connect();
            var fault = DateTime.UtcNow.AddMinutes(1);
            while (!client.client.IsConnected && DateTime.UtcNow < fault)
            {
                await Task.Delay(100);
            }

            var reconnected = 0;

            client.client.OnReconnected += sender =>
            {
                reconnected++;
            };


            var b = "Breakpoint here and pull internet.";
            Trace.WriteLine(b);

            //send to force disc

            client.client.Send("Hello", "Message");

            while (client.client.IsConnected)
            {
                await Task.Delay(100);
            }

            var b2 = "Breakpoint here and reconnect internet.";
            Trace.WriteLine(b2);

            fault = DateTime.UtcNow.AddMinutes(1);
            while (!client.client.IsConnected && DateTime.UtcNow < fault)
            {
                await Task.Delay(100);
            }

            //stable connection ?
            await Task.Delay(5000);
            Assert.IsTrue(client.client.IsConnected);

            // Only reconnected once
            Assert.IsTrue(reconnected == 1);
        }


       
    }
}
