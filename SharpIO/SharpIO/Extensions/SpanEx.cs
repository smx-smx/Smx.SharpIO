#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Smx.SharpIO.Extensions
{
	public static class SpanEx
	{
		public unsafe static T Read<T>(this ReadOnlySpan<byte> data, int offset) where T : unmanaged {
			int length = sizeof(T);
			return data.Slice(offset, length).Cast<T>()[0];
		}

		public unsafe static T Read<T>(this Span<byte> data, int offset) where T : unmanaged {
			int length = sizeof(T);
			return data.Slice(offset, length).Cast<T>()[0];
		}

		public unsafe static void Write<T>(this Span<byte> data, int offset, T value) where T : unmanaged {
			int length = sizeof(T);
			data.Slice(offset, length).Cast<T>()[0] = value;
		}

		public unsafe static void CopyTo<TFrom, TTo>(this Span<TFrom> data, Span<TTo> dest, int dstOffset)
			where TFrom : unmanaged
			where TTo : unmanaged {
			var srcBytes = MemoryMarshal.Cast<TFrom, byte>(data);
			var dstBytes = MemoryMarshal.Cast<TTo, byte>(dest).Slice(dstOffset);
			srcBytes.CopyTo(dstBytes);
		}

		public unsafe static void CopyTo<TFrom, TTo>(this Memory<TFrom> data, Memory<TTo> dest, int dstOffset)
			where TFrom : unmanaged
			where TTo : unmanaged {
			data.Span.CopyTo(dest.Span, dstOffset);
		}

		public unsafe static void WriteBytes(this Span<byte> data, int offset, byte[] bytes) {
			var start = data.Slice(offset, bytes.Length);
			var dspan = new Span<byte>(bytes);
			dspan.CopyTo(start);
		}

		public static ReadOnlySpan<T> Cast<T>(this ReadOnlySpan<byte> data) where T : unmanaged {
			return MemoryMarshal.Cast<byte, T>(data);
		}

		public static Span<TTo> Cast<TTo>(this Span<byte> data) where TTo : unmanaged { 
			return MemoryMarshal.Cast<byte, TTo>(data);
		}

		public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> data)
			where TFrom : unmanaged 
			where TTo : unmanaged
		{
			return MemoryMarshal.Cast<TFrom, TTo>(data);
		}
	}
}
