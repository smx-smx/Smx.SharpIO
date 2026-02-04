#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Runtime.CompilerServices;

namespace Smx.SharpIO.Memory.Buffers;

public static class MemoryMarshal64
{
    /// <summary>
    /// Casts a Span64 of one primitive type to a Span64 of another primitive type.
    /// </summary>
    public static Span64<TTo> Cast<TFrom, TTo>(Span64<TFrom> span)
        where TFrom : struct
        where TTo : struct {
        if (span.IsEmpty) {
            return default;
        }

        long sourceLength = span.Length;
        int fromSize = Unsafe.SizeOf<TFrom>();
        int toSize = Unsafe.SizeOf<TTo>();

        // Optimization: If sizes are the same, just reinterpret the pointer and keep length
        if (fromSize == toSize) {
            return new Span64<TTo>(ref Unsafe.As<TFrom, TTo>(ref span._reference), sourceLength);
        }

        // Calculate total bytes.
        // We use 'checked' to be safe, though 64-bit address space limits make overflow unlikely 
        // unless the input length is corrupt or near long.MaxValue.
        long totalBytes;
        try {
            totalBytes = checked(sourceLength * fromSize);
        } catch (OverflowException) {
            throw new OverflowException($"Cannot cast Span64. The total byte length exceeds long.MaxValue.");
        }

        // Calculate new length (integer division floors the result, ignoring remainder bytes)
        long newLength = totalBytes / toSize;

        return new Span64<TTo>(ref Unsafe.As<TFrom, TTo>(ref span._reference), newLength);
    }

    /// <summary>
    /// Casts a ReadOnlySpan64 of one primitive type to a ReadOnlySpan64 of another primitive type.
    /// </summary>
    public static ReadOnlySpan64<TTo> Cast<TFrom, TTo>(ReadOnlySpan64<TFrom> span)
        where TFrom : struct
        where TTo : struct {
        if (span.IsEmpty) {
            return default;
        }

        long sourceLength = span.Length;
        int fromSize = Unsafe.SizeOf<TFrom>();
        int toSize = Unsafe.SizeOf<TTo>();

        if (fromSize == toSize) {
            return new ReadOnlySpan64<TTo>(ref Unsafe.As<TFrom, TTo>(ref span._reference), sourceLength);
        }

        long totalBytes;
        try {
            totalBytes = checked(sourceLength * fromSize);
        } catch (OverflowException) {
            throw new OverflowException($"Cannot cast ReadOnlySpan64. The total byte length exceeds long.MaxValue.");
        }

        long newLength = totalBytes / toSize;

        return new ReadOnlySpan64<TTo>(ref Unsafe.As<TFrom, TTo>(ref span._reference), newLength);
    }

