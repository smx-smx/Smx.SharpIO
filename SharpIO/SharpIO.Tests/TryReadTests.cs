using NUnit.Framework;
using Smx.SharpIO;
using Smx.SharpIO.Extensions;
using Smx.SharpIO.Memory.Buffers;
using System;

namespace SharpIO.Tests
{
    public class TryReadTests
    {
        [Test]
        public void TestTryReadPrimitive_SpanStream()
        {
            var buf = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var st = new SpanStream(buf);

            // Int16 (2 bytes)
            Assert.IsTrue(st.TryReadInt16(out short s));
            Assert.AreEqual(0x0201, s);
            Assert.AreEqual(2, st.Position);

            // Int16 (2 bytes) - Success
            Assert.IsTrue(st.TryReadInt16(out s));
            Assert.AreEqual(0x0403, s);
            Assert.AreEqual(4, st.Position);

            // Int16 (2 bytes) - Fail (0 bytes left)
            Assert.IsFalse(st.TryReadInt16(out s));
            Assert.AreEqual(4, st.Position); // Position shouldn't change
            Assert.AreEqual(0, s);
        }

        [Test]
        public void TestTryReadGenerics_SpanStream()
        {
            var buf = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var st = new SpanStream(buf);

            Assert.IsTrue(st.TryRead<short>(out short s));
            Assert.AreEqual(0x0201, s);

            Assert.IsTrue(st.TryRead<short>(out s));
            Assert.AreEqual(0x0403, s);

            Assert.IsFalse(st.TryRead<byte>(out byte b));
            Assert.AreEqual(0, b);
        }

        [Test]
        public void TestTryReadBytes_SpanStream()
        {
            var buf = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var st = new SpanStream(buf);

            Assert.IsTrue(st.TryReadBytes(2, out byte[]? b));
            Assert.IsNotNull(b);
            Assert.AreEqual(2, b!.Length);
            Assert.AreEqual(1, b[0]);
            Assert.AreEqual(2, b[1]);

            Assert.IsTrue(st.TryReadBytes(2, out b));
            Assert.IsNotNull(b);
            Assert.AreEqual(3, b![0]);

            Assert.IsFalse(st.TryReadBytes(1, out b));
            Assert.IsNull(b);
        }
        
        [Test]
        public void TestTryReadString_SpanStream()
        {
            var buf = new byte[] { (byte)'A', (byte)'B', (byte)'C' };
            var st = new SpanStream(buf);

            Assert.IsTrue(st.TryReadString(2, out string? s));
            Assert.AreEqual("AB", s);
            
            Assert.IsFalse(st.TryReadString(2, out s));
            Assert.IsNull(s);
            
            Assert.IsTrue(st.TryReadString(1, out s));
            Assert.AreEqual("C", s);
        }

        [Test]
        public void TestTryReadCString_SpanStream()
        {
            var buf = new byte[] { (byte)'H', (byte)'i', 0, (byte)'!' };
            var st = new SpanStream(buf);

            Assert.IsTrue(st.TryReadCString(out string? s));
            Assert.AreEqual("Hi", s);
            Assert.AreEqual(3, st.Position); // Should be after null

            Assert.IsFalse(st.TryReadCString(out s)); // '!' then EOF, no null
            Assert.IsNull(s);
            Assert.AreEqual(3, st.Position); // Should not move
        }

        [Test]
        public void TestTryReadExtensions_Span()
        {
            Span<byte> span = new byte[] { 0x01, 0x02, 0x03 };
            
            Assert.IsTrue(span.TryRead(0, out byte b));
            Assert.AreEqual(1, b);
            
            Assert.IsTrue(span.TryRead(0, out short s));
            Assert.AreEqual(0x0201, s);
            
            Assert.IsTrue(span.TryRead(2, out b));
            Assert.AreEqual(3, b);

            Assert.IsFalse(span.TryRead(2, out s)); // needs 2 bytes, only 1 available
            Assert.AreEqual(0, s);

            Assert.IsFalse(span.TryRead(3, out b));
            Assert.IsFalse(span.TryRead(-1, out b));
        }

        [Test]
        public void TestTryReadExtensions_ReadOnlySpan()
        {
            ReadOnlySpan<byte> span = new byte[] { 0x01, 0x02, 0x03 };
            
            Assert.IsTrue(span.TryRead(0, out byte b));
            Assert.AreEqual(1, b);
            
            Assert.IsTrue(span.TryRead(0, out short s));
            Assert.AreEqual(0x0201, s);
            
            Assert.IsFalse(span.TryRead(2, out s));
        }

        [Test]
        public void TestTryReadExtensions_Span64()
        {
            Span64<byte> span = new Span64<byte>(new byte[] { 0x01, 0x02, 0x03 });
            
            Assert.IsTrue(span.TryRead(0, out byte b));
            Assert.AreEqual(1, b);
            
            Assert.IsTrue(span.TryRead(0, out short s));
            Assert.AreEqual(0x0201, s);
            
            Assert.IsFalse(span.TryRead(2, out s));
        }

        [Test]
        public void TestTryRead_MemoryMarshal64()
        {
            var data = new byte[] { 0x01, 0x02, 0x03 };
            ReadOnlySpan64<byte> span = new ReadOnlySpan64<byte>(data);
            
            Assert.IsTrue(MemoryMarshal64.TryRead(span, out short val));
            Assert.AreEqual(0x0201, val);
            
            var smallSpan = span.Slice(2);
            Assert.IsFalse(MemoryMarshal64.TryRead(smallSpan, out val));
        }

        [Test]
        public void TestTryReadEnum_SpanStream()
        {
            var buf = new byte[] { 0x03 }; // 3 is not A(1) or B(2)
            var st = new SpanStream(buf);
            
            // Should fail and NOT advance position
            Assert.IsFalse(st.TryReadEnum<ByteEnum>(out ByteEnum e));
            Assert.AreEqual(0, st.Position);
            
            // Valid read
            st.WriteByte((byte)ByteEnum.A); // Overwrite with 1
            st.Position = 0;
            
            Assert.IsTrue(st.TryReadEnum<ByteEnum>(out e));
            Assert.AreEqual(ByteEnum.A, e);
            Assert.AreEqual(1, st.Position);
        }

        [Test]
        public void TestTryReadFlagsEnum_SpanStream()
        {
             var buf = new byte[] { (byte)(FlagsEnum.Flag1|FlagsEnum.Flag3), 0x00 };
             var st = new SpanStream(buf);
             
             Assert.IsTrue(st.TryReadFlagsEnum<FlagsEnum>(out FlagsEnum f));
             Assert.AreEqual(FlagsEnum.Flag1 | FlagsEnum.Flag3, f);
             Assert.AreEqual(2, st.Position);
        }
    }
}
