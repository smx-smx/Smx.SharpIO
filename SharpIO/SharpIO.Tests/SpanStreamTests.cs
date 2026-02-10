using NUnit.Framework;
using Smx.SharpIO;
using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

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

public struct UShortStruct {
	public ushort Value;
}

public enum ByteEnum : byte {
	A = 1, B = 2
}

[Flags]
public enum FlagsEnum : ushort {
	None = 0,
	Flag1 = 1,
	Flag2 = 2,
	Flag3 = 4
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

	[Test]
	public void TestBigEndianStruct_WithUShort() {
		var buf = new byte[] { 0x00, 0x01 };
		var st = new SpanStream(buf, Endianness.BigEndian);
		var data = st.ReadStruct<UShortStruct>();
		Assert.AreEqual(0x1, data.Value);
	}

	[Test]
	public void TestReadWritePrimitives_LittleEndian() {
		var buf = new byte[100];
		var st = new SpanStream(buf);
		st.Endianness = Endianness.LittleEndian;

		st.WriteByte(0x12);
		st.WriteUInt16(0x1234);
		st.WriteUInt32(0x12345678);
		st.WriteUInt64(0x123456789ABCDEF0);
		st.WriteSByte(-1);
		st.WriteInt16(-2);
		st.WriteInt32(-3);
		st.WriteInt64(-4);

		st.Position = 0;

		Assert.AreEqual(0x12, st.ReadByte());
		Assert.AreEqual(0x1234, st.ReadUInt16());
		Assert.AreEqual(0x12345678, st.ReadUInt32());
		Assert.AreEqual(0x123456789ABCDEF0, st.ReadUInt64());
		Assert.AreEqual(-1, st.ReadSbyte());
		Assert.AreEqual(-2, st.ReadInt16());
		Assert.AreEqual(-3, st.ReadInt32());
		Assert.AreEqual(-4, st.ReadInt64());
	}

	[Test]
	public void TestReadWritePrimitives_BigEndian() {
		var buf = new byte[100];
		var st = new SpanStream(buf);
		st.Endianness = Endianness.BigEndian;

		st.WriteUInt16(0x1234);
		st.WriteUInt32(0x12345678);
		st.WriteUInt64(0x123456789ABCDEF0);

		st.Position = 0;
		// Verify raw bytes are big endian
		Assert.AreEqual(0x12, st.ReadByte());
		Assert.AreEqual(0x34, st.ReadByte()); 
		
		st.Position = 0;
		Assert.AreEqual(0x1234, st.ReadUInt16());
		Assert.AreEqual(0x12345678, st.ReadUInt32());
		Assert.AreEqual(0x123456789ABCDEF0, st.ReadUInt64());
	}

	[Test]
	public void TestSeekAndPosition() {
		var buf = new byte[10];
		var st = new SpanStream(buf);
		
		Assert.AreEqual(0, st.Position);
		st.WriteByte(1);
		Assert.AreEqual(1, st.Position);
		
		st.Seek(2, SeekOrigin.Begin);
		Assert.AreEqual(2, st.Position);
		
		st.Seek(2, SeekOrigin.Current);
		Assert.AreEqual(4, st.Position);
		
		st.Seek(1, SeekOrigin.End); // 10 - 1 = 9
		Assert.AreEqual(9, st.Position);
	}

	[Test]
	public void TestSlicing() {
		var buf = new byte[] { 0, 1, 2, 3, 4, 5 };
		var st = new SpanStream(buf);
		st.Position = 2;
		
		var slice = st.SliceHere();
		Assert.AreEqual(0, slice.Position);
		Assert.AreEqual(4, slice.Length);
		Assert.AreEqual(2, slice.ReadByte());
		
		var sliceLen = st.SliceHere(2);
		Assert.AreEqual(2, sliceLen.Length);
		Assert.AreEqual(2, sliceLen.ReadByte());
		Assert.AreEqual(3, sliceLen.ReadByte());
		Assert.Throws<ArgumentOutOfRangeException>(() => sliceLen.ReadByte());
	}

	[Test]
	public void TestStrings() {
		var buf = new byte[100];
		var st = new SpanStream(buf);
		
		string testStr = "Hello";
		st.WriteString(testStr, nullTerminator: true);
		long posAfterNull = st.Position;
		
		st.WriteString(testStr, lengthPrefix: true, prefixLength: 4);
		
		st.Position = 0;
		Assert.AreEqual(testStr, st.ReadCString());
		Assert.AreEqual(posAfterNull, st.Position);
		
		uint len = st.ReadUInt32();
		Assert.AreEqual(testStr.Length, len);
		Assert.AreEqual(testStr, st.ReadString(len));
	}

	[Test]
	public void TestEnums() {
		var buf = new byte[10];
		var st = new SpanStream(buf);
		
		st.WriteByte((byte)ByteEnum.B);
		st.WriteUInt16((ushort)(FlagsEnum.Flag1 | FlagsEnum.Flag3)); // 1 | 4 = 5
		
		st.Position = 0;
		Assert.AreEqual(ByteEnum.B, st.ReadEnum<ByteEnum>());
		
		var flags = st.ReadFlagsEnum<FlagsEnum>();
		Assert.IsTrue(flags.HasFlag(FlagsEnum.Flag1));
		Assert.IsTrue(flags.HasFlag(FlagsEnum.Flag3));
		Assert.IsFalse(flags.HasFlag(FlagsEnum.Flag2));
	}

	[Test]
	public void TestPerformAt() {
		var buf = new byte[] { 1, 2, 3 };
		var st = new SpanStream(buf);
		
		st.PerformAt(1, () => {
			Assert.AreEqual(1, st.Position);
			Assert.AreEqual(2, st.ReadByte());
		});
		
		Assert.AreEqual(0, st.Position);
		
		int val = st.PerformAt(2, () => {
			return st.ReadByte();
		});
		Assert.AreEqual(3, val);
		Assert.AreEqual(0, st.Position);
	}
	
	[Test]
	public void TestAlignStream() {
		var buf = new byte[16];
		var st = new SpanStream(buf);
		
		st.WriteByte(1); // Pos 1
		long skipped = st.AlignStream(4);
		
		Assert.AreEqual(4, st.Position);
		Assert.AreEqual(3, skipped);
		
		st.Position = 4;
		skipped = st.AlignStream(4);
		Assert.AreEqual(4, st.Position);
		Assert.AreEqual(0, skipped);
	}
}
