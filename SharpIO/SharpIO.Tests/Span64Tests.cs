using NUnit.Framework;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO.Tests
{
    public class Span64Tests
    {
        [Test]
        public void Clear_Works()
        {
            var array = new int[] { 1, 2, 3, 4 };
            var span = new Span64<int>(array);
            span.Clear();
            
            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(0, array[2]);
            Assert.AreEqual(0, array[3]);
        }

        [Test]
        public void Fill_Works()
        {
            var array = new int[4];
            var span = new Span64<int>(array);
            span.Fill(42);
            
            Assert.AreEqual(42, array[0]);
            Assert.AreEqual(42, array[1]);
            Assert.AreEqual(42, array[2]);
            Assert.AreEqual(42, array[3]);
        }

        [Test]
        public void TryCopyTo_Works()
        {
            var source = new Span64<int>(new int[] { 1, 2, 3 });
            var destShort = new Span64<int>(new int[2]);
            var destLong = new Span64<int>(new int[4]);

            Assert.IsFalse(source.TryCopyTo(destShort));
            Assert.IsTrue(source.TryCopyTo(destLong));
            
            Assert.AreEqual(1, destLong[0]);
            Assert.AreEqual(2, destLong[1]);
            Assert.AreEqual(3, destLong[2]);
            Assert.AreEqual(0, destLong[3]);
        }

        [Test]
        public void EqualityOperators_Work()
        {
            var array = new int[] { 1, 2, 3 };
            var span1 = new Span64<int>(array);
            var span2 = new Span64<int>(array);
            var span3 = new Span64<int>(array).Slice(1);
            var span4 = new Span64<int>(new int[] { 1, 2, 3 });

            Assert.IsTrue(span1 == span2);
            Assert.IsFalse(span1 == span3);
            Assert.IsFalse(span1 == span4); // Different memory regions
            Assert.IsFalse(span1 != span2);
            Assert.IsTrue(span1 != span3);
        }

        [Test]
        public void GetEnumerator_Works()
        {
            var span = new Span64<int>(new int[] { 10, 20, 30 });
            int sum = 0;
            foreach (var item in span)
            {
                sum += item;
            }
            Assert.AreEqual(60, sum);
        }
        
        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var span = new Span64<int>(new int[5]);
            Assert.AreEqual("Smx.SharpIO.Memory.Buffers.Span64<Int32>[5]", span.ToString());
        }
    }
    
    public class ReadOnlySpan64Tests {
        [Test]
        public void TryCopyTo_Works() {
            var source = new ReadOnlySpan64<int>(new int[] { 1, 2, 3 });
            var destShort = new Span64<int>(new int[2]);
            var destLong = new Span64<int>(new int[4]);

            Assert.IsFalse(source.TryCopyTo(destShort));
            Assert.IsTrue(source.TryCopyTo(destLong));
            
            Assert.AreEqual(1, destLong[0]);
            Assert.AreEqual(2, destLong[1]);
            Assert.AreEqual(3, destLong[2]);
        }
        
        [Test]
        public void EqualityOperators_Work()
        {
            var array = new int[] { 1, 2, 3 };
            var span1 = new ReadOnlySpan64<int>(array);
            var span2 = new ReadOnlySpan64<int>(array);
            var span3 = new ReadOnlySpan64<int>(array).Slice(1);

            Assert.IsTrue(span1 == span2);
            Assert.IsFalse(span1 == span3);
        }

        [Test]
        public void GetEnumerator_Works()
        {
            var span = new ReadOnlySpan64<int>(new int[] { 10, 20, 30 });
            int sum = 0;
            foreach (var item in span)
            {
                sum += item;
            }
            Assert.AreEqual(60, sum);
        }
    }
}
