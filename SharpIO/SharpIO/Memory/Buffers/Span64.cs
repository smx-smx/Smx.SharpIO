#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion

using Smx.SharpIO.Extensions;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Smx.SharpIO.Memory.Buffers;

public readonly ref struct Span64<T>
{
    internal readonly ref T _reference;
    private readonly long _length;

    public long Length => _length;
    public bool IsEmpty => _length == 0;

	public static implicit operator ReadOnlySpan64<T>(Span64<T> span64) {
		return MemoryMarshal64.CreateReadOnlySpan(ref span64._reference, span64.Length);
	}

	public static implicit operator Span64<T>(Span<T> span) {
		ref var dref = ref MemoryMarshal.GetReference(span);
		return MemoryMarshal64.CreateSpan(ref dref, span.Length);
	}

	public static implicit operator ReadOnlySpan<T>(Span64<T> span64) {
		if (span64._length > int.MaxValue) {
			throw new OverflowException("Span64 length exceeds the 32-bit limit of Span<T>.");
		}
		return MemoryMarshal.CreateReadOnlySpan(ref span64._reference, (int)span64._length);
	}

	public static implicit operator Span<T>(Span64<T> span64) {
        if (span64._length > int.MaxValue)
        {
            throw new OverflowException("Span64 length exceeds the 32-bit limit of Span<T>.");
        }
        return MemoryMarshal.CreateSpan(ref span64._reference, (int)span64._length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span64<T>(T[]? array) {
        return new Span64<T>(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span64<T>(ArraySegment<T> segment) {
        return new Span64<T>(segment.Array).Slice(segment.Offset, segment.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe Span64(void* pointer, long length) {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _reference = ref Unsafe.AsRef<T>(pointer);
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span64(ref T reference, long length) {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _reference = ref reference;
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span64(T[]? array) {
        if (array == null) {
            this = default;
            return;
        }
        _reference = ref MemoryMarshal.GetArrayDataReference(array);
        _length = array.LongLength;
    }

    public ref T this[long index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            // Cast to ulong handles negative numbers (they become Large Ints)
            if ((ulong)index >= (ulong)_length) ThrowHelper.ThrowIndexOutOfRangeException();
            return ref Unsafe.Add(ref _reference, (nint)index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span64<T> Slice(long start) {
        // Validates: 0 <= start <= _length
        if ((ulong)start > (ulong)_length) ThrowHelper.ThrowArgumentOutOfRangeException();

        // New length is (_length - start), which is guaranteed positive
        return new Span64<T>(ref Unsafe.Add(ref _reference, (nint)start), _length - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span64<T> Slice(long start, long length) {
        // Validates:
        // 1. start is not negative
        // 2. start is not beyond end
        // 3. length is not negative
        // 4. start + length does not overflow or exceed _length
        if ((ulong)start > (ulong)_length || (ulong)length > (ulong)(_length - start)) {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        return new Span64<T>(ref Unsafe.Add(ref _reference, (nint)start), length);
    }

    // CopyTo with bounds checking
    public void CopyTo(Span64<T> destination) {
        // Basic length check
        if ((ulong)_length > (ulong)destination.Length)
            ThrowHelper.ThrowArgumentException_DestTooShort();

        unsafe {
            // Use Buffer.MemoryCopy for safe handling of overlaps and ulong sizes
            long byteCount = _length * Unsafe.SizeOf<T>();
            long destByteCount = destination.Length * Unsafe.SizeOf<T>();

            void* srcPtr = Unsafe.AsPointer(ref _reference);
            void* dstPtr = Unsafe.AsPointer(ref destination._reference);

            Buffer.MemoryCopy(srcPtr, dstPtr, destByteCount, byteCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetPinnableReference() => ref _reference;

    public T[] ToArray() {
        if (_length > int.MaxValue) {
            throw new OverflowException(
                $"Cannot convert Span64 to Array because the length ({_length}) exceeds int.MaxValue. " +
                "Standard .NET arrays are limited to 32-bit lengths.");
        }

        if (_length == 0) {
            return Array.Empty<T>();
        }

        // cast _length to int (safe due to check above)
        var destination = new T[(int)_length];

        // reuse existing CopyTo logic
        this.CopyTo(new Span64<T>(destination));

        return destination;
    }

    /// <summary>
    /// Clears the contents of this span.
    /// </summary>
    public void Clear() {
        if (_length == 0) return;
        long byteCount = _length * Unsafe.SizeOf<T>();
        // Using Unsafe.InitBlock to clear memory (memset)
        Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref _reference), 0, (uint)byteCount); // Note: InitBlock takes uint, so we might need a loop for > 4GB
        
        // Handling > 4GB clear if necessary
        if (byteCount > uint.MaxValue) {
             long remaining = byteCount;
             ref byte ptr = ref Unsafe.As<T, byte>(ref _reference);
             while (remaining > 0) {
                 uint chunk = (remaining > uint.MaxValue) ? uint.MaxValue : (uint)remaining;
                 Unsafe.InitBlockUnaligned(ref ptr, 0, chunk);
                 ptr = ref Unsafe.Add(ref ptr, (nint)chunk);
                 remaining -= chunk;
             }
        } else {
             Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref _reference), 0, (uint)byteCount);
        }
    }

    /// <summary>
    /// Fills the contents of this span with a given value.
    /// </summary>
    public void Fill(T value) {
        if (_length == 0) return;
        
        // Standard spans use block initialization for byte-sized types, 
        // but generic fill usually loops. 
        // We can use a loop here. 
        for (long i = 0; i < _length; i++) {
            Unsafe.Add(ref _reference, (nint)i) = value;
        }
    }

    /// <summary>
    /// Tries to copy the current span into the destination span.
    /// Returns false if the destination is too short.
    /// </summary>
    public bool TryCopyTo(Span64<T> destination) {
        if ((ulong)_length > (ulong)destination.Length) {
            return false;
        }
        CopyTo(destination);
        return true;
    }

    public static bool operator ==(Span64<T> left, Span64<T> right) {
        return left._length == right._length &&
               Unsafe.AreSame(ref left._reference, ref right._reference);
    }

    public static bool operator !=(Span64<T> left, Span64<T> right) {
        return !(left == right);
    }

    public override string ToString() {
        return $"Smx.SharpIO.Memory.Buffers.Span64<{typeof(T).Name}>[{_length}]";
    }

    public override bool Equals(object? obj) => throw new NotSupportedException("Span64<T> cannot be boxed");
    public override int GetHashCode() => throw new NotSupportedException("Span64<T> cannot be boxed");

    public Enumerator GetEnumerator() => new Enumerator(this);

    public ref struct Enumerator {
        private readonly Span64<T> _span;
        private long _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(Span64<T> span) {
            _span = span;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            long index = _index + 1;
            if (index < _span.Length) {
                _index = index;
                return true;
            }
            return false;
        }

        public ref T Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }
    }
}