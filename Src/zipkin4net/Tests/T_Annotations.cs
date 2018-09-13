using System.Net;
using zipkin4net.Annotation;
using NUnit.Framework;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_Annotations
    {

        [Test]
        public void FactoryReturnsCorrectTypes()
        {
            Assert.IsInstanceOf<TagAnnotation>(Annotations.Tag("",""));
            Assert.IsInstanceOf<ClientRecv>(Annotations.ClientRecv());
            Assert.IsInstanceOf<ClientSend>(Annotations.ClientSend());
            Assert.IsInstanceOf<LocalAddr>(Annotations.LocalAddr(null));
            Assert.IsInstanceOf<Rpc>(Annotations.Rpc(""));
            Assert.IsInstanceOf<ServerRecv>(Annotations.ServerRecv());
            Assert.IsInstanceOf<ServerSend>(Annotations.ServerSend());
            Assert.IsInstanceOf<ServiceName>(Annotations.ServiceName(""));
            Assert.IsInstanceOf<Event>(Annotations.Event(""));
            Assert.IsInstanceOf<ClientAddr>(Annotations.ClientAddr(null));
            Assert.IsInstanceOf<ServerAddr>(Annotations.ServerAddr(null, null));
            Assert.IsInstanceOf<MessageAddr>(Annotations.MessageAddr(null, null));
            Assert.IsInstanceOf<ConsumerStart>(Annotations.ConsumerStart());
            Assert.IsInstanceOf<ConsumerStop>(Annotations.ConsumerStop());
            Assert.IsInstanceOf<ProducerStart>(Annotations.ProducerStart());
            Assert.IsInstanceOf<ProducerStop>(Annotations.ProducerStop());
            Assert.IsInstanceOf<LocalOperationStart>(Annotations.LocalOperationStart(""));
            Assert.IsInstanceOf<LocalOperationStop>(Annotations.LocalOperationStop());
        }

        [Test]
        public void ToStringWriteExtraDataInAdditionToType()
        {
            Assert.AreEqual("TagAnnotation: sampleTagKey [sampleTagValue System.String]", Annotations.Tag("sampleTagKey", "sampleTagValue").ToString());
            Assert.AreEqual("Rpc: GET", Annotations.Rpc("GET").ToString());
            Assert.AreEqual("ServiceName: sampleName", Annotations.ServiceName("sampleName").ToString());
            Assert.AreEqual("Event: sampleName", Annotations.Event("sampleName").ToString());
            Assert.AreEqual("LocalOperationStart: sampleName", Annotations.LocalOperationStart("sampleName").ToString());

            var samIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80);
            Assert.AreEqual("ClientAddr: 127.0.0.1:80", Annotations.ClientAddr(samIpEndPoint).ToString());
            Assert.AreEqual("ServerAddr: sampleName/127.0.0.1:80", Annotations.ServerAddr("sampleName", samIpEndPoint).ToString());
            Assert.AreEqual("MessageAddr: sampleName/127.0.0.1:80", Annotations.MessageAddr("sampleName", samIpEndPoint).ToString());
        }

    }
}
