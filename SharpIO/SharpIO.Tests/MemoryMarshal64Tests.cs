using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO.Tests
{
    public class MemoryMarshal64Tests
    {
        [Test]
        public void AsBytes_Span_Works()
        {
            var ints = new int[] { 0x11223344, 0x55667788 };
            var span = new Span64<int>(ints);
            var byteSpan = MemoryMarshal64.AsBytes(span);

            Assert.AreEqual(8, byteSpan.Length);
            
            if (BitConverter.IsLittleEndian)
            {
                Assert.AreEqual(0x44, byteSpan[0]);
                Assert.AreEqual(0x33, byteSpan[1]);
                Assert.AreEqual(0x22, byteSpan[2]);
                Assert.AreEqual(0x11, byteSpan[3]);
            }
        }

        [Test]
        public void AsBytes_ReadOnlySpan_Works()
        {
            var ints = new int[] { 0x11223344 };
            var span = new ReadOnlySpan64<int>(ints);
            var byteSpan = MemoryMarshal64.AsBytes(span);

            Assert.AreEqual(4, byteSpan.Length);
        }

        [Test]
        public void CreateSpan_Works()
        {
            int value = 42;
            var span = MemoryMarshal64.CreateSpan(ref value, 1);
            Assert.AreEqual(1, span.Length);
            Assert.AreEqual(42, span[0]);
            
            span[0] = 100;
            Assert.AreEqual(100, value);
        }

        [Test]
        public void Read_Write_Works()
        {
            var buffer = new byte[Unsafe.SizeOf<int>()];
            var span = new Span64<byte>(buffer);
            int value = 123456789;
            
            MemoryMarshal64.Write(span, ref value);
            
            var readBack = MemoryMarshal64.Read<int>(span);
            Assert.AreEqual(value, readBack);
        }

        [Test]
        public void TryGetArray_Works()
        {
            var array = new int[10];
            ReadOnlyMemory64<int> memory = array;
            
            bool success = MemoryMarshal64.TryGetArray(memory, out var segment);
            Assert.IsTrue(success);
            Assert.AreSame(array, segment.Array);
            Assert.AreEqual(0, segment.Offset);
            Assert.AreEqual(10, segment.Count);
        }
        
        [Test]
        public void AsMemory_Works()
        {
            var array = new int[5];
            ReadOnlyMemory64<int> rom = array;
            var mem = MemoryMarshal64.AsMemory(rom);
            
            Assert.AreEqual(5, mem.Length);
            // Verify it points to the same object
            mem.Span[0] = 99;
            Assert.AreEqual(99, array[0]);
        }

        [Test]
        public void AsRef_Works()
        {
            var ints = new int[] { 42 };
            var span = new Span64<int>(ints);
            ref int r = ref MemoryMarshal64.AsRef(span);
            r = 99;
            Assert.AreEqual(99, ints[0]);
            
            var roSpan = new ReadOnlySpan64<int>(ints);
            ref int r2 = ref MemoryMarshal64.AsRef(roSpan);
            Assert.AreEqual(99, r2);
        }

        [Test]
        public void ToEnumerable_Works()
        {
            var ints = new int[] { 10, 20, 30 };
            ReadOnlyMemory64<int> memory = ints;
            var list = new System.Collections.Generic.List<int>(MemoryMarshal64.ToEnumerable(memory));
            
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(20, list[1]);
            Assert.AreEqual(30, list[2]);
        }
    }
}