    /// <summary>
    /// Returns a reference to the 0th element of the Span64. 
    /// If the span is empty, returns a null reference.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(Span64<T> span) {
        return ref span._reference;
    }

    /// <summary>
    /// Returns a reference to the 0th element of the ReadOnlySpan64.
    /// If the span is empty, returns a null reference.
    /// </summary>
    /// <remarks>
    /// This strips the 'readonly' constraint, allowing unsafe mutation 
    /// if used incorrectly, matching standard MemoryMarshal behavior.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(ReadOnlySpan64<T> span) {
        // We use Unsafe.AsRef to strip the "in" (readonly) modifier 
        // from the internal reference.
        return ref Unsafe.AsRef(in span._reference);
    }

    /// <summary>
    /// Casts a Span64 of one primitive type to a Span64 of bytes.
    /// </summary>
    public static Span64<byte> AsBytes<T>(Span64<T> span) where T : struct {
        if (Unsafe.SizeOf<T>() == 1) {
            return new Span64<byte>(ref Unsafe.As<T, byte>(ref span._reference), span.Length);
        }
        long newLength = checked(span.Length * Unsafe.SizeOf<T>());
        return new Span64<byte>(ref Unsafe.As<T, byte>(ref span._reference), newLength);
    }

    /// <summary>
    /// Casts a ReadOnlySpan64 of one primitive type to a ReadOnlySpan64 of bytes.
    /// </summary>
    public static ReadOnlySpan64<byte> AsBytes<T>(ReadOnlySpan64<T> span) where T : struct {
        if (Unsafe.SizeOf<T>() == 1) {
            return new ReadOnlySpan64<byte>(ref Unsafe.As<T, byte>(ref span._reference), span.Length);
        }
        long newLength = checked(span.Length * Unsafe.SizeOf<T>());
        return new ReadOnlySpan64<byte>(ref Unsafe.As<T, byte>(ref span._reference), newLength);
    }

    /// <summary>
    /// Creates a Memory64 from a ReadOnlyMemory64.
    /// </summary>
    public static Memory64<T> AsMemory<T>(ReadOnlyMemory64<T> memory) {
        return new Memory64<T>(memory._object, memory._indexOrPointer, memory._length);
    }

    /// <summary>
    /// Creates a new Span64 over the target reference.
    /// </summary>
    public static Span64<T> CreateSpan<T>(ref T reference, long length) {
        return new Span64<T>(ref reference, length);
    }

    /// <summary>
    /// Creates a new ReadOnlySpan64 over the target reference.
    /// </summary>
    public static ReadOnlySpan64<T> CreateReadOnlySpan<T>(ref T reference, long length) {
        return new ReadOnlySpan64<T>(ref reference, length);
    }

    /// <summary>
    /// Reads a structure of type T from a read-only span of bytes.
    /// </summary>
    public static T Read<T>(ReadOnlySpan64<byte> source) where T : struct {
        if (source.Length < Unsafe.SizeOf<T>()) {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }
        return Unsafe.ReadUnaligned<T>(ref GetReference(source));
    }

    /// <summary>
    /// Writes a structure of type T to a span of bytes.
    /// </summary>
    public static void Write<T>(Span64<byte> destination, ref T value) where T : struct {
        if (destination.Length < Unsafe.SizeOf<T>()) {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }
        Unsafe.WriteUnaligned(ref GetReference(destination), value);
    }

    /// <summary>
    /// Tries to get the underlying array from a ReadOnlyMemory64.
    /// </summary>
    public static bool TryGetArray<T>(ReadOnlyMemory64<T> memory, out ArraySegment<T> segment) {
        if (memory._object is T[] array) {
            // ArraySegment supports int only
            if (memory._indexOrPointer <= int.MaxValue && memory._length <= int.MaxValue) {
                segment = new ArraySegment<T>(array, (int)memory._indexOrPointer, (int)memory._length);
                return true;
            }
        }
        segment = default;
        return false;
    }

    /// <summary>
    /// Tries to retrieve the underlying memory manager from a ReadOnlyMemory64.
    /// </summary>
    public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory64<T> memory, out TManager? manager) {
        if (memory._object is TManager m) {
            manager = m;
            return true;
        }
        manager = default;
        return false;
    }

    /// <summary>
    /// Tries to retrieve the underlying memory manager, start index and length from a ReadOnlyMemory64.
    /// </summary>
    public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory64<T> memory, out TManager? manager, out long start, out long length) {
        if (memory._object is TManager m) {
            manager = m;
            start = memory._indexOrPointer;
            length = memory._length;
            return true;
        }
        manager = default;
        start = 0;
        length = 0;
        return false;
    }

    /// <summary>
    /// Returns a reference to the element of the span at index 0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(Span64<T> span) {
        return ref span._reference;
    }

    /// <summary>
    /// Returns a reference to the element of the read-only span at index 0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(ReadOnlySpan64<T> span) {
        return ref Unsafe.AsRef(in span._reference);
    }

    /// <summary>
    /// Creates an IEnumerable<T> view of the given read-only memory.
    /// </summary>
    public static System.Collections.Generic.IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory64<T> memory) {
        for (long i = 0; i < memory.Length; i++) {
            yield return memory.Span[i];
        }
    }
}