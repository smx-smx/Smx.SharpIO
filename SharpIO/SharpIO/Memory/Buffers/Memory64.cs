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

public readonly struct Memory64<T> : IEquatable<Memory64<T>>, IDisposable
{
    private readonly object? _object;
    private readonly long _indexOrPointer;
    private readonly long _length;

	public static unsafe implicit operator Memory64<T>(Memory<T> value) {
		var handle = value.Pin();
		var dptr = handle.Pointer;
		var length = value.Length;
		return new Memory64<T>(handle, (long)dptr, length);
	}

    public static implicit operator Memory<T>(Memory64<T> value)
    {
        // Standard Memory<T> is limited to int.MaxValue
        if ((ulong)value._length > int.MaxValue)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        if (value._object is T[] array)
        {
            return new Memory<T>(array, (int)value._indexOrPointer, (int)value._length);
        }

		if (value._object is MemoryManager64<T> manager)
        {
            return manager.Memory.Slice((int)value._indexOrPointer, (int)value._length);
        }

        // Case 3: Native Pointers (void*)
        if (value._object == null || value._object is MemoryHandle)
        {
			var mgr = new UnmanagedMemoryManager<T>(new nint(value._indexOrPointer), (int)value._length);
            return mgr.Memory;
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Memory64<T>(T[] array) {
        return new Memory64<T>(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Memory64<T>(ArraySegment<T> segment) {
        return new Memory64<T>(segment.Array).Slice(segment.Offset, segment.Count);
    }

    public unsafe Memory64(void* pointer, long length) {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _object = null;
        _indexOrPointer = (long)pointer;
        _length = length;
    }

    public Memory64(T[]? array) {
        if (array == null) throw new ArgumentNullException(nameof(array));
        _object = array;
        _indexOrPointer = 0;
        _length = array.LongLength;
    }

    public Memory64(MemoryManager64<T> manager, long index, long length) {
        if (manager == null) throw new ArgumentNullException(nameof(manager));

        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

        long actualLength = length;
        long managerLength = manager.GetSpan().Length;

        if ((ulong)index > (ulong)managerLength || (ulong)actualLength > (ulong)(managerLength - index))
            throw new ArgumentOutOfRangeException();

        _object = manager;
        _indexOrPointer = index;
        _length = actualLength;
    }

    // Internal Constructor for Slice (Trusted inputs)
    internal Memory64(object? obj, long indexOrPtr, long length) {
        _object = obj;
        _indexOrPointer = indexOrPtr;
        _length = length;
    }

    public long Length => _length;
    public bool IsEmpty => _length == 0;

    public unsafe Span64<T> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (_object == null || _object is MemoryHandle) {
                return new Span64<T>((void*)_indexOrPointer, _length);
            } else if (_object is T[] array) {
                return new Span64<T>(array).Slice(_indexOrPointer, _length);
            } else if (_object is MemoryManager64<T> manager) {
                Span64<T> span = manager.GetSpan();
                return new Span64<T>(ref MemoryMarshal64.GetReference(span), span.Length)
                       .Slice(_indexOrPointer, _length);
            }
			throw new InvalidOperationException();
        }
    }

    public Memory64<T> Slice(long start) {
        if ((ulong)start > (ulong)_length) ThrowHelper.ThrowArgumentOutOfRangeException();

        long newLength = _length - start;

        if (_object == null || _object is MemoryHandle) {
            // Native: Advance pointer
            long offsetBytes = start * Unsafe.SizeOf<T>();
            return new Memory64<T>(_object, _indexOrPointer + offsetBytes, newLength);
        } else {
            // Managed: Advance index
            return new Memory64<T>(_object, _indexOrPointer + start, newLength);
        }
    }

    public Memory64<T> Slice(long start, long length) {
        // Overflow-safe check
        if ((ulong)start > (ulong)_length || (ulong)length > (ulong)(_length - start)) {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        if (_object == null || _object is MemoryHandle) {
            long offsetBytes = start * Unsafe.SizeOf<T>();
            return new Memory64<T>(_object, _indexOrPointer + offsetBytes, length);
        } else {
            return new Memory64<T>(_object, _indexOrPointer + start, length);
        }
    }

    public static implicit operator ReadOnlyMemory64<T>(Memory64<T> memory)
        => new ReadOnlyMemory64<T>(memory._object, memory._indexOrPointer, memory._length);

    public bool Equals(Memory64<T> other)
        => _object == other._object && _indexOrPointer == other._indexOrPointer && _length == other._length;

    public override bool Equals(object? obj) => obj is Memory64<T> other && Equals(other);
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
        return $"Smx.SharpIO.Memory.Buffers.Memory64<{typeof(T).Name}>[{_length}]";
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

	public void Dispose() {
		if(_object is MemoryHandle handle) {
			handle.Dispose();
		}
	}
}
