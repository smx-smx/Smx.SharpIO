#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using Smx.SharpIO.Memory.Buffers;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

		public static ReadOnlySpan64<TTo> Cast<TFrom, TTo>(this ReadOnlySpan64<TFrom> data)
			where TFrom : unmanaged
			where TTo : unmanaged
		{
			return MemoryMarshal64.Cast<TFrom, TTo>(data);
		}

		public static long IndexOf<T>(this ReadOnlySpan64<T> span, T value) where T : IEquatable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOf(value);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOf<T>(this Span64<T> span, T value) where T : IEquatable<T>
			=> IndexOf((ReadOnlySpan64<T>)span, value);

		public static long IndexOfAny<T>(this ReadOnlySpan64<T> span, T value0, T value1) where T : IEquatable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAny(value0, value1);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOfAny<T>(this Span64<T> span, T value0, T value1) where T : IEquatable<T>
			=> IndexOfAny((ReadOnlySpan64<T>)span, value0, value1);

		public static long IndexOfAny<T>(this ReadOnlySpan64<T> span, T value0, T value1, T value2) where T : IEquatable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAny(value0, value1, value2);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOfAny<T>(this ReadOnlySpan64<T> span, ReadOnlySpan<T> values) where T : IEquatable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAny(values);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOfAnyExcept<T>(this ReadOnlySpan64<T> span, T value) where T : IEquatable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAnyExcept(value);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOfAnyExcept<T>(this ReadOnlySpan64<T> span, ReadOnlySpan<T> values) where T : IEquatable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAnyExcept(values);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOfAnyInRange<T>(this ReadOnlySpan64<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAnyInRange(lowInclusive, highInclusive);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long IndexOfAnyExceptInRange<T>(this ReadOnlySpan64<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
			long offset = 0;
			foreach (ReadOnlySpan<T> chunk in span.GetChunks()) {
				int index = chunk.IndexOfAnyExceptInRange(lowInclusive, highInclusive);
				if (index >= 0) return offset + index;
				offset += chunk.Length;
			}
			return -1;
		}

		public static long LastIndexOf<T>(this ReadOnlySpan64<T> span, T value) where T : IEquatable<T> {
			long remaining = span.Length;
			while (remaining > 0) {
				int chunkSize = (remaining > int.MaxValue) ? int.MaxValue : (int)remaining;
				long start = remaining - chunkSize;

				// We slice manually here because GetChunks() is forward-only
				ReadOnlySpan<T> chunk = (ReadOnlySpan<T>)span.Slice(start, chunkSize);

				int index = chunk.LastIndexOf(value);
				if (index >= 0) return start + index;

				remaining -= chunkSize;
			}
			return -1;
		}

		public static long LastIndexOfAny<T>(this ReadOnlySpan64<T> span, T value0, T value1) where T : IEquatable<T> {
			long remaining = span.Length;
			while (remaining > 0) {
				int chunkSize = (remaining > int.MaxValue) ? int.MaxValue : (int)remaining;
				long start = remaining - chunkSize;
				ReadOnlySpan<T> chunk = (ReadOnlySpan<T>)span.Slice(start, chunkSize);

				int index = chunk.LastIndexOfAny(value0, value1);
				if (index >= 0) return start + index;

				remaining -= chunkSize;
			}
			return -1;
		}

		public static long LastIndexOfAny<T>(this ReadOnlySpan64<T> span, ReadOnlySpan<T> values) where T : IEquatable<T> {
			long remaining = span.Length;
			while (remaining > 0) {
				int chunkSize = (remaining > int.MaxValue) ? int.MaxValue : (int)remaining;
				long start = remaining - chunkSize;
				ReadOnlySpan<T> chunk = (ReadOnlySpan<T>)span.Slice(start, chunkSize);

				int index = chunk.LastIndexOfAny(values);
				if (index >= 0) return start + index;

				remaining -= chunkSize;
			}
			return -1;
		}

		public static long LastIndexOfAnyExcept<T>(this ReadOnlySpan64<T> span, T value) where T : IEquatable<T> {
			long remaining = span.Length;
			while (remaining > 0) {
				int chunkSize = (remaining > int.MaxValue) ? int.MaxValue : (int)remaining;
				long start = remaining - chunkSize;
				ReadOnlySpan<T> chunk = (ReadOnlySpan<T>)span.Slice(start, chunkSize);

				int index = chunk.LastIndexOfAnyExcept(value);
				if (index >= 0) return start + index;

				remaining -= chunkSize;
			}
			return -1;
		}

		public static long LastIndexOfAnyInRange<T>(this ReadOnlySpan64<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
			long remaining = span.Length;
			while (remaining > 0) {
				int chunkSize = (remaining > int.MaxValue) ? int.MaxValue : (int)remaining;
				long start = remaining - chunkSize;
				ReadOnlySpan<T> chunk = (ReadOnlySpan<T>)span.Slice(start, chunkSize);

				int index = chunk.LastIndexOfAnyInRange(lowInclusive, highInclusive);
				if (index >= 0) return start + index;

				remaining -= chunkSize;
			}
			return -1;
		}

		public static long LastIndexOfAnyExceptInRange<T>(this ReadOnlySpan64<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
			long remaining = span.Length;
			while (remaining > 0) {
				int chunkSize = (remaining > int.MaxValue) ? int.MaxValue : (int)remaining;
				long start = remaining - chunkSize;
				ReadOnlySpan<T> chunk = (ReadOnlySpan<T>)span.Slice(start, chunkSize);

				int index = chunk.LastIndexOfAnyExceptInRange(lowInclusive, highInclusive);
				if (index >= 0) return start + index;

				remaining -= chunkSize;
			}
			return -1;
		}

		public static bool IsWhiteSpace(this ReadOnlySpan64<char> span) {
			// Standard .NET: returns true if span is Empty or all chars are whitespace
			if (span.IsEmpty) return true;

			foreach (ReadOnlySpan<char> chunk in span.GetChunks()) {
				if (!chunk.IsWhiteSpace()) {
					return false;
				}
			}
			return true;
		}

		public static void Replace<T>(this Span64<T> span, T oldValue, T newValue) where T : IEquatable<T> {
			foreach (Span<T> chunk in span.GetChunks()) {
				chunk.Replace(oldValue, newValue);
			}
		}

		public static void ReplaceAny<T>(this Span64<T> span, T oldValue0, T oldValue1, T newValue) where T : IEquatable<T> {
			foreach (Span<T> chunk in span.GetChunks()) {
				for (int i = 0; i < chunk.Length; i++) {
					if (chunk[i].Equals(oldValue0) || chunk[i].Equals(oldValue1)) {
						chunk[i] = newValue;
					}
				}
			}
		}

		public static void ReplaceAny<T>(this Span64<T> span, ReadOnlySpan<T> values, T newValue) where T : IEquatable<T> {
			foreach (Span<T> chunk in span.GetChunks()) {
				for (int i = 0; i < chunk.Length; i++) {
					if (values.Contains(chunk[i])) {
						chunk[i] = newValue;
					}
				}
			}
		}

		public static void ReplaceAnyExcept<T>(this Span64<T> span, T value, T newValue) where T : IEquatable<T> {
			foreach (Span<T> chunk in span.GetChunks()) {
				for (int i = 0; i < chunk.Length; i++) {
					if (!chunk[i].Equals(value)) {
						chunk[i] = newValue;
					}
				}
			}
		}

		public static void ReplaceAnyExcept<T>(this Span64<T> span, ReadOnlySpan<T> values, T newValue) where T : IEquatable<T> {
			foreach (Span<T> chunk in span.GetChunks()) {
				for (int i = 0; i < chunk.Length; i++) {
					if (!values.Contains(chunk[i])) {
						chunk[i] = newValue;
					}
				}
			}
		}


		public static unsafe bool Overlaps<T>(this ReadOnlySpan64<T> span, ReadOnlySpan64<T> other) {
			if (span.IsEmpty || other.IsEmpty)
				return false;

			// We need to compare the underlying memory addresses.
			// GetReference returns a ref to the 0th element.
			ref T ref1 = ref MemoryMarshal64.GetReference(span);
			ref T ref2 = ref MemoryMarshal64.GetReference(other);

			// Convert to pointers to do comparison
			void* ptr1 = Unsafe.AsPointer(ref ref1);
			void* ptr2 = Unsafe.AsPointer(ref ref2);

			ulong start1 = (ulong)ptr1;
			ulong lenBytes1 = (ulong)span.Length * (ulong)Unsafe.SizeOf<T>();
			ulong end1 = start1 + lenBytes1;

			ulong start2 = (ulong)ptr2;
			ulong lenBytes2 = (ulong)other.Length * (ulong)Unsafe.SizeOf<T>();
			ulong end2 = start2 + lenBytes2;

			// Check if ranges overlap
			return start1 < end2 && end1 > start2;
		}

		public static bool Overlaps<T>(this Span64<T> span, ReadOnlySpan64<T> other)
			=> Overlaps((ReadOnlySpan64<T>)span, other);

		public static unsafe void Reverse<T>(this Span64<T> span) {
			if (span.Length <= 1) return;

			// Note: We cannot chunk Reverse easily.
			// We use a standard pointer swap loop.

			ref T first = ref MemoryMarshal64.GetReference(span);
			// Calculate last element reference: first + (length - 1)
			// MemoryMarshal64.GetReference usually handles the pointer conversion

			// To be safe with large offsets, we do manual pointer arithmetic
			void* ptrBase = Unsafe.AsPointer(ref first);
			long elementSize = Unsafe.SizeOf<T>();

			byte* bytePtrLow = (byte*)ptrBase;
			byte* bytePtrHigh = bytePtrLow + ((span.Length - 1) * elementSize);

			// Loop until pointers meet or cross
			while (bytePtrLow < bytePtrHigh) {
				// Get ref to T at Low and High
				ref T tLow = ref Unsafe.AsRef<T>(bytePtrLow);
				ref T tHigh = ref Unsafe.AsRef<T>(bytePtrHigh);

				// Swap
				T temp = tLow;
				tLow = tHigh;
				tHigh = temp;

				// Move pointers
				bytePtrLow += elementSize;
				bytePtrHigh -= elementSize;
			}
		}
	}
}
