using System.Linq;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Tracer
    {

        [SetUp]
        public void Setup()
        {
            Tracer.Clear();
        }

        [Test]
        public void TracersAreCorrectlyRegistered()
        {
            var tracer = new Mock<ITracer>();

            Tracer.Register(tracer.Object);

            Assert.AreEqual(1, Tracer.Tracers.Count);
            Assert.AreEqual(tracer.Object, Tracer.Tracers.First());

            var secondTracer = new Mock<ITracer>();

            Tracer.Register(secondTracer.Object);

            Assert.AreEqual(2, Tracer.Tracers.Count);
            Assert.True(Tracer.Tracers.Contains(tracer.Object));
            Assert.True(Tracer.Tracers.Contains(secondTracer.Object));
        }

        [Test]
        public void TracersAreIndeedCleared()
        {
            var tracer = new Mock<ITracer>();
            Tracer.Register(tracer.Object);

            var secondTracer = new Mock<ITracer>();
            Tracer.Register(secondTracer.Object);

            Assert.AreEqual(2, Tracer.Tracers.Count);

            Tracer.Clear();

            Assert.AreEqual(0, Tracer.Tracers.Count);
        }


    }
}
