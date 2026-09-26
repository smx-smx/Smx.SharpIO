#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using Smx.SharpIO.Memory.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Smx.SharpIO.Extensions
{
	/// <summary>
	/// Struct readers for raw spans with <see cref="EndianAttribute"/> support.
	/// Moved from Yafex (Yafex.Support.SpanExtensions.ReadStruct/GetField) and
	/// unified with <see cref="SpanStream"/> endianness handling: supports both
	/// [MarshalAs(ByValArray)] and [InlineArray] fields, and skips byte-size
	/// elements so InlineArray magic bytes are never swapped.
	/// </summary>
	public static class StructEx
	{
		private static int FieldSize(FieldInfo field)
		{
			InlineArrayAttribute? inlineArray = field.FieldType.GetCustomAttribute<InlineArrayAttribute>();
			if (field.FieldType.IsArray || inlineArray != null) {
				if (inlineArray != null) {
					var elementType = field.FieldType
						.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						.FirstOrDefault()?.FieldType;
					if (elementType == null) {
						throw new InvalidOperationException("InlineArray is empty");
					}
					return Marshal.SizeOf(elementType) * inlineArray.Length;
				} else {
					var attr = field.GetCustomAttribute<MarshalAsAttribute>(false);
					if (attr == null) {
						throw new InvalidOperationException("Cannot determine Field Size: missing [MarshalAs] attribute");
					}
					var elementType = field.FieldType.GetElementType();
					if (elementType == null) {
						throw new InvalidOperationException();
					}
					return Marshal.SizeOf(elementType) * attr.SizeConst;
				}
			} else {
				if (field.FieldType.IsEnum) {
					return Marshal.SizeOf(Enum.GetUnderlyingType(field.FieldType));
				}
				return Marshal.SizeOf(field.FieldType);
			}
		}

		private static void SwapEndian<T>(byte[] data, FieldInfo field, Dictionary<int, int> seenOffsets)
		{
			var type = typeof(T);
			int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
			int fieldSize = FieldSize(field);

			// single-byte elements never need swapping (e.g. magic bytes)
			if (fieldSize == 1) {
				return;
			}

			// $NOTE: this doesn't work in case of 2 overlapping unions
			if (seenOffsets.TryGetValue(offset, out var lastSize) && lastSize >= fieldSize) {
				return;
			} else {
				seenOffsets[offset] = fieldSize;
			}

			InlineArrayAttribute? inlineArray = field.FieldType.GetCustomAttribute<InlineArrayAttribute>();
			if (field.FieldType.IsArray || inlineArray != null) {
				Type? elementType = null;
				int length = 0;
				if (inlineArray != null) {
					elementType = field.FieldType
						.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						.FirstOrDefault()?.FieldType;
					length = inlineArray.Length;
				} else {
					var attr = field.GetCustomAttribute<MarshalAsAttribute>(false);
					elementType = field.FieldType.GetElementType();
					length = attr?.SizeConst ?? 0;
				}
				if (elementType != null && length > 0) {
					int subSize = Marshal.SizeOf(elementType);
					if (subSize > 1) {
						for (int i = 0; i < length; i++) {
							Array.Reverse(data, offset + (i * subSize), subSize);
						}
					}
					return;
				}
			}

			Array.Reverse(data, offset, fieldSize);
		}

		/* Adapted from http://stackoverflow.com/a/2624377 */
		internal static T RespectEndianness<T>(T data, Endianness streamEndianness) where T : notnull
		{
			var structEndianness = streamEndianness;
			var type = typeof(T);
			if (type.IsDefined(typeof(EndianAttribute), false)) {
				var attr = type.GetCustomAttribute<EndianAttribute>(false);
				if (attr != null) {
					structEndianness = attr.Endianness;
				}
			}

			var sz = Marshal.SizeOf(data);
			var mem = Marshal.AllocHGlobal(sz);
			try {
				try {
					Marshal.StructureToPtr(data, mem, false);
				} catch (ArgumentException) {
					// garbage (e.g. encrypted) data with unencodable strings:
					// return unswapped so callers can Fail gracefully
					return data;
				}
				var bytes = new byte[sz];
				Marshal.Copy(mem, bytes, 0, sz);

				var seenOffsets = new Dictionary<int, int>();
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
					if (field.IsDefined(typeof(EndianAttribute), false)) {
						Endianness fieldEndianess = ((EndianAttribute)field.GetCustomAttributes(typeof(EndianAttribute), false)[0]).Endianness;
						if (
							(fieldEndianess == Endianness.BigEndian && BitConverter.IsLittleEndian) ||
							(fieldEndianess == Endianness.LittleEndian && !BitConverter.IsLittleEndian)
						) {
							SwapEndian<T>(bytes, field, seenOffsets);
						}
					} else if (
						(structEndianness == Endianness.BigEndian && BitConverter.IsLittleEndian) ||
						(structEndianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian)
					) {
						SwapEndian<T>(bytes, field, seenOffsets);
					}
				}
				Marshal.Copy(bytes, 0, mem, sz);
				var res = Marshal.PtrToStructure<T>(mem);
				if (res == null) {
					throw new InvalidOperationException("Marshal.PtrToStructure failed");
				}
				data = res;
			} finally {
				Marshal.FreeHGlobal(mem);
			}

			return data;
		}

		internal static T RespectEndianness<T>(T data) where T : notnull
		{
			// raw spans have no stream endianness; the struct attribute decides
			// (defaults to little-endian, matching Yafex firmware headers)
			return RespectEndianness(data, Endianness.LittleEndian);
		}

		public static unsafe T ReadStruct<T>(this Span64<byte> data, long offset = 0) where T : struct
		{
			fixed (byte* dptr = data) {
				return RespectEndianness(Marshal.PtrToStructure<T>(new IntPtr(dptr + offset)));
			}
		}

		public static unsafe T ReadStruct<T>(this Span<byte> data, int offset = 0) where T : struct
		{
			fixed (byte* dptr = data) {
				return RespectEndianness(Marshal.PtrToStructure<T>(new IntPtr(dptr + offset)));
			}
		}

		public static unsafe T ReadStruct<T>(this ReadOnlySpan<byte> data, int offset = 0) where T : struct
		{
			fixed (byte* dptr = data) {
				return RespectEndianness(Marshal.PtrToStructure<T>(new IntPtr(dptr + offset)));
			}
		}

		public static unsafe T ReadStruct<T>(this ReadOnlySpan64<byte> data, long offset = 0) where T : struct
		{
			fixed (byte* dptr = data) {
				return RespectEndianness(Marshal.PtrToStructure<T>(new IntPtr(dptr + offset)));
			}
		}

		public static unsafe T ReadStruct<T>(this Memory<byte> data, int offset = 0) where T : struct
		{
			return ReadStruct<T>(data.Span, offset);
		}

		public static unsafe T ReadStruct<T>(this Memory64<byte> data, long offset = 0) where T : struct
		{
			return ReadStruct<T>(data.Span, offset);
		}

		public static unsafe T ReadStruct<T>(this ReadOnlyMemory<byte> data, int offset = 0) where T : struct
		{
			return ReadStruct<T>(data.Span, offset);
		}

		public static unsafe T ReadStruct<T>(this ReadOnlyMemory64<byte> data, long offset = 0) where T : struct
		{
			return ReadStruct<T>(data.Span, offset);
		}

		public static bool TryReadStruct<T>(this Span64<byte> data, long offset, out T value) where T : struct
		{
			value = default;
			try {
				if (offset < 0 || offset + Marshal.SizeOf<T>() > data.Length) {
					return false;
				}
				value = ReadStruct<T>(data, offset);
				return true;
			} catch {
				return false;
			}
		}

		public static bool TryReadStruct<T>(this Span<byte> data, int offset, out T value) where T : struct
		{
			value = default;
			try {
				if (offset < 0 || offset + Marshal.SizeOf<T>() > data.Length) {
					return false;
				}
				value = ReadStruct<T>(data, offset);
				return true;
			} catch {
				return false;
			}
		}

		public static bool TryReadStruct<T>(this ReadOnlySpan<byte> data, int offset, out T value) where T : struct
		{
			value = default;
			try {
				if (offset < 0 || offset + Marshal.SizeOf<T>() > data.Length) {
					return false;
				}
				value = ReadStruct<T>(data, offset);
				return true;
			} catch {
				return false;
			}
		}

		public static bool TryReadStruct<T>(this ReadOnlySpan64<byte> data, long offset, out T value) where T : struct
		{
			value = default;
			try {
				if (offset < 0 || offset + Marshal.SizeOf<T>() > data.Length) {
					return false;
				}
				value = ReadStruct<T>(data, offset);
				return true;
			} catch {
				return false;
			}
		}

		public static Memory<T> GetField<T, TStruct, TField>(this Memory<T> memory, string fieldName)
			where T : unmanaged
			where TStruct : struct
			where TField : struct
		{
			var offset = Marshal.OffsetOf<TStruct>(fieldName).ToInt32();
			var length = Marshal.SizeOf<TField>();
			return memory.Cast<T, byte>()
				.Slice(offset, length)
				.Cast<byte, T>();
		}

		public static Memory64<T> GetField<T, TStruct, TField>(this Memory64<T> memory, string fieldName)
			where T : unmanaged
			where TStruct : struct
			where TField : struct
		{
			var offset = Marshal.OffsetOf<TStruct>(fieldName).ToInt32();
			var length = Marshal.SizeOf<TField>();
			return memory.Cast<T, byte>()
				.Slice(offset, length)
				.Cast<byte, T>();
		}

		public static Span<T> GetField<T, TStruct, TField>(this Span<T> span, string fieldName)
			where T : unmanaged
			where TStruct : struct
			where TField : struct
		{
			var offset = Marshal.OffsetOf<TStruct>(fieldName).ToInt32();
			var length = Marshal.SizeOf<TField>();
			var bytes = MemoryMarshal.Cast<T, byte>(span).Slice(offset, length);
			return MemoryMarshal.Cast<byte, T>(bytes);
		}

		public static Span64<T> GetField<T, TStruct, TField>(this Span64<T> span, string fieldName)
			where T : unmanaged
			where TStruct : struct
			where TField : struct
		{
			var offset = Marshal.OffsetOf<TStruct>(fieldName).ToInt32();
			var length = Marshal.SizeOf<TField>();
			return span.Cast<T, byte>()
				.Slice(offset, length)
				.Cast<byte, T>();
		}

		public static ReadOnlySpan<T> GetField<T, TStruct, TField>(this ReadOnlySpan<T> span, string fieldName)
			where T : unmanaged
			where TStruct : struct
			where TField : struct
		{
			var offset = Marshal.OffsetOf<TStruct>(fieldName).ToInt32();
			var length = Marshal.SizeOf<TField>();
			var bytes = MemoryMarshal.Cast<T, byte>(span).Slice(offset, length);
			return MemoryMarshal.Cast<byte, T>(bytes);
		}

		public static ReadOnlySpan64<T> GetField<T, TStruct, TField>(this ReadOnlySpan64<T> span, string fieldName)
			where T : unmanaged
			where TStruct : struct
			where TField : struct
		{
			var offset = Marshal.OffsetOf<TStruct>(fieldName).ToInt32();
			var length = Marshal.SizeOf<TField>();
			return span.Cast<T, byte>()
				.Slice(offset, length)
				.Cast<byte, T>();
		}
	}
}
