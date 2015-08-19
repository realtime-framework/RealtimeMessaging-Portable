using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RealtimeDotNetTests
{
    [TestClass]
    public class RealtimeTests
    {

        [TestMethod]
        public async Task TestEvents()
        {
            Trace.WriteLine("TestStates");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            var master = new RealtimeTestClient();
            var client = master.client;

            bool didConnect = false;
            bool didDiconnect = false;
            bool didSub = false;
            bool didUnSub = false;
            bool didSend = false;


            client.OnConnected += sender => didConnect = true;
            client.OnDisconnected += sender => didDiconnect = true;
            client.OnSubscribed += (sender, channel) => didSub = true;
            client.OnUnsubscribed += (sender, channel) => didUnSub = true;

            await master.Connect();
            Assert.IsTrue(didConnect);

            await master.Subscribe("TestStates", true, (sender, channel, message) => didSend = true);
            Assert.IsTrue(didSub);

            master.Send("TestStates", "Hello");
            await Task.Delay(1000);
            Assert.IsTrue(didSend);

            await master.UnSubscribe("TestStates");
            Assert.IsTrue(didUnSub);

            await master.Disconnect();
            await Task.Delay(1000);
            Assert.IsTrue(didDiconnect);
        }

        [TestMethod]
        public async Task TestChat()
        {
            Trace.WriteLine("Test10Clients");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            var master = new RealtimeTestClient();
            var slave = new RealtimeTestClient();

            //connect
            await master.Connect();
            Assert.IsTrue(master.client.IsConnected);

            await slave.Connect();
            Assert.IsTrue(slave.client.IsConnected);

            var scounter = 0;
            var mcounter = 0;
            var sStop = false;
            var mStop = false;

            await master.Subscribe("Master", true, (sender, channel, message) =>
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


            await slave.Subscribe("Slave", true, (sender, channel, message) =>
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

            master.client.Send("Slave", "Start");

            var fault = DateTime.UtcNow.AddMinutes(3);
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
            await master.Disconnect();
            await slave.Disconnect();
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
                Trace.WriteLine("Test Client " + i);

                var counter = 0;
                var client = new RealtimeTestClient();
                await client.Connect();
                Assert.IsTrue(client.client.IsConnected);

                await client.Subscribe("Hello", true, (sender, channel, message) =>
                {
                    Trace.WriteLine(message);
                    counter++;
                });

                for (int j = 0;j < ten;j++)
                {
                    client.client.Send("Hello", "Message" + j);
                    await Task.Delay(100);
                }
                await Task.Delay(100);

                Assert.IsTrue(counter == ten);
                Trace.WriteLine("Time " + client.Watch.ElapsedMilliseconds);
                await client.Disconnect();
            }
        }

        [TestMethod]
        public async Task TestThreadedSpam()
        {
            Trace.WriteLine("TestThreadedSpam");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            var master = new RealtimeTestClient();
            var slave = new RealtimeTestClient();

            //connect
            await master.Connect();
            await slave.Connect();
            Assert.IsTrue(slave.client.IsConnected);

            var scounter = 0;

            await Task.Delay(1000);
            
            await slave.Subscribe("Slave", true, (sender, channel, message) =>
            {
                Trace.WriteLine(message);
                scounter++;
            });

            await Task.Delay(1000);
            
            var num = 5;

            var actions = new Action[num];
            for (int i = 0;i < num;i++)
            {
                actions[i] = () =>
                {
                    for (int j = 0;j < num;j++)
                    {
                        master.client.Send("Slave", j.ToString());
                    }
                };
            }

            Parallel.Invoke(actions);

            await Task.Delay(5000);

            Assert.IsTrue(slave.client.IsConnected);
            Assert.IsTrue(master.client.IsConnected);
            Assert.IsTrue(scounter == (num * num));
            await master.Disconnect();
            await slave.Disconnect();
        }
        
        // Test for Chat messages
        public async Task TestChunkMessages()
        {
            Trace.WriteLine("TestChunking");

            //Set App Key
            Assert.IsFalse(string.IsNullOrEmpty(RealtimeTestClient.AppKey));

            // Make 2 clients and wait for them to connect.
            var master = new RealtimeTestClient();
            var slave = new RealtimeTestClient();
            await master.Connect();
            await slave.Connect();
            Assert.IsTrue(master.client.IsConnected);
            Assert.IsTrue(slave.client.IsConnected);


            var sb = new StringBuilder();
            for (int i = 0;i < 3000;i++)
            {
                sb.Append("A");
            }
            var BigText = sb.ToString();

            var scounter = 0;
            var mcounter = 0;
            var sStop = false;
            var mStop = false;

            // Handle Messages
            master.client.Subscribe("Master", true, (sender, channel, message) =>
            {
                Trace.WriteLine(message);
                Assert.AreEqual(message, BigText);
                mcounter++;
                if (mcounter < 2)
                {
                    master.client.Send("Slave", BigText);
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
                Assert.AreEqual(message, BigText);
                scounter++;
                if (scounter < 2)
                {
                    slave.client.Send("Master", BigText);
                }
                else
                {
                    sStop = true;
                }
            });
            await Task.Delay(200);

            //Send BigText 3000 bytes
            master.client.Send("Slave", BigText);

            // wait for complete. Give 1 minute
            var fault = DateTime.UtcNow.AddMinutes(1);
            while (
                master.client.IsConnected
                && slave.client.IsConnected
                && !sStop
                && !mStop
                && DateTime.UtcNow < fault)
            {
                await Task.Delay(100);
            }

            //Asset messages received
            Assert.IsTrue(slave.client.IsConnected);
            Assert.IsTrue(master.client.IsConnected);
            Assert.IsTrue(scounter == mcounter + 1);

            //end
            await master.Disconnect();
            await slave.Disconnect();
        }
    }
}
