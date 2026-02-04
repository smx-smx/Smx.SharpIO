#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Runtime.InteropServices;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO.Extensions
{
	public static class SpanEx
	{
		public unsafe static T Read<T>(this ReadOnlySpan<byte> data, int offset) where T : unmanaged {
			int length = sizeof(T);
			return data.Slice(offset, length).Cast<T>()[0];
		}

		public unsafe static T Read<T>(this ReadOnlySpan64<byte> data, long offset) where T : unmanaged {
			long length = sizeof(T);
			return data.Slice(offset, length).Cast<T>()[0];
		}

		public unsafe static T Read<T>(this Span<byte> data, int offset) where T : unmanaged {
			int length = sizeof(T);
			return data.Slice(offset, length).Cast<T>()[0];
		}

		public unsafe static T Read<T>(this Span64<byte> data, long offset) where T : unmanaged {
			long length = sizeof(T);
			return data.Slice(offset, length).Cast<T>()[0];
		}

		public unsafe static void Write<T>(this Span<byte> data, int offset, T value) where T : unmanaged {
			int length = sizeof(T);
			data.Slice(offset, length).Cast<T>()[0] = value;
		}

		public unsafe static void Write<T>(this Span64<byte> data, long offset, T value) where T : unmanaged {
			long length = sizeof(T);
			data.Slice(offset, length).Cast<T>()[0] = value;
		}

		public static void CopyTo<TFrom, TTo>(this Span<TFrom> data, Span<TTo> dest, int dstOffset)
			where TFrom : unmanaged
			where TTo : unmanaged {
			var srcBytes = MemoryMarshal.Cast<TFrom, byte>(data);
			var dstBytes = MemoryMarshal.Cast<TTo, byte>(dest).Slice(dstOffset);
			srcBytes.CopyTo(dstBytes);
		}

		public static void CopyTo<TFrom, TTo>(this Span64<TFrom> data, Span64<TTo> dest, long dstOffset)
			where TFrom : unmanaged
			where TTo : unmanaged {
			var srcBytes = MemoryMarshal64.Cast<TFrom, byte>(data);
			var dstBytes = MemoryMarshal64.Cast<TTo, byte>(dest).Slice(dstOffset);
			srcBytes.CopyTo(dstBytes);
		}

		public static void CopyTo<TFrom, TTo>(this Memory<TFrom> data, Memory<TTo> dest, int dstOffset)
			where TFrom : unmanaged
			where TTo : unmanaged {
			data.Span.CopyTo(dest.Span, dstOffset);
		}

		public static void CopyTo<TFrom, TTo>(this Memory64<TFrom> data, Memory64<TTo> dest, long dstOffset)
			where TFrom : unmanaged
			where TTo : unmanaged {
			data.Span.CopyTo(dest.Span, dstOffset);
		}

		public static void WriteBytes(this Span<byte> data, int offset, byte[] bytes) {
			var start = data.Slice(offset, bytes.Length);
			var dspan = new Span<byte>(bytes);
			dspan.CopyTo(start);
		}

		public static void WriteBytes(this Span64<byte> data, long offset, byte[] bytes) {
			var start = data.Slice(offset, bytes.Length);
			var dspan = new Span64<byte>(bytes);
			dspan.CopyTo(start);
		}

		public static ReadOnlySpan<T> Cast<T>(this ReadOnlySpan<byte> data) where T : unmanaged {
			return MemoryMarshal.Cast<byte, T>(data);
		}

		public static ReadOnlySpan64<T> Cast<T>(this ReadOnlySpan64<byte> data) where T : unmanaged {
			return MemoryMarshal64.Cast<byte, T>(data);
		}

		public static Span<TTo> Cast<TTo>(this Span<byte> data) where TTo : unmanaged { 
			return MemoryMarshal.Cast<byte, TTo>(data);
		}

		public static Span64<TTo> Cast<TTo>(this Span64<byte> data) where TTo : unmanaged { 
			return MemoryMarshal64.Cast<byte, TTo>(data);
		}

		public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> data)
			where TFrom : unmanaged 
			where TTo : unmanaged
		{
			return MemoryMarshal.Cast<TFrom, TTo>(data);
		}

		public static Span64<TTo> Cast<TFrom, TTo>(this Span64<TFrom> data)
			where TFrom : unmanaged 
			where TTo : unmanaged
		{
			return MemoryMarshal64.Cast<TFrom, TTo>(data);
		}
	}
}
