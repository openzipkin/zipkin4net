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
        }

    }
}
