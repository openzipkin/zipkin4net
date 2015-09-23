using System.Text;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using NUnit.Framework;
using BinaryAnnotation = Criteo.Profiling.Tracing.Tracers.Zipkin.BinaryAnnotation;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_BinaryAnnotation
    {
        [Test]
        public void ThriftConversionIsCorrect()
        {
            const string key = "myKey";
            var data = Encoding.ASCII.GetBytes("hello");
            const AnnotationType type = AnnotationType.STRING;

            var binAnn = new BinaryAnnotation(key, data, type);

            var thriftBinAnn = binAnn.ToThrift();

            Assert.AreEqual(key, thriftBinAnn.Key);
            Assert.AreEqual(data, thriftBinAnn.Value);
            Assert.IsNull(thriftBinAnn.Host);
            Assert.AreEqual(type, thriftBinAnn.Annotation_type);
        }

    }
}
