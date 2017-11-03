using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using zipkin4net.Tracers.Zipkin;

namespace zipkin4net.Tests.dotnetcore.Utils
{
    [TestFixture]
    internal class T_SerializerUtils
    {
        [Test]
        public void ToEscaped()
        {
            string testString = "\"testString\"";
            string testString2 = "\"!@#$%^&*()\a\b\0\f\n\r\t\v";
            Assert.AreEqual(SerializerUtils.ToEscaped(testString), "\\\"testString\\\"");
            Assert.AreEqual(SerializerUtils.ToEscaped(testString2), "\\\"!@#$%^&*()\\a\\b\\0\\f\\n\\r\\t\\v");

        }
    }
}
