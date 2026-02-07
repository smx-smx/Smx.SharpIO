using System;
using System.Diagnostics;
using NUnit.Framework;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO.Tests
{
    public class Memory64Tests
    {
        [Test]
        public void CopyTo_Works()
        {
            var source = new Memory64<int>([1, 2, 3]);
            var dest = new Memory64<int>(new int[3]);
            
            source.CopyTo(dest);
            Assert.AreEqual(1, dest.Span[0]);
            Assert.AreEqual(2, dest.Span[1]);
            Assert.AreEqual(3, dest.Span[2]);
        }

        [Test]
        public void ImplicitConversion_Works()
        {
            var source64 = new Memory64<int>([1, 2, 3]);
            var dest64 = new Memory64<int>(new int[3]);

            Memory<int> source = (Memory<int>)source64;
            Memory<int> dest = (Memory<int>)dest64;
            
            source.CopyTo(dest);
            Assert.AreEqual(1, dest.Span[0]);
            Assert.AreEqual(2, dest.Span[1]);
            Assert.AreEqual(3, dest.Span[2]);
        }
        

        [Test]
        public void ImplicitConversion_SameReference()
        {
            var source64 = new Memory64<int>([1, 2, 3]);
            Memory<int> source = (Memory<int>)source64;

			source.Span[0] = 4;
			source.Span[1] = 5;
			source.Span[2] = 6;

			Assert.AreEqual(source64.Span[0], 4);
			Assert.AreEqual(source64.Span[1], 5);
			Assert.AreEqual(source64.Span[2], 6);
		}

        [Test]
        public void TryCopyTo_Works()
        {
            var source = new Memory64<int>([1, 2, 3]);
            var destShort = new Memory64<int>(new int[2]);
            var destLong = new Memory64<int>(new int[4]);
            
            Assert.IsFalse(source.TryCopyTo(destShort));
            Assert.IsTrue(source.TryCopyTo(destLong));
        }

        [Test]
        public unsafe void Pin_Works()
        {
            var array = new int[] { 123 };
            var memory = new Memory64<int>(array);
            
            using (var handle = memory.Pin())
            {
                int* ptr = (int*)handle.Pointer;
                Assert.AreEqual(123, *ptr);
            }
        }
    }

    public class ReadOnlyMemory64Tests
    {
        [Test]
        public void CopyTo_Works()
        {
            ReadOnlyMemory64<int> source = new int[] { 1, 2, 3 };
            var dest = new Memory64<int>(new int[3]);
            
            source.CopyTo(dest);
            Assert.AreEqual(1, dest.Span[0]);
            Assert.AreEqual(2, dest.Span[1]);
            Assert.AreEqual(3, dest.Span[2]);
        }
        
        [Test]
        public unsafe void Pin_Works()
        {
            var array = new int[] { 456 };
            ReadOnlyMemory64<int> memory = array;
            
            using (var handle = memory.Pin())
            {
                int* ptr = (int*)handle.Pointer;
                Assert.AreEqual(456, *ptr);
            }
        }
    }
}
