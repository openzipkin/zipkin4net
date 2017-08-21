using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Annotation;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_ClientTrace
    {
        private Mock<IRecordDispatcher> dispatcher;
        private const string serviceName = "service1";
        private const string rpc = "rpc";

		[SetUp]
		public void SetUp()
		{
			TraceManager.ClearTracers();
			TraceManager.Stop();
			dispatcher = new Mock<IRecordDispatcher>();
			TraceManager.Start(new VoidLogger(), dispatcher.Object);
		}

        [Test]
        public void ShouldNotSetCurrentTrace()
        {
            Trace.Current = null;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                Assert.IsNull(client.Trace);
            }
        }

        [Test]
        public void ShouldCallChildWhenCurrentTraceNotNull()
        {
            var trace = Trace.Create();
            Trace.Current = trace;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                Assert.AreEqual(trace.CurrentSpan.SpanId, client.Trace.CurrentSpan.ParentSpanId);
                Assert.AreEqual(trace.CurrentSpan.TraceId, client.Trace.CurrentSpan.TraceId);
            }
        }

		[Test]
		public void ExceptionThrownInTracedActionAsyncShouldAddErrorTagAndRethrow()
		{

			var trace = Trace.Create();
            trace.ForceSampled();
			Trace.Current = trace;
            var clientTrace = new ClientTrace(serviceName, rpc);
			var ex = new Exception("something bad happened");
			Task<int> task = Task.Run(() =>
			{
				try
				{
					return 0;
				}
				finally
				{
					throw ex;
				}
			});
            Assert.ThrowsAsync<Exception>(() => clientTrace.TracedActionAsync(task));

			VerifyDispatcherRecordedAnnotation(new TagAnnotation("error", ex.Message));
		}

		private void VerifyDispatcherRecordedAnnotation(IAnnotation annotation)
		{
			dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation.Equals(annotation))));
		}
    }
}