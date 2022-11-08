// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Diagnostics;
using MQTTnet.Internal;
using MQTTnet.Protocol;

namespace MQTTnet.TestApp
{
    public static class ClientTest
    {
        public static async Task RunAsync()
        {
            try
            {
                var logger = new MqttNetEventLogger();
                MqttNetConsoleLogger.ForwardToConsole(logger);

                // 客户端参数
                var factory = new MqttFactory(logger);
                var client = factory.CreateMqttClient();
                var clientOptions = new MqttClientOptions
                {
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = "192.168.100.7"
                    }
                };

                // 添加“收到订阅消息”的委托函数
                client.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();
                    
                    return CompletedTask.Instance;
                };
                // 添加“连接到服务器”的委托函数
                client.ConnectedAsync += async e =>
                {
                    Console.WriteLine("### CONNECTED WITH SERVER ###");

                    await client.SubscribeAsync("test");

                    Console.WriteLine("### SUBSCRIBED ###");
                };
                // 添加“断开到服务器的连接”的委托函数
                client.DisconnectedAsync += async e =>
                {
                    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    try
                    {
                        await client.ConnectAsync(clientOptions);
                    }
                    catch
                    {
                        Console.WriteLine("### RECONNECTING FAILED ###");
                    }
                };

                // 发起连接
                try
                {
                    await client.ConnectAsync(clientOptions);   // 如果连接成功那么触发“连接到服务器”的委托函数
                }
                catch (Exception exception)
                {
                    Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
                }

                Console.WriteLine("### WAITING FOR APPLICATION MESSAGES ###");

                while (true)
                {
                    Console.ReadLine();     // 命令行停在这里

                    await client.SubscribeAsync("A/B/C");

                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("A/B/C")
                        .WithPayload("Hello World")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await client.PublishAsync(applicationMessage);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
