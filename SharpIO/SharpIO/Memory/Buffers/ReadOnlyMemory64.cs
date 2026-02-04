#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Smx.SharpIO.Memory.Buffers;

public readonly struct ReadOnlyMemory64<T> : IEquatable<ReadOnlyMemory64<T>>
{
    internal readonly object? _object;
    internal readonly long _indexOrPointer;
    internal readonly long _length;

    public static implicit operator ReadOnlyMemory<T>(ReadOnlyMemory64<T> value)
    {
        // Standard Memory<T> is limited to int.MaxValue
        if ((ulong)value._length > int.MaxValue)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        // Case 1: Managed Array
        if (value._object is T[] array)
        {
            return new ReadOnlyMemory<T>(array, (int)value._indexOrPointer, (int)value._length);
        }

        // Case 2: MemoryManager (e.g. NativeMemoryManager, RecyclableMemoryStream)
        if (value._object is MemoryManager<T> manager)
        {
            return manager.Memory.Slice((int)value._indexOrPointer, (int)value._length);
        }

        // Case 3: Native Pointers (void*)
        if (value._object == null)
        {
			var mgr = new UnmanagedMemoryManager<T>(new nint(value._indexOrPointer), (int)value._length);
            return mgr.Memory;
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyMemory64<T>(T[]? array) {
        // Reuse Memory64 logic for safety/simplicity
        return new Memory64<T>(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyMemory64<T>(ArraySegment<T> segment) {
        return new Memory64<T>(segment.Array).Slice(segment.Offset, segment.Count);
    }

    // Constructors mirror Memory64 (omitted for brevity, assume same logic)

    internal ReadOnlyMemory64(object? obj, long indexOrPtr, long length) {
        _object = obj;
        _indexOrPointer = indexOrPtr;
        _length = length;
    }

    public long Length => _length;
    public bool IsEmpty => _length == 0;

    public unsafe ReadOnlySpan64<T> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (_object == null)
                return new ReadOnlySpan64<T>((void*)_indexOrPointer, _length);

            if (_object is T[] array)
                return new ReadOnlySpan64<T>(array).Slice(_indexOrPointer, _length);

            if (_object is MemoryManager<T> manager) {
                Span<T> span = manager.GetSpan();
                return new ReadOnlySpan64<T>(ref MemoryMarshal.GetReference(span), span.Length)
                       .Slice(_indexOrPointer, _length);
            }
            return default;
        }
    }

    public ReadOnlyMemory64<T> Slice(long start) {
        if ((ulong)start > (ulong)_length) ThrowHelper.ThrowArgumentOutOfRangeException();

        long offset = (_object == null) ? start * Unsafe.SizeOf<T>() : start;
        return new ReadOnlyMemory64<T>(_object, _indexOrPointer + offset, _length - start);
    }

    public ReadOnlyMemory64<T> Slice(long start, long length) {
        if ((ulong)start > (ulong)_length || (ulong)length > (ulong)(_length - start))
            ThrowHelper.ThrowArgumentOutOfRangeException();

        long offset = (_object == null) ? start * Unsafe.SizeOf<T>() : start;
        return new ReadOnlyMemory64<T>(_object, _indexOrPointer + offset, length);
    }

    public bool Equals(ReadOnlyMemory64<T> other)
        => _object == other._object && _indexOrPointer == other._indexOrPointer && _length == other._length;

    public override bool Equals(object? obj) => obj is ReadOnlyMemory64<T> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_object, _indexOrPointer, _length);

    public T[] ToArray() {
        return Span.ToArray();
    }

    public void CopyTo(Memory64<T> destination) {
        Span.CopyTo(destination.Span);
    }

    public bool TryCopyTo(Memory64<T> destination) {
        return Span.TryCopyTo(destination.Span);
    }

    public override string ToString() {
        return $"Smx.SharpIO.Memory.Buffers.ReadOnlyMemory64<{typeof(T).Name}>[{_length}]";
    }

    public unsafe MemoryHandle Pin() {
        if (_object is MemoryManager64<T> manager) {
            return manager.Pin(_indexOrPointer);
        } else if (_object is T[] array) {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            ref T refToElement = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), (nint)_indexOrPointer);
            void* pointer = Unsafe.AsPointer(ref refToElement);
            return new MemoryHandle(pointer, handle);
        } else {
            // Native pointer
            return new MemoryHandle((void*)_indexOrPointer);
        }
    }
}
