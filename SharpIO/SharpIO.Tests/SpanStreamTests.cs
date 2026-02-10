using NUnit.Framework;
using Smx.SharpIO;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace SharpIO.Tests;

[InlineArray(4)]
public struct TestInlineArray {
	private byte _element0;
}

[Endian(Endianness.BigEndian)]
public struct TestStructWithInlineArray {
	public TestInlineArray Arr;
}

[Endian(Endianness.BigEndian)]
public struct TestCrashStruct {
	public static byte[] DO_NOT_CRASH = [0x11, 0x22, 0x33, 0x44];

	public uint Foo;
	public uint Bar;
}

public class SpanStreamTests
{
	[SetUp]
	public void Setup() {
	}

	[Test]
	public void TestBigEndianInlineArray() {
		var buf = new byte[] {
			0xDE, 0xAD, 0xBE, 0xEF
		};
		var st = new SpanStream(buf);
		var data = st.ReadStruct<TestStructWithInlineArray>();
		var bytes = ((ReadOnlySpan<byte>)data.Arr).ToArray();
		Assert.AreEqual(buf, bytes);
	}

	[Test]
	public void TestBigEndianStructCrash_WithStaticElement() {
		var buf = new byte[] {
			0xDE, 0xAD, 0xBE, 0xEF,
			0xA0, 0xB0, 0xC0, 0xD0
		};
		var st = new SpanStream(buf);
		var data = st.ReadStruct<TestCrashStruct>();
		Assert.AreEqual(0xDEADBEEF, data.Foo);
		Assert.AreEqual(0xA0B0C0D0, data.Bar);
	}
}
