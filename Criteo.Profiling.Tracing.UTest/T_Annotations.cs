using Criteo.Profiling.Tracing.Annotation;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
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
        }

    }
}
