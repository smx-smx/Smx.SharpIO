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
using System.Text;

namespace Smx.SharpIO.Extensions
{
	/// <summary>
	/// Null-terminated string helpers for raw spans. Moved from Yafex
	/// (Yafex.Support.SpanExtensions.AsString) so firmware parsers and
	/// other binary readers share one implementation.
	/// </summary>
	public static class StringEx
	{
		public static string AsString<TBuffer>(this TBuffer buffer, Encoding encoding)
			where TBuffer : struct
		{
			int length = System.Runtime.CompilerServices.Unsafe.SizeOf<TBuffer>();
			ref var bufferRef = ref System.Runtime.CompilerServices.Unsafe.AsRef(in buffer);
			ref var start = ref System.Runtime.CompilerServices.Unsafe.As<TBuffer, byte>(ref buffer);
			var span = System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref start, length);
			return span.AsString(encoding);
		}

		public static string AsString(this ReadOnlyMemory64<byte> data, Encoding encoding)
		{
			// strings larger than 2GB are not expected; fall back to 32-bit span
			return AsString((ReadOnlySpan<byte>)data.Span, encoding);
		}

		public static string AsString(this Memory64<byte> data, Encoding encoding)
		{
			return AsString((ReadOnlySpan<byte>)data.Span, encoding);
		}

		public static string AsString(this ReadOnlyMemory<byte> data, Encoding encoding)
		{
			return AsString(data.Span, encoding);
		}

		public static string AsString(this Memory<byte> data, Encoding encoding)
		{
			return AsString(data.Span, encoding);
		}

		public static string AsString(this ReadOnlySpan<byte> data, Encoding encoding)
		{
			var nullPos = data.IndexOf((byte)0x00);
			if (nullPos < 0) {
				return encoding.GetString(data);
			} else {
				return encoding.GetString(data.Slice(0, nullPos));
			}
		}

		public static string AsString(this Span<byte> data, Encoding encoding)
		{
			var nullPos = data.IndexOf((byte)0x00);
			if (nullPos < 0) {
				return encoding.GetString(data);
			} else {
				return encoding.GetString(data.Slice(0, nullPos));
			}
		}

		public static string AsString(this ReadOnlySpan64<byte> data, Encoding encoding)
		{
			var nullPos = data.IndexOf((byte)0x00);
			if (nullPos < 0) {
				return encoding.GetString(data.ToArray());
			} else {
				return encoding.GetString(data.Slice(0, nullPos).ToArray());
			}
		}

		public static string AsString(this Span64<byte> data, Encoding encoding)
		{
			return AsString((ReadOnlySpan64<byte>)data, encoding);
		}
	}
}
