using System;
using NUnit.Framework;

namespace Apophysis
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CanLoadAndUnload()
        {
            var apophysis = new ApophysisNative();
            apophysis.Dispose();
        }
    }
}