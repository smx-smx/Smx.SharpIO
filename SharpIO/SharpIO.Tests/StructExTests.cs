using NUnit.Framework;
using Smx.SharpIO;
using Smx.SharpIO.Extensions;
using Smx.SharpIO.Memory.Buffers;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpIO.Tests
{
	[StructLayout(LayoutKind.Sequential)]
	public struct TestHeader
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] magic;
		public uint fileSize;
	}

	public class StructExTests
	{
		[Test]
		public void AsString_StopsAtNull()
		{
			var buf = new byte[] { (byte)'A', (byte)'B', 0, (byte)'C' };
			Assert.AreEqual("AB", ((ReadOnlySpan<byte>)buf).AsString(Encoding.ASCII));
			Assert.AreEqual("AB", ((Span<byte>)buf).AsString(Encoding.ASCII));
		}

		[Test]
		public void AsString_NoNull_ReturnsFull()
		{
			var buf = new byte[] { (byte)'H', (byte)'i' };
			Assert.AreEqual("Hi", ((ReadOnlySpan<byte>)buf).AsString(Encoding.ASCII));
		}

		[Test]
		public void AsString_Span64_StopsAtNull()
		{
			var buf = new byte[] { (byte)'X', 0, (byte)'Y' };
			Span64<byte> span = new Span64<byte>(buf);
			Assert.AreEqual("X", span.AsString(Encoding.ASCII));
			ReadOnlySpan64<byte> ro = new ReadOnlySpan64<byte>(buf);
			Assert.AreEqual("X", ro.AsString(Encoding.ASCII));
		}

		[Test]
		public void ReadStruct_Span_RoundTrips()
		{
			var hdr = new TestHeader { magic = new byte[] { (byte)'E', (byte)'P', (byte)'K', 0 }, fileSize = 0x12345678 };
			int sz = Marshal.SizeOf<TestHeader>();
			var buf = new byte[sz];
			var mem = Marshal.AllocHGlobal(sz);
			try {
				Marshal.StructureToPtr(hdr, mem, false);
				Marshal.Copy(mem, buf, 0, sz);
			} finally {
				Marshal.DestroyStructure<TestHeader>(mem);
				Marshal.FreeHGlobal(mem);
			}
			var back = ((ReadOnlySpan<byte>)buf).ReadStruct<TestHeader>();
			Assert.AreEqual("EPK", ((ReadOnlySpan<byte>)back.magic).AsString(Encoding.ASCII));
			Assert.AreEqual(0x12345678u, back.fileSize);
		}

		[Test]
		public void TryReadStruct_FailsOnShort()
		{
			var buf = new byte[] { 0x01 };
			Assert.IsFalse(((ReadOnlySpan<byte>)buf).TryReadStruct(0, out TestHeader _));
			var ok = new byte[Marshal.SizeOf<TestHeader>()];
			Assert.IsTrue(((ReadOnlySpan<byte>)ok).TryReadStruct(0, out TestHeader _));
		}

		[Test]
		public void GetField_SlicesHeader()
		{
			var hdr = new TestHeader { magic = new byte[] { 1, 2, 3, 4 }, fileSize = 42 };
			int sz = Marshal.SizeOf<TestHeader>();
			var buf = new byte[sz];
			var mem = Marshal.AllocHGlobal(sz);
			try {
				Marshal.StructureToPtr(hdr, mem, false);
				Marshal.Copy(mem, buf, 0, sz);
			} finally {
				Marshal.DestroyStructure<TestHeader>(mem);
				Marshal.FreeHGlobal(mem);
			}
			var magic = ((ReadOnlySpan<byte>)buf).GetField<byte, TestHeader, uint>("fileSize");
			Assert.AreEqual(4, magic.Length);
		}

		[Test]
		public void ReadStruct_ShortBuffer_ThrowsManaged()
		{
			// used to overread natively (SIGSEGV on tiny nested artifacts)
			var tiny = new byte[4];
			Assert.Throws<ArgumentOutOfRangeException>(() => ((ReadOnlySpan<byte>)tiny).ReadStruct<TestHeader>());
		}
	}
}
