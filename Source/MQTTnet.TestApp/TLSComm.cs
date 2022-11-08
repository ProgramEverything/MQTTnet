using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace MQTTnet.TestApp
{
    internal class TLSComm
    {
        public async Task TLSCommAsync()
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var caCert = X509Certificate.CreateFromCertFile(@"D:\emqx-5.0.9-windows-amd64\etc\certs\cacert.pem");
            caCert = new X509Certificate(caCert.Export(X509ContentType.Cert));
            var clientCert = new X509Certificate2(@"D:\emqx-5.0.9-windows-amd64\etc\certs\client-cert.pem", "client");
            var newCert = new X509Certificate2(clientCert.Export(X509ContentType.SerializedCert));
            Console.WriteLine(clientCert.ToString());
            Console.WriteLine(clientCert.HasPrivateKey);
            var options = new MqttClientOptionsBuilder()
            .WithClientId("cubic&bvemp4drrkplpi4tbfp0")
            .WithTcpServer("192.168.100.7", 8883)
            .WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                AllowUntrustedCertificates = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                SslProtocol = SslProtocols.Tls,
                CertificateValidationHandler = (o) =>
                {
                    return true;
                },
                Certificates = new List<X509Certificate>(){
                    caCert,
                    newCert,
                },
            })
            .WithCleanSession()
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .Build();

            // option加载了tls通信所需要的内容，直接用其发起连接
            Console.WriteLine();
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            } catch (Exception)
            {
                Console.WriteLine("[Error]\n\tClient to server connection failed.\n");
            }
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("OverTLS")
                .WithPayload("Hello World")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            try
            {
                await mqttClient.PublishAsync(applicationMessage);
            } catch (Exception)
            {
                Console.WriteLine("[Error]\n\tMessage publishing failed.\n");
            }
        }
    }
}